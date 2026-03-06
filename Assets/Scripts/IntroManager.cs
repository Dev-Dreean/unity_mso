using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.Video;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [Header("Visual Intro")]
    public RawImage telaDoVideo;
    public float tempoDeFadeIntro = 2.0f;

    [Header("ELEMENTOS PARA SUMIR (FADE OUT)")]
    public CanvasGroup textoStatusCG;      
    public CanvasGroup botaoConfigCG;      
    
    [Header("Botão Iniciar")]
    public Button botaoIniciar;
    public TextMeshProUGUI textoBotao; 

    [Header("Transição de Tela")]
    public CanvasGroup cortinaPreta;      
    public float velocidadeTransicao = 1.0f;

    [Header("Referências")]
    public GameObject gameplayRoot; 
    public GameObject menuRoot;
    public GameObject hudCanvas; 

    private bool acessoLiberado = false;
    private MapWorldSwitcher switcher; 

    void Start()
    {
        switcher = FindFirstObjectByType<MapWorldSwitcher>();

        if (hudCanvas != null) hudCanvas.SetActive(false);

        // Configuração Visual Inicial
        if (textoStatusCG) textoStatusCG.alpha = 1f;
        if (botaoConfigCG) botaoConfigCG.alpha = 1f;
        
        if (cortinaPreta) 
        {
            cortinaPreta.alpha = 0f;
            cortinaPreta.blocksRaycasts = false;
        }

        if (telaDoVideo) 
        {
            Color c = telaDoVideo.color;
            c.a = 0f; 
            telaDoVideo.color = c;
        }

        // --- AQUI ESTÁ A CORREÇÃO DE SEGURANÇA ---
        // 1. Começa bloqueado
        BloquearAcesso();

        // 2. Se inscreve no RadioSignal para saber quando liberar
        RadioSignal.OnSessionReady += LiberarAcessoReal;

        // 3. Se já estiver pronto (Admin rodou antes), libera
        if (RadioSignal.SessionReady) LiberarAcessoReal();

        StartCoroutine(AnimarFadeInIntro());
        // REMOVIDO: StartCoroutine(VerificarPermissaoAPI()); <- ISSO QUE LIBERAVA SOZINHO
    }

    void OnDestroy()
    {
        RadioSignal.OnSessionReady -= LiberarAcessoReal;
    }

    void BloquearAcesso()
    {
        acessoLiberado = false;
        if (botaoIniciar) botaoIniciar.interactable = false;
        
        if (textoBotao) 
        {
            textoBotao.text = "SEM ACESSO AO JOGO";
            textoBotao.color = Color.red;
        }
    }

    // Chamado somente quando o RadioSignal confirma o Token ou o Admin
    void LiberarAcessoReal()
    {
        acessoLiberado = true;
        if (botaoIniciar) botaoIniciar.interactable = true;

        if (textoBotao) 
        {
            textoBotao.text = "TOQUE PARA INICIAR";
            textoBotao.color = Color.white;
        }
    }

    // Função vinculada ao Botão na Unity
    public void AoClicarIniciar()
    {
        // Trava final de segurança
        if (!acessoLiberado || !RadioSignal.SessionReady) 
        {
            Debug.Log("Clique ignorado: Sem permissão.");
            return;
        }

        StartCoroutine(SequenciaDeTransicao());
    }

    IEnumerator SequenciaDeTransicao()
    {
        if (cortinaPreta) cortinaPreta.blocksRaycasts = true;
        if (botaoIniciar) botaoIniciar.interactable = false;

        float timer = 0f;
        while (timer < velocidadeTransicao)
        {
            timer += Time.deltaTime;
            float progresso = timer / velocidadeTransicao;
            if (cortinaPreta) cortinaPreta.alpha = Mathf.Lerp(0f, 1f, progresso);
            if (textoStatusCG) textoStatusCG.alpha = Mathf.Lerp(1f, 0f, progresso);
            if (botaoConfigCG) botaoConfigCG.alpha = Mathf.Lerp(1f, 0f, progresso);
            yield return null;
        }

        if (cortinaPreta) cortinaPreta.alpha = 1f;
        if (textoStatusCG) textoStatusCG.alpha = 0f;
        if (botaoConfigCG) botaoConfigCG.alpha = 0f;

        yield return new WaitForSeconds(0.2f);
        
        // Troca de Telas
        if (menuRoot) menuRoot.SetActive(false);
        if (gameplayRoot) gameplayRoot.SetActive(true); 
        if (switcher != null) switcher.Show(1); 
        if (hudCanvas) hudCanvas.SetActive(true);

        timer = 0f;
        while (timer < velocidadeTransicao)
        {
            timer += Time.deltaTime;
            if (cortinaPreta) cortinaPreta.alpha = Mathf.Lerp(1f, 0f, timer / velocidadeTransicao);
            yield return null;
        }

        if (cortinaPreta) 
        {
            cortinaPreta.alpha = 0f;
            cortinaPreta.blocksRaycasts = false;
        }
    }

    IEnumerator AnimarFadeInIntro()
    {
        yield return new WaitForSeconds(0.5f);
        float tempo = 0f;
        while (tempo < tempoDeFadeIntro)
        {
            tempo += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, tempo / tempoDeFadeIntro);
            if (telaDoVideo)
            {
                Color c = telaDoVideo.color;
                c.a = alpha;
                telaDoVideo.color = c;
            }
            yield return null;
        }
    }
}