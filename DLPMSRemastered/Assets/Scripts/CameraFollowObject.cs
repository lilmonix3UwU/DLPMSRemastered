using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [SerializeField] private Player plr;
    [SerializeField] private float flipYRotTime = 0.5f;

    private void Update() => transform.position = plr.transform.position;    

    public void CallTurn(float rot) => LeanTween.rotateY(gameObject, rot, flipYRotTime).setEaseOutSine();
}
