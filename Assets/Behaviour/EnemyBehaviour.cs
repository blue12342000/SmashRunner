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

    public abstract void OnInitialize(MonoBehaviour target);
}
