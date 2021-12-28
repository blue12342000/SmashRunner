using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBIdle : EnemyBehaviour
{
    [SerializeField]
    int m_idleCount;
    IAttack m_attack;

    public override void OnInitialize(MonoBehaviour target)
    {
        m_attack = target as IAttack;
        m_isInitialize = m_attack != null;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_isInitialize && animator.GetBool(m_hashAttackReady) != m_attack.IsAttackReady)
        {
            animator.SetBool(m_hashAttackReady, m_attack.IsAttackReady);
        }
    }

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.SetInteger(m_hashIdle, Random.Range(0, m_idleCount));
    }
}
