using UnityEngine;

public class ExternalAdmin : MonoBehaviour
{
    // Função chamada pelo botão "Set Nível" do HTML
    public void JS_SetLevel(string nivelStr)
    {
        if (int.TryParse(nivelStr, out int nivel))
        {
            // Simula a mensagem que viria da API
            string jsonFake = $"{{\"type\":\"current_level\",\"data\":{{\"current_level\":{nivel}}}}}";

            // Manda para o RadioSignal como se fosse a API real
            // CORREÇÃO DO AVISO AMARELO: Usar FindFirstObjectByType em vez de FindObjectOfType
            if (FindFirstObjectByType<RadioSignal>())
                FindFirstObjectByType<RadioSignal>().OnJsMessage(jsonFake);

            Debug.Log($"<color=cyan>[ADMIN] Forçando Nível: {nivel}</color>");
        }
    }

    // Função chamada pelo botão "Ativar Premium" do HTML
    public void JS_SetPremium(string statusStr)
    {
        // Aceita "true", "True", "1" como verdadeiro
        bool isPremium = (statusStr.ToLower() == "true" || statusStr == "1");

        if (EnergyManager.Instance)
        {
            // Mantém a energia atual, só muda o status Premium
            int energiaAtual = EnergyManager.Instance.energiasAtuais;

            // CORREÇÃO DO ERRO VERMELHO: O nome certo é 'AtualizarDados'
            EnergyManager.Instance.AtualizarDados(isPremium, energiaAtual);

            Debug.Log($"<color=cyan>[ADMIN] Status Premium alterado para: {isPremium}</color>");
        }
    }

    // Função chamada pelo botão "+ Energia" do HTML
    public void JS_AddEnergy(string ignore)
    {
        if (EnergyManager.Instance)
        {
            // REGRA NOVA: Adiciona 10 energias
            EnergyManager.Instance.AdicionarEnergia(10, "Painel Admin");

            Debug.Log("<color=cyan>[ADMIN] +10 Energia adicionada via Painel.</color>");
        }
    }
}