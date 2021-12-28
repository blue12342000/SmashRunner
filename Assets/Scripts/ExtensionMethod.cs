using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethod
{
    public static Vector3 ScreenToWorldPoint(this Camera cam, Vector3 point, float far)
    {
        point.z = far;
        return cam.ScreenToWorldPoint(point);
    }

    public static Vector3 Cross(this Camera cam, Vector3 point1, Vector3 point2)
    {
        return Vector3.Cross(point1 - cam.transform.position, point2 - cam.transform.position);
    }

    public static void Initialize(this EnemyBehaviour[] behaviours, MonoBehaviour target)
    {
        for (int i = 0, e = behaviours.Length; i < e; ++i)
        {
            behaviours[i].OnInitialize(target);
        }
    }
}
