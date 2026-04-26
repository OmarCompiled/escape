using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

// Marching squares algorithm
//
// 0--o--0
// |     |
// o     o
// |     |
// 0--o--0
//
// For an in-depth explanation,
// check the wiki page : https://en.wikipedia.org/wiki/Marching_squares
// or Sebastian Lague's video.
//
// NOTE: Isovalue = 1; considering our map; binary; 1 = wall, 0 = empty;
public class MeshGenerator : MonoBehaviour
{
    public SquareGrid m_SquareGrid;

    private List<Vector3> m_Vertices;
    private List<int> m_Triangles;
    private Mesh m_Mesh;

    public Mesh GenerateMesh(int[,] map, float squareSize)
    {
        m_SquareGrid = new SquareGrid(map, squareSize);
        m_Vertices = new List<Vector3>();
        m_Triangles = new List<int>();
        foreach(Square square in m_SquareGrid.m_Squares)
        {
            TriangulateSquare(square);
        }

        m_Mesh = new Mesh();
        m_Mesh.vertices = m_Vertices.ToArray();
        m_Mesh.triangles = m_Triangles.ToArray();
        m_Mesh.RecalculateNormals();
        return m_Mesh;
    }

    void TriangulateSquare(Square square)
    {
        // NOTE: Unity has a --clock-wise-- winding order, unlike OpenGL.
        switch (square.m_Configuration) {
		case 0:
			break;

		// 1 points:
		case 1:
			AddSubMeshVertices(square.m_CenterBottom, square.m_BottomLeft, square.m_CenterLeft);
			break;
		case 2:
			AddSubMeshVertices(square.m_CenterRight, square.m_BottomRight, square.m_CenterBottom);
			break;
		case 4:
			AddSubMeshVertices(square.m_CenterTop, square.m_TopRight, square.m_CenterRight);
			break;
		case 8:
			AddSubMeshVertices(square.m_TopLeft, square.m_CenterTop, square.m_CenterLeft);
			break;

		// 2 points:
		case 3:
			AddSubMeshVertices(square.m_CenterRight, square.m_BottomRight, square.m_BottomLeft, square.m_CenterLeft);
			break;
		case 6:
			AddSubMeshVertices(square.m_CenterTop, square.m_TopRight, square.m_BottomRight, square.m_CenterBottom);
			break;
		case 9:
			AddSubMeshVertices(square.m_TopLeft, square.m_CenterTop, square.m_CenterBottom, square.m_BottomLeft);
			break;
		case 12:
			AddSubMeshVertices(square.m_TopLeft, square.m_TopRight, square.m_CenterRight, square.m_CenterLeft);
			break;
		case 5:
			AddSubMeshVertices(square.m_CenterTop, square.m_TopRight, square.m_CenterRight, square.m_CenterBottom, square.m_BottomLeft, square.m_CenterLeft);
			break;
		case 10:
			AddSubMeshVertices(square.m_TopLeft, square.m_CenterTop, square.m_CenterRight, square.m_BottomRight, square.m_CenterBottom, square.m_CenterLeft);
			break;

		// 3 point:
		case 7:
			AddSubMeshVertices(square.m_CenterTop, square.m_TopRight, square.m_BottomRight, square.m_BottomLeft, square.m_CenterLeft);
			break;
		case 11:
			AddSubMeshVertices(square.m_TopLeft, square.m_CenterTop, square.m_CenterRight, square.m_BottomRight, square.m_BottomLeft);
			break;
		case 13:
			AddSubMeshVertices(square.m_TopLeft, square.m_TopRight, square.m_CenterRight, square.m_CenterBottom, square.m_BottomLeft);
			break;
		case 14:
			AddSubMeshVertices(square.m_TopLeft, square.m_TopRight, square.m_BottomRight, square.m_CenterBottom, square.m_CenterLeft);
			break;

		// 4 point:
		case 15:
			AddSubMeshVertices(square.m_TopLeft, square.m_TopRight, square.m_BottomRight, square.m_BottomLeft);
			break;
		}
    }

    void AddSubMeshVertices(params Node[] vertices)
    {
        AssignVertexIndex(vertices);
        if (vertices.Length >= 3)
			CreateTriangle(vertices[0], vertices[1], vertices[2]);
		if (vertices.Length >= 4)
			CreateTriangle(vertices[0], vertices[2], vertices[3]);
		if (vertices.Length >= 5) 
			CreateTriangle(vertices[0], vertices[3], vertices[4]);
		if (vertices.Length >= 6)
			CreateTriangle(vertices[0], vertices[4], vertices[5]);
    }

