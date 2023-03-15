using Microsoft.AspNetCore.Components;

namespace TheOmenDen.Crowbot36.UI.Client.Components;

public partial class SEO : ComponentBase
{
    #region Parameters
    [Parameter] public string Title { get; set; }
    [Parameter] public string Description { get; set; }
    [Parameter] public string Canonical { get; set; }
    [Parameter] public string ImageUrl { get; set; }
    #endregion
    #region Injected Members
    [Inject] private NavigationManager NavigationManager { get; init; }
    #endregion
    #region Private Members
    private string _url = String.Empty;
    #endregion
    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        _url = NavigationManager.ToAbsoluteUri(Canonical).AbsoluteUri;

        ImageUrl = String.IsNullOrEmpty(ImageUrl)
            ? NavigationManager.ToAbsoluteUri("images/the-omen-den-logo.png").AbsoluteUri
            : NavigationManager.ToAbsoluteUri(ImageUrl).AbsoluteUri;

        base.OnInitialized();
    }

    #endregion
}