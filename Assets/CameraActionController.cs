using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[ExecuteInEditMode]
public class CameraActionController : MonoBehaviour
{
    static readonly int m_hashCamIndex = Animator.StringToHash("CamIndex");

    [SerializeField]
    Animator m_camAnimator;
    [SerializeField]
    CinemachineVirtualCameraBase m_virtualCam;

    void Awake()
    {
        m_camAnimator = GetComponent<Animator>();
    }

    public void ChangeActionCam(int index)
    {
        m_camAnimator.SetInteger(m_hashCamIndex, index);
    }

    public void RightCamZoomInTarget(GameObject target)
    {
        StartCoroutine(ZoomInTarget(target, 3, 2));
    }

    public void LeftCamZoomInTarget(GameObject target)
    {
        StartCoroutine(ZoomInTarget(target, 3, 1));
    }

    IEnumerator ZoomInTarget(GameObject target, int from, int to)
    {
        ChangeActionCam(from);
        Transform origin = m_virtualCam.LookAt;
        m_virtualCam.LookAt = target.transform;
        yield return new WaitForSecondsRealtime(1.5f);

        m_virtualCam.LookAt = origin;
        ChangeActionCam(to);
    }
}
