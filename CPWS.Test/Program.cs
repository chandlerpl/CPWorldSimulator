using CP.Common.Commands;
using CPWS.WorldGenerator.Noise;
using System;

namespace CPWS.Test
{
    class Program
    {
        static CommandSystem CommandSystem;

        static void Main(string[] args)
        {
            CommandSystem = new CommandSystem();

            Console.WriteLine(Math.Cos(3.12414));
            while (true)
            {
                Console.Write("Please enter a command: ");
                CommandSystem.CommandInterface(null, Console.ReadLine().Split(' '));
            }
        }
    }
}
