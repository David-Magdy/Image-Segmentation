using Segmenetation;
using System;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace Segmentation
{
    public struct Edge
    {
        public int U { get; }
        public int V { get; }
        public short Weight { get; }

        public Edge(int u, int v, short weight)
        {
            U = u;
            V = v;
            Weight = weight;
        }
    }

    public class Graph
    {
        private int rows;
        private int columns;

        public Graph(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
        }

        public int getPixelIndex(int i , int j)
        {
            return i * columns + j;
        }

        private bool valid(int i , int j)
        {
            return (i >= 0 && i < rows && j >= 0 && j < columns);
        }

        public List<Edge> BuildEdges(short[,] channel)
        {
            var edges = new List<Edge>();
            short[] dx = { 1, 1, 1, 0 };
            short[] dy = { 0, 1, -1, 1 };

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int uIndex = getPixelIndex(i, j);
                    for (int k = 0; k < 4; k++)
                    {
                        int ni = i + dx[k];
                        int nj = j + dy[k];
                        if (valid(ni,nj))
                        {
                            int vIndex = getPixelIndex(ni,nj);
                            short weight = (short)Math.Abs(channel[i, j] - channel[ni, nj]);
                            edges.Add(new Edge(uIndex, vIndex, weight));
                        }
                    }
                }
            }

            return edges;
        }

        public List<int> GetNeighbors(int i , int j)
        {
            var neighbors = new List<int>();

            // down, down left, down right, right
            short[] dx = { 1, 1, 1, 0 };
            short[] dy = { 0, -1, 1, 1 };

            for (int k = 0; k < 4; k++)
            {
                int ni = i + dx[k];
                int nj = j + dy[k];
                if (valid(ni,nj))
                {
                    neighbors.Add(getPixelIndex(ni,nj));
                }
            }

            return neighbors;
        }
    }
}