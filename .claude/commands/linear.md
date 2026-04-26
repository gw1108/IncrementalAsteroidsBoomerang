---
description: Manage Linear tickets - create, update, comment, and follow workflow patterns
---

# Linear - Ticket Management

You are tasked with managing Linear tickets for the IncrementalAsteroidBoomerang game project.

## Initial Setup

First, verify that Linear MCP tools are available by checking if any `mcp__linear__` tools exist. If not, respond:
```
I need access to Linear tools to help with ticket management. Please run the `/mcp` command to enable the Linear MCP server, then try again.
```

## Team Workflow & Status Progression

1. **Backlog** → Parking lot for future or human-only work
2. **Todo** → Ready for implementation
3. **In Progress** → Active development
4. **In Review** → PR / code review phase
5. **Done** → Completed

**Key principle**: Keep tickets scoped to remaining work only. Human-only systems (P1b, U2, A1) go to Backlog with a clear note.

## Important Conventions

### Default Values
- **Status**: Create new tickets in "Todo" unless human-only (use "Backlog")
- **Team**: IncrementalAsteroidBoomerang (`95e668b5-e4ee-4ba3-8c0c-22a38097ce0e`)
- **Priority**: Default to Medium (3)
  - Urgent (1): Critical blockers, broken builds
  - High (2): Foundation systems or early-order deliverables
  - Medium (3): Standard feature work (default)
  - Low (4): Human-only items, presentation polish

### Automatic Label Assignment
- **Feature**: New system or major mechanic
- **Improvement**: Enhancement to an existing, partially-implemented system
- **Bug**: Defect in existing behavior

## Action-Specific Instructions

### 1. Creating Tickets

1. **Gather context:**
   - Read the relevant section in `design/gdd/systems-index.md`
   - Identify remaining work only (skip what's already implemented)
   - Note human-only restrictions if present

2. **Draft the ticket:**
   ```
   ## Problem to solve
   [Why this system is needed — player experience or technical dependency]

   ## Scope (remaining work only)
   - [ ] Sub-task 1
   - [ ] Sub-task 2

   ## Acceptance criteria
   - Behavior matches GDD spec
   - No Unity console errors or warnings
   - Stats read from GameStatsContext (no hardcoded values)
   ```

3. **Create the ticket:**
   ```
   mcp__linear__save_issue with:
   - title: [system code] — [short name]
   - description: [draft above]
   - teamId: 95e668b5-e4ee-4ba3-8c0c-22a38097ce0e
   - priority: [1–4]
   - stateId: [Todo or Backlog ID]
   - labelIds: [Feature / Improvement / Bug]
   ```

4. **Add sub-tasks** as separate child issues linked via `parentId`.

### 2. Adding Comments to Tickets

- Keep comments to ~10 lines
- Focus on insight, decisions, and blockers — not mechanical summaries
- Reference code files with backticks: `Assets/_Scripts/Gameplay/Example.cs`

### 3. Updating Ticket Status

Move tickets through the workflow as work progresses:
- Backlog → Todo (human picks it up / LLM unblocked)
- Todo → In Progress (dev started)
- In Progress → In Review (PR open)
- In Review → Done (merged, console clean)

## Comment Quality Guidelines

Focus on:
- **Key insights**: the "aha" moment or critical understanding
- **Decisions and tradeoffs**: what approach was chosen and why
- **Blockers resolved**: what was preventing progress
- **State changes**: what's different now and what it means for next steps

Avoid mechanical change lists that don't add context.

## Important Notes

- All tickets must include a clear "Problem to solve"
- Keep tickets concise and scannable
- Gameplay systems must read stats from `GameStatsContext` — flag any that hardcode values
- Reference code files as: `Assets/_Scripts/[Domain]/File.cs:linenum`
- Human-only systems must include: `> ⚠ Human-only: This system cannot be implemented by LLMs.`
- Implementation order is defined by the design order table in `systems-index.md` — respect it when setting priority

---

## Commonly Used IDs

### Team
- **IncrementalAsteroidBoomerang**: `95e668b5-e4ee-4ba3-8c0c-22a38097ce0e`

### Workflow State IDs
- **Backlog**: `afaeec7e-57ca-44ef-9882-6c2fb2b65dfa`
- **Todo**: `07774a57-a827-4897-8bbb-939cc2f4119b`
- **Research Needed**: `65b78918-248d-4962-a8c0-16866d460d10`
- **In Progress**: `d5988bfa-b373-433e-b55b-f750af748852`
- **In Review**: `140a3257-34d7-4103-9f67-f91e249d4a18`
- **Done**: `cf1c0c6c-0912-4f8e-a244-b922bbefca61`
- **Duplicate**: `cf3de93b-4f34-4518-9e22-727fb7161f5f`
- **Canceled**: `7101ea58-df4b-4e50-a7c1-c9b876e288c6`

### Label IDs
- **Feature**: `fb969dc9-ef16-46cb-a058-d1c00711d4c2`
- **Improvement**: `9f5cfd14-ce0e-402a-ba7c-afb371d320eb`
- **Bug**: `4ad00c18-e04d-4e6c-b9ab-de3f92973f30`

### Linear User IDs
- **George Wang**: `244d386e-4841-4d22-8e71-ea8190417bf1`
