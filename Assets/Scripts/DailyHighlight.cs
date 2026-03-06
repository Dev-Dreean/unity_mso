using UnityEngine;
using System;

public class DailyHighlight : MonoBehaviour
{
    [Header("Arraste os Contornos (Ordem: Seg a Dom)")]
    // Elemento 0 = Segunda, Elemento 6 = Domingo
    public GameObject[] contornosDias; 

    void OnEnable()
    {
        AtualizarDia();
    }

    void AtualizarDia()
    {
        // 1. Apaga tudo
        foreach (var obj in contornosDias)
        {
            if(obj) obj.SetActive(false);
        }

        // 2. Pega o dia de hoje
        DayOfWeek diaIngles = DateTime.Now.DayOfWeek;
        
        // 3. Traduz para nosso array (Segunda = 0)
        int indice = -1;
        switch (diaIngles)
        {
            case DayOfWeek.Monday:    indice = 0; break;
            case DayOfWeek.Tuesday:   indice = 1; break;
            case DayOfWeek.Wednesday: indice = 2; break;
            case DayOfWeek.Thursday:  indice = 3; break;
            case DayOfWeek.Friday:    indice = 4; break;
            case DayOfWeek.Saturday:  indice = 5; break;
            case DayOfWeek.Sunday:    indice = 6; break;
        }

        // 4. Acende o correto
        if (indice >= 0 && indice < contornosDias.Length)
        {
            if(contornosDias[indice]) contornosDias[indice].SetActive(true);
        }
    }
}