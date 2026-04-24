# Skill Test Spec: /quick-design

## Skill Summary

`/quick-design` produces a lightweight design spec for features too small to
warrant a full 8-section GDD. The target scope is under 2 hours of design time
for a single-system feature. Instead of the full 8-section GDD format, the
quick-design spec uses a streamlined 3-section format: Overview, Rules, and
Acceptance Criteria.

The skill has no director gates. The skill asks "May I write" before writing the
design note to `design/quick-notes/[name].md`.

---

## Static Assertions (Structural)

Verified automatically by `/skill-test static` — no fixture needed.

- [ ] Has required frontmatter fields: `name`, `description`, `argument-hint`, `user-invocable`, `allowed-tools`
- [ ] Has ≥2 phase headings
- [ ] Contains verdict keywords: CREATED, BLOCKED, REDIRECTED
- [ ] Contains "May I write" collaborative protocol language (for quick-note file)
- [ ] Has a next-step handoff at the end
- [ ] Explicitly notes: no director gates (lightweight skill by design)
- [ ] Mentions scope check: redirects to `/design-system` if scope exceeds sub-2h threshold

---

## Director Gate Checks

No director gates. Full GDD review is not needed for sub-4-hour single-system features.

---

## Test Cases

### Case 1: Happy Path — Small UI change produces a 3-section spec

**Fixture:**
- No existing quick-note for the target feature
- Feature is clearly scoped: a single UI element change with no cross-system impact

**Input:** `/quick-design [feature-name]`

**Expected behavior:**
1. Skill asks scoping questions: what system, what change, what is the acceptance signal
2. Skill drafts a 3-section spec: Overview, Rules, Acceptance Criteria
3. Draft is shown to user
4. "May I write `design/quick-notes/[name].md`?" is asked
5. File is written after approval

**Assertions:**
- [ ] Spec contains exactly 3 sections: Overview, Rules, Acceptance Criteria
- [ ] File is written to the correct path: `design/quick-notes/[name].md`
- [ ] Verdict is CREATED after successful write

---

### Case 3: Edge Case — File already exists; offered to update

**Fixture:**
- `design/quick-notes/[name].md` already exists from a previous session

**Input:** `/quick-design [name]`

**Expected behavior:**
1. Skill detects existing quick-note file and reads its current content
2. Skill asks: "[name].md already exists. Update it, or create a new version?"
3. User selects update
4. Skill shows the existing spec and asks which section to revise
5. Updated spec is shown, "May I write?" asked, file updated after approval

**Assertions:**
- [ ] Skill detects and reads the existing file before offering to update
- [ ] User is offered update or create-new options — not auto-overwritten
- [ ] Only the revised section is updated (or the whole spec if user chooses full rewrite)

---

### Case 4: Edge Case — No argument provided

**Fixture:**
- `design/quick-notes/` directory may or may not exist

**Input:** `/quick-design` (no argument)

**Expected behavior:**
1. Skill detects no argument is provided
2. Skill outputs a usage error: "No feature name specified. Usage: /quick-design [feature-name]"
3. Skill provides an example: `/quick-design pause-menu-settings`
4. No file is created

**Assertions:**
- [ ] Skill outputs a usage error when no argument is given
- [ ] A usage example is shown with the correct format
- [ ] No quick-note file is written
- [ ] Skill does NOT silently pick a feature name or default to any action

---

## Protocol Compliance

- [ ] Scope check runs before drafting (redirects to `/design-system` if scope too large)
- [ ] 3-section format used (Overview, Rules, Acceptance Criteria) — NOT the 8-section GDD format
- [ ] No director gates — no review-mode.txt read
- [ ] Ends with next-step handoff (e.g., proceed to implementation or `/dev-story`)

---

## Coverage Notes

- The scope threshold heuristic (sub-2h, single-system) is a judgment call —
  the skill's internal check is the authoritative definition and is not
  independently tested by counting hours.
- The `design/quick-notes/` directory is created automatically if it does not
  exist — this filesystem behavior is not independently tested here.
- Integration with the story pipeline (can a quick-design generate a story
  directly?) is out of scope for this spec — quick-designs are standalone.
