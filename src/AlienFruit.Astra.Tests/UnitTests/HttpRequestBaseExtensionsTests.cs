using AlienFruit.Astra.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AlienFruit.Astra.Tests.UnitTests;

public class HttpRequestBaseExtensionsTests
{
    [Fact]
    public void IsAjaxRequest_WithAjaxHeader_ReturnsTrue()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var headers = new HeaderDictionary();
        headers.Add("Ajax-Request", "true");
        requestMock.SetupGet(x => x.Headers).Returns(headers);
        var request = requestMock.Object;

        // Act
        var result = request.IsAjaxRequest();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAjaxRequest_WithoutAjaxHeader_ReturnsFalse()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var headers = new HeaderDictionary();
        requestMock.SetupGet(x => x.Headers).Returns(headers);
        var request = requestMock.Object;

        // Act
        var result = request.IsAjaxRequest();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAjaxRequest_WithAjaxHeaderValue_ReturnsTrue()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var headers = new HeaderDictionary();
        headers.Add("Ajax-Request", "1");
        requestMock.SetupGet(x => x.Headers).Returns(headers);
        var request = requestMock.Object;

        // Act
        var result = request.IsAjaxRequest();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAjaxRequest_WithEmptyAjaxHeaderValue_ReturnsTrue()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var headers = new HeaderDictionary();
        headers.Add("Ajax-Request", "");
        requestMock.SetupGet(x => x.Headers).Returns(headers);
        var request = requestMock.Object;

        // Act
        var result = request.IsAjaxRequest();

        // Assert
        result.Should().BeTrue();
    }
}
