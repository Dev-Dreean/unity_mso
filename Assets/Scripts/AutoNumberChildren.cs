using UnityEngine;
using TMPro; // Importante para mexer no texto

public class AutoNumberChildren : MonoBehaviour
{
    [Header("Configuração")]
    public int numeroInicial = 1;      // Em qual número começa essa pasta? (Ex: 87)
    public string prefixoNome = "Level_"; // Como vai ser o nome na Hierarquia?

    [ContextMenu("RENOMEAR E NUMERAR AGORA")] // Cria um botão no menu de contexto
    public void ExecutarRenomeacao()
    {
        int contador = numeroInicial;

        // Passa por CADA filho dentro deste objeto (na ordem da lista)
        foreach (Transform child in transform)
        {
            // 1. Renomeia o Objeto na Hierarquia (Ex: Level_87)
            child.name = prefixoNome + contador;

            // 2. Procura o componente de texto lá dentro e muda o número visual
            TextMeshProUGUI textoDoBotao = child.GetComponentInChildren<TextMeshProUGUI>();
            
            if (textoDoBotao != null)
            {
                textoDoBotao.text = contador.ToString();
            }
            else
            {
                // Se não achar TMP, tenta o Text legado (caso use)
                UnityEngine.UI.Text textoLegado = child.GetComponentInChildren<UnityEngine.UI.Text>();
                if (textoLegado) textoLegado.text = contador.ToString();
            }

            // Prepara para o próximo
            contador++;
        }
        
        Debug.Log($"✅ Sucesso! {transform.childCount} botões renomeados de {numeroInicial} até {contador - 1}.");
    }
}