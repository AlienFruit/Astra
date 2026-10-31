using System.IO;
using System.Text.Encodings.Web;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.Core.ResourceCompressors;
using AlienFruit.Astra.Tests.Infrastructure;
using AlienFruit.Astra.ViewBox;
using AlienFruit.Astra.ViewBoxLink;
using Microsoft.AspNetCore.Http;

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
        HtmlResourceRendererMock.Verify(x => x.AddResource(
            "load-check.js",
            "AlienFruit.Astra.Core.Resources.load-check.js",
            AstraResourceLocation.Body), Times.Once);
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
        HtmlResourceRendererMock.Verify(x => x.AddResource(
            "viewbox.js",
            ViewBoxSpecification.JsResourcePath), Times.Once);
        HtmlResourceRendererMock.Verify(x.RenderHeaders(), Times.Once);
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

    // Regression for the proven ViewBox collision: AstraEngine used to register
    // "viewbox-init-{Id}.js" as a Header resource (AddViewBox), and ViewBox.Process
    // registered the same name as Body → first-write-wins left location=Header,
    // RenderBodyResource threw "resource ... was not registered as body resource".
    // The engine now registers only startup assets; tag-helper init rendering stays
    // per-instance, so mixed engine + tag helper usage must work.

    [Fact]
    public void EngineRenderHeaders_DoesNotPreRegisterTagHelperInitNames()
    {
        var renderer = CreateRealRenderer();
        var engine = new AstraEngine(renderer);

        engine.RenderHeaders();

        renderer.IsResourceExists("viewbox.js").Should().BeTrue();
        renderer.IsResourceExists("viewbox-init-main.js").Should().BeFalse();
        renderer.IsResourceExists("viewbox-link-init-home").Should().BeFalse();
    }

    [Fact]
    public void RenderHeaders_Then_ViewBoxTagHelper_DoesNotThrow()
    {
        // The exact mixed-usage crash from the bug report: engine headers first,
        // then <view-box id="main"> and <view-box-link> rendered through the real
        // renderer/storage pipeline. Before the fix the box threw ArgumentException.
        var renderer = CreateRealRenderer();
        var compressor = new StubCompressor();
        var engine = new AstraEngine(renderer);
        engine.RenderHeaders();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/Home";

        var viewBox = new AlienFruit.Astra.ViewBox.ViewBox(renderer, compressor)
        {
            Id = "main"
        };
        var boxOutput = CreateTagHelperOutput("view-box");
        var boxAct = () => viewBox.Process(CreateTagHelperContext("view-box"), boxOutput);

        var link = new AlienFruit.Astra.ViewBoxLink.ViewBoxLink(
            renderer, new HttpContextAccessor { HttpContext = httpContext })
        {
            Id = "home",
            Uri = "/Home",
            ViewBoxId = "main",
            DefaultClassName = "nav-link",
            SelectedClassName = "nav-link active"
        };
        var linkOutput = CreateTagHelperOutput("view-box-link");
        var linkAct = () => link.Process(CreateTagHelperContext("view-box-link"), linkOutput);

        boxAct.Should().NotThrow();
        linkAct.Should().NotThrow();

        // init scripts are rendered inline into the helper output (Body), not served from Header
        WriteTo(boxOutput).Should().Contain("ViewBoxRegistry.create('main'");
        WriteTo(linkOutput).Should().Contain("registerUri('/Home', 'home'");
    }

    private HtmlResourceRenderer CreateRealRenderer() =>
        new(new ResourceStorage(new StubCompressor()), CreateAstraConfiguration());

    private static string WriteTo(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        using var writer = new StringWriter();
        output.PostContent.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
