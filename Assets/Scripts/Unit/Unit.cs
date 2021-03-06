using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IHitable
{
    void Hit();
}

[RequireComponent(typeof(Sight), typeof(CharacterController))]
public abstract class Unit : MonoBehaviour, IHitable
{
    [SerializeField]
    protected int m_attackDamage;
    [SerializeField]
    protected int m_hp;
    [SerializeField]
    protected int m_maxHp;
    protected CharacterController m_characterController;
    protected Sight m_sight;
    
    public Sight Sight { get { return m_sight; } }

    public abstract void Hit();
}
