using System.Reflection;

namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Returns stream of required resources
    /// </summary>
    public interface IResourceStorage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Unique name, use extension if compression is needed. Example: test.js</param>
        /// <param name="path">Full path to resource within assembly</param>
        /// <param name="assembly">Library containing the resource</param>
        void RegisterResource(string name, string path, Assembly assembly);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Use extension if compression is needed. Example: test.js</param>
        /// <param name="content"></param>
        void RegisterResource(string name, string content);

        void RegisterResource(Resource resource);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Contains(string name);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<Stream> OpenReadAsync(string name);

        Stream OpenRead(string name);
    }
}
