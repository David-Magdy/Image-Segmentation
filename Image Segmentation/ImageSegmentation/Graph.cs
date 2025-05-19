using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using ImageTemplate;
using static System.Net.Mime.MediaTypeNames;


namespace GraphConstruction
{
    public struct Node : IEquatable<Node>
    {
        public int i, j;
        public Node(int i, int j)
        {
            this.i = i;
            this.j = j;
        }

        public bool Equals(Node other)
        {
            return (i == other.i && j == other.j);
        }

        public override bool Equals(object obj)
        {
            return obj is Node other && Equals(other);
        }

        public override int GetHashCode()
        {
            return i * Graph.columns + j;
        }
    }

        public class Graph
        {
            public static int rows, columns;

            private double[,] channel;

            private int[] dx = { 1, 1, 1, -1, -1, -1, 0, 0 };
            private int[] dy = { 1, -1, 0, 1, -1, 0, 1, -1 };

            private bool valid(int x, int y)
            {
                return (x >= 0 && y >= 0 && x < rows && y < columns);
            }

            private double getWeight(int i, int j, int x, int y)
            {
                return Math.Abs(channel[i, j] - channel[x, y]);
            }

            public List<(Node u, Node v, double weight)> getEdges()
            {
                List<(Node u, Node v, double weight)> edges = new List<(Node, Node, double)>();

                for (int i = 0; i < rows; ++i)
                {
                    for (int j = 0; j < columns; ++j)
                    {
                        Node u = new Node(i, j);
                        for (int k = 0; k < 8; ++k)
                        {
                            int new_x = i + dx[k];
                            int new_y = j + dy[k];

                            if (valid(new_x, new_y))
                            {
                                Node v = new Node(new_x, new_y);
                                double weight = getWeight(i, j, new_x, new_y);
                                edges.Add((u, v, weight));
                            }
                        }
                    }
                }
                return edges;
            }
            public Graph(RGBPixel[,] ImageMatrix, string channelType)
            {
                

                // Create the channel matrix based on the selected color
                channel = new double[rows, columns];

                // Fill the channel matrix based on the color (Red, Green, Blue)
                for (int i = 0; i < rows; ++i)
                {
                    for (int j = 0; j < columns; ++j)
                    {
                        if (channelType == "Red")
                            channel[i, j] = ImageMatrix[i, j].red;
                        else if (channelType == "Green")
                            channel[i, j] = ImageMatrix[i, j].green;
                        else if (channelType == "Blue")
                            channel[i, j] = ImageMatrix[i, j].blue;
                    }
                }
            }
        }
    }
