namespace AlienFruit.Astra.Abstractions
{
    public abstract class Resource(string name)
    {
        public string Name { get; } = name; 

        public abstract Stream GetStream();

        public string GetString()
        {
            using var reader = new StreamReader(GetStream());
            return reader.ReadToEnd();
        }

    }
}
