using System.Net;
using FluentAssertions;

namespace CatalogTest.Integration;

public class HealthControllerTests : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client;

    public HealthControllerTests(CatalogApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Ping_DeveRetornar200()
    {
        var response = await _client.GetAsync("/api/catalogo/health/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