    void AssignVertexIndex(Node[] vertices)
    {
        for(int i = 0; i < vertices.Length; i++)
        {
            if(vertices[i].m_VertexIndex == -1)
            {
                vertices[i].m_VertexIndex = m_Vertices.Count;
                m_Vertices.Add(vertices[i].m_Position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        m_Triangles.Add(a.m_VertexIndex);
        m_Triangles.Add(b.m_VertexIndex);
        m_Triangles.Add(c.m_VertexIndex);
    }

    // void OnDrawGizmos()
    // {
    //     if(m_SquareGrid != null)
    //     {
    //         for(int x = 0; x < m_SquareGrid.m_Squares.GetLength(0); x++)
    //         {
    //             for(int y = 0; y < m_SquareGrid.m_Squares.GetLength(1); y++)
    //             {
    //                 Gizmos.color = m_SquareGrid.m_Squares[x,y].m_TopLeft.m_Active ? Color.black : Color.white;
    //                 Gizmos.DrawCube(m_SquareGrid.m_Squares[x,y].m_TopLeft.m_Position, Vector3.one * 0.4f);

    //                 Gizmos.color = m_SquareGrid.m_Squares[x,y].m_TopRight.m_Active ? Color.black : Color.white;
    //                 Gizmos.DrawCube(m_SquareGrid.m_Squares[x,y].m_TopRight.m_Position, Vector3.one * 0.4f);

    //                 Gizmos.color = m_SquareGrid.m_Squares[x,y].m_BottomLeft.m_Active ? Color.black : Color.white;
    //                 Gizmos.DrawCube(m_SquareGrid.m_Squares[x,y].m_BottomLeft.m_Position, Vector3.one * 0.4f);

    //                 Gizmos.color = m_SquareGrid.m_Squares[x,y].m_BottomRight.m_Active ? Color.black : Color.white;
    //                 Gizmos.DrawCube(m_SquareGrid.m_Squares[x,y].m_BottomRight.m_Position, Vector3.one * 0.4f);

    //                 Gizmos.color = Color.grey;
    //                 Gizmos.DrawCube(m_SquareGrid.m_Squares[x,y].m_CenterTop.m_Position, Vector3.one * 0.2f);
    //                 Gizmos.DrawCube(m_SquareGrid.m_Squares[x,y].m_CenterLeft.m_Position, Vector3.one * 0.2f);
    //                 Gizmos.DrawCube(m_SquareGrid.m_Squares[x,y].m_CenterBottom.m_Position, Vector3.one * 0.2f);
    //                 Gizmos.DrawCube(m_SquareGrid.m_Squares[x,y].m_CenterRight.m_Position, Vector3.one * 0.2f);
    //             }
    //         }
    //     }
    // }

    public class SquareGrid
    {
        public Square[,] m_Squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX  = map.GetLength(0);
            int nodeCountY  = map.GetLength(1);
            float mapWidth  = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
            for(int x = 0; x < nodeCountX; x++)
            {
                for(int y = 0; y < nodeCountY; y++)
                {
                    Vector3 nodePosition = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapHeight/2 + y * squareSize + squareSize/2);
                    controlNodes[x,y] = new ControlNode(nodePosition, map[x,y] == 1, squareSize);
                }
            }

            m_Squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for(int x = 0; x < nodeCountX - 1; x++)
            {
                for(int y = 0; y < nodeCountY - 1; y++)
                {
                    // NOTE: Cartesian Coordinates in use, Not screen coordinates :/ ; 0,0 at bottom left;
                    m_Squares[x,y] = new Square(controlNodes[x+1,y+1], controlNodes[x,y+1], controlNodes[x+1,y], controlNodes[x,y]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode m_TopRight, m_TopLeft;
        public ControlNode m_BottomRight, m_BottomLeft;

        public Node m_CenterRight, m_CenterLeft;
        public Node m_CenterTop, m_CenterBottom;

        public int m_Configuration; 

        public Square(ControlNode topRight, ControlNode topLeft, ControlNode bottomRight, ControlNode bottomLeft)
        {
            (m_TopRight, m_TopLeft, m_BottomRight, m_BottomLeft) = (topRight, topLeft, bottomRight, bottomLeft);

            m_CenterTop       = m_TopRight.m_Right;
            m_CenterLeft      = m_BottomLeft.m_Above;
            m_CenterBottom    = m_BottomLeft.m_Right;
            m_CenterRight     = m_BottomRight.m_Above;

            int bit_0 = m_TopLeft.m_Active ? 1 : 0;
            int bit_1 = m_TopRight.m_Active ? 1 : 0;
            int bit_2 = m_BottomRight.m_Active ? 1 : 0;
            int bit_3 = m_BottomLeft.m_Active ? 1 : 0;
            string nodeBits = bit_0.ToString() + bit_1.ToString() + bit_2.ToString() + bit_3.ToString();   
            m_Configuration = Convert.ToInt32(nodeBits, 2);
        }
    }

    public class Node
    {
        public Vector3 m_Position;
        public int m_VertexIndex = -1; // -1 since we don't know the index yet.

        public Node(Vector3 position)
        {
            m_Position = position;
        }
    }

    public class ControlNode : Node
    {
        public bool m_Active;
        public Node m_Above;
        public Node m_Right;

        public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
        {
            m_Active = active;
            m_Above = new Node(position + (Vector3.forward * squareSize / 2.0f)); // essentially midpoints between control nodes.
            m_Right = new Node(position + (Vector3.right * squareSize / 2.0f));
        }
    }
}
