using System;
using Unity.VisualScripting;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
	public int 		m_Width = 160;
	public int 		m_Height = 90;
	public float 	m_WallHeight = 5;

	public string 	m_Seed;
	public bool 	m_UseRandomSeed = false;

	[SerializeField, Range(0, 100)]
	private int m_FillPercentage = 48;

	[SerializeField, Range(0, 25)]
	private int m_SmoothCount = 15;

	private int[,] 			m_Map;
	private MeshGenerator 	m_MeshGenerator;
	private MeshFilter 		m_MapMeshFilter;
	private MeshFilter		m_WallMeshFilter;
	private System.Random 	m_PseudoRNG;

	void Start()
	{
		m_Seed 				= "NULL";
		m_MeshGenerator		= GetComponent<MeshGenerator>();
		m_MapMeshFilter 	= GameObject.Find("Map").AddComponent<MeshFilter>();
		m_WallMeshFilter	= GameObject.Find("Walls").AddComponent<MeshFilter>(); 
		GenerateMap();
	}

	void Update()
	{
		if(Input.GetMouseButtonDown(0))
		{
			GenerateMap();
		}
	}
	
	// Cellular Automata; much like Conway's game of life :)
	// The behaviour depends on the set of rules used.
    void GenerateMap()
	{
		m_Map = new int[m_Width, m_Height];
		FillMap();
		SmoothMap();
		// some Mesh = GenerateMesh();
		m_MeshGenerator?.GenerateMesh(m_Map, 1.0f, m_WallHeight);
		m_MapMeshFilter.mesh 	= m_MeshGenerator.m_MapMesh;
		m_WallMeshFilter.mesh	= m_MeshGenerator.m_WallMesh;
	}

	void FillMap()
	{	
		if(m_UseRandomSeed) m_Seed = (Time.time + Time.deltaTime).ToString();
		m_PseudoRNG = new System.Random(m_Seed.GetHashCode()); // Null coalescing will cause unwanted behaviour.
		Debug.Log(m_Seed);

		for(int x = 0; x < m_Width; x++) {
			for(int y = 0; y < m_Height; y++) {
				if(x == 0 || x == m_Width - 1 || y == 0 || y == m_Height - 1)
				{
					m_Map[x,y] = 1;
				} else
				{
					m_Map[x,y] = (m_PseudoRNG.Next(0, 100) < m_FillPercentage) ? 1 : 0;
				}
			}
		}        
	}

	void SmoothMap()
	{
		for(int i = 0; i < m_SmoothCount; i++)
		{
			for(int x = 0; x < m_Width; x++)
			{
				for(int y = 0; y < m_Height; y++)
				{
					int neighbourWallCount = GetNeighbourWallCount(x, y);
					if(neighbourWallCount > 4)
					{
						m_Map[x,y] = 1;
					} 
					else if(neighbourWallCount < 4)
					{
						m_Map[x,y] = 0;
					}
				}
			}
		}
	}

	int GetNeighbourWallCount(int gridX, int gridY)
	{
		int neighbourWallCount = 0;
		for(int x = gridX - 1; x <= gridX + 1; x++)
		{
			for(int y = gridY - 1; y <= gridY + 1; y++)
			{
				if((x >= 0) && (x < m_Width-1) && (y >= 0) && (y < m_Height-1))
				{
					if(x != gridX || y != gridY)
					{
						neighbourWallCount += m_Map[x,y];
					}
				} 
				else
				{
					neighbourWallCount++;
				}
			}
		}
		return neighbourWallCount;
	}
}
