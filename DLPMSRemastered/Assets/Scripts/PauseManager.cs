using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;

    private bool _paused;

    private InputManager _input;
    private GameOverManager _gameOver;
    private AudioManager _audio;

    private void Start()
    {
        _gameOver = GameOverManager.Instance;
        _input = InputManager.Instance;
        _audio = AudioManager.Instance;

        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (_input.PressPause() && !_gameOver.gameIsOver)
        {
            Pauser();
        }
    }


    private void Pauser()
    {
        if (_paused)
        {
            Time.timeScale = 1.0f;
            _paused = false;
            pauseMenu.SetActive(_paused);

            foreach(AudioSource src in _audio.GetComponents<AudioSource>())
            {
                src.UnPause();
            }
        }
        else
        {
            Time.timeScale = 0;
            _paused = true;
            pauseMenu.SetActive(_paused);

            foreach (AudioSource src in _audio.GetComponents<AudioSource>())
            {
                src.Pause();
            }
        }
    }
}
