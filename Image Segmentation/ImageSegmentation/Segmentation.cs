using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using ImageTemplate;
using GraphConstruction;
using System.Threading.Tasks;

namespace Segmenetation
{
    public class Segmentation
    {
        public Graph redGrid, greenGrid, blueGrid;
        public Dictionary<Node, int> redLabels, greenLabels, blueLabels;
        public Dictionary<Node, int> finalLabels; // Intersect Three Labels
        public int rows, columns;
        public RGBPixel[,] ImageMatrix { get; set; }

        public Segmentation()
        {
            redLabels = new Dictionary<Node, int>();
            greenLabels = new Dictionary<Node, int>();
            blueLabels = new Dictionary<Node, int>();
            finalLabels = new Dictionary<Node, int>();
        }
        public void Segment(double sigma, double k, int filterSize = 5)
        {
            rows = ImageMatrix.GetLength(0);
            columns = ImageMatrix.GetLength(1);
            
            Graph.rows = rows;
            Graph.columns = columns;

            // Apply Gaussian blur with a larger sigma to smooth more
             RGBPixel[,] blurredImage = ImageOperations.GaussianFilter1D(ImageMatrix, filterSize, sigma);
             //RGBPixel[,] noGaussianFilterImage = ImageMatrix;
            
            redGrid = new Graph(blurredImage, "Red");
            greenGrid = new Graph(blurredImage, "Green");
            blueGrid = new Graph(blurredImage, "Blue");

            // Segment each channel
            Parallel.Invoke(
                () => redLabels = SegmentChannel(redGrid, k),
                () => greenLabels = SegmentChannel(greenGrid, k),
                () => blueLabels = SegmentChannel(blueGrid, k)
            );
            IntersectLabels();
        }

        private Dictionary<Node, int> SegmentChannel(Graph graph, double k)
{
            DSU dsu = new DSU(rows, columns);
            Dictionary<Node, int> labels = new Dictionary<Node, int>();

            // Collect edges
            List<(Node u, Node v, double weight)> edges = graph.getEdges();

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
            }

            int labelCount = 0;
            // Assign labels
            Dictionary<Node, int> rootToLabel = new Dictionary<Node, int>();
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    Node node = new Node(i, j);
                    Node root = dsu.Find(node);
                    if (!rootToLabel.ContainsKey(root))
                    {
                        rootToLabel[root] = labelCount++;
                    }
                    labels[node] = rootToLabel[root];

                }
            }

            int comp = labels.Values.Distinct().Count();
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

        public List<int> GetSegmentSizes()
        {
            // Count segment sizes
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

            // Return sizes sorted in decreasing order
            return segmentSizes.Values.OrderByDescending(size => size).ToList();
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

            RGBPixel[,] outputImage = new RGBPixel[rows, columns];
            Random rand = new Random();
            Dictionary<int, (byte r, byte g, byte b)> labelColors = new Dictionary<int, (byte, byte, byte)>();

            foreach (var label in finalLabels.Values.Distinct())
            {
                labelColors[label] = ((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256));
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