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

    public void RightCanZoomInTarget(GameObject target)
    {
        StartCoroutine(ZoomInTarget(target));
    }

    IEnumerator ZoomInTarget(GameObject target)
    {
        ChangeActionCam(3);
        Transform origin = m_virtualCam.LookAt;
        m_virtualCam.LookAt = target.transform;
        yield return new WaitForSecondsRealtime(1.5f);

        m_virtualCam.LookAt = origin;
        ChangeActionCam(2);
    }
}
