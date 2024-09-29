using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [SerializeField] private CinemachineVirtualCamera[] virtualCams;

    [Header("Controls for lerping the Y Damping during player jump/fall")]
    [SerializeField] private float fallPanAmount = 0.25f;
    [SerializeField] private float fallYPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -15f;

    public bool LerpingYDamping { get; private set; }
    public bool LerpedFromPlayerFalling { get; set; }

    private CinemachineVirtualCamera _curCam;
    private CinemachineFramingTransposer _framingTransposer;

    private float _normYPanAmount;

    private Vector2 _startingTrackedObjectOffset;

    private void Awake() => Instance = this;

    private void Start() 
    {
        for (int i = 0; i < virtualCams.Length; i++) 
        {
            if (virtualCams[i].enabled) 
            {
                _curCam = virtualCams[i];

                _framingTransposer = _curCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            }    
        }

        _normYPanAmount = _framingTransposer.m_YDamping;

        _startingTrackedObjectOffset = _framingTransposer.m_TrackedObjectOffset;
    }

    public void LerpYDamping(bool playerFalling) => StartCoroutine(LerpYAction(playerFalling));

    private IEnumerator LerpYAction(bool playerFalling) 
    {
        LerpingYDamping = true;

        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        if (playerFalling) 
        {
            endDampAmount = fallPanAmount;
            LerpedFromPlayerFalling = true;
        }
        else
            endDampAmount = _normYPanAmount;

        float elapsedTime = 0f;
        while (elapsedTime < fallYPanTime) 
        {
            elapsedTime += Time.deltaTime;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / fallYPanTime);
            _framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        LerpingYDamping = false;
    }

    public void PanCameraOnContact(float panDist, float panTime, PanDirection panDir, bool panToStartingPos)
    {
        StartCoroutine(PanCamera(panDist, panTime, panDir, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDist, float panTime, PanDirection panDir, bool panToStartingPos)
    {
        Vector2 endPos = Vector2.zero;
        Vector2 startingPos = Vector2.zero;

        if (!panToStartingPos)
        {
            switch(panDir)
            {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    endPos = Vector2.left;
                    break;
                case PanDirection.Right:
                    endPos = Vector2.right;
                    break;
                default:
                    break;
            }

            endPos *= panDist;

            startingPos = _startingTrackedObjectOffset;

            endPos += startingPos;
        }
        else
        {
            startingPos = _framingTransposer.m_TrackedObjectOffset;
            endPos = _startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while (elapsedTime < panTime)
        {
            elapsedTime += Time.deltaTime;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, elapsedTime / panTime);
            _framingTransposer.m_TrackedObjectOffset = panLerp;

            yield return null;
        }
    }
}
