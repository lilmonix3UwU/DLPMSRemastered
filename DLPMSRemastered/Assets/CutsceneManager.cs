using System.Collections;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private Transform grandma;
    [SerializeField] private Animator grandmaAnim;
    [SerializeField] private GameObject interactionBox;
    [SerializeField] private Player plr;
    //[SerializeField] private Camera cam;

    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private float grandmaWalkTime = 3f;

    private bool _doneWalking;

    private InteractionManager _interaction;

    private void Start()
    {
        _interaction = InteractionManager.Instance;

        Invoke("StartCutscene", 22);
    }

    private void Update()
    {
        if (!_interaction.interactionActive && _doneWalking)
        {
            StartCoroutine(GrandmaWalk(endPos.position, startPos.position));
        }
    }

    private void StartCutscene()
    {
        StartCoroutine(GrandmaWalk(startPos.position, endPos.position));
    }

    private IEnumerator GrandmaWalk(Vector3 from, Vector3 to)
    {
        yield return new WaitForSeconds(0.5f);

        _doneWalking = false;

        plr.onlyAnimate = true;

        grandmaAnim.SetBool("Walking", true);
        grandma.rotation = from.x - to.x < 0 ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);

        float elapsedTime = 0;

        while (elapsedTime < grandmaWalkTime)
        {
            elapsedTime += Time.deltaTime;
            grandma.position = Vector3.Lerp(from, to, elapsedTime / grandmaWalkTime);
            yield return null;
        }

        grandma.position = to;

        grandmaAnim.SetBool("Walking", false);

        if (grandma.position != startPos.position)
            _doneWalking = true;
        else
        {
            grandma.gameObject.SetActive(false);
            interactionBox.SetActive(false);
            plr.onlyAnimate = false;
        }
    }
}
