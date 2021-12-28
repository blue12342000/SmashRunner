using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : Obstruction, ICutable
{
    public void Cut(Vector3 planeNormal, Vector3 center, Material sliceMat)
    {
        if (!enabled) return;

        GameObject[] subObjects = MeshSlice.SliceMesh(gameObject, planeNormal, center, sliceMat);
        if (subObjects != null)
        {
            foreach (var obj in subObjects)
            {
                if (obj.GetComponent<MeshFilter>().mesh.bounds.size.magnitude < 5)
                {
                    obj.GetComponent<Obstruction>().enabled = false;
                    Destroy(obj, 4.0f);
                }
            }
        }
    }
}
