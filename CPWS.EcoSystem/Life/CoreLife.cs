using CP.Common.Utilities;
using CPWS.WorldGenerator.Test.Noise;
using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.EcoSystem.Life
{
    public abstract class CoreLife : ILife
    {
        public static List<CoreLife> INSTANCES = new List<CoreLife>();
        public static Dictionary<string, CoreLife> LIFE_BY_NAME = new Dictionary<string, CoreLife>();

        public string Name;

        public CoreLife(string name = "")
        {
            Name = name;
            INSTANCES.Add(this);

            SimplexNoise noise = new SimplexNoise(46313, 0.5, 0.5, false);
            Console.WriteLine(noise.Noise(new double[3] { 1920, 1080, 0 }));
        }

        public abstract void Progress();

        public virtual void Remove()
        {
            INSTANCES.Remove(this);
        }

        public string GetName()
        {
            return Name;
        }

        public string GetLifeType()
        {
            return GetType().Name;
        }

        public static void ProgressAll()
        {
            foreach(CoreLife cl in INSTANCES)
            {
                cl.Progress();
            }
        }

        public static void RemoveAll()
        {
            for(int i = INSTANCES.Count - 1; i >= 0; i--)
            {
                INSTANCES.RemoveAt(i);
            }
        }

        public static void RegisterLife()
        {
            LIFE_BY_NAME.Clear();
            foreach(CoreLife cl in ClassLoader.Load<CoreLife>())
            {
                string name = cl.GetName();
                if(name == null)
                {
                    Console.WriteLine("Life Class " + cl.GetType().Name + " has no name!");
                    continue;
                }

                if(LIFE_BY_NAME.ContainsKey(name.ToLower()))
                {
                    Console.WriteLine(name + " already exists, please remove or rename!");
                    continue;
                }

                LIFE_BY_NAME.Add(name.ToLower(), cl);
            }
        }
    }
}
