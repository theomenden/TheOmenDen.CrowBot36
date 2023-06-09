﻿#region Usings
using Blazorise;
using Blazorise.LoadingIndicator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using System.Net;
using TheOmenDen.Crowbot36.Core.Auth.InputModels;
using TheOmenDen.Crowbot36.Extensions;
using TheOmenDen.Crowbot36.Identity.Extensions;
using TheOmenDen.Crowbot36.Services;
using TwitchLib.Api;
#endregion
namespace TheOmenDen.Crowbot36.UI.Pages.Auth;

public partial class Login : ComponentBase
{
    #region Parameters
    [CascadingParameter] public Task<AuthenticationState> AuthenticationStateTask { get; set; }
    [Parameter] public String ReturnUrl { get; set; }
    #endregion
    #region Injected Members
    [Inject] private TokenStateService TokenStateService { get; init; }

    [Inject] private ILogger<Login> Logger { get; init; }

    [Inject] private UserManager<ApplicationUser> UserManager { get; init; }

    [Inject] private IDataProtectionProvider DataProtectionProvider { get; init; }

    [Inject] private NavigationManager NavigationManager { get; init; }

    [Inject] private TokenProvider TokenProvider { get; init; }

    [Inject] private SignInManager<ApplicationUser> SignInManager { get; init; }

    [Inject] private TwitchStrings TwitchStrings { get; init; }

    [Inject] private TwitchAPI TwitchAPI { get; init; }
    #endregion
    #region Private fields

    private const string ClassListForIcons = @"fa-brands fa-{0} text-light display-3";
    private Validations _validationsRef;
    private List<AuthenticationScheme> _externalProviders = new(5);
    private LoadingIndicator _loadingIndicator;
    #endregion
    protected LoginInputModel Input { get; set; } = new();

    protected Boolean HasErrors { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var (couldRetrieveEmail, email) = NavigationManager.TryGetQueryString<string>("email");
        var (couldCheckForErrors, hasErrors) = NavigationManager.TryGetQueryString<bool>("hasErrors");
        var (couldGetReturnUrl, returnUrl) = NavigationManager.TryGetQueryString<string>("ReturnUrl");
        if (couldRetrieveEmail)
        {
            Input.Email = email;
        }

        if (couldCheckForErrors)
        {
            HasErrors = hasErrors;
        }

        if (String.IsNullOrWhiteSpace(ReturnUrl) && couldGetReturnUrl)
        {
            ReturnUrl = returnUrl ?? String.Empty;
        }

        _externalProviders = (await SignInManager.GetExternalAuthenticationSchemesAsync())
            .Where(scheme => !scheme.Name.Equals("OpenIdConnect", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task OnLoginAsync()
    {
        await _loadingIndicator.Show();

        try
        {
            HasErrors = false;

            if (await _validationsRef.ValidateAll())
            {
                var user = await UserManager.FindByEmailAsync(Input.Email);

                if (user is null)
                {
                    HasErrors = true;
                    return;
                }

                Input.Token = await UserManager.GenerateUserTokenAsync(user, TokenOptions.DefaultProvider, "Login");

                var data = $"{Input.Email}|{Input.Password}|{Input.Token}|{Input.IsRemembered}";
                var protector = DataProtectionProvider.CreateProtectorForLogin();
                var protectedData = protector.Protect(data);

                NavigationManager.NavigateTo($"Account/LoginInternal?data={protectedData}", true);
            }
        }
        catch (Exception ex)
        {
            HasErrors = true;
            Logger.LogError("Failed to log in due to exception {@Ex}", ex);
        }
        finally
        {
            await _loadingIndicator.Hide();
        }
    }

    private static string GetLoginProviderIcon(String loginProviderName)
    {
        var result = loginProviderName switch
        {
            _ when String.Equals(loginProviderName, "twitch", StringComparison.OrdinalIgnoreCase) => loginProviderName,
            _ when String.Equals(loginProviderName, "twitter", StringComparison.OrdinalIgnoreCase) => loginProviderName,
            _ when String.Equals(loginProviderName, "discord", StringComparison.OrdinalIgnoreCase) => loginProviderName,
            _ => String.Empty
        };

        return String.Format(ClassListForIcons, result.ToLowerInvariant());
    }

    private static String GetExternalLoginUrl(AuthenticationScheme authenticationScheme)
        => $"Account/challenge/{authenticationScheme.Name}";

}