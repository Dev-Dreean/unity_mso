using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance;

    [Header("UI")]
    public TextMeshProUGUI textoEnergia;
    public TextMeshProUGUI textoTimer;
    public GameObject iconeInfinito;
    public GameObject popupSemEnergia;

    [Header("Configuração do Sistema")]
    public bool isPremium = false;
    public int energiasAtuais = 20;
    public int maxEnergias = 20;
    public int custoPorFase = 10;
    public int recargaQuantidade = 10;
    public float horasParaRecarga = 24f;

    private DateTime dataProximaRecarga;
    private bool timerAtivo = false;

    private const string PREF_NEXT_RECHARGE = "NextRechargeTime";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CarregarTimer();
        AtualizarVisual();
    }

    void Update()
    {
        if (!isPremium && energiasAtuais < maxEnergias)
        {
            if (!timerAtivo)
            {
                DefinirProximaRecarga(DateTime.Now.AddHours(horasParaRecarga));
            }

            TimeSpan restante = dataProximaRecarga - DateTime.Now;

            if (restante.TotalSeconds <= 0)
            {
                AdicionarEnergia(recargaQuantidade, "Recarga Diária");
                timerAtivo = false;
                PlayerPrefs.DeleteKey(PREF_NEXT_RECHARGE);
            }
            else
            {
                if (textoTimer)
                {
                    textoTimer.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        restante.Hours, restante.Minutes, restante.Seconds);
                    textoTimer.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (textoTimer) textoTimer.gameObject.SetActive(false);
            timerAtivo = false;
        }
    }

    public void AtualizarDados(bool premium, int saldo)
    {
        isPremium = premium;
        energiasAtuais = saldo;

        if (!isPremium && energiasAtuais > maxEnergias) energiasAtuais = maxEnergias;
        if (energiasAtuais < 0) energiasAtuais = 0;

        AtualizarVisual();
    }

    public bool ConsumirEnergia()
    {
        if (isPremium) return true;

        if (energiasAtuais >= custoPorFase)
        {
            energiasAtuais -= custoPorFase;

            if (!timerAtivo)
            {
                DefinirProximaRecarga(DateTime.Now.AddHours(horasParaRecarga));
            }

            AtualizarVisual();
            return true;
        }
        else
        {
            if (popupSemEnergia) popupSemEnergia.SetActive(true);
            return false;
        }
    }

    // --- VERSÃO LIMPA (SEM TRAVA DE DATA) ---
    // O Marcelo/Kadu controlam quando chamar isso via App.
    public void AdicionarEnergia(int quantidade, string fonte = "Sistema")
    {
        energiasAtuais += quantidade;
        if (!isPremium && energiasAtuais > maxEnergias) energiasAtuais = maxEnergias;

        AtualizarVisual();
        Debug.Log($"[EnergyManager] +{quantidade} Energia recebida via: {fonte}");

        if (energiasAtuais == maxEnergias)
        {
            dispararNotificacaoEnergiaCheia();
        }
    }

    public void AtualizarVisual()
    {
        if (isPremium)
        {
            if (iconeInfinito) iconeInfinito.SetActive(true);
            if (textoEnergia) textoEnergia.gameObject.SetActive(false);
            if (textoTimer) textoTimer.gameObject.SetActive(false);
        }
        else
        {
            if (iconeInfinito) iconeInfinito.SetActive(false);
            if (textoEnergia)
            {
                textoEnergia.gameObject.SetActive(true);
                textoEnergia.text = $"{energiasAtuais}/{maxEnergias}";
            }
        }
    }

    private void DefinirProximaRecarga(DateTime alvo)
    {
        dataProximaRecarga = alvo;
        timerAtivo = true;
        PlayerPrefs.SetString(PREF_NEXT_RECHARGE, dataProximaRecarga.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void CarregarTimer()
    {
        if (PlayerPrefs.HasKey(PREF_NEXT_RECHARGE))
        {
            try
            {
                long temp = Convert.ToInt64(PlayerPrefs.GetString(PREF_NEXT_RECHARGE));
                dataProximaRecarga = DateTime.FromBinary(temp);
                timerAtivo = true;
            }
            catch
            {
                timerAtivo = false;
            }
        }
    }

    private void dispararNotificacaoEnergiaCheia()
    {
        // Hook para sistema de Notificação Mobile (Android/iOS)
        Debug.Log(">> NOTIFICACAO: Sua energia já está cheia ⚡");
    }
}