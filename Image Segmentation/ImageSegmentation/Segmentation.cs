using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using ImageTemplate;
using System.Threading.Tasks;
using Segmenetation;
using System.Diagnostics;
using System.Windows.Forms;

namespace Segmentation
{
    public class Segmentation
    {
        public Dictionary<int, int> redLabels, greenLabels, blueLabels;
        public Dictionary<int, int> finalLabels; // Intersect Three Labels
        public int rows, columns;
        public RGBPixel[,] ImageMatrix { get; set; }
        private Graph graph;
        Stopwatch stopWatch;
        public Segmentation()
        {
            redLabels = new Dictionary<int, int>();
            greenLabels = new Dictionary<int, int>();
            blueLabels = new Dictionary<int, int>();
            finalLabels = new Dictionary<int, int>();
            stopWatch = new Stopwatch();
        }

        public void Segment(RGBPixel[,] smoothedImage, double k)
        {
            rows = ImageMatrix.GetLength(0);
            columns = ImageMatrix.GetLength(1);
            graph = new Graph(rows, columns);

            short[,] redChannel = new short[rows, columns];
            short[,] greenChannel = new short[rows, columns];
            short[,] blueChannel = new short[rows, columns];
            // smoothedImage = ImageMatrix; // for sample test cases

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    redChannel[i, j] = smoothedImage[i, j].red;
                    greenChannel[i, j] = smoothedImage[i, j].green;
                    blueChannel[i, j] = smoothedImage[i, j].blue;
                }
            }
            stopWatch.Start();
            // Segment each channel
            Parallel.Invoke (
                () => redLabels = SegmentChannel(redChannel, k),
                () => greenLabels = SegmentChannel(greenChannel, k),
                () => blueLabels = SegmentChannel(blueChannel, k)
            );
            ComputeFinalSegments();
        }

        private Dictionary<int, int> SegmentChannel(short[,] channel, double k)
        {
            DSU dsu = new DSU(rows, columns);
            var edges = graph.BuildEdges(channel);

            // edges.Sort((a, b) => a.Weight.CompareTo(b.Weight));

            // counting sort since weight is in range [0, 255]
            //
            List<Edge>[] weights = new List<Edge>[256];

            for (int i = 0; i < 256; i++)
                weights[i] = new List<Edge>();

            foreach (var edge in edges)
            {
                weights[edge.Weight].Add(edge);
            }

            edges.Clear();
            for (int i = 0; i < 256; i++)
            {
                edges.AddRange(weights[i]);
            }

            // Process edges
            foreach (var edge in edges)
            {
                int rootU = dsu.Find(edge.U);
                int rootV = dsu.Find(edge.V);

                if (rootU != rootV)
                {
                    double thresholdU = dsu.GetMaxInternalEdge(rootU) + (k / dsu.GetSize(rootU));
                    double thresholdV = dsu.GetMaxInternalEdge(rootV) + (k / dsu.GetSize(rootV));
                    double minThreshold = Math.Min(thresholdU, thresholdV);

                    if (edge.Weight <= minThreshold)
                    {
                        dsu.Union(edge.U, edge.V, edge.Weight);
                    }
                }
            }

            return AssignLabels(dsu);
        }

        private Dictionary<int, int> AssignLabels(DSU dsu)
        {
            var labels = new Dictionary<int, int>();
            int labelCount = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int pixelIndex = graph.getPixelIndex(i, j);
                    int root = dsu.Find(pixelIndex);
                    if (!labels.ContainsKey(root))
                    {
                        labels[root] = labelCount++;
                    }
                    labels[pixelIndex] = labels[root];
                }
            }
            return labels;
        }

        private void ComputeFinalSegments()
        {
            DSU dsu = new DSU(rows, columns);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int pixelIndex = graph.getPixelIndex(i, j);
                    var triplet = (redLabels[pixelIndex], greenLabels[pixelIndex], blueLabels[pixelIndex]);

                    foreach (var neighborIndex in graph.GetNeighbors(i,j))
                    {
                        var neighborTriplet = (redLabels[neighborIndex], greenLabels[neighborIndex], blueLabels[neighborIndex]);
                        if (triplet == neighborTriplet)
                        {
                            dsu.Union(pixelIndex, neighborIndex, 0); // No need to consider weight
                        }
                    }
                }
            }

            finalLabels = AssignLabels(dsu);
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

            var sortedSegments = GetSegmentSizes();

            // Generate and save the segmented image
            Random rand = new Random();
            Dictionary<int, (byte r, byte g, byte b)> labelColors = new Dictionary<int, (byte, byte, byte)>();

            // Assign random colors to each final segment label
            foreach (var label in finalLabels.Values.Distinct())
            {
                labelColors[label] = ((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256));
            }

            stopWatch.Stop();
            long elapsedSeconds = stopWatch.ElapsedMilliseconds;
            MessageBox.Show($"Segmentation completed in {elapsedSeconds:F3} ms", "Segmentation Time");

            // Write to text file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(textFilePath))
            {
                file.WriteLine(sortedSegments.Count);
                foreach (var segment in sortedSegments)
                {
                    file.WriteLine(segment.ToString());
                }
            }

            // Create the output image matrix
            RGBPixel[,] outputImage = new RGBPixel[rows, columns];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int pixelIndex = graph.getPixelIndex(i, j);
                    var color = labelColors[finalLabels[pixelIndex]];
                    outputImage[i, j] = new RGBPixel { red = color.r, green = color.g, blue = color.b };
                }
            }

            // Save the image using System.Drawing
            using (Bitmap bitmap = new Bitmap(columns, rows))
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        bitmap.SetPixel(j, i, Color.FromArgb(outputImage[i, j].red, outputImage[i, j].green, outputImage[i, j].blue));
                    }
                }
                bitmap.Save(imageFilePath, ImageFormat.Bmp); // Save as Bmp
                 //System.Diagnostics.Process.Start(imageFilePath); // opens image
            }
        }
    }
}