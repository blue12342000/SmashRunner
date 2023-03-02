using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cinemachine;
using System.Text;
using UnityEngine.Events;

public class TrailMovement : MovementBase
{
    [SerializeField]
    [HideInInspector]
    CinemachinePathBase m_trailPath;
    [SerializeField]
    [HideInInspector]
    CinemachinePathBase.PositionUnits m_positionUnit = CinemachinePathBase.PositionUnits.Distance;
    [SerializeField]
    [HideInInspector]
    float m_pathPositoin;

    public float StartPosition => 0;
    public float EndPosition => (m_trailPath == null) ? 0 : m_trailPath.PathLength;
    public float CurrentPosition => m_pathPositoin;

    void Awake()
    {
        m_isJumping = false;
        m_isGround = false;
        m_isFalling = false;
        m_velocity = Vector3.zero;

        if (m_trailPath != null)
        {
            m_pathPositoin = m_trailPath.StandardizeUnit(m_pathPositoin, m_positionUnit);
            transform.position = m_trailPath.EvaluatePositionAtUnit(m_pathPositoin, m_positionUnit).RayCast(Vector3.down, m_collisionLayer);
            transform.rotation = m_trailPath.EvaluateOrientationAtUnit(m_pathPositoin, m_positionUnit).Zero(true, false, true);
        }
    }

        public void SetPathPosition(float distance)
    {
        m_pathPositoin = distance;
        Vector3 position = (m_trailPath.EvaluatePositionAtUnit(m_trailPath.StandardizeUnit(m_pathPositoin, m_positionUnit), m_positionUnit) + Vector3.up * 0.2f).RayCast(Vector3.down, m_collisionLayer.value);
        transform.rotation = m_trailPath.EvaluateOrientationAtUnit(distance, m_positionUnit).Zero(true, false, true);

        position.y += PHYSICS_CAST_MIN_DISTANCE;
        transform.position = position;
    }

    public override MovementData Move(Vector3 velocity)
    {
        if (m_trailPath == null) return default;
        if (velocity.sqrMagnitude < float.Epsilon) return default;

        MovementData output;

        m_pathPositoin = m_pathPositoin + velocity.magnitude;
        output.Distance = m_pathPositoin = m_trailPath.StandardizeUnit(m_pathPositoin, m_positionUnit);
        output.Position = m_trailPath.EvaluatePositionAtUnit(m_pathPositoin, m_positionUnit).RayCast(Vector3.down, m_collisionLayer);

        output.LookAt = output.Rotation = m_trailPath.EvaluateOrientationAtUnit(m_pathPositoin, m_positionUnit).Zero(true, false, true);
        output.Velocity = velocity;
        output.SimulTime = Time.deltaTime;

        output.Position.y += PHYSICS_CAST_MIN_DISTANCE;
        //transform.position = output.Position;
        //transform.rotation = output.Rotation;

        Debug.Log("::: " + LayerMask.LayerToName(Translate(output.Position, output.Rotation)));

        return output;
    }

    public override void Jump(Vector3 velocity)
    {
        m_velocity = velocity;
        m_isFalling = false;
        m_isGround = false;
        m_isJumping = true;

        m_pathPositoin = m_trailPath.StandardizeUnit(m_pathPositoin + m_jumpDistance, m_positionUnit);
    }

    // Degree * 100 을 입력값으로 받음
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

    public override MovementData CalculateJumpEstimated()
    {
        MovementData output = default;
        if (m_trailPath == null) return output;
        m_positionUnit = CinemachinePathBase.PositionUnits.Distance;

        float position = m_pathPositoin + m_jumpDistance;

        position = m_trailPath.StandardizeUnit(position, m_positionUnit);
        output.Distance = m_pathPositoin - position;
        output.Position = (m_trailPath.EvaluatePositionAtUnit(position, m_positionUnit) + Vector3.up * 0.2f).RayCast(Vector3.down, m_collisionLayer.value);
        output.Rotation = m_trailPath.EvaluateOrientationAtUnit(position, m_positionUnit).Zero(true, false, true);

        /*
        1. 시작지점과 도착지점의 Y값이 큰 쪽을 기준으로 삼음
        2. 큰쪽에서 해당 Angle과 Velocity로 이동
        3. 축에 거리를 투영해서 실제 Forward 벡터 구함
        4. 축에 투영한 거리만큼이 예상 시간
        */
        // velocity 예측할 땐 시간도 예측해야함. => 시간을 forward.magnitude / distance; 로 해본다
        // angle 예측할 땐 velocity의 속력으로 거리까지 x속도 구해야함
        // angle 을 예측하는 것은 최소 최대값이 구해짐
        // distance 예측할 땐

        Vector3 vTarget = output.Position - transform.position;
        Vector3 vForward = vTarget.ZeroY();
        output.LookAt = Quaternion.FromToRotation(Vector3.forward, vForward.normalized).Zero(true, false, true);

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

                        output.Velocity = vForward.normalized * distanceXZ / time;
                        output.Velocity.y = velocityXZ * Mathf.Tan(m_jumpMinAngle * Mathf.Deg2Rad);
                        //output.Jump.Time = time;
                    }
                    else
                    {
                        // Possible
                        // min ~ max
                        float angle = FindAngle((int)ShortJumpDegree, (int)(LongJumpDegree), 100, distanceXZ, Mathf.Abs(vTarget.y));
                        output.Velocity = vForward.normalized * m_jumpVelocity * Mathf.Cos(angle);
                        output.Velocity.y = m_jumpVelocity * Mathf.Sin(angle);
                        output.SimulTime = distanceXZ / (m_jumpVelocity * Mathf.Cos(angle));
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

                    output.Velocity = vForward.normalized * distanceXZ / time;
                    output.Velocity.y = velocityXZ * Mathf.Tan(m_jumpMinAngle * Mathf.Deg2Rad);
                    output.SimulTime = time;
                }
                // jumpForce = Mathf.Sqrt(m_jumpDistance * m_gravityAcceleration * 0.5f / (Mathf.Sin(jumpAngle) * Mathf.Cos(jumpAngle) * Mathf.Cos(jumpAngle)));
                break;
        }

        return output;
    }

}