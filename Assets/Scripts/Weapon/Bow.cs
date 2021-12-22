using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon
{
    [SerializeField]
    Animator m_animator;
    bool m_isFireReady;

    public float Tension { get; set; }

    void Awake()
    {
        Tension = 0f;
    }

    void Update()
    {
        if (m_isFireReady) Tension += Time.deltaTime;
        m_animator.SetFloat("Tension", Tension);
    }

    public void Ready()
    {
        m_isFireReady = true;
        m_animator.SetTrigger("Ready");
        Tension = 0;
    }

    public override void Attack()
    {
        m_isFireReady = false;
        m_animator.SetTrigger("Fire");
        Tension = 0;
    }
}
