using OcispCore.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcispCore
{
    public static class OcispProblemExtensions
    {

        public static DirectedBipartiteGraph ConstructDirectedAuxiliaryNetwork(this UndirectedBipartiteGraph undirectedGraph, int[] matchingMap)
        {

            var dbg = undirectedGraph.EmptyEdgeDirectedVersion();

            for(int i = 0; i < undirectedGraph.N1Size; i++)  // For each row of matrix
            {
                for(int j = 0; j < undirectedGraph.N2Size; j++) // And for each column of matrix
                {
                    if(undirectedGraph.EdgeMatrix[i , j]) // If we have an arc (i, j)
                    {
                        if(matchingMap[j] == i) // And if this arc (i, j) is in M
                        {
                            dbg.CreateIncomingEdgeToN1(i, j);
                        }
                        else // and if this arc (i, j) is not in M
                        {
                            dbg.CreateOutgoingEdgeFromN1(i, j);
                        }
                    }
                }
            }

            return dbg;
        }


        public static DirectedBipartiteGraph EmptyEdgeDirectedVersion(this UndirectedBipartiteGraph undirectedBipartiteGraph)
        {
            var dbg = new DirectedBipartiteGraph(undirectedBipartiteGraph.N1Size, undirectedBipartiteGraph.N2Size);

            for(int i = 0; i < undirectedBipartiteGraph.N1Size; i++)
            {
                dbg.N1NodeNumbers[i] = undirectedBipartiteGraph.N1NodeNumbers[i];
            }

            for (int i = 0; i < undirectedBipartiteGraph.N2Size; i++)
            {
                dbg.N2NodeNumbers[i] = undirectedBipartiteGraph.N2NodeNumbers[i];
            }

            return dbg;
        }

        public static int[] UnmatchedIndexes(int N1Size, int[] matchingMap)
        {
            var indexes = Enumerable.Range(0, N1Size).ToList();
            for(int i = 0; i < matchingMap.Length; i++)
            {
                indexes.Remove(matchingMap[i]);   
            }

            return indexes.ToArray();
        }

        public static bool[] BinaryEncoding(int size, int[] selectedVertices)
        {
            var @bool = new bool[size];
            for(int i = 0; i < selectedVertices.Length; i++)
            {
                var verticeNumber = selectedVertices[i];
                @bool[verticeNumber] = true;
            }

            return @bool;
        }

        public static int[] NumbersEncoding(this bool[] selectedVertices)
        {
            var list = new List<int>();
            for(int i = 0; i < selectedVertices.Length; i++)
            {
                if (selectedVertices[i])
                    list.Add(i);
            }
            return list.ToArray();
        }

        public static int[] Subtract(this int[] greater, int[] lower)
        {
            var gList = greater.ToList();
            var lList = lower.ToList();

            var similar = gList.Where(x => lList.Contains(x));

            var result = new List<int>(gList);

            foreach(int x in similar)
            {
                result.Remove(x);
            }

            return result.ToArray();
        }

        public static int Evaluate(this bool[] answer) => answer.Where(x => x == true).Count();



    }
}
