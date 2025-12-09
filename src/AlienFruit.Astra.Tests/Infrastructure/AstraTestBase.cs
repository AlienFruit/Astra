using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Configuration;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Moq;
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

    protected AstraTestBase()
    {
        HtmlResourceRendererMock = new Mock<IHtmlResourceRenderer>();
        ResourceCompressorMock = new Mock<IResourceCompressor>();
        ResourceStorageMock = new Mock<IResourceStorage>();
    }

    /// <summary>
    /// Создает TagHelperContext для тестирования TagHelper'ов
    /// </summary>
    protected TagHelperContext CreateTagHelperContext(string tagName = "test")
    {
        var attributes = new TagHelperAttributeList();
        var items = new Dictionary<object, object>();
        var uniqueId = Guid.NewGuid().ToString();

        return new TagHelperContext(tagName, attributes, items, uniqueId);
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
    /// Настраивает HtmlResourceRendererMock для возврата body ресурса
    /// </summary>
    protected void SetupHtmlResourceRendererRenderBodyResource(string resourceName, string htmlContent)
    {
        var htmlString = new HtmlString(htmlContent);
        HtmlResourceRendererMock.Setup(x => x.RenderBodyResource(resourceName)).Returns(htmlString);
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
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
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
}
