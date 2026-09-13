var builder = DistributedApplication.CreateBuilder(args);

var bot = builder.AddProject<Projects.TheOmenDen_CrowBot36_Bot>("bot");

var dashboard = builder.AddProject<Projects.TheOmenDen_CrowBot36_Dashboard>("dashboard")
    .WithExternalHttpEndpoints();

// Dev-only SQL Server container. The cloud database is Azure SQL serverless, provisioned out-of-band and
// reached through a Key Vault connection string: AddDatabase in publish mode emits a managed database child
// and an AllowAllAzureIps firewall rule (Corvus-Connection spec 068), which azd must never apply.
if (builder.ExecutionContext.IsRunMode)
{
    var db = builder.AddSqlServer("sql")
        .WithDataVolume()
        .AddDatabase("crowbot36db");

    bot.WithReference(db).WaitFor(db);
    dashboard.WithReference(db).WaitFor(db);
}

await builder.Build().RunAsync();
