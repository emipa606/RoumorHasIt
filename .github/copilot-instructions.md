# GitHub Copilot Instructions for [RF] Rumor Has It.... (Continued)

Welcome to the development guidelines for the [RF] Rumor Has It.... (Continued) mod, a project aimed at bringing deeper social interactions and intrigue to RimWorld. This document is designed to guide code structuring, integration, and usage within the project. If you're using GitHub Copilot or contributing to this mod, this document will provide you with helpful instructions and suggestions.

## Mod Overview and Purpose

"[RF] Rumor Has It.... (Continued)" is an update of the original mod by Rainbeau Flambe, focused on enhancing social complexity within RimWorld through new interactions, traits, and events. This mod aims to showcase gossip, secrets, and the social dynamics that emerge when pawns talk about each other. It is a revival of the pivotal concept from SeveralPuffins' earlier mod, expanding upon it with additional content and compatibility updates.

## Key Features and Systems

### New Interactions
- **Chat About Other Pawns:** Pawns can express their opinions about other colony members.
- **Quarrel:** Potential for friendly relationships to be strained through arguments.
- **Spread Rumor:** Circulation of unverified stories among pawns.
- **Share and Reveal Secret:** Trust dynamics and gossip within the colony.
- **Apologize and Make Peace:** Resolution interactions for previous negative actions.
- **Culture Clash:** Misunderstandings due to cultural differences.

### New Traits
- **Compulsive Liar:** Pawns that compulsively fabricate stories.
- **Gossip:** Pawns obsessed with discussing other people’s affairs.
- **Gushing, Manipulative, Peacemaker, Trustworthy:** Traits affecting social interactions and colony harmony.

### New Events
- **Defection, Splinter, Brawl:** Social isolation and clique dynamics leading to colony challenges.

### Trait Amelioration
- **Amelioration of Negative Traits:** Reduced social penalty for pawns sharing traits such as Annoying Voice or Ugly.

## Coding Patterns and Conventions

- **Namespace and Class Organization:** Keep namespaces organized by functionality, e.g., `InteractionWorker`, `ThoughtWorker`, `IncidentWorker`.
- **Descriptive Naming:** Use clear and descriptive names for methods and classes to enhance readability and maintainability.
- **Commenting and Documentation:** Adequately comment complex logic and interaction flows.

## XML Integration

- **Trait and Thought Definitions:** Ensure all trait and thought definitions align with XML data files. These are usually stored under `Defs/`.
- **RulePack and Pawn Interaction:** Link XML definitions with the C# logic to ensure seamless execution in gameplay.
- **Event Settings:** Use XML settings to allow toggling of events via the mod's Options menu.

## Harmony Patching

- **Purpose:** Use Harmony to adjust existing game behavior without overwriting core files.
- **Convention:** Follow the standard `MethodPrefix`, `MethodPostfix`, and `MethodTranspiler` to apply patches effectively.
- **Documentation:** Document the purpose and specifics of each patch in comments.

## Suggestions for Copilot

- **Use Contextual Suggestions:** Leverage Copilot for mundane or repetitive coding tasks by feeding it specific function names and descriptions.
- **Follow Style Guides:** Ensure Copilot adheres to the project’s coding conventions for consistency.
- **Schema and Definitions:** Utilize Copilot to quickly draft repetitive XML schemata and new class prototypes.

---

By following these guidelines, developers and contributors can ensure that "[RF] Rumor Has It.... (Continued)" remains consistent, enhancing gameplay without sacrificing code quality. Feel free to explore the mod's features and extend functionality while adhering to these standards.


This markdown document provides a structured guideline for contributors using GitHub Copilot to maintain and develop features within the mod. It outlines key aspects of the mod, coding standards, integration procedures, and makes suggestions on utilizing Copilot effectively.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
