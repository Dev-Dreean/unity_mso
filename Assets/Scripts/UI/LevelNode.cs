using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class LevelNode : MonoBehaviour
{
    [Header("Config")]
    public int levelNumber = 1;

    [Header("Refs")]
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup lockOverlay;
    [SerializeField] private RadioSignal radio;

    // Variável para controle visual do arrasto
    private bool visualResetado = false; 

    public Button Button => button; 

    // Função Reset (Mantida do original para facilitar no Editor)
    void Reset()
    {
        if (!button) button = GetComponent<Button>();
        if (!lockOverlay) lockOverlay = GetComponentInChildren<CanvasGroup>(true);
        if (!radio) radio = FindRadio();
    }

    void Awake()
    {
        if (!button) button = GetComponent<Button>();
        EnsureListener();
    }

    void OnEnable()
    {
        RefreshMyStatus();
        visualResetado = false;
    }

    // Lógica do Update (Mantida para corrigir o visual ao arrastar)
    void Update()
    {
        if (MapNavigation.Instance == null) return;

        if (MapNavigation.Instance.FoiArrastado)
        {
            if (!visualResetado)
            {
                if (button.interactable)
                {
                    button.interactable = false;
                    button.interactable = true;
                }
                visualResetado = true; 
            }
        }
        else
        {
            visualResetado = false;
        }
    }

    public void RefreshMyStatus()
    {
#if UNITY_2023_1_OR_NEWER
        var manager = Object.FindFirstObjectByType<LevelManager1>();
#else
        var manager = Object.FindObjectOfType<LevelManager1>();
#endif
        if (manager != null)
        {
            bool isLocked = this.levelNumber > manager.unlockedMax;
            ApplyLocked(isLocked);
        }
    }

    void EnsureListener()
    {
        if (!button) return;
        button.onClick.RemoveListener(OnClick);
        button.onClick.AddListener(OnClick);
    }

    public void ApplyLocked(bool locked)
    {
        if (button)
        {
            button.interactable = !locked;
            var img = button.GetComponent<Image>();
            if (img) img.color = locked ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white; 
        }
        if (lockOverlay)
        {
            lockOverlay.alpha = locked ? 0.6f : 0f;
            lockOverlay.blocksRaycasts = locked;
        }
    }

    // Função Bind (Mantida caso você use para inicializar externamente)
    public void Bind(Button btn, CanvasGroup overlay, RadioSignal radioRef)
    {
        button = btn ? btn : GetComponent<Button>();
        lockOverlay = overlay;
        radio = radioRef ? radioRef : FindRadio();
        EnsureListener();
    }

    public void SetLevelNumber(int n) => levelNumber = Mathf.Max(1, n);

    public void OnClick()
    {
        // 1. TRAVA DE ARRASTO
        if (MapNavigation.Instance != null && MapNavigation.Instance.FoiArrastado) return;

        // 2. TENTA ABRIR O POPUP (PRIORIDADE)
        // Aqui ele SÓ abre a janelinha, não muda a câmera nem sai da cena.
        if (LevelInfoPopup.Instance != null)
        {
            LevelInfoPopup.Instance.Abrir(levelNumber, this.transform);
            return; 
        }

        // 3. FALLBACK (Caso não tenha popup, entra direto)
        if (!RadioSignal.SessionReady) return;
        
        if (!radio) radio = FindRadio();

        bool acessoLiberado = true;
        if (EnergyManager.Instance != null)
        {
            acessoLiberado = EnergyManager.Instance.ConsumirEnergia();
        }

        if (acessoLiberado && radio)
        {
            radio.SendLevelClick(levelNumber);
        }
    }

    // Mantendo a compatibilidade de versões da Unity
#if UNITY_2023_1_OR_NEWER
    private RadioSignal FindRadio()
    {
        var r = Object.FindFirstObjectByType<RadioSignal>();
        if (!r) r = Object.FindAnyObjectByType<RadioSignal>();
        return r;
    }
#else
    private RadioSignal FindRadio() => Object.FindObjectOfType<RadioSignal>();
#endif
}