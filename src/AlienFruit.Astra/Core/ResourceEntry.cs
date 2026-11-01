using AlienFruit.Astra.Abstractions;
using System.Reflection;

namespace AlienFruit.Astra.Core
{
    /// <summary>
    /// Discriminates the HTML element a registered Header resource renders to
    /// (stylesheet → <link>, script → <script>). Internal to the registry;
    /// callers pick it via the Add* method they use.
    /// </summary>
    internal enum AstraResourceKind
    {
        Stylesheet,
        Script,
    }

    /// <summary>
    /// Closed content-origin hierarchy of a registered resource — one entry is built
    /// with its source fully resolved, so a "name published, content missing" state
    /// cannot be expressed by the type.
    /// </summary>
    //internal abstract record ResourceSource;

    //internal sealed record EmbeddedSource(string Path, Assembly Assembly) : ResourceSource;

    //internal sealed record CodeSource(string Content) : ResourceSource;

    /// <summary>
    /// A single fully-formed registry entry: name + kind + location + source + version.
    /// Immutable; published into the registry with one atomic TryAdd.
    /// </summary>
    /// <param name="Name">Registry key.</param>
    /// <param name="Kind">Stylesheet (→ <c>link</c>) or Script (→ <c>script</c>).</param>
    /// <param name="Location">Header = URL-served; Body = inlined, no URL, no version.</param>
    /// <param name="Source">Closed content origin — part of the entry, so a registered name
    /// always has its content.</param>
    /// <param name="Version">Content hash, computed at most once on first Header read
    /// (<see cref="Lazy{T}"/> default ExecutionAndPublication). Null for Body resources:
    /// their version is never served.</param>
    internal sealed record ResourceEntry(
        string Name,
        string Path,
        AstraResourceKind Kind,
        string Version);
}
