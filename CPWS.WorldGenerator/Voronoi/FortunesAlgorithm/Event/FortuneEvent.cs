using System;

namespace CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Event
{
    interface FortuneEvent : IComparable<FortuneEvent>
    {
        double X { get; }
        double Y { get; }
    }
}
