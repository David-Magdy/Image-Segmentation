using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using ImageTemplate;
using GraphConstruction;
using static System.Net.Mime.MediaTypeNames;

namespace Segmenetation
{
    public class DSU
    {
        private Dictionary<Node, Node> parent;
        private Dictionary<Node, int> size;
        private Dictionary<Node, double> maxInternalEdge; // Tracks the largest edge weight in each component

        public DSU(int rows, int columns)
        {
            parent = new Dictionary<Node, Node>();
            size = new Dictionary<Node, int>();
            maxInternalEdge = new Dictionary<Node, double>();

            // Initialize each pixel as its own component
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Node node = new Node(i, j);
                    parent[node] = node;
                    size[node] = 1;
                    maxInternalEdge[node] = 0.0; // Initially, no edges within a single-pixel component
                }
            }
        }

        public Node Find(Node x)
        {
            if (!parent[x].Equals(x))
            {
                parent[x] = Find(parent[x]); // Path compression
            }
            return parent[x];
        }
        public bool isConnected(Node u, Node v)
        {
            u = Find(u);
            v = Find(v);
            return u.Equals(v);
        }

        public void Union(Node x, Node y, double edgeWeight)
        {
            Node rootX = Find(x);
            Node rootY = Find(y);

            if (!isConnected(x, y))
            {
                if (size[rootX] < size[rootY])
                {
                    (rootX, rootY) = (rootY, rootX);
                }

                parent[rootY] = rootX;
                size[rootX] += size[rootY];
                maxInternalEdge[rootX] = Math.Max(Math.Max(maxInternalEdge[rootX], maxInternalEdge[rootY]), edgeWeight);
            }
        }

        public int GetSize(Node x)
        {
            return size[Find(x)];
        }

        public double GetMaxInternalEdge(Node x)
        {
            return maxInternalEdge[Find(x)];
        }
    }
}
