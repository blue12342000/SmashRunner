using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cinemachine;
using System.Text;

public class TrailMovement : MovementBase
{
    [SerializeField]
    CinemachinePathBase m_trailPath;
    [SerializeField]
    CinemachinePathBase.PositionUnits m_positionUnit = CinemachinePathBase.PositionUnits.Distance;
    [SerializeField]
    float m_pathPositoin;

    public float StartPosition => 0;
    public float EndPosition => (m_trailPath == null) ? 0 : m_trailPath.PathLength;

    public void SetPathPosition(float distance)
    {
        m_pathPositoin = 0;
        transform.position = (m_trailPath.EvaluatePositionAtUnit(distance, m_positionUnit) + Vector3.up * 0.2f).RayCast(Vector3.down, m_collisionLayer.value);
        transform.rotation = m_trailPath.EvaluateOrientationAtUnit(distance, m_positionUnit).Zero(true, false, true);
    }

    public override MovementData Move(Vector3 velocity)
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
        return default;
    }

    public override MovementData Jump()
    {
        m_isFalling = false;
        m_isJumping = true;

        var moveData = CalculateEstimated(m_jumpDistance);
        m_velocity = moveData.Jump.Velocity;

        return moveData;
    }

    // Degree * 100 �� �Է°����� ����
    int BinearySearchDegree(int shortJumpDegree, int longJumpDegree, float scale, float distanceXZ, float diffY)
    {
        if (Mathf.Abs(shortJumpDegree - longJumpDegree) < 2) { return longJumpDegree; }

        int midDegree = (shortJumpDegree + longJumpDegree) / 2;
        float midAngle = scale * midDegree * Mathf.Deg2Rad;

        float velocityXZ = m_jumpVelocity * Mathf.Cos(midAngle);
        float height = distanceXZ * Mathf.Tan(midAngle) + diffY;
        float time = Mathf.Sqrt(2 * height / m_gravityAcceleration);
        float currDistanceXZ = velocityXZ * time;

        if (Mathf.Abs(currDistanceXZ - distanceXZ) < float.Epsilon)
        {
            return midDegree;
        }
        else if (distanceXZ < currDistanceXZ)
        {
            // new velocity too fast
            return BinearySearchDegree(shortJumpDegree, midDegree, scale, distanceXZ, diffY);
        }
        else
        {
            // new velocity to slow
            return BinearySearchDegree(midDegree, longJumpDegree, scale, distanceXZ, diffY);
        }
    }

    float FindAngle(int shortJumpDegree, int longJumpDegree, int precision, float distanceXZ, float diffY)
    {
        return Mathf.Deg2Rad * BinearySearchDegree(shortJumpDegree * precision, longJumpDegree * precision, 1f / precision, distanceXZ, diffY) / precision;
    }

    public override MovementData CalculateEstimated(float distance)
    {
        MovementData output = default;
        if (m_trailPath == null) return output;
        distance = m_jumpDistance;
        m_positionUnit = CinemachinePathBase.PositionUnits.Distance;

        distance = m_trailPath.StandardizeUnit(m_pathPositoin + distance, m_positionUnit);
        output.Position = (m_trailPath.EvaluatePositionAtUnit(distance, m_positionUnit) + Vector3.up * 0.2f).RayCast(Vector3.down, m_collisionLayer.value);
        output.Rotation = m_trailPath.EvaluateOrientationAtUnit(distance, m_positionUnit).ZeroY();

        /*
        1. ���������� ���������� Y���� ū ���� �������� ����
        2. ū�ʿ��� �ش� Angle�� Velocity�� �̵�
        3. �࿡ �Ÿ��� �����ؼ� ���� Forward ���� ����
        4. �࿡ ������ �Ÿ���ŭ�� ���� �ð�
        */
        // velocity ������ �� �ð��� �����ؾ���. => �ð��� forward.magnitude / distance; �� �غ���
        // angle ������ �� velocity�� �ӷ����� �Ÿ����� x�ӵ� ���ؾ���
        // angle �� �����ϴ� ���� �ּ� �ִ밪�� ������
        // distance ������ ��

        Vector3 vTarget = output.Position - transform.position;
        Vector3 vForward = vTarget.ZeroY();
        output.LookAt = Quaternion.FromToRotation(Vector3.forward, vForward.normalized);

        float distanceXZ = vForward.magnitude;

        switch (m_jumpEstimate)
        {
            case EJumpEstimate.Angle:
                {
                    // Param Velocity, Distance
                    // MinDistance Degree 10, 80 / MaxDistance Degree 45
                    // May Not Between Min.D And Max.D, Angle 45 Fixed And Calculate Velocity
                    float maxAngle = LongJumpAngle, minAngle = ShortJumpAngle;
                    float maxVelocityY = m_jumpVelocity * Mathf.Sin(maxAngle);
                    float maxVelocityXZ = m_jumpVelocity * Mathf.Cos(maxAngle);
                    float maxDistance = maxVelocityXZ * 2 * maxVelocityY / m_gravityAcceleration;

                    float minVelocityY = m_jumpVelocity * Mathf.Sin(minAngle);
                    float minVelocityXZ = m_jumpVelocity * Mathf.Cos(minAngle);
                    float minDistance = minVelocityXZ * 2 * minVelocityY / m_gravityAcceleration;

                    if (distanceXZ < minDistance || maxDistance < distanceXZ)
                    {
                        // Impossible Fixed Degree 45 And Calculate Velocity
                        float distanceY = Mathf.Tan(DEFAULT_JUMP_DEGREE * Mathf.Deg2Rad) * distanceXZ;
                        float height = distanceY + Mathf.Abs(vTarget.y);
                        float time = Mathf.Sqrt(2 * height / m_gravityAcceleration);
                        float velocityXZ = distanceXZ / time;

                        output.Jump.Velocity = vForward.normalized * distanceXZ / time;
                        output.Jump.Velocity.y = velocityXZ * Mathf.Tan(m_jumpMinAngle * Mathf.Deg2Rad);
                        output.Jump.Time = time;
                    }
                    else
                    {
                        // Possible
                        // min ~ max
                        float angle = FindAngle((int)ShortJumpDegree, (int)(LongJumpDegree), 100, distanceXZ, Mathf.Abs(vTarget.y));
                        output.Jump.Velocity = vForward.normalized * m_jumpVelocity * Mathf.Cos(angle);
                        output.Jump.Velocity.y = m_jumpVelocity * Mathf.Sin(angle);
                        output.Jump.Time = distanceXZ / (m_jumpVelocity * Mathf.Cos(angle));
                    }
                }
                // jumpAngle = Mathf.Acos(m_gravityAcceleration * m_jumpDistance * 0.5f / (jumpForce * jumpForce));
                break;
            case EJumpEstimate.Distance:
            case EJumpEstimate.Velocity:
                {
                    // Param Angle / distance
                    // vTarget Proj Axis X,Z 
                    float distanceY = Mathf.Tan(m_jumpMinAngle * Mathf.Deg2Rad) * distanceXZ;
                    float height = distanceY + Mathf.Abs(vTarget.y);
                    float time = Mathf.Sqrt(2 * height / m_gravityAcceleration);
                    float velocityXZ = distanceXZ / time;

                    output.Jump.Velocity = vForward.normalized * distanceXZ / time;
                    output.Jump.Velocity.y = velocityXZ * Mathf.Tan(m_jumpMinAngle * Mathf.Deg2Rad);
                    output.Jump.Time = time;
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