using OcispCore.DataStructure;
using OcispCore.Matching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static OcispCore.OcispProblemExtensions;

namespace OcispCore
{
    public class OcispProblem
    {

        public UndirectedGraph ProblemGraph { get; set; }

        public int ProblemSize => ProblemGraph.NodesCount;

        //public int GenerationCounter { get; private set; }

        public List<bool[]> CurrentGeneration { get; private set; }

        // For debug
        public List<bool[]> CurrentGenerationDebugger { get; private set; }

        public int InitialPopulationSize => 20;

        public Random Random { get; set; } = new Random();

        public double MutateAlphaRate => 0.002; // Based on paper

        public bool[] BestEverChromosome { get; set; }

        public bool TerminationCondition
        {
            get
            {
                return false;
            }
        }

        #region Main Functionality
        public  void CreateInitialPopulation(int populationCount)
        {
            CurrentGeneration = new List<bool[]>();

            for(int i = 0; i < populationCount; i++)
            {
                var randomParent = CreateRandomParent();
                var binaryEncoding = OcispProblemExtensions.BinaryEncoding(ProblemSize, randomParent.NodeNumbers);

                CurrentGeneration.Add(binaryEncoding);
            }
        }

        // Best Fit
        public void Selection()
        {
            var sortedGeneration = from g in CurrentGeneration orderby g.Evaluate() descending select g;
            CurrentGeneration = sortedGeneration.ToList();
        }

        public void Crossover()
        {
            var newChildren = new List<bool[]>();

            for(int i = 0; i < InitialPopulationSize / 2; i++)
            {
                var I2bool = CurrentGeneration[(2 * i)]; // Lower index always has more or equal items, cause it is sorted
                var I1bool = CurrentGeneration[(2 * i) + 1];

                var I2 = I2bool.NumbersEncoding();
                var I1 = I1bool.NumbersEncoding();
                // Now we should create a new Undirected Bipartite Graph
                // N1 = I1
                // N2 = I2 - I1

                var N1temp = I1;
                var N2temp = I2.Subtract(I1);

                var N1 = (I1.Length >= N2temp.Length) ? N1temp : N2temp;
                var N2 = (I1.Length < N2temp.Length) ? N2temp : N1temp;

                var ubg = new UndirectedBipartiteGraph(N1.Length, N2.Length)
                {
                    N1NodeNumbers = N1,
                    N2NodeNumbers = N2
                };

                for(int x = 0; x < ubg.N1Size; x++)
                {
                    for(int z = x; z < ubg.N2Size; z++)
                    {
                        var xIndexInRealGraph = Array.IndexOf(ProblemGraph.NodeNumbers, ubg.N1NodeNumbers[x]);
                        var zIndexInRealGraph = Array.IndexOf(ProblemGraph.NodeNumbers, ubg.N2NodeNumbers[z]);

                        var value = ProblemGraph.EdgeMatrix[xIndexInRealGraph, zIndexInRealGraph];

                        ubg.ModifyEdge(z, x, value);
                    }
                }

                var matchingMap = MatchFinder.MamximumMatching(ubg);
                var dbg = ubg.ConstructDirectedAuxiliaryNetwork(matchingMap);

                var OChild = dbg.DesiredOChildSet(matchingMap);

                var OChildBool = BinaryEncoding(ProblemSize, OChild);
                var EChildBool = GenerateEChild(I2bool, I1bool, OChildBool);

                if(OChildBool.Evaluate() == 0)
                {
                    newChildren.Add(BinaryEncoding(ProblemSize, CreateRandomParent().NodeNumbers));
                }
                else
                {
                    newChildren.Add(OChildBool);
                }

                if(EChildBool.Evaluate() == 0)
                {
                    newChildren.Add(BinaryEncoding(ProblemSize, CreateRandomParent().NodeNumbers));

                }
                else
                {
                    newChildren.Add(EChildBool);
                }
            }

            // Elitism
            if (Random.NextDouble() <= 0.5)
            {
                var copy = new bool[ProblemSize];
                Array.Copy(BestEverChromosome, copy, ProblemSize);
                newChildren[InitialPopulationSize - 1] = copy;
            }

            CurrentGeneration = newChildren;
            CurrentGenerationDebugger = CurrentGeneration;
        }

