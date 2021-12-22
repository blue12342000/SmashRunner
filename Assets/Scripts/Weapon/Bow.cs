using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon
{
    [SerializeField]
    Animator m_animator;

    Coroutine m_reloadHandle;
    public float Tension { get; set; }

    void Awake()
    {
        Tension = 0f;
    }

    void Update()
    {
        m_animator.SetFloat("Tension", 1);
    }

    public override void Attack()
    {

    }

    private void OnAnimatorIK(int layerIndex)
    {
    }
}
