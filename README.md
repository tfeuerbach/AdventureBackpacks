<div align="center">

# 🎒 Adventure Backpacks *(fork)*

### *A progression-based adventuring backpack mod for Valheim.*

[![Upstream](https://img.shields.io/badge/upstream-Vapok%2FAdventureBackpacks-blue?style=for-the-badge&logo=github)](https://github.com/Vapok/AdventureBackpacks)
[![Fork](https://img.shields.io/badge/fork-tfeuerbach%2FAdventureBackpacks-informational?style=for-the-badge&logo=github)](https://github.com/tfeuerbach/AdventureBackpacks)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)

---

</div>

> **This is an unofficial fork** of [Vapok/AdventureBackpacks](https://github.com/Vapok/AdventureBackpacks).  
> Credit for the mod, assets, and ongoing upstream development belongs to **Vapok** and contributors.  
> This fork tracks upstream `main` and adds a local feature that was declined upstream ([issue #204](https://github.com/Vapok/AdventureBackpacks/issues/204) — closed as wontfix).

### Fork addition: Open backpack beside chests

**Open Alongside Container** defaults to **on** in this fork (Local Config). The equipped backpack always opens in a **side panel to the right of the player inventory** — the same window whether empty or filled, with or without a world chest open.

- Keep a chest (or other container) open while using the backpack
- Drag items between player inventory, chest, and backpack
- Ctrl+click transfers involve the backpack without duping
- Inventory overflow into the backpack still works while looting chests
- Optional layout tweaks: `Alongside Panel Gap`, `Alongside Panel Offset Y`, `Alongside Panel Scale`

Default for this option remains **off** in config (opt-in). Upstream behavior is unchanged when it is disabled.

### Fork regression tests

Pure unit + source-contract tests live in `AdventureBackpacks.Tests` (no Valheim install required). Run before/after mirroring upstream:

```bash
dotnet test AdventureBackpacks.Tests/AdventureBackpacks.Tests.csproj
```

GitHub Actions: `.github/workflows/fork-tests.yml`.

### Automatic upstream release mirror

`.github/workflows/mirror-upstream.yml` checks [Vapok/AdventureBackpacks releases](https://github.com/Vapok/AdventureBackpacks/releases) a few times a day (and on manual dispatch).

| Result | Behavior |
| :--- | :--- |
| No new release | No-op |
| Clean merge (or only non-protected conflicts) **and tests pass** | Opens PR, **auto-merges** into `main`, publishes fork release |
| Conflict in a **protected** fork file | Opens PR labeled `needs-human`, fails the job — **does not** merge |
| Tests fail after merge | Opens PR labeled `needs-human` / `tests-failed`, fails the job — **does not** merge |

**Fork releases:** each synced upstream tag gets a matching fork release (upstream `v2.2.11` → fork `v2.2.11`) with a Thunderstore zip `tfeuerbach-AdventureBackpacks-X.Y.Z.zip` for r2modman. Same path runs when you manually merge a `sync/upstream-*` PR.

**Packaging secret:** set repository secret `VALHEIM_REFS_URL` to a downloadable zip of the local `.refs` tree (`BepInEx/`, `Valheim/`, `APIManager/`). Without it, sync still merges but the release zip job fails.

Protected paths: `.github/fork-protected-paths.txt` (side panel, transfer/overflow patches, tests, this README, sync workflows, `manifest.json`).

Watch the repo (or Actions) so a failed sync run pings you; the open PR has the details.

Manual run: Actions → **Mirror upstream release** → Run workflow (optional tag / force).

---

Starting as a wee Viking rummaging through the tranquil fields of the Meadows, you'll gather materials that lead to your destiny. From Deer Hide and Leather Scraps to mystical Wisp-infused fabrics, you'll learn how to craft, upgrade, and customize your very own **Adventure Backpacks**! Go forth and wander, ye wanderer of the wanders!

---

<div align="center">

<br>

[![Survival Servers](https://raw.githubusercontent.com/Vapok/AdventureBackpacks/main/images/survivalservers_banner.png)](https://www.survivalservers.com/services/game_servers/valheim/?ref=vapok)

</div>

## 🧭 How to Use Adventure Backpacks

* 🔨 **Discover & Craft**: As you explore biomes, defeat creatures, and collect new materials, you will unlock recipes for Adventuring Backpacks.
* 🎒 **Open Your Backpack**: The default hotkey is <kbd>I</kbd> to open your equipped backpack. Fully customizable for Keyboard, Mouse, and Gamepad / Controller inputs.
* 🛠️ **Craft From Backpack**: Build with the hammer or craft at stations using supplies stored in your equipped backpack without needing to manually move materials.
* 📥 **Auto Store & Overflow**: Route gathered loot into matching backpack stacks or overflow new items into your backpack when your main inventory is full.
* 📈 **Progression & Upgrades**: Each backpack features unique inventory sizing, carry weight bonuses, movement speed modifiers, and environmental perks. Upgrading your backpack at a workbench or forge expands capacity and strengthens bonuses.
* ⚙️ **Full Customization**: Almost every aspect of these backpacks (sizes, recipes, drop chances, effects, and weight multipliers) can be tailored via configuration files or the in-game Configuration Manager.

---

## 📦 Backpack Catalogue

| Backpack | Biome / Tier | Description & Key Traits |
| :--- | :--- | :--- |
| **Satchel** | 🌸 Meadows | *A small, lightweight starter pack crafted from early Meadow materials.* |
| **Rugged Backpack** | 🌲 Black Forest | *A sturdy leather pack complete with brass buckles and fine strapping.* |
| **Bloodbag Wetpack** | 🐸 Swamp | *A durable Swamp pack sealed with bloodbags; grants the **Waterproof** perk.* |
| **Arctic Sherpa Pack** | ❄️ Mountain | *An insulated Mountain pack fit for freezing treks; grants **Frost & Cold Resistance**.* |
| **Lox Hide Knappsack** | 🌾 Plains | *A massive Plains pack made from thick, durable lox hide with high carry capacity.* |
| **Explorers Wisppack** | 🌫️ Mistlands | *A mystical backpack with a Box of Holding, built-in **Demister / Wisplight**, and **Slow Fall**.* |
| **Ashlands Pack** | 🔥 Ashlands | *🔥 Coming Soon — Forged for the volcanic fires and molten depths.* |
| **Deep North Pack** | 🧊 Deep North | *🧊 Coming Soon — Prepared for the sub-zero permafrost.* |
| **Legacy Packs** | ⚔️ Legacy | *Original Iron and Silver backpacks preserved for existing world saves.* |

<blockquote style="background-color: rgba(56, 139, 253, 0.08); border-left: 4px solid #388bfd; padding: 12px 16px; margin: 16px 0; border-radius: 4px;">
  <p style="margin: 0 0 6px 0;">💡 <strong>Coming Soon: Ashlands &amp; Deep North Backpacks</strong></p>
  <p style="margin: 0;">New biome-specific backpacks for the <strong>Ashlands</strong> and <strong>Deep North</strong> are actively in development, featuring custom models, materials, and unique biome status effects!</p>
</blockquote>

---

## 🕹️ Controls & Interaction Modes

| Control Mode | Input / Keybind | Description |
| :--- | :--- | :--- |
| **Toggle Backpack** | <kbd>I</kbd> *(Default)* | Opens or closes your equipped backpack inventory. |
| **Hover Interaction** | *Hover + Hotkey* | Hover over an equipped backpack in your main inventory and press the hotkey to open it directly. |
| **Open with Inventory** | *Configurable* | Automatically opens your backpack inventory whenever you open your player inventory. |
| **Open Alongside Container** | *On by default (fork)* | Opens the backpack in a side panel to the right of inventory; keeps world chests open for dual looting. |
| **Outward Quick Drop** | <kbd>Y</kbd> *(Default)* | Press while moving to immediately detach and drop your backpack behind you to escape danger. |
| **Gamepad / Controller** | 🎮 Native | Full controller navigation support for opening, closing, and managing backpack inventories. |

---

## 🛡️ Advanced Mechanics & Safeguards

* 🛠️ **Craft From Backpack**:
  * **Hammer Building & Stations**: Available resources for building placeables and crafting station recipes take into account both your Player inventory and your currently equipped backpack.
  * **Tiered Consumption**: Materials are consumed according to configurable priority (`Material Consumption Priority`), only drawing from the secondary source for unmet quantities while protecting equipped items.
  * **Leave One in Backpack**: When enabled (`Leave One Item In Backpack`), at least 1 item of each resource type remains in the backpack and is excluded from crafting/building consumption and availability counts.
  * **Craft Output to Backpack**: When enabled and your player inventory is full, newly crafted items are placed directly into your equipped backpack if space is available (strictly excluding backpacks).
* 📥 **Auto Store to Backpack & Inventory Overflow**:
  * **Auto Store Existing Items**: When picking up or looting items that already exist in your equipped backpack, they automatically route directly into the backpack if space is available.
  * **Inventory Overflow**: If your main player inventory is completely full, newly acquired items continue to enter your equipped backpack without triggering "Inventory Full" errors.
  * **Partial Stack Support**: If a backpack only has room for a partial stack, it absorbs what fits and routes the remainder to player inventory.
* ⚙️ **Personal Automation Settings (`Automation (Local Only)`)**:
  * Craft from backpack, consumption priority, leave one item safeguard, craft output overflow, auto-storing, and inventory overflow are client-side personal preferences that can be tailored individually without server synchronization.
* ⚡ **Thor's Inventory Guard (Inception Prevention)**:
  * Backpack-in-backpack nesting is strictly prevented across all crafting, auto-storing, and inventory interactions to safeguard against data corruption and infinite loops.
* 🏷️ **Yard Sale Overflow Safeguard**:
  * If a backpack is resized or upgraded in a way that reduces slots below the stored item count, excess items are cleanly dropped at the player's feet rather than lost.
* 🔀 **Right-Click Quick Transfer (Fast Item Transfer)**:
  * Move single items or entire stacks between your inventory and any open backpack or container with a single right-click.
* 🌀 **Portal & Teleportation Compatibility**:
  * Fully respects Valheim's `Inventory.IsTeleportable()` checks. Non-teleportable ores/items inside backpacks will properly restrict portal usage across all portal mods.
* 🗝️ **Key & Quest Item Recognition**:
  * Keys (such as Swamp Crypt Keys) and quest items in your **equipped backpack** are recognized by locked doors and crypt entrances without moving them to player inventory.
* 🖼️ **Armor Stand & Item Stand Mounting**:
  * Safely mount backpacks on Item Stands and Armor Stands for base decoration without item loss or duplication.

---

## ✨ Status Effects & Passive Perks

* 💪 **Carry Weight Modification**: Increases base carrying capacity per upgrade level.
* ⚖️ **Weight Reduction Multiplier**: Configures how much weight items inside the backpack contribute *(0% = weightless contents; 100% = normal weight)*.
* 🏃 **Speed Modifiers**: Configurable movement speed adjustments per tier that improve as packs are upgraded.
* ❄️ **Frost & Cold Resistance**: Keeps you warm in freezing Mountain biomes.
* 🌧️ **Waterproof**: Prevents the "Wet" debuff in rain and swamp water.
* 🦹 **Troll Armor Set Synergy**: Integrates into the Sneak set bonus when wearing Troll Leather gear.
* 🪶 **Slow Fall / Feather Fall**: Eliminates fatal fall damage when leaping from mountain peaks.
* 💡 **Demister (Wisplight)**:
  * Clears mist in the Mistlands.
  * Configurable toggle keybind (default <kbd>L</kbd>).
  * Includes smart biome logic to automatically stow the light outside the Mistlands.

---

## 🧩 Adventure Backpacks API (ABAPI)

Adventure Backpacks provides two integration pathways for third-party mod developers:

* **Option A (Repackable API Assembly)**: Reference `AdventureBackpacksAPI.dll` at compile time and bundle via `ILRepack`.
* **Option B (Zero-Dependency Client Wrapper)**: Drop [Docs/ABAPI_Client.cs](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/ABAPI_Client.cs) directly into your project for zero-dependency reflection with cached delegates.

### Developer Guides & Resources:
* 🛠️ **[API Integration Guide](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/API_INTEGRATION.md)** — Step-by-step walkthrough for configuring both integration options.
* 📖 **[ABAPI Class Reference](https://github.com/Vapok/AdventureBackpacks/blob/main/Docs/AdventureBackpacksAPI.md)** — Detailed documentation for all classes, methods, and parameters.
* 💾 **[Download AdventureBackpacksAPI.dll](https://github.com/Vapok/AdventureBackpacks/releases)** — Standalone API assembly archives.

---

## 🤝 Verified Mod Compatibility

<div align="center">

| Mod | Author | Compatibility Status | Notes |
| :--- | :--- | :--- | :--- |
| **AzuCraftyBoxes** | Azumatt | 🟢 Fully Supported | Dynamic proxy bridge; draws crafting materials from equipped backpacks |
| **AzuAutoStore** | Azumatt | 🟢 Fully Supported | External container auto-storing routes seamlessly into equipped packs |
| **AzuExtendedPlayerInventory** | Azumatt | 🟢 Fully Supported | Custom equipment slots with automated bone reordering guard |
| **Equipment & Quick Slots (EAQS)** | RandyKnapp | 🟢 Fully Supported | Dedicated UI durability bar alignment & hotkey integration |
| **Extra Slots** | Shudnal | 🟢 Fully Supported | Native compatibility with expanded hotbar and utility slots |
| **Epic Loot** | RandyKnapp | 🟢 Fully Supported | Seamless integration with enchanted gear, loot tables, and weight modifiers |
| **Cheb's Necromancy** | ChebGonaz | 🟢 Fully Supported | Native API integration (*Spectral Shroud of Holding* custom pack & effect) |
| **ContentsWithin** | MSchmoecker | 🟢 Fully Supported | Safe coexistence with world container hover inspection |
| **Multi-User-Chests (MUC)** | MSchmoecker | 🟢 Fully Supported | Concurrent multi-player chest interaction support |
| **Quick Stack, Store, Sort, Trash** | Goldenrevolver | 🟢 Fully Supported | Safe quick-stacking and inventory sorting |
| **ZenUI / ZenDragon ModLib** | ZenDragon | 🟢 Fully Supported | Defensive UI transpiler hooks prevent layout conflicts |
| **Jewelcrafting** | Smoothbrain | 🟢 Fully Supported | Socketing, gems, and dynamic runtime font isolation |
| **Smoothbrain's Skill Mods** | Smoothbrain | 🟢 Fully Supported | Blacksmithing, Building, and crafting progression skills |
| **Seasonality** | RustyMods | 🟢 Fully Supported | Cold resistance protects against winter seasonal freezing and cold debuffs |
| **Seasons** | Shudnal | 🟢 Fully Supported | Seasonal biome temperature shifts and weather effects respect backpack protections |
| **Valheim Plus (Community Fork)** | Grantapher | 🟢 Supported | Non-destructive crafting transpiler interception |

</div>

<blockquote style="background-color: rgba(235, 87, 87, 0.08); border-left: 4px solid #eb5757; padding: 12px 16px; margin: 16px 0; border-radius: 4px;">
  <p style="margin: 0;"><strong>⚠️ NOTE — JotunnBackpacks Incompatibility:</strong> Adventure Backpacks is incompatible with JotunnBackpacks. However, Adventure Backpacks will automatically convert existing JotunnBackpacks saves into new Adventure Backpacks upon loading your character.</p>
</blockquote>

---

## 🌐 Supported Languages

Adventure Backpacks includes localization support for all 35 languages supported by Valheim:

<div align="center">

🌲 **Abenaki** • 🇧🇬 **Bulgarian** • 🇨🇳 **Chinese (Simplified)** • 🇹🇼 **Chinese (Traditional)** • 🇭🇷 **Croatian**  
🇨🇿 **Czech** • 🇩🇰 **Danish** • 🇳🇱 **Dutch** • 🇺🇸 **English** • 🇫🇮 **Finnish**  
🇫🇷 **French** • 🇬🇪 **Georgian** • 🇩🇪 **German** • 🇬🇷 **Greek** • 🇮🇳 **Hindi**  
🇭🇺 **Hungarian** • 🇮🇸 **Icelandic** • 🇮🇹 **Italian** • 🇯🇵 **Japanese** • 🇰🇷 **Korean**  
🇱🇹 **Lithuanian** • 🇲🇰 **Macedonian** • 🇳🇴 **Norwegian** • 🇵🇱 **Polish** • 🇧🇷 **Portuguese (Brazilian)**  
🇵🇹 **Portuguese (European)** • 🇷🇴 **Romanian** • 🇷🇺 **Russian** • 🇷🇸 **Serbian** • 🇸🇰 **Slovak**  
🇪🇸 **Spanish** • 🇸🇪 **Swedish** • 🇹🇭 **Thai** • 🇹🇷 **Turkish** • 🇺🇦 **Ukrainian**

</div>

*Translations can be customized or added in the `Translations/` folder inside your AdventureBackpacks mod directory (`BepInEx/plugins/Vapok-AdventureBackpacks/Translations/`). Some translations updated by AI. Please let me know if you find any issues with the translations.*

---

## 📥 Installation & Server Setup

### Mod Manager (Recommended)
1. Install via **R2ModMan** or **Thunderstore Mod Manager**.
2. Dependencies are automatically installed: `BepInExPack`, `Jotunn (JVL)`, and `YamlDotNet`.

### Dedicated Servers
* **Required on Both Client & Server**: Adventure Backpacks must be present on both the server and all connecting clients.
* **Network Compatibility**: Built-in version checking ensures game-state and inventory consistency across clients.
* **ServerSync**: Server configuration files automatically lock and sync settings down to non-admin players in real-time.

---

## 🔒 Anonymous Telemetry, Error Reporting & Privacy

Adventure Backpacks includes lightweight, privacy-first telemetry and error reporting to help monitor mod stability, diagnose unhandled bugs, and track active version adoption across game updates.

* **100% Anonymous**: We never collect personal data, Steam IDs, IP addresses, character/world names, or file system paths. Stack traces from errors are automatically sanitized to strip local user directories.
* **Granular Player Control**:
  * **Anonymous Telemetry (Opt-In)**: Tracks version adoption and session launches. Defaults to **unchecked / disabled** when first loaded (`Enable Anonymous Telemetry = false`).
  * **Error Reporting (Opt-Out)**: Captures sanitized mod crash diagnostics to rapidly identify and fix bugs. Defaults to **enabled** (`Send Error Reports = true`) with one-click opt-out.
  * **Data Disclaimers**: Hover over any toggle in the startup modal for interactive tooltip disclaimers detailing exactly what data is transmitted.
* **In-Game & Online Privacy Policy**: The full privacy policy can be viewed directly in-game by clicking **`[ PRIVACY POLICY ]`** on the startup splash modal, or online at [vapok.io/privacy-policy](https://vapok.io/privacy-policy/).
* **Configuration Files**: Settings can be managed in-game via the startup modal, through the BepInEx Configuration Manager, or under `[Local Config]` in `BepInEx/config/vapok.mods.adventurebackpacks.cfg`.

---

<div align="center">

### 👨‍💻 Created by Vapok Gaming

[![Vapok Gaming](https://avatars.githubusercontent.com/u/1264136?s=120&v=4)](https://github.com/Vapok)

**Author**: [Vapok](https://github.com/Vapok)  
**Source Code**: [GitHub Repository](https://github.com/Vapok/AdventureBackpacks)  
**Community & Support**: [Discord Server](https://discord.gg/5YAJkRFBXt)  
**Changelog**: [Release Notes](https://github.com/Vapok/AdventureBackpacks/blob/main/CHANGELOG.md)

</div>
