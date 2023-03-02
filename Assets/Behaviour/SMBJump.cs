using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;

public class SMBJump : SMBBase
{
    enum EJumpState
    {
        Ready,
        Up,
        Down,
        Land
    }

    enum EJumpMode
    {
        Fixed_Both,
        Fixed_Force,
        Fixed_Angle
    }

    [Header("- Jump Action Data")]
    [SerializeField]
    EJumpMode m_jumpMode = EJumpMode.Fixed_Both;
    [SerializeField]
    float m_jumpAngle;
    [SerializeField]
    float m_jumpForce;
    [SerializeField]
    float m_gravityAcceleration;
    [SerializeField]
    AnimationCurve m_landCurve;
    [SerializeField]
    LayerMask m_groundLayer;

    [Header("- Debug Current Info")]
    [SerializeField]
    EJumpState m_jumpState = EJumpState.Ready;
    [SerializeField]
    float m_jumpDistance;

    IMovement m_movement;
    int m_currStateId;
    MovementBase.MovementData m_dest;

    public override void OnInitialize(MonoBehaviour target)
    {
        m_movement = target as IMovement;
        m_isInitialize = m_movement != null;
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (m_jumpState)
        {
            case EJumpState.Ready:
                {
                    //Debug.Log(stateInfo.normalizedTime);
                    animator.transform.rotation = Quaternion.Lerp(animator.transform.rotation, m_dest.LookAt, stateInfo.normalizedTime);
                }
                break;
            case EJumpState.Up:
                {
                    if (!m_movement.IsJumping)
                    {
                        m_jumpState = EJumpState.Land;
                        animator.SetTrigger(m_hashLand);
                    }
                    else if (m_movement.IsFalling)
                    {
                        m_jumpState = EJumpState.Down;
                    }
                }
                break;
            case EJumpState.Down:
                {
                    if (m_movement.IsGround || !m_movement.IsJumping)
                    {
                        m_jumpState = EJumpState.Land;
                        animator.SetTrigger(m_hashLand);
                    }
                }
                break;
            case EJumpState.Land:
                {
                    animator.transform.rotation = Quaternion.Lerp(m_dest.LookAt, m_dest.Rotation, stateInfo.normalizedTime);
                }
                break;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_jumpState == EJumpState.Ready)
        {
            m_jumpState = EJumpState.Up;
            animator.transform.rotation = m_dest.LookAt;
            m_movement.Movement.Jump(m_dest.Velocity);
            //m_movement.AddForce(animator.transform.forward, m_jumpAngle, m_jumpDistance);
        }
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_jumpState == EJumpState.Land && animator.isHuman)
        {
            // Left Foot Rotation & Position
            Vector3 footPoint = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Ray ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hitPoint, 1.1f, m_groundLayer.value))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.5f);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitPoint.point + Vector3.up * 0.1f);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(animator.transform.forward, hitPoint.normal));
            }

            // Right Foot Rotation & Position
            footPoint = animator.GetIKPosition(AvatarIKGoal.RightFoot);
            ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hitPoint, 1.1f, m_groundLayer.value))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.5f);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, hitPoint.point + Vector3.up * 0.1f);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(animator.transform.forward, hitPoint.normal));
            }
        }
    }

    // OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        m_jumpState = EJumpState.Ready;
        m_dest = m_movement.Movement.CalculateJumpEstimated();
    }

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        animator.SetBool(m_hashJump, false);
        animator.transform.rotation = m_dest.Rotation;
    }
}
