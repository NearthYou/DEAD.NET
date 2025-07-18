using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexamap
{
    public static class AStar
    {
        // 노드 풀링을 위한 캐시
        private static Dictionary<Coords, Node> _allNodes = new Dictionary<Coords, Node>();
        private static Queue<Node> _nodePool = new Queue<Node>();
        private static readonly int MAX_POOL_SIZE = 1000;

        // 우선순위 큐 구현 (MinHeap)
        private class PriorityQueue<T> where T : IComparable<T>
        {
            private List<T> _heap = new List<T>();

            public int Count => _heap.Count;

            public void Enqueue(T item)
            {
                _heap.Add(item);
                int childIndex = _heap.Count - 1;
                while (childIndex > 0)
                {
                    int parentIndex = (childIndex - 1) / 2;
                    if (_heap[childIndex].CompareTo(_heap[parentIndex]) >= 0)
                        break;
                    
                    T temp = _heap[childIndex];
                    _heap[childIndex] = _heap[parentIndex];
                    _heap[parentIndex] = temp;
                    childIndex = parentIndex;
                }
            }

            public T Dequeue()
            {
                if (_heap.Count == 0)
                    throw new InvalidOperationException("Queue is empty");

                T result = _heap[0];
                int lastIndex = _heap.Count - 1;
                _heap[0] = _heap[lastIndex];
                _heap.RemoveAt(lastIndex);

                if (_heap.Count > 0)
                {
                    int parentIndex = 0;
                    while (true)
                    {
                        int leftChildIndex = parentIndex * 2 + 1;
                        int rightChildIndex = parentIndex * 2 + 2;
                        int smallestIndex = parentIndex;

                        if (leftChildIndex < _heap.Count && _heap[leftChildIndex].CompareTo(_heap[smallestIndex]) < 0)
                            smallestIndex = leftChildIndex;

                        if (rightChildIndex < _heap.Count && _heap[rightChildIndex].CompareTo(_heap[smallestIndex]) < 0)
                            smallestIndex = rightChildIndex;

                        if (smallestIndex == parentIndex)
                            break;

                        T temp = _heap[parentIndex];
                        _heap[parentIndex] = _heap[smallestIndex];
                        _heap[smallestIndex] = temp;
                        parentIndex = smallestIndex;
                    }
                }

                return result;
            }

            public T Peek()
            {
                if (_heap.Count == 0)
                    throw new InvalidOperationException("Queue is empty");
                return _heap[0];
            }
        }

        public static List<Coords> FindPath(Section start, Section end, HashSet<Coords> available, HashSet<Coords> others, Func<int> randomCost = null)
        {
            HashSet<Coords> allAvailable = available
                .Concat(start.Tiles.Select(t => t.Coords))
                .Concat(end.Tiles.Select(t => t.Coords))
                .ToHashSet();

            Node nodeStart = GetNode(getCentroid(start));
            Node nodeEnd = GetNode(getCentroid(end));

            // 우선순위 큐 사용으로 성능 향상
            var openList = new PriorityQueue<Node>();
            var closedSet = new HashSet<Coords>();
            var gScore = new Dictionary<Coords, int>();
            var fScore = new Dictionary<Coords, int>();

            // 초기화
            gScore[nodeStart.Coords] = 0;
            fScore[nodeStart.Coords] = CalculateHeuristic(nodeStart.Coords, nodeEnd.Coords);
            openList.Enqueue(nodeStart);

            while (openList.Count > 0)
            {
                Node current = openList.Dequeue();

                if (current.Coords.Equals(nodeEnd.Coords))
                {
                    return ReconstructPath(current, nodeStart);
                }

                closedSet.Add(current.Coords);

                foreach (var neighbor in GetNeighbourNodes(current))
                {
                    if (!allAvailable.Contains(neighbor.Coords) || closedSet.Contains(neighbor.Coords))
                        continue;

                    int tentativeGScore = gScore[current.Coords] + 10;
                    
                    // others에 포함된 좌표는 비용 감소
                    if (others.Contains(neighbor.Coords))
                        tentativeGScore -= 8;

                    if (randomCost != null)
                        tentativeGScore += randomCost.Invoke();

                    if (!gScore.ContainsKey(neighbor.Coords) || tentativeGScore < gScore[neighbor.Coords])
                    {
                        neighbor.Parent = current;
                        gScore[neighbor.Coords] = tentativeGScore;
                        fScore[neighbor.Coords] = tentativeGScore + CalculateHeuristic(neighbor.Coords, nodeEnd.Coords);
                        
                        openList.Enqueue(neighbor);
                    }
                }
            }

            return null; // 경로를 찾지 못함
        }

        public static List<Coords> FindPath(Coords start, Coords end)
        {
            Node nodeStart = GetNode(start);
            Node nodeEnd = GetNode(end);

            // 우선순위 큐 사용
            var openList = new PriorityQueue<Node>();
            var closedSet = new HashSet<Coords>();
            var gScore = new Dictionary<Coords, int>();
            var fScore = new Dictionary<Coords, int>();

            // 초기화
            gScore[nodeStart.Coords] = 0;
            fScore[nodeStart.Coords] = CalculateHeuristic(nodeStart.Coords, nodeEnd.Coords);
            openList.Enqueue(nodeStart);

            while (openList.Count > 0)
            {
                Node current = openList.Dequeue();

                if (current.Coords.Equals(nodeEnd.Coords))
                {
                    return ReconstructPath(current, nodeStart);
                }

                closedSet.Add(current.Coords);

                foreach (var neighbor in GetNeighbourNodes(current))
                {
                    if (closedSet.Contains(neighbor.Coords))
                        continue;

                    int tentativeGScore = gScore[current.Coords] + 10;

                    if (!gScore.ContainsKey(neighbor.Coords) || tentativeGScore < gScore[neighbor.Coords])
                    {
                        neighbor.Parent = current;
                        gScore[neighbor.Coords] = tentativeGScore;
                        fScore[neighbor.Coords] = tentativeGScore + CalculateHeuristic(neighbor.Coords, nodeEnd.Coords);
                        
                        openList.Enqueue(neighbor);
                    }
                }
            }

            return null; // 경로를 찾지 못함
        }

        // 경로 재구성 함수
        private static List<Coords> ReconstructPath(Node endNode, Node startNode)
        {
            var path = new List<Coords>();
            Node current = endNode;

            while (current != null && !current.Coords.Equals(startNode.Coords))
            {
                path.Add(current.Coords);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        // 휴리스틱 함수 (맨해튼 거리)
        private static int CalculateHeuristic(Coords from, Coords to)
        {
            return Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
        }

        // 노드 풀링을 통한 메모리 효율성 개선
        private static Node GetNode(Coords coords)
        {
            if (_allNodes.TryGetValue(coords, out Node existingNode))
            {
                return existingNode;
            }

            Node newNode;
            if (_nodePool.Count > 0)
            {
                newNode = _nodePool.Dequeue();
                newNode.Reset(coords);
            }
            else
            {
                newNode = new Node(coords);
            }

            _allNodes[coords] = newNode;
            return newNode;
        }

        // 노드 풀 정리
        public static void ClearNodePool()
        {
            _allNodes.Clear();
            _nodePool.Clear();
        }

        // 메모리 사용량 제한
        private static void LimitMemoryUsage()
        {
            if (_allNodes.Count > MAX_POOL_SIZE)
            {
                var keysToRemove = _allNodes.Keys.Take(_allNodes.Count - MAX_POOL_SIZE / 2).ToList();
                foreach (var key in keysToRemove)
                {
                    var node = _allNodes[key];
                    node.Reset();
                    _nodePool.Enqueue(node);
                    _allNodes.Remove(key);
                }
            }
        }

        // Find at which index an element of the path meet the section and discard every other element after this one
        private static void truncatePath(List<Node> path, Section section)
        {
            var frontiers = Coords.Expand(section.LimitsCoords.ToList(), 1);
            int firstContactIndex = path.FindIndex(n => frontiers.Contains(n.Coords));

            if (firstContactIndex != -1 && firstContactIndex != path.Count - 1)
                path.RemoveRange(firstContactIndex + 1, path.Count - firstContactIndex - 1);
        }

        private static List<Node> GetNeighbourNodes(Node node)
        {
            var neighbourNodes = new List<Node>();

            foreach (Coords c in node.Coords.Neighbours)
            {
                Node neighbourNode = GetNode(c);
                neighbourNodes.Add(neighbourNode);
            }

            return neighbourNodes;
        }

        private static Coords getCentroid(Section section)
        {
            int sumX = section.Tiles.Sum(t => t.Coords.X);
            int sumY = section.Tiles.Sum(t => t.Coords.Y);

            int avgX = (int)Math.Round((double)sumX / section.Tiles.Count());
            int avgY = (int)Math.Round((double)sumY / section.Tiles.Count());

            return new Coords(avgX, avgY);
        }

        private class Node : IComparable<Node>
        {
            public Node Parent;
            public Coords Coords;
            public List<Node> Neighbours;
            public int Cost;
            public int Distance;
            public float Heuristic => Distance + Cost;

            public Node()
            {
                Reset();
            }

            public Node(Coords coords)
            {
                Coords = coords;
                Reset();
            }

            public void Reset(Coords coords = default)
            {
                Parent = null;
                Coords = coords;
                Neighbours = null;
                Cost = 0;
                Distance = 0;
            }

            public int CompareTo(Node other)
            {
                return Heuristic.CompareTo(other.Heuristic);
            }
        }
    }
}
