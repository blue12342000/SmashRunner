using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Enemy
{
    [SerializeField]
    float m_attackSpeed;
    [SerializeField]
    Transform m_IKRightHandTransform;
    [SerializeField]
    Bow m_leftWeapon;

    Animator m_animator;
    bool m_isReload;
    Quaternion m_initSpineAngle;
    Quaternion m_targetAngle;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_sight = GetComponent<Sight>();
    }

    void Start()
    {
        m_initSpineAngle = m_animator.GetBoneTransform(HumanBodyBones.Spine).localRotation;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex == 0)
        {
            // Base Layer
            if (m_sight.ObjectsInSight != null && m_sight.ObjectsInSight.Length > 0)
            {
                // Look At Target
                m_targetAngle = Quaternion.FromToRotation(transform.forward, (m_sight.ObjectsInSight[0].transform.position - transform.position).normalized);
                m_targetAngle.x = -m_targetAngle.y;
                m_targetAngle.y = 0;
                m_targetAngle.z = 0;
            }
            m_animator.SetBoneLocalRotation(HumanBodyBones.Spine, m_initSpineAngle * m_targetAngle);
        }
        else if (layerIndex == 1)
        {
            // Foot Layer
            m_animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            m_animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            m_animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            m_animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

            // Left Foot Rotation & Position
            Vector3 footPoint = m_animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Ray ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hitPoint, 1.1f))
            {
                m_animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitPoint.point + Vector3.up * 0.1f);
                m_animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.right, hitPoint.normal));
            }

            // Right Foot Rotation & Position
            footPoint = m_animator.GetIKPosition(AvatarIKGoal.RightFoot);
            ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hitPoint, 1.1f))
            {
                m_animator.SetIKPosition(AvatarIKGoal.RightFoot, hitPoint.point + Vector3.up * 0.1f);
                m_animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.right, hitPoint.normal));
            }
        }
    }

    void Reload()
    {
        m_isReload = true;
    }

    void Attack()
    {
        m_isReload = false;
    }

    public virtual void Attack(Vector3 point, Quaternion dir, Vector3 scale)
    {

    }
}
