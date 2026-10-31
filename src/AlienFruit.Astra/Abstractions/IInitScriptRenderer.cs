using System;

namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Renders per-instance initialization scripts without registering them in the singleton resource registry.
    /// This avoids unbounded registry growth and URL namespace pollution from data-derived identifiers.
    /// </summary>
    public interface IInitScriptRenderer
    {
        /// <summary>
        /// Renders the initialization script for a component specification.
        /// </summary>
        /// <typeparam name="T">The specification type.</typeparam>
        /// <param name="specification">Component specification (e.g., ViewBoxSpecification, ViewBoxLinkSpecification, etc.)</param>
        /// <returns>Raw JavaScript content ready for inline rendering.</returns>
        string Render<T>(T specification) where T : ISpecification;
    }
}