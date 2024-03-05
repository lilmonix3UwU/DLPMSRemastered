using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverFadeIn : MonoBehaviour
{
    [SerializeField] CanvasGroup canvas;
    [SerializeField] float fadeInSpeed;
    void FixedUpdate()
    {
        canvas.alpha += fadeInSpeed;
    }
    public void AlphaDown()
    {
        canvas.alpha = 0;
    }
}
