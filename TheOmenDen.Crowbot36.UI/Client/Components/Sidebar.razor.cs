using Blazorise;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace TheOmenDen.Crowbot36.UI.Client.Components;

public partial class Sidebar : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; init; }
    private Bar _sidebar;

    private static string AssemblyProductVersion
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
          .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            return attributes.Length == 0 ?
                String.Empty :
                ((AssemblyFileVersionAttribute)attributes[0]).Version;
        }
    }

    private static string ApplicationDevelopmentCompany
    {
        get
        {
            var attributes = Assembly.GetExecutingAssembly()
                 .GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            return attributes.Length == 0 ?
                String.Empty :
                ((AssemblyCompanyAttribute)attributes[0]).Company;
        }
    }
}
