# INC-47 Research: Damage-State Shader Overlay (Visual Integrity Feedback)

**Date:** 2026-04-25  
**Ticket:** [INC-47](https://linear.app/incremental-asteroid-boomerang/issue/INC-47/damage-state-shader-overlay-visual-integrity-feedback)  
**Parent:** [INC-14 — G5 Asteroid Mining](https://linear.app/incremental-asteroid-boomerang/issue/INC-14/g5-asteroid-mining)

---

## Summary

Implement a per-sprite shader effect that shows asteroid/enemy integrity via a horizontal scan line. Above the line: desaturated (damaged). Below the line: normal sprite. White line sits at the threshold. As damage accumulates, the line moves from top toward bottom.

---

## Codebase Findings

### Render Pipeline

- **URP 2D** (Universal Render Pipeline, 2D Renderer)
- Config: `Assets/Settings/UniversalRP.asset` + `Assets/Settings/Renderer2D.asset`
- **SRP Batcher is enabled** (`m_UseSRPBatcher: 1`)
- ShaderGraph is the correct authoring tool for URP — do NOT write raw HLSL shaders

### Existing Shaders

- `Assets/Shaders/` directory **exists and is empty** — ready for new shader
- Only TextMesh Pro shaders are present; no gameplay shaders
- No existing material property manipulation anywhere in the codebase

### Damage System — Current State (`BoomerangTarget.cs:1–20`)

```csharp
public class BoomerangTarget : MonoBehaviour
{
    public bool IsAlive { get; private set; } = true;

    public void TakeDamage(int damage, Vector2 contactPoint)
    {
        if (!IsAlive) return;
        IsAlive = false;
        gameObject.SetActive(false);  // one-hit kill, no integrity tracking
        FuelManager.Instance.OnTargetKilled();
    }
}
```

**Key gap:** `BoomerangTarget` is binary (one-hit kill). INC-47 requires multi-hit integrity tracking. This will also be needed by INC-14's "integrity stages" sub-task.

### Prefab Structure

Both `Assets/Prefabs/Asteroid.prefab` and `Assets/Prefabs/EnemyBox.prefab`:
- Have a `SpriteRenderer` component
- Reference the same shared material slot (GUID `a97c105638bdf8b4a8650670310a4cd3`, currently missing/legacy)
- Layer 7

Each prefab instance needs its own per-instance shader parameters. Use `MaterialPropertyBlock` on the `SpriteRenderer` (no new material instances needed).

### Boomerang Damage Hook (`BoomerangController.cs:107–119`)

```csharp
public void OnProjectileContact(BoomerangTarget target, Vector2 contactPoint)
{
    int damage = ComputeDamage(_contactIndex);
    target.TakeDamage(damage, contactPoint);
    _contactIndex++;
}
```

The call site is clean — `TakeDamage` receives `damage` and `contactPoint`. Shader update can be driven directly from inside `TakeDamage`.

### Available Libraries

- **DOTween** is imported and used in `BaseScreen.cs` — available for animating the line sweep if needed
- Direct function calls preferred over events per `src/CLAUDE.md` coding standards

---

## Effect Specification

| Condition | Visual |
|-----------|--------|
| Full integrity | Line at very top (nearly invisible), all sprite normal |
| 50% damage taken | Line at mid-height, top half desaturated, bottom half normal |
| Near death | Line near bottom, almost all desaturated |
| Dead | Object disabled — no shader update needed |

**UV mapping:** UV.y = 0 at bottom of sprite, UV.y = 1 at top.  
**Shader parameter `_DamageCutoff`:** starts at 1.0, decreases toward 0.0 as damage accumulates.

`_DamageCutoff = currentIntegrity / maxIntegrity`

---

## Implementation Approach

### Step 1: Extend `BoomerangTarget` with Integrity Tracking

Add `MaxIntegrity` (serialized field) and current integrity counter. Death only when integrity hits 0. Expose `IntegrityNormalized` (float 0–1) for the shader.

```csharp
[SerializeField] private int _maxIntegrity = 3;
private int _currentIntegrity;

private void OnEnable()
{
    _currentIntegrity = _maxIntegrity;
    // Reset shader state here
}

public void TakeDamage(int damage, Vector2 contactPoint)
{
    if (!IsAlive) return;
    _currentIntegrity -= damage;
    float t = Mathf.Clamp01((float)_currentIntegrity / _maxIntegrity);
    _renderer.GetPropertyBlock(_mpb);
    _mpb.SetFloat(DamageCutoffId, t);
    _renderer.SetPropertyBlock(_mpb);
    if (_currentIntegrity <= 0)
    {
        IsAlive = false;
        gameObject.SetActive(false);
        FuelManager.Instance.OnTargetKilled();
    }
}
```

### Step 2: Create URP 2D ShaderGraph

File: `Assets/Shaders/DamageOverlay.shadergraph`

ShaderGraph node logic:
1. Sample `_MainTex` at UV
2. Compute `isAboveLine = UV.y > _DamageCutoff`
3. Compute `isOnLine = abs(UV.y - _DamageCutoff) < _LineWidth` (default 0.02)
4. If `isOnLine`: output white (`float3(1,1,1)`) at full alpha
5. If `isAboveLine` and not on line: desaturate using luminance weights `(0.2126R + 0.7152G + 0.0722B)`
6. Otherwise: pass through normal sprite color

Exposed properties:
- `_MainTex` (Texture2D) — sprite texture, set by Unity runtime
- `_DamageCutoff` (Float, range 0–1, default 1) — driven per-instance via MaterialPropertyBlock
- `_LineWidth` (Float, range 0–0.1, default 0.02) — thickness of white scan line

**Important URP 2D ShaderGraph settings:**
- Graph type: Sprite Lit or Sprite Unlit (prefer Sprite Unlit for flat 2D look)
- Enable alpha clipping if sprites have transparency
- Set render queue to Transparent

### Step 3: Create Material + Assign to Prefabs

- Create `Assets/Materials/DamageOverlayMat.mat` referencing the new ShaderGraph
- Assign to the material slot in both Asteroid.prefab and EnemyBox.prefab
- Default `_DamageCutoff = 1.0` so fresh spawns show undamaged

### Step 4: Per-Instance MaterialPropertyBlock

`BoomerangTarget` caches a `MaterialPropertyBlock` and the `SpriteRenderer` ref. Calls `SetPropertyBlock` on every hit. This avoids material instantiation while still allowing per-object shader state.

Note: `MaterialPropertyBlock` does break SRP batcher batching per-object, but with ~10–30 asteroids on screen the GPU cost is negligible.

---

## Risks & Constraints

| Risk | Mitigation |
|------|-----------|
| URP 2D ShaderGraph sprite alpha handling | Use `Sample Texture 2D` node with UVs from `UV` node; hook alpha to output alpha |
| SRP Batcher broken by MaterialPropertyBlock | Acceptable at current enemy counts; monitor if count scales |
| `_DamageCutoff` at exactly 1.0 shows line at top edge | Default to 1.0 but hide line when at full health via alpha = 0 branch |
| Shared prefab material — need per-instance state | MaterialPropertyBlock solves this cleanly |
| `BoomerangTarget` change affects all targets (including EnemyBox) | MaxIntegrity serialized per-prefab; EnemyBox can use 1 for single-hit death |

---

## Files to Create / Modify

| File | Action |
|------|--------|
| `Assets/Shaders/DamageOverlay.shadergraph` | Create new URP 2D ShaderGraph |
| `Assets/Materials/DamageOverlayMat.mat` | Create, reference new shader |
| `Assets/_Scripts/Gameplay/BoomerangTarget.cs` | Add integrity tracking + MaterialPropertyBlock |
| `Assets/Prefabs/Asteroid.prefab` | Assign DamageOverlayMat, set MaxIntegrity = 3 (or per GDD) |
| `Assets/Prefabs/EnemyBox.prefab` | Assign DamageOverlayMat, set MaxIntegrity = 1 |

---

## Open Questions for George

1. **How many integrity stages for asteroids?** (INC-14 mentions "smaller debris at defined thresholds" — is MaxIntegrity 3? tied to ore tier?)
2. **Should EnemyBox also show the damage overlay?** (It shares the material slot but 1-hit kills won't visually show the line)
3. **Line color** — always white, or should it match a tint color per enemy type?
4. **Should `_DamageCutoff` animate** (DOTween sweep) on each hit, or update instantly?
