using Microsoft.AspNetCore.Http;

namespace AlienFruit.Astra.ViewBoxLink
{
    public class ViewBoxLinkSpecification
    {
        public const string InitScriptTemplatePath = "AlienFruit.Astra.ViewBoxLink.Resources.viewboxlink-init-script.template.js";

        public required string Id { get; set; }
        public required string ViewBoxId { get; set; }
        public required Uri Uri { get; set; }
        public string? SelectedClassName { get; set; }
        public string? DefaultClassName { get; set; }
        public string? OnClick { get; set; }
        public bool ScrollUp { get; set; } = true;
    }
}
