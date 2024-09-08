using TMPro;
using UnityEngine;

public class Torch : MonoBehaviour
{
    [SerializeField] private int minDist = 2;
    [SerializeField] private float smoothing = 0.2f;
    [SerializeField] private float scaleSpeed = 0.01f;
    [SerializeField] private float accel = 0.01f;
    [SerializeField] private Player plr;

    [SerializeField] private TMP_Text fireTorchText;
    [SerializeField] private TMP_Text iceTorchText;

    private Vector2 _vel;

    private AudioManager _audio;

    private void Start()
    {
        _audio = AudioManager.Instance;
    }

    private void Update()
    {
        if (Vector2.Distance(transform.position, plr.transform.position) < minDist)
        {
            smoothing -= accel;

            transform.position = Vector2.SmoothDamp(transform.position, plr.transform.position, ref _vel, smoothing);

            if (transform.localScale.x >= 0.5f)
                transform.localScale -= Vector3.one * scaleSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            switch(tag)
            {
                case "FireTorch":
                    plr.fireTorchAmount++;
                    fireTorchText.text = plr.fireTorchAmount.ToString();
                    break;
                case "IceTorch":
                    plr.iceTorchAmount++;
                    iceTorchText.text = plr.iceTorchAmount.ToString();
                    break;
            }

            _audio.Play("Pick Up");
            Destroy(gameObject);
        }
    }
}
