using System.Reflection;

namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Возвращает stream необходимых ресурсов
    /// </summary>
    public interface IResourceStorage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Уникальное имя, Нужно использовать расширение, если нужна компрессия. Например: test.js</param>
        /// <param name="path">Полный путь у ресурсу внутри assembly</param>
        /// <param name="assembly">Библиотека, в которой лежит ресурс</param>
        void RegisterResource(string name, string path, Assembly assembly);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Нужно использовать расширение, если нужна компрессия. Например: test.js </param>
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
