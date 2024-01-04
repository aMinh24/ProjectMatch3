using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;

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
        //HighlightMatches();
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
        if (m_clickedTile != null && IsNextTo(m_clickedTile, tile))
        {
            m_targetTile = tile;
        }
    }
    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTile(m_clickedTile,m_targetTile);
        }
        m_clickedTile = null;
        m_targetTile = null;
    }
    void SwitchTile(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTileRoutine(clickedTile, targetTile));
    }
    IEnumerator SwitchTileRoutine(Tile clickedTile,Tile targetTile)
    {
        GamePiece clickPiece = m_AllGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = m_AllGamePieces[targetTile.xIndex, targetTile.yIndex];
        if (clickPiece != null && targetPiece != null)
        {
            clickPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
            yield return new WaitForSeconds(swapTime);

            List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

            if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
            {
                clickPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                yield return new WaitForSeconds(swapTime);
            }

            HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            HighlightMatchesAt(targetTile.xIndex,targetTile.yIndex);
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
            nextX = startX + (int)searchDir.x * i;
            nextY = startY + (int)searchDir.y * i;
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
        if (matches.Count >= minLength)
        {
            return matches;
        }
        return null;
    }
    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 2)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);
        if (upwardMatches == null) upwardMatches = new List<GamePiece>();
        if (downwardMatches == null) downwardMatches = new List<GamePiece>();
        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        return combinedMatches.Count >= minLength ? combinedMatches : null;
    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 2)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);
        if (rightMatches == null) rightMatches = new List<GamePiece>();
        if (leftMatches == null) leftMatches = new List<GamePiece>();
        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        return combinedMatches.Count >= minLength ? combinedMatches : null;
    }
    private void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    private void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);
        List<GamePiece> combinedMatches = FindMatchesAt(x, y);
        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private void HighlightTileOn(int x, int y, Color col)
    {
        SpriteRenderer sprite = m_AllTiles[x, y].GetComponent<SpriteRenderer>();
        sprite.color = col;
    }
    private void HighlightTileOff(int x, int y)
    {
        SpriteRenderer sprite = m_AllTiles[x, y].GetComponent<SpriteRenderer>();
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
    }

    private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(x, y, minLength);
        List<GamePiece> verticalMatches = FindVerticalMatches(x, y, minLength);
        if (horizontalMatches == null) horizontalMatches = new List<GamePiece>();
        if (verticalMatches == null) verticalMatches = new List<GamePiece>();
        var combinedMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combinedMatches;
    }

}
