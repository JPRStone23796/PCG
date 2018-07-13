using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGrid : MonoBehaviour {

    public GameObject hexPrefab;

    public int gridWidth;
    public int gridHeight;
    public float hexWidth = 0.9486f;
    public float hexHeight = 1.096f;
    public float Hexgap = 0.0f;
    Vector3 startPos;
    public GameObject[,] HexGrid;

    /// <summary>
    /// Retreived from Cornelius Custard (2016).
    /// </summary>

    void Awake()
    {
        //set up gap values on each axis
        AddGap();
        //calculate the starting position of the grid
        CalcStartPos();
        InstantiateGrid();
    }

    void AddGap()
    {
        //calculate the width gap
        hexWidth += hexWidth * Hexgap;
        //calculate the height gap
        hexHeight += hexHeight * Hexgap;
        //set up the array of GameObjects
        HexGrid = new GameObject[gridWidth, gridHeight];
    }
    void CalcStartPos()
    {
        float offset = 0;
        if (gridHeight / 2 % 2 != 0)
            offset = hexWidth / 2;

        float x = -hexWidth * (gridWidth / 2) - offset;
        float z = hexHeight * 0.75f * (gridHeight / 2);

        startPos = new Vector3(x, 0, z);
    }

    Vector3 CalcWorldPos(Vector2 gridPos)
    {
        float offset = 0;
        if (gridPos.y % 2 != 0)
            offset = hexWidth / 2;

        float x = startPos.x + gridPos.x * hexWidth + offset;
        float z = startPos.z - gridPos.y * hexHeight * 0.75f;

        return new Vector3(x, 0, z);
    }

    void InstantiateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject hex = Instantiate(hexPrefab) ;
                Vector2 gridPos = new Vector2(x, y);
                hex.transform.position = CalcWorldPos(gridPos);
                hex.transform.parent = this.transform;
                hex.name =  y + "|" + x;
                HexGrid[x,y] = hex;
            }
        }
    }
}
