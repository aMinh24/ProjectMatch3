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
    public float swapTime;

    Tile m_clickedTile;
    Tile m_targetTile;

    public GameObject tilePrefab;
    public GameObject[] gamePiecePrefabs;

    private Tile[,] m_AllTiles;
    GamePiece[,] m_AllGamePieces;
    void Start()
    {
        m_AllTiles = new Tile[width, height];
        m_AllGamePieces = new GamePiece[width, height];
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
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2, (float)(height - 1) / 2, -10f);
        float aspectRatio = Screen.width * 1f / Screen.height;
        float verticalSize = height * 1f / 2 + borderSize;
        float horizontalSize = (width * 1f / 2 + borderSize) / aspectRatio;
        Camera.main.orthographicSize = verticalSize > horizontalSize ? verticalSize : horizontalSize;
    }
    private GameObject GetRandomPiece()
    {
        int randomIndx = Random.Range(0, gamePiecePrefabs.Length);
        return gamePiecePrefabs[randomIndx];
    }
    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
            m_AllGamePieces[x, y] = gamePiece;
        gamePiece.SetCoord(x, y);
    }
    bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
    private void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomPiece(), Vector3.zero, Quaternion.identity);

                randomPiece.GetComponent<GamePiece>().Init(this);
                PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
                randomPiece.transform.parent = transform;
            }
        }
    }
    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;
        }
    }
    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null)
        {
            m_targetTile = tile;
        }
    }
    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null && IsNextTo(m_clickedTile, m_targetTile))
        {
            GamePiece clickPiece = m_AllGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];
            GamePiece targetPiece = m_AllGamePieces[m_targetTile.xIndex, m_targetTile.yIndex];
            clickPiece.Move(m_targetTile.xIndex, m_targetTile.yIndex, swapTime);
            targetPiece.Move(m_clickedTile.xIndex, m_clickedTile.yIndex, swapTime);
            m_clickedTile = null;
            m_targetTile = null;
        }
    }
    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }
        return false;
    }
    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDir, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;
        if (IsWithinBounds(startX, startY))
        {
            startPiece = m_AllGamePieces[startX, startY];
            matches.Add(startPiece);
        }
        else return null;
        int nextX;
        int nextY;
        int maxValue = width > height ? width : height;
        for (int i = 1; i < maxValue; i++)
        {
            nextX = (int)searchDir.x * i;
            nextY = (int)searchDir.y * i;
            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }
            GamePiece nextPiece = m_AllGamePieces[nextX, nextY];
            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
            {
                matches.Add(nextPiece);
            }
            else break;
        }
        if(matches.Count>=  minLength) { return matches; }
        return null;
    }
}
