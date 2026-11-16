# GitHub Copilot Instructions for [RF] Rumor Has It.... (Continued) Mod

## Mod Overview and Purpose
The [RF] Rumor Has It.... (Continued) mod is an update of Rainbeau Flambes' mod, which adds social interactions, traits, thoughts, and events to enhance the social complexity and intrigue in RimWorld. The mod introduces new social interactions such as "Chat About Other Pawns," "Quarrel," "Spread Rumor," and "Share Secret," among others. It also introduces new traits like "Compulsive Liar," "Gossip," and "Peacemaker," and negative traits with reduced impact for those who share them. The mod includes events like "Defection," "Splinter," and "Brawl," which can be switched off in the mod's options menu.

## Key Features and Systems
### Social Interactions
- **Chat About Other Pawns:** Allows pawns to discuss their relationships and opinions about each other.
- **Quarrel:** Pawns can have arguments that damage their relationships.
- **Spread Rumor:** Pawns can tell unsubstantiated tales to each other.
- **Share Secret:** Pawns can reveal deep and personal secrets to trusted friends.
- **Reveal Secret:** Pawns can gossip about secrets they've been told.
- **Apologize:** Pawns can try to make up with others they've insulted or hurt.
- **Make Peace:** Pawns can apologize on behalf of a third party.
- **Culture Clash:** Pawns may insult each other due to cultural misunderstandings.

### Traits
- **Compulsive Liar:** Pawns are always making up stories.
- **Gossip:** Pawns talk about others all the time and cannot be trusted with secrets.
- **Gushing:** Pawns reveal their darkest secrets to anyone.
- **Manipulative:** Pawns use social tricks to make others see their way.
- **Peacemaker:** Pawns try to resolve conflicts and get a mood boost if successful.
- **Trustworthy:** Pawns keep secrets and feel terrible about spreading rumors.

### Events
- **Defection:** Socially isolated pawns may walk off the map and join an ally.
- **Splinter:** Hostile cliques may leave the colony and form a new faction.
- **Brawl:** Hostile cliques may start fights within the colony.

## Coding Patterns and Conventions
- **Mod Structure:** The mod is structured with C# files for logic and XML files for definitions.
- **Naming Conventions:** Class names are PascalCase, while method names are camelCase.
- **Code Comments:** Code is well-commented to explain complex logic or systems.
- **Error Handling:** Error handling is implemented to manage potential issues gracefully.
- **Mod Compatibility:** The mod is designed to be compatible with other popular mods like "Psychology."

## XML Integration
- **Defs:** XML files define game elements such as factions, incidents, interactions, rule packs, and thoughts.
- **Factions.xml:** Contains definitions for faction and kind.
- **Incidents.xml:** Contains definitions for incidents and letters.
- **Interactions_Social.xml:** Contains definitions for social interactions.
- **RulePacks.xml:** Contains definitions for rule packs.
- **Thoughts_Memory_Social.xml:** Contains definitions for social thoughts and memories.

## Harmony Patching
- **Harmony:** Harmony patches are used to modify or extend existing game functionality.
- **Patch Types:** The mod uses Harmony to patch methods in the base game to add new interactions, traits, and events.
- **Patch Files:** C# files contain Harmony patches, which are applied to modify game behavior without altering the original code.

## Suggestions for Copilot
- **Code Suggestions:** Request suggestions for implementing new interactions or traits.
- **Debugging Assistance:** Ask for help debugging issues related to Harmony patches or XML integration.
- **Documentation:** Request documentation for best practices in modding and using Harmony.
- **Compatibility:** Ask for suggestions on ensuring compatibility with other mods.
- **Testing:** Request assistance in writing tests for new features or patches.

By following these guidelines, Copilot can provide valuable assistance in enhancing and maintaining the [RF] Rumor Has It.... (Continued) mod.

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
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

