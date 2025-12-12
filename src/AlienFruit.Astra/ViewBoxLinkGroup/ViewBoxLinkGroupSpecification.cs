namespace AlienFruit.Astra.ViewBoxLinkGroup
{
    public class ViewBoxLinkGroupSpecification
    {
        public const string InitScriptTemplatePath = "AlienFruit.Astra.ViewBoxLinkGroup.Resources.viewboxlink-group-init-script.template.js";

        public required string Id { get; set; }
        //public required string ViewBoxId { get; set; }
        public required List<Uri> Uris { get; set; }
        public string? SelectedClassName { get; set; }
        public string? DefaultClassName { get; set; }
        public string? TagName { get; set; }
    }
}
