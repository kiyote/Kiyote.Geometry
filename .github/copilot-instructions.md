# Copilot Instructions

## Project Guidelines
- Prefer IDE-aware tools (the editor's edit/build/diagnostics tooling) over terminal commands when working in this workspace; only use the terminal for operations the IDE tooling cannot perform.

## Code Safety
- In Kiyote.Geometry Topology code, types are not internally thread-safe; the user holds topology references behind their own external locks, so do not add internal locking/synchronization.
