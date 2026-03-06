using UnityEngine;
using UnityEngine.UI;

public class SimplePopup : MonoBehaviour
{
    [Header("Configuração")]
    public GameObject conteudoDoBanner;

    // APAGUEI O VOID START DAQUI! 
    // Não precisamos dele porque você já desmarca a caixinha no Inspector.

    public void Abrir()
    {
        if (conteudoDoBanner) 
        {
            conteudoDoBanner.SetActive(true);
        }
    }

    public void Fechar()
    {
        if (conteudoDoBanner) conteudoDoBanner.SetActive(false);
    }
}