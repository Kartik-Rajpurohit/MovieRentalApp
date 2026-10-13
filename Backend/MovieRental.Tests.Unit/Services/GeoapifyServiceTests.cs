using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class GeoapifyServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<GeoapifyService>> _loggerMock;

    public GeoapifyServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<GeoapifyService>>();

        _configurationMock.Setup(c => c["ExternalApis:Geoapify:ApiKey"]).Returns("test-api-key");
    }

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new FakeHttpMessageHandler(responder);
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.geoapify.com/v1/geocode/")
        };
    }

    // =========================================================
    // Validation / Guard Tests
    // =========================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("a")]
    [InlineData("ab")]
    public async Task AutocompleteAsync_WhenQueryShorterThanThreeChars_ReturnsEmptyListWithoutHttpCall(string? query)
    {
        // Arrange
        var called = false;
        var httpClient = CreateHttpClient(_ =>
        {
            called = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var sut = new GeoapifyService(httpClient, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.AutocompleteAsync(query!);

        // Assert
        result.Should().BeEmpty();
        called.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("YOUR_GEOAPIFY_API_KEY")]
    public async Task AutocompleteAsync_WhenApiKeyMissingOrDefault_ReturnsEmptyListWithoutHttpCall(string? configuredKey)
    {
        // Arrange
        _configurationMock.Setup(c => c["ExternalApis:Geoapify:ApiKey"]).Returns(configuredKey);

        var called = false;
        var httpClient = CreateHttpClient(_ =>
        {
            called = true;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var sut = new GeoapifyService(httpClient, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.AutocompleteAsync("Main Street");

        // Assert
        result.Should().BeEmpty();
        called.Should().BeFalse();
    }

    // =========================================================
    // Success / Mapping Tests
    // =========================================================

    [Fact]
    public async Task AutocompleteAsync_WhenValidResponse_ParsesAndMapsDto()
    {
        // Arrange
        const string json = """
        {
            "type": "FeatureCollection",
            "features": [
                {
                    "type": "Feature",
                    "properties": {
                        "country": "Germany",
                        "country_code": "de",
                        "city": "Berlin",
                        "postcode": "10115",
                        "street": "Friedrichstraße",
                        "housenumber": "100",
                        "formatted": "Friedrichstraße 100, 10115 Berlin, Germany"
                    }
                }
            ]
        }
        """;

        var httpClient = CreateHttpClient(req =>
        {
            req.RequestUri!.ToString().Should().Contain("text=Friedrich");
            req.RequestUri.ToString().Should().Contain("apiKey=test-api-key");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var sut = new GeoapifyService(httpClient, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.AutocompleteAsync("Friedrich");

        // Assert
        result.Should().HaveCount(1);
        var item = result.First();
        item.Street.Should().Be("Friedrichstraße");
        item.HouseNumber.Should().Be("100");
        item.City.Should().Be("Berlin");
        item.Country.Should().Be("Germany");
        item.CountryCode.Should().Be("de");
        item.PostalCode.Should().Be("10115");
        item.FormattedAddress.Should().Be("Friedrichstraße 100, 10115 Berlin, Germany");
    }

    [Fact]
    public async Task AutocompleteAsync_WhenStreetMissing_FallsBackToAddressLine1()
    {
        // Arrange
        const string json = """
        {
            "features": [
                {
                    "properties": {
                        "country": "USA",
                        "city": "Springfield",
                        "address_line1": "742 Evergreen Terrace",
                        "formatted": "742 Evergreen Terrace, Springfield, USA"
                    }
                }
            ]
        }
        """;

        var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var sut = new GeoapifyService(httpClient, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.AutocompleteAsync("Evergreen");

        // Assert
        result.Should().HaveCount(1);
        result.First().Street.Should().Be("742 Evergreen Terrace");
        result.First().City.Should().Be("Springfield");
    }

    [Fact]
    public async Task AutocompleteAsync_WhenNoFeaturesProperty_ReturnsEmptyList()
    {
        // Arrange
        const string json = """{ "type": "FeatureCollection" }""";

        var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var sut = new GeoapifyService(httpClient, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.AutocompleteAsync("Broadway");

        // Assert
        result.Should().BeEmpty();
    }

    // =========================================================
    // Error Handling Tests
    // =========================================================

    [Fact]
    public async Task AutocompleteAsync_WhenHttpFails_HandlesGracefullyAndReturnsEmpty()
    {
        // Arrange
        var handler = new FaultyHttpMessageHandler(new HttpRequestException("Network failure"));
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.geoapify.com/v1/geocode/")
        };

        var sut = new GeoapifyService(httpClient, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.AutocompleteAsync("Main Street");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AutocompleteAsync_WhenJsonIsMalformed_HandlesGracefullyAndReturnsEmpty()
    {
        // Arrange
        var httpClient = CreateHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("INVALID JSON CONTENT", Encoding.UTF8, "application/json")
        });

        var sut = new GeoapifyService(httpClient, _configurationMock.Object, _loggerMock.Object);

        // Act
        var result = await sut.AutocompleteAsync("Sunset Boulevard");

        // Assert
        result.Should().BeEmpty();
    }

    // =========================================================
    // Test Helpers
    // =========================================================

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private class FaultyHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public FaultyHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromException<HttpResponseMessage>(_exception);
        }
    }
}
