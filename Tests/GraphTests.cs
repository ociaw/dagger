using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dagger.Tests
{
    [TestClass]
    public sealed class GraphTests
    {
        [TestMethod]
        public void TestMultiple()
        {
            var graph = new Graph<int, int>();
            graph.AddNode(1, 1, new[] { 2, 3, 4 });
            graph.AddNode(2, 4, new[] { 4 });
            graph.AddNode(3, 9, new[] { 2, 4 });
            graph.AddNode(4, 16, new int[0]);

            var (layers, detached) = graph.TopologicalSort();

            Assert.AreEqual(4, layers.Count);
            Assert.AreEqual(0, detached.Count);
            Assert.AreEqual(16, graph[layers[0][0]]);
            Assert.AreEqual(4, graph[layers[1][0]]);
            Assert.AreEqual(9, graph[layers[2][0]]);
            Assert.AreEqual(1, graph[layers[3][0]]);
        }

        [TestMethod]
        public void TestBranch()
        {
            var graph = new Graph<int, int>();
            graph.AddNode(7, 1, new[] { 4, 6 });
            graph.AddNode(5, 1, new[] { 1, 4 });
            graph.AddNode(4, 1, new[] { 2, 3 });
            graph.AddNode(3, 1, new[] { 1, 2 });
            graph.AddNode(2, 1, new[] { 1 });
            graph.AddNode(1, 1, new int[0]);
            graph.AddNode(6, 1, new[] { 5, 4 });

            Assert.AreEqual(1, graph[1]);
        }

        [TestMethod]
        public void TestFlat()
        {
            var graph = new Graph<int, int>();
            graph.AddNode(1, 1, new int[0]);
            graph.AddNode(2, 1, new int[0]);
            graph.AddNode(3, 1, new int[0]);
            graph.AddNode(4, 1, new int[0]);

            var (layers, detached) = graph.TopologicalSort();

            Assert.AreEqual(1, layers.Count);
            Assert.AreEqual(0, detached.Count);
            Assert.AreEqual(4, layers[0].Count);
        }

        [TestMethod]
        public void TestDuplicate()
        {
            var graph = new Graph<int, int>();
            graph.AddNode(1, 1, new[] { 2 });
            graph.AddNode(2, 1, new[] { 3 });
            graph.AddNode(3, 1, new[] { 4 });
            graph.AddNode(4, 1, new[] { 5 });

            Assert.ThrowsException<ArgumentException>(() => graph.AddNode(1, 1, new[] { 5 }));
        }

        [TestMethod]
        public void TestCycle()
        {
            var graph = new Graph<int, int>();
            graph.AddNode(1, 1, new[] { 2 });
            graph.AddNode(2, 1, new[] { 3 });
            graph.AddNode(3, 1, new[] { 4 });
            graph.AddNode(4, 1, new[] { 5 });

            Assert.ThrowsException<ArgumentException>(() => graph.AddNode(5, 1, new[] { 1 }));
            Assert.ThrowsException<ArgumentException>(() => graph.AddNode(5, 1, new[] { 5 }));
        }

        [TestMethod]
        public void TestDetached()
        {
            var graph = new Graph<int, int>();
            graph.AddNode(1, 1, new int[0]);
            graph.AddNode(2, 1, new[] { 1 });
            graph.AddNode(3, 1, new[] { 2 });
            graph.AddNode(4, 1, new[] { 10 });
            graph.AddNode(5, 1, new[] { 3, 4 });

            var (layers, detached) = graph.TopologicalSort();

            Assert.AreEqual(3, layers.Count);
            Assert.AreEqual(2, detached.Count);
        }

        [TestMethod]
        public void TestAllDetached()
        {
            var graph = new Graph<int, int>();
            graph.AddNode(1, 1, new[] { 10 });
            graph.AddNode(2, 1, new[] { 11 });
            graph.AddNode(3, 1, new[] { 12 });
            graph.AddNode(4, 1, new[] { 13 });

            var (layers, detached) = graph.TopologicalSort();

            Assert.AreEqual(0, layers.Count);
            Assert.AreEqual(4, detached.Count);
        }
    }
}
