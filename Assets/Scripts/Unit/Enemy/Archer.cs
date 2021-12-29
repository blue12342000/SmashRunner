using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISeek
{
    Transform AlertPoint { get; }
    bool Seeking();
}

public interface ICatch
{
    ITakeOut TakeOutHandle { get; }
    bool Catch(GameObject obj);
}

public interface ITakeOut
{
    bool IsEmpty { get; }
    bool TakeOut(out GameObject obj);
}

public interface IAttack
{
    bool IsAttackReady { get; }
    void Attack();
}

public interface IReload
{
    void Reload();
}

public class Archer : Enemy, ISeek, ICatch, IAttack, IReload
{
    [SerializeField]
    float m_attackSpeed;
    [SerializeField]
    Transform m_IKRightHandTransform;
    [SerializeField]
    Bow m_leftWeapon;
    [SerializeField]
    Quiver m_backSlot;
    [SerializeField]
    GameObject m_catchObj;

    Animator m_animator;
    bool m_isCatchString;
    Quaternion m_initSpineAngle;
    Quaternion m_targetAngle;
    GameObject m_target;

    public Transform AlertPoint => m_sight.ObjectInSight.transform;
    public ITakeOut TakeOutHandle => m_backSlot;
    public bool IsAttackReady => m_backSlot && !m_backSlot.IsEmpty;

    void Awake()
    {
        m_sight = GetComponent<Sight>();
        m_animator = GetComponent<Animator>();
        m_animator?.GetBehaviours<EnemyBehaviour>().Initialize(this);
        m_initSpineAngle = m_animator.GetBoneTransform(HumanBodyBones.Spine).localRotation;
    }

    void Start()
    {
        m_targetAngle = Quaternion.identity;
        m_initSpineAngle = m_animator.GetBoneTransform(HumanBodyBones.Spine).localRotation;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (m_animator == null) return;

        if (layerIndex == 0)
        {
            // Base Layer
            if (m_target)
            {
                // Spin Spine IK
                m_targetAngle = Quaternion.FromToRotation((transform.forward + transform.right * 0.1f).normalized, (m_target.transform.position - transform.position).normalized);
                m_targetAngle.x = -m_targetAngle.y;
                m_targetAngle.y = 0;
                m_targetAngle.z = 0;
            }
            m_animator.SetBoneLocalRotation(HumanBodyBones.Spine, m_initSpineAngle * m_targetAngle);

            // Right Hand Catch Bow String
            if (m_isCatchString)
            {
                m_animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.2f);
                m_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.1f);
                m_animator.SetIKPosition(AvatarIKGoal.RightHand, m_leftWeapon.IKRigtHand.position - m_leftWeapon.transform.forward * 0.1f);
                m_animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(m_leftWeapon.transform.forward, m_animator.GetBoneTransform(HumanBodyBones.RightHand).forward));
            }

            // Look At Target
            if (m_target)
            {
                m_animator.SetLookAtWeight(1);
                m_animator.SetLookAtPosition(m_target.transform.position);
            }
        }
        else if (layerIndex == 1)
        {
            // Foot Layer
            m_animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            m_animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.5f);
            m_animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            m_animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.5f);

            // Left Foot Rotation & Position
            Vector3 footPoint = m_animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Ray ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hitPoint, 1.1f))
            {
                m_animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitPoint.point + Vector3.up * 0.1f);
                m_animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hitPoint.normal));
            }

            // Right Foot Rotation & Position
            footPoint = m_animator.GetIKPosition(AvatarIKGoal.RightFoot);
            ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hitPoint, 1.1f))
            {
                m_animator.SetIKPosition(AvatarIKGoal.RightFoot, hitPoint.point + Vector3.up * 0.1f);
                m_animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hitPoint.normal));
            }
        }
    }

    public void Reload()
    {
        m_isCatchString = true;
        m_leftWeapon.Ready(m_catchObj);
        m_catchObj = null;
    }

    public void Attack()
    {
        m_isCatchString = false;
        m_leftWeapon.Attack(m_target);
    }

    public bool Seeking()
    {
        if (m_sight == null || m_sight.IsEmpty) { m_target = null; return false; }
        m_target = m_sight.ObjectInSight;
        return true;
    }

    public bool Catch(GameObject obj)
    {
        if (obj == null) { return false; }

        m_catchObj = obj;
        obj.transform.parent = m_IKRightHandTransform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.rotation = m_IKRightHandTransform.rotation;

        return true;
    }
}
