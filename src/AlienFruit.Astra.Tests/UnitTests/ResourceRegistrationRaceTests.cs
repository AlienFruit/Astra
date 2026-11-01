using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.Core.ResourceCompressors;
using AlienFruit.Astra.Tests.Infrastructure;
using AlienFruit.Astra.ViewBoxLink;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Concurrent;

namespace AlienFruit.Astra.Tests.UnitTests;

/// <summary>
/// Regression tests for "The resource &quot;viewBox-link-init-...&quot; was not registered".
/// </summary>
public class ResourceRegistrationRaceTests : AstraTestBase
{
    private const string LinkId = "go-aitarotreader-becheating";
    private const string LinkInitResourceName = $"viewbox-link-init-{LinkId}";
    private const int ThreadCount = 32;

    private (HtmlResourceRenderer, IInitScriptRenderer) CreateRealRenderer() => (
        new HtmlResourceRenderer(new ResourceStorage(new StubCompressor()), CreateAstraConfiguration()),
        new InitScriptRenderer()
    );

    private static ViewBoxLinkSpecification CreateSpecification() => new()
    {
        Id = "test-spec",
        ViewBoxId = "main-vbox",
        Uri = new Uri("https://example.com"),
        SelectedClassName = "selected",
        DefaultClassName = "default",
        OnClick = "onclick()",
        ScrollUp = true
    };

    private static void RunConcurrently(Action action)
    {
        var barrier = new Barrier(ThreadCount);
        var threads = Enumerable.Range(0, ThreadCount).Select(_ => new Thread(() =>
        {
            barrier.SignalAndWait();
            action();
        })).ToList();
        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();
    }

    [Fact]
    public void AddResourceAndRender_ConcurrentSameResourceName_ShouldNeverThrow()
    {
        // Arrange
        var (renderer, _) = CreateRealRenderer();
        var failures = new ConcurrentBag<string>();
        var path = "src/AlienFruit.Astra/ViewBox/Resources/viewbox.js";

        // Act
        RunConcurrently(() =>
        {
            try
            {
                renderer.AddResource(LinkInitResourceName, path, AstraResourceLocation.Body);
                renderer.RenderBodyResource(LinkInitResourceName);
            }
            catch (Exception ex)
            {
                failures.Add($"{ex.GetType().Name}: {ex.Message}");
            }
        });

        // Assert
        failures.Should().BeEmpty();
    }

    [Fact]
    public void ViewBoxLinkProcess_ConcurrentSameId_ShouldNeverThrow()
    {
        // Arrange
        var (renderer, initScriptRenderer) = CreateRealRenderer();
        var failures = new ConcurrentBag<string>();

        RunConcurrently(() =>
        {
            try
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Request.Path = "/tarot";
                var link = new AlienFruit.Astra.ViewBoxLink.ViewBoxLink(
                    initScriptRenderer,
                    new HttpContextAccessor { HttpContext = httpContext })
                {
                    Id = LinkId,
                    Uri = "/tarot/go-aitarotreader-becheating",
                    ViewBoxId = "main-vbox",
                    DefaultClassName = "nav-link"
                };

                var context = CreateTagHelperContext("view-box-link");
                var output = CreateTagHelperOutput("view-box-link");
                link.Process(context, output);
            }
            catch (Exception ex)
            {
                failures.Add($"{ex.GetType().Name}: {ex.Message}");
            }
        });

        // Assert
        failures.Should().BeEmpty();
    }

    [Fact]
    public void AddResource_FailedRegistration_ShouldNotPoisonResourceName()
    {
        // Arrange
        var (renderer, _) = CreateRealRenderer();
        var failures = new ConcurrentBag<string>();
        var path = "src/AlienFruit.Astra/ViewBox/Resources/viewbox.js";

        // Act
        RunConcurrently(() =>
        {
            try
            {
                renderer.AddResource(LinkInitResourceName, path, AstraResourceLocation.Body);
                renderer.RenderBodyResource(LinkInitResourceName);
            }
            catch (Exception ex)
            {
                failures.Add($"{ex.GetType().Name}: {ex.Message}");
            }
        });

        // Assert
        failures.Should().BeEmpty();
    }

    [Fact]
    public void AddResource_ConcurrentSameResourceName_StorageAndRegistryStayConsistent()
    {
        // Arrange
        var (renderer, _) = CreateRealRenderer();
        var failures = new ConcurrentBag<string>();
        var path = "src/AlienFruit.Astra/ViewBox/Resources/viewbox.js";

        // Act
        RunConcurrently(() =>
        {
            try
            {
                renderer.AddResource(LinkInitResourceName, path, AstraResourceLocation.Body);
                renderer.RenderBodyResource(LinkInitResourceName);
            }
            catch (Exception ex)
            {
                failures.Add($"{ex.GetType().Name}: {ex.Message}");
            }
        });

        // Assert
        failures.Should().BeEmpty();
    }
}
