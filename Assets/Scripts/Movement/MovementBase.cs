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
        Capsule,    // center, radius, height
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
    protected bool m_isGround;
    [SerializeField]
    protected bool m_isJumping;

    [SerializeField]
    MovementData m_current;

    public ColliderInfo Collider => m_collider;
    public Vector3 Velocity => m_velocity;
    public EJumpEstimate JumpEstimate => m_jumpEstimate;
    public virtual bool IsFalling => m_isFalling;
    public virtual bool IsGround => m_isGround;
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
    public abstract MovementData Move(Vector3 velocity);
    // Foward Jump
    public abstract MovementData Jump();
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
                    Vector3 height = transform.up * m_collider.Param.y;
                    isCollision = Physics.CapsuleCast(center + height, center - height, m_collider.Param.x, velocity.normalized, out hitInfo, velocity.magnitude, m_collisionLayer.value);
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

    protected virtual IEnumerator CheckFloor()
    {
        yield return null;

        while (true)
        {
            Vector3 center = transform.position + m_collider.Center;
            float maxDistance = 0;
            switch (m_collider.Type)
            {
                case ECollider.Box:
                    {
                        if (m_isGround = Physics.BoxCast(center, m_collider.Param * 0.5f, Vector3.down, out RaycastHit hitInfo, transform.rotation, -m_velocity.y, m_collisionLayer.value))
                        {
                            transform.position = hitInfo.point;
                        }
                        else
                        {
                            m_isFalling = true;
                        }
                    }
                    break;
                case ECollider.Capsule:
                    {
                        center -= transform.up * (m_collider.Param.y * 0.5f - m_collider.Param.x);
                        if (m_isGround = Physics.SphereCast(center, m_collider.Param.x, Vector3.down, out RaycastHit hitInfo, -m_velocity.y, m_collisionLayer.value))
                        {
                            transform.position = hitInfo.point + hitInfo.normal * m_collider.Param.x;
                        }
                        else
                        {
                            m_isFalling = true;
                        }
                    }
                    break;
                case ECollider.Sphere:
                    {
                        if (m_velocity.y > 0)
                        {
                            m_isFalling = false;
                        }
                        else
                        {
                            if (m_isGround = Physics.SphereCast(center, m_collider.Param.x, Vector3.down, out RaycastHit hitInfo, -m_velocity.y, m_collisionLayer.value))
                            {
                                transform.position = hitInfo.point + hitInfo.normal * m_collider.Param.x;
                            }
                            else
                            {
                                m_isFalling = true;
                            }
                        }
                    }
                    break;
            }

            if (m_isFalling)
            {
                transform.position += m_velocity;
                m_velocity.y -= m_gravityAcceleration * Time.deltaTime;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
