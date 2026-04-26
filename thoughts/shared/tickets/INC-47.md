# INC-47 — Damage-state shader overlay (visual integrity feedback)

**Status:** Research Needed  
**Priority:** Medium (3)  
**Parent:** INC-14  
**URL:** https://linear.app/incremental-asteroid-boomerang/issue/INC-47/damage-state-shader-overlay-visual-integrity-feedback

## Description

Shader parameter updated on each hit to show damage taken. Should look like a white line that travels from the top to the bottom as the enemy takes damage. The main enemy sprite above the line should look like the same as the enemy's normal sprite state, but with de saturated colors.

## Research Questions

- What shaders exist in the project today?
- How are enemy sprites rendered (SpriteRenderer, materials)?
- Is there an existing damage/hit system that could trigger shader updates?
- What's the Unity shader authoring approach used (ShaderGraph, HLSL, URP Lit)?
- How should the shader parameter be driven (script sets a float on the material)?
