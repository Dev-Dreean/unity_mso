using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonResizer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [Header("Configuração de Tamanhos (Padrão Níveis)")]
    // Defini os valores exatos aqui. Se você resetar o componente, ele volta para estes números.
    public Vector2 tamanhoNormal = new Vector2(54.34f, 59.08f); 
    public Vector2 tamanhoGrande = new Vector2(70.22f, 76.34f);

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Garante que o botão comece no tamanho certo ao dar Play
        if(rectTransform) rectTransform.sizeDelta = tamanhoNormal;
    }

    // Pressionado (Segurando o clique)
    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.sizeDelta = tamanhoGrande;
    }

    // Soltou o clique
    public void OnPointerUp(PointerEventData eventData)
    {
        // Mantém a lógica de seleção do Unity (se soltar, geralmente fica selecionado)
    }

    // Selecionado (Foco / Navegação)
    public void OnSelect(BaseEventData eventData)
    {
        rectTransform.sizeDelta = tamanhoGrande;
    }

    // Deselecionado (Clicou fora)
    public void OnDeselect(BaseEventData eventData)
    {
        rectTransform.sizeDelta = tamanhoNormal;
    }
}