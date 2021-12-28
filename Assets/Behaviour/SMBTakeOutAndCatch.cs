using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBTakeOutAndCatch : EnemyBehaviour
{
    ITakeOut m_takeOut;
    ICatch m_catch;

    public override void OnInitialize(MonoBehaviour target)
    {
        m_catch = target as ICatch;
        m_takeOut = m_catch?.TakeOutHandle;
        m_isInitialize = m_catch != null && m_takeOut != null;
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (!m_isInitialize || m_takeOut.IsEmpty) { animator.SetTrigger(m_hashFail); }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_isInitialize || m_takeOut.IsEmpty) return;

        if (m_takeOut.TakeOut(out GameObject obj))
        {
            m_catch.Catch(obj);
        }    
    }
}
