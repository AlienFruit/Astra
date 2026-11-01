using AlienFruit.Astra.Abstractions;
using System.Reflection;

namespace AlienFruit.Astra.Core
{
    /// <summary>
    /// Discriminates the HTML element a registered resource renders to
    /// (stylesheet → <c>link</c>, script → <c>script</c>). Internal to the registry;
    /// callers pick it via the file extension of the registered path.
    /// </summary>
    internal enum AstraResourceKind
    {
        Stylesheet,
        Script,
    }

    /// <summary>
    /// A single fully-formed registry entry: name + path + kind + version.
    /// Immutable; published into the registry with one atomic TryAdd.
    /// </summary>
    /// <param name="Name">Registry key.</param>
    /// <param name="Path">Manifest resource path the content is registered from;
    /// compared on re-registration to reject conflicting registrations.</param>
    /// <param name="Kind">Stylesheet (→ <c>link</c>) or Script (→ <c>script</c>).</param>
    /// <param name="Version">Content hash of the stored bytes, computed at registration;
    /// served as the cache-busting query.</param>
    internal sealed record ResourceEntry(
        string Name,
        string Path,
        AstraResourceKind Kind,
        string Version);
}
