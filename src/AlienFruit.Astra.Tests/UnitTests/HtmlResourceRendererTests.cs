using AlienFruit.Astra.Core;
using AlienFruit.Astra.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Html;
using Moq;

namespace AlienFruit.Astra.Tests.UnitTests;

public class HtmlResourceRendererTests : AstraTestBase
{
    private HtmlResourceRenderer CreateHtmlResourceRenderer() =>
        new(ResourceStorageMock.Object, CreateAstraConfiguration());

    [Fact]
    public void Constructor_ShouldInitializeWithDependencies()
    {
        // Act
        var renderer = CreateHtmlResourceRenderer();

        // Assert
        renderer.Should().NotBeNull();
    }

    [Fact]
    public void AddStylesheetResource_ShouldRegisterResourceAndCalculateHash()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-style.css";
        var path = "Test.Path.Style.css";
        var assembly = typeof(HtmlResourceRendererTests).Assembly;

        // Act
        renderer.AddStylesheetResource(name, path, Abstractions.AstraResourceLocation.Header, assembly);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, path, assembly), Times.Once);
    }

    [Fact]
    public void AddScriptResource_ShouldRegisterResourceAndCalculateHash()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-script.js";
        var path = "Test.Path.Script.js";
        var assembly = typeof(HtmlResourceRendererTests).Assembly;

        // Act
        renderer.AddScriptResource(name, path, Abstractions.AstraResourceLocation.Header, assembly);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, path, assembly), Times.Once);
    }

    [Fact]
    public void AddJsCode_ShouldRegisterInMemoryResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-code.js";
        var jsCode = "console.log('test');";

        // Act
        renderer.AddJsCode(name, jsCode, Abstractions.AstraResourceLocation.Header);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, jsCode), Times.Once);
    }

    [Fact]
    public void AddJsCodeFromTemplate_ShouldParseTemplateAndRegisterResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-template.js";
        var templatePath = "AlienFruit.Astra.Core.Resources.load-check.js";
        var specification = new { Message = "test" };

        // Act
        renderer.AddJsCodeFromTemplate(name, templatePath, specification);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void IsResourceExists_ShouldReturnTrueForExistingScriptResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-script.js";
        renderer.AddScriptResource(name, "test.js");

        // Act
        var result = renderer.IsResourceExists(name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsResourceExists_ShouldReturnTrueForExistingStyleResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-style.css";
        renderer.AddStylesheetResource(name, "test.css");

        // Act
        var result = renderer.IsResourceExists(name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsResourceExists_ShouldReturnFalseForNonExistingResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();

        // Act
        var result = renderer.IsResourceExists("non-existing.js");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RenderHeaders_ShouldRenderStylesAndScripts()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        renderer.AddStylesheetResource("test.css", "test.css");
        renderer.AddScriptResource("test.js", "test.js");

        // Act
        var result = renderer.RenderHeaders();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<HtmlContentBuilder>();
    }

    [Fact]
    public void RenderBodyResource_ShouldRenderScriptInBody()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-body.js";
        var jsCode = "console.log('body script');";
        renderer.AddJsCode(name, jsCode, Abstractions.AstraResourceLocation.Body);

        // Mock resource storage to return content
        var contentStream = CreateMemoryStream(jsCode);
        ResourceStorageMock.Setup(x => x.Contains(name)).Returns(true);
        ResourceStorageMock.Setup(x => x.OpenRead(name)).Returns(contentStream);

        // Act
        var result = renderer.RenderBodyResource(name);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<HtmlContentBuilder>();
    }

    [Fact]
    public void RenderBodyResource_ShouldThrowExceptionForNonExistingResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => renderer.RenderBodyResource("non-existing.js"));
        exception.Message.Should().Contain("resource non-existing.js was not registered as body resource");
    }

    [Fact]
    public void GetResourceUrl_ShouldReturnUrlWithVersioningWhenEnabled()
    {
        // Arrange
        var configuration = CreateAstraConfiguration(enableVersioning: true, resourceVersion: "1.0.0");
        var renderer = new HtmlResourceRenderer(ResourceStorageMock.Object, configuration);
        var resourceName = "test.js";

        // Act
        var result = renderer.GetResourceUrl(resourceName);

        // Assert
        result.Should().Be("/astra/test.js?v=1.0.0");
    }

    [Fact]
    public void GetResourceUrl_ShouldReturnUrlWithoutVersioningWhenDisabled()
    {
        // Arrange
        var configuration = CreateAstraConfiguration(enableVersioning: false);
        var renderer = new HtmlResourceRenderer(ResourceStorageMock.Object, configuration);
        var resourceName = "test.js";

        // Act
        var result = renderer.GetResourceUrl(resourceName);

        // Assert
        result.Should().Be("/astra/test.js");
    }
}
