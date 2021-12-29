using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBStanding : EnemyBehaviour
{
    ISeek m_seek;
    Transform m_target;
    Quaternion m_destAngle;

    public override void OnInitialize(MonoBehaviour target)
    {
        m_seek = target as ISeek;
        m_target = target.transform;
        m_isInitialize = m_seek != null;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_isInitialize) return;
        m_target.rotation = Quaternion.Lerp(m_target.rotation, m_destAngle, stateInfo.normalizedTime);
    }

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (!m_isInitialize) return;
        animator.SetFloat(m_hashDirection, Vector3.Dot(m_target.forward, m_seek.AlertPoint.position - m_target.position));
        m_destAngle = Quaternion.FromToRotation(Vector3.forward, (m_seek.AlertPoint.position - m_target.position).normalized);
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        m_target.rotation = m_destAngle;
    }
}
