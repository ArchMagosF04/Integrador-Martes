using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ModularLabyrinth : MonoBehaviour
{
    [Header("Labyrinth Variables")] //Variables to be edited before entering play mode to change some parameters in the labyrinth.

    [SerializeField] private int Rows = 12;
    [SerializeField] private int Columns = 12;
    [SerializeField] private float timeBetweenSteps = 0.3f; 

    [Header("Components")] //Components needed to operate this script.

    [SerializeField] private NodeObject nodePrefab;

    [SerializeField] private GameObject pointer;

    [SerializeField] private GameObject player;

    [SerializeField] private TextMeshProUGUI solveText;

    //All this collections are used for the pathfinding of the exit.
    private Stack<NodeObject> nodesToTravel = new Stack<NodeObject>();
    private List<NodeObject> nodesExplored = new List<NodeObject>();
    private Dictionary<NodeObject, NodeObject> chartingDict = new Dictionary<NodeObject, NodeObject>();
    private Stack<NodeObject> nodesToGoal = new Stack<NodeObject>();

    private NodeObject[,] nodeGrid; //Stores our created prefabs.

    private NodeObject currentNodeSelected; //References the node selected by the pointer.
    private Vector2 currentNodeLocation = new Vector2 (0, 0);

    private TDAGraphGeneric<MazeSpace> mazeGraph; //The graph that stores all the nodes and connections.

    private NodeObject startNode;
    private NodeObject endNode;

    private bool isSearching = false;

    private void Awake()
    {
        mazeGraph = new TDAGraphGeneric<MazeSpace>();

        nodeGrid = new NodeObject[Rows, Columns];

        CreateGrid();

        SetPointer(0, 0);

        solveText.text = "Labyrinth has no start and/or no end.";
        solveText.color = Color.yellow;
    }

    private void SetPointer(int row, int column) //Sets the selected node to the pointers position.
    {
        if (row < 0) //Loops to the other end of the grid 
        {
            row = Rows - 1;
        }
        else if (row >= Rows)
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

    private void CreateGrid() //Spawns all the nodes to fill a grid determined by the Rows & Columns chosen in the inspector.
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                NodeObject node = Instantiate(nodePrefab, transform);
                node.transform.position = new Vector2 (transform.position.x + 0.75f * j, transform.position.y + 0.75f * -i); //Sets the position of the node relative to its value on the grid.

                node.Initialize(i, j, mazeGraph); //The NodeObject receives the row & column, and we also send the graph so all nodes reference the same one.

                nodeGrid[i, j] = node; //The node is added to the array so it can be accessed later.
            }
        }
    }

    private void Update() //Controls the player's inputs.
    {
        MovePointer();
        if (!isSearching) PointerActions();

        if (Input.GetKeyDown(KeyCode.Space))
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
        player.transform.position = nodeGrid[(int)startNode.node.Row, (int)startNode.node.Column].transform.position;

        if (ExploreLabyrinth())
        {
            isSearching = true;

            StartCoroutine(Pathing());
        }
    }

    private bool ExploreLabyrinth() //Searches for the exit from the start and returns whether it is possible to solve.
    {
        //Clears collections to reset all routes.
        nodesToGoal.Clear();
        chartingDict.Clear();
        nodesToTravel.Clear();
        nodesExplored.Clear();

        if (startNode == null || endNode == null) //if there is no start or end, it is automaticly unsolvable. 
        {
            solveText.text = "Labyrinth has no start and/or no end.";
            solveText.color = Color.yellow;

            return false;
        }

        CatalogNode(startNode);

        NodeObject currentNode = null;

        while (currentNode != endNode && nodesToTravel.Count > 0) //Does the actual search.
        {
            currentNode = nodesToTravel.Pop(); //Selects the node to explore from.

            CheckPosibleConnections(currentNode);
        }

        if (currentNode == endNode) //If when the search ends and the node we ended up on is the ending node, then returns that it is solvable.
        {
            solveText.text = "Labyrinth is Solvable.";
            solveText.color = Color.green;

            return true;
        }
        else //If the program search all possible nodes and didn't find the exit, then it returns that it is not solvable.
        {
            solveText.text = "Labyrinth is not Solvable.";
            solveText.color = Color.red;

            return false;
        }
    }

    private void CheckPosibleConnections(NodeObject currentNode) //Searches all connections of the given node.
    {
        foreach (var target in mazeGraph.AdjacencyList[currentNode.node])
        {
            if (!nodesExplored.Contains(nodeGrid[target.Item1.Row, target.Item1.Column]))
            {
                chartingDict.Add(nodeGrid[target.Item1.Row, target.Item1.Column], currentNode);
                CatalogNode(nodeGrid[target.Item1.Row, target.Item1.Column]);
            }
        }
    }

    private IEnumerator Pathing() //Makes the player move through the path to the exit.
    {
        NodeObject current = null;

        ChartFinalPath(endNode);

        while (nodesToGoal.Count > 0) //Updates the player's position until it reaches the end.
        {
            current = nodesToGoal.Pop();
            player.transform.position = nodeGrid[(int)current.node.Row, (int)current.node.Column].transform.position;

            if (current != startNode && current != endNode) current.ChangeNodeColor(Color.white);

            yield return new WaitForSeconds(timeBetweenSteps);
        }

        Debug.Log("Exit Reached");

        isSearching = false;
    }

    private void ChartFinalPath(NodeObject node) //Creates a (not always optimal) route that goes directly to the exit from the start.
    {
        nodesToGoal.Push(node);

        if (node != startNode && node != endNode) node.ChangeNodeColor(Color.yellow); //Colors the path tiles in yellow.

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


    private void CatalogNode(NodeObject node) //Archives the given node as an explored one.
    {
        nodesToTravel.Push(node);
        nodesExplored.Add(node);
    }

    private void MovePointer() //Moves the pointer around the grid.
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

    private void PointerActions() //Turns the selected node into the different types.
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetStartingPoint();
            ExploreLabyrinth();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetEndPoint();
            ExploreLabyrinth();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            MakeFloorNode();
            ExploreLabyrinth();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            MakeWallNode();
            ExploreLabyrinth();
        }
    }

    private void SetStartingPoint() 
    {
        if (currentNodeSelected == endNode) //If overriding the end node, then set it to null, so the same node can't be both the start and end.
        {
            endNode = null;
        }

        if (startNode != null) //If there is an existing start node, then change it to a normal floor tile.
        {
            startNode.ChangeNodeColor(Color.white);
        }

        if (currentNodeSelected.IsWall) //if the selected node is a wall, turned into floor, so the node can have connections.
        {
            currentNodeSelected.ToggleWall(false);
        }

        //Makes the selected node the new start node.

        startNode = currentNodeSelected;
        startNode.ChangeNodeColor(Color.green);

        player.transform.position = nodeGrid[startNode.node.Row, startNode.node.Column].transform.position;
    }

    private void SetEndPoint()
    {
        if (currentNodeSelected == startNode) //If overriding the start node, then set it to null, so the same node can't be both the start and end.
        {
            startNode = null;
        }

        if (endNode != null) //If there is an existing end node, then change it to a normal floor tile.
        {
            endNode.ChangeNodeColor(Color.white);
        }

        if (currentNodeSelected.IsWall) //if the selected node is a wall, turned into floor, so the node can have connections.
        {
            currentNodeSelected.ToggleWall(false);
        }

        //Makes the selected node the new end node.

        endNode = currentNodeSelected;
        endNode.ChangeNodeColor(Color.red);
    }

    private void MakeFloorNode()
    {
        currentNodeSelected.ToggleWall(false); //Makes the selected node a floor tile.

        //if the selected node is the start or end then, set them to null to turn them into normal floor tiles.

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
        currentNodeSelected.ToggleWall(true); //Makes the selected node a wall tile.

        //if the selected node is the start or end then, set them to null to turn them into walls, as they don't have connections.

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