        public void Mutate()
        {
            for (int i = 0; i < CurrentGeneration.Count; i++) // Do for every chromosome
            {
                var encode = CurrentGeneration[i];

                for (int j = 0; j < encode.Length; j++)
                {
                    var mutateCondition = Random.NextDouble() < MutateAlphaRate;

                    if (mutateCondition)
                    {
                        encode[j] = !encode[j];

                        if (encode[j] == true) // Repair
                        {
                            for (int f = 0; f < ProblemSize; f++)
                            {
                                if (ProblemGraph.HasEdge(j, f))
                                {
                                    encode[f] = false;
                                }
                            }
                        }
                    }
                }

                CurrentGeneration[i] = encode;
            }
        }

        public void GeneticAlgorithm()
        {
            CreateInitialPopulation(InitialPopulationSize);
            BestEverChromosome = new bool[ProblemSize];

            while (!TerminationCondition)
            {
                Selection();
                Crossover();
                Mutate();

                UpdateMaximum();
            }
        }

        public void UpdateMaximum()
        {
            bool[] max = CurrentGeneration[0];
            for(int i = 0; i < CurrentGeneration.Count; i++)
            {
                var eval = CurrentGeneration[i].Evaluate();
                if (eval > max.Evaluate())
                {
                    max = CurrentGeneration[i];
                }
            }

            if (max.Evaluate() > BestEverChromosome.Evaluate())
                BestEverChromosome = max;
        }

        #endregion Main Functionality

        /*
         * Is Node Compatible For Adding To Initial Population?
         */
        public bool IsCompatible(int nodeNumber, List<int> list)
        {
            foreach(var number in list)
            {
                if (ProblemGraph.EdgeMatrix[nodeNumber, number])
                    return false;
            }
            return true;
        }

        public UndirectedGraph CreateRandomParent()
        {
            var shuffleNodes = Enumerable.Range(0, ProblemGraph.NodesCount).ToArray();

            shuffleNodes = shuffleNodes.ToList().OrderBy(x => Guid.NewGuid()).ToArray();

            var selectedIndexes = new List<int>();

            for (int i = 0; i < shuffleNodes.Count(); i++)
            {
                if (IsCompatible(shuffleNodes[i], selectedIndexes)){
                    selectedIndexes.Add(shuffleNodes[i]);
                }
            }

            return UndirectedGraph.BuildSmallerGraph(ProblemGraph, selectedIndexes.ToArray());
        }

        #region Helper Static Methods
        public static OcispMetaData Parse(string url)
        {
            var lines = File.ReadAllLines(url);

            var result = new OcispMetaData();

            UndirectedGraph graph = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("c"))
                {

                }
                else if (line.StartsWith("p"))
                {
                    if (line.StartsWith("p col"))
                    {
                        string[] s = line.Substring(6).Split(' ');
                        result.Nodes = int.Parse(s[0]);
                        result.Edges = int.Parse(s[1]);

                        graph = new UndirectedGraph(result.Nodes, true);
                        for(int i = 0; i < result.Nodes; i++)
                        {
                            for(int j = 0; j < result.Nodes; j++)
                            {
                                graph.EdgeMatrix[i, j] = true;
                            }
                        }
                    }
                    if (line.StartsWith("p edge"))
                    {
                        string[] s = line.Substring(7).Split(' ');
                        result.Nodes = int.Parse(s[0]);
                        result.Edges = int.Parse(s[1]);

                        graph = new UndirectedGraph(result.Nodes, true);
                        for (int i = 0; i < result.Nodes; i++)
                        {
                            for (int j = 0; j < result.Nodes; j++)
                            {
                                graph.EdgeMatrix[i, j] = true;
                            }
                        }
                    }
                }
                else if (line.StartsWith("e"))
                {
                    var x = int.Parse(line.Substring(2).Split(' ')[0]);
                    var y = int.Parse(line.Substring(2).Split(' ')[1]);
                    graph?.DeleteEdge(x - 1, y - 1);
                }
            }
            result.Graph = graph;
            return result;
        }


        public bool[] GenerateEChild(bool[] p1, bool[] p2, bool[] o)
        {
            var result = new bool[p1.Length];
            for(int i = 0; i < p1.Length; i++)
            {
                // E[i] = P1[i] + P2[i] + O[i]
                // Karnaugh map
                //
                //        [P2 O]
                //     00  01  11  10
                //    ----------------
                // 0 | 0   0   0   1 |
                // 1 | 1   0   1   1 |
                //    ----------------
                //
                result[i] = (p1[i] && o[i]) || (p1[i] && p2[i]) || (p2[i] && !o[i]);

                if(result[i] == true)
                {
                    for(int k = 0; k < ProblemSize; k++)
                    {
                        if(ProblemGraph.HasEdge(i, k))
                        {
                            result[i] = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }
        #endregion Helper Static Methods
    }
}
