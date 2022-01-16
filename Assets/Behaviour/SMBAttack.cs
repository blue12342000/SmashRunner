using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBAttack : SMBBase
{
    IAttack m_attack;

    public override void OnInitialize(MonoBehaviour target)
    {
        m_attack = target as IAttack;
        m_isInitialize = m_attack != null;
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_isInitialize) return;
        m_attack.Attack();
    }
}
