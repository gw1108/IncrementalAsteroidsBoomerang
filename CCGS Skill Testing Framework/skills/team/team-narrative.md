# Skill Test Spec: /team-narrative

## Skill Summary

Orchestrates the narrative team through a five-phase pipeline: narrative direction
(narrative-director) → world foundation + dialogue drafting (world-builder and writer
in parallel) → level narrative integration (level-designer) → consistency review
(narrative-director) → polish + localization compliance (writer, localization-lead,
and world-builder in parallel). Uses `AskUserQuestion` at each phase transition to
present proposals as selectable options. Produces a narrative summary report and
delivers narrative documents via subagents that each enforce the "May I write?"
protocol. Verdict is COMPLETE when all phases succeed, or BLOCKED when a dependency
is unresolved.

---

## Static Assertions (Structural)

- [ ] Has required frontmatter fields: `name`, `description`, `argument-hint`, `user-invocable`, `allowed-tools`
- [ ] Has ≥2 phase headings
- [ ] Contains verdict keywords: COMPLETE, BLOCKED
- [ ] Contains "File Write Protocol" section
- [ ] File writes are delegated to sub-agents — orchestrator does not write files directly
- [ ] Sub-agents enforce "May I write to [path]?" before any write
- [ ] Has a next-step handoff at the end (references `/design-review`, `/localize extract`, `/dev-story`)
- [ ] Error Recovery Protocol section is present
- [ ] `AskUserQuestion` is used at phase transitions before proceeding
- [ ] Phase 2 explicitly spawns world-builder and writer in parallel
- [ ] Phase 5 explicitly spawns writer, localization-lead, and world-builder in parallel

---

## Test Cases

### Case 1: Happy Path — All five phases complete, narrative doc delivered

**Fixture:**
- A game concept and GDD exist for the target feature (e.g., `design/gdd/faction-intro.md`)
- Character voice profiles exist (e.g., `design/narrative/characters/`)
- Existing lore entries exist for cross-reference (e.g., `design/narrative/lore/`)
- No lore contradictions exist between existing entries and the new content

**Input:** `/team-narrative faction introduction cutscene for the Ironveil faction`

**Expected behavior:**
1. Phase 1: narrative-director is spawned; outputs a narrative brief defining the story beat, characters involved, emotional tone, and lore dependencies
2. `AskUserQuestion` presents the narrative brief; user approves before Phase 2 begins
3. Phase 2: world-builder and writer are spawned in parallel; world-builder produces lore entries for the Ironveil faction; writer drafts dialogue lines using character voice profiles
4. `AskUserQuestion` presents world foundation and dialogue drafts; user approves before Phase 3 begins
5. Phase 3: level-designer is spawned; produces environmental storytelling layout, trigger placement, and pacing plan
6. `AskUserQuestion` presents level narrative plan; user approves before Phase 4 begins
7. Phase 4: narrative-director reviews all dialogue against voice profiles, verifies lore consistency, confirms pacing; approves or flags issues
8. `AskUserQuestion` presents review results; user approves before Phase 5 begins
9. Phase 5: writer, localization-lead, and world-builder are spawned in parallel; writer performs final self-review; localization-lead validates i18n compliance; world-builder finalizes canon levels
10. Final summary report is presented; subagent asks "May I write the narrative document to [path]?" before writing
11. Verdict: COMPLETE

**Assertions:**
- [ ] narrative-director is spawned in Phase 1 before any other agents
- [ ] `AskUserQuestion` appears after Phase 1 output and before Phase 2 launch
- [ ] world-builder and writer Task calls are issued simultaneously in Phase 2 (not sequentially)
- [ ] level-designer is not launched until Phase 2 `AskUserQuestion` is approved
- [ ] narrative-director is re-spawned in Phase 4 for consistency review
- [ ] Phase 5 spawns all three agents (writer, localization-lead, world-builder) simultaneously
- [ ] Summary report includes: narrative brief status, lore entries created/updated, dialogue lines written, level narrative integration points, consistency review results
- [ ] No files are written by the orchestrator directly
- [ ] Verdict is COMPLETE after delivery

---

### Case 2: Lore Contradiction Found — world-builder finds conflict before writer proceeds

