using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dagger
{
    public class Graph<TKey, TData> : IEnumerable<KeyValuePair<TKey, TData>>
        where TKey : IEquatable<TKey>
    {
        private Dictionary<TKey, TData> Data { get; } = new Dictionary<TKey, TData>();

        private Dictionary<TKey, HashSet<TKey>> IncomingEdges { get; } = new Dictionary<TKey, HashSet<TKey>>();

        private Dictionary<TKey, HashSet<TKey>> OutgoingEdges { get; } = new Dictionary<TKey, HashSet<TKey>>();

        public Int32 Count => Data.Count;

        public TData this[TKey key] => Data[key];

        /// <summary>
        /// Returns the nodes topologically sorted into layers. Nodes with no outgoing edges are in the first layer,
        /// while nodes that only point to nodes in the first layer are in the second layer, and so on. Any node that
        /// points to a node that has not been added to the graph is considered detached.
        /// </summary>
        /// <returns>A tuple containing a list of layers and a list of detached keys.</returns>
        public (List<List<TKey>> layers, List<TKey> detached) TopologicalSort()
        {
            // This method could probably be optimized significantly.
            // Now that I know it's called a topological sort, could use Kahn's algorithm.
            // However, we'll still need to take into account detached nodes where the nodes they point
            // to don't actually exist.

            List<List<TKey>> layers = new List<List<TKey>> { new List<TKey>() };
            List<TKey> detached = new List<TKey>();
            foreach (TKey key in Data.Keys)
            {
                var outgoing = OutgoingEdges[key];
                if (OutgoingEdges[key].Count == 0)
                    layers[0].Add(key); // If a key has no outgoing edges, it's added to the first layer
                else if (outgoing.Any(e => !Data.ContainsKey(e)))
                    detached.Add(key); // If a key has any outgoing edges that are not in the graph, it is considered detached.
            }
            
            HashSet<TKey> satisfiedKeys = new HashSet<TKey>(layers[0]);
            HashSet<TKey> unsatisfiedKeys = new HashSet<TKey>();

            while (layers[layers.Count - 1].Count > 0)
            {
                IEnumerable<TKey> candidates =
                    layers[layers.Count - 1]
                    .SelectMany(previous => IncomingEdges.ContainsKey(previous) ? IncomingEdges[previous] : new HashSet<TKey>())
                    .Where(key => Data.ContainsKey(key))
                    .Concat(unsatisfiedKeys)
                    .Distinct();

                unsatisfiedKeys.Clear();

                List<TKey> currentLevel = new List<TKey>();
                foreach (TKey candidate in candidates)
                {
                    Boolean satisfied = true;
                    foreach (TKey outgoing in OutgoingEdges[candidate])
                    {
                        // Check if each of the outgoing keys have been set already.
                        if (satisfiedKeys.Contains(outgoing))
                            continue;

                        // If not, then the candidate gets bumped up to the next level.
                        satisfied = false;
                        break;
                    }

                    if (!satisfied)
                    {
                        unsatisfiedKeys.Add(candidate);
                        continue;
                    }

                    currentLevel.Add(candidate);
                }

                layers.Add(currentLevel);
                foreach (var key in currentLevel)
                    satisfiedKeys.Add(key);
            }

            layers.RemoveAt(layers.Count - 1);
            detached.AddRange(unsatisfiedKeys);
            return (layers, detached);
        }

        /// <summary>
        /// Adds a node with the specified key, data, and outgoing edges to the graph.
        /// </summary>
        public void AddNode(TKey key, TData data, IList<TKey> outgoing)
        {
            if (Data.ContainsKey(key))
                throw new ArgumentException("Node with the provided key already exists.");
            if (CausesCycle(key, outgoing))
                throw new ArgumentException("Adding this node causes a cycle.");

            Data.Add(key, data);
            AddEdges(key, outgoing);
        }

        /// <summary>
        /// Checks if adding the node causes a cycle.
        /// </summary>
        public Boolean CausesCycle(TKey key, IList<TKey> outgoing)
        {
            if (outgoing.Contains(key))
                return true; // Self cycle
            if (!IncomingEdges.TryGetValue(key, out HashSet<TKey> incoming) || incoming.Count == 0)
                return false; // No incoming edges, so we can't have a cycle.

            // If a path exists from any outgoing edge to any incoming edge, adding the node will cause a cycle.
            foreach (TKey start in outgoing)
            {
                foreach (TKey end in incoming)
                {
                    if (PathExists(start, end))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a path exists between the given start and end nodes.
        /// </summary>
        public Boolean PathExists(TKey start, TKey end)
        {
            HashSet<TKey> tested = new HashSet<TKey> { start };
            Queue<TKey> queued = new Queue<TKey>(tested);

            while (queued.Count > 0)
            {
                TKey current = queued.Dequeue();
                if (current.Equals(end)) // A path exists
                    return true;


                if (!OutgoingEdges.TryGetValue(current, out HashSet<TKey> destinations))
                    continue; // No out edges for current

                foreach (TKey destination in destinations)
                {
                    if (tested.Contains(destination))
                        continue;

                    tested.Add(destination);
                    queued.Enqueue(destination);
                }
            }

            return false;
        }

        private void AddEdges(TKey key, IList<TKey> outgoing)
        {
            OutgoingEdges.Add(key, new HashSet<TKey>(outgoing));
            foreach (var dest in outgoing)
            {
                if (!IncomingEdges.TryGetValue(dest, out HashSet<TKey> incoming))
                    IncomingEdges[dest] = new HashSet<TKey> { key };
                else
                    incoming.Add(key);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TData>> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
