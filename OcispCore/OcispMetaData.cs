using OcispCore.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcispCore
{
    public class OcispMetaData
    {
        public int Nodes { get; set; }
        public int Edges { get; set; }

        public UndirectedGraph Graph { get; set; }
    }
}
