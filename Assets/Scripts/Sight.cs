using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    private class MoreClose : IComparer<GameObject>
    {
        public Vector3 Point;

        public int Compare(GameObject x, GameObject y)
        {
            return Vector3.Distance(Point, x.transform.position) < Vector3.Distance(Point, y.transform.position)?-1:1;
        }
    }

    [SerializeField]
    Vector3 m_pivot;
    [SerializeField]
    float m_range;
    [SerializeField]
    float m_fov;
    [SerializeField]
    [Range(0.1f, 1)]
    float m_pricision = 0.1f;
    [SerializeField]
    LayerMask m_alertLayer;
    [SerializeField]
    LayerMask m_blockLayer;

    MoreClose m_compareMoreClose = new MoreClose();
    SortedSet<GameObject> m_objectsInSight = new SortedSet<GameObject>();
    SortedSet<GameObject> m_objectsOutSight = new SortedSet<GameObject>();

    Coroutine m_checkSightHandle;

    float LeftSeightAngle => (transform.eulerAngles.y - m_fov * 0.5f) * Mathf.Deg2Rad;
    float RightSeightAngle => (transform.eulerAngles.y + m_fov * 0.5f) * Mathf.Deg2Rad;
    Vector3 LeftSightVector => new Vector3(Mathf.Sin(LeftSeightAngle), 0, Mathf.Cos(LeftSeightAngle)) * m_range;
    Vector3 RightSightVector => new Vector3(Mathf.Sin(RightSeightAngle), 0, Mathf.Cos(RightSeightAngle)) * m_range;
    


    public GameObject ObjectInSight => m_objectsInSight.Count > 0 ? m_objectsInSight.Min : null;
    public IReadOnlyCollection<GameObject> ObjectsInSight => m_objectsInSight;
    public GameObject ObjectOutSight => m_objectsOutSight.Count > 0 ? m_objectsOutSight.Min : null;
    public IReadOnlyCollection<GameObject> ObjectsOutSight => m_objectsOutSight;
    public bool IsEmptyInSight => m_objectsInSight.Count == 0;
    public bool IsEmptyOutSight => m_objectsOutSight.Count == 0;
    public bool IsEmptyInRange => IsEmptyInSight || IsEmptyInRange;
    public bool IsExistInSight => m_objectsInSight.Count > 0;
    public bool IsExistOutSight => m_objectsOutSight.Count > 0;
    public bool IsExistInRange => IsExistInSight || IsExistOutSight;

    List<Vector3> debugSearchList = new List<Vector3>();
    List<Vector3> debugShortList = new List<Vector3>();

    void Awake()
    {
        m_objectsInSight = new SortedSet<GameObject>(m_compareMoreClose);
        m_objectsOutSight = new SortedSet<GameObject>(m_compareMoreClose);
    }

    void OnDisable()
    {
        m_objectsInSight.Clear();
        m_objectsOutSight.Clear();
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
        Gizmos.DrawWireSphere(transform.position + m_pivot, m_range);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + m_pivot, transform.position + m_pivot + LeftSightVector);
        Gizmos.DrawLine(transform.position + m_pivot, transform.position + m_pivot + RightSightVector);

        Gizmos.color = Color.blue;
        foreach (var target in m_objectsInSight)
        {
            Gizmos.DrawLine(transform.position + m_pivot, target.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Vector3 size = Vector3.one * 0.04f;
        foreach (Vector3 v in debugSearchList)
        {
            Gizmos.DrawLine(transform.position + m_pivot, v);
            Gizmos.DrawCube(v, size);
        }

        Gizmos.color = Color.blue;
        foreach (Vector3 v in debugShortList)
        {
            Gizmos.DrawCube(v, size);
        }
    }

    IEnumerator CheckSight()
    {
        float targetCos;
        Ray ray = new Ray();
        float tempDistance;
        RaycastHit hitInfo;

        while (true)
        {
            m_objectsInSight.Clear();
            m_objectsOutSight.Clear();
            debugSearchList.Clear();
            debugShortList.Clear();

            m_compareMoreClose.Point = transform.position;
            ray.origin = transform.position + m_pivot;
            foreach (var collider in Physics.OverlapSphere(transform.position + m_pivot, m_range, m_alertLayer))
            {
                if (collider.gameObject == gameObject) continue;

                // confirm between origin point and target Point
                bool isCanSee = false;
                bool isInSight = false;

                int targetLayer = 1 << collider.gameObject.layer;
                Vector3 colliderDir = (collider.bounds.center - (transform.position + m_pivot)).normalized;
                Vector3 projOrigin = collider.bounds.center + colliderDir * Vector3.Project(collider.bounds.extents, (collider.bounds.center - transform.position).normalized).magnitude;

                ray.direction = projOrigin - ray.origin;
                tempDistance = (projOrigin - ray.origin).magnitude;
                if (Physics.Raycast(ray, out hitInfo, tempDistance, m_blockLayer.value | targetLayer))
                {
                    debugShortList.Add(hitInfo.point);
                    isCanSee = hitInfo.collider == collider;
                }

                Vector3 projUpVector = Vector3.Project(collider.bounds.extents, transform.up);
                Vector3 projRightVector = Vector3.Project(collider.bounds.extents, Vector3.Cross(colliderDir, transform.up));
                Vector3 projUpDir = projUpVector.normalized;
                Vector3 projRightDir = projRightVector.normalized;
                int projVerticlScale = Mathf.CeilToInt(projUpVector.magnitude / m_pricision);
                int projHorizontalScale = Mathf.CeilToInt(projRightVector.magnitude / m_pricision);

                debugSearchList.Add(projOrigin);
                for (int v = 1; v < projVerticlScale; ++v)
                {
                    debugSearchList.Add(projOrigin + projUpDir * v * m_pricision);
                    debugSearchList.Add(projOrigin - projUpDir * v * m_pricision);
                }

                for (int h = 1; h < projHorizontalScale; ++h)
                {
                    debugSearchList.Add(projOrigin + projRightDir * h * m_pricision);
                    debugSearchList.Add(projOrigin - projRightDir * h * m_pricision);
                }

                // Check Vertical Line
                if (!isCanSee)
                {
                    for (int v = 1; v < projVerticlScale; ++v)
                    {
                        ray.direction = projOrigin + projUpDir * v * m_pricision - ray.origin;
                        tempDistance = (projOrigin + projUpDir * v * m_pricision - ray.origin).magnitude;
                        if (Physics.Raycast(ray, out hitInfo, tempDistance, m_blockLayer.value | targetLayer))
                        {
                            debugShortList.Add(hitInfo.point);
                            isCanSee = hitInfo.collider == collider;
                        }

                        if (isCanSee) break;

                        ray.direction = projOrigin - projUpDir * v * m_pricision - ray.origin;
                        tempDistance = (projOrigin - projUpDir * v * m_pricision - ray.origin).magnitude;
                        if (Physics.Raycast(ray, out hitInfo, tempDistance, m_blockLayer.value | targetLayer))
                        {
                            debugShortList.Add(hitInfo.point);
                            isCanSee = hitInfo.collider == collider;
                        }

                        if (isCanSee) break;
                    }
                }
                // Check Hrizontal Line
                if (!isCanSee)
                {
                    for (int h = 1; h < projHorizontalScale; ++h)
                    {
                        ray.direction = projOrigin + projRightDir * h * 0.1f - ray.origin;
                        tempDistance = (projOrigin + projRightDir * h * 0.1f - ray.origin).magnitude;
                        if (Physics.Raycast(ray, out hitInfo, tempDistance, m_blockLayer.value | targetLayer))
                        {
                            debugShortList.Add(hitInfo.point);
                            isCanSee = hitInfo.collider == collider;
                        }

                        if (isCanSee) break;

                        ray.direction = projOrigin - projRightDir * h * 0.1f - ray.origin;
                        tempDistance = (projOrigin - projRightDir * h * 0.1f - ray.origin).magnitude;
                        if (Physics.Raycast(ray, out hitInfo, tempDistance, m_blockLayer.value | targetLayer))
                        {
                            debugShortList.Add(hitInfo.point);
                            isCanSee = hitInfo.collider == collider;
                        }

                        if (isCanSee) break;
                    }
                }

                if (!isCanSee) continue;

                if (isInSight) m_objectsInSight.Add(collider.gameObject);
                else m_objectsOutSight.Add(collider.gameObject);

                // Valid Object In Range
                //targetCos = Vector3.Dot(transform.forward, (collider.transform.position - transform.position).normalized);
                //if (targetCos > 0 && Mathf.Acos(targetCos) < m_fov * 0.5f * Mathf.Deg2Rad)
                //{
                //    // In Sight
                //    m_objectsInSight.Add(collider.gameObject);
                //}
                //else
                //{
                //    // Out Sight
                //    m_objectsOutSight.Add(collider.gameObject);
                //}
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
