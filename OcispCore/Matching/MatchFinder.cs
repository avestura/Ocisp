using OcispCore.DataStructure;

namespace OcispCore.Matching
{
    public static class MatchFinder
    {
        // A DFS based recursive function that returns true if a
        // matching for vertex u is possible
        private static bool IsMatchingPossibleForVertex(UndirectedBipartiteGraph bipartiteGraph, int u, bool[] seen, int[] assignedItems)
        {

            int N1 = bipartiteGraph.N1Size;
            int N2 = bipartiteGraph.N2Size;

            bool[,] adjacencyMatrix = bipartiteGraph.EdgeMatrix;
            // Try every job one by one
            for (int v = 0; v < N2; v++)
            {
                // If N1 item u is interested in N2 item v and v
                // is not visited
                if (adjacencyMatrix[u, v] && !seen[v])
                {
                    seen[v] = true; // Mark v as visited

                    // If N2 item 'v' is not assigned to an N1 item OR
                    // previously assigned N1 item for N2 item v (which
                    // is assignedItems[v]) has an alternate N2 item available.
                    // Since v is marked as visitx ed in the above line,
                    // assignedItems[v] in the following recursive call will
                    // not get N2 item 'v' again
                    if (assignedItems[v] < 0 ||
                        IsMatchingPossibleForVertex(bipartiteGraph, assignedItems[v], seen, assignedItems))
                    {
                        assignedItems[v] = u;
                        return true;
                    }
                }
            }
            return false;
        }

        // Returns maximum number of matching from N1 to N2
        public static int[] MamximumMatching(UndirectedBipartiteGraph bipartiteGraph)
        {
            int N1 = bipartiteGraph.N1Size;
            int N2 = bipartiteGraph.N2Size;
            // An array to keep track of the N1 items assigned to
            // N2 items. The value of assignedItems[i] is the N1 item number
            // assigned to N2 item i, the value -1 indicates no item is
            // assigned.
            int[] assignedItems = new int[N2];

            // Initially all N2 items are available
            for (int i = 0; i < N2; ++i)
                assignedItems[i] = -1;

            int result = 0; // Count of N2 items assigned to N1 items
            for (int u = 0; u < N1; u++)
            {
                // Mark all N2 items as not seen for next N1 item.
                bool[] seen = new bool[N2];
                for (int i = 0; i < N2; i++)
                    seen[i] = false;

                // Find if the N1 item 'u' can get a N2 item
                if (IsMatchingPossibleForVertex(bipartiteGraph, u, seen, assignedItems))
                    result++;
            }
            return assignedItems;
        }
    }
}
