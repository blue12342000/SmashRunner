using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBReload : EnemyBehaviour
{
    IReload m_reload;
    public override void OnInitialize(MonoBehaviour target)
    {
        m_reload = target as IReload;
        m_isInitialize = m_reload != null;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_isInitialize) { m_reload.Reload(); }
    }
}
