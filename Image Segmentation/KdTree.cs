using System;
using System.Collections.Generic;

public class KdTree
{
    public class KdNode
    {
        public double[] Point { get; set; }
        public int PixelIndex { get; set; }
        public KdNode Left { get; set; }
        public KdNode Right { get; set; }
        public int Depth { get; set; }
    }

    private KdNode root;
    private int dimensions;

    public KdTree(double[][] points, int dimensions)
    {
        this.dimensions = dimensions;
        root = BuildKdTree(points, 0, 0, points.Length - 1);
    }

    // Build a balanced k-d tree
    private KdNode BuildKdTree(double[][] points, int depth, int start, int end)
    {
        if (start > end)
            return null;

        // Select axis based on depth
        int axis = depth % dimensions;

        // Sort points along the axis and find median
        int median = start + (end - start) / 2;
        QuickSelectMedian(points, start, end, median, axis);

        // Create node and construct subtrees
        KdNode node = new KdNode
        {
            Point = points[median],
            PixelIndex = median,
            Depth = depth
        };

        node.Left = BuildKdTree(points, depth + 1, start, median - 1);
        node.Right = BuildKdTree(points, depth + 1, median + 1, end);

        return node;
    }

    // QuickSelect algorithm to find the median in O(n) time
    private void QuickSelectMedian(double[][] points, int start, int end, int k, int axis)
    {
        if (start >= end) return;

        int pivotIndex = Partition(points, start, end, axis);

        if (pivotIndex == k)
            return;
        else if (k < pivotIndex)
            QuickSelectMedian(points, start, pivotIndex - 1, k, axis);
        else
            QuickSelectMedian(points, pivotIndex + 1, end, k, axis);
    }

    private int Partition(double[][] points, int start, int end, int axis)
    {
        double pivot = points[end][axis];
        int i = start;

        for (int j = start; j < end; j++)
        {
            if (points[j][axis] <= pivot)
            {
                Swap(points, i, j);
                i++;
            }
        }

        Swap(points, i, end);
        return i;
    }

    private void Swap(double[][] points, int i, int j)
    {
        double[] temp = points[i];
        points[i] = points[j];
        points[j] = temp;
    }

    // Find k nearest neighbors for a target point
    public List<int> FindKNearestNeighbors(double[] targetPoint, int k, double epsilon = 0.0)
    {
        PriorityQueue<int, double> pq = new PriorityQueue<int, double>();
        SearchNode(root, targetPoint, k, pq, epsilon);

        List<int> neighbors = new List<int>();
        while (pq.Count > 0)
        {
            neighbors.Add(pq.Dequeue());
        }

        return neighbors;
    }

    // Search node recursively for nearest neighbors
    private void SearchNode(KdNode node, double[] targetPoint, int k, PriorityQueue<int, double> neighbors, double epsilon)
    {
        if (node == null) return;

        // Calculate distance to current point
        double distance = CalculateDistance(targetPoint, node.Point);

        // Add to neighbors if it's among k closest or we haven't found k yet
        if (neighbors.Count < k || distance < neighbors.PeekPriority())
        {
            // If queue is full, remove the farthest neighbor
            if (neighbors.Count == k)
                neighbors.Dequeue();

            neighbors.Enqueue(node.PixelIndex, distance);
        }

        // Select axis based on node depth
        int axis = node.Depth % dimensions;

        // Determine which subtree to search first
        KdNode firstSearch = node.Left;
        KdNode secondSearch = node.Right;

        if (targetPoint[axis] > node.Point[axis])
        {
            firstSearch = node.Right;
            secondSearch = node.Left;
        }

        // Search the near side
        SearchNode(firstSearch, targetPoint, k, neighbors, epsilon);

        // Check if we need to search the other side
        double axisDistance = Math.Abs(targetPoint[axis] - node.Point[axis]);

        // Only search other side if there's a possibility of finding closer points
        if (neighbors.Count < k || axisDistance < neighbors.PeekPriority() * (1 + epsilon))
        {
            SearchNode(secondSearch, targetPoint, k, neighbors, epsilon);
        }
    }

    // Calculate Euclidean distance
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

// Simple priority queue implementation
public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
{
    private List<(TElement Element, TPriority Priority)> _elements = new List<(TElement, TPriority)>();

    public int Count => _elements.Count;

    public void Enqueue(TElement element, TPriority priority)
    {
        _elements.Add((element, priority));
        int currentIndex = _elements.Count - 1;

        while (currentIndex > 0)
        {
            int parentIndex = (currentIndex - 1) / 2;
            if (_elements[parentIndex].Priority.CompareTo(_elements[currentIndex].Priority) <= 0)
                break;

            Swap(currentIndex, parentIndex);
            currentIndex = parentIndex;
        }
    }

    public TElement Dequeue()
    {
        if (_elements.Count == 0)
            throw new InvalidOperationException("Queue is empty");

        int lastIndex = _elements.Count - 1;
        TElement frontItem = _elements[0].Element;
        _elements[0] = _elements[lastIndex];
        _elements.RemoveAt(lastIndex);

        if (_elements.Count > 0)
            HeapifyDown(0);

        return frontItem;
    }

    public TElement Peek() => _elements.Count > 0 ? _elements[0].Element : throw new InvalidOperationException("Queue is empty");

    public TPriority PeekPriority() => _elements.Count > 0 ? _elements[0].Priority : throw new InvalidOperationException("Queue is empty");

    private void HeapifyDown(int currentIndex)
    {
        int leftChild = 2 * currentIndex + 1;
        int rightChild = 2 * currentIndex + 2;
        int smallestIndex = currentIndex;

        if (leftChild < _elements.Count && _elements[leftChild].Priority.CompareTo(_elements[smallestIndex].Priority) < 0)
            smallestIndex = leftChild;

        if (rightChild < _elements.Count && _elements[rightChild].Priority.CompareTo(_elements[smallestIndex].Priority) < 0)
            smallestIndex = rightChild;

        if (smallestIndex != currentIndex)
        {
            Swap(currentIndex, smallestIndex);
            HeapifyDown(smallestIndex);
        }
    }

    private void Swap(int i, int j)
    {
        var temp = _elements[i];
        _elements[i] = _elements[j];
        _elements[j] = temp;
    }
}