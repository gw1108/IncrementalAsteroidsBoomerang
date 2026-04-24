# Agent Coordination and Delegation Map

## Organizational Hierarchy

```
                           [Human Developer]
                                 |
                 +---------------+---------------+
                 |               |               |
         creative-director  technical-director  producer
                 |               |               |
        +--------+--------+     |        (coordinates all)
        |        |               |
  game-designer art-dir     lead-programmer  audio-dir
        |        |               |                |
     +--+--+     |           +--+--+--+--+--+   |
     |  |  |     |           |  |  |  |  |  |   |
    sys lvl eco  ta         gp ep  ai net tl ui snd
                                 |
                             +---+    
                             |        
                          perf-a

  Additional Leads (report to producer/directors):
    prototyper              -- Rapid throwaway prototypes, concept validation

  Engine Specialists (use the SET matching your engine):
    unreal-specialist  -- UE5 lead: Blueprint/C++, GAS overview, UE subsystems
      ue-gas-specialist         -- GAS: abilities, effects, attributes, tags, prediction
      ue-blueprint-specialist   -- Blueprint: BP/C++ boundary, graph standards, optimization
      ue-replication-specialist -- Networking: replication, RPCs, prediction, bandwidth
      ue-umg-specialist         -- UI: UMG, CommonUI, widget hierarchy, data binding

    unity-specialist   -- Unity lead: MonoBehaviour/DOTS, Addressables, URP/HDRP
      unity-dots-specialist         -- DOTS/ECS: Jobs, Burst, hybrid renderer
      unity-shader-specialist       -- Shaders: Shader Graph, VFX Graph, SRP customization
      unity-addressables-specialist -- Assets: async loading, bundles, memory, CDN
      unity-ui-specialist           -- UI: UI Toolkit, UGUI, UXML/USS, data binding

    godot-specialist   -- Godot 4 lead: GDScript, node/scene, signals, resources
      godot-gdscript-specialist    -- GDScript: static typing, patterns, signals, performance
      godot-shader-specialist      -- Shaders: Godot shading language, visual shaders, VFX
      godot-gdextension-specialist -- Native: C++/Rust bindings, GDExtension, build systems
```

### Legend
```
sys  = systems-designer       gp  = gameplay-programmer
lvl  = level-designer         ep  = engine-programmer
eco  = economy-designer       ai  = ai-programmer
ta   = technical-artist       net = network-programmer
tl   = tools-programmer       ui  = ui-programmer
snd  = sound-designer         perf-a = performance-analyst
art-dir = art-director
```

## Delegation Rules

### Who Can Delegate to Whom

| From | Can Delegate To |
|------|----------------|
| creative-director | game-designer, art-director, audio-director |
| technical-director | lead-programmer, technical-artist (technical decisions) |
| producer | Any agent (task assignment within their domain only) |
| game-designer | systems-designer, level-designer, economy-designer |
| lead-programmer | gameplay-programmer, engine-programmer, ai-programmer, network-programmer, tools-programmer, ui-programmer |
| art-director | technical-artist, ux-designer |
| audio-director | sound-designer |
| prototyper | (works independently, reports findings to producer and relevant leads) |
| [engine]-specialist | engine sub-specialists (delegates subsystem-specific work) |
| [engine] sub-specialists | (advises all programmers on engine subsystem patterns and optimization) |

### Escalation Paths

| Situation | Escalate To |
|-----------|------------|
| Two designers disagree on a mechanic | game-designer |
| Game design vs technical feasibility | producer (facilitates), then creative-director + technical-director |
| Art vs audio tonal conflict | creative-director |
| Code architecture disagreement | technical-director |
| Cross-system code conflict | lead-programmer, then technical-director |
| Schedule conflict between departments | producer |
| Scope exceeds capacity | producer, then creative-director for cuts |
| Performance budget violation | performance-analyst flags, technical-director decides |

## Common Workflow Patterns

### Pattern 1: New Feature (Full Pipeline)

