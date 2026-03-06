using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class WebVideoFix : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nomeDoArquivo = "MENU_ANIMADO.mp4";
    public float prepareTimeoutSeconds = 8f;
    [Tooltip("Em safe mode WebGL (safe>=1), nao prepara video para reduzir pico de RAM no iPhone.")]
    public bool desativarVideoEmSafeModeWebGL = true;
    [Tooltip("Desativa audio do VideoPlayer para melhorar autoplay em iOS/WebView.")]
    public bool compatAutoplaySemAudio = true;

    private Coroutine playRoutine;
    private bool prepareFailed;

    void OnEnable()
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        playRoutine = StartCoroutine(PlayVideo());
    }

    void OnDisable()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (videoPlayer)
        {
            videoPlayer.errorReceived -= OnVideoError;
        }
    }

    IEnumerator PlayVideo()
    {
        if (!videoPlayer)
        {
            Debug.LogWarning("[VideoFix] VideoPlayer nao foi atribuido.");
            yield break;
        }

        if (desativarVideoEmSafeModeWebGL && IsWebGLSafeModeAtivo())
        {
            videoPlayer.Stop();
            Debug.LogWarning("[VideoFix] Safe mode WebGL ativo. Video de menu desativado para reduzir memoria.");
            yield break;
        }

        string videoPath = BuildStreamingAssetsUrl(nomeDoArquivo);
        prepareFailed = false;

        Debug.Log("[VideoFix] Carregando: " + videoPath);

        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.playOnAwake = false;
        videoPlayer.skipOnDrop = true;
        videoPlayer.waitForFirstFrame = true;
        if (compatAutoplaySemAudio)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.Stop();
        videoPlayer.Prepare();

        float timer = 0f;
        while (!videoPlayer.isPrepared && !prepareFailed && timer < prepareTimeoutSeconds)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        videoPlayer.errorReceived -= OnVideoError;

        if (prepareFailed)
        {
            Debug.LogWarning("[VideoFix] Falha ao preparar o video.");
            yield break;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning($"[VideoFix] Timeout ao preparar o video apos {prepareTimeoutSeconds:0.0}s.");
            yield break;
        }

        videoPlayer.Play();
        playRoutine = null;
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        prepareFailed = true;
        Debug.LogError("[VideoFix] erro do VideoPlayer: " + message);
    }

    private static string BuildStreamingAssetsUrl(string fileName)
    {
        string basePath = Application.streamingAssetsPath.TrimEnd('/', '\\');
        string cleanFile = string.IsNullOrWhiteSpace(fileName) ? string.Empty : fileName.TrimStart('/', '\\');
        return $"{basePath}/{cleanFile}";
    }

    private static bool IsWebGLSafeModeAtivo()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string url = Application.absoluteURL;
        if (string.IsNullOrEmpty(url)) return false;
        int idx = url.IndexOf('?');
        if (idx < 0 || idx >= url.Length - 1) return false;

        string query = url.Substring(idx + 1);
        string[] parts = query.Split('&');
        for (int i = 0; i < parts.Length; i++)
        {
            string[] kv = parts[i].Split('=');
            if (kv.Length != 2) continue;
            if (!string.Equals(kv[0], "safe", StringComparison.OrdinalIgnoreCase)) continue;
            string val = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(kv[1]);
            if (int.TryParse(val, out int safe)) return safe >= 1;
            return false;
        }
#endif
        return false;
    }
}
