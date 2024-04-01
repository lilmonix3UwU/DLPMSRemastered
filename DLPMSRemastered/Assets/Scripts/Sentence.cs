using UnityEngine;

[System.Serializable]
public class Sentence
{
    public string text;

    [Range(0.01f, 1f)] public float secondsPerWord;
}
