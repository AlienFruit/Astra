using AlienFruit.Astra.ViewBox;
using AlienFruit.Astra.ViewBoxLink;

namespace AlienFruit.Astra.Tests.Infrastructure;

/// <summary>
/// Фабрика для создания тестовых спецификаций ViewBox и ViewBoxLink
/// </summary>
public static class SpecificationFactory
{
    /// <summary>
    /// Создает базовую спецификацию ViewBox для тестирования
    /// </summary>
    public static ViewBoxSpecification CreateViewBoxSpecification(
        string id = "test-viewbox",
        int startLoadingEventDelay = 100,
        string? onFinishLoadingJsFunction = null,
        string? onStartLoadingJsFunction = null,
        string? onTimeoutAfterStartLoadingJsFunction = null,
        string? onScriptsExecutedJsFunction = null,
        bool changingBrowserAddressEnable = true,
        string? parentViewBoxId = null,
        string? connectionErrorMessage = null)
    {
        return new ViewBoxSpecification
        {
            Id = id,
            StartLoadingEventDelay = startLoadingEventDelay,
            OnFinishLoadingJsFunction = onFinishLoadingJsFunction,
            OnStartLoadingJsFunction = onStartLoadingJsFunction,
            OnTimeoutAfterStartLoadingJsFunction = onTimeoutAfterStartLoadingJsFunction,
            OnScriptsExecutedJsFunction = onScriptsExecutedJsFunction,
            ChangingBrowserAddressEnable = changingBrowserAddressEnable,
            ParrentViewBoxId = parentViewBoxId,
            ConnectionErrorMessage = connectionErrorMessage ?? "Default error message"
        };
    }

    /// <summary>
    /// Создает базовую спецификацию ViewBoxLink для тестирования
    /// </summary>
    public static ViewBoxLinkSpecification CreateViewBoxLinkSpecification(
        string id = "test-link",
        string uri = "/test",
        string viewBoxId = "test-viewbox",
        string? selectedClassName = "active",
        string? defaultClassName = "link",
        string? onClick = null,
        bool scrollUp = true)
    {
        return new ViewBoxLinkSpecification
        {
            Id = id,
            Uri = new Uri(uri, UriKind.RelativeOrAbsolute),
            ViewBoxId = viewBoxId,
            SelectedClassName = selectedClassName,
            DefaultClassName = defaultClassName,
            OnClick = onClick,
            ScrollUp = scrollUp
        };
    }

    /// <summary>
    /// Создает спецификацию ViewBox с минимальными настройками
    /// </summary>
    public static ViewBoxSpecification CreateMinimalViewBoxSpecification(string id = "minimal-viewbox")
    {
        return new ViewBoxSpecification
        {
            Id = id,
            StartLoadingEventDelay = 100,
            ChangingBrowserAddressEnable = true,
            ConnectionErrorMessage = "Error"
        };
    }

    /// <summary>
    /// Создает спецификацию ViewBoxLink с минимальными настройками
    /// </summary>
    public static ViewBoxLinkSpecification CreateMinimalViewBoxLinkSpecification(
        string id = "minimal-link",
        string uri = "/test",
        string viewBoxId = "minimal-viewbox")
    {
        return new ViewBoxLinkSpecification
        {
            Id = id,
            Uri = new Uri(uri, UriKind.RelativeOrAbsolute),
            ViewBoxId = viewBoxId
        };
    }
}
