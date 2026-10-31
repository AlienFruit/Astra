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

    [Fact]
    public void IsResourceExists_Existing_ReturnsTrue()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        var name = "test-script.js";
        renderer.AddResource(name, "test.js", AstraResourceLocation.Header);
        ResourceStorageMock.Setup(x => x.Contains(name)).Returns(true);

        // Act
        var result = renderer.IsResourceExists(name);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsResourceExists_NonExisting_ReturnsFalse()
    {
        // Arrange
        var renderer = CreateHtmlResourceRenderer();
        ResourceStorageMock.Setup(x => x.Contains("non-existing.js")).Returns(false);

        // Act
        var result = renderer.IsResourceExists("non-existing.js");

        // Assert
        result.Should().BeFalse();
    }
}
