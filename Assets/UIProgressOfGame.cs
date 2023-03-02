using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class UIProgressOfGame : MonoBehaviour
{
    [SerializeField]
    Slider m_slider;
    [SerializeField]
    TrailMovement m_target;

    void Start()
    {
        m_slider.maxValue = m_target.EndPosition;
    }

    void Update()
    {
        m_slider.value = m_target.CurrentPosition;
    }
}
