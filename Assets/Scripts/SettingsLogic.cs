using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsLogic : MonoBehaviour
{
    [Header("UI References")]
    public Slider sliderMusica;
    public Slider sliderSom;
    public TMP_Dropdown dropdownIdioma;
    public GameObject painelConfig; // O FundoEscuro

    void Start()
    {
        // 1. INICIALIZAÇÃO: Pega o volume atual do AudioManager e ajusta o Slider visualmente
        if (AudioManager.Instance != null)
        {
            if (sliderMusica) sliderMusica.value = AudioManager.Instance.musicVolume;
            if (sliderSom) sliderSom.value = AudioManager.Instance.sfxVolume;
        }

        // 2. CONEXÃO: Avisa o script quando o slider mexer
        if (sliderMusica) sliderMusica.onValueChanged.AddListener(OnMusicChanged);
        if (sliderSom) sliderSom.onValueChanged.AddListener(OnSFXChanged);
        
        // Garante que o painel comece fechado se quiser
        // if (painelConfig) painelConfig.SetActive(false);
    }

    public void OnMusicChanged(float valor)
    {
        // Debug para ver se está funcionando
        // Debug.Log($"Música: {valor * 100}%"); 

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(valor);
        }
    }

    public void OnSFXChanged(float valor)
    {
        // Debug.Log($"Som: {valor * 100}%");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(valor);
        }
    }

    // Se tiver função de fechar/OK
    public void FecharConfig()
    {
        if (painelConfig) painelConfig.SetActive(false);
    }
}