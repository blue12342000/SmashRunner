using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public abstract class EnemyBehaviour : StateMachineBehaviour
{
    protected bool m_isInitialize = false;
    static protected readonly int m_hashIdle = Animator.StringToHash("Idle");
    static protected readonly int m_hashFail = Animator.StringToHash("Fail");
    static protected readonly int m_hashSeek = Animator.StringToHash("Seek");
    static protected readonly int m_hashAttackReady = Animator.StringToHash("AttackReady");
    static protected readonly int m_hashDetect = Animator.StringToHash("Detect");
    static protected readonly int m_hashInSight = Animator.StringToHash("InSight");
    static protected readonly int m_hashSit = Animator.StringToHash("Sit");

    public abstract void OnInitialize(MonoBehaviour target);
}
