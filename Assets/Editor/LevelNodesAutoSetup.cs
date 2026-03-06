#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class LevelNodesAutoSetup
{
    [MenuItem("Tools/Levels/Auto-Setup na Seleção")]
    public static void AutoSetupOnSelection()
    {
        var sel = Selection.activeTransform;
        if (!sel) { Debug.LogError("Selecione a raiz onde estão os botões."); return; }

        var boot = sel.GetComponent<LevelNodesBootstrapper>();
        if (!boot) boot = sel.gameObject.AddComponent<LevelNodesBootstrapper>();
        boot.root = sel;

        // tenta achar RadioSignal na cena
#if UNITY_2023_1_OR_NEWER
        var r = Object.FindFirstObjectByType<RadioSignal>();
        if (!r) r = Object.FindAnyObjectByType<RadioSignal>();
#else
        var r = Object.FindObjectOfType<RadioSignal>();
#endif
        boot.radio = r;

        Undo.RegisterCompleteObjectUndo(boot, "LevelNodes Auto-Setup");
        boot.BootstrapNow();
        EditorUtility.SetDirty(boot);
    }
}
#endif
