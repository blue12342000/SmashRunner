using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cinemachine;

public class TrailMovement : MovementBase
{
    [SerializeField]
    CinemachinePathBase m_trailPath;
    CinemachinePathBase.PositionUnits m_positionUnit = CinemachinePathBase.PositionUnits.Distance;
    float m_position;
    [SerializeField]
    MoveInfo m_moveInfo;

    public override void Move(Vector3 velocity)
    {
        m_isFalling = false;
        //if (TrailPath == null) return default;
        //MoveInfo output;
        //// �̵��ϰ� �� �Ÿ�
        //output.Distance = TrailPath.StandardizeUnit(m_moveInfo.Distance + m_timeScale * moveScale, PositionUnit) - m_moveInfo.Distance;
        //m_moveInfo.Distance += output.Distance;
        //m_moveInfo.Position = output.Position = TrailPath.EvaluatePositionAtUnit(m_moveInfo.Distance, PositionUnit).RayCast(Vector3.down, m_collisionLayer.value); ;
        //m_moveInfo.Rotation = output.Rotation = TrailPath.EvaluateOrientationAtUnit(m_moveInfo.Distance, PositionUnit).Freeze(false, true, false);
        //return output;
        //return Move(velocity.normalized, velocity.Freeze(false, true, false).magnitude);
    }

    public override void JumpTo(Vector3 direction, float angle, float distance)
    {
        m_isFalling = false;
        m_isJumping = true;

        // ��ġ ���
        // �ּ� Angle�� ���� 
        direction = direction.Freeze(false, true, false).normalized;
        CalculateEstimatedPoint(direction, distance);
        // �ش� ��ġ���� 

        //MoveInfo output;
        //
        //Vector3 distance = dest.Position - m_moveInfo.Position;
        //Vector3 projVelocity = Vector3.Project(velocity, distance.normalized);
        //
        //float ratio = projVelocity.magnitude / distance.magnitude;
        //
        //output.Position = m_moveInfo.Position += projVelocity;
        //output.Rotation = Quaternion.FromToRotation(m_moveInfo.Position, dest.Position).Freeze(false, true, false);
        //output.Distance = m_moveInfo.Distance;
        //return output;
    }

    public override MoveInfo CalculateEstimatedPoint(Vector3 direction, float distance)
    {
        if (m_trailPath == null) return default;
        m_positionUnit = CinemachinePathBase.PositionUnits.Distance;
        MoveInfo output;
        output.Distance = m_trailPath.StandardizeUnit(m_position + distance, m_positionUnit);
        output.Position = (m_trailPath.EvaluatePositionAtUnit(output.Distance, m_positionUnit)+Vector3.up).RayCast(Vector3.down, m_collisionLayer.value);
        output.Rotation = m_trailPath.EvaluateOrientationAtUnit(output.Distance, m_positionUnit).Freeze(false, true, false);

        /*
        1. ���������� ���������� Y���� ū ���� �������� ����
        2. ū�ʿ��� �ش� Angle�� Velocity�� �̵�
        3. �࿡ �Ÿ��� �����ؼ� ���� Forward ���� ����
        4. �࿡ ������ �Ÿ���ŭ�� ���� �ð�
        */
        // velocity ������ �� �ð��� �����ؾ���.
        // angle ������ �� velocity�� �ӷ����� �Ÿ����� x�ӵ� ���ؾ���
        // angle �� �����ϴ� ���� �ּ� �ִ밪�� ������
        Vector3 dest = output.Position - transform.position;
        Vector3 vForward = Vector3.ProjectOnPlane(dest, Vector3.up);
        float time = vForward.magnitude;

        Vector3 vUp = Vector3.up * vForward.magnitude * Mathf.Tan(m_jumpMinAngle * Mathf.Deg2Rad);
        float jumpTime = 2 * vUp.y / m_gravityAcceleration;
        Vector3 gapVForward = vForward * (time - jumpTime);
        Vector3 gapVUp = Vector3.up * Mathf.Abs(dest.y) / (time - jumpTime);

        m_velocity = gapVUp + gapVForward;
        float velocity = (m_velocity / (time - jumpTime) * time).magnitude;
        float jumpAngle = Mathf.Atan(gapVUp.y / (time - jumpTime));

        Debug.Log($"distance : {m_position + distance}");
        Debug.Log($"output.Distance : {output.Distance}");
        Debug.Log($"origin = {transform.position}, dest = {output.Position}");
        Debug.Log($"total time : {time}");
        Debug.Log($"jump Time : {jumpTime}");
        Debug.Log($"==================================");
        Debug.Log($"Angle : {jumpAngle}, Degree : {jumpAngle * Mathf.Rad2Deg}, Min Degree : {m_jumpMinAngle}");
        Debug.Log($"velocity : {velocity}");
        Debug.Log($"angle * velocity : { Mathf.Sin(jumpAngle) * Vector3.up + vForward.normalized * Mathf.Cos(jumpAngle)}");
        Debug.Log($"m_velocity : {m_velocity / (time - jumpTime) * time}");
        Debug.Log($"vForward : {vForward}");
        Debug.Log($"Proj Forward : {Vector3.ProjectOnPlane(m_velocity, Vector3.up)}");

        switch (m_jumpEstimate)
        {
            case EJumpEstimate.Distance:
                {
                    
                }
                break;
            case EJumpEstimate.Angle:
                {

                }
                // jumpAngle = Mathf.Acos(m_gravityAcceleration * m_jumpDistance * 0.5f / (jumpForce * jumpForce));
                break;
            case EJumpEstimate.Force:
                {
                    
                }
                // jumpForce = Mathf.Sqrt(m_jumpDistance * m_gravityAcceleration * 0.5f / (Mathf.Sin(jumpAngle) * Mathf.Cos(jumpAngle) * Mathf.Cos(jumpAngle)));
                break;
        }

        return output;
    }

    public override bool PhysicsCast(Vector3 velocity, out RaycastHit hitInfo)
    {
        if (velocity.y > 0) { hitInfo = default; return false; }

        return base.PhysicsCast(velocity, out hitInfo);
    }
}