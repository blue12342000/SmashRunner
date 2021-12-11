using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Animator m_animator;
    Rigidbody m_rigidbody;
    CharacterController m_character;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
        //m_rigidbody = GetComponent<Rigidbody>();
        m_character = GetComponent<CharacterController>();
    }

    void Update()
    {
        m_animator.SetFloat("MoveSpeed", Input.GetAxis("Vertical") * 2);
        m_animator.SetFloat("Rotate", Input.GetAxis("Horizontal"));

        if (Input.GetButtonDown("Jump") && m_character.isGrounded)
        {
            m_animator.SetTrigger("Jump");
            //m_rigidbody.AddForce(Vector3.up * 300);
        }
    }
}
