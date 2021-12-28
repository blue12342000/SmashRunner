using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quiver : MonoBehaviour, ITakeOut
{
    [SerializeField]
    int m_leftoverArrows;
    [SerializeField]
    List<GameObject> m_leftoverArrowObjs = new List<GameObject>();
    [SerializeField]
    GameObject m_arrowPrefab;

    public virtual bool IsEmpty { get { return m_leftoverArrows < 1; } }
    public int LeftoverArrows { get { return m_leftoverArrows; } }

    void Awake()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(m_leftoverArrowObjs.Count < m_leftoverArrows);
            m_leftoverArrowObjs.Add(child.gameObject);
        }
    }

    public bool TakeOut(out GameObject obj)
    {
        if (m_leftoverArrows < 1)
        {
            obj = null;
            return false;
        }

        obj = Instantiate(m_arrowPrefab);

        --m_leftoverArrows;
        if (m_leftoverArrows < m_leftoverArrowObjs.Count)
        {
            m_leftoverArrowObjs[m_leftoverArrows].SetActive(false);
        }
        return true;
    }
}
