using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementBase : MonoBehaviour
{
    public const float LIMIT_MIN_JUMP_DEGREE = 10;
    public const float LIMIT_MAX_JUMP_DEGREE = 80;
    public const float DEFAULT_JUMP_DEGREE = 60;

    public enum ECollider
    {
        Box,        // center, x, y, z
        Capsule,    // center, height, radius
        Sphere      // center, radius
    }

    public enum EUpdateType
    {
        FixedUpdate,
        Update
    }

    public enum EJumpEstimate
    {
        Distance,
        Angle,
        Velocity
    }

    [System.Serializable]
    public struct ColliderInfo
    {
        public ECollider Type;
        public Vector3 Center;
        public Vector3 Param;
    }

    [System.Serializable]
    public struct MovementData
    {
        // 위치
        public Vector3 Position;
        // 도착 했을때 방향
        public Quaternion Rotation;
        // 목적지 바라보는 방향
        public Quaternion LookAt;
        // 점프
        public JumpData Jump;
    }

    [System.Serializable]
    public struct JumpData
    {
        public Vector3 Velocity;
        public float Time;
    }

    [SerializeField]
    protected ColliderInfo m_collider;

    [SerializeReference]
    public CharacterController Character;

    [SerializeField]
    protected float m_timeScale;
    [SerializeField]
    protected bool m_isUseGravity;
    [SerializeField]
    protected float m_gravityAcceleration;
    [SerializeField]
    protected LayerMask m_collisionLayer;

    [SerializeField]
    protected Vector3 m_velocity;
    [SerializeField]
    protected EJumpEstimate m_jumpEstimate = EJumpEstimate.Distance;
    [SerializeField]
    protected float m_jumpDistance = 4;
    [SerializeField]
    [Range(LIMIT_MIN_JUMP_DEGREE, LIMIT_MAX_JUMP_DEGREE)]
    protected float m_jumpMinAngle = 60;
    [SerializeField]
    protected float m_jumpVelocity = 0;
    [SerializeField]
    protected bool m_isFalling;
    [SerializeField]
    protected bool m_isJumping;

    public ColliderInfo Collider => m_collider;
    public Vector3 Velocity => m_velocity;
    public EJumpEstimate JumpEstimate => m_jumpEstimate;
    public virtual bool IsGround => false;
    public virtual float TimeScale => 1;
    public double Gravity => m_gravityAcceleration;

    public static float LongJumpDegree
    { 
        get
        {
            if (45 < LIMIT_MIN_JUMP_DEGREE)
            {
                return LIMIT_MIN_JUMP_DEGREE;
            }
            else if (LIMIT_MAX_JUMP_DEGREE < 45)
            {
                return LIMIT_MAX_JUMP_DEGREE;
            }
            else
            {
                return 45;
            }
        }
    }

    public static float ShortJumpDegree
    {
        get
        {
            if (45 < LIMIT_MIN_JUMP_DEGREE)
            {
                return LIMIT_MAX_JUMP_DEGREE;
            }
            else if (LIMIT_MAX_JUMP_DEGREE < 45)
            {
                return LIMIT_MIN_JUMP_DEGREE;
            }
            else
            {
                if (LIMIT_MIN_JUMP_DEGREE < 90 - LIMIT_MAX_JUMP_DEGREE) { return LIMIT_MIN_JUMP_DEGREE; }
                else { return LIMIT_MAX_JUMP_DEGREE; }
            }
        }
    }

    public static float LongJumpAngle => LongJumpDegree * Mathf.Deg2Rad;
    public static float ShortJumpAngle => ShortJumpDegree * Mathf.Deg2Rad;

    // 방향으로 Velocity만큼 이동
    public abstract void Move(Vector3 velocity);
    // Foward Jump
    public abstract void Jump(float distance);
    // 예상 위치 정보 계산
    public abstract MovementData CalculateEstimated(float distance);

    public bool PhysicsCast()
    {
        return PhysicsCast(m_velocity, out RaycastHit hitInfo);
    }

    public bool PhysicsCast(out RaycastHit hitInfo)
    {
        return PhysicsCast(m_velocity, out hitInfo);
    }

    public bool PhysicsCast(Vector3 velocity)
    {
        return PhysicsCast(velocity, out RaycastHit hitInfo);
    }

    public virtual bool PhysicsCast(Vector3 velocity, out RaycastHit hitInfo)
    {
        bool isCollision = false;
        Vector3 center = m_collider.Center + transform.position;
        switch (m_collider.Type)
        {
            case ECollider.Box:
                isCollision = Physics.BoxCast(center, m_collider.Param, velocity.normalized, out hitInfo, transform.rotation, velocity.magnitude, m_collisionLayer.value);
                break;
            case ECollider.Capsule:
                {
                    Vector3 height = transform.up * m_collider.Param.x;
                    isCollision = Physics.CapsuleCast(center + height, center - height, m_collider.Param.y, velocity.normalized, out hitInfo, velocity.magnitude, m_collisionLayer.value);
                }
                break;
            case ECollider.Sphere:
                isCollision = Physics.SphereCast(center, m_collider.Param.x, velocity.normalized, out hitInfo, velocity.magnitude, m_collisionLayer.value);
                break;
            default:
                hitInfo = default;
                break;
        }
        return isCollision;
    }
}
