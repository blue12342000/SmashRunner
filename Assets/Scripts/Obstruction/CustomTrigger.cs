using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class CustomTrigger : MonoBehaviour
{
    [SerializeField]
    LayerMask m_targetLayer;
    [SerializeField]
    Vector3 m_center;
    [SerializeField]
    float m_radius;

    [SerializeField]
    UnityEvent<GameObject> m_enter;
    [SerializeField]
    UnityEvent<GameObject> m_exit;

    Coroutine m_handle;

    void OnEnable()
    {
        if (m_handle != null) StopAllCoroutines();
        m_handle = StartCoroutine(EnterTargetCheck());
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_center + transform.position, m_radius);
    }

    void And(GameObject b)
    { }

    IEnumerator EnterTargetCheck()
    {
        yield return null;
        while (true)
        {
            if (Physics.SphereCast(m_center + transform.position, m_radius, transform.forward, out RaycastHit hitInfo, m_targetLayer.value))
            {
                if (m_enter != null) m_enter.Invoke(hitInfo.collider.gameObject);
                m_handle = StartCoroutine(ExitTargetCheck(hitInfo.collider));
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ExitTargetCheck(Collider target)
    {
        yield return null;
        while (true)
        {
            Vector3 point = m_center + transform.position;
            if (Vector3.Distance(point, target.ClosestPoint(point)) > m_radius)
            {
                if (m_exit != null) m_exit.Invoke(target.gameObject);
                m_handle = null;
                break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        enabled = false;
    }
}
