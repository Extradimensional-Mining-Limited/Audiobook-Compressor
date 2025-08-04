Filename: Onboarding-Notes.md
Last Updated: 2025-07-31 08:50
Version: 1.2.A
State: Experimental
Signed: Codewright

Synopsis:
Practical onboarding notes for new AI implementers joining the Audiobook Compressor project, supplementing the official SOP and Summary.md.

---

# Onboarding Notes for New AI Implementers

These are practical notes and clarifications to help new AI implementers get up to speed quickly, in addition to the official onboarding process described in Summary.md and AI-Collaboration-SOP.md.

## 1. Documentation and Process
- Always read the current version from ChangelogExperimental.md before making any changes.
- Use GetTime.exe to generate the Last Updated timestamp for all file headers you edit.
- Sign all file headers and documentation changes with your name.
- All documentation changes must be non-destructive unless a literal block replacement is explicitly requested.

## 2. Project Structure
- The core compression logic is defined in blueprint.ps1 and must be ported to C# as the project evolves.
- The project uses a two-changelog system: Changelog.md (stable) and ChangelogExperimental.md (development/experimental).
- High-level documentation (Summary.md, Changelog.md) is managed by Praxis; do not edit unless explicitly instructed.

## 3. Communication and Directives
- All implementation work is driven by literal Focus.md directives or explicit user instructions.
- If you are unsure about a directive or encounter ambiguity, check in with the Architect (User) or Praxis (Strategist) before proceeding.

## 4. Practical Tips
- Use the text_search tool to gather context before making changes, especially in unfamiliar areas of the codebase.
- When editing code, make only the minimal changes required to fulfill the directive.
- If you encounter a process or technical issue not covered in the SOP, document it here for future implementers.

---

Feel free to expand this file with additional practical notes as the project evolves.
