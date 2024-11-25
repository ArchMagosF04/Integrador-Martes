using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ModularLabyrinth : MonoBehaviour
{
    [Header("Labyrinth Variables")]

    [SerializeField] private int Rows = 12;
    [SerializeField] private int Columns = 12;

    [Header("Components")]

    [SerializeField] private NodeObject nodePrefab;

    [SerializeField] private GameObject pointer;

    [SerializeField] private GameObject player;




    private Stack<NodeObject> nodesToTravel = new Stack<NodeObject>();
    private List<NodeObject> nodesExplored = new List<NodeObject>();
    private Dictionary<NodeObject, NodeObject> chartingDict = new Dictionary<NodeObject, NodeObject>();
    private Stack<NodeObject> nodesToGoal = new Stack<NodeObject>();

    private NodeObject[,] nodeGrid;

    private NodeObject currentNodeSelected;
    private Vector2 currentNodeLocation = new Vector2 (0, 0);

    private TDAGraphGeneric<MazeSpace> mazeGraph;

    private NodeObject startNode;
    private NodeObject endNode;

    private bool isSearching = false;

    private void Awake()
    {
        mazeGraph = new TDAGraphGeneric<MazeSpace>();

        nodeGrid = new NodeObject[Rows, Columns];

        CreateGrid();

        SetPointer(0, 0);
    }

    private void SetPointer(int row, int column)
    {
        if (row < 0)
        {
            row = Rows - 1;
        }else if (row >= Rows)
        {
            row = 0;
        }

        if (column < 0)
        {
            column = Columns - 1;
        }
        else if (column >= Columns)
        {
            column = 0;
        }

        currentNodeLocation = new Vector2 (row, column);
        pointer.transform.position = nodeGrid[row, column].transform.position;
        currentNodeSelected = nodeGrid[row, column];
    }

    private void CreateGrid()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                NodeObject node = Instantiate(nodePrefab, transform);
                node.transform.position = new Vector2 (transform.position.x + 0.75f * j, transform.position.y + 0.75f * -i);

                node.Initialize(i, j, mazeGraph);

                nodeGrid[i, j] = node;
            }
        }
    }

    private void Update()
    {
        MovePointer();
        if (!isSearching) PointerActions();

        if (Input.GetKeyDown(KeyCode.Space) && CanStartToSolve())
        {
            FindExit();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ShowVertexConnections(currentNodeSelected.node);
        }
    }

    private void FindExit()
    {
        isSearching = true;

        nodesToTravel.Clear();
        nodesExplored.Clear();
        nodesToGoal.Clear();
        chartingDict.Clear();

        NodeObject currentNode = startNode;
        player.transform.position = nodeGrid[(int)startNode.node.Row, (int)startNode.node.Column].transform.position;

        CatalogNode(currentNode);
        ExploreLabyrinth(currentNode);
    }

    private void ExploreLabyrinth(NodeObject currentNode)
    {
        while (currentNode != endNode && nodesToTravel.Count > 0)
        {
            currentNode = nodesToTravel.Pop();

            NodeObject BottomNode = null;
            NodeObject RightNode = null;
            NodeObject TopNode = null;
            NodeObject LeftNode = null;



            if (currentNode.node.Row < Rows)
            {
                BottomNode = nodeGrid[currentNode.node.Row + 1, currentNode.node.Column];
            }

            if (currentNode.node.Column < Columns)
            {
                RightNode = nodeGrid[currentNode.node.Row, currentNode.node.Column + 1];
            }

            if (currentNode.node.Row > 0)
            {
                TopNode = nodeGrid[currentNode.node.Row - 1, currentNode.node.Column];
            }

            if (currentNode.node.Column > 0)
            {
                LeftNode = nodeGrid[currentNode.node.Row, currentNode.node.Column - 1];
            }

            if (BottomNode != null)
            {
                if (mazeGraph.DoesEdgeExist(mazeGraph.FindVertex(currentNode.node), mazeGraph.FindVertex(BottomNode.node)) && !nodesExplored.Contains(BottomNode))
                {
                    chartingDict.Add(BottomNode, currentNode);
                    CatalogNode(BottomNode);
                }
            }
            if (RightNode != null)
            {
                if (mazeGraph.DoesEdgeExist(mazeGraph.FindVertex(currentNode.node), mazeGraph.FindVertex(RightNode.node)) && !nodesExplored.Contains(RightNode))
                {
                    chartingDict.Add(RightNode, currentNode);
                    CatalogNode(RightNode);
                }
            }
            if (TopNode != null)
            {
                if (mazeGraph.DoesEdgeExist(mazeGraph.FindVertex(currentNode.node), mazeGraph.FindVertex(TopNode.node)) && !nodesExplored.Contains(TopNode))
                {
                    chartingDict.Add(TopNode, currentNode);
                    CatalogNode(TopNode);
                }
            }
            if (LeftNode != null)
            {
                if (mazeGraph.DoesEdgeExist(mazeGraph.FindVertex(currentNode.node), mazeGraph.FindVertex(LeftNode.node)) && !nodesExplored.Contains(LeftNode))
                {
                    chartingDict.Add(LeftNode, currentNode);
                    CatalogNode(LeftNode);
                }
            }
        }

        if (currentNode == endNode)
        {
            Debug.Log("Exit Reachable");
            StartCoroutine(Pathing());
        }
        else
        {
            Debug.Log("Couldn't find Exit");
            isSearching = false;
        }
    }

    private IEnumerator Pathing()
    {
        NodeObject current = null;

        ChartFinalPath(endNode);

        while (nodesToGoal.Count > 0)
        {
            current = nodesToGoal.Pop();
            player.transform.position = nodeGrid[(int)current.node.Row, (int)current.node.Column].transform.position;

            Debug.Log($"({current.node.Row}, {current.node.Column})");

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Exit Reached");

        isSearching = false;
    }

    private void ChartFinalPath(NodeObject node)
    {
        nodesToGoal.Push(node);
        if (chartingDict.ContainsKey(node))
        {
            ChartFinalPath(chartingDict[node]);
        }
    }

    public void ShowVertexConnections(MazeSpace origin)
    {
        if (mazeGraph.AdjacencyList.ContainsKey(origin))
        {
            foreach (var target in mazeGraph.AdjacencyList[origin])
            {
                Debug.Log($"Origin: {origin.Row}, {origin.Column}.    Target: {target.Item1.Row}, {target.Item1.Column}.");
            }
        }
    }


    private void CatalogNode(NodeObject node)
    {
        nodesToTravel.Push(node);
        nodesExplored.Add(node);
    }

    private bool CanStartToSolve()
    {
        if (startNode == null || endNode == null) return false;

        return true;
    }

    private void MovePointer()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetPointer((int)currentNodeLocation.x - 1, (int)currentNodeLocation.y);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SetPointer((int)currentNodeLocation.x + 1, (int)currentNodeLocation.y);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            SetPointer((int)currentNodeLocation.x, (int)currentNodeLocation.y - 1);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            SetPointer((int)currentNodeLocation.x, (int)currentNodeLocation.y + 1);
        }
    }

    private void PointerActions()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetStartingPoint();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetEndPoint();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            MakeFloorNode();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            MakeWallNode();
        }
    }

    private void SetStartingPoint()
    {
        if (currentNodeSelected == endNode) return;

        if (startNode != null)
        {
            startNode.ChangeNodeColor(Color.white);
        }

        if (currentNodeSelected.IsWall)
        {
            currentNodeSelected.ToggleWall(false);
        }

        startNode = currentNodeSelected;
        startNode.ChangeNodeColor(Color.green);

        player.transform.position = nodeGrid[startNode.node.Row, startNode.node.Column].transform.position;
    }

    private void SetEndPoint()
    {
        if (currentNodeSelected == startNode) return;

        if (endNode != null)
        {
            endNode.ChangeNodeColor(Color.white);
        }

        if (currentNodeSelected.IsWall)
        {
            currentNodeSelected.ToggleWall(false);
        }

        endNode = currentNodeSelected;
        endNode.ChangeNodeColor(Color.red);
    }

    private void MakeFloorNode()
    {
        currentNodeSelected.ToggleWall(false);

        if (currentNodeSelected == startNode)
        {
            startNode = null;
        }

        if (currentNodeSelected == endNode)
        {
            endNode = null;
        }
    }

    private void MakeWallNode()
    {
        currentNodeSelected.ToggleWall(true);

        if (currentNodeSelected == startNode)
        {
            startNode = null;
        }

        if (currentNodeSelected == endNode)
        {
            endNode = null;
        }
    }
}
