namespace AlienFruit.Astra.Abstractions;

public interface ISpecification
{
    /// <summary>
    /// Gets the path to the init script template for this specification.
    /// </summary>
    string InitScriptTemplatePath { get; }
}