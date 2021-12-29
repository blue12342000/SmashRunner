using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon
{
    static readonly int m_hashReady = Animator.StringToHash("Ready");
    static readonly int m_hashTension = Animator.StringToHash("Tension");
    static readonly int m_hashFire = Animator.StringToHash("Fire");

    [SerializeField]
    Animator m_animator;
    bool m_isFireReady;
    [SerializeField]
    GameObject m_arrow;

    public float Tension { get; set; }

    void Awake()
    {
        Tension = 0f;
        m_animator.SetTrigger(m_hashReady);
    }

    void Update()
    {
        if (m_isFireReady) Tension += Time.deltaTime;
        m_animator.SetFloat(m_hashTension, Tension);
    }

    public void Ready(GameObject arrow)
    {
        if (arrow == null) return;

        m_isFireReady = true;
        m_animator.SetTrigger(m_hashReady);
        Tension = 0.4f;
        m_arrow = arrow;
        arrow.transform.parent = m_IKRightHandTransform;
        arrow.transform.localPosition = Vector3.zero;
        arrow.transform.LookAt(transform.position, Vector3.up);
    }

    public override void Attack(GameObject target)
    {
        m_isFireReady = false;
        m_animator.SetTrigger(m_hashFire);
        Tension = 0;
        if (m_arrow != null)
        {
            m_arrow.transform.parent = null;
            if (target != null) m_arrow.GetComponent<Arrow>().Fire(target.transform, 1);
            m_arrow = null;
        }
    }
}
