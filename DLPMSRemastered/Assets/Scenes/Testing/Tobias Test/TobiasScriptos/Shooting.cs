using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private float attackCooldown;
    private Animator anim;
    private Player PlayerMovement;
    private float cooldownTimer = Mathf.Infinity;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        PlayerMovement = GetComponent<Player>();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && cooldownTimer > attackCooldown && PlayerMovement)
            Attack();

        cooldownTimer += Time.deltaTime;
    }

    private void Attack()
    {
        anim.SetTrigger("Attack");
        cooldownTimer = 0;
    }


}