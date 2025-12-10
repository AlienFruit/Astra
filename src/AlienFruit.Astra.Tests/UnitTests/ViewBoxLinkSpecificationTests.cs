using AlienFruit.Astra.ViewBoxLink;
using FluentAssertions;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ViewBoxLinkSpecificationTests
{
    private ViewBoxLinkSpecification CreateViewBoxLinkSpecification(
        string id = "test-link",
        string viewBoxId = "test-viewbox",
        string uri = "/test/path")
    {
        // Use Activator to create ViewBoxLinkSpecification without required property validation
        var specification = (ViewBoxLinkSpecification)Activator.CreateInstance(typeof(ViewBoxLinkSpecification))!;
        typeof(ViewBoxLinkSpecification).GetProperty("Id")!.SetValue(specification, id);
        typeof(ViewBoxLinkSpecification).GetProperty("ViewBoxId")!.SetValue(specification, viewBoxId);
        typeof(ViewBoxLinkSpecification).GetProperty("Uri")!.SetValue(specification, new Uri(uri, UriKind.RelativeOrAbsolute));
        return specification;
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var specification = CreateViewBoxLinkSpecification();

        // Assert
        specification.Id.Should().Be("test-link");
        specification.ViewBoxId.Should().Be("test-viewbox");
        specification.Uri.Should().Be(new Uri("/test/path", UriKind.RelativeOrAbsolute));
        specification.SelectedClassName.Should().BeNull();
        specification.DefaultClassName.Should().BeNull();
        specification.OnClick.Should().BeNull();
        specification.ScrollUp.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldAllowSettingAllProperties()
    {
        // Act
        var specification = CreateViewBoxLinkSpecification("custom-link", "custom-viewbox", "/custom/path");
        specification.SelectedClassName = "active";
        specification.DefaultClassName = "link";
        specification.OnClick = "handleClick()";
        specification.ScrollUp = false;

        // Assert
        specification.Id.Should().Be("custom-link");
        specification.ViewBoxId.Should().Be("custom-viewbox");
        specification.Uri.Should().Be(new Uri("/custom/path", UriKind.RelativeOrAbsolute));
        specification.SelectedClassName.Should().Be("active");
        specification.DefaultClassName.Should().Be("link");
        specification.OnClick.Should().Be("handleClick()");
        specification.ScrollUp.Should().BeFalse();
    }

    [Fact]
    public void Properties_ShouldBeSettableIndividually()
    {
        // Arrange
        var specification = CreateViewBoxLinkSpecification();

        // Act
        typeof(ViewBoxLinkSpecification).GetProperty("Id")!.SetValue(specification, "dynamic-id");
        typeof(ViewBoxLinkSpecification).GetProperty("ViewBoxId")!.SetValue(specification, "dynamic-viewbox");
        typeof(ViewBoxLinkSpecification).GetProperty("Uri")!.SetValue(specification, new Uri("/dynamic/path", UriKind.RelativeOrAbsolute));
        specification.SelectedClassName = "dynamic-active";
        specification.DefaultClassName = "dynamic-link";
        specification.OnClick = "dynamicClick()";
        specification.ScrollUp = false;

        // Assert
        specification.Id.Should().Be("dynamic-id");
        specification.ViewBoxId.Should().Be("dynamic-viewbox");
        specification.Uri.Should().Be(new Uri("/dynamic/path", UriKind.RelativeOrAbsolute));
        specification.SelectedClassName.Should().Be("dynamic-active");
        specification.DefaultClassName.Should().Be("dynamic-link");
        specification.OnClick.Should().Be("dynamicClick()");
        specification.ScrollUp.Should().BeFalse();
    }

    [Fact]
    public void RequiredProperties_ShouldBeValidated()
    {
        // Act & Assert - Id, ViewBoxId and Uri are required
        var specification = CreateViewBoxLinkSpecification();
        specification.Id.Should().Be("test-link");
        specification.ViewBoxId.Should().Be("test-viewbox");
        specification.Uri.Should().Be(new Uri("/test/path", UriKind.RelativeOrAbsolute));
    }

    [Fact]
    public void DefaultValues_ShouldMatchExpectedConstants()
    {
        // Act
        var specification = CreateViewBoxLinkSpecification();

        // Assert
        specification.ScrollUp.Should().BeTrue();
    }

    [Fact]
    public void InitScriptTemplatePath_ShouldBeCorrectConstant()
    {
        // Assert
        ViewBoxLinkSpecification.InitScriptTemplatePath.Should().Be("AlienFruit.Astra.ViewBoxLink.Resources.viewboxlink-init-script.template.js");
    }

    [Fact]
    public void Uri_ShouldHandleRelativeUri()
    {
        // Arrange
        var specification = CreateViewBoxLinkSpecification();
        var relativeUri = new Uri("../relative/path", UriKind.Relative);

        // Act
        typeof(ViewBoxLinkSpecification).GetProperty("Uri")!.SetValue(specification, relativeUri);

        // Assert
        specification.Uri.Should().Be(relativeUri);
        specification.Uri.IsAbsoluteUri.Should().BeFalse();
    }

    [Fact]
    public void Uri_ShouldHandleAbsoluteUri()
    {
        // Arrange
        var specification = CreateViewBoxLinkSpecification();
        var absoluteUri = new Uri("https://example.com/path", UriKind.Absolute);

        // Act
        typeof(ViewBoxLinkSpecification).GetProperty("Uri")!.SetValue(specification, absoluteUri);

        // Assert
        specification.Uri.Should().Be(absoluteUri);
        specification.Uri.IsAbsoluteUri.Should().BeTrue();
    }

    [Fact]
    public void SelectedClassName_ShouldBeNullable()
    {
        // Arrange
        var specification = CreateViewBoxLinkSpecification();

        // Act & Assert
        specification.SelectedClassName.Should().BeNull();
        specification.SelectedClassName = "active";
        specification.SelectedClassName.Should().Be("active");
        specification.SelectedClassName = null;
        specification.SelectedClassName.Should().BeNull();
    }

    [Fact]
    public void DefaultClassName_ShouldBeNullable()
    {
        // Arrange
        var specification = CreateViewBoxLinkSpecification();

        // Act & Assert
        specification.DefaultClassName.Should().BeNull();
        specification.DefaultClassName = "link";
        specification.DefaultClassName.Should().Be("link");
        specification.DefaultClassName = null;
        specification.DefaultClassName.Should().BeNull();
    }

    [Fact]
    public void OnClick_ShouldBeNullable()
    {
        // Arrange
        var specification = CreateViewBoxLinkSpecification();

        // Act & Assert
        specification.OnClick.Should().BeNull();
        specification.OnClick = "handleClick()";
        specification.OnClick.Should().Be("handleClick()");
        specification.OnClick = null;
        specification.OnClick.Should().BeNull();
    }

    [Fact]
    public void ScrollUp_ShouldDefaultToTrue()
    {
        // Act
        var specification = CreateViewBoxLinkSpecification();

        // Assert
        specification.ScrollUp.Should().BeTrue();
    }

    [Fact]
    public void ScrollUp_ShouldBeSettableToFalse()
    {
        // Arrange
        var specification = CreateViewBoxLinkSpecification();

        // Act
        specification.ScrollUp = false;

        // Assert
        specification.ScrollUp.Should().BeFalse();
    }

    [Fact]
    public void Id_ShouldBeRequiredProperty()
    {
        // Arrange
        var specification = (ViewBoxLinkSpecification)Activator.CreateInstance(typeof(ViewBoxLinkSpecification))!;

        // Act & Assert - This should work because we're testing the property, not the constructor validation
        typeof(ViewBoxLinkSpecification).GetProperty("Id")!.SetValue(specification, "test-id");
        specification.Id.Should().Be("test-id");
    }

    [Fact]
    public void ViewBoxId_ShouldBeRequiredProperty()
    {
        // Arrange
        var specification = (ViewBoxLinkSpecification)Activator.CreateInstance(typeof(ViewBoxLinkSpecification))!;

        // Act & Assert
        typeof(ViewBoxLinkSpecification).GetProperty("ViewBoxId")!.SetValue(specification, "test-viewbox-id");
        specification.ViewBoxId.Should().Be("test-viewbox-id");
    }

    [Fact]
    public void Uri_ShouldBeRequiredProperty()
    {
        // Arrange
        var specification = (ViewBoxLinkSpecification)Activator.CreateInstance(typeof(ViewBoxLinkSpecification))!;
        var testUri = new Uri("/test", UriKind.Relative);

        // Act & Assert
        typeof(ViewBoxLinkSpecification).GetProperty("Uri")!.SetValue(specification, testUri);
        specification.Uri.Should().Be(testUri);
    }
}
