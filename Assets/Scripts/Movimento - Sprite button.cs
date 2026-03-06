#nullable enable
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIButtonFlipbook : MonoBehaviour
{
    public enum LoopMode { Loop, PingPong, Once }

    [Header("Frames (ordem da animação)")]
    public Sprite[] frames = default!;

    [Header("Playback")]
    [Min(1e-3f)] public float framesPerSecond = 12f;
    public LoopMode loopMode = LoopMode.Loop;
    public bool playOnStart = true;
    public bool useUnscaledTime = true;

    [Header("Controle opcional")]
    public bool pauseOnDisable = true;   // pausa se o GO for desativado
    public bool randomizeStartFrame = true;

    private Image _img = default!;
    private int _index;
    private int _dir = 1;
    private float _accum;
    private bool _playing;

    void Awake()
    {
        _img = GetComponent<Image>();
        if (frames is { Length: > 0 })
        {
            if (randomizeStartFrame)
                _index = Random.Range(0, frames.Length);
            _img.sprite = frames[_index];
        }
    }

    void OnEnable()
    {
        if (playOnStart) _playing = true;
    }

    void OnDisable()
    {
        if (pauseOnDisable) _playing = false;
    }

    void Update()
    {
        if (!_playing || frames is not { Length: > 1 }) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _accum += dt;

        float frameTime = 1f / framesPerSecond;
        while (_accum >= frameTime)
        {
            _accum -= frameTime;
            Step();
        }
    }

    private void Step()
    {
        switch (loopMode)
        {
            case LoopMode.Loop:
                _index = (_index + 1) % frames.Length;
                break;

            case LoopMode.PingPong:
                _index += _dir;
                if (_index >= frames.Length)
                {
                    _index = frames.Length - 2;
                    _dir = -1;
                }
                else if (_index < 0)
                {
                    _index = 1;
                    _dir = 1;
                }
                break;

            case LoopMode.Once:
                if (_index < frames.Length - 1) _index++;
                else { _playing = false; return; }
                break;
        }

        _img.sprite = frames[_index];
    }

    // Controles públicos (se quiser usar via código/OnClick)
    public void Play() => _playing = true;
    public void Pause() => _playing = false;
    public void SetFPS(float fps) => framesPerSecond = Mathf.Max(1e-3f, fps);
    public void SetFrame(int i)
    {
        if (frames is not { Length: > 0 }) return;
        _index = Mathf.Clamp(i, 0, frames.Length - 1);
        _img.sprite = frames[_index];
    }
}
