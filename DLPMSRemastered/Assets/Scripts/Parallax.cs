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
        Vector2 newPos = startPos + travel * parallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }
}
