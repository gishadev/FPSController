using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();    
    }

    public void Attack()
    {
        animator.SetTrigger("Swing");
    }
}
