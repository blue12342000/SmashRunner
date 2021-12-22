using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit, IHitable
{
    public override void Hit()
    {
        Debug.Log("Hit");
    }

    public override void Attack(Vector3 point, Quaternion dir, Vector3 scale)
    {

    }
}
