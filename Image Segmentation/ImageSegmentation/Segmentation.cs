using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using ImageTemplate;
using GraphConstruction;

namespace Segmenetation
{
    public class Segmentation
    {
        public Graph redGrid, greenGrid, blueGrid;
        public Dictionary<Node, int> redLabels, greenLabels, blueLabels;
        public Dictionary<Node, int> finalLabels;
        public int rows, columns;
        public RGBPixel[,] ImageMatrix { get; set; }

        public Segmentation()
        {
            redLabels = new Dictionary<Node, int>();
            greenLabels = new Dictionary<Node, int>();
            blueLabels = new Dictionary<Node, int>();
            finalLabels = new Dictionary<Node, int>();
        }
        public void Segment(double sigma, double k, int filterSize = 100)
        {
            rows = ImageMatrix.GetLength(0);
            columns = ImageMatrix.GetLength(1);

            // Apply Gaussian blur with a larger sigma to smooth more
            RGBPixel[,] blurredImage = ImageOperations.GaussianFilter1D(ImageMatrix, filterSize, sigma);

            redGrid = new Graph(blurredImage, "Red");
            greenGrid = new Graph(blurredImage, "Green");
            blueGrid = new Graph(blurredImage, "Blue");

            // Segment each channel
            redLabels = SegmentChannel(redGrid, k);
            greenLabels = SegmentChannel(greenGrid, k);
            blueLabels = SegmentChannel(blueGrid, k);
            IntersectLabels();
        }

        private Dictionary<Node, int> SegmentChannel(Graph graph, double k)
        {
            DSU dsu = new DSU(rows, columns);
            Dictionary<Node, int> labels = new Dictionary<Node, int>();

            // Collect edges
            List<(Node u, Node v, double weight)> edges = new List<(Node, Node, double)>();
            foreach (var node in graph.adj)
            {
                foreach (var (neighbor, weight) in node.Value)
                {
                   edges.Add((node.Key, neighbor, weight));
                }
            }

            // Sort edges
            edges.Sort((a, b) => a.weight.CompareTo(b.weight));

            // Process edges
            foreach (var (u, v, weight) in edges)
            {
                Node rootU = dsu.Find(u);
                Node rootV = dsu.Find(v);

                if (dsu.isConnected(u, v) == false)
                {
                    double thresholdU = dsu.GetMaxInternalEdge(rootU) + (k / dsu.GetSize(rootU));
                    double thresholdV = dsu.GetMaxInternalEdge(rootV) + (k / dsu.GetSize(rootV));
                    double minThreshold = Math.Min(thresholdU, thresholdV);

                    if (weight <= minThreshold)
                    {
                        dsu.Union(u, v, weight);
                    }
                }
                else
                {
                    dsu.Union(u, v, weight);
                }
            }

            int labelCount = 0;
            // Assign labels
            Dictionary<Node, int> rootToLabel = new Dictionary<Node, int>();
            foreach (var node in graph.adj.Keys)
            {
                Node root = dsu.Find(node);
                if (!rootToLabel.ContainsKey(root))
                {
                    rootToLabel[root] = labelCount++;
                }
                labels[node] = rootToLabel[root];
            }

            return labels;
        }
        private void IntersectLabels()
        {
            Dictionary<(int, int, int), int> labelTripletToFinal = new Dictionary<(int, int, int), int>();
            int finalLabelCount = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Node node = new Node(i, j);
                    var triplet = (redLabels[node], greenLabels[node], blueLabels[node]);
                    if (!labelTripletToFinal.ContainsKey(triplet))
                    {
                        labelTripletToFinal[triplet] = finalLabelCount++;
                    }
                    finalLabels[node] = labelTripletToFinal[triplet];
                }
            }
        }
        public int getNumberOfSegments()
        {
            return finalLabels.Values.Distinct().Count();
        }

        public void SaveOutput(string textFilePath, string imageFilePath)
        {
            // Count segments and sizes
            Dictionary<int, int> segmentSizes = new Dictionary<int, int>();
            foreach (var label in finalLabels.Values)
            {
                if (segmentSizes.ContainsKey(label))
                {
                    segmentSizes[label]++;
                }
                else
                {
                    segmentSizes[label] = 1;
                }
            }

            // Sort segments by size
            var sortedSegments = segmentSizes.OrderByDescending(x => x.Value).ToList();

            // Write to text file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(textFilePath))
            {
                file.WriteLine(sortedSegments.Count);
                foreach (var segment in sortedSegments)
                {
                    file.WriteLine(segment.Value);
                }
            }

            // Generate visualization image with fixed colors for better distinction
            RGBPixel[,] outputImage = new RGBPixel[rows, columns];
            var fixedColors = new (byte r, byte g, byte b)[]
            {
                (255, 0, 0),   // Red
                (0, 255, 0),   // Green
                (0, 0, 255),   // Blue
                (255, 255, 0), // Yellow
                (255, 0, 255), // Magenta
                (0, 255, 255), // Cyan
            };
            Dictionary<int, (byte r, byte g, byte b)> labelColors = new Dictionary<int, (byte, byte, byte)>();
            int colorIndex = 0;

            foreach (var label in finalLabels.Values.Distinct())
            {
                labelColors[label] = fixedColors[colorIndex % fixedColors.Length];
                colorIndex++;
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Node node = new Node(i, j);
                    var color = labelColors[finalLabels[node]];
                    outputImage[i, j] = new RGBPixel { red = color.r, green = color.g, blue = color.b };
                }
            }

            // Save the image
            using (Bitmap bitmap = new Bitmap(columns, rows))
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        bitmap.SetPixel(j, i, Color.FromArgb(outputImage[i, j].red, outputImage[i, j].green, outputImage[i, j].blue));
                    }
                }
                bitmap.Save(imageFilePath, ImageFormat.Png);
            }
        }
    }
}