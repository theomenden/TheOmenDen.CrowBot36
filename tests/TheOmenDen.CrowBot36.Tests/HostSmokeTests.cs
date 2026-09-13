using Microsoft.AspNetCore.Mvc.Testing;

namespace TheOmenDen.CrowBot36.Tests;

public sealed class HostSmokeTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task HomePage_ReturnsSuccess()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
