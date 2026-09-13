# TheOmenDen.CrowBot36 — Hosted Twitch Chat Bot

## Project Context

CrowBot36 is The Omen Den's **hosted, multi-channel** Twitch chat bot, in the vein of NightBot, StreamElements, and Sery_Bot. It runs on **Azure Container Apps**, orchestrated by **Aspire**, and serves **allowlisted channels** (mg36Crow first; designed for ~100+). Streamers are approved by an admin, then sign in and grant the bot access to their channel.

It is a **deliberately stripped-down sibling of Corvus-Connection** (StreamPulse). It keeps exactly three things:

1. **Chat commands**
2. **Moderation actions** (Helix ban / timeout / delete / warnings / shield mode / etc.)
3. **Calls to external APIs** from commands

It does **not** have custom scripting, overlays, node graphs, desktop companions, a message bus, or open sign-up. Anything from that list needs an explicit decision first.

**Performance is a requirement**: a raid in one channel must never delay another channel, and the socket must never wait on handlers.

## Related Repositories

| Repo | Role for CrowBot36 | Rule |
|---|---|---|
| `../Corvus-Connection` | Reference implementation for EventSub, commands, moderation, Helix, and the Aspire/azd setup | **Port and slim files; never reference its projects** |
| `../TheOmenDen.TwitchOAuthGen` | Auth broker at `https://oauth.corvid.online`; owns the Twitch client secret | All Twitch OAuth goes through it. CrowBot36 never holds a client secret |
| `../TheOmenDen.CorvidOnline` | Shared platform packages (`TheOmenDen.Corvid.*`) | `Corvid.Security` now; `Corvid.Hosting` once a release > 0.4.0 ships |

## Solution Layout

```
aspire/TheOmenDen.CrowBot36.AppHost/   # Aspire orchestration + Azure publish model
src/TheOmenDen.CrowBot36.Bot/          # Worker: conduit shard, chat pipeline, commands, moderation (no ingress)
src/TheOmenDen.CrowBot36.Dashboard/    # Blazor Web App: admin allowlist, streamer onboarding, settings (ingress)
tests/TheOmenDen.CrowBot36.Tests/      # xUnit v3 on MTP
```

- Add a shared class library (for example `TheOmenDen.CrowBot36.Data` for the DbContext and entities) when the first type is actually shared by Bot and Dashboard, not before.
- Inside each app, use vertical slices: `Twitch/EventSub`, `Twitch/Helix`, `Auth`, `Features/Commands`, `Features/Moderation`, `Features/[Integration]`. Create folders when the first file needs them.

## Tech Stack

- **.NET 10** / C# 14, CPM with GitHub Packages for `TheOmenDen.*` (`nuget.config`), warnings as errors
- **Aspire 13.5** AppHost (`AspireUseCliBundle=true`, Aspire CLI installed) → `azd` → Azure Container Apps + ACR
- **Azure SQL serverless** (EF Core SqlServer) for tenants, settings, the bot token, shard leases, and Data Protection keys. A SQL Server container stands in for it locally
- **Twitch**: EventSub **conduit** with WebSocket shards for inbound chat, Helix for outbound chat and moderation. No TwitchLib, no IRC
- **Auth**: `TheOmenDen.OAuthBroker.Client` (consumer `crowbot36`)
- **Blazor Web App** (Interactive Server) for the dashboard
- **HTTP**: typed clients + `Microsoft.Extensions.Http.Resilience`, source-generated JSON, `HybridCache` where a feature needs it
- **Testing**: xUnit v3 on Microsoft.Testing.Platform (`global.json`), `WebApplicationFactory`; bUnit and `Aspire.Hosting.Testing` when needed

## Architecture

### Topology

```
Twitch EventSub Conduit (app access token; up to 20k shards)
  ├─ shard 0 ── WebSocket ──► bot replica A ─┐
  └─ shard 1 ── WebSocket ──► bot replica B ─┤  Twitch hashes each channel ID to one shard,
                                             │  so each channel is owned by exactly one replica
Azure SQL ◄──────────────────────────────────┤  (tenants, settings, bot token, shard leases)
    ▲                                        │
Dashboard (Blazor, sticky sessions) ─────────┘  Admin allowlist + streamer onboarding via broker
```

