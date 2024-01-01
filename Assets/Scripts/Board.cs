using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public int borderSize;
    public GameObject tilePrefab;
    private Tile[,] m_AllTiles;
    void Start()
    {
        m_AllTiles = new Tile[width, height];
        SetupTiles();
        SetupCamera();
    }
    private void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(i, j, 0), Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = "Tile (" + i + "," + j + ")";
                m_AllTiles[i, j] = tile.GetComponent<Tile>();
                m_AllTiles[i, j].Init(i, j, this);
            }
        }
    }
    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width-1) / 2, (float)(height-1) / 2, -10f);
        float aspectRatio = Screen.width*1f / Screen.height;
        float verticalSize = height*1f/2 +borderSize;
        float horizontalSize = (width*1f/2 + borderSize) / aspectRatio;
        Camera.main.orthographicSize = verticalSize>horizontalSize?verticalSize:horizontalSize;
    }
}
