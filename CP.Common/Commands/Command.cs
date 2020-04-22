using CP.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CP.Common.Commands
{
    public class Command
    {
        /*
         * TODO: Work on a Command system that can be instantiated multiple times while still being able to dynamically load CPCommands but only commands that correlate to 
         *       the correct Command System.
         */

        public static Dictionary<string, CPCommand> Commands { get; private set; } = new Dictionary<string, CPCommand>();

        public Command()
        {
            RegisterCommand();
        }

        public static void RegisterCommand()
        {
            Commands.Clear();
            Dictionary<string, string> commandAliases = new Dictionary<string, string>();
            List<CPCommand> commands = ClassLoader.Load<CPCommand>().ToList();
            commands = commands.OrderBy(t => t.Name).ToList();
            foreach (CPCommand cl in commands)
            {
                string name = cl.Name;
                if (name == null)
                {
                    Console.WriteLine("Command " + cl.GetType().Name + " has no name!");
                    continue;
                }

                if (Commands.ContainsKey(name.ToLower()))
                {
                    Console.WriteLine(name + " already exists, please remove or rename!");
                    continue;
                }

                if (cl.Aliases == null || cl.Aliases.Count == 0)
                {
                    Console.WriteLine("Command " + name + " has no aliases! Using " + name.ToLower() + " as Alias");
                    cl.Aliases.Add(name.ToLower());
                }

                foreach(string alias in cl.Aliases.ToArray())
                {
                    if(commandAliases.ContainsKey(alias))
                    {
                        if(name.Equals(commandAliases[alias]))
                        {
                            Console.WriteLine("Warning! The Command " + name + " has duplicate alias entries.");
                            cl.Aliases.RemoveAll(t => t == alias);
                            cl.Aliases.Add(alias);
                        } else
                        {
                            Console.WriteLine("The alias " + alias + " for Command " + name + " is already in use by Command " + commandAliases[alias] + ", removing!");
                            cl.Aliases.RemoveAll(t => t == alias);
                        }
                        continue;
                    }

                    commandAliases.Add(alias, name);
                }

                Commands.Add(name.ToLower(), cl);
            }
        }
        
        public bool commandInterface(object obj, string[] args)
        {
            foreach (CPCommand command in Commands.Values)
            {
                if (command.Aliases.Contains(args[0].ToLower()))
                {
                    if (!command.Execute(obj, args.ToList()))
                    {
                        Console.WriteLine(command);
                    }

                    return true;
                }
            }

            Console.WriteLine("Invalid Command!\n");
            return false;
        }
    }
}