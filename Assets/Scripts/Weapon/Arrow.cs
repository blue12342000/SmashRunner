using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour, ICutable
{
    public void Cut(Vector3 planeNormal, Vector3 center, Material sliceMat)
    {
        //GameObject[] subObjects = MeshSlice.SliceMesh(gameObject, planeNormal, center, sliceMat);
        //if (subObjects != null)
        //{
        //    foreach (var obj in subObjects)
        //    {
        //        Destroy(obj, 4.0f);
        //    }
        //}
    }
}
