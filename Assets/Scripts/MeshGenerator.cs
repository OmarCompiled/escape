using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using NUnit.Framework.Internal;
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
//
// Another Very Important NOTE: Unity uses a clock-wise winding order.
// Not conventional (Like OpenGL).
public class MeshGenerator : MonoBehaviour  
{
    public SquareGrid                       m_SquareGrid;

    private List<Vector3>                   m_VerticesList;
    private List<int>                       m_TrianglesList;
    private Dictionary<int, List<Triangle>> m_TriangleMap;
    private List<List<int>>                 m_OutlinesList;
    private HashSet<int>                    m_CheckedVerticesSet; // For optimization

    [HideInInspector]
    public Mesh                             m_MapMesh;
    [HideInInspector]
    public Mesh                             m_WallMesh;

    public void GenerateMesh(int[,] map, float squareSize, float wallHeight)
    {
        m_SquareGrid            = new SquareGrid(map, squareSize);
        m_VerticesList          = new List<Vector3>();
        m_TrianglesList         = new List<int>();
        m_TriangleMap           = new Dictionary<int, List<Triangle>>();
        m_OutlinesList          = new List<List<int>>();
        m_CheckedVerticesSet    = new HashSet<int>();

        foreach(Square square in m_SquareGrid.m_Squares)
        {
            TriangulateSquare(square);
        }

        m_MapMesh               = new Mesh();
        m_MapMesh.vertices      = m_VerticesList.ToArray();
        m_MapMesh.triangles     = m_TrianglesList.ToArray();
        m_MapMesh.RecalculateNormals();

        GenerateWallMesh(wallHeight);
    }

