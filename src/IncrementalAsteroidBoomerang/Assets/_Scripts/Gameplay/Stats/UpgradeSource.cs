using UnityEngine;

/// <summary>Base class for stat producers (P1a Skill Tree, G4 Mod System). Extend and return deltas from GetDeltas().</summary>
public abstract class UpgradeSource : MonoBehaviour
{
    public abstract StatDelta[] GetDeltas();
}
