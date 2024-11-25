using System.Collections;
using System.Collections.Generic;

public class TDAGraphGeneric<T>
{
    public List<T> VertexList;

    public Dictionary<T, List<(T, int)>> AdjacencyList;

    public TDAGraphGeneric()
    {
        Initialize();
    }

    private void Initialize()
    {
        VertexList = new List<T>();
        AdjacencyList = new Dictionary<T, List<(T, int)>>();
    }

    public bool AddVertex(T value)
    {
        if (FindVertex(value) == null)
        {
            VertexList.Add(value);
            AdjacencyList.Add(value, new List<(T, int)>());
            return true;
        }

        return false;
    }

    public T FindVertex(T index)
    {
        foreach (var vertex in VertexList)
        {
            if (index.Equals(vertex))
            {
                return vertex;
            }
        }

        return default;
    }

    public bool AddEdge(T origin, T target, int weight)
    {
        if (!DoesEdgeExist(origin, target))
        {
            AdjacencyList[FindVertex(origin)].Add((FindVertex(target), weight));
        }

        return false;
    }

    public bool RemoveEdge(T origin, T target)
    {
        if (DoesEdgeExist(origin, target))
        {
            AdjacencyList[FindVertex(origin)].Remove((FindVertex(target), 0));
            return true;
        }

        return false;
    }

    public bool DoesEdgeExist(T origin, T target)
    {
        if (AdjacencyList.ContainsKey(origin))
        {
            foreach (var edge in AdjacencyList[origin])
            {
                if (edge.Item1.Equals(FindVertex(target)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int GetEdgeWeight(T origin, T target)
    {
        if (DoesEdgeExist(origin, target))
        {
            foreach (var edge in AdjacencyList[FindVertex(origin)])
            {
                if (edge.Item1.Equals(FindVertex(target)))
                {
                    return edge.Item2;
                }
            }
        }

        return -1;
    }
}
