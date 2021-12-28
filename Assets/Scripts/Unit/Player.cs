using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit, IHitable
{
    void Awake()
    {
        m_sight = GetComponent<Sight>();
    }

    public override void Hit()
    {
        Debug.Log("Hit");
    }
}
