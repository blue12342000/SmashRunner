using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    Unit m_character;
    Vector3 m_startDragPoint;
    Vector3 m_endDragPoint;
    bool m_isDragDrop;

    void Awake()
    {
        m_character = GetComponent<Unit>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            m_character.Jump();
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_isDragDrop = false;
            m_startDragPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition, 1);
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_isDragDrop = true;
            m_endDragPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition, 1);

            foreach (var obj in m_character.Sight.ObjectsInSight)
            {
                if (obj == null) continue;
                obj.GetComponent<ICutable>()?.Cut(Camera.main.Cross(m_startDragPoint, m_endDragPoint), (m_startDragPoint + m_endDragPoint) * 0.5f, null);
            }
        }
    }
}
