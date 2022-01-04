using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit, IHitable
{
    static readonly int m_hashJump = Animator.StringToHash("Jump");
    static readonly int m_hashMoveSpeed = Animator.StringToHash("MoveSpeed");

    Animator m_animator;

    void Awake()
    {
        m_sight = GetComponent<Sight>();
        m_animator = GetComponent<Animator>();
    }

    public override void Hit()
    {
        Debug.Log("Hit");
    }

    public void Jump()
    {
        m_animator.SetBool(m_hashJump, true);
    }

    public void Move(float scale)
    {
        m_animator.SetFloat(m_hashMoveSpeed, scale * 2);
    }
}
