using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Trap : MonoBehaviour
{
    [SerializeField]
    Transform m_spikeTransform;
    [SerializeField]
    float m_speed;
    [SerializeField]
    AnimationCurve m_curve;

    Coroutine m_handle;

    private void Start()
    {
        StartCoroutine(FireSpike());
    }

    public virtual void Excute()
    {
        if (m_handle != null)
        {
            m_handle = StartCoroutine(FireSpike());
        }
    }

    IEnumerator FireSpike()
    {
        float time = 0;
        yield return new WaitForSeconds(1.0f);

        while (time < 1)
        {
            time += m_speed * Time.deltaTime;
            m_spikeTransform.transform.localPosition = m_curve.Evaluate(time) * Vector3.Lerp(Vector3.zero, Vector3.up * 3.5f, time);
            yield return null;
        }
    }
}
