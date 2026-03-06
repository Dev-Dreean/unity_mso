#nullable enable
using UnityEngine;

public class ScalePulse : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Escala base (1,1,1 mantém a escala atual do objeto).")]
    public Vector3 baseScale = Vector3.one;

    [Tooltip("Quanto a escala varia no pico (0.05 = 5%).")]
    [Range(0f, 1f)] public float amplitude = 0.08f;

    [Tooltip("Pulsos por segundo.")]
    [Min(0.01f)] public float frequency = 1.5f;

    [Tooltip("Usar Time.unscaledDeltaTime (ignora pausas/timeScale).")]
    public bool useUnscaledTime = false;

    [Tooltip("Começar em fase aleatória para evitar sincronizar com outros.")]
    public bool randomizePhase = true;

    [Tooltip("Eixo de escala (1 = afeta, 0 = não afeta).")]
    public Vector3 axisMask = new Vector3(1, 1, 1);

    private Vector3 _initialScale;
    private float _phase;

    void Awake()
    {
        // Guarda a escala inicial para compor com a base
        _initialScale = transform.localScale;
        if (baseScale == Vector3.one) baseScale = _initialScale;
        if (randomizePhase) _phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _phase += dt * frequency * Mathf.PI * 2f;

        // Seno no range [-1,1] → mapeia pra [0,1] com 0.5*(1+sin)
        float s = 0.5f * (1f + Mathf.Sin(_phase));

        // Ease-in-out leve (suaviza começo/fim)
        s = Mathf.SmoothStep(0f, 1f, s);

        // Escala alvo: base ± amplitude
        float factor = 1f + Mathf.Lerp(-amplitude, amplitude, s);

        // Aplica apenas nos eixos desejados
        Vector3 target = new Vector3(
            baseScale.x * Mathf.Lerp(1f, factor, axisMask.x),
            baseScale.y * Mathf.Lerp(1f, factor, axisMask.y),
            baseScale.z * Mathf.Lerp(1f, factor, axisMask.z)
        );

        transform.localScale = target;
    }

    // Chamada rápida para ligar/desligar em runtime
    public void SetEnabled(bool on) => enabled = on;
}
