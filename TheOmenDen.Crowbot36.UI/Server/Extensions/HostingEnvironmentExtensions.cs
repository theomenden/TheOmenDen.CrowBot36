namespace TheOmenDen.Crowbot36.UI.Server.Extensions;

public static class HostingEnvironmentExtensions
{
    public static Boolean IsDevelopmentOrStaging(this IWebHostEnvironment env) => env.IsDevelopment() || env.IsStaging();
}