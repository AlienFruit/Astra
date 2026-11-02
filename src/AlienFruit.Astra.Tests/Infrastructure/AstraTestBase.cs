using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using System.Reflection;

namespace AlienFruit.Astra.Tests.Infrastructure;

/// <summary>
/// Базовый класс для всех тестов Astra, предоставляющий общие моки и хелперы
/// </summary>
public abstract class AstraTestBase
{
    protected Mock<IHtmlResourceRenderer> HtmlResourceRendererMock { get; }
    protected Mock<IResourceCompressor> ResourceCompressorMock { get; }
    protected Mock<IResourceStorage> ResourceStorageMock { get; }

    protected Mock<IInitScriptRenderer> InitScriptRendererMock { get; }

    protected AstraTestBase()
    {
        HtmlResourceRendererMock = new Mock<IHtmlResourceRenderer>();
        ResourceCompressorMock = new Mock<IResourceCompressor>();
        ResourceStorageMock = new Mock<IResourceStorage>();
        InitScriptRendererMock = new Mock<IInitScriptRenderer>();
    }

    /// <summary>
    /// Настраивает InitScriptRendererMock для возврата контента по пути
    /// </summary>
    protected void SetupInitScriptRendererGetResource(string path, string content)
    {
        InitScriptRendererMock.Setup(x => x.GetResource(path)).Returns(content);
    }

    /// <summary>
    /// Создает TagHelperContext для тестирования TagHelper'ов
    /// </summary>
    protected TagHelperContext CreateTagHelperContext(string tagName = "test")
    {
        return new TagHelperContext(tagName, new TagHelperAttributeList(), new Dictionary<object, object>(), Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Создает TagHelperOutput для тестирования TagHelper'ов
    /// </summary>
    protected TagHelperOutput CreateTagHelperOutput(string tagName = "test")
    {
        var attributes = new TagHelperAttributeList();
        return new TagHelperOutput(tagName, attributes, (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }

    /// <summary>
    /// Настраивает HtmlResourceRendererMock для возврата указанного HTML контента
    /// </summary>
    protected void SetupHtmlResourceRendererRenderHeaders(string htmlContent)
    {
        var htmlString = new HtmlString(htmlContent);
        HtmlResourceRendererMock.Setup(x => x.RenderHeaders()).Returns(htmlString);
    }

    /// <summary>
    /// Настраивает ResourceCompressorMock для компрессии в строку
    /// </summary>
    protected void SetupResourceCompressorCompressToString(string expectedResult)
    {
        ResourceCompressorMock.Setup(x => x.CompressToString(It.IsAny<Resource>())).Returns(expectedResult);
    }

    /// <summary>
    /// Настраивает ResourceCompressorMock для компрессии в поток
    /// </summary>
    protected void SetupResourceCompressorCompressToStream(Stream expectedStream)
    {
        ResourceCompressorMock.Setup(x => x.CompressToStream(It.IsAny<Resource>())).Returns(expectedStream);
    }

    /// <summary>
    /// Вспомогательный метод для создания MemoryStream с текстовым содержимым
    /// </summary>
    protected Stream CreateMemoryStream(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    /// <summary>
    /// Вспомогательный метод для создания EmbeddedResource
    /// </summary>
    protected EmbeddedResource CreateEmbeddedResource(string name, string path, Assembly assembly)
    {
        return new EmbeddedResource(name, path, assembly);
    }

    /// <summary>
    /// Вспомогательный метод для создания InMemoryResource
    /// </summary>
    protected InMemoryResource CreateInMemoryResource(string name, string content)
    {
        return new InMemoryResource(name, content);
    }

    /// <summary>
    /// Создает AstraConfiguration для тестирования
    /// </summary>
    protected AstraConfiguration CreateAstraConfiguration(
        string resourcesRoute = "astra",
        bool enableVersioning = false,
        string? resourceVersion = null)
    {
        return new AstraConfiguration
        {
            ResourcesRoute = resourcesRoute,
            EnableVersioning = enableVersioning,
            ResourceVersion = resourceVersion
        };
    }

    protected string GetContent(IHtmlContent content)
    {
        TextWriter writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Creates a real renderer and init script renderer for integration-style tests.
    /// </summary>
    protected (IHtmlResourceRenderer, IInitScriptRenderer) CreateRealRenderer() => (
        new HtmlResourceRenderer(new ResourceStorage(ResourceCompressorMock.Object), CreateAstraConfiguration()),
        new InitScriptRenderer()
    );
}
