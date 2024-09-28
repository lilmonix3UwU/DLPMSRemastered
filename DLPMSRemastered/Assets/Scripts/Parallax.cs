using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private float parallaxFactor;

    private Vector2 startPos;
    private float startZ;

    private Camera cam => Camera.main;

    private Vector2 travel => (Vector2)cam.transform.position - startPos;

    void Start()
    {
        startPos = transform.position;
        startZ = transform.position.z;
    }

    void Update()
    {
        float newX = startPos.x + travel.x * parallaxFactor;
        float newY = startPos.y + travel.y * parallaxFactor / 10;
        transform.position = new Vector3(newX, newY, startZ);
    }
}
