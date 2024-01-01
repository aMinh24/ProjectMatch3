using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public int borderSize;

    public GameObject tilePrefab;
    public GameObject[] gamePiecePrefabs;

    private Tile[,] m_AllTiles;
    GameObject[,] m_AllGamePieces;
    void Start()
    {
        m_AllTiles = new Tile[width, height];
        m_AllGamePieces = new GameObject[width, height];
        SetupTiles();
        SetupCamera();
        FillRandom();
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
    private GameObject GetRandomPiece()
    {
        int randomIndx = Random.Range(0,gamePiecePrefabs.Length);
        return gamePiecePrefabs[randomIndx];
    }
    private void PlaceGamePiece(GamePiece gamePiece,int x,int y)
    {
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        gamePiece.SetCoord(x, y);
    }
    private void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomPiece(), Vector3.zero, Quaternion.identity);
                PlaceGamePiece(randomPiece.GetComponent<GamePiece>(),i,j);
            }
        }
    }
}
