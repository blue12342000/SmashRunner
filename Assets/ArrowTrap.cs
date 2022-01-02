using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    [SerializeField]
    GameObject m_arrowPrefab;
    [SerializeField]
    float m_force;
    [SerializeField]
    float m_fireDelaySecond;
    [SerializeField]
    int m_arrowCount;

    Coroutine m_handle;

    private void Start()
    {
        StartCoroutine(FireArrow());
    }

    public void Excute()
    {
        m_handle = StartCoroutine(FireArrow());
    }

    IEnumerator FireArrow()
    {
        yield return new WaitForSeconds(1.0f);

        while (m_arrowCount > 0)
        {
            --m_arrowCount;

            Arrow arrow = Instantiate(m_arrowPrefab, transform.position, transform.rotation).GetComponent<Arrow>();
            arrow.Fire(null, m_force);

            yield return new WaitForSeconds(m_fireDelaySecond);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        Gizmos.color = Color.blue;
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Gizmos.DrawLine(transform.position, hitInfo.point);
        }
        else
        {
            Gizmos.DrawRay(ray);
        }    
    }
}
