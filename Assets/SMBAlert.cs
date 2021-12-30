using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBAlert : EnemyBehaviour
{
    IDetect m_detect;

    public override void OnInitialize(MonoBehaviour target)
    {
        m_detect = target as IDetect;
        m_isInitialize = m_detect != null;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_isInitialize && m_detect.Detect())
        {
            animator.SetTrigger(m_hashAlert);
        }
    }
}
