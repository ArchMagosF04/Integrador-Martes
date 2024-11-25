using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

public class NodeObject : MonoBehaviour
{
    public MazeSpace node {  get; private set; }

    private SpriteRenderer sprite;

    public bool IsWall {  get; private set; }

    private TDAGraphGeneric<MazeSpace> graph;

    [SerializeField] private Transform topPoint;
    [SerializeField] private Transform bottomPoint;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;

    [SerializeField] private Vector2 position;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void Initialize(int row, int column, TDAGraphGeneric<MazeSpace> graph)
    {
        node = new MazeSpace(row, column);
        position = new Vector2(row, column);
        this.graph = graph;

        graph.AddVertex(node);

        ToggleWall(true);
    }

    public void ChangeNodeColor(Color color)
    {
        sprite.color = color;
    }

    public void ToggleWall(bool input)
    {
        IsWall = input;

        if (input)
        {
            ChangeNodeColor(Color.black);
            RemoveAllConnections();
        }
        else
        {
            ChangeNodeColor(Color.white);
            CreateConnections();
            ReactivateConnections();
        }
    }

    private void Update()
    {
        //Debug.DrawRay(topPoint.position, Vector2.up * 0.3f, Color.green);
        //Debug.DrawRay(bottomPoint.position, Vector2.down * 0.3f, Color.green);
        //Debug.DrawRay(leftPoint.position, Vector2.left * 0.3f, Color.green);
        //Debug.DrawRay(rightPoint.position, Vector2.right * 0.3f, Color.green);
    }

    public void RemoveAllConnections()
    {
        RaycastHit2D topNode = Physics2D.Raycast(topPoint.position, Vector2.up, 0.3f);

        if (topNode.collider != null) 
        {
            RemoveConnectionTo(RaycastNodeDetection(topNode).node);
            RaycastNodeDetection(topNode).RemoveConnectionTo(this.node);
        } 


        RaycastHit2D bottomNode = Physics2D.Raycast(bottomPoint.position, Vector2.down, 0.3f);

        if (bottomNode.collider != null)
        {
            RemoveConnectionTo(RaycastNodeDetection(bottomNode).node);
            RaycastNodeDetection(bottomNode).RemoveConnectionTo(this.node);
        }


        RaycastHit2D rightNode = Physics2D.Raycast(rightPoint.position, Vector2.right, 0.3f);

        if (rightNode.collider != null)
        {
            RemoveConnectionTo(RaycastNodeDetection(rightNode).node);
            RaycastNodeDetection(rightNode).RemoveConnectionTo(this.node);
        }


        RaycastHit2D leftNode = Physics2D.Raycast(leftPoint.position, Vector2.left, 0.3f);

        if (leftNode.collider != null)
        {
            RemoveConnectionTo(RaycastNodeDetection(leftNode).node);
            RaycastNodeDetection(leftNode).RemoveConnectionTo(this.node);
        }
    }

    public void RemoveConnectionTo(MazeSpace node)
    {
        graph.RemoveEdge(this.node, node);
    }

    public void CreateConnections()
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

    private void ReactivateConnections()
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

    private NodeObject RaycastNodeDetection(RaycastHit2D hit)
    {
        NodeObject space = hit.collider.GetComponent<NodeObject>();

        return space;
    }

}
