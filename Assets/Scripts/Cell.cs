using System;
using System.Collections;
using System.Collections.Generic;
using Constants;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public CellData cellData;

    public Vector2Int CurrentPos =>
        new Vector2Int((int)transform.position.x, (int)transform.position.y); 
    [SerializeField] private GameObject first, second;
    [SerializeField] private List<GameObject> circles;
    private Square frontSquare, backSquare;


    private void Awake()
    {
        cellData = new CellData();
        cellData.color = -1;
        cellData.moves = 0;
        cellData.gridPos = CurrentPos;
        
        first.SetActive(false);
        second.SetActive(false);
        frontSquare = first.GetComponent<Square>();
        backSquare = second.GetComponent<Square>();

        for (int i = 0; i < circles.Count; i++)
        {
            circles[i].SetActive(false);
        }
    }

    public void InitCell(CellData data)
    {
        cellData = data;
        frontSquare.gameObject.SetActive(true);
        frontSquare.InitSquare(cellData.color);
        StartCoroutine(UpdateMoves());
    }

    public IEnumerator UpdateMoves()
    {
        yield return new WaitForSeconds(VALUES.ANIMATION_TIME);

        for (int i = 0; i < circles.Count; i++)
        {
            circles[i].SetActive(i < cellData.moves);
        }
    }

    public IEnumerator MoveToPos()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(cellData.gridPos.x + 0.5f, cellData.gridPos.y + 0.5f);
        float animationTime = VALUES.ANIMATION_TIME;
        Vector3 speed = (endPos - startPos) / animationTime;
        float endTime = 0f;
        while (endTime < animationTime)
        {
            transform.position += speed * Time.deltaTime;
            endTime += Time.deltaTime;
            yield return null;
        }
    }

    public IEnumerator ChangeColor(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        backSquare.gameObject.SetActive(true);
        backSquare.StartColorSettings(cellData.color, true);
        frontSquare.StartColorSettings(cellData.color, false);

        var temp = frontSquare;
        frontSquare = backSquare;
        backSquare = temp;

        yield return frontSquare.StartAnimation();
        
        backSquare.gameObject.SetActive(false);
    }
}

[System.Serializable]
public struct CellData
{
    public int moves;
    public int color;
    public Vector2Int gridPos;
}