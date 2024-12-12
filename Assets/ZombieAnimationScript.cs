using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimationScript : MonoBehaviour
{
    Animator animator;
    public bool walking;
    public bool attacking;
    public bool dead;


    void Start()
    {   
        walking = true;
        attacking = false;
        dead = false;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        checkAction();
    }

    private void checkAction() {
        if(walking) {
            animator.SetBool("isWalking", true);
        }
        if(!walking) {
            animator.SetBool("isWalking", false);
        }
        if(attacking) {
            animator.SetBool("isAttacking", true);
        }
        if(!attacking) {
            animator.SetBool("isAttacking", false);
        }
        //if(dead) {
        //    animator.SetBool("isDead", true);
        //}
        //if(!dead) {
        //    animator.SetBool("isDead", false);
        //}
    }

    public void SetWalkingTrue() {
        walking = true;
    }

    public void SetAttackingTrue() {
        attacking = true;
    }

    public void SetWalkingFalse() {
        walking = false;
    }

    public void SetAttackingFalse() {
        attacking = false;
    }
}
