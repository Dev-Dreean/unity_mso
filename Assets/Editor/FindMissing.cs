#nullable enable
using UnityEditor;
using UnityEngine;

public static class FindMissing
{
    [MenuItem("Tools/Missing/Remove Missing Scripts in Scene")]
    static void RemoveMissingInScene()
    {
        int count = 0;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        Debug.Log($"Missing removidos na cena: {count}");
    }

    [MenuItem("Tools/Missing/Remove Missing Scripts in Selection")]
    static void RemoveMissingInSelection()
    {
        int count = 0;
        foreach (var obj in Selection.gameObjects)
            count += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
        Debug.Log($"Missing removidos na seleção: {count}");
    }
}
