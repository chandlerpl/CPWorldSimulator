using CP.Common.Commands;
using CPWS.WorldGenerator.Noise;
using System;

namespace CPWS.Test
{
    class Program
    {
        static Command CommandSystem;

        static void Main(string[] args)
        {
            CommandSystem = new Command();

            while (true)
            {
                Console.Write("Please enter a command: ");
                CommandSystem.commandInterface(null, Console.ReadLine().Split(' '));
            }
        }
    }
}
