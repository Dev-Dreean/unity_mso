#nullable enable
using UnityEngine;

public class MapWorldSwitcher : MonoBehaviour
{
    // ✅ Atalhos (você pediu “Menu / MapWorld / Caverna”)
    // Ajuste os índices conforme a ordem do array "worlds" no Inspector.
    public enum WorldId
    {
        Menu = 0,
        MapWorld = 1,
        Caverna = 2
    }

    [System.Serializable]
    public class WorldConfig
    {
        public string nome = "Mundo";
        public GameObject rootObject = default!;

        [Header("Câmera")]
        public bool cameraTravada = false;
        public SpriteRenderer? limitesParaEnquadrar;
    }

    [Header("Configuração dos Mundos")]
    public WorldConfig[] worlds = default!;

    [Header("Referências")]
    public Camera mainCamera = default!;
    public LevelManager1 levelManager = default!;

    public int CurrentIndex { get; private set; } = 0;

    private Vector3 savedCamPos;

    private void Start()
    {
        if (mainCamera) savedCamPos = mainCamera.transform.position;
        if (!levelManager) levelManager = FindFirstObjectByType<LevelManager1>();

        Show(CurrentIndex);
    }

    // =========================
    // ✅ Overloads por enum/nome
    // =========================

    public void Show(WorldId world) => Show((int)world);

    public void ShowWithOverride(WorldId world, float targetX, float targetY) =>
        ShowWithOverride((int)world, targetX, targetY);

    public bool Show(string worldName)
    {
        int idx = FindWorldIndex(worldName);
        if (idx < 0) return false;
        Show(idx);
        return true;
    }

    public bool ShowWithOverride(string worldName, float targetX, float targetY)
    {
        int idx = FindWorldIndex(worldName);
        if (idx < 0) return false;
        ShowWithOverride(idx, targetX, targetY);
        return true;
    }

    private int FindWorldIndex(string worldName)
    {
        if (worlds == null) return -1;
        for (int i = 0; i < worlds.Length; i++)
        {
            if (worlds[i] != null && !string.IsNullOrEmpty(worlds[i].nome) &&
                string.Equals(worlds[i].nome, worldName, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    // =========================
    // ✅ Funções originais
    // =========================

    // Essa função é chamada pelo AutoSwitchWorld
    public void ShowWithOverride(int index, float targetX, float targetY)
    {
        savedCamPos = new Vector3(targetX, targetY, -10f);
        Show(index);
    }

    public void Show(int index)
    {
        if (worlds == null || worlds.Length == 0) return;
        index = Mathf.Clamp(index, 0, worlds.Length - 1);

        CurrentIndex = index;
        PerformShow(index);
    }

    private void PerformShow(int index)
    {
        WorldConfig activeWorld = worlds[index];

        for (int i = 0; i < worlds.Length; i++)
        {
            if (worlds[i].rootObject)
                worlds[i].rootObject.SetActive(i == index);
        }

        HandleCamera(activeWorld);

        if (MusicController.Instance != null)
            MusicController.Instance.TrocarEstado(index);

        if (levelManager) levelManager.ForceRefresh();
    }

    private void HandleCamera(WorldConfig world)
    {
        if (!mainCamera) return;

        var navScript = mainCamera.GetComponent<MapNavigation>();

        if (navScript != null && world.limitesParaEnquadrar != null)
        {
            navScript.enabled = !world.cameraTravada;
            if (!world.cameraTravada) navScript.SetBounds(world.limitesParaEnquadrar);
        }

        if (world.cameraTravada && world.limitesParaEnquadrar != null)
        {
            Bounds bounds = world.limitesParaEnquadrar.bounds;
            mainCamera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);

            float screenRatio = (float)Screen.width / (float)Screen.height;
            float targetRatio = bounds.size.x / bounds.size.y;

            if (screenRatio >= targetRatio)
                mainCamera.orthographicSize = bounds.size.y / 2f;
            else
                mainCamera.orthographicSize = bounds.size.y / 2f * (targetRatio / screenRatio);
        }
        else
        {
            // ✅ Obedece override do AutoSwitchWorld
            mainCamera.transform.position = new Vector3(savedCamPos.x, savedCamPos.y, -10f);
        }
    }

    // =========================
    // ✅ Debug: “listagem” no console
    // =========================
    [ContextMenu("Debug/Imprimir lista de mundos")]
    private void DebugPrintWorlds()
    {
        if (worlds == null || worlds.Length == 0)
        {
            Debug.LogWarning("[MapWorldSwitcher] worlds está vazio.");
            return;
        }

        for (int i = 0; i < worlds.Length; i++)
        {
            var w = worlds[i];
            string n = w != null ? w.nome : "(null)";
            string hasObj = (w != null && w.rootObject != null) ? "OK" : "SEM ROOT";
            Debug.Log($"[World {i}] {n} | {hasObj}");
        }
    }
}
