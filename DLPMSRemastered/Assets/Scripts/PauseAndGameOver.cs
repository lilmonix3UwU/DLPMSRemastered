using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseAndGameOver : MonoBehaviour
{
    public int health;
    public bool GameOver;
    [SerializeField] bool Paused;
    private InputManager _input;
    [SerializeField] GameObject PauseMenu;
    [SerializeField] GameObject GameOverMenu;

    private void Start()
    {
        GameOver = false;
        Paused = false;
        _input = InputManager.Instance;
        health = 5;
    }

    void Update()
    {
        if (_input.PressPause() && !GameOver)
        {
            StartCoroutine("Pauser");
        }
        if (health <= 0)
        {
            GameOver = true;
            Time.timeScale = 0;
            GameOverMenu.SetActive(true);
        }
    }


    private void Pauser()
    {
        if (Paused)
        {
            Time.timeScale = 1.0f;
            Paused = false;
            PauseMenu.SetActive(Paused);
        }
        else
        {
            Time.timeScale = 0;
            Paused = true;
            PauseMenu.SetActive(Paused);
        }
    }
}
