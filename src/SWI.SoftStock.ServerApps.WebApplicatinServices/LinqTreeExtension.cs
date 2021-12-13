using System;
using System.Collections.Generic;
using System.Linq;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    /// <summary>
    /// http://social.technet.microsoft.com/wiki/contents/articles/17556.how-to-query-trees-using-linq.aspx
    /// http://www.codeproject.com/Articles/62397/LINQ-to-Tree-A-Generic-Technique-for-Querying-Tree
    /// </summary>
    public static class LinqTreeExtension
    {
        /// <summary>
        /// Descendants items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Descendants<T>(this T root, Func<T, IEnumerable<T>> selector)
        {
            var nodes = new Stack<T>(new T[] { root });
            while (nodes.Any())
            {
                var node = nodes.Pop();
                yield return node;
                foreach (var n in selector(node)) nodes.Push(n);
            }
        }

        /// <summary>
        /// Ancestors items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="includeSelf"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<T> Ancestors<T>(this T node, bool includeSelf, Func<T, T> selector) where T : class
        {
            if (includeSelf)
                yield return node;
            var parent = selector(node);
            while (parent != null)
            {
                yield return parent;
                parent = selector(parent);
            }
        }
    }
}