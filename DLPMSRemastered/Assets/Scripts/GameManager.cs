using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Vector2 lastCheckpointPos;
    public int fireTorchAmount;
    public int iceTorchAmount;
    public int curTorch;
    public bool skipIntro;

    private void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
            Destroy(gameObject);
    }
}
