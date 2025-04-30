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

    public struct Node
    {
        public int i, j;
        public Node(int i, int j)
        {
            this.i = i;
            this.j = j;
        }
    }

    public class Graph
    {
        public Dictionary<Node, List<Tuple<Node, double>>> adj;

        private int columns;
        private int rows;


        private int[] dx = { 1, 1, 1, -1, -1, -1, 0, 0 };
        private int[] dy = { 1, -1, 0, 1, -1, 0, 1, -1 };

        private bool valid(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < rows && y < columns);
        }

        private double getIntensity(RGBPixel[,] image, int i, int j, int x, int y)
        {
            double intensity1 = (image[i, j].red + image[i, j].green + image[i, j].blue) / 3.0;
            double intensity2 = (image[x, y].red + image[x, y].green + image[x, y].blue) / 3.0;

            return Math.Abs(intensity1 - intensity2);
        }

        public Graph(string imagePath, double sigma, int filterSize)
        {
            RGBPixel[,] image = ImageOperations.OpenImage(imagePath);
            image = ImageOperations.GaussianFilter1D(image, filterSize, sigma);

            columns = ImageOperations.GetWidth(image);
            rows = ImageOperations.GetHeight(image);

            adj = new Dictionary<Node, List<Tuple<Node, double>>>();

            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    for (int k = 0; k < 8; ++k)
                    {
                        int new_x = i + dx[k];
                        int new_y = j + dy[k];
                        if (valid(new_x, new_y))
                        {
                            Node u = new Node(i, j);
                            Node v = new Node(new_x, new_y);

                            double weight = getIntensity(image, i, j, new_x, new_y);

                            if (!adj.ContainsKey(u))
                                adj[u] = new List<Tuple<Node, double>>();

                            if (!adj.ContainsKey(v))
                                adj[v] = new List<Tuple<Node, double>>();

                            adj[u].Add(new Tuple<Node, double>(v, weight));
                            adj[v].Add(new Tuple<Node, double>(u, weight));
                        }
                    }
                }
            }
        }
    }
}