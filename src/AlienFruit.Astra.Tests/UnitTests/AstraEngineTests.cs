using AlienFruit.Astra.Tests.Infrastructure;
using Microsoft.AspNetCore.Html;

namespace AlienFruit.Astra.Tests.UnitTests;

public class AstraEngineTests : AstraTestBase
{
    private AstraEngine CreateAstraEngine() => new(HtmlResourceRendererMock.Object, InitScriptRendererMock.Object);

    [Fact]
    public void Constructor_OnInitialize_AddRequiredResources()
    {
        // Act
        var engine = CreateAstraEngine();
        // Assert
        HtmlResourceRendererMock.Verify(x => x.AddResource(
            "viewbox.js",
            ViewBoxSpecification.JsResourcePath), Times.Once);
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
        HtmlResourceRendererMock.Verify(m => m.AddResource(
            "viewbox.js",
            ViewBoxSpecification.JsResourcePath), Times.Once);
        HtmlResourceRendererMock.Verify(m => m.RenderHeaders(), Times.Once);
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
        result.Should().BeOfType<HtmlContentBuilder>();
        // Check the content by writing to StringWriter
        // Using GetContent from AstraTestBase
        var content = GetContent(result);
        content.Should().Contain(expectedHeadersHtml);
        content.Should().Contain("<meta id=\"load-check\">");
    }

    [Fact]
    public void IncompleteLoadCheck_OnCall_RenderExpectedBodyResource()
    {
        // Arrange
        var engine = CreateAstraEngine();
        var expectedHtml = "<script><script>load check code</script></script>";
        SetupInitScriptRendererGetResource("AlienFruit.Astra.Core.Resources.load-check.js", expectedHtml);
        // Act
        var result = engine.IncompleteLoadCheck();
        // Assert
        GetContent(result).Replace("\r\n", "\n").Should().Contain(expectedHtml.Replace("\r\n", "\n"));
        // Verify that the script was actually retrieved from the renderer
        InitScriptRendererMock.Verify(x => x.GetResource("AlienFruit.Astra.Core.Resources.load-check.js"), Times.Once);
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
    public void EngineRenderHeaders_DoesNotPreRegisterTagHelperInitNames()
    {
        HtmlResourceRendererMock.Setup(x => x.RenderHeaders()).Returns(new HtmlContentBuilder().AppendHtml("<script src='viewbox.js'></script>"));
        var engine = CreateAstraEngine();
        GetContent(engine.RenderHeaders()).Should().Contain("viewbox.js");
        GetContent(engine.RenderHeaders()).Should().NotContain("viewbox-init-main.js");
        GetContent(engine.RenderHeaders()).Should().NotContain("viewbox-link-init-home");
    }
}
