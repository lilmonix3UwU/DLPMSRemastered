using Cinemachine;
using UnityEngine;
using UnityEditor;

public class CameraControlTrigger : MonoBehaviour
{
    public CustomOptions customOptions;

    private Collider2D _coll;

    private CameraManager _camMgr;

    private void Start()
    {
        _camMgr = CameraManager.Instance;

        _coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player") && customOptions.panCamOnContact)
            _camMgr.PanCameraOnContact(customOptions.panDist, customOptions.panTime, customOptions.panDir, false);
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
if (other.CompareTag("Player") && customOptions.panCamOnContact)
            _camMgr.PanCameraOnContact(customOptions.panDist, customOptions.panTime, customOptions.panDir, true);
    }
}

[System.Serializable]
public class CustomOptions 
{
    public bool swapCams = false;
    public bool panCamOnContact = false;

    [HideInInspector] public CinemachineVirtualCamera camOnLeft;
    [HideInInspector] public CinemachineVirtualCamera camOnRight;

    [HideInInspector] public PanDirection panDir;
    [HideInInspector] public float panDist = 3f;
    [HideInInspector] public float panTime = 0.35f;
}

public enum PanDirection 
{
    Up,
    Down,
    Left,
    Right
}

[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor : Editor 
{
    CameraControlTrigger camControlTrigger;

    private void OnEnable() 
    {
        camControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (camControlTrigger.customOptions.swapCams) 
        {
            camControlTrigger.customOptions.camOnLeft = EditorGUILayout.ObjectField("Camera on Left", camControlTrigger.customOptions.camOnLeft, 
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

            camControlTrigger.customOptions.camOnRight = EditorGUILayout.ObjectField("Camera on Right", camControlTrigger.customOptions.camOnRight, 
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        }

        if (camControlTrigger.customOptions.panCamOnContact) 
        {
            camControlTrigger.customOptions.panDir = (PanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction", 
                camControlTrigger.customOptions.panDir);

            camControlTrigger.customOptions.panDist = EditorGUILayout.FloatField("Pan Distance", camControlTrigger.customOptions.panDist);
            camControlTrigger.customOptions.panTime = EditorGUILayout.FloatField("Pan Time", camControlTrigger.customOptions.panTime);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(camControlTrigger);
            }
        }
    }
}
