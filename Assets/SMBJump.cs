using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SMBJump : StateMachineBehaviour
{
    static readonly int m_hashJump = Animator.StringToHash("Jump");
    static readonly int m_hashJumpDistance = Animator.StringToHash("JumpDistance");
    static readonly int m_hashLand = Animator.StringToHash("Land");

    enum JumpState
    {
        UP,
        Down,
        Land
    }

    [Header("- Jump Action Data ([Distance Mode] Not Use Angle)")]
    [SerializeField]
    bool m_isJumpDistanceMode;
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
    JumpState m_jumpState;
    [SerializeField]
    float m_jumpDistance;
    [SerializeField]
    Vector3 m_velocityUp;
    [SerializeField]
    Vector3 m_velocityFoward;
    [SerializeField]
    Vector3 m_groundPoint;

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch (m_jumpState)
        {
            case JumpState.UP:
                {
                    //float deltaHeight = m_speedCurve.Evaluate(stateInfo.normalizedTime) * m_force * Time.deltaTime;
                    animator.transform.position += (m_velocityFoward + m_velocityUp) * Time.deltaTime;
                    m_velocityUp += Vector3.down * m_gravityAcceleration * Time.deltaTime;
                    if (m_velocityUp.y < 0) { m_jumpState = JumpState.Down; }
                }
                break;
            case JumpState.Down:
                {
                    Vector3 moveVelocity = (m_velocityFoward + m_velocityUp) * Time.deltaTime;
                    if (Physics.Raycast(animator.transform.position, moveVelocity.normalized, out RaycastHit hitInfo, moveVelocity.magnitude + 1, m_groundLayer.value))
                    {
                        animator.SetTrigger(m_hashLand);
                        m_groundPoint = hitInfo.point;
                        m_jumpState = JumpState.Land;
                    }
                    else
                    {
                        animator.transform.position += moveVelocity;
                    }
                    m_velocityUp += Vector3.down * m_gravityAcceleration * Time.deltaTime;
                }
                break;
            case JumpState.Land:
                {
                    animator.transform.position = Vector3.Lerp(animator.transform.position, m_groundPoint, m_landCurve.Evaluate(stateInfo.normalizedTime));
                }
                break;
        }
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_jumpState == JumpState.Land && animator.isHuman)
        {
            // Left Foot Rotation & Position
            Vector3 footPoint = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Ray ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hitPoint, 1.1f))
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.5f);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitPoint.point + Vector3.up * 0.1f);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(animator.transform.forward, hitPoint.normal));
            }

            // Right Foot Rotation & Position
            footPoint = animator.GetIKPosition(AvatarIKGoal.RightFoot);
            ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hitPoint, 1.1f))
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
        m_jumpState = JumpState.UP;

        float jumpAngle = m_jumpAngle * Mathf.Deg2Rad;
        if (m_isJumpDistanceMode)
        {
            m_jumpDistance = animator.GetFloat(m_hashJumpDistance);
            jumpAngle = Mathf.Acos(m_gravityAcceleration * m_jumpDistance * 0.5f / (m_jumpForce * m_jumpForce));
        }
        m_velocityFoward = animator.transform.forward * m_jumpForce * Mathf.Cos(jumpAngle);
        m_velocityUp = Vector3.up * m_jumpForce * Mathf.Sin(jumpAngle);
    }

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        animator.SetBool(m_hashJump, false);
    }
}
