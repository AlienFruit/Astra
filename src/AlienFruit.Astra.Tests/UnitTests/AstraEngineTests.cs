using AlienFruit.Astra.Core;
using AlienFruit.Astra.Extensions;
using AlienFruit.Astra.Tests.Infrastructure;
using AlienFruit.Astra.ViewBox;
using AlienFruit.Astra.ViewBoxLink;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AlienFruit.Astra.Tests.UnitTests;

public class AstraEngineTests : AstraTestBase
{
    private AstraEngine CreateAstraEngine() => new(HtmlResourceRendererMock.Object);

    [Fact]
    public void Constructor_ShouldInitializeWithHtmlResourceRenderer()
    {
        // Act
        var engine = CreateAstraEngine();

        // Assert
        engine.Should().NotBeNull();
        HtmlResourceRendererMock.Verify(x => x.AddScriptResource(
            "load-check.js",
            "AlienFruit.Astra.Core.Resources.load-check.js",
            Abstractions.AstraResourceLocation.Body), Times.Once);
    }

    [Fact]
    public void AddViewBox_ShouldAddSpecificationAndCallRenderer()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var specification = new ViewBoxSpecification
        {
            Id = "test-viewbox",
            ConnectionErrorMessage = "Connection error"
        };

        // Act
        var result = engine.AddViewBox(specification);

        // Assert
        result.Should().Be(engine);
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            $"viewbox-init-{specification.Id}.js",
            ViewBoxSpecification.InitScriptTemplatePath,
            specification), Times.Once);
    }

    [Fact]
    public void AddViewBoxLink_ShouldAddNewSpecificationAndCallRenderer()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var specification = new ViewBoxLinkSpecification
        {
            Id = "test-link",
            ViewBoxId = "test-viewbox",
            Uri = new Uri("/test", UriKind.Relative)
        };

        // Act
        var result = engine.AddViewBoxLink(specification);

        // Assert
        result.Should().Be(engine);
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            $"viewbox-link-init-{specification.Id}.js",
            ViewBoxLinkSpecification.InitScriptTemplatePath,
            specification), Times.Once);
    }

    [Fact]
    public void AddViewBoxLink_ShouldNotAddDuplicateSpecification()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var specification = new ViewBoxLinkSpecification
        {
            Id = "test-link",
            ViewBoxId = "test-viewbox",
            Uri = new Uri("/test", UriKind.Relative)
        };

        // Act
        engine.AddViewBoxLink(specification);
        var result = engine.AddViewBoxLink(specification);

        // Assert
        result.Should().Be(engine);
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<ViewBoxLinkSpecification>()), Times.Once); // Only called once for first addition
    }

    [Fact]
    public void RenderHeaders_ShouldAddViewBoxScriptAndRenderHeaders()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var expectedHeadersHtml = "<script src='viewbox.js'></script>";
        SetupHtmlResourceRendererRenderHeaders(expectedHeadersHtml);

        // Act
        var result = engine.RenderHeaders();

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddScriptResource(
            "viewbox.js",
            ViewBoxSpecification.JsResourcePath), Times.Once);
        HtmlResourceRendererMock.Verify(x => x.RenderHeaders(), Times.Once);

        // The result should be HtmlContentBuilder containing the rendered headers and meta tag
        result.Should().NotBeNull();
        result.Should().BeOfType<Microsoft.AspNetCore.Html.HtmlContentBuilder>();
    }

    [Fact]
    public void IncompleteLoadCheck_ShouldRenderBodyResource()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var expectedHtml = "<script>load check code</script>";
        SetupHtmlResourceRendererRenderBodyResource("load-check.js", expectedHtml);

        // Act
        var result = engine.IncompleteLoadCheck();

        // Assert
        HtmlResourceRendererMock.Verify(x => x.RenderBodyResource("load-check.js"), Times.Once);
        result.ToString().Should().Be(expectedHtml);
    }

    [Fact]
    public void RouteLayout_ShouldReturnNullForAjaxRequest()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var context = new DefaultHttpContext();
        context.Request.Headers["Ajax-Request"] = "true";
        var defaultLayout = "_Layout";

        // Act
        var result = engine.RouteLayout(context, defaultLayout);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RouteLayout_ShouldReturnDefaultLayoutForRegularRequest()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var context = new DefaultHttpContext();
        // No Ajax-Request header for regular request
        var defaultLayout = "_Layout";

        // Act
        var result = engine.RouteLayout(context, defaultLayout);

        // Assert
        result.Should().Be(defaultLayout);
    }

    [Fact]
    public void GetViewBoxLinkClass_ShouldReturnSelectedClassForActiveLink()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var specification = new ViewBoxLinkSpecification
        {
            Id = "test-link",
            ViewBoxId = "test-viewbox",
            Uri = new Uri("/test", UriKind.Relative),
            SelectedClassName = "active",
            DefaultClassName = "inactive"
        };
        engine.AddViewBoxLink(specification);

        var context = new DefaultHttpContext();
        context.Request.Path = "/test";

        // Act
        var result = engine.GetViewBoxLinkClass(context, "test-link");

        // Assert
        result.Should().Be("active");
    }

    [Fact]
    public void GetViewBoxLinkClass_ShouldReturnDefaultClassForInactiveLink()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var specification = new ViewBoxLinkSpecification
        {
            Id = "test-link",
            ViewBoxId = "test-viewbox",
            Uri = new Uri("/test", UriKind.Relative),
            SelectedClassName = "active",
            DefaultClassName = "inactive"
        };
        engine.AddViewBoxLink(specification);

        var context = new DefaultHttpContext();
        context.Request.Path = "/other";

        // Act
        var result = engine.GetViewBoxLinkClass(context, "test-link");

        // Assert
        result.Should().Be("inactive");
    }

    [Fact]
    public void GetViewBoxLinkClass_ShouldThrowExceptionForNonExistentLink()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var context = new DefaultHttpContext();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => engine.GetViewBoxLinkClass(context, "non-existent"));
        exception.Message.Should().Contain("There is no NavLink with id:non-existent");
    }
}
