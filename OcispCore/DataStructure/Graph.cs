using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcispCore.DataStructure
{
    public interface IGrapher
    {
        int NodesCount { get; }
        bool HasEdge(int source, int destination);
        void ModifyEdge(int source, int destination, bool value);
        void CreateEdge(int source, int destination);
        void DeleteEdge(int source, int destination);
    }

    public abstract class Graph : IGrapher
    {
        public int NodesCount { get; }

        protected bool[,] _edges;

        public int[] NodeNumbers { get; set; }

        protected Graph(int count, bool useSequentialNumbering = false)
        {
            NodesCount = count;
            _edges = new bool[count, count];
            NodeNumbers = new int[NodesCount];

            if (useSequentialNumbering)
            {
                for (int i = 0; i < NodesCount; i++)
                {
                    NodeNumbers[i] = i;
                }
            }
        }

        public bool[,] EdgeMatrix => _edges;

        public bool HasEdge(int source, int destination) => _edges[source, destination];

        public abstract void ModifyEdge(int source, int destination, bool value);

        public void CreateEdge(int source, int destination) => ModifyEdge(source, destination, true);

        public void DeleteEdge(int source, int destination) => ModifyEdge(source, destination, false);
    }

    public class UndirectedGraph : Graph
    {
        public UndirectedGraph(int count, bool useSequentialNumbering = false) : base(count, useSequentialNumbering) { }

        public override void ModifyEdge(int source, int destination, bool value)
        {
            _edges[source, destination] = value;
            _edges[destination, source] = value;
        }

        public static UndirectedGraph BuildSmallerGraph(UndirectedGraph parentGraph, int[] selectionNumbers, bool copyEdges = false)
        {
            var ug = new UndirectedGraph(selectionNumbers.Length);

            // Copy node numbers
            for(int i = 0, j = 0; i < parentGraph.NodesCount; i++)
            {
                if (selectionNumbers.Contains(parentGraph.NodeNumbers[i]))
                {
                    ug.NodeNumbers[j++] = parentGraph.NodeNumbers[i];
                }
            }

            if (copyEdges)
            {
                // Copy Edge matrix
                for (int i = 0; i < selectionNumbers.Length; i++)
                {
                    for (int j = i; j < selectionNumbers.Length; j++)
                    {
                        var iInParentGraph = Array.IndexOf(parentGraph.NodeNumbers, ug.NodeNumbers[i]);
                        var jInParentGraph = Array.IndexOf(parentGraph.NodeNumbers, ug.NodeNumbers[j]);
                        var valueOfParentGraph = parentGraph.EdgeMatrix[iInParentGraph, jInParentGraph];

                        ug.ModifyEdge(i, j, valueOfParentGraph);
                    }
                }
            }

            return ug;
        }
    }

    public class DirectedGraph : Graph
    {
        public DirectedGraph(int count, bool useSequentialNumbering = false) : base(count, useSequentialNumbering) { }

        public override void ModifyEdge(int source, int destination, bool value)
        {
            _edges[source, destination] = value;
        }

        public static DirectedGraph BuildSmallerGraph(DirectedGraph parentGraph, int[] selectionNumbers, bool copyEdges = false)
        {
            var ug = new DirectedGraph(selectionNumbers.Length);

            // Copy node numbers
            for (int i = 0, j = 0; i < parentGraph.NodesCount; i++)
            {
                if (selectionNumbers.Contains(parentGraph.NodeNumbers[i]))
                {
                    ug.NodeNumbers[j++] = parentGraph.NodeNumbers[i];
                }
            }

            if (copyEdges)
            {
                // Copy Edge matrix
                for (int i = 0; i < selectionNumbers.Length; i++)
                {
                    for (int j = i; j < selectionNumbers.Length; j++)
                    {
                        var iInParentGraph = Array.IndexOf(parentGraph.NodeNumbers, ug.NodeNumbers[i]);
                        var jInParentGraph = Array.IndexOf(parentGraph.NodeNumbers, ug.NodeNumbers[j]);
                        var valueOfParentGraph = parentGraph.EdgeMatrix[iInParentGraph, jInParentGraph];

                        ug.ModifyEdge(i, j, valueOfParentGraph);
                    }
                }
            }

            return ug;
        }
    }

    public class UndirectedBipartiteGraph
    {
        public UndirectedBipartiteGraph(int N1Size, int N2Size)
        {
            _edges = new bool[N1Size, N2Size];

            N1NodeNumbers = new int[N1Size];
            N2NodeNumbers = new int[N2Size];

            this.N1Size = N1Size;
            this.N2Size = N2Size;
        }

        public int NodesCount { get; }

        protected bool[,] _edges;

        public bool[,] EdgeMatrix => _edges;

        public bool HasEdge(int source, int destination) => _edges[source, destination];

        public void CreateEdge(int source, int destination) => ModifyEdge(source, destination, true);

        public void DeleteEdge(int source, int destination) => ModifyEdge(source, destination, false);

        public void ModifyEdge(int source, int destination, bool value)
        {
            _edges[source, destination] = value;
        }

        public int[] N1NodeNumbers { get; set; }

        public int[] N2NodeNumbers { get; set; }

        public int N1Size { get; private set;  }
        public int N2Size { get; private set;  }
    }

    public class DirectedBipartiteGraph
    {
        public int NodesCount { get; }

        protected bool[,] _inEdges;
        protected bool[,] _outEdges;

        public int[] N1NodeNumbers { get; set; }
        public int[] N2NodeNumbers { get; set; }

        public bool[] N1DFSIsVisited { get; }
        public bool[] N2DFSIsVisited { get; }

        public DirectedBipartiteGraph(int N1Size, int N2Size)
        {
            NodesCount = N1Size + N2Size;
            _inEdges = new bool[N1Size, N2Size];
            _outEdges = new bool[N1Size, N2Size];
            N1NodeNumbers = new int[N1Size];
            N2NodeNumbers = new int[N2Size];

            this.N1Size = N1Size;
            this.N2Size = N2Size;

            N1DFSIsVisited = new bool[N1Size];
            N2DFSIsVisited = new bool[N2Size];
        }

        public bool[,] IncomingEdgeToN1Matrix => _inEdges;
        public bool[,] OutgoingEdgeFromN1Matrix => _outEdges;

        public bool HasIncomingEdge(int N1Item, int N2Item) => _inEdges[N1Item, N2Item];

        public bool HasOutgoingEdge(int N1Item, int N2Item) => _outEdges[N1Item, N2Item];

        public void ModifyIncomingEdge(int N1Item, int N2Item, bool value)
        {
            _inEdges[N1Item, N2Item] = value;
        }
        public void ModifyOutgoingEdge(int N1Item, int N2Item, bool value)
        {
            _outEdges[N1Item, N2Item] = value;
        }

        public void CreateIncomingEdgeToN1(int N1Item, int N2Item) => ModifyIncomingEdge(N1Item, N2Item, true);
        public void CreateOutgoingEdgeFromN1(int N1Item, int N2Item) => ModifyOutgoingEdge(N1Item, N2Item, true);

        public void DeleteIncomingEdgeToN1(int N1Item, int N2Item) => ModifyIncomingEdge(N1Item, N2Item, false);
        public void DeleteOutgoingEdgeFromN1(int N1Item, int N2Item) => ModifyOutgoingEdge(N1Item, N2Item, false);

        /**
         * Calculates L1 U L2 - L2
         **/
        public int[] DesiredOChildSet(int[] map)
        {
            var unmatchedIndexes = OcispProblemExtensions.UnmatchedIndexes(N1Size, map);
            for (int i = 0; i < unmatchedIndexes.Length; i++)
            {
                var currentUnmatchedIndex = unmatchedIndexes[i];
                var nodeNumber = N1NodeNumbers[currentUnmatchedIndex];

                DFS(nodeNumber);
            }

            // L1 is visited nodes on N1
            // N2 is N2 !
            // L2 is visited nodes of N2
            // Lets have a baby and name it OChild!

            var OchildSelectedNumbers = new List<int>();

            for(int i = 0; i < N1Size; i++)
            {
                if (N1DFSIsVisited[i])
                    OchildSelectedNumbers.Add(N1NodeNumbers[i]);
            }

            for (int i = 0; i < N2Size; i++)
            {
                    OchildSelectedNumbers.Add(N2NodeNumbers[i]);
            }

            for (int i = 0; i < N2Size; i++)
            {
                if (N2DFSIsVisited[i])
                    OchildSelectedNumbers.Remove(N2NodeNumbers[i]);
            }

            return OchildSelectedNumbers.ToArray();
        }

        private void DFS(int startingVerticeNumber)
        {
            if (IsInN1(startingVerticeNumber)) // If is N1
            {
                var index = Array.IndexOf(N1NodeNumbers, startingVerticeNumber);
                N1DFSIsVisited[index] = true;

                foreach(int i in GetAdjacent(startingVerticeNumber))
                {
                    if(!N2DFSIsVisited[i])
                    {
                        int numberOfItem = N2NodeNumbers[i];
                        DFS(numberOfItem);
                    }
                }
            }
            else
            {
                var index = Array.IndexOf(N2NodeNumbers, startingVerticeNumber);
                N2DFSIsVisited[index] = true;

                foreach (int i in GetAdjacent(startingVerticeNumber))
                {
                    if (!N1DFSIsVisited[i])
                    {
                        int numberOfItem = N1NodeNumbers[i];
                        DFS(numberOfItem);
                    }
                }
            }
        }

        private bool IsInN1(int number) => N1NodeNumbers.Any(integer => integer == number);

        // Returns index of adjacents in format of cell number (0, 1, ...)
        public int[] GetAdjacent(int number)
        {
            var result = new List<int>();
            if (IsInN1(number))
            {
                var indexInArray = Array.IndexOf(N1NodeNumbers, number);
                for (int j = 0; j < N2Size; j++)
                {
                    if (OutgoingEdgeFromN1Matrix[indexInArray, j])
                        result.Add(j);
                }
            }
            else
            {
                var indexInArray = Array.IndexOf(N2NodeNumbers, number);
                for (int i = 0; i < N1Size; i++)
                {
                    if (IncomingEdgeToN1Matrix[i, indexInArray])
                        result.Add(i);
                }
            }

            return result.ToArray();
        }

        public int N1Size { get; private set; }
        public int N2Size { get; private set; }
    }
}
