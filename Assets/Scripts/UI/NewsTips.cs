using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class NewsTips : MonoBehaviour
{
    public Text Content;

    [Header("Toast Settings")]
    [Tooltip("How far to move upward during the show animation (in px)")]
    public float moveDistance = 80f;

    [Tooltip("Initial vertical offset below the target position (in px)")]
    public float startYOffset = -20f;

    [Tooltip("Fade-in duration (seconds)")]
    public float fadeInDuration = 0.25f;

    [Tooltip("Stay visible duration before fade-out (seconds)")]
    public float holdDuration = 7f;

    [Tooltip("Fade-out duration (seconds)")]
    public float fadeOutDuration = 0.3f;
    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private Sequence _seq;
    private Vector2 _baseAnchoredPos;

    void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (_rect == null)
        {
            Debug.LogError("NewsTips requires a RectTransform (UI element).");
        }
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        _baseAnchoredPos = _rect != null ? _rect.anchoredPosition : Vector2.zero;
        gameObject.SetActive(false);
    }

    public void Show(string content)
    {
        if (Content != null)
            Content.text = content;

        // Kill previous sequence if any
        if (_seq != null && _seq.IsActive())
        {
            _seq.Kill();
        }

        gameObject.SetActive(true);

        // Prepare start state
        if (_rect != null)
        {
            _baseAnchoredPos = _rect.anchoredPosition; // current target pos defined by layout
            _rect.anchoredPosition = _baseAnchoredPos + new Vector2(0, startYOffset);
        }
        _canvasGroup.alpha = 0f;

        // Build sequence: move up + fade in, hold, then fade out
        _seq = DOTween.Sequence();
        _seq.Append(_rect.DOAnchorPos(_baseAnchoredPos + new Vector2(0, moveDistance), fadeInDuration).SetEase(Ease.OutQuad));
        _seq.Join(_canvasGroup.DOFade(1f, fadeInDuration));
        _seq.AppendInterval(holdDuration);
        _seq.Append(_canvasGroup.DOFade(0f, fadeOutDuration));
        _seq.OnComplete(() => { Hide(); });
    }

    public void Hide()
    {
        Debug.Log("Hide");
        DestroyImmediate(this.gameObject);
        /*
        if (_seq != null && _seq.IsActive())
        {
            _seq.Kill();
        }
        // Quick fade out
        if (gameObject.activeSelf)
        {
            _canvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() =>
            {
                gameObject.SetActive(false);
                ResetPosition();
            });
        }
        else
        {
            ResetPosition();
        }
        */
    }

    private void ResetPosition()
    {
        if (_rect != null)
        {
            _rect.anchoredPosition = _baseAnchoredPos;
        }
        _canvasGroup.alpha = 0f;
    }
}
