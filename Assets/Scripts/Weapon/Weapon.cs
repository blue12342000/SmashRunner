using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Weapon : MonoBehaviour
{
    [SerializeField]
    protected Transform m_IKLeftHandTransform;
    [SerializeField]
    protected Transform m_IKRightHandTransform;

    public Transform IKLeftHand { get { return m_IKLeftHandTransform; } }
    public Transform IKRigtHand { get { return m_IKRightHandTransform; } }

    public abstract void Attack();
}
