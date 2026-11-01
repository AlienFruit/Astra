using AlienFruit.Astra.Core;
using AlienFruit.Astra.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Html;
using Moq;
using System.IO;

namespace AlienFruit.Astra.Tests.UnitTests;

public class HtmlResourceRendererTests : AstraTestBase
{
    private HtmlResourceRenderer CreateHtmlResourceRenderer() =>
        new(ResourceStorageMock.Object, CreateAstraConfiguration());

    [Fact]
    public void AddResource_NewResource_RegistersCorrectly()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-resource.css";
        var path = "Test.Path.Css";
        var assembly = typeof(HtmlResourceRendererTests).Assembly;

        // Act
        renderer.AddResource(name, path, AstraResourceLocation.Header, assembly);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, path, assembly), Times.Once);
    }

    [Fact]
    public void AddJsCodeFromTemplate_ValidTemplate_RegistersCorrectly()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-template.js";
        var templatePath = "AlienFruit.Astra.Tests.Resources.test-template.js";
        var specification = new { Message = "test" };

        // Act
        renderer.AddJsCodeFromTemplate(name, templatePath, specification);

        // Assert
        ResourceStorageMock.Verify(x => x.RegisterResource(name, It.IsAny<string>()), Times.Once);
    }


}
