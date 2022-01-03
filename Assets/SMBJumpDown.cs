using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBJumpDown : StateMachineBehaviour
{
    static readonly int m_hashLand = Animator.StringToHash("Land");

    [SerializeField]
    AnimationCurve m_speedCurve;
    [SerializeField]
    float m_gravity;
    [SerializeField]
    LayerMask m_groundLayer;

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float deltaHeight = m_speedCurve.Evaluate(stateInfo.normalizedTime) * m_gravity * Time.deltaTime;
        if (Physics.Raycast(animator.transform.position, Vector3.down, deltaHeight + 1, m_groundLayer.value))
        {
            animator.SetTrigger(m_hashLand);
        }
        else
        {
            animator.transform.position += Vector3.down * deltaHeight;
        }
    }
}
