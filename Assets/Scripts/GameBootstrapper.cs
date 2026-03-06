using System;
using System.Collections;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Configuracao")]
    [Tooltip("Se marcado, sempre forca o Menu a abrir e o Gameplay a fechar ao dar Play.")]
    public bool forcarMenuNoInicio = true;

    [Header("Referencias")]
    public GameObject mainMenu;
    public GameObject gameplay;

    private void Awake()
    {
        ApplyWebGLLowMemoryProfile();

        // Seguranca no frame 0: evita ativar blocos pesados cedo demais.
        if (mainMenu != null) mainMenu.SetActive(false);
        if (gameplay != null) gameplay.SetActive(false);
    }

    private IEnumerator Start()
    {
        // Pequeno atraso para estabilizar carga inicial no WebGL mobile.
        yield return new WaitForSeconds(2.5f);

#if UNITY_WEBGL && !UNITY_EDITOR
        // Limpeza preventiva de heap antes de abrir elementos de UI/Video.
        yield return Resources.UnloadUnusedAssets();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        yield return null;
#endif

        if (forcarMenuNoInicio)
        {
            if (mainMenu != null)
            {
                mainMenu.SetActive(true);
            }
            // gameplay continua desligado ate o clique em jogar
        }
    }

    private static void ApplyWebGLLowMemoryProfile()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        int safe = GetUrlIntParam("safe", GetDefaultSafeLevel());

        // safe=1 (padrao mobile) e safe=2 (ultra) reduzem memoria de textura e render.
        int textureLimit = safe >= 2 ? 3 : (safe >= 1 ? 2 : 1);
        int targetFps = safe >= 2 ? 20 : 30;

        QualitySettings.globalTextureMipmapLimit = Mathf.Max(QualitySettings.globalTextureMipmapLimit, textureLimit);
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowDistance = 0f;
        QualitySettings.pixelLightCount = 0;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.softParticles = false;
        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = targetFps;

#if UNITY_2020_2_OR_NEWER
        UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = safe >= 2 ? 2 : 1;
#endif

        Debug.Log($"[Bootstrapper] WebGL memory profile aplicado. safe={safe}, globalTextureMipmapLimit={QualitySettings.globalTextureMipmapLimit}, targetFps={Application.targetFrameRate}");
#endif
    }

    private static int GetDefaultSafeLevel()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string url = Application.absoluteURL;
        if (string.IsNullOrEmpty(url)) return 0;
        if (url.IndexOf("/mobile", StringComparison.OrdinalIgnoreCase) >= 0) return 1;
#endif
        return 0;
    }

    private static int GetUrlIntParam(string key, int fallback)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string url = Application.absoluteURL;
        if (string.IsNullOrEmpty(url)) return fallback;

        int queryIndex = url.IndexOf('?');
        if (queryIndex < 0 || queryIndex >= url.Length - 1) return fallback;

        string query = url.Substring(queryIndex + 1);
        string[] parts = query.Split('&');
        for (int i = 0; i < parts.Length; i++)
        {
            string p = parts[i];
            if (string.IsNullOrEmpty(p)) continue;
            string[] kv = p.Split('=');
            if (kv.Length != 2) continue;
            if (!string.Equals(kv[0], key, StringComparison.OrdinalIgnoreCase)) continue;

            string value = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(kv[1]);
            if (int.TryParse(value, out int parsed)) return parsed;
            return fallback;
        }
#endif
        return fallback;
    }
}
