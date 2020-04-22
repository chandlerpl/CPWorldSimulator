using CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Structure;

namespace CPWS.WorldGenerator.Voronoi.FortunesAlgorithm.Event
{
    internal class FortuneSiteEvent : FortuneEvent
    {
        public double X => Site.X;
        public double Y => Site.Y;
        internal FSite Site { get; }

        internal FortuneSiteEvent(FSite site)
        {
            Site = site;
        }

        public int CompareTo(FortuneEvent other)
        {
            var c = Y.CompareTo(other.Y);
            return c == 0 ? X.CompareTo(other.X) : c;
        }
     
    }
}