using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class MovementBase : MonoBehaviour
{
    // 물리체크 보정값
    const float PHYSICS_CAST_MIN_DISTANCE = 0.001f;
    public const float LIMIT_MIN_JUMP_DEGREE = 10;
    public const float LIMIT_MAX_JUMP_DEGREE = 80;
    public const float DEFAULT_JUMP_DEGREE = 60;

    public enum ECollider
    {
                    // center / param
        Box,        // center / x, y, z
        Capsule,    // center / radius, height
        Sphere      // center / radius
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
    [HideInInspector]
    protected ColliderInfo m_collider;

    [SerializeField]
    [HideInInspector]
    protected float m_timeScale;
    [SerializeField]
    [HideInInspector]
    protected bool m_isUseGravity;
    [SerializeField]
    [HideInInspector]
    protected float m_gravityAcceleration;
    [SerializeField]
    [HideInInspector]
    protected LayerMask m_collisionLayer;

    [SerializeField]
    [HideInInspector]
    protected Vector3 m_velocity;
    [SerializeField]
    [HideInInspector]
    protected EJumpEstimate m_jumpEstimate = EJumpEstimate.Distance;
    [SerializeField]
    [HideInInspector]
    protected float m_jumpDistance = 4;
    [SerializeField]
    [Range(LIMIT_MIN_JUMP_DEGREE, LIMIT_MAX_JUMP_DEGREE)]
    [HideInInspector]
    protected float m_jumpMinAngle = 60;
    [SerializeField]
    [HideInInspector]
    protected float m_jumpVelocity = 0;
    [SerializeField]
    [HideInInspector]
    protected bool m_isFalling;
    [SerializeField]
    [HideInInspector]
    protected bool m_isGround;
    [SerializeField]
    [HideInInspector]
    protected bool m_isJumping;

    [SerializeField]
    [HideInInspector]
    MovementData m_current;

    protected event UnityAction m_onPhysicsUpdate;

    public ColliderInfo Collider => m_collider;
    public Vector3 Velocity => m_velocity;
    public EJumpEstimate JumpEstimate => m_jumpEstimate;
    public virtual bool IsFalling => m_isFalling;
    public virtual bool IsGround => m_isGround;
    public virtual float TimeScale => 1;
    public double Gravity => m_gravityAcceleration;

    static int number = 0;

    protected virtual void OnEnable()
    {
        m_onPhysicsUpdate += OnPhysicsUpdate;
        if (m_isUseGravity) StartCoroutine(GravityFlagUpdate());
    }

    protected virtual void OnDisable()
    {
        m_onPhysicsUpdate -= OnPhysicsUpdate;
        StopAllCoroutines();
    }

    protected virtual void FixedUpdate()
    {
        if (m_onPhysicsUpdate != null) m_onPhysicsUpdate();
    }

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

    // 움직임 처리
    void OnPhysicsUpdate()
    {
        if (m_isUseGravity)
        {
            if (!m_isGround)
            {
                m_velocity += Vector3.down * m_gravityAcceleration * Time.deltaTime;
            }
        }

        // 정지해 있다면 멈춰!
        if (m_velocity.sqrMagnitude < float.Epsilon) return;

        Vector3 velocity = m_velocity * Time.deltaTime;
        if (!PhysicsCast(transform, velocity, m_collider, out RaycastHit hitInfo, m_collisionLayer))
        {
            // Not Collision
            transform.position += velocity;
        }
        else
        {
            // Collision move stop
            transform.position += m_velocity.normalized * hitInfo.distance;
            m_velocity = Vector3.zero;
        }
    }

    IEnumerator GravityFlagUpdate()
    {
        yield return null;

        while (true)
        {
            Vector3 gravity = Vector3.down * m_gravityAcceleration * Time.deltaTime * Time.deltaTime;
            m_isGround = PhysicsCast(transform, gravity, m_collider, m_collisionLayer);
            m_isFalling = m_isGround?false:(m_velocity.y < 0);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public static bool PhysicsCast(Transform trs, Vector3 velocity, ColliderInfo colliderInfo, int layerMask)
    {
        return PhysicsCast(trs, velocity, colliderInfo, out RaycastHit hitInfo, layerMask);
    }

    public static bool PhysicsCast(Transform trs, Vector3 velocity, ColliderInfo colliderInfo, out RaycastHit hitInfo, int layerMask)
    {
        bool isCollision = false;
        Vector3 center = trs.TransformPoint(colliderInfo.Center);

        switch (colliderInfo.Type)
        {
            case ECollider.Box:
                isCollision = Physics.BoxCast(center, colliderInfo.Param, velocity.normalized, out hitInfo, trs.rotation, velocity.magnitude + PHYSICS_CAST_MIN_DISTANCE, layerMask);
                break;
            case ECollider.Capsule:
                {
                    Vector3 height = trs.up * colliderInfo.Param.y * 0.25f;
                    isCollision = Physics.CapsuleCast(center + height, center - height, colliderInfo.Param.x, velocity.normalized, out hitInfo, velocity.magnitude + PHYSICS_CAST_MIN_DISTANCE, layerMask);
                }
                break;
            case ECollider.Sphere:
                isCollision = Physics.SphereCast(center, colliderInfo.Param.x, velocity.normalized, out hitInfo, velocity.magnitude + PHYSICS_CAST_MIN_DISTANCE, layerMask);
                break;
            default:
                hitInfo = default;
                break;
        }

        if (isCollision)
        {
            hitInfo.distance -= PHYSICS_CAST_MIN_DISTANCE;
        }

        return isCollision;
    }
}
