using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    [SerializeField]
    Vector3 m_moveHeight;
    [SerializeField]
    Vector3 m_velocity;
    [SerializeField]
    Vector3 m_groundPoint;

    IMovement m_movement;
    MovementBase.MovementData m_currMoveInfo;
    MovementBase.MovementData m_destMoveInfo;
    Quaternion m_rotationToDest;

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
                    animator.transform.rotation = Quaternion.Lerp(m_currMoveInfo.Rotation, m_rotationToDest, stateInfo.normalizedTime);
                }
                break;
            case EJumpState.Up:
                {
                    if (m_movement.Velocity.y < 0)
                    {
                        m_jumpState = EJumpState.Down;
                    }

                    //float deltaHeight = m_speedCurve.Evaluate(stateInfo.normalizedTime) * m_force * Time.deltaTime;
                    //if (m_movement.CheckCast(vDelta, m_destMoveInfo.Position, out RaycastHit hitInfo))
                    //{
                    //    vDelta = vDelta.normalized * hitInfo.distance;
                    //    m_moveHeight.y += vDelta.y;
                    //
                    //    var moveInfo = m_movement.MoveTo(vDelta, m_destMoveInfo);
                    //    moveInfo.Position = moveInfo.Position.Freeze(false, true, false) + m_moveHeight;
                    //    animator.transform.SetMoveInfo(moveInfo);
                    //
                    //    m_velocity = Vector3.down * m_gravityAcceleration * deltaTime;
                    //    m_jumpState = JumpState.Down;
                    //}
                    //else
                    //{
                    //    m_moveHeight.y += vDelta.y;
                    //    var moveInfo = m_movement.MoveTo(vDelta, m_destMoveInfo);
                    //    moveInfo.Position = moveInfo.Position.Freeze(false, true, false) + m_moveHeight;
                    //    animator.transform.SetMoveInfo(moveInfo);
                    //
                    //    m_velocity += Vector3.down * m_gravityAcceleration * deltaTime;
                    //    if (m_velocity.y < 0) { m_jumpState = JumpState.Down; }
                    //}
                    //animator.transform.position += (m_velocityFoward + m_velocityUp) * deltaTime;
                }
                break;
            case EJumpState.Down:
                {
                    if (m_movement.IsGround)
                    {
                        m_jumpState = EJumpState.Land;
                    }

                    // Ground Check
                    //if (m_movement.CheckCast(vDelta, m_destMoveInfo.Position, out Vector3 hitPoint))
                    //{
                        // Landing Motion

                        // if hitPoint is not dest position;
                        //vDelta = m_velocity.normalized * hitInfo.distance;
                        //
                        //var moveInfo = m_movement.MoveTo(vDelta, m_destMoveInfo);
                    //    animator.transform.position = hitPoint;
                    //
                    //    animator.SetTrigger(m_hashLand);
                    //    m_jumpState = JumpState.Land;
                    //}
                    //else
                    //{
                    //    m_moveHeight.y += vDelta.y;
                    //    var moveInfo = m_movement.MoveTo(vDelta, m_destMoveInfo);
                    //    moveInfo.Position = moveInfo.Position.Freeze(false, true, false) + m_moveHeight;
                    //    //animator.transform.position = moveInfo.Position;
                    //    animator.transform.SetMoveInfo(moveInfo);
                    //
                    //    m_velocity += Vector3.down * m_gravityAcceleration * deltaTime;
                    //}

                    /**
                    Vector3 moveVelocity = (m_velocityFoward + m_velocityUp) * deltaTime;
                    if (Physics.Raycast(animator.transform.position, moveVelocity.normalized, out RaycastHit hitInfo, moveVelocity.magnitude + 1 * animator.speed, m_groundLayer.value))
                    {
                        animator.SetTrigger(m_hashLand);
                        m_groundPoint = hitInfo.point;
                        m_jumpState = JumpState.Land;
                    }
                    else
                    {
                        animator.transform.position += moveVelocity;
                    }
                    **/
                }
                break;
            case EJumpState.Land:
                {
                    //if (m_movement.CheckCast(vDelta, out RaycastHit hitInfo))
                    //{
                    //    vDelta = vDelta.normalized * hitInfo.distance;
                    //    var moveInfo = m_movement.Move(vDelta);
                    //    animator.transform.SetMoveInfo(moveInfo);
                    //}
                    //else
                    //{
                    //    m_moveHeight.y += vDelta.y;
                    //    var moveInfo = m_movement.Move(vDelta);
                    //    moveInfo.Position = moveInfo.Position.Freeze(false, true, false) + m_moveHeight;
                    //    moveInfo.Rotation = Quaternion.Lerp(animator.transform.rotation, moveInfo.Rotation, stateInfo.normalizedTime);
                    //    animator.transform.SetMoveInfo(moveInfo);
                    //
                    //    m_velocity += Vector3.down * m_gravityAcceleration * deltaTime;
                    //}

                    // Rotation
                    animator.transform.rotation = Quaternion.Lerp(animator.transform.rotation, m_destMoveInfo.Rotation, stateInfo.normalizedTime);
                }
                break;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_jumpState == EJumpState.Ready)
        {
            m_jumpState = EJumpState.Up;
            m_movement.AddForce(animator.transform.forward, m_jumpAngle, m_jumpDistance);
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
        //m_moveHeight = animator.transform.position.Freeze(true, false, true);
        //float jumpAngle = m_jumpAngle * Mathf.Deg2Rad;
        //float jumpForce = m_jumpForce;
        //
        //m_currMoveInfo = m_movement.Current;
        //switch (m_jumpMode)
        //{
        //    case JumpMode.Fixed_Both:
        //        m_destMoveInfo = m_currMoveInfo;
        //        m_rotationToDest = animator.transform.rotation;
        //        break;
        //    case JumpMode.Fixed_Force:
        //        m_jumpDistance = animator.GetFloat(m_hashJumpDistance);
        //        // 점프 거리만큼의 목적지 계산
        //        m_destMoveInfo = m_movement.CalculatePoint(animator.transform.forward, m_jumpDistance);
        //        m_rotationToDest = Quaternion.LookRotation(m_destMoveInfo.Position - m_currMoveInfo.Position).Freeze(false, true, false);
        //
        //        m_jumpDistance = Vector3.Distance(m_currMoveInfo.Position, m_destMoveInfo.Position);
        //        jumpAngle = Mathf.Acos(m_gravityAcceleration * m_jumpDistance * 0.5f / (jumpForce * jumpForce));
        //        break;
        //    case JumpMode.Fixed_Angle:
        //        m_jumpDistance = animator.GetFloat(m_hashJumpDistance);
        //        m_destMoveInfo = m_movement.CalculatePoint(animator.transform.forward, m_jumpDistance);
        //        m_rotationToDest = Quaternion.LookRotation(m_destMoveInfo.Position - m_currMoveInfo.Position).Freeze(false, true, false);
        //
        //        m_jumpDistance = Vector3.Distance(m_currMoveInfo.Position, m_destMoveInfo.Position);
        //        jumpForce = Mathf.Sqrt(m_jumpDistance * m_gravityAcceleration * 0.5f / (Mathf.Sin(jumpAngle) * Mathf.Cos(jumpAngle) * Mathf.Cos(jumpAngle)));
        //        break;
        //}
        //
        //m_velocity = (m_destMoveInfo.Position - m_currMoveInfo.Position).normalized.Freeze(false, true, false) * jumpForce * Mathf.Cos(jumpAngle);
        //m_velocity += Vector3.up * jumpForce * Mathf.Sin(jumpAngle);
    }

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        animator.SetBool(m_hashJump, false);
    }
}
