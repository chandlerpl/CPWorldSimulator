using CP.Common.Commands;
using CPWS.EcoSystem.Life;
using CPWS.WorldGenerator.Noise;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPWS.Test
{
    class LifeTest : CPCommand
    {
        public override bool Init()
        {
            Name = "Life";
            Desc = "Generates a Life System";
            Aliases = new List<string>() { "life" };
            ProperUse = "life";

            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
            int people = args.Count > 1 ? int.Parse(args[1]) : 10;

            CoreLife cl = new AnimalLife();

            return true;
        }
    }
}
