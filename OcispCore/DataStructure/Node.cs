using System.Collections.Generic;

namespace OcispCore.DataStructure
{
    public class Node<T>
    {
        public string Name { get; set; }

        public T Value { get; set; }

        public HashSet<Node<T>> Neighbours { get; set; }

        public void AddNeighbour(Node<T> item)
        {
            if(item != null)
            {
                Neighbours.Add(item);
                item.Neighbours.Add(this);
            }
        }

        public void AddAll(params Node<T>[] items)
        {
            foreach (var item in items)
                AddNeighbour(item);
        }

        public bool IsNeighbourWith(Node<T> graphNode) => Neighbours.Contains(graphNode);
    }

    public class DirectedNode<T>
    {
        public string Name { get; set; }

        public T Value { get; set; }

        public HashSet<DirectedNode<T>> Neighbours { get; set; }

        public void AddNeighbour(DirectedNode<T> item) => Neighbours.Add(item);

        public void AddAll(params DirectedNode<T>[] items)
        {
            foreach (var item in items)
                AddNeighbour(item);
        }
    }
}
