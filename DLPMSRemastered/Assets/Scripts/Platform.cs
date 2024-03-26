using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public Collider2D[] platforms;
    private Player player;
    private float objectSize;
    [SerializeField] private Vector2 capsuleSize;
    [SerializeField] private Vector2 actualCapsuleSize;

    void Start()
    {
        actualCapsuleSize = new Vector2(capsuleSize.x + transform.localScale.x - 1, capsuleSize.y + transform.localScale.y - 1);
        platforms = Physics2D.OverlapCapsuleAll(transform.position, capsuleSize, CapsuleDirection2D.Horizontal, 0.0f);
        player = FindObjectOfType<Player>();
    }
}