    void GenerateWallMesh(float wallHeight)
    {
        CreateMeshOutlines(); 
        List<Vector3> wallVerticesList  = new List<Vector3>();
        List<int> wallTrianglesList     = new List<int>();
        m_WallMesh                      = new Mesh();

        foreach(List<int> outline in m_OutlinesList)
        {
            for(int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVerticesList.Count;
                wallVerticesList.Add(m_VerticesList[outline[i]]); // top left
                wallVerticesList.Add(m_VerticesList[outline[i+1]]); // top right
                wallVerticesList.Add(m_VerticesList[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVerticesList.Add(m_VerticesList[outline[i+1]] - Vector3.up * wallHeight); // bottom right

                wallTrianglesList.Add(startIndex);
                wallTrianglesList.Add(startIndex + 2);
                wallTrianglesList.Add(startIndex + 3);

                wallTrianglesList.Add(startIndex + 3);
                wallTrianglesList.Add(startIndex + 1);
                wallTrianglesList.Add(startIndex);
            }
        }

        m_WallMesh.vertices   = wallVerticesList.ToArray();
        m_WallMesh.triangles  = wallTrianglesList.ToArray();
        m_WallMesh.RecalculateNormals();
    }

    void TriangulateSquare(Square square)
    {
        // NOTE: Unity has a --clock-wise-- winding order, unlike OpenGL.
        switch (square.m_Configuration) {
		case 0:
			break;
		// 1 points:
		case 1:
			AddSubMeshVertices(square.m_CenterLeft, square.m_CenterBottom, square.m_BottomLeft);
			break;
		case 2:
			AddSubMeshVertices(square.m_BottomRight, square.m_CenterBottom, square.m_CenterRight);
			break;
		case 4:
			AddSubMeshVertices(square.m_TopRight, square.m_CenterRight, square.m_CenterTop);
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
            // No edge nodes are of case 15; They might be quads but not of case 15
            m_CheckedVerticesSet.Add(square.m_TopLeft.m_VertexIndex);
            m_CheckedVerticesSet.Add(square.m_TopRight.m_VertexIndex);
            m_CheckedVerticesSet.Add(square.m_BottomRight.m_VertexIndex);
            m_CheckedVerticesSet.Add(square.m_BottomLeft.m_VertexIndex);            
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
                vertices[i].m_VertexIndex = m_VerticesList.Count;
                m_VerticesList.Add(vertices[i].m_Position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        m_TrianglesList.Add(a.m_VertexIndex);
        m_TrianglesList.Add(b.m_VertexIndex);
        m_TrianglesList.Add(c.m_VertexIndex);

        Triangle triangle = new Triangle(a.m_VertexIndex, b.m_VertexIndex, c.m_VertexIndex);
        AddTriangleToMap(triangle.m_VertexA, triangle);
        AddTriangleToMap(triangle.m_VertexB, triangle);
        AddTriangleToMap(triangle.m_VertexC, triangle);
    }

    void AddTriangleToMap(int vertexIndex, Triangle triangle)
    {
        if(m_TriangleMap.ContainsKey(vertexIndex))
        {
            m_TriangleMap[vertexIndex].Add(triangle);
        } else
        {
            List<Triangle> newList = new List<Triangle>();
            newList.Add(triangle);
            m_TriangleMap.Add(vertexIndex, newList);
        }
    }

    bool AreOutlineEdge(int vertexIndexA, int vertexIndexB)
    {
        // Two vertices form an outline edge iff they share exactly 1 triangle.
        List<Triangle>  trianglesContainingA = m_TriangleMap[vertexIndexA];
        int sharedTriangles = 0;
        foreach(Triangle triangle in trianglesContainingA)
        {
            if(triangle.ContainsVertex(vertexIndexB))
            {
                sharedTriangles++;
            }
            if(sharedTriangles > 1) break;
        }
        return sharedTriangles == 1;
    }

    void CreateMeshOutlines()
    {
        for(int vertexIndex = 0; vertexIndex < m_VerticesList.Count; vertexIndex++)
        {
            if(!m_CheckedVerticesSet.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if(newOutlineVertex != -1)
                {
                    m_CheckedVerticesSet.Add(vertexIndex);
                    List<int> newOutlineList = new List<int>();
                    m_OutlinesList.Add(newOutlineList);
                    FollowOutlineFromVertex(vertexIndex, m_OutlinesList.Count - 1);
                    m_OutlinesList[m_OutlinesList.Count - 1].Add(vertexIndex); // making the outline loop back to connect.
                }
            }
        }
    }

    void FollowOutlineFromVertex(int vertexIndex, int outlineIndex)
    {
        m_OutlinesList[outlineIndex].Add(vertexIndex);
        m_CheckedVerticesSet.Add(vertexIndex);
        int newVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if(newVertexIndex != -1)
        {
            FollowOutlineFromVertex(newVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = m_TriangleMap[vertexIndex];

        for(int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for(int j = 0; j < 3; j++)
            {
                if(AreOutlineEdge(vertexIndex, triangle[j]) && triangle[j] != vertexIndex && !m_CheckedVerticesSet.Contains(triangle[j]))
                {
                    return triangle[j];
                }
            }
        }
        return -1; // On failure obviously
    }


    struct Triangle
    {
        public int m_VertexA;
        public int m_VertexB;
        public int m_VertexC;
        private int[] vertices;
        public Triangle(int a, int b, int c)
        {
            (m_VertexA, m_VertexB, m_VertexC) = (a, b, c);
            vertices = new int[3] {a, b, c};
        }

        public bool ContainsVertex(int vertexIndex)
        {
            return vertexIndex == m_VertexA || vertexIndex == m_VertexB || vertexIndex == m_VertexC;
        }

        public int this[int index]
        {
            get => vertices[index];
            set => vertices[index] = value;
        }
    }

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
                    m_Squares[x,y] = new Square(controlNodes[x,y+1], controlNodes[x+1,y+1], controlNodes[x+1,y], controlNodes[x,y]);
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

        public int m_Configuration = 0; 

        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            (m_TopLeft, m_TopRight, m_BottomRight, m_BottomLeft) = (topLeft, topRight, bottomRight, bottomLeft);

            m_CenterTop       = m_TopLeft.m_Right; // Caused Bug... 
            m_CenterLeft      = m_BottomLeft.m_Above;
            m_CenterBottom    = m_BottomLeft.m_Right; 
            m_CenterRight     = m_BottomRight.m_Above;

            if(m_TopLeft.m_Active) m_Configuration += 8;
            if(m_TopRight.m_Active) m_Configuration += 4;
            if(m_BottomRight.m_Active) m_Configuration += 2;
            if(m_BottomLeft.m_Active) m_Configuration += 1;
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
