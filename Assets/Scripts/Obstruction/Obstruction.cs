using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICutable
{
    void Cut(Vector3 planeNormal, Vector3 center, Material sliceMat);
}

public class Obstruction : MonoBehaviour
{

}
