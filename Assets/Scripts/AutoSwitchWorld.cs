using UnityEngine;

public class AutoSwitchWorld : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("Nível que dispara a saída")]
    public int nivelGatilho = 89; 

    [Tooltip("Mundo destino (0 = Floresta)")]
    public int irParaMundo = 0; 

    [Header("Posição Exata da Câmera")]
    public float overrideCameraX = -1f; // <--- O VALOR FIXO AQUI
    public float overrideCameraY = 337.0222f;

    [Header("Referências")]
    public MapWorldSwitcher switcher;

    void Start()
    {
        if (!switcher) switcher = FindFirstObjectByType<MapWorldSwitcher>();
    }

    void OnEnable()
    {
        RadioSignal.OnCurrentLevel += VerificarNivel;
    }

    void OnDisable()
    {
        RadioSignal.OnCurrentLevel -= VerificarNivel;
    }

    void VerificarNivel(int novoNivel)
    {
        if (novoNivel == nivelGatilho)
        {
            Debug.Log($"[AutoSwitch] Nível {novoNivel}! Saindo para X={overrideCameraX}, Y={overrideCameraY}...");
            
            if (switcher) 
            {
                // Passa o X (-1) e o Y (337...) para travar a câmera perfeita
                switcher.ShowWithOverride(irParaMundo, overrideCameraX, overrideCameraY);
            }
        }
    }
}