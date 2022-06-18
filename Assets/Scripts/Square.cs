using System;
using System.Collections;
using System.Collections.Generic;
using Constants;
using UnityEngine;

public class Square : MonoBehaviour
{
    private SpriteRenderer sr;

    private static Vector3 startScale = Vector3.one * 0.7f;
    private static Vector3 endScale = Vector3.one;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void InitSquare(int colorValue)
    {
        Color color = GameManager.instance.colors[colorValue];
        sr.color = color;
        sr.sortingOrder = VALUES.FRONT;
        StartCoroutine(StartAnimation());
    }

    public IEnumerator StartAnimation()
    {
        transform.localScale = startScale;
        float animationTime = VALUES.ANIMATION_TIME;
        Vector3 speed = (endScale - startScale) / animationTime;
        float endTime = 0f;
        while (endTime < animationTime)
        {
            transform.localScale += speed * Time.deltaTime;
            endTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
    }
}