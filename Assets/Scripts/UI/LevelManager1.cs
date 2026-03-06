using System.Linq;
using UnityEngine;

public class LevelManager1 : MonoBehaviour
{
    [Header("Estado")]
    [Tooltip("Nível máximo liberado no jogo (1 liberado por padrão).")]
    public int unlockedMax = 1;

    private LevelNode[] nodes;

    void Awake()
    {
        nodes = FindObjectsByType<LevelNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (nodes == null) nodes = new LevelNode[0];
        RefreshLocks();
    }

    void OnEnable()
    {
        RadioSignal.OnCurrentLevel += HandleCurrentLevel;    // vindo do portal: current_level
    }

    void OnDisable()
    {
        RadioSignal.OnCurrentLevel -= HandleCurrentLevel;
    }

    private void HandleCurrentLevel(int currentLevel)
    {
        unlockedMax = Mathf.Max(1, currentLevel);
        RefreshLocks();
        Debug.Log($"[LevelManager] current_level={currentLevel} (unlockedMax={unlockedMax})");
    }

    public void ForceRefresh() => RefreshLocks(); // <- chamada pelo AutoSetup/Bootstrapper

    private void RefreshLocks()
    {
        // revarre se alguém criou nós em runtime
        nodes = FindObjectsByType<LevelNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (nodes == null) nodes = new LevelNode[0];

        foreach (var n in nodes)
        {
            if (!n) continue;
            bool locked = n.levelNumber > unlockedMax;
            n.ApplyLocked(locked);
        }
    }
}