```
1. creative-director  -- Approves feature concept aligns with vision
2. game-designer      -- Creates design document with full spec
3. producer           -- Schedules work, identifies dependencies
4. lead-programmer    -- Designs code architecture, creates interface sketch
5. [specialist-programmer] -- Implements the feature
6. technical-artist   -- Implements visual effects (if needed)
7. sound-designer     -- Creates audio event list (if needed)
8. lead-programmer    -- Code review
9. producer           -- Marks task complete
```

### Pattern 2: Bug Fix

```
1. Bug report filed with /bug-report
2. producer           -- Assigns to sprint
3. lead-programmer    -- Identifies root cause, assigns to programmer
4. [specialist-programmer] -- Fixes the bug
5. lead-programmer    -- Code review
6. producer           -- Closes bug
```

### Pattern 3: Balance Adjustment

```
1. game-designer      -- Evaluates balance issue against design intent
2. economy-designer   -- Models the adjustment
3. game-designer      -- Approves the new values
4. [data file update] -- Change configuration values
```

### Pattern 4: New Area/Level

```
1. level-designer     -- Designs layout, encounters, pacing
2. game-designer      -- Reviews mechanical design of encounters
3. art-director       -- Defines visual direction for the area
4. audio-director     -- Defines audio direction for the area
5. [implementation by relevant programmers and artists]
```

### Pattern 5: Sprint Cycle

```
1. producer           -- Plans sprint with /sprint-plan new
2. [All agents]       -- Execute assigned tasks
3. producer           -- Daily status with /sprint-plan status
4. lead-programmer    -- Continuous code review during sprint
5. producer           -- Sprint retrospective with post-sprint hook
6. producer           -- Plans next sprint incorporating learnings
```

### Pattern 6: Milestone Checkpoint

```
1. producer           -- Runs /milestone-review
2. creative-director  -- Reviews creative progress
3. technical-director -- Reviews technical health
4. producer           -- Facilitates go/no-go discussion
5. [All directors]    -- Agree on scope adjustments if needed
6. producer           -- Documents decisions and updates plans
```

### Pattern 7: Release Pipeline (User-Managed)

```
1. producer             -- Declares release candidate, confirms milestone criteria met
2. performance-analyst  -- Confirms performance benchmarks within targets
3. technical-director   -- Final sign-off on major releases
4. User manually        -- Cuts release branch, tags release, deploys, manages rollouts
5. producer             -- Marks release complete
```

### Pattern 8: Rapid Prototype

```
1. game-designer        -- Defines the hypothesis and success criteria
2. prototyper           -- Scaffolds prototype with /prototype
3. prototyper           -- Builds minimal implementation (hours, not days)
4. game-designer        -- Evaluates prototype against criteria
5. prototyper           -- Documents findings report
6. creative-director    -- Go/no-go decision on proceeding to production
7. producer             -- Schedules production work if approved
```

## Cross-Domain Communication Protocols

### Design Change Notification

When a design document changes, the game-designer must notify:
- lead-programmer (implementation impact)
- producer (schedule impact assessment)
- Relevant specialist agents depending on the change

### Architecture Change Notification

When an ADR is created or modified, the technical-director must notify:
- lead-programmer (code changes needed)
- All affected specialist programmers
- producer (schedule impact)

### Asset Standard Change Notification

When the art bible or asset standards change, the art-director must notify:
- technical-artist (pipeline changes)
- All content creators working with affected assets

## Anti-Patterns to Avoid

1. **Bypassing the hierarchy**: A specialist agent should never make decisions
   that belong to their lead without consultation.
2. **Cross-domain implementation**: An agent should never modify files outside
   their designated area without explicit delegation from the relevant owner.
3. **Shadow decisions**: All decisions must be documented. Verbal agreements
   without written records lead to contradictions.
4. **Monolithic tasks**: Every task assigned to an agent should be completable
   in 1-3 days. If it is larger, it must be broken down first.
5. **Assumption-based implementation**: If a spec is ambiguous, the implementer
   must ask the specifier rather than guessing. Wrong guesses are more expensive
   than a question.
