using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour, ICutable
{
    float m_force;
    Transform m_target;
    MeshCollider[] m_childColliders;

    void Awake()
    {
        m_childColliders = GetComponentsInChildren<MeshCollider>();
    }

    void FixedUpdate()
    {
        if (m_target)
        {
            transform.LookAt(m_target.position + Vector3.up);
        }
        transform.Translate(Vector3.forward * m_force * Time.fixedDeltaTime);
    }

    public void Fire(Transform target, float force)
    {
        m_target = target;
        m_force = force;
    }

    public void Cut(Vector3 planeNormal, Vector3 center, Material sliceMat)
    {
        if (m_childColliders == null) return;
        foreach (Collider collider in m_childColliders)
        {
            GameObject[] subObjects = MeshSlice.SliceMesh(collider.gameObject, planeNormal, center, sliceMat);
            if (subObjects != null)
            {
                m_force = 0;
                m_target = null;
                Destroy(this, 4.0f);
            }
        }
    }
}
