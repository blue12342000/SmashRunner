using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBRun : SMBBase
{
    IMovement m_movement;

    public override void OnInitialize(MonoBehaviour target)
    {

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //m_movement.Movement.Move(animator.transform.forward, animator.speed * Time.deltaTime);
    }
}
