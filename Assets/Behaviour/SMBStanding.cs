using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBStanding : SMBBase
{
    IDetect m_detect;
    Quaternion m_destAngle;

    public override void OnInitialize(MonoBehaviour target)
    {
        m_detect = target as IDetect;
        m_isInitialize = m_detect != null;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!m_isInitialize) return;
        m_detect.Eye.transform.rotation = Quaternion.Lerp(m_detect.Eye.transform.rotation, m_destAngle, stateInfo.normalizedTime);
    }

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (!m_isInitialize) return;
        animator.SetBool(m_hashSit, false);
        animator.SetBool(m_hashInSight, m_detect.Eye.IsExistInSight);

        if (m_detect.Eye.IsExistInSight)
        {
            m_destAngle = Quaternion.FromToRotation(Vector3.forward, (m_detect.Eye.ObjectInSight.transform.position - m_detect.Eye.transform.position).normalized);
        }
        else if (m_detect.Eye.IsExistInRange)
        {
            m_destAngle = Quaternion.FromToRotation(Vector3.forward, (m_detect.Eye.ObjectOutSight.transform.position - m_detect.Eye.transform.position).normalized);
        }
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        m_detect.Eye.transform.rotation = m_destAngle;
    }
}
