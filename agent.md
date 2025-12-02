
# Mount & Blade II: Bannerlord Modding Agent Instructions

## Repository Overview
This workspace is dedicated to creating and editing mods for **Mount & Blade II: Bannerlord** using TaleWorlds' modding framework and C# .NET development.

## Official Resources

### Primary Documentation
- **TaleWorlds Modding Documentation**: https://docs.bannerlordmodding.com/
- **Official Modding Kit**: Available via Steam Tools (Mount & Blade II: Bannerlord - Modding Kit)
- **TaleWorlds Forums**: https://forums.taleworlds.com/index.php?forums/modding.722/
- **Bannerlord Docs**: https://apidoc.bannerlord.com/ (API reference)

### Community Resources
- **Nexus Mods**: https://www.nexusmods.com/mountandblade2bannerlord
- **ModDB**: https://www.moddb.com/games/mount-blade-ii-bannerlord/mods
- **Bannerlord Modding Discord**: Active community support

## Development Environment

### Required Tools
- **Visual Studio 2022** (Community Edition or higher)
- **.NET Framework 4.7.2** (minimum SDK version)
- **Bannerlord Game Files** (Steam installation required)
- **Bannerlord Modding Kit** (install via Steam)
- **Harmony Library** (for runtime patching): https://github.com/pardeike/Harmony

### Project Structure
```
ModName/
├── bin/
│   └── Win64_Shipping_Client/  # Compiled DLLs go here
├── SubModule.xml               # Mod manifest (REQUIRED)
├── ModuleData/
│   ├── Languages/
│   ├── module_strings.xml      # Localization strings
│   └── *.xml                   # Game data files (items, troops, etc.)
├── GUI/
│   └── Prefabs/                # UI prefabs
├── SceneObj/                   # 3D scene objects
└── src/                        # C# source files
    └── SubModule.cs            # Entry point class
```

## Critical Conventions

### SubModule.xml Format
**Every mod MUST have a SubModule.xml** in the root directory:
```xml
<Module>
    <Name value="Mod Name"/>
    <Id value="ModName"/>
    <Version value="v1.0.0"/>
    <SingleplayerModule value="true"/>
    <MultiplayerModule value="false"/>
    <DependedModules>
        <DependedModule Id="Native"/>
        <DependedModule Id="SandBoxCore"/>
        <DependedModule Id="Sandbox"/>
        <DependedModule Id="StoryMode" Optional="true"/>
    </DependedModules>
    <SubModules>
        <SubModule>
            <Name value="ModName"/>
            <DLLName value="ModName.dll"/>
            <SubModuleClassType value="ModName.SubModule"/>
            <Tags>
                <Tag key="DedicatedServerType" value="none"/>
                <Tag key="IsNoRenderModeElement" value="false"/>
            </Tags>
        </SubModule>
    </SubModules>
</Module>
```

### SubModule.cs Entry Point
**All mods require a class inheriting from `MBSubModuleBase`**:
```csharp
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace ModName
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            // Initialization code
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            // Game start logic
        }
    }
}
```

### Harmony Patching Pattern
**Use Harmony for non-invasive code injection** (prefix/postfix/transpiler):
```csharp
using HarmonyLib;
using TaleWorlds.CampaignSystem;

[HarmonyPatch(typeof(Campaign), "OnGameLoaded")]
public class CampaignPatch
{
    static void Postfix(Campaign __instance)
    {
        // Code runs after Campaign.OnGameLoaded
    }
}

// Apply in OnSubModuleLoad:
var harmony = new Harmony("com.modauthor.modname");
harmony.PatchAll();
```

## Key TaleWorlds Namespaces

### Core Gameplay
- `TaleWorlds.Core` - Base game mechanics (items, equipment, damage)
- `TaleWorlds.MountAndBlade` - Combat, agents, missions
- `TaleWorlds.CampaignSystem` - Campaign map, parties, settlements
- `TaleWorlds.CampaignSystem.Party` - Party management
- `TaleWorlds.CampaignSystem.Actions` - High-level campaign actions

### UI & Screens
- `TaleWorlds.ScreenSystem` - Screen management
- `TaleWorlds.GauntletUI` - UI framework
- `TaleWorlds.Library` - Utility classes (MathF, Vec3, etc.)

### Data Models
- `TaleWorlds.ObjectSystem` - Game object system
- `TaleWorlds.Localization` - Text localization (`TextObject` class)

## Best Practices

### Code Style
- **Namespace convention**: `YourName.ModName` or `ModName`
- **Use `TextObject`** for all player-facing strings (enables localization)
- **Null checks**: Bannerlord can have null references; always validate
- **Performance**: Avoid heavy operations in frequently-called methods (OnTick, etc.)

### Data Modification
**Prefer CampaignBehaviors over direct patches**:
```csharp
public class CustomBehavior : CampaignBehaviorBase
{
    public override void RegisterEvents()
    {
        CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
    }

    private void OnSessionLaunched(CampaignGameStarter starter)
    {
        // Add game menus, dialogs, etc.
    }

    public override void SyncData(IDataStore dataStore)
    {
        // Save/load custom data
    }
}
```

### XML Data Files
- **Items**: `ModuleData/items.xml` (weapons, armor)
- **Troops**: `ModuleData/troops.xml` (NPCs, soldiers)
- **Cultures**: `ModuleData/cultures.xml`
- **Follow vanilla XML schema** - reference Native module files

### Debugging
- **Enable developer mode**: Launch with `/debug` parameter
- **Use `InformationManager.DisplayMessage`** for runtime logging
- **Attach Visual Studio debugger** to TaleWorlds.Bannerlord.exe process

## Versioning & Compatibility

### Game Version Targeting
- Check `ApplicationVersion` in code for compatibility
- Tag releases with supported game version (e.g., "e1.2.9")
- **Breaking changes occur frequently** - test after game updates

### Load Order
- Dependencies listed in `<DependedModules>` control load order
- Native modules load first (Native → SandBoxCore → Sandbox → StoryMode)
- Mods load alphabetically unless dependencies specified

## Common Modding Tasks

### Adding Custom Items
1. Create `items.xml` in `ModuleData/`
2. Define item using vanilla schema
3. Reference in crafting recipes or add to merchant inventories via code

### Creating Custom Troops
1. Define in `ModuleData/troops.xml`
2. Reference equipment from items.xml
3. Add to spawn lists via `CampaignBehavior` or XML patches

### UI Modifications
1. Create Gauntlet ViewModel (C# class extending `ViewModel`)
2. Create XML prefab in `GUI/Prefabs/`
3. Bind ViewModel to Screen via `ScreenBase` inheritance

### Save Game Compatibility
- Implement `SyncData(IDataStore dataStore)` in CampaignBehaviors
- Use `[SaveableField]` attribute for custom properties
- Test save/load cycles thoroughly

## Distribution

### Packaging
- Include **SubModule.xml** in root
- Compiled DLLs in `bin/Win64_Shipping_Client/`
- All XML data in `ModuleData/`
- Optional: Include source in separate folder

### Publishing Platforms
1. **Nexus Mods** (primary distribution)
2. **Steam Workshop** (official but limited)
3. **ModDB** (alternative host)
4. Include clear installation instructions and dependencies
