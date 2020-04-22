using System;
using System.Collections.Generic;
using System.Text;

namespace CPWS.EcoSystem.Life
{
    public interface ILife
    {
        void Progress();
        void Remove();
        string GetName();
        string GetLifeType();
    }
}
