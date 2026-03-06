using UnityEngine;
using System.Collections.Generic;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance;

    [Header("Configurações Gerais")]
    public Transform cameraTransform; 
    public AudioClip musicaMenu;      
    public AudioClip musicaCastelo;   

    [Header("Músicas da Gameplay (Biomas)")]
    public List<BiomeConfig> biomas;

    [System.Serializable]
    public class BiomeConfig
    {
        public string nome;
        public float alturaMinimaY; 
        public AudioClip musica;
    }

    private int estadoAtual = -1;
    private AudioClip ultimaMusicaSolicitada;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Só monitora na Gameplay (Estado 1)
        if (estadoAtual == 1 && cameraTransform != null)
        {
            ChecarAlturaEBioma();
        }
    }

    public void TrocarEstado(int novoIndex)
    {
        Debug.Log($"[MUSIC] Trocando Estado para Index: {novoIndex}"); // LOG DE DIAGNÓSTICO
        estadoAtual = novoIndex;

        switch (novoIndex)
        {
            case 0: // MENU
                Tocar(musicaMenu, "Menu");
                break;
            case 1: // GAMEPLAY
                ChecarAlturaEBioma(true); // Força verificação imediata
                break;
            case 2: // CASTELO
                Tocar(musicaCastelo, "Castelo");
                break;
        }
    }

    void ChecarAlturaEBioma(bool forcar = false)
    {
        if (biomas.Count == 0) return;

        float yCam = cameraTransform.position.y;
        
        // Começa assumindo o primeiro bioma (fundo)
        AudioClip clipParaTocar = biomas[0].musica; 
        string nomeBioma = biomas[0].nome;

        // Procura o bioma mais alto que a câmera alcançou
        foreach (var b in biomas)
        {
            if (yCam >= b.alturaMinimaY)
            {
                clipParaTocar = b.musica;
                nomeBioma = b.nome;
            }
        }

        // Se forçamos (entrada na cena) ou se a música mudou
        if (clipParaTocar != ultimaMusicaSolicitada || forcar)
        {
            Debug.Log($"[MUSIC] Altura Câmera: {yCam}. Bioma Detectado: {nomeBioma}");
            Tocar(clipParaTocar, nomeBioma);
        }
    }

    void Tocar(AudioClip clip, string nome)
    {
        if (clip == null)
        {
            Debug.LogError($"[MUSIC] Tentou tocar {nome}, mas o AudioClip está VAZIO/NULL!");
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogError("[MUSIC] AudioManager não encontrado na cena!");
            return;
        }

        Debug.Log($"[MUSIC] Enviando para AudioManager: {nome}");
        ultimaMusicaSolicitada = clip;
        AudioManager.Instance.PlayMusic(clip);
    }
}