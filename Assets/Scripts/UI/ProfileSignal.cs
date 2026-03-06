using UnityEngine;
using System;

public class ProfileSignal : MonoBehaviour
{
    // Evento que o EnergyManager está procurando e não achou
    public static event Action<bool, int> OnProfileUpdate;

    [Serializable]
    public class UserProfileMsg
    {
        public string type;       
        public bool is_premium;   
        public int energy_balance;
    }

    public void OnProfileMessage(string json)
    {
        Debug.Log($"[ProfileSignal] Recebido: {json}");
        try
        {
            var msg = JsonUtility.FromJson<UserProfileMsg>(json);
            if (msg != null && msg.type == "user_profile")
            {
                OnProfileUpdate?.Invoke(msg.is_premium, msg.energy_balance);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[ProfileSignal] Erro JSON: {e.Message}");
        }
    }
}