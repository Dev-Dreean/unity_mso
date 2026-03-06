using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManagerSimple : MonoBehaviour
{
    [Header("O que precisa travar?")]
    public Button botaoIniciar;      
    public TextMeshProUGUI textoBotao; 

    [Header("O que ligar/desligar?")]
    public GameObject objetoGameplay; 
    public Canvas canvasMenu;         

    private bool estaBloqueado = false;

    void OnEnable()
    {
        // Garante que verifica assim que a tela aparece
        VerificarEstado();
        RadioSignal.OnSessionReady += LiberarOJogo;
    }

    void OnDisable()
    {
        RadioSignal.OnSessionReady -= LiberarOJogo;
    }

    void Start()
    {
        // Configura o clique do botão apenas uma vez
        if (botaoIniciar) 
        {
            botaoIniciar.onClick.RemoveAllListeners();
            botaoIniciar.onClick.AddListener(EntrarNoJogo);
        }
        VerificarEstado();
    }

    // Força o texto a cada frame caso alguma animação esteja tentando mudar
    void Update()
    {
        if (estaBloqueado && textoBotao != null)
        {
            if (textoBotao.text != "SEM ACESSO AO JOGO")
            {
                textoBotao.text = "SEM ACESSO AO JOGO";
                textoBotao.color = Color.red;
            }
        }
    }

    void VerificarEstado()
    {
        if (RadioSignal.SessionReady)
        {
            LiberarOJogo();
        }
        else
        {
            BloquearOJogo();
        }
    }

    void BloquearOJogo()
    {
        estaBloqueado = true;
        if (botaoIniciar) botaoIniciar.interactable = false;
        
        if (textoBotao) 
        {
            textoBotao.text = "SEM ACESSO AO JOGO"; 
            textoBotao.color = Color.red;
        }
    }

    void LiberarOJogo()
    {
        estaBloqueado = false; // Para o Update de forçar texto
        
        if (botaoIniciar) botaoIniciar.interactable = true;
        
        if (textoBotao) 
        {
            textoBotao.text = "TOQUE PARA INICIAR"; 
            textoBotao.color = Color.white;
        }
    }

    void EntrarNoJogo()
    {
        if (!RadioSignal.SessionReady) return; // Segurança extra

        if (canvasMenu) canvasMenu.enabled = false;
        if (objetoGameplay) objetoGameplay.SetActive(true);
    }
}