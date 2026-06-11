# GitHub Copilot Instructions for [RF] Rumor Has It.... (Continued)

## Mod Overview and Purpose

[RF] Rumor Has It.... (Continued) is an enhancement and continuation of the original mod by SeveralPuffins, which adds depth and intrigue to the social interactions between pawns in RimWorld. This mod introduces new interactions, traits, and events that emphasize the social dynamics and gossip within the colony, aiming to create a more complex and engaging social experience.

## Key Features and Systems

- **New Social Interactions:**
  - **Chat About Other Pawns:** Pawns can express their opinions about other colonists.
  - **Quarrel:** Friendly pawns can have arguments that negatively impact their relationships.
  - **Spread Rumor:** Pawns may share unsubstantiated stories, especially about disliked individuals.
  - **Share Secret:** Trusted pawns can share intimate secrets, fostering closer relationships.
  - **Reveal Secret:** Secrets previously entrusted can be gossiped about.
  - **Apologize:** Pawns can attempt to reconcile after insulting or hurting others.
  - **Make Peace:** Interceding pawns might apologize on behalf of someone else.
  - **Culture Clash:** Misunderstandings arising from cultural differences can lead to unintentional insults.

- **New Traits:**
  - From compulsive liars to peacemakers, each trait affects how pawns interact socially.

- **New Events:**
  - Events like defection, splintering cliques, and brawls emerge from social dynamics within the colony.

- **Trait Interaction:**
  - Negative traits such as Annoying Voice are mitigated among pawns sharing those traits.

## Coding Patterns and Conventions

- The codebase adheres to C# conventions with a strong emphasis on readability and maintainability.
- Use of descriptive naming conventions for classes, methods, and variables.
- Organized use of regions within files, dividing sections logically for clarity.
- Integration of XML Defs for RimWorld's external definitions, structured clearly for integration.

## XML Integration

XML plays a critical role in defining game behaviors such as interactions, events, and traits. The following files are essential:

- `Factions.xml`: Defines faction-specific behaviors and characteristics.
- `Incidents.xml`: Contains definitions for incidents such as brawls.
- `Interactions_Social.xml`: Details the new social interactions.
- `RulePacks.xml`: Provides rules for interaction dialogues.
- `Thoughts_Memory_Social.xml`: Defines social memory thoughts relevant to new interactions.

## Harmony Patching

- Harmony is employed extensively for patching game methods to introduce new behaviors without modifying core game code.
- Ensure patches are well-documented and utilize prefixes, postfixes, and transpilers as needed.
- Manage patches carefully to avoid conflicts with other mods, especially "Psychology," with which compatibility is intended.

## Suggestions for Copilot

1. **Code Suggestions:**
   - Implement new social interactions by suggesting code for calculating relationship impacts.
   - Enhance trait definitions by suggesting methods for interaction effects.
   - Suggest improvements in code performance for event handling and social interaction checks.

2. **XML Suggestions:**
   - Assist in structuring new thought or interaction definitions.
   - Offer synonyms for trait-related text to diversify dialogue options.

3. **Debugging Help:**
   - Recommend ways to identify and resolve errors by suggesting checking log files.
   - Offer suggestions for debugging Harmony patches by redirecting debug output.

By adhering to these guidelines and suggestions, developers can maintain and expand the mod effectively, ensuring continued player engagement through enhanced social interactions within RimWorld.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).

