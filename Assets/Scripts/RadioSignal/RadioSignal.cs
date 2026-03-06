using System;
using UnityEngine;

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class RadioSignal : MonoBehaviour
{
    // --- ADIÃ‡ÃƒO NECESSÃRIA PARA O SEU DEBUGSIMULATOR FUNCIONAR ---
#pragma warning disable 67
    public static event Action<string> OnEditorSpy;
#pragma warning restore 67
    // -------------------------------------------------------------

    [Header("Identidade")]
    [Tooltip("Recebe do init do portal. Se nÃ£o vier, fica user_missing.")]
    public string usuarioId = "user_missing";

    [Header("App")]
    public string appId = "plansul-game";

    public static bool SessionReady { get; private set; } = false;
    public static event Action OnSessionReady;

    // --- 1. INIT ATUALIZADO (A Ãºnica mudanÃ§a estrutural necessÃ¡ria) ---
    [Serializable]
    public class InitMsg
    {
        public string type;
        public string user_uuid;
        public string user_name;
        public bool is_premium;
        public int energy_balance;
        public string from;
        public string ts;
    }

    [Serializable]
    public class CurrentLevelMsg
    {
        public string type;
        public string world_uuid;
        public Current data;
        public string from;
        public string ts;

        [Serializable] public class Current { public int current_level; }
    }

    // --- 2. ESTRUTURA COMPLETA RESTAURADA (Para nÃ£o quebrar vÃ­deos/DRM) ---
    [Serializable]
    public class LevelContentMsg
    {
        public string type;
        public string world_uuid;
        public int level;
        public Payload data;
        public string from;
        public string ts;

        [Serializable]
        public class Payload
        {
            public string uuid;
            public string description;
            public int level_number;
            public string type;
            public Inner data;

            [Serializable]
            public class Inner
            {
                public string drm_token;
                public string title;
                public string mpeg_dash_url;
                public string hls_url;
                public string asset_id;
                public string duration;
            }
        }
    }

    [Serializable]
    public class RewardMsg
    {
        public string type;
        public int amount;
        public string source;
    }

    public static event Action<int> OnCurrentLevel;
    public static event Action<LevelContentMsg> OnLevelContent;
    private const int BridgeInitRetries = 6;
    private const float BridgeInitRetryInterval = 1.5f;
    private static RadioSignal instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        if (gameObject.name != "RadioSignal") gameObject.name = "RadioSignal";
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    System.Collections.IEnumerator Start()
    {
        // --- O FREIO DE MAO ---
        yield return new WaitForSeconds(2.0f);

#if UNITY_WEBGL && !UNITY_EDITOR
        for (int attempt = 1; attempt <= BridgeInitRetries && !SessionReady; attempt++)
        {
            PlansulBridge_Init();
            Debug.Log($"[RadioSignal] Bridge init tentativa {attempt}/{BridgeInitRetries}.");
            if (attempt < BridgeInitRetries) yield return new WaitForSeconds(BridgeInitRetryInterval);
        }
#endif
        if (!SessionReady)
        {
            Debug.LogWarning("[RadioSignal] current_level ainda nao chegou; aguardando host/API.");
        }
        Debug.Log("[RadioSignal] Bridge inicializada.");
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !SessionReady)
        {
            PlansulBridge_Init();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && !SessionReady)
        {
            PlansulBridge_Init();
        }
    }
