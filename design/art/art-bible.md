# Art Bible: Incremental Asteroids Boomerang

*Created: 2026-04-23*
*Status: Draft — Section 1 approved; Sections 2–9 in progress*
*Direction: **Deep Forge** (AD-CONCEPT-VISUAL STRONG verdict 2026-04-23)*

> **Art Director Sign-Off (AD-ART-BIBLE)**: PENDING — will be recorded on completion of Section 9.

---

## Section 1: Visual Identity Statement

### The Frame

*Incremental Asteroids Boomerang* is designed under the **Deep Forge** direction: the visual language of a craftsman's workshop translated into space. Asteroids are raw ore. The skill tree is a schematic blueprint. The boomerang is the work. Every visual element in this game — environments, UI, VFX, typography — exists to reinforce a single shift in player identity: **you are not a pilot reacting to chaos. You are an engineer whose decisions are becoming unstoppable physics.**

This section is the root of the art bible. All downstream sections (color, shape language, asset specs, VFX budget, tree UI) derive from the principles stated here. If a downstream decision conflicts with this section, this section wins.

---

### The One-Line Visual Rule

> **You are building a weapon, not piloting a ship.**

This rule governs every visual hierarchy decision. When two choices are on the table, prefer the one that makes the player feel their *construction* — not their *movement*.

- **The boomerang is the protagonist.**
- **The ship is the hand that throws it.**
- **The tree is the forge.**

---

### Supporting Visual Principles

#### 1. Hue Is Reserved for Meaning

Every distinct hue in the game is owned by a specific gameplay role, and that ownership is exclusive and permanent. No two gameplay concepts may share a hue. New entities do not get new hues — they inherit or subordinate to an existing role's hue.

**Design test**: A new pickup type is proposed. Before assigning it a color, ask: which existing hue role does this pickup belong to? If it belongs to none, the role must be formally defined and approved before the asset is created. If it belongs to an existing role, use that hue at reduced saturation or value to signal subordination.

**Pillar anchor**: **P2 — Positional Mastery**. Distinct hues for enemies, hazards, currency enable spatial decision-making.

---

#### 2. Impact Owns Saturation

The highest-saturation moment in any frame must be a contact event: the boomerang striking an asteroid, enemy, or the return catch. Non-contact VFX (ambient particles, ship trail, arc indicator) must never approach the saturation ceiling. The white-yellow flash of impact is the loudest visual event the game ever produces — everything else is quieter.

**Design test**: A new VFX is proposed — say, a thruster exhaust glow or an ambient nebula pulse. Ask: is this a contact event? If no, its saturation must be measurably lower than the current lowest-saturation impact effect.

**Pillar anchor**: **P4 — Weighty Everything**. Weight is not communicated by numbers or text — it is communicated by the moment of contact being the undeniable visual climax of every boomerang cycle.

---

#### 3. Beauty Must Be Cheap

Visual quality in this game is achieved through geometry, sprite craft, and shape language — not shader complexity, glow passes, or high-resolution texture atlases. Aesthetic density is earned by drawing well within the WebGL constraint (<8 MB atlas, no post-process glow, sprite-based VFX). The Deep Forge aesthetic is made of facets, forge-lit darkness, and schematic linework — none of which require expensive rendering.

**Design test**: A visual treatment is proposed that requires a new gradient, a bloom pass, or a texture larger than fits the atlas budget. Reject it and reformulate: can the same mood be achieved with a drawn silhouette, a sprite animation, or a palette swap? If yes — and it almost always can — that is the correct answer. If no, escalate to technical-artist with a specific budget justification.

**Pillar anchor**: **P4 — The Tree IS the Game**. The skill tree must be a beautiful artifact the player actively wants to look at. That beauty must be durable across a 30-second cold load and a 3-minute run loop on a mid-tier browser. Expensive beauty is fragile beauty.

---

### Critical Tension — The Weight-Cost Paradox

**P4 demands that every hit feels heavy — premium, physical, undeniable. The WebGL platform constraint demands that the VFX which deliver that weight stay sprite-based and texture-lean.** These two requirements pull against each other at every impact effect, every asteroid shatter, every boomerang catch. This tension is not a problem to resolve; it is the defining creative pressure of the project.

The resolution is **craft, not budget**: weight is delivered through timing (hitstop frames, anticipation), shape (debris silhouettes, angular shatter patterns), and audio-visual sync — not through particle density or shader glow. Any VFX proposal that relies on "more particles" or "stronger bloom" to feel weighty is solving the wrong problem. The correct question is always:

> **Where is the hitstop, where is the flash frame, and is the debris silhouette readable?**

**Production implication**: the VFX artist and technical artist must treat every impact effect as a **hitstop-and-shape problem first, a texture problem second, and a shader problem never**.

---

## Section 2: Mood & Atmosphere

*Emotional and atmospheric target for each of the six primary game states. All lighting described here uses Unity 2D URP Light2D nodes, sprite-based VFX, and sprite shader properties (color tint, multiply/additive blend). **No bloom. No volumetric fog. No post-process of any kind.** If a desired mood cannot be expressed within those constraints, it must be reformulated — not escalated to a post-process budget that does not exist.*

---

### 2.1 In-Run (Exploration / Combat Blend)

**Primary mood target**: Controlled Pressure

The player is *working*. Not panicking, not triumphant — working. The forge is hot, the ore is moving, the boomerang is in the air. This is the game's resting state and must sustain 1–3 minutes without fatiguing the eye or numbing the player to impact events.

**Atmospheric descriptors**: Smoldering · Sparse · Readable · Pressured · Industrial

**Energy level**: Measured-to-frenetic (escalates with enemy density; baseline is measured)

