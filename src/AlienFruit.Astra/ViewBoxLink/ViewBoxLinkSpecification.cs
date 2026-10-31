using AlienFruit.Astra.Abstractions;

namespace AlienFruit.Astra.ViewBoxLink
{
    public class ViewBoxLinkSpecification : ISpecification
    {
        private const string initScriptTemplatePath = "AlienFruit.Astra.ViewBoxLink.Resources.viewboxlink-init-script.template.js";

        public required string Id { get; set; }
        public required string ViewBoxId { get; set; }
        public required Uri Uri { get; set; }
        public string? SelectedClassName { get; set; }
        public string? DefaultClassName { get; set; }
        public string? OnClick { get; set; }
        public bool ScrollUp { get; set; } = true;
        public string InitScriptTemplatePath => initScriptTemplatePath;
    }
}
