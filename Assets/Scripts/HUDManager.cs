using UnityEngine;
using UnityEngine.UI; 
using System.Collections;

public class HUDManager : MonoBehaviour
{
    [Header("Menu Lateral - Referências")]
    public GameObject sideMenuPanel;
    public Button btnHamburguer; 

    [Header("Menu Lateral - Ícones")]
    public Sprite iconeHamburguer; 
    public Sprite iconeFechar;     

    [Header("Configuração da Animação")]
    public RectTransform menuRect;
    public float velocidade = 10f;
    public float margemDireita = 30f;

    private bool estaAberto = false;
    private float larguraDoMenu;
    private Image imagemIconeBotao; 
    private float posicaoYFixa; // <--- NOVO: Guarda a altura que você configurou

    void Start()
    {
        if(sideMenuPanel && menuRect)
        {
            larguraDoMenu = menuRect.rect.width;
            
            // NOVO: Salva a posição Y que você definiu no editor
            posicaoYFixa = menuRect.anchoredPosition.y; 

            // Começa escondido, mas MANTENDO o Y original
            menuRect.anchoredPosition = new Vector2(larguraDoMenu, posicaoYFixa);
            sideMenuPanel.SetActive(false);
        }

        if (btnHamburguer)
        {
            imagemIconeBotao = btnHamburguer.GetComponent<Image>();
            if (imagemIconeBotao && iconeHamburguer)
            {
                imagemIconeBotao.sprite = iconeHamburguer;
            }
        }
    }

    public void AlternarMenu()
    {
        if (estaAberto) FecharMenu();
        else AbrirMenu();
    }

    public void AbrirMenu()
    {
        if(!sideMenuPanel) return;

        sideMenuPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(AnimarMovimento(-margemDireita));
        estaAberto = true;

        if (imagemIconeBotao && iconeFechar) imagemIconeBotao.sprite = iconeFechar;
    }

    public void FecharMenu()
    {
        if(!sideMenuPanel) return;

        StopAllCoroutines();
        StartCoroutine(AnimarMovimento(larguraDoMenu, true));
        estaAberto = false;

        if (imagemIconeBotao && iconeHamburguer) imagemIconeBotao.sprite = iconeHamburguer;
    }

    IEnumerator AnimarMovimento(float alvoX, bool desligarAoTerminar = false)
    {
        float atualX = menuRect.anchoredPosition.x;
        while (Mathf.Abs(atualX - alvoX) > 1f)
        {
            atualX = Mathf.Lerp(atualX, alvoX, Time.deltaTime * velocidade);
            
            // CORREÇÃO: Usa 'posicaoYFixa' em vez de 0
            menuRect.anchoredPosition = new Vector2(atualX, posicaoYFixa);
            
            yield return null;
        }
        
        menuRect.anchoredPosition = new Vector2(alvoX, posicaoYFixa);
        
        if (desligarAoTerminar) sideMenuPanel.SetActive(false);
    }
}