# [RF] Rumor Has It.... (Continued) - GitHub Copilot Instructions

## Mod Overview and Purpose
"[RF] Rumor Has It.... (Continued)" is an engaging update to the original mod "Rumours and Deception" by SeveralPuffins, continuing the work of Rainbeau Flambe. This mod enriches the social dynamics in RimWorld by introducing a suite of new social interactions, traits, and events that revolve around pawns gossiping, sharing secrets, and navigating relationships within the colony. If you've ever wished your pawns had more to say about each other or enjoyed the intricacies of colony life, this mod is for you.

## Key Features and Systems
### New Interactions:
- **Chat About Other Pawns:** Enhances social interactions with discussions about other community members.
- **Quarrel and Make Peace:** Enables arguments and reconciliations, adding depth to relationships.
- **Spread and Reveal Secrets:** Allows pawns to share and gossip about secrets.
- **Apologize and Culture Clash:** Provides mechanisms for resolving conflicts and understanding cultural differences.

### New Traits:
- Traits like **Compulsive Liar**, **Gossip**, and **Trustworthy** define how pawns interact based on their personalities.

### New Events:
- Events like **Defection**, **Splinter**, and **Brawl** trigger based on social dynamics, affecting colony stability and prompting new narratives.

### Negative Trait Amelioration:
- Certain negative traits have reduced impact when shared among pawns, promoting solidarity.

## Coding Patterns and Conventions
- Follow C# naming conventions: PascalCase for class names and methods, camelCase for method arguments and local variables.
- Utilize static classes like `Watchers` and `ThirdPartyManager` for shared utilities and settings, ensuring modular and organized code.
- Emphasize readability and maintainability by keeping methods succinct and single-purposed.

## XML Integration
- Integrate XML for defining traits and incidents. Utilize files like `RumorsTraitDefOf` and `RumorsThoughtDefOf` for trait and thought definitions.
- Ensure XML files are well-structured, with appropriate schema definitions. Consistency is key for easy debugging and updates.

## Harmony Patching
- Use Harmony to patch core methods and add new functionality without directly modifying the base game code, ensuring future compatibility.
- Ensure patch classes are organized logically and perform necessary checks to avoid conflicts with other mods. Harmony patches can be applied in methods within files like `Controller.cs`.

## Suggestions for Copilot
When using GitHub Copilot for additional development, consider the following:
1. **New Interactions:** Prompt Copilot to help design methods for upcoming interactions by describing expected player experiences.
2. **Conflict Resolution:** Leverage Copilot to suggest improvements or alternative approaches to handling complex event resolutions, like defection or splinter events.
3. **Trait Balancing:** Utilize Copilot to simulate how newly added traits might affect inter-pawn relations and overall game balance.
4. **Event Customization:** Ask Copilot to help craft new events or tweak existing settings within the framework established by `IncidentWorker` classes.
5. **Code Documentation:** Encourage Copilot to auto-generate XML Doxygen comments for methods, detailing the intent, parameters, and expected outcomes of code segments.

This document aims to streamline the modding process for developers while making enhancement and collaborative efforts straightforward and effective.
