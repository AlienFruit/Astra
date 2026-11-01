namespace AlienFruit.Astra.Abstractions;

/// <summary>
/// Component configuration passed to <see cref="IInitScriptRenderer.Render{T}"/>:
/// plain data rendered into the component's init-script template.
/// </summary>
public interface ISpecification
{
    /// <summary>
    /// Gets the path to the init script template for this specification.
    /// </summary>
    string InitScriptTemplatePath { get; }
}