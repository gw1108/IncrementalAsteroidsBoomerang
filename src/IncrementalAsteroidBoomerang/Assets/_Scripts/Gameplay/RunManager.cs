using UnityEngine;

/// <summary>
/// Owns the run lifecycle. Calls all subsystem start-of-run initialisation in the correct order.
/// Add any future run-start hooks here (C5 run-end, G5 mining, etc.) rather than in Awake/Start
/// on individual MonoBehaviours.
/// </summary>
public class RunManager : MonoBehaviour
{
    [SerializeField] private C6StatResolver _resolver;
    [SerializeField] private FuelManager _fuelManager;

    private void Awake()
    {
        // setup FBPP
        InitializeFBPP();
    }

    private const string c_encryptionSecret = "IncrementalAsteroidsEncryptionSecret";

    private static void InitializeFBPP()
    {
        string path = SaveSystem.GetSaveFilePath();
        Debug.Log($"[SaveManager] Save path: {path} filename: {SaveSystem.c_saveFileName}");

        FBPP.Start(new FBPPConfig()
        {
            SaveFileName = SaveSystem.c_saveFileName,
            AutoSaveData = true,
            ScrambleSaveData = true,
            EncryptionSecret = c_encryptionSecret,
            SaveFilePath = path,
        });
    }

    private void Start()
    {
        if (_resolver == null)
        {
            Debug.LogError("[RunManager] _resolver is not assigned. Assign C6StatResolver in the inspector.", this);
            return;
        }
        if (_fuelManager == null)
        {
            Debug.LogError("[RunManager] _fuelManager is not assigned. Assign FuelManager in the inspector.", this);
            return;
        }

        StartRun();
    }

    /// <summary>
    /// Resolves all stats and initialises every run-start subsystem in dependency order.
    /// </summary>
    public void StartRun()
    {
        _resolver.TriggerAggregation();
        _fuelManager.Initialize(_resolver.GetContext());
    }
}
