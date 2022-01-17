using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public interface IMovement
{
    MovementBase Movement { get; }

    Vector3 Velocity { get; }
    float EstimatedTime { get; }
    bool IsGround { get; }
    void AddForce(Vector3 velocity);
    void AddForce(Vector3 direction, float angle, float distance);
}

public class Player : Unit, IHitable, IMovement//, ITrailMovement
{
    static readonly int m_hashJump = Animator.StringToHash("Jump");
    static readonly int m_hashJumpDistance = Animator.StringToHash("JumpDistance");
    static readonly int m_hashMoveSpeed = Animator.StringToHash("MoveSpeed");

    [SerializeField]
    float m_jumpDistance;
    [SerializeField]
    [Range(0, 3)]
    float m_timeScale;
    [SerializeField]
    float m_moveSpeed;

    [SerializeReference]
    TrailMovement m_movement;

    [SerializeField]
    CinemachinePathBase m_path;
    //[SerializeField]
    //CinemachinePathBase.PositionUnits m_positionUnit;
    //[SerializeField]
    //float m_position;
    //[SerializeField]
    //LayerMask m_groundLayer;

    Animator m_animator;

    public MovementBase Movement => m_movement;

    public Vector3 Velocity => Vector3.zero;
    public float EstimatedTime => 0;
    public bool IsGround => true;
    public void AddForce(Vector3 velocity) { }
    public void AddForce(Vector3 direction, float angle, float distance) { }

    void Awake()
    {
        m_characterController = GetComponent<CharacterController>();

        m_sight = GetComponent<Sight>();
        m_animator = GetComponent<Animator>();
        m_animator.SetFloat(m_hashJumpDistance, m_jumpDistance);
        m_animator.speed = m_timeScale;

        m_animator?.GetBehaviours<SMBBase>().Initialize(this);
    }

    public override void Hit()
    {
        Debug.Log("Hit");
    }

    void Update()
    {
        //SetTranform(m_position);
        //MovementBase.MoveInfo init = m_movement.Current;
        //transform.position = init.Position;
        //transform.rotation = init.Rotation;
    }

    public void Jump()
    {
        m_animator.SetBool(m_hashJump, true);
        //m_position += m_jumpDistance;
    }

    public void Move(float scale)
    {
        if (m_animator.GetBool(m_hashJump)) { m_animator.SetFloat(m_hashMoveSpeed, 0); }
        else
        {
            m_animator.SetFloat(m_hashMoveSpeed, scale);
            //m_movement.Move();
            //m_position += scale * 2 * Time.deltaTime * m_timeScale;
        }
    }

    //void SetTranform(float distanceAlongPath)
    //{
    //    if (m_path != null && !m_animator.GetBool(m_hashJump))
    //    {
    //        m_position = m_path.StandardizeUnit(distanceAlongPath, m_positionUnit);
    //        transform.position = m_path.EvaluatePositionAtUnit(m_position, m_positionUnit);
    //        Quaternion rotation = m_path.EvaluateOrientationAtUnit(m_position, m_positionUnit);
    //        rotation.x = 0;
    //        rotation.z = 0;
    //        transform.rotation = rotation;
    //        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, Vector3.down, out RaycastHit hitInfo, 3f, m_groundLayer))
    //        {
    //            transform.position = hitInfo.point;
    //        }
    //    }
    //}
    IEnumerator TimeScaleCurve(float fromScale, float toScale, float inSecond)
    {
        float timer = 0;
        float scale = fromScale;
        yield return null;

        while (timer < inSecond)
        {
            scale = Mathf.Lerp(fromScale, toScale, timer / inSecond);
            m_timeScale = scale;
            m_animator.speed = scale;

            timer += Time.deltaTime;
            yield return null;
        }

        m_timeScale = toScale;
        m_animator.speed = toScale;
    }

    public void SlowTimeScale()
    {
        StartCoroutine(TimeScaleCurve(m_timeScale, 0.05f, 0.5f));
    }

    public void ResetTimeScale()
    {
        StartCoroutine(TimeScaleCurve(m_timeScale, 1, 0.5f));
    }
}
