using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    Player m_character;
    Vector3 m_startDragPoint;
    Vector3 m_endDragPoint;

    void Awake()
    {
        m_character = GetComponent<Player>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            m_character.Jump();
        }

        m_character.Move(Input.GetAxis("Vertical") * 3);

        if (Input.GetMouseButtonDown(0))
        {
            m_startDragPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition, 1);
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_endDragPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition, 1);

            foreach (var obj in m_character.Sight.ObjectsInSight)
            {
                if (obj == null) continue;
                obj.GetComponent<ICutable>()?.Cut(Camera.main.Cross(m_startDragPoint, m_endDragPoint), (m_startDragPoint + m_endDragPoint) * 0.5f, null);
            }
        }
    }
}
