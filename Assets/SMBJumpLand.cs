using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBJumpLand : StateMachineBehaviour
{
    static readonly int m_hashJump = Animator.StringToHash("Jump");

    [SerializeField]
    float m_gravity;
    [SerializeField]
    LayerMask m_groundLayer;

    Vector3 m_position;
    Vector3 m_groundPoint;

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.transform.position = Vector3.Lerp(m_position, m_groundPoint, stateInfo.normalizedTime / 0.3f);
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float deltaHeight = m_gravity;
        m_position = m_groundPoint = animator.transform.position;
        if (Physics.Raycast(animator.transform.position, Vector3.down, out RaycastHit hitInfo, deltaHeight, m_groundLayer.value))
        {
            m_groundPoint = hitInfo.point;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(m_hashJump, false);
    }
}
