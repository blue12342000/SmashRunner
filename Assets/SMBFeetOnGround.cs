using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBFeetOnGround : StateMachineBehaviour
{
    [SerializeField]
    Vector3 m_leftFootPivot;
    [SerializeField]
    AnimationCurve m_leftFootWeight;

    [SerializeField]
    Vector3 m_rightFootPivot;
    [SerializeField]
    AnimationCurve m_rightFootWeight;
    [SerializeField]
    LayerMask m_groundLayer;

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.isHuman)
        {
            // Left Foot Rotation & Position
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, m_leftFootWeight.Evaluate(stateInfo.normalizedTime));
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, m_leftFootWeight.Evaluate(stateInfo.normalizedTime) * 0.5f);
            Vector3 footPoint = animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Ray ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hitPoint, 1f, m_groundLayer))
            {
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, hitPoint.point + m_leftFootPivot);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(animator.transform.forward, hitPoint.normal));
            }
            else
            {
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, animator.GetIKPosition(AvatarIKGoal.LeftFoot) + m_rightFootPivot);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(animator.transform.forward, Vector3.up));
            }

            // Right Foot Rotation & Position
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, m_rightFootWeight.Evaluate(stateInfo.normalizedTime));
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, m_rightFootWeight.Evaluate(stateInfo.normalizedTime) * 0.5f);
            footPoint = animator.GetIKPosition(AvatarIKGoal.RightFoot);
            ray = new Ray(footPoint + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hitPoint, 1f, m_groundLayer))
            {
                //Debug.LogWarning($"hitPoint.distance : {hitPoint.distance}, AvatarIKGoal.RightFoot : {Mathf.Lerp(0f, 1f, 2f - hitPoint.distance)}");
                animator.SetIKPosition(AvatarIKGoal.RightFoot, hitPoint.point + m_rightFootPivot);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(animator.transform.forward, hitPoint.normal));
            }
            else
            {
                animator.SetIKPosition(AvatarIKGoal.RightFoot, animator.GetIKPosition(AvatarIKGoal.RightFoot) + m_rightFootPivot);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(animator.transform.forward, Vector3.up));
            }
        }
    }
}
