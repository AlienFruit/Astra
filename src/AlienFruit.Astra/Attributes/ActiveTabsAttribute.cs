namespace AlienFruit.Astra.Attributes
{
    /// <summary>
    /// Marks an action method with the set of tab identifiers that should be
    /// rendered as active when the action is invoked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ActiveTabsAttribute(params string[] tabs) : Attribute
    {
        /// <summary>
        /// Gets the identifiers of the tabs to activate for the decorated action.
        /// </summary>
        public string[] Tabs { get; } = tabs;
    }
}
