using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] GameObject GameOverMenu;
    public void GameOver()
    {
        GameOverMenu.SetActive(true);
    }
}
