using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    [SerializeField]
    float m_range;
    [SerializeField]
    float m_fov;
    [SerializeField]
    LayerMask m_layerMask;

    [SerializeField]
    List<GameObject> m_objectsInSight = new List<GameObject>();
    HashSet<GameObject> m_outOfSight = new HashSet<GameObject>();
    Coroutine m_checkSightHandle;

    float LeftSeightAngle { get { return (transform.eulerAngles.y - m_fov * 0.5f) * Mathf.Deg2Rad; } }
    float RightSeightAngle { get { return (transform.eulerAngles.y + m_fov * 0.5f) * Mathf.Deg2Rad; } }
    Vector3 LeftSightVector { get { return new Vector3(Mathf.Sin(LeftSeightAngle), 0, Mathf.Cos(LeftSeightAngle)) * m_range; } }
    Vector3 RightSightVector { get { return new Vector3(Mathf.Sin(RightSeightAngle), 0, Mathf.Cos(RightSeightAngle)) * m_range; } }
    public GameObject[] ObjectsInSight { get { return m_objectsInSight.ToArray(); } }
    public HashSet<GameObject> OutOfSight { get { return m_outOfSight; } }
    public bool IsEmpty { get { return m_objectsInSight.Count == 0; } }

    void OnDisable()
    {
        ResetData();
        StopCoroutine(m_checkSightHandle);
        m_checkSightHandle = null;
    }

    void OnEnable()
    {
        m_checkSightHandle = StartCoroutine(CheckSight());
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up, m_range);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + LeftSightVector);
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + RightSightVector);

        Gizmos.color = Color.blue;
        foreach (var target in m_objectsInSight)
        {
            Gizmos.DrawLine(transform.position + Vector3.up, target.transform.position + Vector3.up);
        }
    }

    void ResetData()
    {
        m_objectsInSight.Clear();
        m_outOfSight.Clear();
    }

    IEnumerator CheckSight()
    {
        ResetData();

        float targetCos;
        while (true)
        {
            m_outOfSight.Clear();
            foreach (var collider in m_objectsInSight)
            {
                m_outOfSight.Add(collider.gameObject);
            }
            m_objectsInSight.Clear();
            foreach (var collider in Physics.OverlapSphere(transform.position, m_range, m_layerMask))
            {
                if (collider.gameObject == gameObject) continue;

                targetCos = Vector3.Dot(transform.forward, (collider.transform.position - transform.position).normalized);
                if (targetCos > 0 && Mathf.Acos(targetCos) < m_fov * 0.5f * Mathf.Deg2Rad)
                {
                    m_objectsInSight.Add(collider.gameObject);
                    if (m_outOfSight.Contains(collider.gameObject)) { m_outOfSight.Remove(collider.gameObject); }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