- **Why a conduit and not per-channel sockets**: plain EventSub WebSockets need a user token and cap at 3 connections × 300 subscriptions per token. Subscriptions die with the socket, and app tokens fail on WebSockets. A conduit holds subscriptions independently of sockets, balances notifications across shards, and survives replica restarts.
- **Channel affinity is the scaling model.** Because a channel always lands on one shard, cooldowns, deduplication, and per-channel send limits stay **in memory** on the owning replica. There is no Redis and no distributed lock on the hot path. When shards are rebalanced, moved channels lose in-memory cooldowns, which is accepted.
- **Shard ownership**: each bot replica claims a shard through a lease row in SQL (renewed on a timer), connects a WebSocket, and on `session_welcome` (and every reconnect) calls `PATCH /eventsub/conduits/shards` with its `session_id`. Keep **shard count == bot replica count**. If every shard is disabled, Twitch drops notifications.
- **Subscriptions** (`channel.chat.message`, condition `broadcaster_user_id` + `user_id`=bot) are created with the app token against the conduit when a channel is approved. A single-leader reconcile loop (SQL app lock) keeps them matched to the allowlist.
- **SignalR is for browsers only.** Dashboard live views use groups keyed by broadcaster ID. The bot never opens per-channel SignalR connections or sockets.

### Bot hot path (per replica)

```
shard WebSocket read loop: pooled buffer → Utf8JsonReader header peek → dedupe(message_id) → translate
  └─ router: mailbox[broadcaster_id]  (Channel<ChatEvent>, bounded, single reader, created on first use, evicted when idle)
       └─ per-channel loop: ModerationGate → CommandDispatcher (FrozenDictionary, span parsing, roles, cooldowns)
            ├─ ChatSender  → per-channel send limiter → Helix POST /chat/messages (app token → Chat Bot badge)
            ├─ HelixModeration → global limiter for the bot user token (fair across channels)
            └─ external typed HttpClients (+ HybridCache)
```

- **The read loop never awaits a handler.** It only routes into a mailbox. Corvus-Connection's sequential Mediator publish inside its read loop is exactly what to avoid.
- **One mailbox per channel** keeps each channel's messages in order and stops a noisy channel from starving the others. Bound each mailbox; on overflow, drop the oldest *command* events but never skip moderation of newer messages. Cap the total number of in-flight channel loops (`ConnectionSlots` from `Corvid.Security` fits).
- **Tenant settings** (enabled commands, prefix, cooldowns, moderation rules) are read into an immutable per-channel snapshot, refreshed on a short interval or when the dashboard signals a change. Never query SQL per message.

### Porting from Corvus-Connection

Paths relative to `../Corvus-Connection/src`:

| Concern | Source | Change when porting |
|---|---|---|
| EventSub client | `TheOmenDen.StreamPulse.Runtime/Ingest/Twitch/TwitchEventSubClient.cs` | Keep the state machine, keepalive watchdog, reconnect-before-close, and pooled buffer. Add conduit shard mode: on welcome, PATCH the shard instead of creating per-session subscriptions. Route to mailboxes; don't await sinks |
| Session loop | `…/EventSubWebsocketHostedService.cs`, `EventSubEventTranslator.cs` | One loop per leased shard; chat events only |
| Commands | `…Application/Features/Twitch/Commands/` (`IChatCommand`, `ChatCommandDispatcher`, `ChatRoleEvaluator`, `ChatRolePresets`) | Drop Mediator. Per-channel overrides come from the tenant snapshot, not EF per call. Span parsing instead of `string.Split` |
| Moderation | `…/Features/Twitch/Moderation/` (`ChatModerationGate`, `ChatContentFilter`, `ModerationEscalationPolicy`, `ChatModerationClient`) | Per-channel rules from the snapshot; strikes in memory (persist only if a feature needs history); `[GeneratedRegex]`; no per-message allocations |
| Helix auth | `TwitchHelixAuthHandler` | App token or bot user token from memory, never a DbContext + decrypt per call |
| AppHost / azd | `aspire/TheOmenDen.StreamPulse.AppHost/Program.cs`, `azure.yaml` | Same Key Vault `AsExisting` and out-of-band Azure SQL pattern |

Coupling to avoid: scripting/overlay/desktop Mediator handlers, `INowPlayingOverlayPublisher`, weather overlay publishers, VTube Studio/TITS clients, `StreamPulseDbContextBase`.

## Twitch Integration

