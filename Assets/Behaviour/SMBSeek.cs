using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBSeek: EnemyBehaviour
{
    ISeek m_seeker;

    public override void OnInitialize(MonoBehaviour target)
    {
        m_seeker = target as ISeek;
        m_isInitialize = m_seeker != null;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_isInitialize && m_seeker.Seeking())
        {
            animator.SetTrigger(m_hashSeek);
        }
    }
}
