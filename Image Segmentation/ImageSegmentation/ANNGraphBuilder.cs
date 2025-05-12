using System;
using System.Collections.Generic;

public class ANNGraphBuilder
{
    public class Graph
    {
        public int VertexCount { get; private set; }
        public List<Edge>[] AdjList { get; private set; }

        public class Edge
        {
            public int Destination { get; set; }
            public double Weight { get; set; }
        }

        public Graph(int vertexCount)
        {
            VertexCount = vertexCount;
            AdjList = new List<Edge>[vertexCount];
            for (int i = 0; i < vertexCount; i++)
                AdjList[i] = new List<Edge>();
        }

        public void AddEdge(int source, int destination, double weight)
        {
            // Check if edge already exists
            bool exists = false;
            foreach (var edge in AdjList[source])
            {
                if (edge.Destination == destination)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                AdjList[source].Add(new Edge { Destination = destination, Weight = weight });
                AdjList[destination].Add(new Edge { Destination = source, Weight = weight });
            }
        }
    }

    // Main method to build the graph using ANN
    public Graph BuildGraph(RGBPixel[,] imageMatrix, int k = 10)
    {
        int width = imageMatrix.GetLength(1);
        int height = imageMatrix.GetLength(0);
        int pixelCount = width * height;

        // Step 1: Convert pixels to 5D points
        double[][] points = new double[pixelCount][];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                var pixel = imageMatrix[y, x];
                points[index] = new double[]
                {
                    x, y,
                    pixel.red, pixel.green, pixel.blue
                };
            }
        }
        // Complexity: O(n) where n is number of pixels

        // Step 2: Build k-d tree
        KdTree kdTree = new KdTree(points, 5); // 5 dimensions (x,y,r,g,b)
        // Complexity: O(n log n) for balanced tree construction

        // Step 3 & 4: Find nearest neighbors and construct graph
        Graph graph = new Graph(pixelCount);

        for (int i = 0; i < pixelCount; i++)
        {
            // Find k nearest neighbors
            List<int> neighbors = kdTree.FindKNearestNeighbors(points[i], k + 1); // +1 because it includes self

            // Add edges to the graph
            foreach (int neighborIndex in neighbors)
            {
                if (i != neighborIndex) // Don't add self-loops
                {
                    double distance = CalculateDistance(points[i], points[neighborIndex]);
                    graph.AddEdge(i, neighborIndex, distance);
                }
            }
        }
        // Complexity: O(n * k log n) for finding k neighbors for each point

        return graph;
    }

    // Calculate Euclidean distance in 5D space
    private double CalculateDistance(double[] p1, double[] p2)
    {
        double sum = 0;
        for (int i = 0; i < p1.Length; i++)
        {
            double diff = p1[i] - p2[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }
}