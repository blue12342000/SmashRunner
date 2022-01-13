using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class CustomTrigger : MonoBehaviour
{
    enum TriggerState
    {
        Ready,
        OnEnter,
        OnExit,
        Terminated
    }

    [SerializeField]
    TriggerState m_state = TriggerState.Ready;
    [SerializeField]
    LayerMask m_targetLayer;
    [SerializeField]
    Vector3 m_center;
    [SerializeField]
    float m_radius;

    [SerializeField]
    UnityEvent m_enter;
    [SerializeField]
    UnityEvent m_exit;

    Coroutine m_handle;

    void OnEnable()
    {
        if (m_handle != null) StopAllCoroutines();
        m_handle = StartCoroutine(TargetCheck());
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_center + transform.position, m_radius);
    }

    IEnumerator TargetCheck()
    {
        if (m_state == TriggerState.Ready) m_state = TriggerState.OnEnter;
        yield return null;

        while (m_state != TriggerState.Terminated)
        {
            switch (m_state)
            {
                case TriggerState.OnEnter:
                    if (Physics.CheckSphere(m_center + transform.position, m_radius, m_targetLayer.value))
                    {
                        if (m_enter != null) m_enter.Invoke();
                        m_state = TriggerState.OnExit;
                    }
                    break;
                case TriggerState.OnExit:
                    if (!Physics.CheckSphere(m_center + transform.position, m_radius, m_targetLayer.value))
                    {
                        if (m_exit != null) m_exit.Invoke();
                        m_state = TriggerState.Terminated;
                    }
                    break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
