using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class NodeObject : MonoBehaviour
{
    public MazeSpace node {  get; private set; }

    private SpriteRenderer sprite; //We create a reference for the sprite of the object so we can change its color to differentiate what type of node it is.

    public bool IsWall {  get; private set; }

    private TDAGraphGeneric<MazeSpace> graph;

    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomPoint;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>(); 
    }

    public void Initialize(int row, int column, TDAGraphGeneric<MazeSpace> graph) //This function needs to be called when instantiating this objects prefab.
    {
        node = new MazeSpace(row, column); //We initialize the node and pass it its row & column.
        this.graph = graph; //We also give it the graph that it will use so that all nodes have access to the same one.

        graph.AddVertex(node); //We add the newly created node to the list of nodes in the graph.

        ToggleWall(true); //All nodes default to walls.
    }

    public void ChangeNodeColor(Color color) //Gets call to change the color of the node.
    {
        sprite.color = color;
    }

    public void ToggleWall(bool input) //Changes the node from a wall to a floor tile.
    {
        IsWall = input;

        if (input)
        {
            ChangeNodeColor(Color.black);
            RemoveAllConnections(); //If it becomes a wall then we remove all conections so that it can not be walked through.
        }
        else
        {
            ChangeNodeColor(Color.white);
            CreateConnections(); //We create connections with all non wall nodes in the cardinal directions.
            ReactivateConnections(); //We also tell those nodes to create connections to us so we have a bidirectional edge.
        }
    }

    private void Update()
    {
        //Debug.DrawRay(topPoint.position, Vector2.up * 0.3f, Color.green);
        //Debug.DrawRay(bottomPoint.position, Vector2.down * 0.3f, Color.green);
        //Debug.DrawRay(leftPoint.position, Vector2.left * 0.3f, Color.green);
        //Debug.DrawRay(rightPoint.position, Vector2.right * 0.3f, Color.green);
    }

    public void RemoveAllConnections() //Searches the neighbouring nodes to sever their connections with this nodes. 
    {
        RaycastHit2D topNode = Physics2D.Raycast(topPoint.position, Vector2.up, 0.3f);

        if (topNode.collider != null) 
        {
            SeverLink(topNode);
        } 


        RaycastHit2D bottomNode = Physics2D.Raycast(bottomPoint.position, Vector2.down, 0.3f);

        if (bottomNode.collider != null)
        {
            SeverLink(bottomNode);
        }


        RaycastHit2D rightNode = Physics2D.Raycast(rightPoint.position, Vector2.right, 0.3f);

        if (rightNode.collider != null)
        {
            SeverLink(rightNode);
        }


        RaycastHit2D leftNode = Physics2D.Raycast(leftPoint.position, Vector2.left, 0.3f);

        if (leftNode.collider != null)
        {
            SeverLink(leftNode);
        }
    }

    public void SeverLink(RaycastHit2D nodeHit) //Deletes the the connection that this node has to the other, and the one the other has to this one. 
    {
        RemoveConnectionTo(RaycastNodeDetection(nodeHit).node);
        RaycastNodeDetection(nodeHit).RemoveConnectionTo(this.node);
    }

    public void RemoveConnectionTo(MazeSpace node)
    {
        graph.RemoveEdge(this.node, node);
    }

    public void CreateConnections() //Creates a conection to the nodes in the vicinity, as long as they aren't wall tiles.
    {
        RaycastHit2D topNode = Physics2D.Raycast(topPoint.position, Vector2.up, 0.3f);

        if (topNode.collider != null)
        {
            if (!RaycastNodeDetection(topNode).IsWall)
            {
                graph.AddEdge(node, RaycastNodeDetection(topNode).node, 0);
            }
        }


        RaycastHit2D bottomNode = Physics2D.Raycast(bottomPoint.position, Vector2.down, 0.3f);

        if (bottomNode.collider != null)
        {
            if (!RaycastNodeDetection(bottomNode).IsWall)
            {
                graph.AddEdge(node, RaycastNodeDetection(bottomNode).node, 0);
            }
        }


        RaycastHit2D rightNode = Physics2D.Raycast(rightPoint.position, Vector2.right, 0.3f);

        if (rightNode.collider != null)
        {
            if (!RaycastNodeDetection(rightNode).IsWall)
            {
                graph.AddEdge(node, RaycastNodeDetection(rightNode).node, 0);
            }
        }


        RaycastHit2D leftNode = Physics2D.Raycast(leftPoint.position, Vector2.left, 0.3f);

        if (leftNode.collider != null)
        {
            if (!RaycastNodeDetection(leftNode).IsWall)
            {
                graph.AddEdge(node, RaycastNodeDetection(leftNode).node, 0);
            }
        }
    }

    private void ReactivateConnections() //Tells the nodes in the vicinity, except walls, to create a conection from themselves to this node.
    {
        RaycastHit2D topNode = Physics2D.Raycast(topPoint.position, Vector2.up, 0.3f);

        if (topNode.collider != null) 
        {
            if (!RaycastNodeDetection(topNode).IsWall)
            {
                graph.AddEdge(RaycastNodeDetection(topNode).node, node, 0);
            }
        }


        RaycastHit2D bottomNode = Physics2D.Raycast(bottomPoint.position, Vector2.down, 0.3f);

        if (bottomNode.collider != null)
        {
            if (!RaycastNodeDetection(bottomNode).IsWall)
            {
                graph.AddEdge(RaycastNodeDetection(bottomNode).node, node, 0);
            }
        }


        RaycastHit2D rightNode = Physics2D.Raycast(rightPoint.position, Vector2.right, 0.3f);

        if (rightNode.collider != null)
        {
            if (!RaycastNodeDetection(rightNode).IsWall)
            {
                graph.AddEdge(RaycastNodeDetection(rightNode).node, node, 0);
            }
        }


        RaycastHit2D leftNode = Physics2D.Raycast(leftPoint.position, Vector2.left, 0.3f);

        if (leftNode.collider != null)
        {
            if (!RaycastNodeDetection(leftNode).IsWall)
            {
                graph.AddEdge(RaycastNodeDetection(leftNode).node, node, 0);
            }
        }
    }

    private NodeObject RaycastNodeDetection(RaycastHit2D hit) //Gets the NodeObject script from a raycastHit.
    {
        NodeObject space = hit.collider.GetComponent<NodeObject>();

        return space;
    }

}
