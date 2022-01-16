using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ExtensionMethod
{
    // 0~1 사이의 뎁스 사이의 위치의 포인트 확인
    public static Vector3 ScreenToWorldPoint(this Camera cam, Vector3 point, float far)
    {
        point.z = far;
        return cam.ScreenToWorldPoint(point);
    }

    public static Vector3 Cross(this Camera cam, Vector3 point1, Vector3 point2)
    {
        return Vector3.Cross(point1 - cam.transform.position, point2 - cam.transform.position);
    }

    // Vector3 기준으로 RayCast
    public static Vector3 RayCast(this Vector3 origin, Vector3 dir, int layerMask)
    {
        Ray ray = new Ray(origin, dir);
        if (Physics.Raycast(ray, out RaycastHit hitinfo, float.MaxValue, layerMask))
        {
            return hitinfo.point;
        }
        return origin;
    }

    public static Vector3 Freeze(this Vector3 point, bool x, bool y, bool z)
    {
        if (x) point.x = 0;
        if (y) point.y = 0;
        if (z) point.z = 0;
        return point;
    }

    public static Quaternion Freeze(this Quaternion rotate, bool x, bool y, bool z)
    {
        if (x) rotate.x = 0;
        if (y) rotate.y = 0;
        if (z) rotate.z = 0;
        return rotate;
    }

    // List 디폴트 값으로 초기화
    public static void Resize<T>(this List<T> list, int size, T elem = default(T))
    {
        if (list.Count > size)
        {
            list.RemoveRange(size, list.Count - size);
        }
        else
        {
            if (list.Capacity < size) list.Capacity = size;
            list.AddRange(Enumerable.Repeat(elem, size - list.Count));
        }
    }

    // SMBBase 초기화
    public static void Initialize(this SMBBase[] behaviours, MonoBehaviour target)
    {
        for (int i = 0, e = behaviours.Length; i < e; ++i)
        {
            behaviours[i].OnInitialize(target);
        }
    }

    public static void SetMoveInfo(this Transform transform, MovementBase.MoveInfo moveInfo)
    {
        transform.position = moveInfo.Position;
        transform.rotation = moveInfo.Rotation;
    }
}
