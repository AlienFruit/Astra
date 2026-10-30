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
/// Regression tests for "The resource &quot;viewbox-link-init-...&quot; was not registered".
/// Root cause: HtmlResourceRenderer marked the name in resourceScripts (TryAdd) BEFORE the
/// rendered content reached ResourceStorage, so concurrent requests for the same link id -
/// or any exception inside the add call, permanently poisoning the key - made
/// RenderBodyResource look up a name the storage never received.
/// These tests use the real ResourceStorage/HtmlResourceRenderer, not mocks.
/// </summary>
public class ResourceRegistrationRaceTests : AstraTestBase
{
    private const string LinkId = "go-aitarotreader-becheating";
    private const string LinkInitResourceName = $"viewbox-link-init-{LinkId}";
    private const int ThreadCount = 32;

    private HtmlResourceRenderer CreateRealRenderer() =>
        new(new ResourceStorage(new StubCompressor()), CreateAstraConfiguration());

    private static ViewBoxLinkSpecification CreateSpecification() => new()
    {
        Id = LinkId,
        Uri = new Uri("/tarot/go-aitarotreader-becheating", UriKind.Relative),
        ViewBoxId = "main-viewbox",
        DefaultClassName = "nav-link",
        SelectedClassName = "nav-link active"
    };

    private static void RunConcurrently(Action action)
    {
        using var barrier = new Barrier(ThreadCount);
        var threads = Enumerable.Range(0, ThreadCount).Select(_ => new Thread(() =>
        {
            barrier.SignalAndWait();
            action();
        })).ToList();
        foreach (var thread in threads) thread.Start();
        foreach (var thread in threads) thread.Join();
    }

    [Fact]
    public void AddJsCodeFromTemplateAndRenderBodyResource_ConcurrentSameResourceName_ShouldNeverThrow()
    {
        // Arrange
        var renderer = CreateRealRenderer();
        var failures = new ConcurrentBag<string>();

        // Act: every thread performs exactly the sequence ViewBoxLink.Process uses
        RunConcurrently(() =>
        {
            try
            {
                renderer.AddJsCodeFromTemplate(
                    LinkInitResourceName,
                    ViewBoxLinkSpecification.InitScriptTemplatePath,
                    CreateSpecification(),
                    AstraResourceLocation.Body);
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
        // Arrange: tag helper with the real renderer/storage, as in the production stack trace
        var renderer = CreateRealRenderer();
        var failures = new ConcurrentBag<string>();

        RunConcurrently(() =>
        {
            try
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Request.Path = "/tarot";
                var link = new AlienFruit.Astra.ViewBoxLink.ViewBoxLink(
                    renderer,
                    new HttpContextAccessor { HttpContext = httpContext })
                {
                    Id = LinkId,
                    Uri = "/tarot/go-aitarotreader-becheating",
                    ViewBoxId = "main-viewbox",
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
    public void AddJsCodeFromTemplate_FailedRegistration_ShouldNotPoisonResourceName()
    {
        // Arrange
        var renderer = CreateRealRenderer();
        var act = () => renderer.AddJsCodeFromTemplate(
            LinkInitResourceName,
            "AlienFruit.Astra.ViewBoxLink.Resources.DoesNotExist.template.js",
            CreateSpecification(),
            AstraResourceLocation.Body);

        // Act: first registration throws (missing template, scriban error, io hiccup...)
        act.Should().Throw<InvalidOperationException>();

        // Then the same resource name must be re-registerable and renderable
        var render = () =>
        {
            renderer.AddJsCodeFromTemplate(
                LinkInitResourceName,
                ViewBoxLinkSpecification.InitScriptTemplatePath,
                CreateSpecification(),
                AstraResourceLocation.Body);
            return renderer.RenderBodyResource(LinkInitResourceName);
        };

        // Assert: before the fix the failed attempt left the name in resourceScripts while
        // ResourceStorage stayed empty, so this threw "The resource ... was not registered"
        // until process restart.
        render.Should().NotThrow();
    }

    [Fact]
    public void AddJsCode_ConcurrentSameResourceName_StorageAndRegistryStayConsistent()
    {
        // Arrange
        var renderer = CreateRealRenderer();
        var failures = new ConcurrentBag<string>();

        // Act: same add-then-render sequence through the non-template overload
        RunConcurrently(() =>
        {
            try
            {
                renderer.AddJsCode(LinkInitResourceName, "console.log('init');", AstraResourceLocation.Body);
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
