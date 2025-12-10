using AlienFruit.Astra.Core;
using AlienFruit.Astra.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Html;
using Moq;
using System.IO;
using System.Text.Encodings.Web;

namespace AlienFruit.Astra.Tests.UnitTests;

public class HtmlResourceRendererTests : AstraTestBase
{
    private HtmlResourceRenderer CreateHtmlResourceRenderer() =>
        new(ResourceStorageMock.Object, CreateAstraConfiguration());

    [Fact]
    public void AddStylesheetResource_NewResourceName_RegisterResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-style.css";
        var path = "Test.Path.Style.css";
        var assembly = typeof(HtmlResourceRendererTests).Assembly;

        // Act
        renderer.AddStylesheetResource(name, path, AstraResourceLocation.Header, assembly);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, path, assembly), Times.Once);
    }

    [Fact]
    public void AddScriptResource_NewResourceName_RegisterResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-script.js";
        var path = "Test.Path.Script.js";
        var assembly = typeof(HtmlResourceRendererTests).Assembly;

        // Act
        renderer.AddScriptResource(name, path, AstraResourceLocation.Header, assembly);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, path, assembly), Times.Once);
    }

    [Fact]
    public void AddJsCode_NewResourceName_RegisterInMemoryResource()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-code.js";
        var jsCode = "console.log('test');";

        // Act
        renderer.AddJsCode(name, jsCode, AstraResourceLocation.Header);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, jsCode), Times.Once);
    }

    [Fact]
    public void AddJsCodeFromTemplate_ValidTemplate_ParseAndRegisterResource()
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
    public void IsResourceExists_ExistingScriptResource_ReturnTrue()
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
    public void IsResourceExists_ExistingStyleResource_ReturnTrue()
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
    public void IsResourceExists_NonExistingResource_ReturnFalse()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();

        // Act
        var result = renderer.IsResourceExists("non-existing.js");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RenderHeaders_WithResources_RenderStylesAndScripts()
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

        // Check HTML content
        using var stringWriter = new StringWriter();
        result.WriteTo(stringWriter, HtmlEncoder.Default);
        var htmlContent = stringWriter.ToString();

        htmlContent.Should().Contain("<link href=\"/astra/test.css\" rel=\"stylesheet\" type=\"text/css\" />");
        htmlContent.Should().Contain("<script src=\"/astra/test.js\"></script>");
    }

    [Fact]
    public void RenderBodyResource_RegisteredBodyResource_RenderScript()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-body.js";
        var jsCode = "console.log('body script');";
        renderer.AddJsCode(name, jsCode, AstraResourceLocation.Body);

        // Mock resource storage to return content
        var contentStream = CreateMemoryStream(jsCode);
        ResourceStorageMock.Setup(x => x.Contains(name)).Returns(true);
        ResourceStorageMock.Setup(x => x.OpenRead(name)).Returns(contentStream);

        // Act
        var result = renderer.RenderBodyResource(name);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<HtmlContentBuilder>();

        // Check HTML content
        using var stringWriter = new StringWriter();
        result.WriteTo(stringWriter, HtmlEncoder.Default);
        var htmlContent = stringWriter.ToString();

        htmlContent.Should().Contain("<script>");
        htmlContent.Should().Contain(jsCode);
        htmlContent.Should().Contain("</script>");
    }

    [Fact]
    public void RenderBodyResource_NonExistingResource_ThrowException()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => renderer.RenderBodyResource("non-existing.js"));
        exception.Message.Should().Contain("resource non-existing.js was not registered as body resource");
    }

    [Fact]
    public void GetResourceUrl_VersioningEnabled_ReturnUrlWithVersion()
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
    public void GetResourceUrl_VersioningDisabled_ReturnUrlWithoutVersion()
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
