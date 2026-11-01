namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Contract for Astra tag helpers with self-initialization state.
    /// Currently has no implementations in the library.
    /// </summary>
    internal interface IAstraTag
    {
        /// <summary>
        /// Initializes the tag's internal state before processing.
        /// </summary>
        public void Init();
    }
}
