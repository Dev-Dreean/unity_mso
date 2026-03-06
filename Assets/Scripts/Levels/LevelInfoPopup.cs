using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelInfoPopup : MonoBehaviour
{
    public static LevelInfoPopup Instance;

    [Header("Visual")]
    public GameObject painelTotal;
    public TextMeshProUGUI textoTitulo;
    public Image imagemIcone;
    public Button botaoAssistir;
    public Button botaoFechar;

    [Header("Posicionamento")]
    public float alturaOffset = 150f;
    public Camera mainCamera;

    [Header("Configuração")]
    public LevelDatabase database;

    private int nivelAtual;
    private Transform alvoParaSeguir;
    private bool estaAberto = false;
    private readonly Vector3 POSICAO_ESCONDIDA = new Vector3(-9000f, -9000f, 0f);

    public float TempoDeAbertura { get; private set; } = 0f;

    void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;

        if (painelTotal)
        {
            painelTotal.SetActive(true);
            painelTotal.transform.position = POSICAO_ESCONDIDA;
        }

        if (botaoFechar) botaoFechar.onClick.AddListener(Fechar);

        if (botaoAssistir)
        {
            botaoAssistir.onClick.RemoveAllListeners();
            botaoAssistir.onClick.AddListener(AssistirAula);
        }
    }

    void LateUpdate()
    {
        if (estaAberto && painelTotal != null) SeguirBotao();
    }

    void SeguirBotao()
    {
        if (mainCamera == null || alvoParaSeguir == null)
        {
            Fechar();
            return;
        }

        Vector3 screenPos = mainCamera.WorldToScreenPoint(alvoParaSeguir.position);
        Vector3 novaPos = screenPos + new Vector3(0, alturaOffset, 0);
        novaPos.z = 0f;
        painelTotal.transform.position = novaPos;
    }

    public void Abrir(int nivel, Transform botao)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        nivelAtual = nivel;
        alvoParaSeguir = botao;
        TempoDeAbertura = Time.time;

        if (database != null)
        {
            var (titulo, icone, cor) = database.GetInfo(nivel);
            if (textoTitulo) textoTitulo.text = titulo;
            if (imagemIcone)
            {
                imagemIcone.gameObject.SetActive(icone != null);
                if (icone != null) imagemIcone.sprite = icone;
            }
        }

        if (painelTotal)
        {
            if (!painelTotal.activeSelf) painelTotal.SetActive(true);
            estaAberto = true;
            painelTotal.transform.SetAsLastSibling();
            SeguirBotao();
        }
    }

    public void Fechar()
    {
        estaAberto = false;
        alvoParaSeguir = null;
        if (painelTotal) painelTotal.transform.position = POSICAO_ESCONDIDA;
    }

    void AssistirAula()
    {
        Debug.Log($"[POPUP] Iniciando Nível {nivelAtual}...");

        // === AQUI ESTA A SOLUÇÃO FINAL ===
        // Se for o Nível 88 (Castelo), forçamos a volta para o MapWorld (Index 1).
        // Isso age EXATAMENTE como o botão Return: sai do castelo e volta pra porta.
        if (nivelAtual == 88)
        {
            var switcher = FindFirstObjectByType<MapWorldSwitcher>();
            if (switcher != null)
            {
                switcher.Show(1); // 1 = MapWorld
            }
        }
        // Para o Nível 87 e outros, NÃO mexemos no Switcher, 
        // assim a câmera não pula e não te joga para fora.
        // =================================

        if (EnergyManager.Instance != null)
        {
            bool temEnergia = EnergyManager.Instance.ConsumirEnergia();
            if (!temEnergia) return;
        }

        var radio = FindFirstObjectByType<RadioSignal>();
        if (radio != null)
        {
            radio.SendLevelClick(nivelAtual);
        }

        Fechar();
    }
}