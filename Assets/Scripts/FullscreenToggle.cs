#nullable enable
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class FullscreenToggle : MonoBehaviour
{
    public Sprite? enterFullscreenSprite;
    public Sprite? exitFullscreenSprite;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void UnityFS_toggle();
    [DllImport("__Internal")] private static extern int  UnityFS_isFullscreen();
    [DllImport("__Internal")] private static extern void UnityFS_fullFitEnable(int enable);
#else
    private static void UnityFS_toggle() { }
    private static int UnityFS_isFullscreen() => 0;
    private static void UnityFS_fullFitEnable(int enable) { }
#endif

    private Button _btn = default!;
    private Image _img = default!;
    private float _tick;

    public float pollInterval = 0.5f;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _img = GetComponent<Image>();
        _btn.onClick.AddListener(OnClickToggle);
    }

    void Start() => RefreshIcon();

    void Update()
    {
        _tick += Time.unscaledDeltaTime;
        if (_tick >= pollInterval) { _tick = 0f; RefreshIcon(); }
    }

    private void OnClickToggle()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityFS_toggle();
        // habilita full-fit somente se realmente entrou
        Invoke(nameof(AfterToggle), 0.1f);
#endif
    }

    private void AfterToggle()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        bool fs = UnityFS_isFullscreen() == 1;
        UnityFS_fullFitEnable(fs ? 1 : 0);
#endif
        RefreshIcon();
    }

    private void RefreshIcon()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        bool fs = UnityFS_isFullscreen() == 1;
        if (fs && exitFullscreenSprite) _img.sprite = exitFullscreenSprite;
        else if (!fs && enterFullscreenSprite) _img.sprite = enterFullscreenSprite;
#endif
    }
}
