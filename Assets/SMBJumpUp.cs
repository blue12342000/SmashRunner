using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBJumpUp : StateMachineBehaviour
{
    [SerializeField]
    AnimationCurve m_speedCurve;
    [SerializeField]
    float m_force;
    
    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float deltaHeight = m_speedCurve.Evaluate(stateInfo.normalizedTime) * m_force * Time.deltaTime;
        animator.transform.position += Vector3.up * deltaHeight;
    }
}
