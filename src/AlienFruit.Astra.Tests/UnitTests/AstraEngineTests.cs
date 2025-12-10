using System.IO;
using System.Text.Encodings.Web;
using AlienFruit.Astra.Tests.Infrastructure;

namespace AlienFruit.Astra.Tests.UnitTests;

public class AstraEngineTests : AstraTestBase
{
    private AstraEngine CreateAstraEngine() => new(HtmlResourceRendererMock.Object);

    [Fact]
    public void Constructor_OnInitialize_AddRequiredResources()
    {
        // Act
        var engine = CreateAstraEngine();

        // Assert
        engine.Should().NotBeNull();
        HtmlResourceRendererMock.Verify(x => x.AddScriptResource(
            "load-check.js",
            "AlienFruit.Astra.Core.Resources.load-check.js",
            AstraResourceLocation.Body), Times.Once);
    }

    [Fact]
    public void AddViewBox_WithViewBoxSpecification_AddJsCodeWithCorrectResourceNamePathAndSpec()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var specification = new ViewBoxSpecification
        {
            Id = "test-viewbox",
            ConnectionErrorMessage = "Connection error"
        };

        // Act
        engine.AddViewBox(specification);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            $"viewbox-init-{specification.Id}.js",
            ViewBoxSpecification.InitScriptTemplatePath,
            specification), Times.Once);
    }

    [Fact]
    public void AddViewBoxLink_WithViewBoxLinkSpecification_AddJsCodeWithCorrectResourceNamePathAndSpec()
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

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            $"viewbox-link-init-{specification.Id}.js",
            ViewBoxLinkSpecification.InitScriptTemplatePath,
            specification), Times.Once);
    }

    [Fact]
    public void AddViewBoxLink_WithDuplicateSpecification_AddJsCodeOnce()
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
        engine
            .AddViewBoxLink(specification)
            .AddViewBoxLink(specification);

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddJsCodeFromTemplate(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<ViewBoxLinkSpecification>()), Times.Once); // Only called once for first addition
    }

    [Fact]
    public void RenderHeaders_OnFirstCall_AddViewBoxScript()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var expectedHeadersHtml = "<script src='viewbox.js'></script>";
        SetupHtmlResourceRendererRenderHeaders(expectedHeadersHtml);

        // Act
        engine.RenderHeaders();

        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddScriptResource(
            "viewbox.js",
            ViewBoxSpecification.JsResourcePath), Times.Once);
        HtmlResourceRendererMock.Verify(x => x.RenderHeaders(), Times.Once);
    }

    [Fact]
    public void RenderHeaders_WithHeaders_SetupHtmlContentBuilderWithExpectedHeaders()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var expectedHeadersHtml = "<script src='viewbox.js'></script>";
        SetupHtmlResourceRendererRenderHeaders(expectedHeadersHtml);

        // Act
        var result = engine.RenderHeaders();

        // Assert
        // The result should be HtmlContentBuilder containing the rendered headers and meta tag
        result.Should().NotBeNull();
        result.Should().BeOfType<Microsoft.AspNetCore.Html.HtmlContentBuilder>();

        // Check the content by writing to StringWriter
        using var writer = new StringWriter();
        result.WriteTo(writer, HtmlEncoder.Default);
        var content = writer.ToString();
        content.Should().Be($"{expectedHeadersHtml}<meta id=\"load-check\">\r\n");
    }

    [Fact]
    public void IncompleteLoadCheck_OnCall_RenderExpectedBodyResource()
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
    public void RouteLayout_AjaxRequest_ReturnNull()
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
    public void RouteLayout_RegularRequest_ReturnDefaultLayout()
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
    public void GetViewBoxLinkClass_ActiveLink_ReturnSelectedClass()
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
    public void GetViewBoxLinkClass_InactiveLink_ReturnDefaultClass()
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
    public void GetViewBoxLinkClass_NonExistentLink_ThrowException()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var context = new DefaultHttpContext();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => engine.GetViewBoxLinkClass(context, "non-existent"));
        exception.Message.Should().Contain("There is no NavLink with id:non-existent");
    }
}
