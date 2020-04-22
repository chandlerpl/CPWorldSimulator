using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CP.Common.Utilities
{
    public static class ClassLoader
    {
        public static IEnumerable<T> Load<T>()
        {
            List<T> objects = new List<T>();

            foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T)Activator.CreateInstance(type));
            }

            return objects;
        }
    }
}