- **Client ID** `ayb6veribz0tx102ai0ks2aukq0x2n` (`Twitch:ClientId` in the Bot's `appsettings.json`) is public. It must match the broker's `crowbot36` registration.
- **Tokens and grants**:
  - **App access token** (broker `POST /api/twitch/token`): manages the conduit, creates subscriptions, sends chat. Cache it in memory per replica and re-mint on expiry.
  - **Bot user token** (the bot account links once through the broker): `user:read:chat`, `user:write:chat`, `user:bot`, and the `moderator:manage:*` / `moderator:read:*` scopes used for moderation. The bot must be **modded in each channel** before moderation works.
  - **Broadcaster grant**: each streamer grants `channel:bot` during onboarding. No broadcaster token is stored unless a feature needs broadcaster-only scopes.
- **Chat send limits are per channel**: 20 messages/30s, or 100/30s when the bot is a mod or VIP. Verified bots get far more. Enforce them in the per-channel limiter.
- **Helix rate limits**: use a token bucket per token driven by `Ratelimit-Limit` / `Ratelimit-Remaining` / `Ratelimit-Reset`; on 429 wait until reset. All moderation calls share the one bot user token, so that limiter is global and must stay fair across channels.
- **Never retry** Helix POST/PUT/PATCH/DELETE: use `AddStandardResilienceHandler` with `Retry.DisableForUnsafeHttpMethods()`.
- **EventSub**: honour `keepalive_timeout_seconds`; on `session_reconnect`, connect the new URL, PATCH the shard, then close the old socket within 30s; dedupe on `message_id`.

## Auth & Tokens

- **Broker**: `https://oauth.corvid.online`, consumer `crowbot36`. `Broker:ApiKey` comes from Key Vault (cloud) or user-secrets (local).
- **Broker registration today** (uncommitted, TwitchOAuthGen branch `feat/crowbot36-consumer-oidc-spec`): `Deeplink` surface pointed at `https://localhost:7198/auth/broker/callback`. The target is the `Web` surface in TwitchOAuthGen `docs/specs/003-web-consumer-oidc-sign-in.md`: multiple redirect URIs (dev + prod), automatic 302, and `id_token`.
- **Dashboard sign-in** (blocked on spec 003): `openid`-only sign-in, cookie session. **Admins** are allowlisted Twitch user IDs. **Streamers** can sign in only if their channel is approved, and then run a separate flow to grant `channel:bot`. Never request bot scopes in a sign-in flow.
- **Bot user token storage**: stored in Azure SQL, encrypted with ASP.NET Core Data Protection. The key ring is persisted to SQL and protected by a Key Vault key, so every replica can read it.
- **Refresh is single-flight across replicas**: take a SQL app lock (`sp_getapplock`) per account, re-read the row, refresh only if still needed, and persist the rotated refresh token **before** using the new access token. Never retry a refresh POST. A 400/401 means the bot account must be re-linked, so show that on the dashboard.

## Deployment

- `aspire run` locally. It starts the SQL Server container, so Docker is required.
- **Cloud**: Aspire publish model → `azd` → the **existing** Container Apps environment and ACR (names still to be wired). Containers are built with SDK container publishing; there is no Dockerfile unless one is needed.
  - **Bot**: no ingress; `minReplicas == maxReplicas == shard count`.
  - **Dashboard**: external ingress, **sticky sessions** (Blazor Server circuits), Data Protection keys shared via SQL.
  - **Key Vault**: the existing `theomenden-vault` through `AddAzureKeyVault(...).AsExisting(...)`. Managed identity gets secret access.
  - **Azure SQL**: provisioned **out-of-band**. Never let `AddDatabase` run in publish mode, because it emits a managed database and an `AllowAllAzureIps` firewall rule. The cloud connection string comes from Key Vault.
- **Corvid.Hosting** (Serilog, OpenTelemetry, health checks) is deferred until a release > 0.4.0, because 0.4.0 sends exception text unredacted over OTLP. Until then the apps export no OpenTelemetry, so the Aspire dashboard shows console logs only. When adopted:
  - Use `builder.AddCorvidDefaults()` + `app.MapCorvidDefaultEndpoints()`.
  - **Never** use `Corvid.Hosting.Http` or `Corvid.Hosting.AspNetCore`: they retry unsafe HTTP methods with no opt-out.
  - Point the always-on file sink somewhere sane in containers.
  - Log IDs rather than logins, because properties named `*login*` / `*username*` are redacted.
- **`Corvid.Security`** (Dashboard today): `ConnectionSlots` for bounded concurrency, `HandshakeGate`/`AllowedOrigins` for any browser-facing socket endpoint. Its `AccessToken` / `IAccessTokenStore` are **local pairing keys, not OAuth tokens**. Name Twitch types `TwitchCredential` and similar to avoid collisions.

## Performance Rules

On the hot path (per chat message):

- **No LINQ, no `string.Split`, no per-message collections.** Parse with `ReadOnlySpan<char>`.
- **`FrozenDictionary` / `FrozenSet`** for anything built at startup or per tenant snapshot.
- **Source-generated `System.Text.Json`** (`JsonSerializerContext`) and **`[LoggerMessage]`** everywhere.
- **`ValueTask`** for handler contracts that usually complete synchronously.
- **Pooled buffers** (`ArrayPool<byte>`, `IBufferWriter<byte>`) for socket frames. No `new MemoryStream()`, no `ToArray()` on hot buffers.
- **`TimeProvider`** for cooldowns, keepalive, leases, and token expiry.
- **No SQL, file, or network I/O in the read loop or router.** No distributed calls per message.
- **No Mediator or reflection dispatch.**
- Claims of "faster" need a BenchmarkDotNet run. Add `benchmarks/` when the first such claim is made.

## Coding Standards

- **C# 14**: primary constructors, collection expressions, `field` keyword, records, pattern matching
- **File-scoped namespaces**, `var` when the type is obvious
- **Naming**: PascalCase public members, `_camelCase` private fields, `Async` suffix
- **No regions**. Comment the "why", never the "what"
- **No interface with a single implementation** unless it is a real test seam (Helix, broker, `TimeProvider` users). No interface per external API provider
- **One component per file**; `@inject` at the top, `@code` at the bottom, `.razor.cs` past ~30 lines
- Components never talk to Twitch; the dashboard reads SQL and calls the broker

## Skills

`modern-csharp`, `vertical-slice`, `aspire`, `ef-core`, `httpclient-factory`, `resilience`, `caching`, `configuration`, `dependency-injection`, `authentication`, `serilog`, `opentelemetry`, `container-publish`, `testing`. Use the `performance-analyst` agent for hot-path reviews and `devops-engineer` for azd/ACA.

## MCP Tools

`cwm-roslyn-navigator`: `find_symbol` → `get_public_api` before modifying a type; `find_references` before changing usages; `get_diagnostics` after changes.

## Commands

```bash
dotnet build
aspire run                                                  # from repo root or aspire/TheOmenDen.CrowBot36.AppHost; needs Docker
dotnet test --solution TheOmenDen.CrowBot36.slnx            # MTP runner via global.json
dotnet add src/TheOmenDen.CrowBot36.Bot package [Name]      # version lands in Directory.Packages.props
dotnet user-secrets set "Broker:ApiKey" "[value]" --project src/TheOmenDen.CrowBot36.Bot
dotnet format --verify-no-changes
```

## Workflow

- **Plan first** for anything with 3+ steps or an architecture decision.
- **Verify before done**: `dotnet build` and `dotnet test` must pass; check `get_diagnostics`.
- **Cross-repo changes** (broker config, specs) go on a branch in that repo. Implementation there belongs to a session in that repo.

## Anti-patterns

Do NOT generate code that:

- **Opens a socket or SignalR connection per Twitch channel**: use conduit shards and per-channel mailboxes
- **Awaits handlers inside the EventSub read loop**, or does I/O per message
- **Keeps per-channel state in Redis or SQL on the hot path**: channel affinity makes memory correct
- **Refreshes the bot token without the SQL app lock**, retries a refresh, or retries unsafe Helix calls
- **Runs more bot replicas than conduit shards** (or fewer shards than running replicas)
- **Holds a Twitch client secret or calls `id.twitch.tv/oauth2/token` directly**: that is the broker's job
- **Adds scripting, overlays, node graphs, Mediator, a message bus, or open sign-up** without an explicit decision
- **Lets `AddDatabase` / `AddAzureSqlServer` run in publish mode**
- **References Corvus-Connection projects**: port the file instead
- **Uses `DateTime.Now` / `DateTime.UtcNow`, `new HttpClient()`, `async void`, `.Result`, or `.Wait()`**
- **Logs tokens or chatter logins**; commits keys or tokens
- **Catches bare `Exception`**, except at a hosted service's top-level loop, which logs and backs off (2s → 2min)