#endif

    // -------- UNITY -> IFRAME --------

    public void SendLevelClick(int level)
    {
        if (!SessionReady)
        {
            Debug.Log("[RadioSignal] Clique ignorado: sessÃ£o ainda nÃ£o pronta (aguardando current_level).");
            return;
        }

        var msg = new Envelope
        {
            type = "level_click",
            level = level,
            userId = SafeUserId(),
            from = "game",
            ts = DateTime.UtcNow.ToString("o")
        };

        SendToBridge(msg);
        Debug.Log($"[RadioSignal] level_click -> level={level} userId={msg.userId}");
    }

    public void SendLevelPlay(int level)
    {
        if (!SessionReady)
        {
            Debug.Log("[RadioSignal] level_play ignorado: sessÃ£o nÃ£o pronta.");
            return;
        }

        var msg = new Envelope
        {
            type = "level_play",
            level = level,
            userId = SafeUserId(),
            from = "game",
            ts = DateTime.UtcNow.ToString("o")
        };

        SendToBridge(msg);
        Debug.Log($"[RadioSignal] level_play -> level={level} userId={msg.userId}");
    }

    public void SendLevelComplete(int level)
    {
        if (!SessionReady)
        {
            Debug.Log("[RadioSignal] level_complete ignorado: sessÃ£o nÃ£o pronta.");
            return;
        }

        var msg = new Envelope
        {
            type = "level_complete",
            level = level,
            userId = SafeUserId(),
            from = "game",
            ts = DateTime.UtcNow.ToString("o")
        };

        SendToBridge(msg);
        Debug.Log($"[RadioSignal] level_complete -> level={level} userId={msg.userId}");
    }

    private string SafeUserId()
    {
        return string.IsNullOrWhiteSpace(usuarioId) ? "user_missing" : usuarioId;
    }

    private void SendToBridge(Envelope msg)
    {
        var json = JsonUtility.ToJson(msg);

#if UNITY_WEBGL && !UNITY_EDITOR
        PlansulBridge_Send(json);
#else
        Debug.Log($"(Editor) {json}");
#if UNITY_EDITOR
        OnEditorSpy?.Invoke(json); // Mantido para o seu DebugSimulator funcionar no Editor
#endif
#endif
    }

    // -------- IFRAME -> UNITY --------

    // -------- IFRAME -> UNITY --------

    public void OnJsMessage(string json)
    {
        string normalizedJson = NormalizeJsonPayload(json);
        if (string.IsNullOrWhiteSpace(normalizedJson)) return;
        string messageType = GetMessageType(normalizedJson);

        // 1. TENTA RECOMPENSA (NOVO - Faltava isso!)
        if (messageType == "reward")
        {
            try
            {
                RewardMsg reward = JsonUtility.FromJson<RewardMsg>(normalizedJson);
                if (reward != null && EnergyManager.Instance != null)
                {
                    // Chama o EnergyManager para somar a energia
                    EnergyManager.Instance.AdicionarEnergia(reward.amount, reward.source);
                }
            }
            catch (Exception e) { Debug.LogError($"[RadioSignal] Erro Reward: {e.Message}"); }
            return;
        }

        // 2. Tenta INIT (Login, Premium e Energia)
        try
        {
            var init = JsonUtility.FromJson<InitMsg>(normalizedJson);
            if (init != null && init.type == "init")
            {
                if (!string.IsNullOrWhiteSpace(init.user_uuid)) usuarioId = init.user_uuid;
                else if (!string.IsNullOrWhiteSpace(init.user_name)) usuarioId = init.user_name;
                else usuarioId = "user_missing";

                Debug.Log($"[RadioSignal] init recebido -> User={usuarioId}, Premium={init.is_premium}, Energy={init.energy_balance}");

                // Atualiza o EnergyManager (Limpo e sem duplicidade)
                if (EnergyManager.Instance != null)
                {
                    EnergyManager.Instance.AtualizarDados(init.is_premium, init.energy_balance);
                }
                return;
            }
        }
        catch { }

        // 3. Tenta Current Level (Liberacao do Jogo)
        try
        {
            if (messageType == "current_level")
            {
                int currentLevel = -1;
                var cur = JsonUtility.FromJson<CurrentLevelMsg>(normalizedJson);
                if (cur != null && cur.data != null)
                {
                    currentLevel = cur.data.current_level;
                }

                if (currentLevel < 0)
                {
                    var flat = JsonUtility.FromJson<CurrentLevelFlatMsg>(normalizedJson);
                    if (flat != null) currentLevel = flat.current_level;
                }

                if (currentLevel >= 0)
                {
                    OnCurrentLevel?.Invoke(currentLevel);
                    EnsureSessionReady();
                    Debug.Log($"[RadioSignal] current_level={currentLevel}");
                    return;
                }
            }
        }
        catch { }

        // 4. Tenta Level Content (VIDEO/DRM - MANTIDO!)
        try
        {
            var content = JsonUtility.FromJson<LevelContentMsg>(normalizedJson);
            if (content != null && content.type == "level_content" && content.data != null)
            {
                OnLevelContent?.Invoke(content);
                Debug.Log($"[RadioSignal] level_content recebido (level {content.level}) type={content.data.type}");
                return;
            }
        }
        catch { }

        // Debug para mensagens de retorno (play/complete)
        try
        {
            var env = JsonUtility.FromJson<Envelope>(normalizedJson);
            if (env != null && (env.type == "level_complete" || env.type == "level_play"))
            {
                Debug.Log($"[RadioSignal] {env.type} recebido de volta (echo/debug) level={env.level}");
                return;
            }
        }
        catch { }

        // Se chegou aqui, e algo desconhecido
        Debug.Log("[RadioSignal] (raw) " + normalizedJson);
    }

    private static string NormalizeJsonPayload(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        string payload = raw.Trim();

        // Alguns WebViews iOS entregam JSON em formato string escapada: "{\"type\":\"init\"...}"
        if (payload.Length > 1 && payload[0] == '"' && payload[payload.Length - 1] == '"')
        {
            payload = payload.Substring(1, payload.Length - 2);
            payload = payload.Replace("\\\"", "\"");
            payload = payload.Replace("\\\\", "\\");
        }

        return payload;
    }

    private void EnsureSessionReady()
    {
        if (SessionReady) return;
        SessionReady = true;
        OnSessionReady?.Invoke();
        Debug.Log("[RadioSignal] SessionReady=true (current_level recebido).");
    }

    private static string GetMessageType(string normalizedJson)
    {
        try
        {
            var typeOnly = JsonUtility.FromJson<TypeOnlyMsg>(normalizedJson);
            return typeOnly != null ? typeOnly.type : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    [Serializable]
    private class TypeOnlyMsg
    {
        public string type;
    }

    [Serializable]
    private class CurrentLevelFlatMsg
    {
        public string type;
        public int current_level;
    }

    [Serializable]
    private class Envelope
    {
        public string type;   // "level_click" | "level_play" | "level_complete"
        public int level;
        public string userId;
        public string from;
        public string ts;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void PlansulBridge_Init();
    [DllImport("__Internal")] private static extern void PlansulBridge_Send(string json);
#endif
}


