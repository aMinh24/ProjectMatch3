using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex;
    public int yIndex;
    public InterType interpolation = InterType.SmootherStep;
    private Board m_board;
    private bool isMoving = false;
    public enum InterType
    {
        Linear,
        EaseIn,
        EaseOut,
        SmoothStep,
        SmootherStep
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            Move(xIndex + 1, yIndex, 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(xIndex - 1, yIndex, 0.5f);
        }
    }
    public void Init(Board b)
    {
        m_board = b;
    }
    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
    public void Move(int destX, int destY, float timeToMove)
    {
        if (isMoving) return;
        StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
    }
    private IEnumerator MoveRoutine(Vector3 dest, float timeToMove)
    {

        isMoving = true;
        Vector3 startPos = transform.position;
        float time = 0;
        while (true)
        {
            if (Vector3.Distance(transform.position, dest) < 0.01f)
            {

                m_board.PlaceGamePiece(this,(int)dest.x,(int)dest.y);
                break;
            }
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / timeToMove);
            switch (interpolation)
            {
                case (InterType.Linear):
                    break;
                case InterType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;
                case InterType.EaseIn:
                    t = Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;
                case InterType.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;
                case InterType.SmootherStep:
                    t = t * t * t * (t*(t * 6 - 15) + 10);
                    break;

            }
            transform.position = Vector3.Lerp(startPos, dest, t);
            yield return null;
        }
        isMoving = false;
    }
}
