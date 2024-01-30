using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private Vector3 offset;

    private Vector3 _vel;

    private void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref _vel, smoothTime);
    }
}
