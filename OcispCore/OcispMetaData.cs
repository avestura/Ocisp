using OcispCore.DataStructure;

namespace OcispCore
{
    public class OcispMetaData
    {
        public int Nodes { get; set; }
        public int Edges { get; set; }

        public UndirectedGraph Graph { get; set; }
    }
}
