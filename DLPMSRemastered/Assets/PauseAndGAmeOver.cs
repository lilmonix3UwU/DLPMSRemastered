using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseAndGameOver : MonoBehaviour
{
    public bool GameOver;
    [SerializeField] bool Paused;
    private InputManager _input;

    private void Start()
    {
        GameOver = false;
        Paused = false;
        _input = InputManager.Instance;
    }

    void Update()
    {
        if (_input.PressPause())
        {
            
        }
    }
}
