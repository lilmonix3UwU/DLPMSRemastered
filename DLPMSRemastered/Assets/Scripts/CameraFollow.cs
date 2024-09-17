using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float zoomFOV = 5f;
    [SerializeField] private Camera envCam;

    private float _defaultFOV;

    private Vector3 _vectorVel;
    private float _floatVel;

    private void Start() 
    {
        _defaultFOV = envCam.orthographicSize;
    }

    private void Update()
    {
        if (player.faceEnemy && player.ClosestEnemy() != null) 
        {
            Vector3 pos = (player.ClosestEnemy().transform.position + player.transform.position) / 2;
            transform.position = Vector3.SmoothDamp(transform.position, pos + offset, ref _vectorVel, smoothTime);
            envCam.orthographicSize = Mathf.SmoothDamp(envCam.orthographicSize, zoomFOV, ref _floatVel, smoothTime);
        }
        else 
        {
            transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + offset, ref _vectorVel, smoothTime);
            envCam.orthographicSize = Mathf.SmoothDamp(envCam.orthographicSize, _defaultFOV, ref _floatVel, smoothTime);
        }
    }
}