**Lighting character**:
- **Background**: Near-black with warm undertone (#0D0A08 range — warm near-black, never cold blue-black). No ambient fill. The field is dark by default; only gameplay-relevant objects are lit.
- **Global Light2D**: Very low intensity (0.08–0.12), warm amber cast (#A06030) — the furnace glow from below, not overhead studio lighting.
- **Boomerang Point Light2D**: Tight electric-cyan Light2D attached to the boomerang. Intensity 0.4–0.6, steep falloff.
- **Asteroid Point Light2D (ore-bearing only)**: Small, very dim warm-amber, intensity 0.1–0.2. Non-ore asteroids receive no light.
- **Shadow/normal-map direction**: Normal-mapped sprites set their normal-map light direction to upper-left (10–11 o'clock) consistently across all asteroid sprites.
- **Enemy lighting**: No dedicated enemy light. Dark crimson sprites against near-black. Hue handles spatial distinction.

**Anchoring visual element**: The boomerang trail **dulls progressively as fuel depletes**. At full fuel, a sharp electric-cyan line with a short additive-blend tail. At 25% fuel, desaturated to a steel-blue grey and shortened — the weapon is cooling, the forge is running low. Single sprite-shader lerp on trail color; costs nothing, communicates fuel state without a UI number.

**Distinction from adjacent (Boss Engagement)**: In-run lighting is sparse and stable — only the boomerang moves the light. Boss engagement adds a second dominant light source (the boss) and raises the global ambient — the field gets brighter and more chaotic precisely to signal that something larger has entered it.

---

### 2.2 Boss Engagement

**Primary mood target**: Mounting Dread

The boss is not a bigger asteroid. It is a *presence*. The field does not become a boss arena — the boss enters the player's workspace and starts filling it. The mood is the specific dread of something too large for the room.

**Atmospheric descriptors**: Oppressive · Encroaching · Tense · Saturated · Confrontational

**Energy level**: Tense, escalating to frenetic

**Lighting character**:
- **Global Light2D raise**: On boss spawn, global ambient intensity steps up to 0.20–0.30 (from in-run's 0.08–0.12). Color shifts from warm amber toward cooler, slightly desaturated red-amber (#8A4020). The field gets **less dark** — which counterintuitively feels more ominous because the player can now see more of what is threatening them. Implemented as a 1–2 second timed lerp on Global Light2D color + intensity.
- **Boss Point Light2D**: Large-radius, low-intensity dark-crimson Light2D centered on the boss. Intensity 0.25–0.35, radius covering ~40% of the field. Crimson — the enemy hue — visually contaminates the field.
- **Boomerang light (unchanged)**: The cyan must remain visually dominant over the boss's crimson wash. Tune boomerang light intensity/saturation to win that competition. The boomerang is still the protagonist.
- **Vignette via sprite overlay**: Full-screen dark sprite at low alpha (0.15–0.25), multiply blend, tightens the visible field toward center. Hand-drawn sprite, not a post-process vignette.

**Anchoring visual element**: The boss casts a **slow, irregular crimson pulse** — a freeform Light2D whose intensity keyframes between 0.20 and 0.35 on a **2–3 second irregular interval**. The pulse is *not* rhythmic (rhythm would be readable and safe-feeling); it is slightly irregular (unpredictable, threatening). Animation clip on the Light2D intensity property — zero shader cost.

**Distinction from adjacent (In-Run)**: In-run is dark, sparse, and stable. Boss engagement is brighter, red-tinted, and breathing. The ambient raise is the tell. If the field suddenly feels less like a dark workshop and more like *the inside of something's throat*, the transition is working.

---

### 2.3 Death / Run End / Return to Shop

**Primary mood target**: Spent

Not failure. Not punishment. The run is over — fuel hit zero or the boss fell — and the player is being carried back. This state must not feel like a loss screen (invites frustration) or a victory screen (wrong for fuel death). It must feel like *decompression*.

**Atmospheric descriptors**: Cooling · Dimming · Quiet · Settling · Amber-washed

**Energy level**: Contemplative

**Lighting character**:
- **Field fade**: All gameplay lights (boomerang, asteroid, boss if applicable) fade to zero over 1.5–2 seconds. Global ambient dims to 0.05–0.08 — the lowest it ever reaches. The field becomes its darkest, most forge-interior self.
- **Warm amber wash**: As gameplay lights fade, a single large-radius warm amber Light2D (centered on ship, radius = full field) fades in slowly — intensity 0.15–0.25, color #C07830. The amber is the ore hue — the thing you were mining — now bathing everything. **The value you collected is what's left.**
- **Ship sprite tint**: Slightly desaturated, value-lowered, as if the engine is cooling. Material tint change, not a new sprite.
- **No new effects**: The mood is subtraction, not addition.

**Anchoring visual element**: The boomerang trail **extinguishes last** — half a second after all other lights have faded. The final image before the skill tree transition is the ship sitting in warm amber with a ghost-trail of cyan still fading from the boomerang's last position. **The weapon outlasts the run.**

**Distinction from adjacent**: Death feels different from Zone Victory because Zone Victory *brightens and reaches upward* while Death *dims and settles*. Death differs from In-Run because In-Run has cyan in the field; Death has only amber. **The absence of cyan is the visual signal that the run is over.**

---

### 2.4 Skill Tree

**Primary mood target**: Deliberate Craft

The player is at the forge, planning. This is P5's domain — the tree must be a beautiful artifact the player *actively wants to look at*. The atmosphere is a draftsman's table at night: focused light on the work, the rest of the room in comfortable shadow.

**Atmospheric descriptors**: Schematic · Warm · Weighty · Precise · Inviting

**Energy level**: Contemplative

**Lighting character**:
- **Background**: The deep warm near-black of the in-run field, but *stilled* — no moving asteroids, no field debris. The forge interior at rest. #0D0A08 warm near-black base.
- **Parchment panel lighting**: The skill tree panel itself is lit by a soft, wide Point Light2D placed **below** the panel center — simulating forge-heat rising from beneath the schematic. Color #C08040 (warm amber-gold), intensity 0.3–0.4, radius wide enough to fade at panel edges. **A document lit by the thing it describes.**
- **Unlocked node lights**: Each unlocked node gets a small Point Light2D — color matching the node's mechanic hue (amber for ore nodes, cyan for boomerang-power nodes, neutral gold for general). Intensity 0.15–0.25. Like lit candles on a map. They accumulate as the tree fills — **the tree grows visibly brighter as the player's build develops. This is the P1 visual metaphor.**
- **Locked node treatment**: No Point Light2D. Sprite-shader desaturation → faint ink on parchment, visible but inert. The contrast between lit and unlit is the primary navigation signal.
- **Gold linework**: Prerequisite lines rendered in aged-gold (#B8962E), 60–70% opacity when connecting locked nodes, 100% opacity when connecting two unlocked nodes. Lines glow via additive-blend from node lights — no dedicated line light. **The light from unlocked nodes naturally illuminates the path of the player's build.**

**Anchoring visual element**: The background parchment has a **very subtle, very slow warm pulse** on the bottom-center Point Light2D — intensity oscillating 0.28 ↔ 0.38 over a 4-second period. **The forge breathes. The schematic is alive.** One node, four keyframes, loops forever.

**Distinction from adjacent**: The skill tree is the only state where warm amber dominates without any cyan competing for attention. The boomerang does not exist here (the player is at the forge, the weapon is being redesigned). **In-run has cyan. The skill tree has gold. This hue distinction is the clearest signal in the game that the player has changed modes.**

---

### 2.5 Main Menu / Boot

**Primary mood target**: The Promise

A cold browser visitor has 3–5 seconds before the stay-or-leave decision is made. The main menu must communicate the core fantasy *without a tutorial, cutscene, or text they will not read*. What it must show: **there is a weapon, it moves with purpose, and something large is in your way.**

**Atmospheric descriptors**: Dramatic · Focused · Sparse · Confident · Unspoken

**Energy level**: Tense (held breath before the first throw)

**Lighting character**:
- **Field**: Same near-black warm background as in-run. No stars, no nebula, no ambient space decoration. Empty except for what matters.
- **Idle boomerang light**: The boomerang is present in the menu — positioned center-left, not moving, its Point Light2D active at full in-run intensity. **The only moving or glowing element in the frame before interaction.** The player's eye goes there first. It says: this object is the game.
- **Distant asteroid silhouettes**: 2–3 large asteroid silhouettes at frame edges, unlit, steel-grey. Spatial scale and implied resistance. Not focus; context.
- **Title text lighting**: Game title above the boomerang, blueprint-gold linework. Soft warm Point Light2D beneath the title — intensity 0.2, color #A07830 — makes the title feel lit by the same forge heat as the skill tree. **The schematic is signing its work.**
- **Menu option treatment**: Start / Settings in blueprint aesthetic, low luminance. They do not compete with the boomerang for visual attention.

**Anchoring visual element**: The boomerang performs a **single idle animation loop** — a slow, 3-second clockwise rotation in place, with its cyan Light2D rotating correspondingly. Unhurried and precise: **not a restless fidget, a tool being held, ready.** Weight and readiness simultaneously. **No other element on the screen moves.**

**Distinction from adjacent**: The main menu is the sparsest state in the game — fewer lit objects than in-run, fewer warm elements than the skill tree, no crimson of any kind. It is a held breath. **The moment Start is pressed, the forge wakes up**: global ambient raises, asteroids populate, the boomerang's first throw begins. The contrast between this quiet and that ignition is the first dopamine hit the game delivers.

---

### 2.6 Zone Victory

**Primary mood target**: You Built This

The boss is dead. The zone is clear. This is not generic triumph — it is the specific satisfaction of a craftsman looking at something they made and watching it work. The player did not dodge their way to victory; they *engineered* it across multiple runs. Visual language must make that authorship legible: **this win came from the forge.**

**Atmospheric descriptors**: Luminous · Earned · Expansive · Reverberant · Golden

**Energy level**: Triumphant (but not frantic — the work is done, not still happening)

**Lighting character**:
- **Global ambient raise**: Global Light2D lifts from in-run 0.08–0.12 to **0.40–0.50 over 1.5 seconds** — the highest ambient intensity in the game. Color shifts from warm amber (#A06030) toward a brighter saturated gold (#C09040). **The field has never been this bright. The forge is at full heat.**
- **Boomerang return flash**: On the kill hit, the boomerang's Light2D fires a **one-frame maximum-intensity spike** (intensity 2.0–3.0 for ~0.016–0.032s) before settling to a sustained elevated glow (0.8–1.0) for the duration of the victory state. **The game's loudest single visual event — brighter than any in-run impact flash, because it is not just an impact, it is a culmination.** Single-frame keyframe spike; two keyframes one frame apart.
- **Field warmth**: All asteroid debris on field tinted toward warm amber via sprite material tint — as if forge heat is radiating outward from the kill point. One material property set; no new lights.
- **Zone unlock visual**: New schematic line appears in the UI — path to next zone, blueprint gold linework, extending from current zone node outward. Brief draw-on animation (left-to-right, 0.5s). **The forge has extended its reach.**
- **No crimson in frame**: The boss's crimson light dies with the boss. The field goes from crimson-contaminated to gold-luminous. **This color transition is the victory: the threatening hue is replaced by the earned hue.**

**Anchoring visual element**: The boomerang **hovers in its post-catch position and does not immediately re-enter the throw cycle.** For 2–3 seconds it simply sits, glowing at elevated cyan intensity, with the field lit gold beneath it. No UI prompt yet. No text. **Just the weapon in the light. The forge is looking at its own work.** After 2–3 seconds the skill tree transition begins.

> **Implementation protection rule**: This 2–3 second held pause is an art-direction-mandated beat. Programmers must not skip it to show UI sooner. The pause is the section; without it, Zone Victory is not Zone Victory.

**Distinction from adjacent**: Zone Victory differs from In-Run because the field is bright, not dark. Differs from Death/Run End because Death dims and cools while Zone Victory lifts and warms. Differs from Skill Tree because Zone Victory is kinetic and momentary (a beat, not a browsable state). **Zone Victory is the emotional hinge**: the run ends in gold, the tree opens in gold, and the player carries that warmth into their next planning session.

---

### Mood Contrast Matrix

*Quick-reference to confirm each state is visually distinct. If two rows share all three values, one state must be sharpened.*

| State | Ambient Level | Dominant Hue | Energy Signature |
|---|---|---|---|
| In-Run | Very low (0.08–0.12) | Cyan (boomerang) on near-black | Measured to frenetic |
| Boss Engagement | Low-medium (0.20–0.30) | Crimson wash over amber | Tense, escalating |
| Death / Run End | Minimal (0.05–0.08) | Amber only, cyan fading | Cooling, still |
| Skill Tree | Low (parchment-local) | Gold / aged parchment | Contemplative |
| Main Menu | Very low | Cyan point on near-black | Held breath |
| Zone Victory | High (0.40–0.50) | Gold-luminous, no crimson | Triumphant, momentary |

No two rows share the same combination of all three values. The matrix holds. **Production use**: when proposing a new VFX, confirm it does not blur two rows of this matrix. If it does, sharpen one.

---

## Section 3: Shape Language

*The geometric vocabulary of the game. Every object in *Incremental Asteroids Boomerang* belongs to one of two registers — **hero** (constructed) or **supporting** (found). Shape rules are absolute: violating them breaks the Deep Forge identity.*

---

### 3.1 Silhouette Philosophy — The 32×32 Rule

Every gameplay object must be distinguishable from every other gameplay object at **32×32 pixels in the player's peripheral vision**. Not aspirational — technical definition of visual legibility.

**Why 32×32 specifically**: At 1080p with a 512×512 gameplay field, enemies and asteroids at medium range occupy roughly 24–40 pixels per side. The player is tracking the boomerang arc and reading density. Everything else communicates through peripheral silhouette alone.

**Required distinguishing traits — one per archetype, non-negotiable**:

| Archetype | Required Distinguishing Silhouette Trait |
|---|---|
| Boomerang | Asymmetric elongated wedge — wider at strike end, pointed tail; always the longest, most horizontal object in frame |
| Ship | Compact, low, nearly equilateral — smallest autonomous object in frame |
| Grunt enemy | Jagged irregular polygon; radially spiky; roughly symmetrical about vertical axis |
| Ranged enemy | Taller-than-wide silhouette; 1–2 protruding limbs/barrels pointing outward |
| Tank enemy | Widest enemy silhouette; lower center of mass; hunched and blocky |
| Basic asteroid | Faceted convex polygon; 6–8 sides; roughly equilateral |
| Dense asteroid | Same base as Basic but squatter, 9–12 sides, larger overall |
| Exotic asteroid | Elongated non-convex — one or more concavities as if crystal growth broke its outline |
| Boss | ≥3× the area of the largest enemy; multiple readable sub-shapes reading as components |

> **Production rule (BLOCKING pipeline step)**: Before any object is animated, composite the final idle frame against black at 32×32. If the type is ambiguous at that scale, revise the silhouette. This check is a blocking step in the asset pipeline, not a nice-to-have.

**Pillar anchor**: **P2 — Positional Mastery**. Field mastery requires the player to read object types at a glance from any position.

---

### 3.2 Hero Shape vs. Supporting Shape

The game has two geometric registers. Every object belongs to exactly one.

#### Hero register — Boomerang, Ship, Skill Tree

Objects that read as *made*, not found. Angular precision, clean edges, intentional asymmetry, patterned wear (not random erosion).

- **Primary forms**: trapezoids, elongated wedges, rectilinear panels
- **Secondary detail**: scored parallel lines, rivet/bolt implication, modular panel seams
- **Curvature**: strictly prohibited on primary contours
- **Symmetry**: Ship bilaterally symmetric on horizontal axis. Boomerang deliberately asymmetric (heavy end vs light end). Tree nodes radially symmetric (hexagonal). None organic.

#### Supporting register — Asteroids, Enemies, VFX debris

Objects that read as *natural* or *improvised*, not engineered. Irregular faceting, asymmetric weight, unpredictable edge angles.

- **Primary forms**: irregular polygons, jagged spikes, mismatched-angle faceted planes
- **Secondary detail**: rough edge variations, implied erosion/fracture lines, asymmetric protrusions
- **Curvature**: permitted only as an interior facet feature (concave scoops on faces); not on outline
- **Symmetry**: strongly discouraged. Enemies asymmetric across vertical axis by default. Bosses asymmetric in ≥2 sub-component placements.

> **The critical distinction at a glance**: If an object looks like it was *drawn with a ruler*, it is hero. If it looks like it was *broken off a larger thing*, it is supporting. These two registers must not blur.

**Pillar anchor**: **P4 — Weighty Everything**. Hero objects that look constructed and supporting objects that look found creates an implicit conflict at contact: a *made* thing striking a *found* thing. That contrast is part of what gives impacts their weight.

---

### 3.3 Environment Geometry — Angular-Crystalline

The field is a forge interior translated to space. Its geometry is defined by the asteroid population — mineral matter with crystal structure.

**What dominates the field**: Multi-faceted silhouettes with flat planes meeting at sharp angles. Nothing smoothly curved in the background.

**How shape carries Deep Forge without relying on color**:
- **Mass through angularity**: A faceted polygon with 8–12 sides reads as more massive than a simple quad at the same scale. Dense asteroids use more facets precisely for this — the eye reads more surface planes as more material.
- **Implied fracture**: Angle relationships between facets must suggest the asteroid was *broken* from a larger mass, not formed into a sphere and chipped. Adjacent faces have mismatched angles implying internal fault lines. Sprite design instruction: when drawing facet geometry, ask *"where would this break next?"* and let that dictate the next edge.
- **The grounding lie**: All asteroids, regardless of drift trajectory, appear to have a gravitational "floor" implied by one flat face oriented roughly horizontally. Objects with a flat bottom read as heavier than those without. Sprites authored with this in mind, then rotated during drift within ±25° of authored orientation to preserve the grounding read.
- **Void as negative space**: The background near-black is not empty — it is the *absence of material*. Sparse populations are a deliberate composition ("the forge has already worked here"), not scarcity of content.

**Pillar anchor**: **P3 — Read the Arc**. Faceted field geometry with flat-face structure creates consistent negative-space gaps that the boomerang's cyan trail can occupy without visual occlusion. Organic/cloudy field geometry would compete with the arc.

---

### 3.4 UI Shape Grammar — Adjacent but Not Identical

UI shares the world's angular flat-plane vocabulary but operates at a different level of precision. World objects are broken or forged with physical imprecision — irregular facets, scored edges. UI objects are **drafted** — ruled angles, grid-locked alignments, intentional marks rather than implied wear.

> **The analogy**: The world is the forge floor. The UI is the schematic pinned above the forge. Both exist in the same building; neither is confused for the other.

**Specific grammar rules**:

- **Hexagonal language is UI-exclusive for nodes.** Hexagons do not appear in the world. They are the signature shape of the skill tree. If a hex ever appeared on an asteroid or enemy, it would read as interactive — a false signal.
- **Schematic lines differ from world edge lines.** World objects have thick, irregular, hand-drawn-style outlines. UI connector lines are **thin, ruled, consistent-weight wiring-diagram strokes**. A connector and an asteroid edge must not be confusable peripherally.
- **HUD uses rectilinear panel geometry with chamfered corners** (one or two clipped angles, not radiused). The chamfer is the one shared geometric mark between hero register and UI — signals both are *constructed objects*, not field matter.
- **NO organic shape in UI.** No rounded rectangles. No circular health bars. No pill-shaped buttons. Hex for tree nodes; rectilinear chamfered for HUD/panels; ruled lines for connectors. The UI is a blueprint, not a bubble.

#### Skill tree — explicit treatment

1. **Hex tiles**: Regular hexagon, all six sides equal, 120° interior angles. Always same size. State indicated by fill/outline treatment, not scale. Locked = outline only desaturated. Unlocked = full fill + border. Hover = thin inner-hex additive-blend ring.
2. **Connector lines**: Single straight line between adjacent node centers along one of six hex-grid cardinal directions. **No connectors deviate from hex-grid angles.** Line weight 1–2px, consistent, not tapered, not dashed. Terminates 2px before node edge.
3. **Cluster grouping marks**: Mechanic archetype clusters bounded by faint thicker outline polygon tracing the hull of the cluster's nodes, hex-grid-angle edges. Subtle background tint diff (near-invisible at rest, readable on focus).

**Pillar anchor**: **P5 — The Tree IS the Game**. Hex precision + ruled lines + cluster marks create a spatial language reading as *a map of decisions made and available* — a schematic of build identity. Rounded menus or radial dials would read as interface. Hex + schematic lines read as *document*.

---

### 3.5 Per-Object-Class Shape Rules

#### 3.5.1 Boomerang — Base and Mod Archetypes

**Base silhouette**: Asymmetric elongated wedge — wider/blunter at strike end, tapering to narrower *truncated* tail (not pointed — cut). 3:1 aspect ratio (length:max width). Strike edge has one flat face at ~30° from long axis — creates the "hit zone" visual where contact registers.

> **This silhouette is immutable across all mod archetypes. The base shape does not change. Mods add to it; they do not replace it.**

**Mod expression rule — Additive Silhouette, Not Replacement Silhouette**:
Each mod adds a readable visual indicator that does not alter base silhouette's overall width/length beyond **±15%**. Player must identify trajectory from any variant. Indicators stack visibly; multi-mod configs read as layered additions to the same base form.

- **Pierce mod** — single thin blade-fin from the blunt strike face, parallel to long axis, ~60% of weapon length, ≤12% of weapon width. Steel-grey, no new hue.
- **Chain mod** — 2–3 angular ring/hook protrusions on side face, evenly spaced. Angular not circular (closed hex/oct loop at this resolution). Max +15% lateral width. Steel-grey or darker accent.
- **Explode-on-Return mod** — single angular charge-block bolted to flat tail end, ~40–50% of weapon cross-sectional width, with a **small amber accent mark (≤20% of block face, desaturated vs pickup-amber)**. This is the one place amber appears on the boomerang — signaling "stored energy converts to ore/currency on return."
- **Multi-throw (stretch)** — secondary smaller wedge mirrored at tail, 60–70% size of primary, reading as two-headed weapon. Only mod extending overall length (~+30%). Primary wedge must remain dominant silhouette read.

**P3 protection rule**: Each mod indicator must pass the 32×32 test **while still reading as the boomerang**. Question: can a player who has seen the base boomerang recognize any modded variant as "the boomerang, plus something" rather than "a new object"? If no, indicator is too large or too divergent.

**Pillar anchor**: **P3 — Read the Arc**. Muscle memory for "where is the weapon" attaches to the base form. Additive indicators keep the base form always present and trackable.

---

#### 3.5.2 Ship — Base and Tree-Fill Evolution

**Base silhouette**: Compact low-slung trapezoid, wider at base than nose. ~40% the longest dimension of the boomerang, ~1.2:1 width:height. Reads as *tool housing, not vehicle*. Two angular rectilinear thruster nozzles at rear. No cockpit dome, no windscreen curve, no aerodynamic wing sweep.

**Tree-fill evolution — attachment-based only**. Base silhouette never replaced, only added to.

**Permissible attachments**:
- Rectilinear panels bolted to hull (defensive node clusters → armored read)
- Angular fin-extensions at thruster nozzles (fuel-economy/speed clusters)
- Mounted hardpoints on hull top-face (weapon-adjacent nodes)

**Prohibited attachments**:
- Organic protrusions (nothing grown/biological)
- Curved add-ons (no rounded fairings or domed sensor arrays)
- Wing sweeps (no aerodynamic shaping making the ship read as a fighter)
- Cockpit/passenger-space indicators (the ship is not a vehicle)

**Max evolution**: At full tree fill, ship silhouette ≤150% base area. Additions make the ship look more *equipped*, not larger. Ship is always smallest autonomous object in field.

**Pillar anchor**: **P1 — Multiplicative Dopamine**. Ship evolution is a secondary P1 signal — visible read of "how much you have built." Attachments unlock at **node-cluster thresholds, not per-node** — visual change is threshold-felt, not incremental-drift.

---

#### 3.5.3 Asteroid Types

**Basic**: 6–8 sided convex irregular polygon. Roughly equilateral footprint (width ≈ height ±20%). Face count suggests single mineral seam — workable. One implied horizontal grounding face. No interior markings at sprite resolution. Medium size.

**Dense**: 9–12 sided convex polygon, squatter than Basic (height ~80% of width). Larger mass. Increased facet count = "more surface planes = more mineral content." **Secondary fracture line** scored on sprite body (disappears at 32×32, reads at in-game scale). One face has a small angular recess — compressed crystalline structure. Medium-large, visibly heavier than Basic.

**Exotic**: Non-convex. One or two angular concavities re-entering the outline — profile reads as *crystalline growth extended unevenly*, not a broken chunk. Elongated ~2:1 on one axis. Interior shows angular crystalline facet marks (visible in-game, gone at 32×32). **No implied grounding face** — drifts as a floating crystal fragment. Medium, narrow concentrated mass.

**Pillar anchor**: **P2 — Positional Mastery**. Player routes through the field toward high-yield asteroids. That decision requires yield type read *before* route commitment. Shape is the zero-cost signal.

---

#### 3.5.4 Enemy Archetypes

**Grunt**: Equilateral irregular polygon with **5–7 outward radial spikes**. Reads as "omnidirectionally aggressive." Roughly as wide as tall (matches Basic asteroid footprint at medium range). Distinguished from asteroids by spikes (vs facets). Erratic drift with rotational motion exposing spike array.

**Ranged**: Taller than wide (~1.5:1 height:width — unique among enemies). **1–2 elongated directional protrusions** (longer than Grunt spikes relative to body size). Reads as "directed threat, pointed outward." Less chaotic than Grunt.

**Tank**: Widest enemy (~1.6:1 width:height — unmistakably horizontal). **Low center of mass** — visual bulk in lower half, upper smaller and angular. Fewer spikes; thick armor-plating protrusions. Reads as "too much mass to kill quickly" — resistance, not aggression.

**Zone 2 re-skin rule**: Zone 2 variants **share silhouette with Zone 1 variants**. Hue shifts only (deeper or shifted crimson per Section 2). **Shape is never the zone differentiator.** Player must recognize archetype at 32×32 regardless of zone.

**Pillar anchor**: **P2 — Positional Mastery**. Positioning to maximize chain-pierce through clusters requires type distinction.

---

#### 3.5.5 Boss Silhouettes

Both MVP bosses must read as bosses **before any attack pattern is observed**.

1. **Minimum area**: ≥3× the area of the largest enemy in the zone. Boss is always visually the largest object in the field except accumulated asteroid debris.
2. **Multiple readable sub-components**: Silhouette decomposes into 3–5 distinct sub-shapes legible as separate elements at field scale. Creates natural phase targets (components damaged, retracted, or destroyed as phase transitions).
3. **Asymmetric component placement**: Sub-components NOT symmetric. One side more mass/more protrusions than the other. Reads as improvised or evolved — supporting-register vocabulary. **Bosses are large *found* objects, not large *constructed* objects.**
4. **At least one oversized protrusion**: ≥50% beyond main body mass. Reads as primary threat limb before player sees it used.
5. **Silhouette contrast at 64×64**: Must be distinguishable from asteroid cluster of equivalent area. Distinguishing feature: sub-component decomposition (boss is *contiguous but articulated*; asteroid clusters have gaps between individual rocks).

**Pillar anchor**: **P4 — Weighty Everything**. A boss must feel heavy before it acts. Silhouette communicates weight before first attack frame.

---

#### 3.5.6 Skill Tree Node and Connector Geometry

**Node**:
- Regular hexagon, precise (never approximated)
- Base size: contains a 24×24 icon with 4px padding all sides
- Interior holds one of: icon for mechanic, text label for named milestones, cluster archetype symbol for gateway nodes — within inner hexagon at 85% of outer hexagon's inscribed circle
- State indicators: overlay layers, not geometry changes
  - Locked: outer hex outline only, desaturated
  - Unlocked: full fill + border
  - Hover: inner hex additive-blend ring
  - Selected/active: small anchor dot at center

**Connector**:
- Single straight line between adjacent node centers along one of six hex-grid cardinal directions
- 1–2px constant weight, not tapered, not dashed
- Terminates 2px before node edge (connects without overlapping)
- Fork connectors use explicit T-junction at grid angles (not a Y, not curved split)

**Pillar anchor**: **P5 — The Tree IS the Game**. Grid discipline signals every node's position is meaningful. Looseness would dissolve the schematic quality.

---

### 3.6 Pillar Anchor Summary

| Pillar | Where it appears in Section 3 |
|---|---|
| **P1 — Multiplicative Dopamine** | Ship evolution (3.5.2): attachments at node-cluster thresholds → threshold-felt visual jump |
| **P2 — Positional Mastery** | 32×32 legibility (3.1); enemy archetypes (3.5.4): distinct types; asteroid types (3.5.3): visual distinction |
| **P2 — Mastery in where the player moves** | Boomerang mods (3.5.1): additive-not-replacement rule preserves base silhouette |
| **P4 — Weighty Everything** | Hero vs supporting register (3.2): contact events get charged contrast; boss silhouettes (3.5.5): boss mass reads before first attack; the no-curves rule (3.7): angular = heavier perception |
| **P5 — The Tree IS the Game** | UI shape grammar (3.4): hex precision + ruled lines; node/connector geometry (3.5.6): no geometric looseness |

All five pillars anchored. P2 and P3 carry the most rules (as expected).

---

### 3.7 Geometric Prohibition — NO CURVES ON PRIMARY OBJECT CONTOURS

**Absolute rule**: Not on the boomerang. Not on the ship. Not on enemies. Not on asteroids. Not on bosses. Not on UI elements.

**Definition**: "Curves on primary contours" means a smooth arc as any portion of the outer bounding silhouette of a gameplay object or UI element. Radiused panel corners, dome cockpits, round enemy bodies, circular health bars — all prohibited.

**Why this is the single most load-bearing prohibition**:

Curves communicate two things in shape language: **biological life** and **manufactured safety**. Neither belongs in Deep Forge.

- **Biological curves** would read as creature-design — this game's enemies are improvised machines, not organisms.
- **Manufactured-safety curves** (rounded buttons, pill shapes) would read as consumer-product design — the opposite of schematic precision.

The Deep Forge aesthetic is built on the **forged-versus-natural dichotomy**. Both artisanal objects (hero register) and raw material (supporting register) are angular because:
- **Forged objects** have sharp angles from controlled impact/cutting
- **Ore and mineral matter** has sharp angles from crystalline fracture and geological pressure

The one object type that would naturally have curves is something **grown** — biological, organic. Introducing curves anywhere would silently suggest biology and soften the industrial-forge identity.

#### The chamfer exception

Chamfered corners (single-cut clipped angles, not radiused arcs) are permitted on **hero-register and UI objects only** — HUD panels, ship hull panels, tree node outlines on decorative flourishes. A chamfer is an *additional flat face*, the constructed-object equivalent of a beveled machining mark. **Chamfers never appear on supporting-register objects** (enemies, asteroids) where angled edges are fractures, not machining marks.

**Pillar anchor**: **P4 — Weighty Everything**. A rounded object looks lighter than an angular object of equivalent size (documented perceptual effect in industrial design). Every curve removed from the game's shape language is weight returned to it.

---

## Section 4: Color System

*The complete, production-ready color specification. No color used in production may exist outside the palette below unless formally added through the amendment process in Section 4.1.*

---

### 4.1 Primary Palette — Definitive

| # | Palette Name | Hex | Role (Reserved For) | Value / Saturation Notes |
|---|---|---|---|---|
| 1 | **Forge Black** | `#0D0A08` | Background / void / empty space | Fixed. Never tinted in-run. Shifted only via overlaid Light2D tints during transition states, never by changing this value. |
| 2 | **Weapon Cyan** | `#00CFDB` | Boomerang — body, trail at full fuel, Point Light2D color — **exclusive** | Full saturation at this hex. Trail desaturation lerps toward Spent Cyan as fuel depletes. Never applied to any other object. |
| 3 | **Spent Cyan** | `#5A8A8F` | Boomerang trail at fuel depletion / post-run ghost trail | Low-fuel trail target only. Not a standalone object color. Never in UI. |
| 4 | **Ore Amber** | `#C07830` | All pickups (ore, currency, XP) — "things that pay you" | Full saturation at `#C07830`. Contextual lighting shifts: `#A06030` (global ambient low-intensity), `#C08040` (skill tree forge-heat warmer). Hue stays 25–40° regardless of value shift. |
| 5 | **Pirate Crimson** | `#8A1A1A` | Zone 1 enemies — Grunt / Ranged / Tank / Boss / enemy projectiles — **exclusive** | Zone 1 base. Value shifts within class for type differentiation. Hue 0° ± 10° always. |
| 6 | **Deepwater Crimson** | `#6B2030` | Zone 2 enemy hue shift — same silhouettes, cooler-red tint | Zone 2 only. Sole zone differentiator for enemies. |
| 7 | **Ore Stone** | `#4A4640` | Asteroid body — all types | Desaturated warm grey. Facet accent `#5C5450`. Zone 2 shift: `#3D3C42` (cooler violet-grey). Hue must never approach cyan or amber. |
| 8 | **Impact White** | `#FFFFF0` | Impact flash peak — contact moments only | Appears <0.2s exclusively. Blends toward Impact Gold over flash duration. Never held. Never in UI. |
| 9 | **Impact Gold** | `#FFE040` | Trailing edge of impact flash — warm falloff after white peak | Flash tail only. Never independent. |
| 10 | **Schematic Parchment** | `#C8B87A` | Skill tree background panel base — aged document substrate | Desaturated to `#8A7C56` for locked-region panel areas. UI-exclusive. |
| 11 | **Blueprint Gold** | `#B8962E` | Skill tree linework, node outlines, connector lines — schematic language | 60–70% opacity (locked connectors) / 100% (unlocked). UI-exclusive. |
| — | *(Reserved)* | — | Post-MVP Zone 3 differentiator | Formal amendment required before any Zone 3 asset authored. |

> **Amendment rule**: Adding any color to this palette requires written justification of which hue role it occupies and explicit confirmation that it does not collapse any existing role under colorblind simulation (Section 4.5). **Undocumented colors in production assets are a blocking pipeline failure.**

---

### 4.2 Semantic Color Vocabulary

One sentence per color — what it *means* to the player perceptually, before conscious thought.

- **Forge Black** `#0D0A08` — The forge interior; you are inside something that works, not drifting through empty space.
- **Weapon Cyan** `#00CFDB` — This is your weapon, your construction, the physics you have built and command; nothing else in this game is this color, ever.
- **Spent Cyan** `#5A8A8F` — Your weapon is cooling, the forge is running low; time pressure made visible on the object that matters most.
- **Ore Amber** `#C07830` — This pays you; every object in this hue range is something of value to collect, always, in every zone.
- **Pirate Crimson** `#8A1A1A` — This is a threat; it wants to reduce your fuel and end your run, and it will.
- **Deepwater Crimson** `#6B2030` — The threat has deepened; same threat as before, harder, cooler, more dangerous.
- **Ore Stone** `#4A4640` — Raw material; breakable, mineable, an obstacle that becomes a resource if you route correctly.
- **Impact White** `#FFFFF0` — Something just happened; this is the game's loudest moment, a contact event, the forge is striking.
- **Impact Gold** `#FFE040` — The heat of that contact is fading; the forge-strike just landed, now cooling.
- **Schematic Parchment** `#C8B87A` — You are at the forge looking at the plan; this surface is the document of who you are building.
- **Blueprint Gold** `#B8962E` — These are the paths of your decisions; where the lines are drawn in gold, the forge has opened a route.

---

### 4.3 Zone Color Temperature Rules

#### Zone 1 — Baseline (Deep Forge Interior)

All palette entries at full assigned hex values. Color temperature: **warm-dark** — the forge at working temperature, amber-lit from below, the field deep and pressured.

| Object class | Zone 1 Color |
|---|---|
| Background | `#0D0A08` (no shift) |
| Boomerang body / full-fuel trail | `#00CFDB` |
| Boomerang trail (depleted) | `#5A8A8F` |
| Asteroid body | `#4A4640` + `#5C5450` facet accents |
| Asteroid (ore-bearing) ambient light | `#A06030` Light2D, 0.1–0.2 intensity |
| Enemy / boss / projectile sprites | `#8A1A1A` |
| Ore/currency pickups | `#C07830` |
| Global ambient Light2D | `#A06030`, 0.08–0.12 intensity |

#### Zone 2 — Deepwater Shift

**What shifts**:

| Object class | Zone 1 | Zone 2 | Rule |
|---|---|---|---|
| Enemies / bosses / projectiles | `#8A1A1A` | `#6B2030` | Same silhouette, cooler-red tint |
| Asteroid body | `#4A4640` | `#3D3C42` | Cooler violet-grey — subtly different mineral |
| Global ambient Light2D | `#A06030` | `#6A5030` | Same hue family, pulled toward ochre-brown, intensity unchanged |
| Background | `#0D0A08` | `#0A0A0D` | Near-identical, fractionally cooler. Apply only if enemy/asteroid shifts read insufficiently distinct. |

**What NEVER shifts (zone-invariant)**:

| Object | Rule | Reason |
|---|---|---|
| Weapon Cyan `#00CFDB` | Absolute. No zone/value/saturation shift. | The boomerang is the player's construction — it exists outside zone theming. |
| Ore Amber `#C07830` pickups | Sprites stay full amber. Light2D ambient may shift, but pickup sprites do not. | "This pays you" must be unconditional across zones. |
| Impact White / Impact Gold | No zone shift. | Impact flashes are physics events, not environmental color. |
| Schematic Parchment / Blueprint Gold | UI is not zoned. | Zoning UI would suggest the tree belongs to a zone — violates P5. |

#### Zone 3+ — Extension Rule (Post-MVP)

> Each zone shifts one additional color temperature step along the warm-to-cool axis, preserving hue family membership for all object classes. The boomerang and ore pickups remain zone-invariant. A new zone may not introduce a new hue without formal palette amendment.

Zone 3 enemies shift further toward cooler red-violet. Zone 3 asteroids shift cooler-grey still. Field ambient pushes further toward blue-ochre. **Specific hex values are the job of the active Zone 3 art sprint**, constrained by this rule.

---

### 4.4 UI Palette

#### World hues and their UI eligibility

| World hue | UI-allowed? | UI role if allowed |
|---|---|---|
| Weapon Cyan `#00CFDB` | **Yes — constrained** | Only for tree nodes directly upgrading boomerang power/behavior. Never HUD panels, buttons, or generic chrome. |
| Ore Amber `#C07830` | **Yes — constrained** | Currency counter in HUD. Ore-yield / currency tree nodes. Never generic structural UI. |
| Pirate Crimson `#8A1A1A` | **No — forbidden** | Would signal threat/danger in UI. A settings button or locked-node outline in crimson would produce a false alarm. Forbidden without exception. |
| Deepwater Crimson `#6B2030` | **No — same rule** | |
| Ore Stone `#4A4640` | **Yes — structural only** | HUD panel base tint, inactive connectors, neutral UI surfaces. Background structure, not primary. |
| Impact White / Impact Gold | **No — forbidden** | These hues signal physical contact. A glowing white UI element would read as a missed impact. |

#### UI-exclusive colors

| Name | Hex | Role |
|---|---|---|
| **HUD Panel Dark** | `#1A1612` | Rectilinear chamfered panel backing — HUD fuel bar, counter frames. Slightly warmer than Forge Black. |
| **HUD Neutral Text** | `#C4B89A` | In-run counter text (fuel, currency, XP values). Reads as part of the schematic system, not alien UI chrome. |
| **Node Locked** | `#3A3428` | Locked hex node fill. Outlined in Blueprint Gold at 45% opacity. |
| **Node Unlocked** | `#5C4A20` | Unlocked hex node fill base (before Light2D overlay). Warm dark amber-brown — "this purchase is built into your weapon." |
| **Node Hover** | `#7A6030` | Hover state fill — brighter warm amber-brown. Plus inner hex ring additive-blend at Weapon Cyan (for weapon-adjacent nodes) or Ore Amber (for ore/mining nodes). |

#### Cluster archetype grouping tints (applied at 8–12% opacity over parchment background)

| Archetype cluster | Cluster Mark Hex | Logic |
|---|---|---|
| **Boomerang Power** (damage, speed, arc) | `#002A30` | Very dark desaturated cyan — ties to Weapon Cyan without reproducing it |
| **Ore & Mining** (yield, range, magnet) | `#302010` | Very dark desaturated amber — ties to Ore Amber without reproducing it |
| **Fuel & Survivability** (duration, regen, shields) | `#181820` | Cool near-black, slightly violet — fuel is defensive |
| **Mod Archetype Unlocks** (Pierce, Chain, Explode-on-return) | `#2A1808` | Dark warm rust — gateway nodes, most forge-adjacent read |

> **Production note**: Tints are intentionally near-invisible at rest to prevent competing with node fills. If playtesting shows players cannot group nodes by archetype at a glance, the fix is **opacity lift to ~18–20%**, not a new hue.

#### Tree connector state

| State | Color | Opacity | Weight |
|---|---|---|---|
| Active (both nodes unlocked) | `#B8962E` Blueprint Gold | 100% | 1–2px |
| Inactive (one/both locked) | `#B8962E` | 60% | 1px |
| Hover trace (path to hovered) | `#B8962E` | 85% | 2px |

> Connectors are a single color in all states — **differentiation is opacity + weight, never hue**. "The path is the path," accessibility depends on contrast with background, not hue variation.

#### Menu and settings neutrals

| Element | Color | Note |
|---|---|---|
| Main menu background | `#0D0A08` | Identical to game field — menu is not a separate environment |
| Menu title text | `#B8962E` Blueprint Gold | Schematic language for the game's own name |
| Menu option text (idle) | `#C4B89A` | Warm, quiet, subservient to the boomerang |
| Menu option text (hover) | `#E8D8A8` | Lifted warm white — same hue family as parchment, higher value |
| Settings / dialog panel | `#1A1612` HUD Panel Dark | Same chamfered-panel language as in-run HUD |
| Destructive confirmation | `#C4B89A` bold weight + explicit text label | **No red for destructive actions.** Crimson is enemy-exclusive. Use weight + "are you sure?" text, not color. |
| Pause overlay | `#0D0A08` at 70% alpha | Sprite overlay, multiply blend — same as boss vignette |

---

### 4.5 Colorblind Safety

#### Priority pairs (gameplay failure if collapsed)

| Priority | Pair | Consequence of collapse |
|---|---|---|
| **P-A** | Weapon Cyan vs Background (Forge Black) | Player cannot track boomerang arc — **P3 fails entirely** |
| **P-B** | Ore Amber vs Pirate Crimson | Player cannot distinguish "collect this" from "avoid this" — **P2 fails** |
| **P-C** | Ore Amber vs Ore Stone (asteroid) | Player misreads currency pickups as inert asteroid matter |
| **P-D** | Node Unlocked vs Node Locked | Player cannot read their build at a glance — **P5 navigation fails** |

#### Protanopia (red-cone absent)

| Pair | Collapse? | Backup cue |
|---|---|---|
| P-A Cyan vs Black | No | None needed |
| P-B Ore Amber vs Pirate Crimson | **Partial** — both shift toward similar dark brownish-orange | **Shape + motion**: pickups = small convex angular fragments, slow float drift. Enemies = radial-spike silhouettes, directed motion toward ship. 32×32 silhouette rule is the safety net. |
| P-C Ore Amber vs Ore Stone | No | None |
| P-D Unlocked vs Locked | No | Value contrast (node brightness) + Light2D luminance on unlocked |

**Verdict**: Amber-crimson collapse is the only concern. Shape differentiation (already load-bearing from Section 3) is the primary discriminator.

#### Deuteranopia (green-cone absent)

Identical concern to protanopia — same amber-crimson partial collapse. Same backup: shape + motion + HUD ore counter updating when a pickup is collected.

#### Tritanopia (blue-cone absent — blue/yellow confusion)

| Pair | Collapse? | Backup cue |
|---|---|---|
| **P-A Cyan vs Black** | **Most significant CVD issue** — Weapon Cyan shifts toward green with reduced contrast | **Luminance is the backup**: boomerang Point Light2D is luminance-based; additive-blend trail preserves brightness regardless of hue shift. The boomerang is always the brightest moving object in the field with directed linear motion and asymmetric elongated silhouette. Shape + motion + luminance together preserve arc readability. **Implementation note**: additive-blend trail rendering in URP 2D must be confirmed functional for WebGL specifically — this is part of the tritanopia safety net, not just a visual preference. |
| P-B Ore Amber vs Pirate Crimson | No collapse | — |
| P-C Ore Amber vs Ore Stone | No | — |
| **P-D Unlocked vs Locked nodes** | Concern — Blueprint Gold vs lighter parchment tones may partially collapse | **Value contrast is backup**: Node Unlocked `#5C4A20` vs Node Locked `#3A3428` differentiated by brightness. Light2D on unlocked nodes is luminance-based. Distinction survives on luminance alone. |

**Verdict**: Tritanopia's P-A cyan-on-black is the single most significant CVD concern. Mitigation is luminance + motion — both already required by P3.

#### CVD summary — base palette holds without a toggle

The intent is that the base palette is playable across all three major CVD types through intrinsic properties:

1. **Shape primacy** (Section 3 32×32 rule) — silhouette is the first discriminator; color is secondary.
2. **Luminance differentiation** — all critical pairs differ in brightness as well as hue.
3. **Motion differentiation** — pickups drift, enemies pursue, boomerang traces a directed arc. Motion requires no color vision.
4. **Additive-blend luminance** — boomerang trail + unlocked-node Point Light2D are additive, so distinctiveness is anchored in luminance.

A CVD toggle may be added later as courtesy (pattern-overlay on node states, iconographic pickup labels), but is **not required for baseline legibility**.

---

### 4.6 Damage Number + Impact Feedback Colors

#### MVP scope note

**Critical hits are not in MVP (Tier 1 or 2).** The boomerang deals deterministic damage — values set by tree nodes, not RNG. The crit entry below is stubbed for Tier 3.

#### Damage number colors

| Hit type | Color | Hex | Notes |
|---|---|---|---|
| **Standard hit** | HUD Neutral Text | `#C4B89A` | Warm parchment-adjacent — part of the schematic language, not a separate HUD system |
| **Boss hit** | Ore Amber | `#C07830` | Communicates "significant strike on significant target" without introducing a new hue |
| **Critical (stub — Tier 3)** | Impact Gold | `#FFE040` | Reserved meaning. Impact Gold is forbidden in any other damage context. |

No separate enemy-type colors. Enemy type is already communicated by sprite silhouette + hue; duplicating in damage numbers is noise without information.

#### Damage number timing vs impact flash

| Event | Timing (60 Hz) | Color |
|---|---|---|
| Impact flash peak | Frame 0 | `#FFFFF0` Impact White, full-screen-adjacent on contact zone |
| Impact flash falloff | Frames 1–3 | Lerps toward `#FFE040` Impact Gold |
| Impact flash complete | Frame 4 (~0.067s) | — |
| Damage number spawn | Frame 2 | `#C4B89A` (or `#C07830` for boss) |
| Damage number hold | Frames 2–18 (~0.3s) | Static, no animation |
| Damage number rise + fade | Frames 18–36 (~0.3–0.6s) | Rises 12–16px, alpha → 0 |

> **Rationale**: Damage numbers appear during impact falloff, not peak. **The blow registers before the number does.** Numbers are not in Impact White/Gold because a sustained number in those colors would be indistinguishable from a held impact flash.

---

### 4.7 Color Tests — Production Rules (Blocking)

Every new asset must pass all three tests before pipeline entry.

#### Test 1 — Saturation Budget

**Rule**: No non-contact-event asset may have HSL saturation ≥ the lowest-saturation impact VFX frame.

**Procedure**:
1. Sample HSL saturation of asset's most saturated pixel in idle/default.
2. Sample lowest-saturation impact VFX frame (typically Impact Gold `#FFE040` falloff — HSL S ~100%, L 62%).
3. If asset S ≥ impact S: **FAIL**. Reduce saturation or justify as contact-event asset.

**Exception**: Weapon Cyan `#00CFDB` (HSL S ~100%, L 43%) is contact-adjacent — the boomerang body is the contact instrument. All other non-contact assets must fall below its saturation ceiling.

#### Test 2 — Zone-Invariance

**Rule**: Zone-invariant colors (Weapon Cyan, Ore Amber pickups, Impact White, Impact Gold, all UI) must not shift H, S, or L between Zone 1 and Zone 2 sprites by more than ±3 points.

**Procedure**:
1. Eyedropper most saturated pixel in Zone 1 version.
2. Eyedropper same pixel in Zone 2 version.
3. Compare HSL. Any H/S/L difference >±3 for zone-invariant assets: **FAIL**.
4. For zone-variant assets (enemies, asteroid body, ambient), confirm H shifts within permitted range (Section 4.3) and does not collapse any priority pair (Section 4.5).

#### Test 3 — Colorblind Collapse

**Rule**: Any asset adjacent in gameplay role to another must be tested against protanopia, deuteranopia, tritanopia simulation.

**Procedure**:
1. Run asset + adjacent-role peer through CVD simulation (Coblis, Stark, or equivalent). 3 passes.
2. Confirm priority pairs (P-A through P-D) do not collapse to indistinguishable silhouettes.
3. If a pair collapses: identify backup cue (shape, motion, luminance, audio) per Section 4.5. Confirm backup cue is present in the asset's implementation.
4. If no backup cue present and pair collapses: **FAIL**. Do not add a CVD mode as the fix — correct the backup cue on the asset.

**Test 3 caveat**: Advisory for non-priority assets (cluster tints, hover states). **Blocking only for priority-pair assets** (the four pairs in Section 4.5).

---

### Pillar Anchor Summary — Section 4

| Pillar | Where Section 4 anchors it |
|---|---|
| **P1 — Multiplicative Dopamine** | Boss hit amber vs standard parchment (4.6) — "this hit counted more." Unlocked node warmth accumulating on tree (4.4) — tree visibly warms as build fills. |
| **P2 — Positional Mastery** | Hue reservation (4.1/4.2) — field objects read by color role at a glance. CVD backup cues (4.5) — this holds for colorblind players. |
| **P2 — Mastery in where the player moves** | Weapon Cyan zone-invariance (4.3). Trail desaturation rule (4.1) — fuel state visible on the weapon itself. |
| **P4 — Weighty Everything** | Saturation Budget Test (4.7) — impact events always own the saturation ceiling. Impact flash timing (4.6) — blow registers before number. |
| **P5 — The Tree IS the Game** | Full UI palette spec (4.4) — tree has its own palette language. Crimson forbidden in UI — tree never signals threat. Node accumulation warmth — tree grows visibly brighter. |

All five pillars anchored. P2 and P3 carry the most surface area.

---

## Section 5: Character Design Direction

*Defines the visual personality, animation direction, and design intent for every entity that carries identity beyond shape and color. Does not duplicate Section 3 (shape) or Section 4 (color). Answers: **what does each entity feel like as a designed thing?***

### 5.0 Scope Clarification

This game has no player avatar, no NPCs, no dialogue, no narrative (AP5). "Character" here = **designed visual personality**. Four entity classes carry designed personality:

- **The ship** — the player's physical presence. A tool, not a vehicle.
- **Six enemy variants** — improvised pirate machines. Grunt, Ranged, Tank × 2 zones.
- **Two bosses** — larger articulated hostile constructs.
- **The boomerang** — the protagonist the player identifies with most deeply.

> **Pillar anchor**: Section 5 as a whole serves **P4 — Weighty Everything**. Weight is not just impact VFX. It is the weight of things that feel *authored*: a ship that looks lived-in, a boomerang that behaves differently on different surfaces, enemies that die in ways that suit them.

---

### 5.1 Ship Design Direction

The ship is a throwing arm with a fuel tank. The player operates it like a craftsperson holds a grip.

#### 5.1.1 Base Ship — 5 Detail Marks (not 6)

| Detail Mark | Description | Purpose |
|---|---|---|
| **Scored seam lines** | 2–3 parallel scored lines crossing the hull at non-matching angles | "Assembled from mismatched salvaged panels" |
| **Mismatched plate break** | One hull section ~10–15% lighter value than adjacent section (same Ore Stone hue family) | Repair history without introducing new color |
| **Thruster scoring** | Rear nozzle ends with visible heat-score marks radiating inward, darker value | Accumulated use. "Forge has been running many runs." |
| **Asymmetric rivet cluster** | 2–3 chamfered-corner marks on larger hull panel, one side only (not mirrored) | Hero-register construction mark (chamfer exception from Section 3.7) |
| **Forward edge angle accent** | Leading edge's chamfered face slightly lighter value (wear from forward motion) | Directional read beyond silhouette — "nose" visible peripherally |

> **Quantity discipline**: Five marks. Not six. Overdetailing produces noise that competes with the boomerang — **direct P3 violation**.

**Pillar anchor**: **P4**. Scored panels and mismatched plates = visible history. History implies the cost of the journey so far.

#### 5.1.2 Evolution Storytelling — "A Worker Upgrading Their Tools"

Visual story: **a craftsperson upgrading equipment**, NOT a soldier acquiring combat power.

- Attachments look **fitted, not designed**. Bolted, clamped, lashed into place. A plate on a plate, not a new chassis.
- Each attachment threshold (Section 3.5.2) reads as **added function**, not **increased threat**. A thruster fin = faster, not more dangerous. An armored panel = harder to kill, not more aggressive.
- At full tree-fill: **overequipped and functional**, the bench-worn tool of someone who has been at this a long time. NOT a final-form aesthetic.

Attachments have **no idle animation of their own** — static hull modifications. A craftsperson's tool at rest does not perform.

**Pillar anchor**: **P1**. Threshold-felt visual change reinforces threshold-felt capability change.

#### 5.1.3 Idle Animation Direction

**Main menu idle**: 3-second hover loop, ±2–3 pixels vertical on **step curve, not sine** (step = mechanical/stabilizer-like, sine = organic/drifting). Thruster nozzles fire tiny 2-frame angular particle: 2×4 pixels, Ore Amber `#C07830` at 30% opacity, 6–8 frame lifetime, zero spread. Barely visible — engine idling, not running.

**In-run idle**: Thruster output scales with velocity. At rest = minimal 2-frame particle. At movement speed = double emission rate, particle length 6–8 pixels. Functional readout, not visual upgrade.

**No body breathing animation.** No oscillating scale, no ambient glow pulse. **Tools do not breathe.**

Implementation: Transform hover tween + 2–4 frame thruster sprite-sheet. No bones. No secondary controllers.

**Pillar anchor**: **P2**. A ship that performs its own idle introduces competing visual motion.

#### 5.1.4 Damage States — Non-Crimson Feedback

When the player takes a hit, ship must communicate damage clearly — but per Section 4, **no red/crimson in UI**, no Impact White/Gold except on contact.

**Approach: value drop + thruster flicker + local shake.**

| Response | Implementation | Duration |
|---|---|---|
| **Hull flash** | Sprite material tint to `#1A1612` HUD Panel Dark (darkening, not brightening). 3-frame tint, 8-frame lerp back. | ~0.18s |
| **Thruster flicker** | Emission stops 4 frames, resumes at double intensity 4 frames. Engine stuttering under impact load. | ~0.13s |
| **Shake displacement** | Transform ±3–4 pixels toward hit source for 1 frame, 3-frame return. **Ship-local, not screen shake.** | ~0.07s |

**What this communicates**: Ship got hit, power disrupted, recovered. Information via momentary power loss, not threat-coded warning.

**Distinction from boomerang catch**: Catch = positive event (satisfying sound + elevated-glow state). Damage = negative event (darkening + stutter). **Opposite value directions.** Player will not confuse them.

**Persistent low-HP state** (if HP is multi-point future mechanic): Thruster particle desaturates permanently toward neutral grey `#6A6460`. (Spent Cyan is reserved for boomerang fuel trail.)

**Pillar anchor**: **P4**. Hit that doesn't register costs the player information and the game its feel.

---

### 5.2 Enemy Character Personality

#### 5.2.1 Grunt — "Pack Animal"

Individually dangerous mostly because of where it is relative to other Grunts. Personality = **proximity + chaos**: gets closer, makes space smaller, rotates constantly.

**Motion signature**: Erratic drift + continuous slow rotation. Irregular left/right oscillation over general approach vector. Rotates 15–25°/s (direction per-Grunt at spawn, not reversed mid-life). The rotation is a *personality* signal (chaotic because improvised), not a gameplay signal. Rotating spike array makes exact collision boundary slightly unpredictable.

**Telegraph**: No projectile. Damage is contact. Telegraph = **approach acceleration** — in 0.4s before entering contact-damage window, drift speed +40%, rotation rate increases noticeably. No flash, no particle — purely kinematic read. Players develop muscle memory: *it is coming faster.*

**Destruction**:
- Non-kill hit: 2–3 spike silhouettes detach as angular shards, drift outward, fade 0.5s. Pirate Crimson, no new color.
- Kill: Body splits into 4–6 irregular angular shards at consistent 60–90° authored spread. Inherit boomerang velocity biased toward impact direction. ~0.4s lifetime, no bounce. **Read: something tight and spiky came apart all at once.**

**Pillar anchor**: **P2**. Chaotic motion = spatial threat demanding field awareness. Reading Grunt clusters and routing the boomerang to intercept = Positional Mastery expression.

#### 5.2.2 Ranged — "Sniper"

Personality = **deliberate targeting**. Hangs at distance, faces the player, fires on cadence.

**Motion signature**: Slow deliberate approach + axis-lock rotation. Keeps barrel protrusion(s) oriented toward the player ship at all times via slow ~10°/s rotation. Maintains standoff — has approach vector but slows dramatically at mid-range, preferring to orbit. **The most readable enemy motion in the game**: a shape that points at you and keeps pointing at you as it arcs around.

**Telegraph**: **Charge accumulation mark** on barrel tip — small angular sprite (3–4 pixels in-game), brightens over 0.6–0.8s before fire. **Blueprint Gold `#B8962E` at 40% opacity climbing to 80%**. Holds and builds (no pulse). Extinguishes on fire.

> **Exception note**: Blueprint Gold is UI-exclusive per Section 4.4. The Ranged charge mark is a documented exception — the alternative (brightness-only white) risks ambiguity with impact flash in busy scenes; Blueprint Gold is the most distinct hue available from Pirate Crimson that reads against Forge Black. This exception is explicit and limited to this mark.

**Destruction**: On kill, splits along primary (tall) axis — 2 large body halves + 1–2 smaller shards. Barrel protrusion(s) detach separately as elongated spinning fragments. ~0.5–0.6s lifetime. **Read: a pointed thing snapped at its length.**

**Pillar anchor**: **P2**. Standoff behavior + directional telegraph telegraphs intent.

#### 5.2.3 Tank — "Bulwark"

Personality = **denying space, not pursuing**. Does not move to kill — moves to fill the field.

**Motion signature**: Ponderous weighted approach. Slowest enemy velocity (40–50% Grunt speed). Maximum heading change ~5°/s — once pointed at you, it will reach you. Cannot simply orbit; player must plan ahead. **Does NOT rotate its body.** Keeps flat face forward and pushes. **Moves like a wall moving, not like a creature moving.**

**Telegraph**: **Body compression animation** — sprite squashes Y-axis ~15–20% over 0.8s (Transform scale tween), then fires or rams. Anticipation pose for a machine. 3-keyframe sprite-sheet: rest → compressed → expanded → rest.

> **Design note on squash-without-curves**: The squash temporarily makes the Tank's proportions rounder at max compression. Acceptable because: (a) 1–2 frame pose, not resting state; (b) silhouette still reads angular (scale change only, no radii); (c) cheapest possible anticipation signal that doesn't rely on color.

**Destruction**: Most dramatic of the three archetypes (earned via most hits to kill). Central body fragments into 3 large heavy-looking angular blocks + 2–3 smaller debris. Primary blocks drift outward at HALF the Grunt shard speed — they are *heavy even in death*. Fade over 0.8–1.0s. **Read: a mass of material came apart and the pieces are still heavy.**

**Pillar anchor**: **P4**. Tank must feel expensive to kill. Slow motion + ponderous heading change + heavy debris compound "this object has mass the boomerang is overcoming."

---

### 5.3 Boss Personality

#### 5.3.1 Zone 1 Boss — "The Claim Jumper"

Internal art-direction name only. Not shown in-game.

**Character read**: "A dig machine that decided it could fight." A mining apparatus weaponized — improvised, not purpose-built. Visually continuous with Grunt/Ranged/Tank register — the biggest pirate machine, not a fundamentally different kind of threat.

**Sub-components (5)**:

| Component | Description | Gameplay function |
|---|---|---|
| **Central body** | Wide low asymmetric mass, left heavier than right. ~55% of total boss area. Ore Stone base + Pirate Crimson plating. | Primary hit target. Stationary. Carries HP. |
| **Port mining claw (oversized)** | 3-segment hinged arm extending left. ≥50% beyond body mass. At rest angled 25–30° down-outward. | Sweeps 90° arc during melee phase. Telegraphed by raising 45° over 0.8s before sweep. |
| **Starboard mining claw (smaller, 65%)** | Same shape smaller, positioned higher on body. Creates required asymmetry. | Secondary attack — fires when port on cooldown. 60° sweep arc. |
| **Rotating turret dome** | Chamfered hexagonal profile atop body at rightmost point. Rotates 20–30°/s toward player ship. | Fires Ranged-type projectiles. Always tracking. Persistent threat between claw windows. |
| **Engine/thruster block** | Rear rectangular with angular exhaust vents. Thruster scoring marks identical to ship (same forge origin). | No attack. Visual "this machine is under power." |

**Phase transition** (50% HP): Port claw seizes at fixed angle (static sprite swap — bent/jammed state, not new animation). Boss compensates: turret rotation +30% speed, movement hesitation reduced. **Read: adaptation under damage — the machine routes around its broken part.**

Transition signal: 3-frame Impact White spark burst at claw joint. No crimson flash, no color change. **Structural information only — a part broke.**

**Death sequence** (1.8–2.2s): hitstop 1 frame → turret dome detaches first (spinning upward-rightward, 2s lifetime) → mining claws drift apart low arcs (1.5s) → central body fractures 4–6 angular slabs (slow outward drift) → engine block fires 4-frame last-gasp thruster then dark. Last crimson fade triggers Zone Victory ambient shift.

**Pillars**: P4 (death sequence), P2 (claw + turret force dual-axis awareness).

#### 5.3.2 Zone 2 Boss — "The Warden"

Internal art-direction name only. Never shown in-game.

**Character read**: "A guardian built to hold ground, not to pursue." Qualitative escalation from Zone 1 — *purpose-built* rather than *repurposed*. All enemy-facing surfaces in Deepwater Crimson `#6B2030`.

**Sub-components (4)**:

| Component | Description | Gameplay function |
|---|---|---|
| **Core hull** | **Tall rather than wide** (Zone 2 primary differentiation from Zone 1). Upper half heavier than lower (inverted CoM vs Claim Jumper). ~50% area. Deepwater Crimson. | Primary hit target. Higher HP than Zone 1 boss. |
| **Shield petal array (port, 2 panels)** | Angular petal-shaped armored panels hinged at hull left. At rest folded forward partially screening hull. Asymmetric internal (two non-identical panels). | Active: swing outward 90° over 0.6s, hull briefly exposed during swing. Shield phase: folded closed, boomerang deflects (not pierces). |
| **Anchor spike array (base, 3–4 spikes)** | Large downward-pointing angular spikes from hull underside. Do NOT attack. | Territorial mass read. Boss looks dug-in. Close range = collision hazard for player ship only (not boomerang). |
| **Artillery arm (starboard, oversized)** | Single articulated arm. ≥50% beyond hull mass on right. Heavier/more angular than Zone 1 claws — purpose-built. At rest 45° upward. | Primary ranged. Rotates toward player over 1.2s wind-up before firing 3-projectile angular burst. |

**Phase transition** (40% HP, earlier than Zone 1 — rising pressure): Shield petals deactivate permanently, freeze at 45° half-open. Artillery fire rate +30%. Second heavier projectile type introduced. Transition signal: **same 3-frame Impact White spark burst** at shield joint, then 8-frame artillery arm value-lift. Artillery cadence then increases.

> **Design note**: Both bosses use the same mechanical signal (joint-seizing spark). The game teaches its own language — player who learned Zone 1's phase signal reads Zone 2's without instruction.

**Death sequence** (2.5–3.0s — longer than Zone 1, more mass): artillery arm detaches first (sharp spin outward, 3-second arc, 2s lifetime) → shield petals flutter open and fragment individually (1.5s each, 0.2s sequential delay) → anchor spikes **sink 8px downward over 0.8s then detach and drop** (weight read) → core hull shatters 5–7 angular slabs, Deepwater Crimson fading to near-black over 1.5s.

**Pillars**: P2 (shield petal opening windows — deliberate "aim here" moments), P4 (anchor spike drop gives physical conviction).

---

### 5.4 The Boomerang as Character

#### 5.4.1 Presence Beats — 4 Authored Moments

**Beat 1 — Throw** (2–4 frames):
- F0 (hold): at ship hardpoint, scale 1.0, no motion
- F1 (anticipation): scale 0.85 long-axis squash — throwing arm pulling back
- F2 (release): scale 1.15 long-axis stretch — release "snap." Travel velocity begins.
- F3–6 (normalize): scale lerps back to 1.0 over 4 frames as boomerang reaches travel speed

**All Transform scale tweens**. No bones, no sprite-sheet swap. Minimal cost, delivers *thrown* feel.

**Beat 2 — Flight**:
- Boomerang body rotates 180–220°/s. **Rotation speed varies slightly with tree progression** — well-upgraded boomerang rotates visibly faster. **P1 made visible in motion.**
- Trail (Section 2.1: cyan additive at full fuel, Spent Cyan desaturated at low fuel) is the primary P3 readability signal. No other flight animation needed.

**Beat 3 — Return**:
- **Return arc trail is visually identical to throw arc trail.** Direction of motion is the only cue. **Symmetry of visual information is a P3 protection rule** — a different return visual would subconsciously signal "rules changed."
- Exception at 2+ mods loaded: return trail widens +2 pixels, color temp ~10° warmer. Not consciously readable at play speed. **Subliminal cue that a loaded boomerang is returning.**

**Beat 4 — Catch**:
- F0 (approach): boomerang nearing ship hardpoint
- F1 (seat): arrives at hardpoint, scale briefly 1.2 overscale — the catch has physical impact, weapon "seats" hard
- F2–5 (settle): scale lerps to 1.0. Boomerang Point Light2D **pulses to 0.7 intensity for 3 frames** — the catch flash (smaller than Impact White, in boomerang's own cyan). Returns to normal 0.4–0.6.

> **Audio sync point**: the Light2D pulse must coincide exactly with the catch sound cue.

**Pillar anchor**: **P4**. Squash-stretch throw + overscale catch seat are the minimum animation craft to make the throw cycle feel like physical activity, not mechanical teleport.

#### 5.4.2 Mod-Fill Visual Story — "The Weapon is Becoming a Legend"

Metaphor: **a tool gaining visible history of use and modification**, NOT a weapon unlocking its true form.

- **Unmodded**: Functional, slightly worn throwing wedge. Scored lines, blunt strike face. Nothing extra. First run should look like this.
- **1 mod**: One addition. Fits awkwardly but purposefully. Looks fitted, not installed.
- **2 mods**: Combination reads as intentional. Begins to look like a *build*.
- **3 mods max**: Recognizable but dense. Half-second to read all additions. **This is the "legend" read — not gleaming mythic, but thoroughly-worked-over. Looks earned.**

**Animation changes at mod stack**: 2+ mods → rotation speed increases slightly (visual only). 3 mods → throw squash/stretch more pronounced (0.80 → 1.20 vs base 0.85 → 1.15). Subliminal reinforcement of "more mass in motion."

**Zone Victory hold pause** (Section 2.6 reference): The 2–3s pause exists to let the player read the boomerang's accumulated form. No UI, no summary card. **Just the weapon, lit by zone gold, showing its authored form.** *Look at what I built.*

**Pillars**: **P1** + **P4**. Every tree node that unlocks a mod archetype or stacks a mod changes the boomerang's visible form. **The tree is legible in the weapon the player is throwing.**

#### 5.4.3 Interaction Character

Boomerang travels on a single scripted arc (kinematic, not physics). **Visual reactions on contact differ by target type** — behavioral personality without trajectory change.

| Target type | Visual contact reaction | Communicates |
|---|---|---|
| **Ore-bearing asteroid** | Impact White + Impact Gold flash. **3–4 Ore Amber particle fragments** drifting 20–30° from impact angle, 0.3s lifetime. | Asteroid giving up value. Amber fragments preview the drop. |
| **Non-ore asteroid** | Same flash. **No amber spray.** Debris in Ore Stone only. | *This one doesn't pay.* Absence of amber is information. |
| **Grunt (non-kill)** | Impact White flash + minimal crimson shard (2-pixel, single frame, immediate fade) | Landed. Grunt still up. |
| **Ranged/Tank (non-kill)** | Slightly larger Impact White flash + same minimal shard | Surface-area-scaled — Tank especially has more to hit. |
| **Enemy (kill)** | Full destruction VFX per archetype (5.2.1–5.2.3). Boomerang passes through (pierce) or stops (chain). | The boomerang did its job. Enemy reaction tells the story. |
| **Boss (any hit)** | Impact White → Impact Gold, **1.5× flash radius**. Amber damage number (Section 4.6). No special boomerang reaction. | *Even the boss is just a surface the boomerang is working through.* Weapon's job unchanged by target scale. |

**Pillars**: **P2** (same weapon, same physics, different surface response = consistent). **P2** (amber fragment preview teaches route decisions before the ore drops).

---

### 5.5 LOD Philosophy — 3 Detail Registers

#### 5.5.1 In-Run Gameplay — Functional Register

- **Authoring**: Sprites at **2× intended display size** (64×64 auth for 32×32 display). Room for scored lines, mismatched plates, angular detail marks. Downsampling produces clean slightly-soft edge reading as hand-crafted.
- **Detail floor**: Every sprite carries all required section marks (ship: 5 marks, enemy: motion-relevant features, boss: sub-component reads). Detail not surviving downsampling: not authored.
- **Detail ceiling**: **No texture-level surface detail.** No hatching, no fill patterns, no cross-face gradients. **Flat per-face color values only**, consistent with Beauty Must Be Cheap. A face = one value, or one angular accent mark in neighboring value. That is the ceiling.

**Pillar anchor**: **P2**. Flat per-face color + authored edges give the boomerang trail maximum contrast against every object it passes.

#### 5.5.2 Skill Tree Icon Register

- **Authoring**: Icons at 48×48, displayed at 24×24. At 24×24, detail below 3 pixels is lost. Icons must carry identity in 3+ pixel marks only.
- **Icon language**:
  - Pierce: blade angle against small wedge (3 marks)
  - Chain: angular hook/loop shapes (NOT circle)
  - Explode-on-return: charge block with one amber mark (the only amber in icon)
  - Ore mining: basic asteroid 6-side silhouette
  - Boomerang power: base wedge silhouette, no mods
  - Fuel/survivability: 3 parallel diagonal lines (schematic fill level)
- **NO circles, curves, or rounded shapes at this scale.** If a mechanic can't be conveyed in angular 3+ pixel marks, the design must be revised.

**Pillar anchor**: **P4**. Tree as beautiful document requires every node icon legible at display scale.

#### 5.5.3 Showcase Register — Main Menu + Zone Victory

- **Authoring**: Same in-run sprite, **displayed at 2–3× scale**. Point sampling (no bilinear). No separate high-res asset.
- **Detail addition allowed**: ONE very fine scored line on boomerang top face (~1px width), wear-from-thousands-of-throws. Resolves to 0.5px at in-run scale and disappears. Only showcase-exclusive detail. Does not alter silhouette.

> **Anti-nanotexture rule (explicit)**: No sprite may be authored with texture-level detail (stippling, hatching, sub-pixel gradient, fill pattern) on the assumption players won't see it at in-run scale. If detail is only visible at 3×+, **it does not belong in the base sprite.** The place for zoomed-in beauty is geometry-based mod-fill accumulation — inherently scale-independent.

**Pillar anchor**: **P2** (boomerang's menu rotation is the game's first introduction of P3 — eye learns the weapon before the first throw) + **Beauty Must Be Cheap** (showcase earned by craft within constraints, not a separate asset pass).

---

### 5.6 Pillar Anchor Summary — Section 5

| Pillar | Where Section 5 anchors it |
|---|---|
| **P1** | Ship attachment thresholds (5.1.2). Boomerang mod-fill visual story (5.4.2). Rotation speed +mod stack (5.4.1). |
| **P2** | Enemy telegraphs: Grunt acceleration, Ranged Blueprint Gold charge, Tank squash (5.2). Boss dual-axis threats (5.3). Boomerang ore-fragment preview (5.4.3). |
| **P2** | Ship idle minimum-motion (5.1.3). Throw/return arc visual symmetry (5.4.1). Anti-nanotexture rule (5.5.3). In-run flat color ceiling (5.5.1). |
| **P4** | Ship damage feedback (5.1.4). Enemy destruction per archetype (5.2). Boss death sequences (5.3). Throw squash-stretch + catch overscale (5.4.1). Tank heavy debris drift (5.2.3). |
| **P4** | Zone Victory boomerang "read what you built" pause (5.4.2). Tree icon legibility (5.5.2). Ship attachment at cluster thresholds (5.1.2). |

All five pillars anchored. P4 carries most rules (entity personality + destruction). P2 second-highest (telegraph aesthetics).

---

### 5.7 What This Section Does NOT Cover

- **NPC character design** — none (AP5). No deferred section.
- **Player avatar portraits** — none. Ship is the presence (5.1).
- **Dialogue portraits / emote systems** — architecturally absent.
- **Faction visual vocabularies beyond zone re-tints** — Zone 2 is Zone 1 + hue shift (Section 4.3). No separate factions.
- **Character-driven narrative cutscenes** — none (AP5). Zone Victory + Death are mood states, not narrative beats.
- **Boss backstory / lore visualization** — internal names ("Claim Jumper", "Warden") are AD reference only, never shown in-game. Player never told what bosses are or why they exist.

**The absence of these is a pillar-level decision, not a gap in this document.**

---

## Section 6: Environment Design Language

*The field is not a backdrop — it is the game's primary stage, the forge floor. The environment IS the asteroid field.*

### 6.1 Background / Space Treatment

**Rule: The in-run background is pure Forge Black. No starfield. No nebula. No parallax depth layers.** *Permanent rejection* — not revisited unless Section 1 is revised.

Rationale (3-way):
- **P3 protection** — starfield = isotropic dot noise competing with directional cyan trail
- **P4 via absence** — forge interior is contained workspace; starfield would render infinite depth
- **WebGL budget** — parallax requires camera layer systems yielding nothing the game needs

#### 6.1.1 Permitted exception: Background Scoring Marks

- 8–12 angular hair-line marks per zone background (2–4px length, 1px width, no fill, no closed shape)
- **Color**: Ore Stone `#4A4640` at 15–20% opacity
- **Placement**: non-repeating, authored once per zone as single sprite, concentrated slightly toward field edges
- **Purpose**: breaks pure black without introducing spatial depth — reads as surface marks on a nearby wall, not distant objects

### 6.2 Asteroid Field Composition

#### 6.2.1 Arena Architecture — Bounded Per Zone

**Bounded arena. Fixed camera. No scroll, no follow-cam.**

Rationale: fuel-limited bullet hell = fundamentally arena game. Fixed field enables spatial mental-map building for P2 route planning. Scroll-cam raises "what lives beyond frame?" questions this game doesn't need.

#### 6.2.2 Density Rules

| State | Active Asteroids | When | Read |
|---|---|---|---|
| **Sparse** | 6–10 | Early zone / post-clearance / depleted | "Forge has worked here already" |
| **Moderate** | 12–18 | Zone baseline / standard waves | Working conditions — pressured but navigable |
| **Dense** | 20–28 | Late-zone escalation / boss pre-phase | Overcrowded — ore-rich but dangerous routing |

**Boss phase reduces ambient asteroid count by 4–6.** Less to dodge; focus on single large threat. **Field composition communicates what demands attention — P2.**

#### 6.2.3 Placement — Authored Constraint Templates

Not fully hand-placed per-wave, not fully random: per-zone **placement constraint templates** define seeding regions, clear lanes (boomerang flight corridors), cluster zones (vein storytelling). Procedural placement within regional constraints. Templates are per-zone assets; technical-artist handoff for specific implementation.

#### 6.2.4 Distribution Patterns — Per Zone

| Zone | Primary | Secondary | Read |
|---|---|---|---|
| **Zone 1** | Scattered-open | 3–5 Dense cluster at field center | Fresh territory; open approach vectors |
| **Zone 2** | Tight-clustered (2–3 denser groupings, wider clear lanes between) | Sparse outer ring of Exotic asteroids at field edges | Worked-over; ore consolidated, outer field exhausted |

Zone 1 supports early-game learning (full field at a glance). Zone 2 increases planning complexity at same field size without increasing enemy count.

#### 6.2.5 Drift Behavior

Slow authored velocity vectors within ±25° of authored orientation (Section 3.3 grounding-lie rule). Speed low enough that composition does not materially change in a 1–3 min run. **Field is not actively hostile through motion** — enemies are kinetic threat; asteroids are slow-rotation and gentle drift.

### 6.3 Zone-Level Environmental Beats

#### 6.3.1 Asteroid Composition Mix by Zone

| Type | Zone 1 | Zone 2 |
|---|---|---|
| Basic | 60% | 35% |
| Dense | 30% | 40% |
| Exotic | 10% | 25% |

**Zone 1**: fresh territory. Mostly accessible mineral matter. Exotic rare (anomalies).

**Zone 2**: older, harder. Standard seams mined out (fewer Basics); what remains is consolidated (more Dense) and unusual crystal from deeper strata (significantly more Exotic).

No separate Zone 2 asteroid sprite required — Section 4.3 hue shift + composition mix constitutes the environmental differentiation. Silhouettes identical.

#### 6.3.2 Mineral Texture Differentiation

The Zone 2 Ore Stone shift toward cooler violet-grey (`#4A4640` → `#3D3C42`) IS the texture differentiation. No additional sprite detail. Zone 2 asteroid material *feels* different because cooler tone reads as older, more pressure-compressed, petrified older seam material. Achieved entirely via sprite-shader tint property — zero new art assets.

#### 6.3.3 Ambient Sprite Elements

Zone 1 baseline: none. Zone 2: **Exotic asteroid density increase IS the ambient differentiation** — their elongated non-convex crystalline silhouettes populate peripheral vision more.

**Explicitly rejected**: glowing zone-ambient particles, floating debris clouds, persistent environmental animation. Ambient animation competes with P3.

### 6.4 Environmental Storytelling

No narrative (AP5). Spatial implication is fully in scope.

#### 6.4.1 Mineral Seam Storytelling

**Vein cluster signature**: 3–5 asteroids within roughly one ship-length, ≥2 carrying ore-bearing ambient Light2D (`#A06030` at 0.1–0.2 intensity per Section 2.1). Warm-ambient glowing rocks against dark spacing = *go here, this pays*. No additional marking needed.

**Vein-edge visual detritus**: 2–3 very small angular debris sprites (4–6 pixels in-game, static, non-interactive) at cluster edges — Ore Stone `#4A4640` at 60% opacity, no Light2D. Implies seam has history of prior workings.

#### 6.4.2 Field Emptiness as Narrative Implication

**Sparse field reads as prior mining, not failed content.** Section 3.3's "the forge has already worked here" rule applies directly.

Compositional implementation:
- **Compose toward voids with purpose** — void between vein cluster and open lane, not isolated
- **Background scoring marks denser near field edges** (2–3 extra marks concentrated toward boundary)
- **Exotic asteroid in sparse outer regions** — single non-convex silhouette against Forge Black + nearby scoring marks = "exhausted outer seam, worth approaching to check"

#### 6.4.3 Zone 2 Environmental Read

**Deeper into the same territory, not a different territory.** Forge analogy: Zone 1 = outer work area. Zone 2 = back room where older stock sits, walls darker, material compressed.

Implications conveyed through existing systems (no new assets):
- Cooler ambient Light2D → less heat, further from active forge
- Cooler asteroid color → older material
- Tighter cluster distribution → partially worked-over field
- Higher Exotic presence → deeper strata exposed

**Zone 2 visual detritus escalation**: small static debris sprites (6.4.1) appear more frequently and slightly larger. Passive and non-interactive. Zone 2 has been in use longer.

#### 6.4.4 Prior Presence — Approved vs Rejected

**Approved** (low cost, exactly needed):
- Static angular debris at seam cluster edges
- Background scoring marks concentrated near field edges and cleared regions

**Rejected** (too narrative, too expensive, AP5-adjacent):
- Pirate wreckage hulks at field edges (new object class, specific lore implication)
- Crystallized fragment clouds (particle/shader system cost)
- Stripped asteroid shell props (silhouette/palette ambiguity)

Rejection correct because approved implementation communicates exactly what's needed: *this space has been worked*. Wreckage/fragments communicate too specifically (battle happened here) — pushes toward narrative this game deliberately avoids.

### 6.5 Field Boundaries and Edges

#### 6.5.1 Arena Boundary Treatment

**Vignette fade to Forge Black + thin angular boundary marks.**

- **Vignette overlay**: multiply-blend sprite at `#0D0A08` covering outer 8–10% of arena boundary. Same tech as Section 2.2 boss vignette. Field "falls into forge darkness" rather than hitting invisible wall.
- **Angular boundary L-marks**: 3–4 points per edge, single chamfered L-shaped sprites (8–12 pixels), Ore Stone `#4A4640`, irregular spacing. Read as forge's own construction marks — boundary has been *built*, not just implied.

**Player boundary feedback** (on the object, not the camera):
- Ship approaches vignette zone → thruster emission drops to minimum
- Hull performs 2-frame value-lift brightening (sprite material tint, no new sprite)
- **No camera shake.** Camera-shake would occlude arc = P3 violation.

Technical-artist handoff: trigger zone distance should be tunable. Player should feel resistance *before* entering vignette, not after.

#### 6.5.2 Rejected Alternatives

- **Parallax far-field / infinite scrolling** — implies traversal the game doesn't support
- **Hard opaque border sprite** — reads as fence, breaks forge-interior immersion
- **Explicit text/UI boundary warning** — all feedback is visual and anchored to ship physical behavior

### 6.6 Skill Tree Environment

#### 6.6.1 Forge Interior at Panel Edges

**Parchment does NOT float in void.** At left/right edges of skill tree screen:
- 2–3 large angular silhouettes (structural beams, stacked material forms)
- Color: HUD Panel Dark `#1A1612` at 20–30% opacity against Forge Black
- Register peripherally as "mass in the room," not as foreground objects requiring attention

Grounds parchment reading as *document in physical space*, not UI panel floating in nothing. If a playtester reports noticing silhouettes during normal navigation: reduce opacity until forge is *felt but not seen*.

#### 6.6.2 Parchment Surface Detail

1. **Forge-grime and ash marks** — sparse static authored per parchment section. Angular marks only (no curves on decorative marks). Blueprint Gold `#B8962E` at 8–12% opacity, scattered asymmetrically at panel edges and corners.
2. **Corner anchor marks** — 4 chamfered L-shaped marks at parchment corners (same language as arena boundary marks). Blueprint Gold at 50% opacity. Reads as schematic's own registration/alignment marks — **the document was pinned to something.**
3. **No hand-drawn fill patterns, no background ruled grid, no hatching.** Parchment surface between nodes is Schematic Parchment `#C8B87A` flat. **The node network IS the document's structure.**

#### 6.6.3 Weathered, Not Pristine

**Parchment carries history.** Schematic has been worked with before. Forge-grime + corner anchor wear achieves this.

**Upper limit**: no weathering element may obscure a connector line or node outline. Grime placement must be checked against node layout.

### 6.7 Texture Philosophy — Environmental Context

#### 6.7.1 Asteroid Surface

- Every face of every asteroid sprite is a **single flat color value**
- Facet accent color: `#5C5450` on Zone 1 (proportionally cooler Zone 2)
- Accent appears as **single angular mark** on the most prominent face (facing implied upper-left light)
- **ONE accent mark per asteroid, maximum.** Not per face.
- **Dense asteroid exception**: carries secondary fracture line (Section 3.5.3) in darker Ore Stone `#3A3630` — structural shape info, not accent. Total permitted: fracture + one face accent.

#### 6.7.2 Sprite-Shader Tint Variation — Allowed, Hard-Limited

- Per-asteroid tint shift: **±5 HSL VALUE points only**. No hue shift. No saturation shift.
- Set per-sprite material property at spawn, not runtime animation
- Random within ±5 range, seeded per instance
- Zone 2 base already cooler; ±5 variation applies to Zone 2 hex equivalently

**Hard limit justification**: ±5 value barely perceptible at play distance but breaks stamped-clone read. ±10 would read as different materials (palette violation). Hue shift toward amber false-signals ore-bearing (P2 violation); hue shift toward cyan false-signals boomerang proximity (P3 violation).

**Prohibition**: no per-asteroid shader tint variation during in-run play. Tint set once at spawn. Animated variation would compete with P3.

#### 6.7.3 No Normal Maps, No PBR, No Baked Lighting

Environmental sprites carry no runtime normal map data. The "normal-map light direction upper-left" from Section 2.1 refers to **authored face values simulating the light direction** — not a runtime normal map pass.

**PBR permanently prohibited on field objects.** Expensive, specular response would register as impact-saturation competition, aesthetically wrong for Deep Forge (sooty, industrial, non-reflective).

### 6.8 Pillar Anchor Summary

| Rule | Pillar |
|---|---|
| Pure Forge Black — no starfield | **P2** |
| Background scoring marks at 15–20% opacity | **P4** |
| Bounded arena + fixed camera | **P2** |
| Density scales with wave, drops for boss | **P2** |
| Placement constraint templates with clear lanes | **P2** |
| Zone 2 clustered distribution | **P2** |
| Vein cluster via Light2D grouping | **P2** |
| Vignette boundary + angular L-marks | **P4** |
| Ship boundary feedback on object, not camera | **P2** |
| Forge silhouettes at parchment edges | **P4** |
| Parchment weathering marks | **P4** |
| Flat per-face color + max one accent | **P2** |
| ±5 HSL value tint only, set at spawn | **P2 / P3** |
| No animated ambient tint variation | **P2** |

P2 + P3 carry 9 of 14 environmental anchors. Correct — the field exists to be *read and navigated*.

### 6.9 Environmental Prohibitions (Quick Reference)

| Prohibited | Reason |
|---|---|
| Any starfield (static or parallax) | P3 + forge-interior aesthetic |
| Nebula / space clouds | P3 + WebGL texture budget |
| Any in-run ambient background animation | P3 |
| Procedural shader-based asteroid surface detail | Beauty Must Be Cheap / budget |
| Pirate wreckage props at field edges | AP5 + new object class |
| Stripped asteroid shell props | Palette ambiguity + new silhouette class |
| Per-face shader tint animation on asteroids | P3 |
| Hue-shift sprite-tint variation | P2 (amber/cyan false signals) |
| Rounded/smooth background decorative marks | Section 3 no-curves rule |
| Parchment grid lines behind tree nodes | Competes with connector linework |
| PBR materials on any field object | Budget + wrong aesthetic |
| Camera-shake for boundary feedback | P3 violation |

---

## Section 7: UI / HUD Visual Direction

*Integrates art-director visual spec + ux-designer interaction/accessibility pass. Three AD/UX conflicts resolved explicitly (see 7.8 for decisions).*

### 7.1 HUD Type Declaration

**Screen-space HUD. The boomerang trail is the sole diegetic signal** (and doubles as fuel gauge per Section 2.1).

Diegetic ship/boomerang-mounted readouts would require reading at in-game object scale (32–48px ship) — illegible or intrusive, and competing with the boomerang arc (P3 violation).

The schematic aesthetic (Section 3.4: *UI is the blueprint above the forge, not the forge floor itself*) means screen-space HUD already belongs to the Deep Forge identity. Chamfered panels + ruled lines + blueprint-gold typography + parchment-adjacent neutral text read as **instrument readouts**, not alien consumer UI.

**One semi-diegetic embellishment**: the currency-counter icon is an angular ore-fragment silhouette (not a coin symbol) — the icon exists in the world's visual register while counter frame and number stay screen-space. Single foot in diegetic register without tracking-conflict.

### 7.2 In-Run HUD Elements

**Governing principle — Sparsity is identity.** A cluttered HUD violates the Deep Forge identity. **Every HUD element avoids the center and bottom-left quadrants** (arc zone).

**HUD panel color**: `#1A1612` HUD Panel Dark. Not Forge Black (indistinguishable from background), not brighter (competes with boomerang).

#### 7.2.1 Fuel Bar (top-left)

- **Panel**: horizontal chamfered-rectangle, ~80×14px at 1080p, chamfered on right side
- **Bar**: Blueprint Gold `#B8962E` at 70% opacity, depletes left-to-right. **Below 25%**: remaining segment shifts to Spent Cyan `#5A8A8F` — **matches the trail's low-fuel color**. Screen-space indicator and diegetic trail agree on color.
- **Glyph**: 3 parallel diagonal lines (schematic fill-level mark, same as Fuel/Survivability tree icon language per 5.5.2) at 8×8, HUD Neutral `#C4B89A`
- **Fuel-critical animation (UX-resolved, Conflict 2)**: Below 25%, fuel bar has a **slow 2-second low-amplitude repeating brightness pulse** (sub-photosensitive, 0.5Hz). **Gated by the Reduced Motion toggle** — when Reduced Motion is ON, no pulse, only the color shift + a persistent "LOW FUEL" text label (same fix as option B). Off in Reduced Motion mode; on by default.
- **'LOW FUEL' text**: Supplementary persistent text label appears at 25% threshold in HUD Neutral `#C4B89A` alongside the fuel bar — **always visible at low fuel regardless of Reduced Motion setting**. Satisfies WCAG 1.4.1 without relying on color alone.

#### 7.2.2 Currency Counter (top-left, below fuel)

- **Panel**: chamfered-rectangle, ~90×14px
- **Icon (left)**: angular ore-fragment silhouette (5–6 sided faceted polygon, consistent with Basic Asteroid from 3.5.3 but smaller). Ore Amber `#C07830` at 60% opacity rest, 100% on pickup
- **Counter digits (right)**: Share Tech Mono Regular at T3 (16–18pt), HUD Neutral `#C4B89A`, tabular figures
- **Animation on pickup**: **3-frame digital digit-flip** (hold 1f → flip 1f → brightness +10% 2f). Mechanical increment, not smooth tween. Icon opacity lifts 60% → 100% for 3 frames, returns.

#### 7.2.3 XP Counter (conditional)

**MVP recommendation: merge XP and currency into one resource** — cleaner HUD, cleaner shop loop, one less counter. If distinct: positioned top-left below currency, schematic vertical-line-cluster icon (`#C4B89A`, no distinct world hue for XP), same digit-flip animation without the icon brightness lift.

#### 7.2.4 Score / Run Timer

**Not in MVP.** No space reserved.

#### 7.2.5 Zone Indicator (top-right)

- **Panel**: chamfered-rectangle, ~80×14px, chamfer on left side (mirrored from left-edge panels)
- **Glyph**: chamfered L-mark (same language as arena boundary marks from 6.5.1) at 6×6
- **Text**: "ZONE I" / "ZONE II" — Roman numerals, uppercase, Share Tech Mono T5, Blueprint Gold `#B8962E` at 80% opacity
- **Animation**: none in-run. Fades in during run-start transition (7.5.1).

#### 7.2.6 Boss HP Bar (bottom-center)

- **Position**: bottom-center, ~24px from bottom edge. Industry convention (Hollow Knight, Hades, Dark Souls).
- **Panel**: wide chamfered-rectangle, ~240–280px wide, chamfered both ends
- **Fill**: `#8A1A1A` Pirate Crimson interior — **documented exception to no-crimson-in-UI rule**. The bar represents the boss; crimson semantics are load-bearing. Depletes left-to-right. Below 50%/40% (phase transition threshold): fill shifts to Deepwater Crimson `#6B2030`.
- **Phase transition tick**: thin 1px Blueprint Gold vertical mark at the threshold position inside the bar. Always visible — reads "if bar depletes past this, something changes."
- **World-space phase-transition flash**: per UX recommendation, **a brief angular flash sprite appears in world-space above the boss** at the exact moment the phase threshold is crossed (the same 3-frame Impact White spark burst as the boss's joint-seize signal from Section 5.3.1). Eliminates eye-travel risk from arc to bottom-center HP bar during phase transition.
- **Panel frame**: HUD Panel Dark (not crimson)
- **Animation on spawn**: slides up from below screen edge (12px, 0.25s, linear)
- **Boss label**: anonymous for MVP (internal "Claim Jumper"/"Warden" names are AD reference only per 5.7)

#### 7.2.7 Damage Numbers

Per Section 4.6 (colors, timing, rise-and-fade). **World-space, NOT HUD canvas.** Confirm with ui-programmer: damage numbers must not be on HUD canvas layer.

#### 7.2.8 HUD Layout Summary

| Element | Position | Color | Always visible |
|---|---|---|---|
| Fuel bar + glyph + 'LOW FUEL' | Top-left | Blueprint Gold bar / Spent Cyan below 25% | Yes |
| Currency counter + ore icon | Top-left (below fuel) | Ore Amber icon / HUD Neutral digits | Yes |
| XP counter (if distinct) | Top-left (below currency) | HUD Neutral | Yes (if present) |
| Zone indicator | Top-right | Blueprint Gold | Yes |
| Boss HP bar + world-space phase flash | Bottom-center + above boss | Crimson interior / Blueprint Gold tick | Boss phase only |
| Damage numbers | World-space, contact point | Per Section 4.6 | On hit only |

**Center and bottom-left are clear** — the arc has room to breathe.

### 7.3 Typography Direction

#### 7.3.1 Font Personality & Choice

Register: **condensed technical/engineering sans-serif** — reads as instrument-panel labeling from an industrial manual. Slightly narrow proportions, near-uniform stroke weight, squared terminals on C/G/S. Not rounded. Not decorative.

**Font stack (free-commercial-use, embeddable, Latin subset WOFF2)**:
- **Rajdhani Medium** — T4 body / tree node labels / HUD non-numeric text (primary)
- **Rajdhani SemiBold** — T1 Title / T2 headings / gateway node labels (landmarks)
- **Share Tech Mono Regular** — all HUD counter numerals, damage numbers (mechanical readout precision, tabular)

**Build size estimate**: ~50–80KB total Latin WOFF2. Negligible.

#### 7.3.2 Size Tiers at 1920×1080 (with UX-resolved element-by-element floors — Conflict 3)

| Tier | Size | Use | Floor rule |
|---|---|---|---|
| **T1 — Title** | 48–56pt | Main menu game title | SemiBold, one-time use |
| **T2 — Section heading** | 20–24pt | Run summary headers, pause menu sections | SemiBold |
| **T3 — HUD Counter** | **16–18pt** | Currency, fuel %, XP digits, damage numbers | Share Tech Mono tabular. **Gameplay-critical floor: 12pt absolute minimum at in-game display; 16pt recommended** |
| **T4 — Node label** | **12–13pt** | Skill tree node names | Medium weight. **12pt functional floor for WebGL** (up from AD's 11pt — WebGL text rendering degrades ~2pt below native) |
| **T5 — Micro / tooltip** | 10pt | Zone indicator, cluster archetype labels, tooltip footnotes | **10pt floor ONLY for non-gameplay / read-while-stopped contexts (tooltip, tree screen labels)**. Never for in-run gameplay labels. |

**No italic.** **No weight above SemiBold.** **No numeric-character size below 12pt.**

#### 7.3.3 Localization Note

English-only MVP per AP5. Rajdhani covers Latin + Devanagari; lacks CJK/Arabic/Hebrew/Cyrillic. Post-MVP localization requires typography system decision before content sprint — flag to producer.

### 7.4 Iconography Style

**Core rule**: every icon is a **schematic glyph** — angular marks communicating function, not appearance. Not a pictogram, not a photographic reduction, not a silhouette drawn naturalistically.

- **Skill tree icons**: 48×48 authored, 24×24 displayed, 3+ pixel marks only (per 5.5.2)
- **HUD icons**: 16×16 authored, 8×8 displayed
  - **Fuel glyph**: 3 parallel diagonal lines (matches tree Fuel/Survivability icon language)
  - **Currency icon**: angular ore-fragment silhouette, Ore Amber, 5–6 sided faceted polygon
- **No circles, drops, hearts, flames, coins, stars, dollar signs.** No external icon library assets.

### 7.5 UI Animation Feel

#### 7.5.1 Menu Transitions

**Main menu → skill tree (schematic draw-on reveal)**:
- Connectors draw left-to-right over 0.4s
- Node outlines appear in radiating wave from root (~0.05s per node)
- Forge silhouettes (6.6.1) fade in simultaneously, 0–20% opacity over 0.5s
- Total: ~0.6–0.8s
- **Reduced Motion**: skip to final state, 1-frame crossfade

**Skill tree → run start**: Reverse — nodes dim (0.2s), connectors fade (0.15s), parchment fades to Forge Black (0.3s) simultaneous with game field fade-up. Recently purchased node dims last. ~0.5–0.6s total.

**Pause overlay**: `#0D0A08` at 70% alpha fades in 0.15s. Pause panel slides up 8px from 8px below resting position over 0.2s linear.

#### 7.5.2 Node Purchase Animation

5-step sequence (~0.55s total):
1. **F0–F3 (0.05s)**: Clicked node outline lifts to Blueprint Gold 100% — confirmation flash (this is also the **first click** in the two-click purchase pattern from 7.UX.4)
2. **F4–F12 (0.15s)**: Interior fills `#5C4A20` Node Unlocked via **radial sweep from center outward** (not fade-in)
3. **F12–F20 (0.13s)**: Node Point Light2D fades 0 → target intensity over 8 frames. Connector opacity lifts 60% → 100% simultaneously.
4. **F20–F28 (0.13s)**: **Single expanding Blueprint Gold hex outline** from node center to ~150% node diameter, 10–20% opacity, fading to 0. **One ring, one pass — no particle burst.**
5. **F28+**: Settled. Light2D accumulates with existing unlocked nodes — tree grows visibly brighter.

Audio sync point: F12 (Point Light2D fade-in start).

**Reduced Motion**: skip to step 5, 1-frame crossfade.

#### 7.5.3 Counter Animations

- **In-run (on pickup)**: 3-frame digital digit-flip + icon brightness lift (per 7.2.2)
- **Run-end summary**: slower theatrical tick-up from 0 to final over 1–2s, one digit per frame. Between-run, acceptable.
- **Reduced Motion**: instant jump to final value, no animation.

#### 7.5.4 Hover and Focus Animations

**Tree node hover**:
- Fill: Locked → Hover `#7A6030` over 2 frames
- Inner hex ring: Blueprint Gold additive-blend at ~30% opacity (per 3.5.6)
- Connector path highlight: 85% opacity on path to hovered node
- Tooltip: 400ms mouse dwell delay (prevents flicker during cursor travel), instant appear after delay

**Menu option hover**:
- Text color lift `#C4B89A` → `#E8D8A8`
- 1px Blueprint Gold baseline underline at 60% opacity (baseline rule only, not full underline)
- 2-frame transition

**Button hover**:
- Text: Blueprint Gold 100%, Panel: `#221E18`, inner chamfered-edge highlight 25% Blueprint Gold
- 2-frame transition

#### 7.5.5 Focus Ring (gamepad / keyboard nav) — Conflict 1 resolved

- **Outer chamfered-rectangle, 1px Blueprint Gold at 75% opacity, 2px outside element boundary**
- **Persistent** until navigation moves (unlike hover which requires pointer/focus)
- **Distinct from hover**: hover = interior (fill + inner ring), focus = exterior (outer frame). Both can coexist on the same element.
- **Contrast resolved**: 75% opacity on HUD Panel Dark satisfies WCAG 1.4.11 3:1 minimum for non-text UI component contrast.

### 7.6 UX — Interaction Patterns & Input

#### 7.6.1 Skill Tree Node States (UX-added — 3 states, not 2)

| State | Visual treatment |
|---|---|
| **Locked** (prereq unmet) | Node Locked fill `#3A3428`, Blueprint Gold outline 45%, no hover response, connectors 30–60% |
| **Available-unaffordable** (prereq met, currency insufficient) | Full-opacity node body BUT with **reduced Blueprint Gold outline saturation** + small angular mark on node corner indicating "prereq met, cost unmet." Hover activates interior ring but purchase click is rejected with a short 1-frame outline flash (no sound alarm, no text popup) |
| **Available-to-purchase** (prereq met, currency sufficient) | Full hover + Blueprint Gold brightening + connector highlight as specified |

Without this differentiation, players attempt unaffordable purchases, fail without understanding why → P1 violation.

#### 7.6.2 Two-Click Purchase Pattern

1. **First click** on a tree node: triggers confirmation flash (step 1 of 7.5.2 node purchase animation), opens tooltip/detail panel
2. **Second click** on the same node: confirms purchase, triggers steps 2–5 of the node purchase animation
3. Click on different node or empty space: cancels pending purchase, tooltip dismisses

Prevents accidental irreversible spends.

#### 7.6.3 Tooltip Behavior

- **Mouse hover**: 400ms dwell delay, then appear instantly. Dismiss immediately on cursor-leave.
- **Gamepad**: North button (Y/Triangle) **toggles** tooltip for focused node. Tooltip tracks focus on d-pad movement (does not require re-press). Dismisses on Cancel (East) or screen context change.
- **DO NOT use hold-to-confirm for tooltip** — conflicts with purchase-confirm flow, creates accidental-purchase risk.

#### 7.6.4 Skill Tree Navigation (d-pad / Tab)

**UGUI default scene order will produce broken-feeling navigation on a hex grid.** Required: explicit UGUI Navigation override on every node.

**D-pad mapping**: spatial-nearest-neighbor per quadrant. Hex has 6 logical neighbors → collapse to 4 d-pad directions (up-right + right → Right, down-right + right → Right, etc.). Nearest wins. Pre-computed at tree load.

**Keyboard Tab**: spatial left-to-right, tier-by-tier. Root = Tab start. Shift+Tab reverses.

#### 7.6.5 Zoom / Pan

- **Mouse**: scroll-wheel zoom; middle-mouse or right-click-drag to pan; left-click-drag on empty parchment to pan; cursor changes to grab on parchment hover
- **Gamepad**: L/R triggers = smooth zoom (analog) or bumpers = stepped zoom. Left stick = pan when zoomed in (faster than d-pad repositioning).
- **"Scroll to zoom" hint**: first-entry-only Share Tech Mono text at bottom of skill tree screen (HUD Neutral, small), dismisses after first zoom input.

#### 7.6.6 Gamepad Action Bindings (tree screen)

- **South (A/Cross)**: confirm purchase
- **East (B/Circle)**: cancel / close tree / return to run-start
- **North (Y/Triangle)**: toggle tooltip
- **Start/Options**: no-op in tree screen (reserved for pause in-run)

#### 7.6.7 Pause

- **Key/button**: Escape (keyboard) / Start/Options (gamepad). Genre standard.
- **Behavior**: halt game tick (separate from render, per TD custom fixed-timestep mandate), present pause menu within 1 frame of input (no fade-in delay — input-delay anxiety in bullet-hell context)
- **WebGL focus-trap (REQUIRED)**: pause must apply JS-side canvas focus lock. Without it, Tab escapes to browser chrome. Handoff to ui-programmer.

### 7.7 Accessibility Baseline (UX-added, REQUIRED at MVP)

1. **Reduced Motion toggle** — replaces all animated transitions with instant state-changes. Every animated transition already has a final state; Reduced Motion skips to it with 1-frame crossfade.
   - Affects: skill tree reveal, node purchase animation (steps 2–4), digit-flip counters, fuel-critical looping pulse (7.2.1)
2. **UI Scale setting** — 75% / 100% / 125% via Canvas Scaler reference resolution. Accessibility for low-vision / monitor-size players.
3. **WebGL focus-trap on pause** (see 7.6.7)
4. **'LOW FUEL' persistent text label** (see 7.2.1) — satisfies WCAG 1.4.1 regardless of animation state
5. **Element-by-element font size floor** (see 7.3.2) — 12pt functional floor for gameplay-critical labels in WebGL; 10pt OK for tooltip/secondary
6. **75% focus-ring opacity** (see 7.5.5) — WCAG 1.4.11 3:1 contrast
7. **CVD backup cues** — inherit from Section 4.5. Blueprint Gold at 40% on HUD Panel Dark was flagged; resolved by the 75% focus ring lift above.

**Deferred to post-MVP with documented target**: High Contrast toggle (white-substitute for Blueprint Gold in focus ring + fuel bar, full opacity on all UI). Target: first post-launch accessibility sprint.

### 7.8 Conflict Resolution Log

| Conflict | AD position | UX position | **Resolved as** |
|---|---|---|---|
| Focus ring opacity | 40% | ≥70% (WCAG 1.4.11) | **75% opacity** |
| Fuel-critical signal | Single 2-frame pulse, no looping | Slow 2s repeating pulse + LOW FUEL text | **Slow 2s pulse (Reduced-Motion gated) + persistent 'LOW FUEL' text label** |
| Font size floor | Blanket 9–10pt T5 | Element-by-element: 12pt gameplay / 10pt tooltip | **Element-by-element per 7.3.2** |

### 7.9 Pillar Anchors — Section 7

| Rule | Pillar |
|---|---|
| Screen-space HUD; boomerang trail sole diegetic | **P2** |
| HUD sparsity; center + bottom-left clear | **P2** |
| Fuel bar Spent Cyan matches trail below 25% | **P2** |
| Currency Ore Amber icon activation | **P2** |
| 'LOW FUEL' persistent text + Reduced-Motion gated pulse | **P2 + Accessibility** |
| Boss HP bar crimson interior (documented exception) | **P2** |
| World-space phase-transition flash + HP bar tick | **P2** (no eye-travel risk) |
| Node purchase: radial fill + single ring, no burst | **P1 + P5 + P4 restraint** |
| Tree grows visibly brighter as nodes unlock | **P1** |
| Schematic draw-on reveal | **P4** |
| Angular chamfered-rectangle buttons/panels | **P4 via shape grammar** |
| Rajdhani + Share Tech Mono typography | **P4** |
| No italic, no weight above SemiBold | **P4** |
| Schematic-glyph icons, no circles/organic | **P4 via shape grammar** |
| Three tree node states (locked/unaffordable/affordable) | **P1** |
| Two-click purchase | **P1 + Accessibility** |
| Spatial-nearest-neighbor d-pad + explicit UGUI Navigation | **P5 + Accessibility** |
| WebGL focus-trap on pause | **Functional + Accessibility** |
| Reduced Motion + UI Scale toggles | **Accessibility** |
| 75% focus-ring opacity (WCAG 1.4.11) | **Accessibility** |
| 12pt gameplay-label floor in WebGL | **Accessibility** |

### 7.10 Production Constraints (handoff notes)

- In-run HUD on single Screen Space — Overlay canvas. Damage numbers on separate world-space canvas layer below HUD.
- Skill tree = UGUI (TD mandate per architectural mandates). Node purchase steps 2–4 may require technical-artist support if expanding ring needs world-space approach.
- Font embedding: WOFF2 Latin subset, Unity TextMeshPro, verify rendering at 12pt at target canvas resolution in actual WebGL build (not editor).
- HUD animations via event-listener pattern, not polling. Deterministic, not framerate-dependent.
- Boss HP bar prefab enable/disable, not alpha-zero (prevents raycast capture on hidden UI).
- Build size: font family ~50–80KB Latin WOFF2. Add to build manifest tracking.

---

## Section 8: Asset Standards

> **Purpose:** Define the technical envelope and production discipline that every shipped asset must conform to. This section is the contract between the art pipeline and the WebGL build. If a standard here conflicts with an art-director preference elsewhere, **this section wins** — it is grounded in Unity 6.3 URP 2D + WebGL IL2CPP platform reality and anchored to TD-FEASIBILITY's memory, draw-call, and GC budgets.
>
> **Conflicts resolved against art-director preferences:**
> 1. **Audio sample rate: 44.1 kHz** (AD preferred 48 kHz). WebGL AudioContext defaults to 44.1 kHz; runtime resampling wastes CPU and can introduce pitch artifacts on low-end browsers. AD's artifact concern at impact moments is mitigated by pre-mastering at 44.1 kHz rather than resampling at load.
> 2. **Zone 1 / Zone 2 separate atlases** (AD preferred co-packing). `Addressables.Release()` operates at whole-atlas granularity; co-packing would prevent Zone 1 memory reclaim during Zone 2 play and break TD's <512 MB peak heap budget.

### 8.1 Sprite Assets

**Source authority:**

- **Source format:** PNG 32-bit RGBA, no interlacing, no metadata. Authored in Aseprite (recommended) or Photoshop with sRGB color profile.
- **Source PPU:** 100 pixels per Unity unit. All game-field sprites author at native PPU — no scaling at import.
- **Color space:** sRGB (Project → Linear color space, but textures remain sRGB-tagged). Hero/character sprites: sRGB **on**. UI: sRGB **on**. Data-only textures (noise masks, ramps): sRGB **off**.
- **Power-of-two dimensions:** **Required for all atlased sprites.** WebGL has strict NPOT performance penalties on some older drivers. Atlases are POT; individual sprites can be NPOT inside a POT atlas, but isolated (non-atlased) textures must be POT.

**Per-asset-class resolution tiers:**

| Tier | Native Size | Examples | Filter | Mipmaps |
|------|-------------|----------|--------|---------|
| **Hero** | 128×128 | Player ship, bosses, boomerang | Point | Off |
| **Field** | 32×32 – 64×64 | Enemies, ore, FX quads, particles | Point | Off |
| **UI** | 256×256 – 512×512 | Tree panels, HUD frames, tooltips | Bilinear | Off |
| **Decorative** | 32×32 | Forge silhouettes, scoring marks | Point | Off |

**Per-sprite import settings:**

| Setting | Hero/Field | UI | Rationale |
|---------|-----------|-----|-----------|
| Texture Type | Sprite (2D and UI) | Sprite (2D and UI) | Standard 2D workflow |
| Pixels Per Unit | 100 | 100 | Uniform camera scale |
| Mesh Type | Tight | Full Rect | Tight for overdraw reduction; UI rarely benefits |
| Filter Mode | Point (no filter) | Bilinear | Preserve chamfered pixel-art edges; UI text/panel smoothness |
| Wrap Mode | Clamp | Clamp | No tiling in this art style |
| Compression | **None** (RGBA32) | DXT5 (Normal Quality) acceptable | Flat-color edges destroyed by DXT on game-field; UI is more forgiving |
| Generate Mip Maps | **Off** | Off | No distance scaling in fixed-camera 2D |
| Read/Write Enabled | **Off** | Off | Doubles memory; only needed for procedural mesh readback |
| sRGB | On (color) | On | Correct gamma for authored colors |
| Max Size | Matches source | Matches source | Never upscale at import |

**Compression policy (hard rule):**

- **Game-field sprites (Hero + Field + Decorative):** **RGBA32 uncompressed.** DXT1/DXT5/Crunch breaks flat-color edges and the Weapon Cyan hue at boomerang pixel boundaries — measured visible degradation.
- **UI sprites:** DXT5 Normal Quality permitted. UI viewed at stable scale with less edge scrutiny; compression saves 3–4× memory on large panels.
- **Decorative/environmental:** RGBA32 if ≤32×32; DXT5 acceptable if a background element >64×64 (e.g. forge silhouette vignettes).

### 8.2 Sprite Atlases

**Atlas technology:** Unity **Sprite Atlas V2** (built-in). No external packers (TexturePacker etc.). V2 enables Late Binding for Addressables and proper group-scope variants.

**Atlas inventory (5 atlases):**

| Atlas | Contents | Max Size | Est. Runtime Memory | Load Trigger |
|-------|----------|----------|---------------------|--------------|
| `Atlas_Core` | Player ship + boomerang + persistent UI-in-run HUD + damage-number glyphs + impact-white flash | 1024×1024 | ~4 MB (RGBA32) | Always loaded |
| `Atlas_Zone1_Field` | Zone 1 enemies (Grunt/Ranged/Tank) + Zone 1 ore + Zone 1 boss ("Claim Jumper") + Zone 1 VFX quads | 2048×2048 | ~16 MB (RGBA32) | Zone 1 enter |
| `Atlas_Zone2_Field` | Zone 2 enemies (same silhouettes, Zone 2 palette shift) + Zone 2 ore + Zone 2 boss ("Warden") + Zone 2 VFX quads | 2048×2048 | ~16 MB (RGBA32) | Zone 2 enter |
| `Atlas_VFX` | Shared VFX frame strips (impact bursts, destruction, debris, cyan trail frames) | 1024×1024 | ~4 MB (RGBA32) | Always loaded |
| `Atlas_SkillTree` | Tree panel chrome, node icons, connector base, tooltip frames, schematic-parchment background texture | 2048×2048 | ~8 MB (DXT5 UI) | Tree open; unload on tree close (optional optimization) |

**Total atlas envelope: ~48 MB peak at Zone boundary** (Atlas_Core + both Zone atlases during brief transition + VFX + SkillTree). Steady-state in-zone: ~32 MB. Memory safety factor vs 512 MB budget: ~16× headroom.

**Atlas packing rules:**

- Padding: **2 px** on all sprite edges in the atlas (prevents bleed at point-filtered scales).
- Max sprite dimension inside atlas: 256×256 for Hero; 128×128 for Field; 512×512 for UI.
- **No rotation** in packing (Sprite Atlas V2 option: unchecked). Breaks animation frame strips and debug readability.
- **Tight mesh** enabled on packing for game-field atlases (reduces per-quad overdraw).

### 8.3 Sprite-Sheet Animations

- **Format:** Horizontal strip, left-to-right temporal order, equal-width frames, even frame count preferred (pairs for symmetric anticipation/recovery).
- **Frame rate standards:**
  - Impact / VFX: **24 fps** (cinematic feel for hits)
  - Boomerang flight cycle: **24 fps** (per Section 5's 4-beat cycle)
  - Destruction sequences: **12 fps** (reads as weighty, not frantic)
  - Enemy idle / loop: **8–12 fps** (low end for moderate/dense enemy counts)
- **Max frames per clip:** 16 (keeps atlas footprint bounded; longer sequences split into state transitions).
- **No bone rigging / no skeletal animation.** Sprite-sheet frames only. Keeps CPU cost predictable and avoids Unity 2D Animation package dependency.
- **Authoring resolution = runtime resolution.** No pre-render-at-high-res-and-downsample.

### 8.4 Particles / VFX

**System:** Unity built-in `ParticleSystem` (Shuriken). **Not VFX Graph** (Unity 6.3 deprecates Legacy Particle System in favor of VFX Graph, but VFX Graph requires compute shader support not universal on WebGL 2.0; Shuriken is WebGL-safe).

**Hard prohibitions:**

- **Collision module: PROHIBITED.** CPU-expensive, scales poorly on WebGL. Particles are visual-only in this game.
- **Trails module: PROHIBITED.** Generates geometry + overdraw; the boomerang trail uses a dedicated `TrailRenderer` on the boomerang itself, managed at a higher LOD tier.
- **Lights module: PROHIBITED.** Each particle-light spawns a Light2D draw; breaks draw-call budget instantly.
- **Sub-emitters: Allowed** for destruction cascades only, with explicit count caps.
- **GPU Instancing:** enabled on all particle materials (required for batching under URP 2D).

**Budgets:**

- **Simultaneous particle cap: 500** across all systems, engine-enforced via `maxParticles`.
- **Per-impact-event cap: 80 particles** (one enemy destruction = max 80 particles sum across its burst systems).
- **Particle-system count per scene: ≤8 active** (pool-managed).

**Pool sizes (shipping defaults):**

| Pool | Size | Notes |
|------|------|-------|
| Impact flash | 12 | Reused across all enemy/asteroid hits |
| Destruction debris | 8 | One-shot on enemy kill; auto-return on `Stopped` callback |
| Damage numbers | 24 | TextMeshPro instances; pooled, not re-allocated |
| Boomerang trail | 1 | Singleton; reset on throw, not pooled |
| Boss phase flash | 2 | Rare events, small pool |

### 8.5 Shaders and Materials

- **Shader basis:** Sprite-Lit-Default (URP 2D) for all illuminated sprites. Sprite-Unlit-Default for UI and decorative elements not affected by Light2D.
- **Custom shaders:** Only permitted when a specific visual effect cannot be achieved via material parameter — every custom shader requires `unity-shader-specialist` review and must justify its per-frame cost.
- **Material variants:** **Use `MaterialPropertyBlock`, not `Renderer.material.SetColor()`.** The latter breaks SRP Batcher instance groups (each unique material instance = separate draw call). Zone tinting, damage-flash value-drop, charge-mark gold mark: all `MaterialPropertyBlock`.
- **Alpha:** Premultiplied alpha rejected — standard straight alpha on all sprite materials (WebGL shader compatibility).
- **No post-processing stack.** All glow/aura effects are additive sprite quads on top of base sprites (per Section 4's "beauty must be cheap" principle + TD's feature-stripped URP 2D mandate).

### 8.6 Audio Assets

**Source authority:**

- **Source format:** WAV PCM, **44.1 kHz**, 16-bit, mono for SFX, stereo for music. (Resolves conflict against AD's 48 kHz preference — see section header.)
- **Ship format:** Ogg Vorbis.
- **SFX bitrate:** 96–128 kbps Vorbis, mono.
- **Music bitrate:** 160–192 kbps Vorbis, stereo.
- **Loudness normalization (pre-encode):**
  - SFX peak: **−6 dBFS true peak**
  - SFX loudness: **−12 dBFS RMS**
  - Music peak: **−3 dBFS true peak**
  - Music loudness: **−18 dBFS RMS**
  - No auto-normalize in Unity Import Settings — bake the loudness in the source file. Preserve headroom for ducking and mix.

**Per-clip import settings:**

| Clip type | Load Type | Compression | Preload Audio Data |
|-----------|-----------|-------------|--------------------|
| UI SFX (click, hover) | Decompress On Load | Vorbis | ✓ |
| In-run SFX (impacts, catches) | Decompress On Load | Vorbis | ✓ |
| Boss SFX | Compressed In Memory | Vorbis | ✗ (load on boss spawn) |
| Music loops | **Streaming** | Vorbis | ✗ |
| Stingers (Zone complete, death) | Decompress On Load | Vorbis | ✓ |

- **Force to Mono:** **On** for all SFX. Mono halves runtime memory and preserves localization of hits at the panner layer.
- **Sample rate override:** Preserve source rate (Preserve Sample Rate: ✓).

**Voice cap / ducking:**

- `AudioSource` voice cap: **24 concurrent** (WebGL `AudioContext` soft limit varies per browser; 24 is the floor tested safe).
- Pool `AudioSource` components with priority tiers. Steal oldest low-priority voice when cap hit.
- Music bus ducks SFX bus by −3 dB during boss phases (optional; Tier 2 feature, not MVP blocking).

### 8.7 Fonts

- **Typefaces:** Rajdhani (primary), Share Tech Mono (numeric/diegetic) — from Google Fonts, SIL Open Font License, embedded legally.
- **Format:** WOFF2 Latin subset only for Tier 1 MVP (no CJK, no extended Latin). Subset size per face: ~30–50 KB WOFF2.
- **Unity import:** TextMeshPro `.asset` font atlases generated at import, **Static mode** (fixed glyph set, not dynamic).
- **Atlas sizes per face:** 512×512, R8 single-channel (0.25 MB per face raw, ~0.05 MB DXT1-like in Unity).
- **Total font atlas memory:** ~0.75 MB (3 atlases if a dedicated bold variant is needed).
- **Fallback chain:** Rajdhani → system `sans-serif`. If WOFF2 fails to load, graceful fallback via CSS `@font-face` is impossible in WebGL canvas; TextMeshPro pre-bakes atlas at build time so this cannot fail at runtime.
- **Build validation:** verify the atlas renders at 12pt in an actual WebGL build (not editor Play mode) per Section 7 UX mandate.

### 8.8 File Naming Convention

`[category]_[name]_[variant]_[size].[ext]`

| Category prefix | Scope | Example |
|-----------------|-------|---------|
| `ship_` | Player ship assets | `ship_body_base_128.png` |
| `weapon_` | Boomerang + boomerang-mod assets | `weapon_boomerang_flight_01_128.png` |
| `enemy_` | Enemy sprites by archetype | `enemy_grunt_idle_01_32.png` |
| `boss_` | Boss-specific sprites | `boss_claimjumper_phase1_256.png` |
| `ore_` | Asteroid / ore sprites | `ore_common_32.png` |
| `vfx_` | Particle strips and flash quads | `vfx_impact_flash_01_64.png` |
| `ui_` | HUD, tree, menu chrome | `ui_tree_node_unlocked_64.png` |
| `bg_` | Background elements (forge silhouettes, vignette) | `bg_forge_silhouette_left_256.png` |
| `sfx_` | Sound effect sources | `sfx_boomerang_catch_01.wav` |
| `mus_` | Music stems | `mus_zone1_loop.wav` |
| `font_` | Font atlas source references | `font_rajdhani_semibold.woff2` |

**Rules:**

- **Snake_case only.** No spaces, no hyphens, no PascalCase in asset filenames. (Unity meta files preserve case exactly; cross-OS case-sensitivity bites WebGL CI on Linux.)
- **Variant suffix** (`_01`, `_02`) is two-digit, zero-padded.
- **Size suffix** matches authored tier — a 128 px sprite is `_128`, never omit or round.
- **No spaces anywhere in the Assets path.** Breaks Addressables catalog paths.

### 8.9 Directory Layout (`Assets/Art/` + `Assets/Audio/`)

```
Assets/
├── Art/
│   ├── Sprites/
│   │   ├── Ship/
│   │   ├── Weapon/
│   │   ├── Enemies/
│   │   │   ├── Zone1/
│   │   │   └── Zone2/
│   │   ├── Bosses/
│   │   ├── Ore/
│   │   ├── VFX/
│   │   └── Background/
│   ├── Atlases/         # Sprite Atlas V2 assets (.spriteatlasv2)
│   ├── UI/
│   │   ├── HUD/
│   │   ├── Tree/
│   │   └── Menus/
│   ├── Materials/
│   ├── Shaders/
│   └── Fonts/            # TMP .asset atlases + WOFF2 source
├── Audio/
│   ├── SFX/
│   │   ├── UI/
│   │   ├── Weapon/
│   │   ├── Enemies/
│   │   └── Bosses/
│   └── Music/
│       ├── Zone1/
│       ├── Zone2/
│       └── Stingers/
└── AddressableAssetsData/   # catalogs, group assets
```

### 8.10 Addressables Group Structure

- **Group_Core** — always loaded. Contains: `Atlas_Core`, `Atlas_VFX`, font atlases, core UI prefabs, player ship prefab, boomerang prefab. Flagged as **Cannot Change Post Release** (shipped in initial bundle).
- **Group_Zone1** — loaded on Zone 1 enter, released on Zone 1 exit. Contains: `Atlas_Zone1_Field`, Zone 1 enemy prefabs, Zone 1 boss prefab, Zone 1 music stems, Zone 1 SFX pack.
- **Group_Zone2** — mirror of Zone 1.
- **Group_SkillTree** — loaded on tree open, optionally released on tree close (Tier 2 optimization). Contains: `Atlas_SkillTree`, tree panel prefabs, schematic parchment texture.
- **Group_Stingers** — preloaded at session start (small footprint, frequent access): death stinger, zone-complete stinger, purchase confirmation SFX.

**Labels:** `zone1`, `zone2`, `boss`, `ui`, `persistent`, `audio`. Used for filtered preload queries.

**Initial download target:** **<8 MB compressed (Brotli)**, **<20 MB decompressed** on first cold-load — per TD <30s cold-load mandate. Only `Group_Core` + `Group_Stingers` + `Group_SkillTree` (if skill tree is the landing screen) ship in the initial bundle. Zone groups stream on demand.

### 8.11 CI Validation (asset-lint hooks)

Pre-commit (or CI) hook `tools/art-lint.py` must flag any asset violating the following, blocking the commit:

| Rule | Violation |
|------|-----------|
| Source PNG dimension >2× authored tier | Artist tried to author at wrong scale |
| `Generate Mip Maps: true` on any sprite | Wastes 33% memory on fixed-camera 2D |
| `Filter Mode: Trilinear` on any sprite | No mipmap chain — defaults to bilinear anyway, but flag as misconfiguration |
| `Texture Compression: Crunch` on game-field sprite | Breaks flat-color edges |
| `Read/Write Enabled: true` on any sprite | Doubles runtime memory; unnecessary outside procedural mesh cases |
| Filename contains space or uppercase outside category prefix | Breaks Linux CI + Addressables catalog paths |
| WAV source sample rate ≠ 44.1 kHz | Forces runtime resampling |
| Audio peak exceeds −3 dBFS | Loudness discipline violation |
| Asset is not referenced by any atlas, scene, or prefab for 30 days | Garbage — flag for removal |

### 8.12 Section 8 → Pillar Trace

| Standard | Anchors to |
|----------|-----------|
| RGBA32 uncompressed for game-field sprites | **P1** (Weapon Cyan hue integrity) + **P2** (boomerang edge readability) |
| Separate Zone 1 / Zone 2 atlases | **TD** (memory reclaim under 512 MB ceiling) |
| 44.1 kHz audio source | **WebGL AudioContext compatibility** (cold-load <30 s) |
| RGBA straight alpha, no premultiplied | **WebGL shader correctness** |
| Snake_case + no-spaces filenames | **Linux CI + Addressables catalog reliability** |
| Shuriken particles (not VFX Graph) | **WebGL 2.0 compute-shader non-support** |
| Collision/Trails/Lights modules prohibited | **Draw-call budget** (<100 typical) |
| MaterialPropertyBlock for tinting | **SRP Batcher integrity** (draw-call minimization) |
| WOFF2 Latin subset | **Cold-load <30 s** |

### 8.13 Asset Standards Production Notes (handoff)

- Set up `Assets/Editor/AssetImporter_Defaults.cs` as an `AssetPostprocessor` that applies the import settings tables above on any new PNG/WAV dropped into the relevant subfolder. This prevents artists from needing to remember per-file settings.
- Establish `tools/art-lint.py` as the CI validation script; fail the Unity Cloud Build if any rule fires. GitHub Actions job runs this on PR.
- Atlases should be built in a dedicated pre-build step (`Window > 2D > Sprite Atlas Packer`). Do NOT leave atlas packing to on-play-mode auto-packing; inconsistent builds across team members are a common source of "works in editor, breaks in WebGL" bugs.
- Audio source files must be checked in as WAV (uncompressed); Unity re-encodes to Vorbis at build time. Never check in pre-encoded Vorbis — loses authoring fidelity on future re-encodes.
- Font atlas baking: commit the TMP `.asset` files, not the WOFF2 source alone. The runtime never sees WOFF2 — Unity uses the baked atlas. Keep WOFF2 source in repo as archival reference only.

---

## Section 9: Reference Direction

*Establishes the reference corpus for the Deep Forge visual direction. This section is the art director's working brief: what to borrow, what to reject, how to enforce the direction when artists are commissioned, and how the reference set maps to the five pillars. All recommendations derive from the visual identity stated in Sections 1–8. No reference may override Section 1; every reference is subordinate to the principles already locked.*

---

### 9.1 Primary Visual References (Anchor Set)

These seven works define the range of Deep Forge. They are not a mood board; they are a doctrine. For each, one specific visual quality is borrowed and one specific quality is explicitly rejected. Rejections are as important as borrowings — knowing what not to take prevents reference drift.

---

#### 9.1.1 Dead Cells (Motion Twin, 2018)

**Medium**: 2D sprite game, pixel-art at integer scale.

**What we borrow — silhouette-first combat readability under high visual density.** Dead Cells operates at a framerate where individual frames are unreadable; the player navigates by reading silhouette and motion direction. Every enemy type in Dead Cells reads as a unique shape at a glance, not by color alone. This is the exact discipline required by Section 3's 32×32 rule and P2. Dead Cells demonstrates that dense enemy variety is survivable at high speed when silhouette language is rigorously maintained across every archetype.

**What we reject — polychromatic background environments.** Dead Cells' dungeons use rich environmental color: purple-grey crypts, glowing amber-orange prisons, teal catacombs. Every environment has its own chromatic identity built into the background and wall tiles. Deep Forge does not have multiple chromatic environments — the background is Forge Black permanently, and zone variation happens entirely through enemy and asteroid hue shifts, not environmental re-theming. An artist who looks at Dead Cells and concludes "zones should each have a distinct background palette" is importing the wrong lesson.

**Pillar anchor**: P2 (silhouette discipline) and Section 3 (32×32 rule).

---

#### 9.1.2 Into the Breach (Subset Games, 2018)

**Medium**: 2D sprite strategy, minimalist grid aesthetic.

**What we borrow — meaning through geometry and sparse color commitment.** Into the Breach uses an extreme economy of palette: the player faction is blue, enemy faction is a warm-brown-grey, environmental hazards inherit from their type. The entire game communicates faction and threat through geometric register without ever resorting to text labels or UI callouts. Every tile and unit reads its role. This maps directly to the principle established in Section 4.2: semantic color vocabulary where each hue is a concept, not a decoration.

Additionally, Into the Breach's skill/upgrade panel uses schematic grid-and-line presentation that anticipates Deep Forge's skill tree language: information organized spatially, not alphabetically, on a grid that carries navigational meaning.

**What we reject — warmth and approachability in UI chrome.** Into the Breach's UI is warm-neutral, softly contrasted, designed not to intimidate. Deep Forge's UI is a schematic from a working forge: Blueprint Gold on near-black, chamfered hard edges, cool parchment surface. Warmth is earned specifically (skill tree parchment, Zone Victory gold), not distributed through the interface. An artist referencing Into the Breach must not mistake its comfort-of-use visual tone for Deep Forge's calculated restraint.

**Pillar anchor**: P2 (hue as meaning), P5 (schematic tree language).

---

#### 9.1.3 Path of Exile (Grinding Gear Games, 2013–present)

**Medium**: 2D isometric action RPG, dark fantasy.

**What we borrow — weight through palette restriction and high-contrast impact.** Path of Exile's visual approach is built on near-black environments where saturated ability effects detonate. Its passive skill tree — the Passive Skill Web — is the direct structural ancestor of Deep Forge's tree: thousands of nodes organized on a schematic constellation map, with dense linework on a dark background. The player learns to read spatial clusters. Node unlocks feel like switching on something in a machine.

The game's impact feedback philosophy is also directly relevant: hits in Path of Exile are felt before they are read. The contact moment dominates; ambient environmental effects stay dark and quiet. This is exactly the P4 direction in Section 1 (Impact Owns Saturation).

**What we reject — gothic narrative visual weight and 3D perspective.** Path of Exile uses gothic architecture, demonic iconography, 3D ground-plane perspective, elaborate environmental storytelling through ruins and corpses, and explicit lore embedded in environmental props. Deep Forge has none of these: there is no narrative (AP5), no 3D perspective, no ruins or corpses, and the environment is industrial-minimal rather than gothic-elaborate. Artists referencing Path of Exile may not import grimness as decoration, may not add environmental narrative detail, and may not confuse "dark and serious" with "gothic and symbolic." Deep Forge is dark because the forge interior is dark, not because darkness signals doom.

**Pillar anchor**: P4 (impact contrast), P5 (spatial tree-as-document).

---

#### 9.1.4 Industrial CNC Machining Photography (documentary / still photography)

**Medium**: Industrial still photography and documentary footage of machine shops, forge interiors, and CNC fabrication environments.

**What we borrow — the specific quality of manufactured geometry at rest.** A CNC-milled metal component photographed on a shop floor has a specific visual character: flat machined faces catching single-point light sources, scored reference lines from tooling passes, chamfered edges that are marks of intent rather than aesthetics, and surrounding negative space that is functional (clearance for tooling access), not decorative. This is the exact character of the hero register in Section 3: boomerang and ship read as manufactured objects with machining history, not as illustrations of weapons.

The shop-floor light quality is also directly relevant: a single overhead or below-level amber work light, near-black surrounding shadow, and the brightest object being the most recently worked surface. This is the in-run lighting model from Section 2.1 (global Light2D warm amber at 0.08–0.12 intensity, tight-radius cyan Point2D on the boomerang).

**What we reject — textures and surface noise.** Industrial photography shows genuine surface roughness: micro-scratches, oxidation patterns, lubricant residue, material grain. Deep Forge's beauty-is-cheap principle (Section 1) and its flat-per-face-color rule (Section 5.5.1) prohibit texture simulation. The lesson from industrial photography is **geometry and light**, not surface. Any attempt to add metallic texture, surface grain, or oxidation patterns to game sprites is importing the surface-noise of the reference, not its lesson.

**Pillar anchor**: P4 (Weighty Everything — weight through geometry), Section 3 (hero register chamfer/score marks).

---

#### 9.1.5 Bret Victor — "Inventing on Principle" Technical Diagrams and Schematic UI Illustrations

**Medium**: Technical illustration / academic presentation slides / circuit and systems diagrams, historical engineering manual tradition.

**What we borrow — the visual grammar of a document that is a map of decisions.** Engineering schematics — circuit diagrams, wiring diagrams, architectural section drawings, WWII-era radio equipment documentation — have a visual grammar that is load-bearing: every mark has a specific meaning, marks are arranged in spatial relationships that encode logical relationships, and the diagram is read both locally (one node) and globally (one cluster's relationship to another). The Deep Forge skill tree is specifically this kind of document. Blueprint Gold linework on Schematic Parchment, ruled connections, cluster boundary marks — all derive from this schematic tradition, not from game UI conventions.

The specific reference: engineering drawings use very limited hue (typically one color: draft blue or sepia or ink black on a light ground), high-contrast linework, and systematic visual hierarchy. Annotation is sparse and placed precisely. Whitespace is structured, not filled. The skill tree's use of Schematic Parchment as the ground and Blueprint Gold as linework is this tradition translated.

**What we reject — decorative illustration.** Technical diagrams are not illustrations and are not decorative. They carry no flourish, no shadow, no gradient, no ambient decoration added for beauty. The parchment surface in Section 6.6.2 carries corner anchor marks and forge-grime only — sparse, functional, historical — because the schematic tradition prohibits decorative background fills. Any artist who looks at "parchment map" imagery for the skill tree background and concludes "add decorative illustration or cartouche motifs" has imported the wrong reference genre. The parchment is a working document, not a prop.

**Pillar anchor**: P5 (tree as beautiful artifact), Section 3.4 (UI shape grammar), Section 6.6 (skill tree environment).

---

#### 9.1.6 Minit (Jan Willem Nijman / Vlambeer, 2018)

**Medium**: 2D sprite game, monochrome palette with one-bit visual field.

**What we borrow — proof of concept that restricted palettes carry more narrative weight, not less.** Minit uses only two colors: black and white. Every silhouette, every object, every world-space character reads entirely through shape and motion. Minit demonstrates empirically that palette restriction increases visual clarity rather than reducing it. It is the purest available demonstration of the Gestalt law of figure-ground: when palette is fixed, form must do all the work, and form becomes exceptionally powerful as a result.

This is the underlying theory behind Section 4's hue-reservation system: Weapon Cyan is more powerful as the hero color because nothing else is that hue, not because it is saturated. Minit proves that richness of perception does not require richness of palette.

**What we reject — the monochrome approach itself.** Deep Forge has a specific palette with specific hue meanings. Minit's aesthetic is not the target — its lesson is. An artist looking at Minit as a reference must extract the silhouette-primacy lesson and apply it to Deep Forge's chamfered-angular vocabulary. Monochrome palette, extreme pixel-art minimalism, and sprite simplicity far below Deep Forge's authored complexity are all characteristics that belong to Minit's identity, not to Deep Forge's.

**Pillar anchor**: P2 (silhouette as first discriminator), Section 1 (hue reserved for meaning).

---

#### 9.1.7 Aliens (James Cameron, 1986) — Production Design

**Medium**: Practical production design, film.

**What we borrow — the visual language of equipment people work with, not equipment that performs.** The colonial marine equipment in Aliens — the Pulse Rifle, the dropship interior, the APC instrument panel — reads as functional, scored with use, asymmetric in arrangement, and constructed from mismatched components. None of it looks designed for aesthetic appeal. All of it looks assembled for a specific job. This is the exact character of Deep Forge's ship and boomerang: worked tools, assembled for function, carrying visible history of that function.

Specifically: the scored panel seams on the ship (Section 5.1.1), the mismatched plate break, the thruster heat-score marks — these are direct descendants of Aliens' production design principle. Equipment has lived in before it is heroic.

**What we reject — grit-for-atmosphere texture work.** Aliens achieves its aesthetic through practical sets and practical patina. Deep Forge does not use surface texture at all (Section 5.5.1, flat-per-face color ceiling). The practical-prop quality must be approximated through geometry-based marks (scored lines, chamfered corners, asymmetric rivet clusters) alone. An artist who references Aliens and concludes "add dirt texture overlays and burn-shadow gradients" has imported the production-design medium's toolset rather than its visual logic.

**Pillar anchor**: P4 (Weighty Everything via visible material history), Section 5.1.1 (ship detail marks).

---

### 9.2 Secondary References (Technique Borrowing)

These references address specific technical problems. They are narrower than the primary set — each covers one technique and nothing more. Importing context beyond the stated technique is incorrect use of these references.

---

#### 9.2.1 Hit-Flash Timing — Spelunky (Mossmouth, 2012)

**Technique borrowed**: The single-frame white-flash on contact as the primary legibility signal for a hit registering. Spelunky's impact flash at 60 Hz is approximately one frame of maximum saturation followed by a rapid falloff. This is the specific timing model in Section 4.6: Impact White at frame 0, lerp to Impact Gold by frame 3, complete by frame 4. The flash duration is exactly short enough to read as instantaneous (a single event) rather than a held glow.

**What makes this reference tight**: Spelunky's design context — a physics-driven platformer — forced impact clarity to be solved through single-frame signals because the game cannot pause mid-physics for a protracted hit animation. Deep Forge has the same constraint: boomerang travel does not stop for impact reads.

**Import limit**: Spelunky hit-flash timing only. Not Spelunky's platformer silhouettes, not its treasure glow system, not its environmental destructibility aesthetic.

---

#### 9.2.2 Silhouette Discipline — Hades (Supergiant Games, 2020)

**Technique borrowed**: How to maintain clear silhouette readability for enemies that are visually elaborate. Hades' enemies are highly detailed at their native scale but each reads as a distinct angular silhouette against the field. The technique is specific: the outline of each archetype is designed before the interior, and no interior detail creates a false outer-contour read. The silhouette is the contract.

This is exactly Section 3.1's 32×32 test applied at production scale. The lesson from Hades is that silhouette legibility does not constrain interior detail — it constrains the *boundary* of the object. Interior detail is only legible when the silhouette contract is satisfied first.

**Import limit**: Silhouette-first design discipline only. Not Hades' color system (full polychromatic, extremely high saturation, entirely incompatible with Deep Forge), not its character designs (mythological narrative, organic forms, curves everywhere), not its environmental design (elaborate architecture, far beyond Deep Forge scope).

---

#### 9.2.3 Animation Economy — Nuclear Throne (Vlambeer, 2015)

**Technique borrowed**: Maximum character and impact expressiveness from a minimum frame budget. Nuclear Throne produces viscerally weighty impacts and personality-heavy enemy behavior from sprite-sheet animations of 3–6 frames per action. The squash-stretch snap of a throw, the rotation wobble of a projectile, the separation of death debris — all achieved with very few frames and very clear authored posing on each keyframe.

This is the technical model for every Section 5 animation spec: boomerang throw (4 frames), destruction sequences (3–6 frames), boss phase transition (3-frame spark burst). Nuclear Throne proves that 8 frames of well-authored animation outweighs 24 frames of average animation in every game-feel metric.

**Import limit**: Frame economy and keyframe discipline only. Not Nuclear Throne's weapon variety aesthetic (extreme variety, Deep Forge has one weapon type), not its chaotic screen density, not its screen-shake-heavy camera direction (prohibited in Deep Forge per Section 6.5.1).

---

#### 9.2.4 UI Chrome — Deus Ex: Human Revolution (Eidos Montreal, 2011)

**Technique borrowed**: The specific visual grammar of a UI that reads as machinery rather than interface. Human Revolution's UI uses amber/gold angular chrome, chamfered panel edges, schematic linework separating data fields, and a deliberate restriction of curve in interactive elements. This is the closest existing game-UI reference to Deep Forge's HUD: chamfered rectangles, Blueprint Gold linework, instrumental readout typography. The schematic-engineering quality of the HUD panel in Section 7.2 — instrument readout, not consumer product — has this reference as its direct visual ancestor.

**Import limit**: Panel chamfer geometry and instrumental-aesthetic typography treatment only. Not Deus Ex's amber lens-flare glow passes (prohibited: no post-processing), not its animated human interface glitch effects (no animation budget for this), not its polished-metallic surface sheen (no PBR in Deep Forge).

---

#### 9.2.5 Schematic Typography — Historical Military Technical Manuals (WWII-era, US/UK government publication)

**Technique borrowed**: How text is placed within schematics as annotation, not headline. WWII technical manuals for aircraft instruments, radio equipment, and ordinance handling use a very specific typographic register: condensed near-uniform-weight sans-serif, all-caps for labels, sparse line weights, and text placed to serve the diagram rather than draw attention. This is the precise typographic personality in Section 7.3: Rajdhani's condensed proportions, uniform stroke weight, near-squared terminals. The font reads as annotation, not branding.

**Import limit**: Typographic register (weight, proportion, case) only. Not the mimeograph or offset-print texture of the historical documents themselves, not the specific historical context, not WWII military iconography of any kind.

---

### 9.3 Anti-References (What We Are Not)

Anti-references are as important as positive references. They exist to prevent drift that is seductive precisely because the rejected aesthetic is adjacent to Deep Forge in some dimension (sci-fi, dark, incremental, minimal). Each rejection is specific.

---

#### 9.3.1 Cyberpunk Neon Aesthetic — Blade Runner, Cyberpunk 2077, and derivatives

**Why we reject this**: The cyberpunk neon aesthetic is defined by ambient saturation — glowing hues distributed across the environment as decoration, not meaning. In Deep Forge, Weapon Cyan is the only cyan in the game, and it is cyan because it is the player's weapon and for no other reason. A cyberpunk neon reference would produce: non-cyan neon accents on enemies, saturated ambient environment decoration, glowing architectural elements as atmosphere. Every one of these directly violates Section 1's "Hue Reserved for Meaning" and Section 2.1's P3 protection rule (no cyan except the boomerang, ever).

Additionally, cyberpunk's visual identity is built on excess: more neon, more grime, more chrome, more advertisements. Deep Forge's visual discipline is built on scarcity. The principle from Section 1 — "Impact Owns Saturation" — is structurally incompatible with a visual environment where saturation is used atmospherically.

**The specific test**: If a new asset, effect, or environmental element would look at home in a cyberpunk game, it almost certainly violates Deep Forge's hue-reservation and saturation-budget rules. Reject it.

---

#### 9.3.2 Vampire Survivors — Visual Anarchy and Density-as-Spectacle

**Why we reject this**: Vampire Survivors is built on deliberate visual chaos: hundreds of simultaneous enemies, projectiles filling the screen, particle effects overlapping to produce intentional visual noise. The aesthetic reward of Vampire Survivors is saturation-overload — the screen becomes an overwhelming wall of color and motion. The player experience is deliberately one of loss-of-control.

Deep Forge's visual discipline prioritizes clarity through restraint: distinct hues for spatial decision-making, impact saturation reserved for contact, restrained particle effects. P2 (Positional Mastery) governs hue assignment and enemy type distinction.

Vampire Survivors is not a bad game. It is the correct answer to a different design question. The question Deep Forge answers is the opposite.


---

#### 9.3.3 Astro Prospector — Our Direct Genre Neighbor

**Why we reject this (or rather, how we differentiate from it)**: Astro Prospector is the closest genre reference to Deep Forge: top-down 2D space, sprite-based, mining asteroids with combat. The visual differentiation must be specific and defensible, not vague.

Astro Prospector's visual identity is defined by: a relatively conventional sci-fi color palette (blues, greens, ship-grey with saturated pickup colors), generic HUD chrome (rounded panels, familiar iconographic language), and an environmental aesthetic that is legible but not authored — asteroids look like game asteroids, not like mineral matter with a specific geological character.

Deep Forge differentiates on three specific axes:

**Axis 1 — Palette restriction and hue ownership.** Deep Forge has five named hues, each owned by a gameplay role. Astro Prospector uses a conventional multi-hue sci-fi palette without semantic restriction. A player who picks up Deep Forge after Astro Prospector will immediately feel that cyan means something specific in a way it does not in Astro Prospector.

**Axis 2 — Shape language as identity statement.** Deep Forge's angular-chamfered construction and the hero-vs-supporting register distinction (Section 3.2) produce a visual field that reads as a craftsman's environment, not a generic space. Astro Prospector's field reads as a space game. Deep Forge's field reads as a forge.

**Axis 3 — The skill tree as foreground visual artifact.** Deep Forge's skill tree is its primary visual product. The schematic parchment, Blueprint Gold linework, and draw-on reveal are designed to be a beautiful object the player chooses to look at. Astro Prospector's upgrade system is a game-UI upgrade screen. These are fundamentally different.

**The specific test**: If a visual element reads as "space game HUD" rather than "schematic from a forge," it is drifting toward Astro Prospector's register. The corrective question is always: does this element look like it belongs in a machine shop's blueprint documents, or in a sci-fi film's spaceship cockpit?

---

#### 9.3.4 Pastel Cozy-Incremental Aesthetics — Stardew Valley, A-Train All Aboard Tourism, Spiritfarer, and derivatives

**Why we reject this**: The cozy-incremental genre has developed a visual shorthand that is antithetical to Deep Forge at every level: soft rounded forms, desaturated warm pastels, gentle ambient animation (flowers swaying, particles drifting), and an explicit design intention of producing low-stress visual comfort. Every visual choice is aimed at safety and softness.

Deep Forge's pillar P4 (Weighty Everything) requires visual heaviness. Section 3.7's no-curves-on-primary-contours rule is a direct structural rejection of the rounded-form language that defines cozy aesthetics. The forge interior is not safe or soft. The asteroid field is not a meadow.

The specific risk: Deep Forge is an incremental game, and cozy-incremental aesthetics are visually dominant in the incremental genre. A commissioned artist who works primarily in mobile or idle game contexts will arrive with this aesthetic as their default. The art director's first brief-review task is to verify the artist's default is not cozy-incremental before any production work begins.

**The specific test**: Does the submitted concept have rounded corners anywhere on a gameplay object? Does any element have a pastel desaturated tone that reads as "soft"? These are immediate drift signals.

---

#### 9.3.5 Realistic Space Sim Fidelity — Elite Dangerous, Kerbal Space Program, and derivatives

**Why we reject this**: Realistic space simulators use a visual vocabulary of photorealistic planetary bodies, accurate star fields, specular-lit spacecraft surfaces, and environmental scale-fidelity. Every element of this approach is antithetical to Deep Forge.

Stars: permanently rejected (Section 6.1). Specular-lit surfaces: no PBR, no specular, flat-per-face color (Section 6.7.3). Environmental scale-fidelity: the field is a bounded arena, not a spatial environment (Section 6.2.1). Photorealism: explicitly violated by the flat-color, chamfered-geometry aesthetic.

Additionally, realistic space sims communicate through informational density — large numbers, orbital mechanics data, distance readouts. Deep Forge communicates through visual hue, shape, and motion with minimal text. The informational register of realistic space sims would produce a HUD that is the opposite of Section 7.2's sparsity principle.

**The specific test**: If a reference image requires a space sim's rendering pipeline to achieve its look, it is incompatible with Deep Forge's WebGL sprite-and-Light2D constraint.

---

#### 9.3.6 Neon Particle-Heavy Indie Aesthetics — Rez, Geometry Wars, Tempest

**Why we reject this**: These games use light and particle density as the primary aesthetic instrument: screens filled with additive-blend glow trails, particle explosions measured in thousands, and backgrounds that are themselves animated luminous fields. The experience is explicitly about overwhelm-through-light.

Deep Forge's particle cap is 500 total simultaneous (Section 8.4). There is no post-process bloom, no glow pass, no volumetric light (Sections 1, 6.7.3). The boomerang is cyan against near-black specifically because the field is dark and sparse — not because the field itself is a light show.

The risk is subtle: P4 (Weighty Everything) creates a temptation to add "more impact VFX" when something feels unweighty. The correct response to insufficient weight is always timing, shape, and hitstop (Section 1's weight-cost paradox resolution) — never adding particles or glow passes.

**The specific test**: If "the proposed VFX looks good in isolation but requires bloom to look good in the game," it is borrowing from this aesthetic and must be reformulated.

---

#### 9.3.7 Diablo-Style Gothic ARPG Visual Density — Diablo IV, Grim Dawn

**Why we reject this**: Gothic ARPGs use environmental visual density as atmosphere: elaborate architecture, lore-embedded environment storytelling, elaborate material variation across ground tiles, wall tiles, and architectural props. The "world" communicates its history through density of environmental detail.

Deep Forge explicitly prohibits environmental narrative (AP5, Section 6.4.4). The environment communicates exactly two things: field density (how much to mine, how much pressure to expect) and zone progression (Zone 1 = warmer, Zone 2 = cooler). Background scoring marks at 15–20% opacity (Section 6.1.1) are the ceiling on environmental detail.

Additionally, gothic ARPG skill trees are visually dense with icon variety, stat text, and lore tooltip depth. Deep Forge's tree is a schematic where node identity comes from the tree's spatial structure and the player's build path, not from elaborate per-node illustration or stat text density.

**The specific test**: Any proposed background element, environmental prop, or tree-node icon that requires reading at rest (versus reading from motion and shape) is importing the narrative-density register of gothic ARPG aesthetics. Deep Forge's environment is read, not studied.

---

### 9.4 Mood Board Anchor Images (Specification)

This section does not provide external links or specific image names. It specifies the categories of image that the art director must collect when building the physical mood board. Mood board images are reference material for the art director's own judgment, not for distribution to contractors. Contractors receive the art bible, not raw mood board imagery.

The mood board is organized by game state. Each category lists the specific image types to source. "Source" means: curate from industrial photography, film frame-grabs, technical illustration archives, and game screenshots.

---

#### 9.4.1 In-Run Combat

Images to collect in this category:

1. **CNC machine shop at night lit by single amber work-light**, viewed from above or oblique angle: shot from above a machine floor where one amber overhead lamp illuminates the nearest machine surface. Surrounding floor is near-black. The single-light read demonstrates exactly the in-run ambient model (Section 2.1: global amber at 0.08–0.12 intensity).

2. **Industrial press or punch operation caught at moment of contact**, black-and-white or desaturated: the frame-capture of a metal press striking a material surface — deformation, contact flash, ejected spray. This is the visual mental model for Impact White / Impact Gold at contact — the moment is the loudest frame, everything before and after is quieter.

3. **Scrap yard viewed at night with single arc-light source**, medium shot: irregular angular metal forms against near-black background, lit by one harsh directional source. The spatial read of angular silhouettes against void is the field composition target.

4. **Angular mineral specimens on black velvet** (photography): specimen photography places crystalline or fractured mineral matter against pure black to show facet geometry. The faceting, flat planes, and irregular edge angles of mineral matter are the source material for asteroid silhouette design.

5. **Boomerang flight photography**, high-speed or blurred: whatever is available showing a throwing arc. The visual question is: what does the afterimage of a rotating tool in flight look like? The trail length, rotation blur, and silhouette clarity inform the boomerang trail design.

---

#### 9.4.2 Skill Tree

Images to collect in this category:

1. **WWII US Army or RAF technical manual page**: circuit diagram or aircraft instrument wiring schematic on aged paper, blue-line or sepia-ink. The specific visual: ruled lines, annotation in all-caps condensed type, no decorative border, linework carries all the structure. Source from public domain US government archives.

2. **Physical circuit board photographed on parchment or aged paper**: the juxtaposition of angular metallic traces (Blueprint Gold in spirit) against warm paper ground — the exact color relationship target for tree linework on parchment.

3. **Candlelit cartography**, medieval or Renaissance map on parchment photographed by candle-light: not to import the cartographic style but to capture the quality of a flat document lit by a below-level warm point source. The illumination model for Section 2.4's "parchment panel lit from below."

4. **Japanese electrical wiring diagrams or exploded-view engineering drawings from post-war industrial manuals**: specifically the spare black-ink ruled-line aesthetic on aged paper, angular component symbols connected by grid-aligned lines. These are the closest visual reference to the skill tree's schematic language: system logic expressed as spatial document.

5. **Hex grid handmade game-design prototype (physical board game prototype)**: a hand-drawn or printed hex grid on graph paper with component placements — the visual quality of a designed system whose spatial structure IS its meaning. Not a finished game; a working document.

---

#### 9.4.3 Boss Encounter

Images to collect in this category:

1. **Mining excavator or dragline bucket-wheel photographed from below at close range**: the scale relationship of industrial machinery that makes a human-scale object feel inconsequential in the frame. The visual read of something that is too large for the space — Section 2.2's "something too large for the room."

2. **Forge press or industrial stamping machine mid-operation**, high-contrast photograph: massive mechanical mass in motion. The visual quality of a machine that cannot be stopped — ponderous and committed. Crimson-adjacent industrial light, near-black surroundings.

3. **Fractured ship hull or industrial accident aftermath**, desaturated: structural mass that has partially broken but is still structurally coherent. This is the reference for boss phase transitions (Section 5.3.1): a machine that has broken in one place and is compensating. The part is damaged; the machine is still operating.

4. **Red welding arc photographed in dark workshop**, long exposure or high-speed: the specific quality of Pirate Crimson light contaminating surrounding surfaces. The boss's crimson point light wash (Section 2.2) — a reddish-orange light source that tints nearby surfaces with threat-coded color.

5. **Multiple industrial machines operating simultaneously in a dark factory**, viewed from elevated angle: the compositional feel of multiple simultaneous kinetic threats in a bounded space — the dual-axis awareness demanded by boss encounters (Section 5.3, P2).

---

#### 9.4.4 Death Screen

Images to collect in this category:

1. **Forge at rest after workday**, interior photograph: cooling embers, surfaces warmed by orange glow, tools laid down, no active motion. The specific quality of amber warmth in a space that has been working and is now still. This is Section 2.3's "amber wash as the value you collected."

2. **Industrial equipment cooling at shift-end**: a machine press or furnace cooling, with visible residual heat glow. The transition from active (cyan-blue arc-flash) to cooling (amber glow) is the visual model for the death transition's ambient shift.

3. **Workshop interior at the moment all lights are turned off except one amber emergency light**: the minimal-ambient condition of Section 2.3 (global ambient drops to 0.05–0.08). Almost-dark, only the essential warmth remaining.

4. **Foundry ingot just pulled from mold**: still glowing amber-orange, cooling at edges. The ambient light it casts on the surrounding surface is the visual model for the ore-amber wash that appears as gameplay lights fade in the death transition.

---

#### 9.4.5 Main Menu

Images to collect in this category:

1. **Single hand tool on a work bench, lit by a single lamp, nothing else in frame**: a hammer, a wrench, a single machined component — the specific weight of an object at rest that is about to be used. The menu boomerang idle in a dark field is this image. The tool is the game.

2. **Clock or instrument panel photographed at close range before a precise operation**: the visual quality of readiness — everything set, nothing moving, moment held. The "held breath" described in Section 2.5.

3. **Lone industrial silhouette against dark horizon**: an angular industrial form (not a ship, not a building — a tool or machine component) partially visible against near-black, lit from a single source. Scale and implied resistance without narrative context.

4. **Technical manual title page**: a spare, ruled-line title page with minimal typography — the visual model for the main menu's Blueprint Gold schematic title treatment over near-black.

---

### 9.5 Zone-by-Zone Visual Reference Register

The palette shifts between Zone 1 and Zone 2 are locked in Section 4.3. This section governs the reference register shift: how the visual inspiration for the zone changes, independent of palette.

---

#### 9.5.1 Zone 1 Reference Register — Outer Workshop

Zone 1 is the active working surface of the forge. The reference register is: **functional working industrial space at operating temperature.** The forge is in use. The material being worked is accessible. The light comes from below, warm, consistent with an active heat source.

The artistic tone words for Zone 1 reference collection: functional, pressured, accessible, amber-lit, fresh-seam.

Zone 1 asteroid reference: recently fractured mineral specimens — fresh fracture faces, light grey-warm, crisp edges. The field has not been worked before.

Zone 1 enemy reference: improvised machines assembled from salvage. The direct reference is military field-modification: weapons modified with field materials because the original broke. Functional, irregular, not purpose-built.

Zone 1 environmental reference: outer workshop floor, active operations, moderate density of tools and materials. The space is organized but not sparse — there is work happening here.

---

#### 9.5.2 Zone 2 Reference Register — Back Room, Deeper Strata

Zone 2 is deeper into the same territory. The reference register shifts: **older, cooler, more compressed, more exhausted.** The ore that remains in Zone 2 is harder material from deeper strata — it has been under pressure longer. The pirates in Zone 2 are not improvised; they have had time to develop their equipment.

The artistic tone words for Zone 2 reference collection: consolidated, cooler, petrified, purpose-built (enemies), worked-over (environment).

**The specific reference shift for Zone 2 asteroid** inspiration: older geological specimens, not fresh fracture — mineral matter that has been under compression longer. The facets are the same angular geometry as Zone 1 but the color reference shifts toward cooler-grey mineral (basalt over sandstone, in geological metaphor). The Zone 2 Ore Stone shift from `#4A4640` to `#3D3C42` is exactly this: cooler grey, older feel.

**The specific reference shift for Zone 2 enemies**: from field-modified salvage (Zone 1) toward equipment with a history of being purpose-built for this environment. Zone 2 Pirate enemies (using Deepwater Crimson `#6B2030`) should reference more consolidated machinery — fewer random protrusions, more deliberate component arrangement — even though silhouettes are identical to Zone 1 per Section 3.5.4. The hue shift carries the story; the reference shift informs the detail-mark placement within the same silhouette.

**The specific reference shift for Zone 2 boss ("The Warden")**: where Zone 1's Claim Jumper references a repurposed mining machine, Zone 2's Warden references purpose-built industrial fortification — a lock, a sealed hatch, a structural barrier. The visual references shift from improvised assembly toward deliberate structural engineering. Same angular vocabulary; different intentionality.

**What does NOT shift in reference between zones**: the boomerang's reference is zone-invariant (Section 4.3 zone-invariance rule). The tool is the player's construction; it exists outside zone theming entirely. The ship's reference does not shift either — the ship is the player's presence, and the player's presence is constant. Only the environment and the enemies shift reference register.

---

### 9.6 Reference Anti-Patterns

These are the specific failures that occur when borrowing from the primary references is done carelessly. Each names the reference being borrowed, the specific wrong lesson that can be drawn from it, and the correct lesson.

---

#### 9.6.1 Borrowing Dead Cells' Backlit Silhouettes Without Its Color Discipline

**What can go wrong**: Dead Cells uses extremely saturated background environmental lighting that makes enemies read as dark silhouettes against bright surfaces. The silhouette-against-bright-background is a legitimate technique, but Dead Cells earns it by controlling the background saturation precisely in each zone.

**The wrong lesson**: "Enemies should be dark silhouettes against bright backgrounds." Deep Forge's enemies are dark crimson sprites against Forge Black (a near-black background). The silhouette reads because of hue contrast (crimson on near-black), not because of value contrast (dark on bright). If an artist imports the "dark silhouette on bright background" approach from Dead Cells, the result will be a bright or saturated background that violates Section 6.1's pure-Forge-Black rule and competes directly with the boomerang's cyan light.

**The correct lesson**: Silhouettes read through consistent edge-definition and shape uniqueness. The specific lighting arrangement of Dead Cells is not the lesson.

---

#### 9.6.2 Borrowing Into the Breach's Grid Precision Without Its Warmth

**What can go wrong**: Into the Breach's UI is warm-neutral in tone, despite being spatially precise. The warmth comes from subtle yellow-tinted neutral greys that soften the hard grid. If an artist imports the grid-precision alone and ignores the warmth, the result is a cold blue-grey grid — which pushes the skill tree toward a clinical sci-fi register rather than a forge-schematic register.

**The correct lesson**: The spatial precision of a schematic grid carries emotional weight when the grid's color is warm (Schematic Parchment `#C8B87A`, Blueprint Gold `#B8962E`). The warmth of the skill tree's document substrate is not incidental — it is what makes the schematic read as a working document rather than a corporate flowchart.

---

#### 9.6.3 Borrowing Path of Exile's Visual Darkness Without Its Contrast Reserve

**What can go wrong**: Path of Exile's environments are very dark, but the game reserves extreme contrast for its ability effects — so much so that the contrast reserve is effectively unlimited (the engine supports bloom, volumetric effects, and extreme particle density). Deep Forge does not have this contrast reserve: no bloom, no post-process, particle cap at 500.

An artist who looks at Path of Exile and concludes "everything should be very dark, including the field details" will produce a Deep Forge environment where the boomerang's cyan Light2D fails to provide readable contrast because the ambient-to-highlight ratio has been compressed incorrectly. Deep Forge's darkness is calibrated precisely at Section 2.1's 0.08–0.12 global ambient — not darker, not lighter. The contrast reserve is built into the sprite-layer structure, not the rendering pipeline.

**The correct lesson**: Near-black environments require precise calibration of how dark, not simply "as dark as possible."

---

#### 9.6.4 Borrowing Aliens' Production Design Grit Without Its Texture Budget

**What can go wrong**: The Aliens production design achieves its lived-in quality through surface texture: scuff marks, grime patterns, actual dirt, material weathering that varies continuously across surfaces. These are physical properties of practical props.

Deep Forge cannot and must not simulate this through texture maps or procedural surface detail. The lived-in quality must be approximated through geometry-based marks: the five ship detail marks in Section 5.1.1, the edge-accent value on asteroid faces, the corner grime marks on the parchment. An artist who references Aliens and adds surface texture overlays to game-field sprites is violating Section 5.5.1's flat-per-face ceiling and adding WebGL build weight without visual authority.

**The correct lesson**: Lived-in quality comes from mark placement and geometry, not surface texture.

---

#### 9.6.5 Borrowing Hades' Impact Polish Without Its Post-Process Stack

**What can go wrong**: Hades achieves premium impact feedback through a combination of hitstop, screen-space effects, high-density particles, and a post-process bloom pass that makes the impact "breathe" outward into surrounding air. The bloom pass is a significant contributor to the perceived weight of each hit.

Deep Forge has no bloom pass (Section 1, Beauty Must Be Cheap; Section 8.5, no post-processing stack). An artist or technical artist who references Hades' impact feel and concludes "we need bloom on the impact flash" is importing the wrong mechanism. The correct mechanism — hitstop timing, shape of debris silhouettes, Impact White-to-Impact Gold lerp timing — is specified in Section 4.6 and Section 1's weight-cost paradox resolution.

**The correct lesson**: Perceived impact weight from the Hades reference is attributable to timing and shape as much as glow. Extract the timing lessons; discard the glow mechanism.

---

#### 9.6.6 Borrowing Deus Ex: Human Revolution's HUD Amber Without Its Saturation Level

**What can go wrong**: Human Revolution's amber UI chrome uses a quite saturated amber/orange throughout, and the game's pipeline supports bloom that makes the amber glow. The high saturation is part of its visual identity.

Deep Forge's Ore Amber `#C07830` in the HUD is restrained: the currency counter icon is at 60% opacity at rest (rising to 100% on pickup). Blueprint Gold `#B8962E` is the structural UI hue. If an artist references Human Revolution and pushes the HUD amber toward that game's saturation level, the result violates Section 4.7's saturation budget (no non-contact-event asset may approach the impact event saturation ceiling) and competes with Ore Amber pickups in the field — a direct P2 violation.

**The correct lesson**: The instrumental-aesthetic panel register is the lesson from Human Revolution. The saturation level is not.

---

#### 9.6.7 Borrowing Nuclear Throne's Animation Economy as License for Low-Quality Keyframes

**What can go wrong**: Nuclear Throne gets away with 3–6 frame animations because each frame is authored with extreme pose clarity — the silhouette at each keyframe is a maximally distinct, readable shape. The economy works because the craft within each frame is very high. An artist who takes "fewer frames is fine" from this reference and uses it as permission to produce low-effort keyframes will produce animations that feel unweighty rather than economical.

**The correct lesson**: Animation economy means maximum expressiveness per frame, not minimum effort per animation. Each frame of the boomerang throw (Section 5.4.1) must carry its full squash-stretch pose clearly. The 4-frame throw is 4 exceptional frames, not 4 adequate frames.

---

### 9.7 Competitor Differentiation Map

| Dimension | Astro Prospector (direct competitor) | Vampire Survivors | Brotato | Halls of Torment | Diablo-Style ARPG Skill Trees |
|---|---|---|---|---|---|
| **Similar visual element** | Top-down 2D space, asteroid sprites, mining loop, similar field scale | Incremental enemy density escalation; resource-loop feedback | Auto-attack incremental loop; per-run upgrade selection | Dark field, enemy density, impact feedback | Passive skill tree as primary progression system; node-unlock satisfaction |
| **We are deliberately different** | Palette is semantic (5 hues, each owned by a role). Field has forge-industrial aesthetic, not generic sci-fi. Tree is schematic-parchment document, not UI panel. | Field is readable at all times. Boomerang arc is always the primary visual. Saturation is reserved for contact events only. Particle cap enforced. | Weapon is single and authored (boomerang only). Boomerang's visual identity drives all progression expression. No weapon variety as aesthetic escape valve. | No gothic architecture, no narrative decay aesthetics, no skull/bone visual vocabulary. Deep Forge is a craftsman's workshop, not a dungeon. | Tree is a spatial document with linework schematic identity. Nodes read by spatial position as much as icon. The tree is beautiful as an object, not merely functional as a screen. |
| **Our dominant visual signature that none of them have** | The Weapon Cyan + chamfered-geometry + schematic-parchment combination: a single tool whose hue is the exclusive brightest object in the field, constructed by a hero-register shape language, and whose growth is documented on a physical schematic the player reads as their own blueprint. No competitor uses hue-as-exclusive-role at this level of discipline, no competitor has the forge-schematic tree visual, and no competitor has the constructed-vs-found shape register that makes the boomerang read as the player's authored object in a field of found material. | Same as above | Same as above | Same as above | Same as above |

---

### 9.8 Style Guide Enforcement Process

This section specifies how Section 9 — and by extension, the entire art bible — is enforced when contracted artists or future team members produce assets for Deep Forge. The process is written for the art director's use when commissioning work.

---

#### 9.8.1 The Contractor Reference Packet

Every contracted artist receives the following materials before beginning any work:

1. **Art Bible Sections 1, 3, 4, and the relevant Section 9 subsections for their domain.** These four sections constitute the minimum briefing. Section 1 (Visual Identity Statement) is mandatory for all contractors without exception — it is the single-page source of truth from which every other rule derives. Section 3 (Shape Language) is mandatory for any sprite work. Section 4 (Color System) is mandatory for any sprite or UI work. Relevant Section 9 subsections: 9.1 (Primary References), 9.3 (Anti-References), and the game-state-relevant 9.4 subsection for their target domain.

2. **The three color tests from Section 4.7.** Contractors must be able to apply Saturation Budget, Zone-Invariance, and Colorblind Collapse tests to their own submitted work before delivery. The art director runs the tests again on receipt — but a contractor who cannot pass their own work before submission is a delivery risk.

3. **The 32×32 silhouette test from Section 3.1.** Any sprite work must be submitted with a 32×32 composite of the final idle frame against black alongside the full-resolution deliverable. This is not optional — it is a required submission element.

4. **Section 9.1 reference names and what-we-borrow / what-we-reject summaries.** Contractors are not given the mood board images (those are the art director's working material). They receive the verbal description of what is borrowed and rejected from each anchor reference. This is sufficient for a skilled artist to locate the visual territory.

5. **Section 9.3 Anti-References**, in full. An artist who does not know what they must not produce is at risk of producing it.

Sections 2, 5, 6, 7, and 8 are supplementary. They are shared on request or when the contractor's domain specifically requires them (Section 8 for technical specs; Section 2 for any animator working on game-state transitions; Section 5 for character animation; Section 7 for any UI contractor).

---

#### 9.8.2 Reject Criteria — Visual Tells of Reference Drift

The following specific visual characteristics are immediate grounds for asset rejection without review discussion. Each maps to a documented rule.

| Visual Tell | Violated Rule | Rejection grounds |
|---|---|---|
| Any curve on a primary object contour | Section 3.7 | Hard rule, no exception |
| Cyan on any object other than the boomerang or boomerang-adjacent UI nodes | Section 4.1 (Weapon Cyan exclusive) | Hue-ownership violation |
| Ambient particle or background effect using cyan | Section 2.1 P3 protection | P3 violation |
| Background contains any non-black non-scoring-mark element | Section 6.1 | P3 + forge-identity violation |
| Any new color not in the Section 4.1 palette | Section 4.1 amendment rule | Pipeline blocking failure |
| Non-contact-event asset with saturation at or above lowest impact VFX frame | Section 4.7 Test 1 | Saturation budget violation |
| Game-field sprite with surface texture, gradient, or hatching | Section 5.5.1 flat-per-face ceiling | Beauty-Must-Be-Cheap violation |
| Rounded corner on any UI element | Section 3.4 no-organic-shape-in-UI | Shape grammar violation |
| Enemy or asteroid sprite with hex form or hex sub-element | Section 3.4 (hexagonal language is UI-exclusive) | False interactive signal |
| HUD element placed in the center or bottom-left quadrant | Section 7.2 governing principle | Obstructs critical game area |
| Any Pirate Crimson in a UI panel, button, or HUD element (not Boss HP bar) | Section 4.4 UI palette | False threat signal |
| Boomerang sprite with mod-indicator that changes base silhouette beyond ±15% | Section 3.5.1 | Base-silhouette consistency |
| Skill tree node that is not a regular hexagon | Section 3.5.6 | UI shape grammar violation |
| Zone-invariant asset (boomerang, ore pickup, UI) with any hue/saturation shift between zones | Section 4.3 zone-invariance rule | Semantic hue meaning collapsed |

Assets with any of these tells are returned to the contractor with the specific violated rule cited. The art director does not negotiate on these — they are absolute rules, not guidelines.

---

#### 9.8.3 Review Cadence

**Per-asset review** is required for all game-field sprites and VFX. Batch review is not permitted for game-field assets — a missed drift on an enemy sprite or VFX quad creates palette violations that compound as dependent assets reference the drifted asset.

**Per-batch review** is acceptable for UI chrome and schematic-parchment surface elements, provided all assets in a batch share a single function (all node states, all HUD panel frames). The batch is reviewed as a group against Section 4.4, Section 3.4, and the 32×32 test.

**Weekly review** is the cadence when a contractor is in active production. The art director reviews all submitted assets in the week's batch at the end of each week. Assets that pass are marked for pipeline entry. Assets that fail are returned with violation citations.

**First-submission review** is a separate mandatory gate for any new contractor before volume work begins. The contractor submits one complete asset (one enemy sprite, one tree node set, one VFX strip — whichever matches their domain) before any volume work. The art director reviews against the full reject-criteria list. If the first submission fails, a single revision cycle is permitted. If the revision fails, the contractor engagement ends. The first-submission gate exists because volume correction costs are substantially higher than early termination.

---

#### 9.8.4 Escalation Path for Ambiguous Reference Drift

Some submitted assets will be technically within the documented rules but feel wrong — they satisfy the letter of the rules but not the spirit. This is the hardest review case and requires an escalation path.

**Step 1 — Name the specific drift.** The art director must be able to name the specific principle from Section 1 (Hue Reserved for Meaning, Impact Owns Saturation, Beauty Must Be Cheap) or Section 3–6 that the asset violates. If the drift cannot be named in specific terms, it is not grounds for rejection — return to the mood board and confirm whether the asset is actually inconsistent or only unexpected.

**Step 2 — Apply the Section 1 design tests.** Each principle in Section 1 includes a "design test" in its definition. Apply the test verbatim: for a new VFX, ask "is this a contact event?" and then apply Test 1 from Section 4.7. If the asset fails a formal test, rejection is grounded.

**Step 3 — Reference the specific anchor image category.** If the asset is in a category covered by Section 9.4's mood board specification, compare against the collected reference images for that game state. If the asset reads as belonging to a different game state's reference category, that is specific grounds for revision guidance.

**Step 4 — Escalate to creative director only for pillar-level conflicts.** If the art director cannot resolve the ambiguity through steps 1–3, the question may represent a pillar-level tension (for example: P4 demanding more visual weight against P3 demanding more arc clearance). Pillar-level tension resolution is a creative-director decision, not a contractor note. The asset is held pending that decision; the contractor is not asked to revise without a clear direction.

---

### 9.9 Section 9 → Pillar Trace

| Section 9 Standard | P1 Multiplicative Dopamine | P2 Positional Mastery | P3 Read the Arc | P4 Weighty Everything | P5 The Tree IS the Game |
|---|---|---|---|---|---|
| **9.1.1** Dead Cells silhouette discipline | — | Primary anchor (32×32 field read) | Supporting (enemies readable without obscuring arc) | — | — |
| **9.1.2** Into the Breach semantic palette | — | Primary anchor (hue = role identity) | — | — | Supporting (spatial tree navigation) |
| **9.1.3** Path of Exile impact contrast + tree structure | — | — | — | Primary anchor (contact moment dominates field) | Primary anchor (schematic-spatial tree) |
| **9.1.4** CNC machining photography | Supporting (ship/boomerang show investment history) | — | — | Primary anchor (weight through manufactured geometry) | — |
| **9.1.5** Engineering schematic diagrams | — | — | — | — | Primary anchor (tree as spatial decision-document) |
| **9.1.6** Minit palette restriction proof | — | Supporting (silhouette over color) | Supporting (field clarity) | — | — |
| **9.1.7** Aliens production design | Supporting (ship evolution looks earned) | — | — | Primary anchor (lived-in tools feel used) | — |
| **9.2.1** Spelunky hit-flash timing | — | — | Supporting (arc uninterrupted by prolonged flash) | Primary anchor (contact moment timing) | — |
| **9.2.2** Hades silhouette discipline | — | Primary anchor (silhouette contract before interior) | — | — | — |
| **9.2.3** Nuclear Throne animation economy | — | — | Supporting (no wasted frame drawing eye) | Primary anchor (frame clarity = weight) | — |
| **9.2.4** Deus Ex HRI chrome | — | — | — | Supporting (instrumental UI reads as weighted) | Primary anchor (tree chrome aesthetic language) |
| **9.2.5** Military manual typography | — | — | — | — | Primary anchor (annotation-register type = schematic language) |
| **9.3.1** Cyberpunk neon rejected | — | Hue pollution rejected (protects color role meanings) | Primary rejection (ambient cyan impossible) | — | — |
| **9.3.2** Vampire Survivors rejected | — | Primary rejection (field must remain readable) | Primary rejection (arc must be primary visual) | — | — |
| **9.3.3** Astro Prospector differentiation | Supporting (tree as distinguishing P1 signal) | Supporting (semantic hue = cleaner field) | Supporting (forge register ≠ space-game register) | Supporting (shape register distinction) | Primary (tree as artifact is our dominant differentiator) |
| **9.3.4** Cozy-incremental rejected | — | — | — | Primary rejection (soft forms violate weight) | — |
| **9.3.5** Realistic space sim rejected | — | Supporting (bounded arena not simulated space) | Primary rejection (stars, parallax prohibited) | — | — |
| **9.3.6** Neon particle-aesthetic rejected | — | — | Primary rejection (additive-glow ambient visually competing) | Rejection protects (weight from timing, not glow) | — |
| **9.3.7** Gothic ARPG density rejected | — | Supporting (environmental read, not study) | Supporting (field readable, not storied) | — | Supporting (tree readable spatially, not by icon density) |
| **9.4 Mood board specification** | Supporting (Zone Victory category images carry it) | Supporting (in-run category calibrates field read) | Primary anchor (in-run lighting model grounded) | Primary anchor (boss category establishes scale) | Primary anchor (tree category establishes schematic tradition) |
| **9.5 Zone reference register** | — | Primary (zone escalation readable by register shift) | — | Supporting (Zone 2 material registers older/harder) | — |
| **9.6 Anti-patterns** | — | Supporting (prevents palette drift that collapses roles) | Supporting (saturation and particle discipline) | Supporting (glow-mechanism anti-pattern protects weight-from-timing discipline) | Supporting (warmth and schematic precision anti-patterns) |
| **9.7 Competitor map** | Supporting (evolution signature is our differentiator) | Supporting (semantic palette = cleaner field) | — | Supporting (single-weapon weight vs multi-weapon dilution) | Primary anchor (tree as artifact is the column's final answer) |
| **9.8 Enforcement process** | — | Reject criteria protect P2 at pipeline level | Supporting (cyan exclusion, saturation discipline blocking rejects) | Supporting (reject criteria enforce contact-saturation discipline) | Supporting (reject criteria enforce hex/connector/parchment rules) |

---

### 9.10 Reference Direction Production Notes (Handoff)

These are operational notes for the art director when building and maintaining the physical mood board. They are not creative direction — they are workflow specification.

---

#### 9.10.1 Image Hosting Tool

The mood board is maintained in a dedicated image-organization tool that supports folder hierarchy and is accessible offline (not browser-dependent). Miro, PureRef, or a local PureRef canvas are all acceptable. The specific constraint is: the mood board must be accessible during asset review sessions without requiring a live internet connection. Game-day reviews happen in Unity's asset window, not in a browser.

PureRef is the recommended tool on Windows for this project because it allows the canvas to be kept open alongside the Unity Editor without requiring a separate monitor or application switch. The canvas sits in the corner of the secondary monitor (or is alt-tabbed) during review.

The art director's mood board file is stored at `design/art/mood-board/` in the project repository. It is version-controlled. The canvas file (PureRef `.pur` or equivalent) is committed; the raw source images that cannot be redistributed are not committed, but their source descriptions (per Section 9.4) are maintained as a text file in the same directory so a replacement mood board can be reconstructed.

---

#### 9.10.2 File Organization

The mood board canvas organizes images into labeled zones matching the game-state categories in Section 9.4:

1. **In-Run Combat** (anchored top-left, always visible)
2. **Skill Tree** (anchored top-right)
3. **Boss Encounter** (anchored center-right)
4. **Death Screen** (anchored bottom-left)
5. **Main Menu** (anchored bottom-right)
6. **Anti-References** (anchored far-right, clearly labeled as DO NOT IMPORT — distinct background color from other zones)
7. **Zone 2 Reference Shift** (anchored beneath Zone 1 in-run images, explicitly labeled "Zone 2 only — not Zone 1")

Anti-references are given a visually distinct zone specifically so they are never accidentally treated as positive reference. The zone is labeled "DO NOT IMPORT THESE" in large text, not "anti-references." Clarity of the prohibition matters more than elegance of the label.

---

#### 9.10.3 Version Control for the Board

The mood board is versioned at each art-direction milestone: on initial population (now), after Zone 2 sprint begins (update the Zone 2 zone), and after each contractor engagement in which a new contractor's feedback reveals a gap in the reference set.

**Version markers**: canvas is exported as a flat JPG at each milestone, committed to `design/art/mood-board/versions/` with filename `mood-board-vN-YYYY-MM-DD.jpg`. The canonical canvas file is always `mood-board-current.pur` (or equivalent). The versioned JPGs are read-only snapshots — they are never edited.

**Drift tracking**: when an asset fails review due to a reference-not-covered drift, a note is added to `design/art/mood-board/drift-log.md` specifying: the asset, the nature of the drift, whether the drift reveals a gap in the reference set (meaning a new image category should be added) or a gap in the brief material given to the contractor (meaning Section 9.8.1 needs an update). Not every rejection is a reference board gap — most are failures of the first-submission gate process.

---

#### 9.10.4 Re-Review Cadence as Style Evolves

The mood board is a living document. Two specific triggers require a full re-review of all collected images against current art bible sections:

**Trigger 1 — Section 4 palette amendment.** Any formal amendment to the Section 4.1 palette (adding a new color per the amendment rule) requires reviewing all mood board images to confirm that no collected image represents a style that normalizes the new color's hue in ways inconsistent with its assigned role. A palette amendment changes what images are safe to collect.

**Trigger 2 — Zone 3 art sprint (post-MVP).** Zone 3 requires a third zone temperature step along the warm-to-cool axis (Section 4.3). When Zone 3 work begins, the mood board must add a Zone 3 zone with reference images for the new temperature register. Existing Zone 1 and Zone 2 zones are not revisited; the new zone is additive.

**Routine cadence**: outside the above triggers, the art director reviews the mood board at the start of each sprint to confirm it still represents the current design direction. As the game enters the polish phase, the reference set may narrow (removing images that were useful for early exploration but are now too broad relative to the locked visual direction). This narrowing is intentional — a mood board for a polish sprint is more specific than a mood board for a concept sprint.

The mood board is not public. It is a working document for the art director and relevant contracted artists during briefing sessions. It is shared screen-shared, not distributed as a file, when used in contractor briefings.

---
