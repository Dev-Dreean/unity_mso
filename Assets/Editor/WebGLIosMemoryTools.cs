using System;
using System.Linq;
using System.Reflection;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEngine;

public static class WebGLIosMemoryTools
{
    private const string WebGLPlatform = "WebGL";

    [MenuItem("Tools/MSO/WebGL/Apply iOS Safe Texture Profile (Conservative)")]
    public static void ApplyConservativeProfile()
    {
        ApplyTextureProfile(2048, 1024, "conservative");
    }

    [MenuItem("Tools/MSO/WebGL/Apply iOS Safe Texture Profile (Aggressive)")]
    public static void ApplyAggressiveProfile()
    {
        ApplyTextureProfile(1024, 512, "aggressive");
    }

    [MenuItem("Tools/MSO/WebGL/Set WebGL Compression to ETC2")]
    public static void SetWebGLCompressionToEtc2()
    {
        bool changed = TrySetWebGLTextureCompressionEtc2();
        if (changed)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("[WebGLIosMemoryTools] WebGL texture compression set to ETC2.");
        }
        else
        {
            Debug.LogWarning("[WebGLIosMemoryTools] Could not set WebGL texture compression via API. Set manually in Player Settings > WebGL.");
        }
    }

    private static void ApplyTextureProfile(int mapsMaxSize, int defaultMaxSize, string label)
    {
        int changed = 0;
        int total = 0;
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });

        try
        {
            EditorUtility.DisplayProgressBar("WebGL iOS Profile", "Applying texture overrides...", 0f);

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path)) continue;
                total++;

                float p = (float)i / Mathf.Max(1, guids.Length);
                EditorUtility.DisplayProgressBar("WebGL iOS Profile", path, p);

                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null) continue;

                bool isUiLike =
                    path.IndexOf("/UI/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    path.IndexOf("/Menu/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    path.IndexOf("/Banco de Questoes/", StringComparison.OrdinalIgnoreCase) >= 0;

                bool isMapLike =
                    path.IndexOf("/MAPS/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    path.IndexOf("map -", StringComparison.OrdinalIgnoreCase) >= 0;

                bool hasAlpha = ti.DoesSourceTextureHaveAlpha();
                int targetMaxSize = isMapLike ? mapsMaxSize : defaultMaxSize;

                bool fileChanged = false;
                TextureImporterPlatformSettings web = ti.GetPlatformTextureSettings(WebGLPlatform);
                web.name = WebGLPlatform;
                web.overridden = true;
                web.maxTextureSize = Mathf.Clamp(targetMaxSize, 256, 8192);
                web.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                web.format = hasAlpha ? TextureImporterFormat.ETC2_RGBA8 : TextureImporterFormat.ETC2_RGB4;
                web.textureCompression = TextureImporterCompression.Compressed;
                web.compressionQuality = isUiLike ? 70 : 50;
                web.crunchedCompression = true;

                TextureImporterPlatformSettings current = ti.GetPlatformTextureSettings(WebGLPlatform);
                if (!PlatformSettingsEqual(current, web))
                {
                    ti.SetPlatformTextureSettings(web);
                    fileChanged = true;
                }

                // Help mobile memory by disabling mipmaps for UI/menu textures.
                if (isUiLike && ti.mipmapEnabled)
                {
                    ti.mipmapEnabled = false;
                    fileChanged = true;
                }

                // Ensure sprites are not forcing pointlessly high quality for WebGL.
                if (isUiLike && ti.textureCompression != TextureImporterCompression.Compressed)
                {
                    ti.textureCompression = TextureImporterCompression.Compressed;
                    fileChanged = true;
                }

                if (!fileChanged) continue;

                ti.SaveAndReimport();
                changed++;
            }

            bool webGlCompressionSet = TrySetWebGLTextureCompressionEtc2();
            AssetDatabase.SaveAssets();
            Debug.Log($"[WebGLIosMemoryTools] Applied {label} profile. Changed textures: {changed}/{total}. WebGL ETC2 set: {webGlCompressionSet}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static bool TrySetWebGLTextureCompressionEtc2()
    {
        // 1) Try PlayerSettings.WebGL.textureCompressionFormat
        if (TrySetViaPlayerSettingsReflection()) return true;

        // 2) Fallback used by some Unity 6 variants:
        //    EditorUserBuildSettings.webGLBuildSubtarget = ETC2
        if (TrySetViaWebGlBuildSubtarget()) return true;

        return false;
    }

    private static bool TrySetViaPlayerSettingsReflection()
    {
        Type psType = typeof(PlayerSettings);
        Type webGlType = psType.GetNestedType("WebGL", BindingFlags.Public);
        if (webGlType == null) return false;

        PropertyInfo prop = webGlType.GetProperty("textureCompressionFormat", BindingFlags.Public | BindingFlags.Static);
        if (prop == null) return false;

        Type enumType = prop.PropertyType;
        if (!enumType.IsEnum) return false;

        string etc2Name = Enum.GetNames(enumType).FirstOrDefault(
            n => string.Equals(n, "ETC2", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(etc2Name)) return false;

        object etc2Value = Enum.Parse(enumType, etc2Name);
        object current = prop.GetValue(null, null);
        if (current != null && current.Equals(etc2Value)) return true;

        prop.SetValue(null, etc2Value, null);
        return true;
    }

    private static bool TrySetViaWebGlBuildSubtarget()
    {
        Type eubsType = typeof(EditorUserBuildSettings);
        PropertyInfo subtargetProp = eubsType.GetProperty("webGLBuildSubtarget", BindingFlags.Public | BindingFlags.Static);
        if (subtargetProp == null) return false;

        Type enumType = subtargetProp.PropertyType;
        if (!enumType.IsEnum) return false;

        string etc2Name = Enum.GetNames(enumType).FirstOrDefault(
            n => n.IndexOf("ETC2", StringComparison.OrdinalIgnoreCase) >= 0);
        if (string.IsNullOrEmpty(etc2Name)) return false;

        object etc2Value = Enum.Parse(enumType, etc2Name);
        object current = subtargetProp.GetValue(null, null);
        if (current != null && current.Equals(etc2Value)) return true;

        subtargetProp.SetValue(null, etc2Value, null);
        return true;
    }

    private static bool PlatformSettingsEqual(TextureImporterPlatformSettings a, TextureImporterPlatformSettings b)
    {
        return a.overridden == b.overridden
            && a.maxTextureSize == b.maxTextureSize
            && a.resizeAlgorithm == b.resizeAlgorithm
            && a.format == b.format
            && a.textureCompression == b.textureCompression
            && a.compressionQuality == b.compressionQuality
            && a.crunchedCompression == b.crunchedCompression;
    }
}

[InitializeOnLoad]
public static class WebGLIosMemoryAutoSetup
{
    static WebGLIosMemoryAutoSetup()
    {
        EditorApplication.delayCall += EnsureWebGlCompression;
    }

    private static void EnsureWebGlCompression()
    {
        // Silent enforcement on editor load so WebGL does not regress to DXT.
        WebGLIosMemoryTools.SetWebGLCompressionToEtc2();
    }
}

public class WebGLIosBuildGuard : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report == null) return;
        if (report.summary.platform != BuildTarget.WebGL) return;
        WebGLIosMemoryTools.SetWebGLCompressionToEtc2();
        Debug.Log("[WebGLIosBuildGuard] WebGL compression validated as ETC2 before build.");
    }
}
