using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class MovementBase : MonoBehaviour
{
    // 물리체크 보정값
    public const float PHYSICS_CAST_MIN_DISTANCE = 0.001f;
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
        // 이동거리
        public float Distance;
        // 위치
        public Vector3 Position;
        // 도착 했을때 방향
        public Quaternion Rotation;
        // 목적지 바라보는 방향
        public Quaternion LookAt;
        // 움직임 속도
        public Vector3 Velocity;
        // 시뮬레이션 타임
        public float SimulTime;
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
    protected UnityAction m_jumpCallback;

    [SerializeField]
    [HideInInspector]
    MovementData m_current;

    protected event UnityAction m_onPhysicsUpdate;

    public ColliderInfo Collider => m_collider;
    public Vector3 Velocity => m_velocity;
    public EJumpEstimate JumpEstimate => m_jumpEstimate;
    public virtual bool IsFalling => m_isFalling;
    public virtual bool IsGround => m_isGround;
    public virtual bool IsJumping => m_isJumping;
    public virtual float TimeScale => 1;
    public double Gravity => m_gravityAcceleration;

    static int number = 0;

    public UnityAction<System.Type> OnMoveSuccess;
    public UnityAction<System.Type> OnMoveFail;

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
    public abstract void Jump(Vector3 velocity);
    public void AddForce(Vector3 velocity)
    {
        m_isJumping = false;
        m_isGround = false;
        m_isFalling = false;

        m_velocity = velocity;
    }
    public int Translate(Vector3 position, Quaternion rotate)
    {
        Vector3 velocity = position - transform.position;

        int collisionLayer = PhysicsCast(transform, velocity, m_collider, out RaycastHit[] hitInfos, m_collisionLayer);

        if (collisionLayer == 0)
        {
            transform.position = position;
        }
        else
        {
            float distance = 0;
            foreach (var info in hitInfos) { distance = Mathf.Min(info.distance, distance); }
            transform.position += velocity.normalized * (distance - PHYSICS_CAST_MIN_DISTANCE);
        }
        transform.rotation = rotate;

        return collisionLayer;
    }
    // 예상 위치 정보 계산
    public abstract MovementData CalculateJumpEstimated();

    // 움직임 처리
    void OnPhysicsUpdate()
    {
        if (m_isUseGravity)
        {
            if (!m_isGround)
            {
                m_velocity.y -= m_gravityAcceleration * Time.deltaTime;
            }
        }

        // if velocity is zero
        if (m_velocity.sqrMagnitude < float.Epsilon) return;

        Vector3 velocity = m_velocity * Time.deltaTime;

        int collisionLayer = PhysicsCast(transform, velocity, m_collider, out RaycastHit[] hitInfos, m_collisionLayer);
        if (collisionLayer == 0)
        {
            // Not Collision
            transform.position += velocity;
        }
        else
        {
            // Collision move stop
            float distance = float.MaxValue;
            foreach (var info in hitInfos) { distance = Mathf.Min(info.distance, distance); }
            transform.position += m_velocity.normalized * (distance - PHYSICS_CAST_MIN_DISTANCE);


            m_velocity = Vector3.zero;

            Debug.Log("Distance :: " + distance + " ::: " + (distance - PHYSICS_CAST_MIN_DISTANCE));

            if (m_isJumping)
            {
                m_isJumping = false;

                // Collision Event Callback
                
                //AddForce((Vector3.up * 10 - transform.forward * 2));
            }
        }
    }

    IEnumerator GravityFlagUpdate()
    {
        yield return null;

        // Gravity Check Update Falling
        while (true)
        {
            if (m_velocity.y > 0)
            {
                m_isGround = false;
            }
            else
            {
                Vector3 gravity = Vector3.down * m_gravityAcceleration * Time.deltaTime * Time.deltaTime;
                //m_isGround = PhysicsCast(transform, gravity, m_collider, m_collisionLayer) > 0;
            }
            //m_isFalling = m_isGround?false:(m_velocity.y < 0);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public static int PhysicsCast(Transform trs, Vector3 velocity, ColliderInfo colliderInfo, int layerMask)
    {
        return PhysicsCast(trs, velocity, colliderInfo, out RaycastHit[] hitInfos, layerMask);
    }

    public static int PhysicsCast(Transform trs, Vector3 velocity, ColliderInfo colliderInfo, out RaycastHit[] hitInfos, int layerMask)
    {
        int collisionLayer = 0;
        Vector3 center = trs.TransformPoint(colliderInfo.Center);
        switch (colliderInfo.Type)
        {
            case ECollider.Box:
                hitInfos = Physics.BoxCastAll(center, colliderInfo.Param * 0.5f, velocity.normalized, trs.rotation, velocity.magnitude + PHYSICS_CAST_MIN_DISTANCE, layerMask);
                foreach (var info in hitInfos)
                {
                    //Debug.Log("Collistion :: " + LayerMask.LayerToName(info.transform.gameObject.layer));
                    //Debug.Log(info.transform.gameObject.name);
                    collisionLayer += info.transform.gameObject.layer;
                }
                break;
            case ECollider.Capsule:
                {
                    Vector3 height = trs.up * colliderInfo.Param.y * 0.25f;
                    hitInfos = Physics.CapsuleCastAll(center - height, center + height, colliderInfo.Param.x, velocity.normalized, velocity.magnitude + PHYSICS_CAST_MIN_DISTANCE, layerMask);
                    //hitInfos = Physics.RaycastAll(center - height * 2, velocity.normalized, velocity.magnitude + PHYSICS_CAST_MIN_DISTANCE, layerMask);
                    Debug.Log("start");
                    foreach (var info in hitInfos)
                    {
                        //Debug.Log("Collistion :: " + LayerMask.LayerToName(info.transform.gameObject.layer));
                        Debug.Log(info.transform.gameObject.name + " ::: " + info.distance + " ::: " + (velocity.magnitude + PHYSICS_CAST_MIN_DISTANCE));
                        collisionLayer += info.transform.gameObject.layer;
                    }
                    Debug.Log("end");
                }
                break;
            case ECollider.Sphere:
                hitInfos = Physics.SphereCastAll(center, colliderInfo.Param.x, velocity.normalized, velocity.magnitude + PHYSICS_CAST_MIN_DISTANCE, layerMask);
                foreach (var info in hitInfos)
                {
                    //Debug.Log("Collistion :: " + LayerMask.LayerToName(info.transform.gameObject.layer));
                    //Debug.Log(info.transform.gameObject.name);
                    collisionLayer += info.transform.gameObject.layer;
                }
                break;
            default:
                hitInfos = default;
                break;
        }

        return collisionLayer;
    }
}
