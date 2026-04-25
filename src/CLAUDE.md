# Source Directory

When writing or editing game code in this directory, follow these standards.

## Engine Version Warning

**Always check `docs/engine-reference/` before using any engine API.**

## Coding Standards

- All public APIs require doc comments
- Gameplay values must be **data-driven** (external config files), never hardcoded

## Coupling Style

Prefer direct function calls over pub/sub or event-broadcast systems (EventRouter, UnityEvent, Action callbacks). Tight coupling between systems is fine — favor simplicity and stack-trace traceability over decoupling abstractions.

## File Routing

Match the engine-specialist agent to the file type being written.
See `CLAUDE.md` → Technical Preferences → Engine Specialists → File Extension Routing.
