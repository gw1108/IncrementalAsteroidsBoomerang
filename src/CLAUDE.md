## Coding Standards

- All public APIs require doc comments
- Gameplay values must be **data-driven** (external config files)

## Coupling Style

Prefer direct function calls over pub/sub or event-broadcast systems (EventRouter, UnityEvent, Action callbacks). Tight coupling between systems is fine — favor simplicity and stack-trace traceability over decoupling abstractions.

---

## FBPP (File Based Player Prefs)

Replaces `PlayerPrefs`. Saves data as JSON to a text file. **Keys are type-specific** — a string and an int can share the same key without conflict.

### Init (required before any get/set)
```csharp
FBPP.Start(new FBPPConfig
{
    SaveFileName = "saveData.txt",   // default
    AutoSaveData = true,             // default — writes on every Set
    ScrambleSaveData = true,         // default — light obfuscation
    EncryptionSecret = "my-secret",  // pick one and never change it
    // SaveFilePath defaults to Application.persistentDataPath
});
```
Call once at app startup (e.g., `Awake` of a persistent manager). All config fields are optional.

### Save / Load
```csharp
// Strings
FBPP.SetString("key", value);
string v = FBPP.GetString("key");               // returns "" if missing
string v = FBPP.GetString("key", "default");

// Ints
FBPP.SetInt("key", value);
int v = FBPP.GetInt("key");
int v = FBPP.GetInt("key", 0);

// Floats
FBPP.SetFloat("key", value);
float v = FBPP.GetFloat("key");
float v = FBPP.GetFloat("key", 0f);

// Bools
FBPP.SetBool("key", value);
bool v = FBPP.GetBool("key");
bool v = FBPP.GetBool("key", false);
```

### Utilities
```csharp
FBPP.HasKey("key");           // true if any type exists under key
FBPP.HasKeyForString("key");  // type-specific checks also available for Int/Float/Bool
FBPP.DeleteKey("key");        // deletes ALL types under key
FBPP.DeleteString("key");     // type-specific deletes also available for Int/Float/Bool
FBPP.DeleteAll();             // wipes entire save file
```

### Performance — Manual Save
If `AutoSaveData = false`, Sets are in-memory only until you flush:
```csharp
FBPP.Save(); // writes + encrypts to disk — call at natural save points, not per-frame
```
Use this when calling Set methods during active gameplay to avoid frame hitches.

**Warning:** Never change `EncryptionSecret` after shipping — it will break all existing save files.
