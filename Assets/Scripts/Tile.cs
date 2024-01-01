using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    Board m_board;
    void Start()
    {
        
    }
    public void Init(int x,int y, Board b)
    {
        xIndex = x;
        yIndex = y;
        m_board = b;
    }
}
