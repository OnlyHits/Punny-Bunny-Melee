using UnityEngine;
using DG.Tweening;
using Unity.Cinemachine;
using System;

public class ArenaCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera m_virtualCamera;

    [SerializeField] private Vector3 m_startAnimTarget;
    [SerializeField] private float m_startAnimDuration = 1f;

    public CinemachineCamera Camera { get => m_virtualCamera; protected set { } }

    private CinemachinePositionComposer m_composer;

    /// <summary>
    /// Plays the offset animation and calls the provided callback when complete.
    /// </summary>
    /// <param name="onComplete">Callback to invoke after the tween finishes.</param>
    public void PlayStartAnim(Action onComplete)
    {
        m_composer = m_virtualCamera.GetComponent<CinemachinePositionComposer>();

        DOTween.To(
            () => m_composer.TargetOffset,
            value => m_composer.TargetOffset = value,
            m_startAnimTarget,
            m_startAnimDuration
        )
        .SetEase(Ease.InOutQuad)
        .OnComplete(() => onComplete?.Invoke());
    }
}
