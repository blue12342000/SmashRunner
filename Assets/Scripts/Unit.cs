using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IJump
{
    void Jump();
}

interface IHitable
{
    void Hit();
}

public abstract class Unit : MonoBehaviour, IHitable
{
    [SerializeField]
    protected float m_fov;
    [SerializeField]
    protected float m_attackRange;
    [SerializeField]
    protected int m_attackDamage;
    [SerializeField]
    protected int m_hp;
    [SerializeField]
    protected int m_maxHp;

    public abstract void Hit();

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, m_attackRange);

        Gizmos.color = Color.red;
        float leftAngle = transform.eulerAngles.y - m_fov * 0.5f;
        float rightAngle = transform.eulerAngles.y + m_fov * 0.5f;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + new Vector3(Mathf.Sin(leftAngle * Mathf.Deg2Rad), 0, Mathf.Cos(leftAngle * Mathf.Deg2Rad)) * m_attackRange);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + new Vector3(Mathf.Sin(rightAngle * Mathf.Deg2Rad), 0, Mathf.Cos(rightAngle * Mathf.Deg2Rad)) * m_attackRange);
    }
}