**Fixture:**
- Existing lore entry at `design/narrative/lore/ironveil-history.md` states the Ironveil faction was founded 200 years ago
- The new narrative brief (from Phase 1) states the Ironveil were founded 50 years ago
- The writer has been spawned in parallel with the world-builder in Phase 2

**Input:** `/team-narrative ironveil faction introduction cutscene`

**Expected behavior:**
1. Phases 1–2 begin normally
2. Phase 2 world-builder detects a factual contradiction between the narrative brief and existing lore: founding date conflict
3. world-builder returns BLOCKED with reason: "Lore contradiction found — founding date conflicts with `design/narrative/lore/ironveil-history.md`"
4. Orchestrator surfaces the contradiction immediately: "world-builder: BLOCKED — Lore contradiction: founding date in narrative brief (50 years ago) conflicts with existing canon (200 years ago in `ironveil-history.md`)"
5. Orchestrator assesses dependency: the writer's dialogue depends on canon lore — the writer's draft cannot be finalized without resolving the contradiction
6. `AskUserQuestion` presents options:
   - Revise the narrative brief to match existing canon (200 years ago)
   - Update the existing lore entry to reflect the new canon (50 years ago)
   - Stop here and resolve the contradiction in the lore docs first
7. Writer output is preserved but flagged as pending canon resolution — work is not discarded
8. Orchestrator does NOT proceed to Phase 3 until the contradiction is resolved or user explicitly chooses to skip

**Assertions:**
- [ ] Contradiction is surfaced before Phase 3 begins
- [ ] Orchestrator does not silently resolve the contradiction by picking one version
- [ ] `AskUserQuestion` presents at least 3 options including "stop and resolve first"
- [ ] Writer's draft output is preserved in the partial report, not discarded
- [ ] Phase 3 (level-designer) is not launched until the user resolves the contradiction
- [ ] Verdict is BLOCKED (not COMPLETE) if the user stops to resolve the contradiction

---

### Case 3: No Argument — Usage guidance shown

**Fixture:**
- Any project state

**Input:** `/team-narrative` (no argument)

**Expected behavior:**
1. Skill detects no argument is provided
2. Outputs usage guidance: e.g., "Usage: `/team-narrative [narrative content description]` — describe the story content, scene, or narrative area to work on (e.g., `boss encounter cutscene`, `faction intro dialogue`, `tutorial narrative`)"
3. Skill exits without spawning any agents

**Assertions:**
- [ ] Skill does NOT spawn any agents when no argument is provided
- [ ] Usage message includes the correct invocation format with an argument example
- [ ] Skill does NOT attempt to guess or infer a narrative topic from project files
- [ ] No `AskUserQuestion` is used — output is direct guidance

---

## Protocol Compliance

- [ ] `AskUserQuestion` is used after every phase output before the next phase launches
- [ ] Parallel spawning: Phase 2 (world-builder + writer) and Phase 5 (writer + localization-lead + world-builder) issue all Task calls before waiting for results
- [ ] No files are written by the orchestrator directly — all writes are delegated to sub-agents
- [ ] Each sub-agent enforces the "May I write to [path]?" protocol before any write
- [ ] BLOCKED status from any agent is surfaced immediately — not silently skipped
- [ ] A partial report is always produced when some agents complete and others block
- [ ] Verdict is exactly COMPLETE or BLOCKED — no other verdict values used
- [ ] Next Steps handoff references `/design-review`, `/localize extract`, and `/dev-story`

---

## Coverage Notes

- Phase 3 (level-designer) and Phase 4 (narrative-director review) happy-path behavior are
  validated implicitly by Case 1. Separate edge cases are not needed for these phases as
  their failure modes follow the standard Error Recovery Protocol.
- The "Retry with narrower scope" and "Skip this agent" resolution paths from the Error
  Recovery Protocol are not separately tested — they follow the same `AskUserQuestion`
  + partial-report pattern validated in Cases 2 and 5.
- Localization concerns that are advisory (e.g., German/Finnish +30% expansion warnings)
  vs. blocking (hardcoded formats) are distinguished in Case 4; advisory-only scenarios
  follow the same pattern but do not change the verdict.
- The writer's "all lines under 120 characters" and "string keys not raw strings" checks
  in Phase 5 are covered implicitly by Case 4's localization compliance scenario.
