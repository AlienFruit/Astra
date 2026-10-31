using AlienFruit.Astra.Abstractions;

namespace AlienFruit.Astra.ViewBox
{
    public class ViewBoxSpecification : ISpecification
    {
        public const string JsResourcePath = "AlienFruit.Astra.ViewBox.Resources.viewbox.js";
        private const string initScriptTemplatePath = "AlienFruit.Astra.ViewBox.Resources.viewbox-init-script.template.js";

        public required string Id { get; set; }
        public string? OnStartLoadingJsFunction { get; set; }
        public string? OnTimeoutAfterStartLoadingJsFunction { get; set; }
        public string? OnFinishLoadingJsFunction { get; set; }
        public string? OnScriptsExecutedJsFunction { get; set; }
        public int StartLoadingEventDelay { get; set; } = 100;
        public bool ChangingBrowserAddressEnable { get; set; } = true;
        public string? ParrentViewBoxId { get; set; }
        public required string ConnectionErrorMessage { get; set; }
        public bool LockAfterFirstLoad { get; set; } = false;
        public string InitScriptTemplatePath => initScriptTemplatePath;
    }
}
