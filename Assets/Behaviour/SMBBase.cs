using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public abstract class SMBBase : StateMachineBehaviour
{
    protected bool m_isInitialize = false;
    static protected readonly int m_hashIdle = Animator.StringToHash("Idle");
    static protected readonly int m_hashFail = Animator.StringToHash("Fail");
    static protected readonly int m_hashSeek = Animator.StringToHash("Seek");
    static protected readonly int m_hashAttackReady = Animator.StringToHash("AttackReady");
    static protected readonly int m_hashDetect = Animator.StringToHash("Detect");
    static protected readonly int m_hashInSight = Animator.StringToHash("InSight");
    static protected readonly int m_hashSit = Animator.StringToHash("Sit");
    static protected readonly int m_hashJump = Animator.StringToHash("Jump");
    static protected readonly int m_hashJumpDistance = Animator.StringToHash("JumpDistance");
    static protected readonly int m_hashLand = Animator.StringToHash("Land");

    public virtual void OnInitialize(MonoBehaviour target) {}
}
