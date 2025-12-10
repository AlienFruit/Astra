using AlienFruit.Astra.ViewBoxLink;
using FluentAssertions;

namespace AlienFruit.Astra.Tests.UnitTests;

public class ViewBoxLinkSpecificationTests
{
    [Fact]
    public void Create_ObjectInitializer_DefaultValuesApplied()
    {
        // Act
        var specification = new ViewBoxLinkSpecification
        {
            Id = "test-link",
            ViewBoxId = "test-viewbox",
            Uri = new Uri("/test/path", UriKind.RelativeOrAbsolute)
        };

        // Assert
        specification.SelectedClassName.Should().BeNull();
        specification.DefaultClassName.Should().BeNull();
        specification.OnClick.Should().BeNull();
        specification.ScrollUp.Should().BeTrue();
    }
}
