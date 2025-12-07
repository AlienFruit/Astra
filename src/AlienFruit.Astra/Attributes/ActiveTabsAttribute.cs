namespace AlienFruit.Astra.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ActiveTabsAttribute(params string[] tabs) : Attribute
    {
        public string[] Tabs { get; } = tabs;
    }
}
