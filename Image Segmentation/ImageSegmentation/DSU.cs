using System;

namespace Segmenetation
{
    public class DSU
    {
        private int[] parent;
        private int[] size;
        private double[] maxInternalEdge; // Tracks the largest edge weight in each component
        private int totalPixels;
        public DSU(int rows, int columns)
        {
            totalPixels = rows * columns;
            parent = new int[totalPixels];
            size = new int[totalPixels];
            maxInternalEdge = new double[totalPixels];

            // Initialize each pixel as its own component
            for (int i = 0; i < totalPixels; ++i)
            {
                parent[i] = i;
                size[i] = 1;
                maxInternalEdge[i] = 0.0;
            }
        }

        public int Find(int i)
        {
            if (parent[i] != i)
            {
                parent[i] = Find(parent[i]); // Path compression
            }
            return parent[i];
        }

        public void Union(int u, int v, double edgeWeight)
        {
            int rootU = Find(u);
            int rootV = Find(v);

            if (rootU != rootV)
            {
                if (size[rootU] < size[rootV])
                {
                    (rootU, rootV) = (rootV, rootU);
                }
                parent[rootV] = rootU;
                size[rootU] += size[rootV];
                maxInternalEdge[rootU] = Math.Max(Math.Max(maxInternalEdge[rootU], maxInternalEdge[rootV]), edgeWeight);
            }
        }

        public int GetSize(int x)
        {
            return size[Find(x)];
        }

        public double GetMaxInternalEdge(int x)
        {
            return maxInternalEdge[Find(x)];
        }
    }
}
