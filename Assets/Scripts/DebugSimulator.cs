using UnityEngine;
using UnityEngine.UI;
using TMPro; // Se estiver usando TextMeshPro
using System.Collections;

public class DebugSimulator : MonoBehaviour
{
    public static DebugSimulator Instance;

    [Header("UI do Botão Debug")]
    public Button btnToggle;        // Arraste seu botão aqui
    public TextMeshProUGUI txtStatus; // Arraste o texto do botão aqui
    public Image imgFundo;          // Arraste a imagem do botão aqui

    [Header("Configurações")]
    public bool simulacaoAtiva = false; // Começa desligado por padrão?
    public float tempoDeResposta = 0.5f;
    public int nivelInicial = 1;

    private RadioSignal radio;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        // Se inscreve para escutar tudo que o jogo manda pro rádio
        RadioSignal.OnEditorSpy += EspiaoDeMensagens;
    }

    void OnDisable()
    {
        RadioSignal.OnEditorSpy -= EspiaoDeMensagens;
    }

    void Start()
    {
        radio = FindFirstObjectByType<RadioSignal>();

        // Configura o clique do botão visual
        if (btnToggle != null)
        {
            btnToggle.onClick.AddListener(AlternarModo);
            AtualizarVisual(); // Já ajusta a cor no começo
        }

        // Se já começar ativo, simula o Login
        if (simulacaoAtiva) StartCoroutine(SimularLoginInicial());
    }

    // Chamado quando você clica no botão da tela
    public void AlternarModo()
    {
        simulacaoAtiva = !simulacaoAtiva;
        AtualizarVisual();
        
        Debug.Log($"[DebugSimulator] Modo Simulação alterado para: {simulacaoAtiva}");
    }

    void AtualizarVisual()
    {
        if (txtStatus) txtStatus.text = simulacaoAtiva ? "DEBUG: ON" : "DEBUG: OFF";
        if (imgFundo) imgFundo.color = simulacaoAtiva ? Color.green : Color.red;
    }

    // --- LÓGICA DE INTERCEPTAÇÃO ---
    private void EspiaoDeMensagens(string json)
    {
        // SE O DEBUG ESTIVER OFF, IGNORA TUDO!
        // Deixa o RadioSignal falar com a API real do Marcelo.
        if (!simulacaoAtiva) return;

        // SE O DEBUG ESTIVER ON, SIMULA A RESPOSTA:
        if (json.Contains("\"type\":\"level_click\""))
        {
            int nivelClicado = ExtrairNivelDoJson(json);
            StartCoroutine(SimularConclusaoNivel(nivelClicado));
        }
    }

    IEnumerator SimularLoginInicial()
    {
        if (!radio) yield break;
        yield return new WaitForSeconds(tempoDeResposta);
        
        // Simula resposta do Init
        string jsonInit = "{\"type\": \"init\", \"user_uuid\": \"DEV_MODE\", \"user_name\": \"Debug User\"}";
        radio.OnJsMessage(jsonInit);

        // Simula nível atual
        AtualizarNivel(nivelInicial);
    }

    IEnumerator SimularConclusaoNivel(int nivelClicado)
    {
        // Finge que está pensando (Lag da internet)
        yield return new WaitForSeconds(tempoDeResposta);

        // Libera o próximo
        int proximo = nivelClicado + 1;
        Debug.Log($"<color=green>[DebugSimulator] (Fake API) Liberando Nível {proximo}</color>");
        
        AtualizarNivel(proximo);
    }

    private void AtualizarNivel(int nivel)
    {
        if (!radio) return;
        // Manda a resposta JSON fingindo ser a API
        string jsonLevel = $"{{\"type\": \"current_level\", \"data\": {{ \"current_level\": {nivel} }} }}";
        radio.OnJsMessage(jsonLevel);
    }

    private int ExtrairNivelDoJson(string json)
    {
        try {
            // Gambiarra rápida pra ler JSON sem criar classe extra
            // Procura por "level": X
            string busca = "\"level\":";
            int index = json.IndexOf(busca);
            if (index != -1)
            {
                string cortado = json.Substring(index + busca.Length);
                string numero = cortado.Split(',')[0].Split('}')[0].Trim();
                return int.Parse(numero);
            }
            return 1;
        } catch { return 1; }
    }
}