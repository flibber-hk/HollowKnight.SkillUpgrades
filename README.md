# HollowKnight.SkillUpgrades

Mod that adds toggleable upgrades and tweaks to some of the Knight's skills.

Includes the following:

- **Multiple Air Dash**: Dash more than once before landing.
- **Directional Dash**: Dash in the 8 cardinal directions.
- **Wall Climb**: Climb walls without having to jump. Hold up or down to move vertically while wall-clinging.
- **Multiple Wings**: Use wings more than once in the air.
- **Wings Glide**: Continue holding jump after a wings jump to glide down slowly.
- **Vertical Cdash**: Cdash vertically and diagonally. Hold up when releasing a ground cdash charge to cdash upwards, hold up and forward to cdash diagonal. Hold up or down when releasing a wall cdash charge to cdash diagonally up or down.
- **Downward Fireball**: Shoot fireballs downward. Hold no direction when casting to send a fireball downwards (or hold left/right to shoot normally).
- **Horizontal Dive**: Dive to the left or right. Hold left or right as well as down when casting to dive horizontally.
- **Spiral Scream**: Scream in a circle around the knight. Hold left or right as well as up when casting to cause the wraiths/shriek to sweep a circle around the knight.

Also includes a global toggle - if this is off, all upgrades will be disabled. Each upgrade, as well as the global toggle, can be toggled individually from the mod menu. 

## Global settings
Manually editing the global settings file (while the game is closed) can let the player tweak some of the properties of the skill upgrades. These options include:
- UnmodifiedDownDashes: if this is set to true, down dashes will behave normally (so no down diagonal dashes, and down dashes require dashmaster to be equipped).
- MaintainVerticalDashMomentum: By default, the knight will continue moving vertically after an upward or up-diagonal dash ends (slowed down by gravity); this behaviour can be disabled, to match how the knight stops moving horizontally with no left/right input after a regular dash.
- DiagonalSuperdash: If this is disabled, the knight will only be able to cdash upward or horizontally (no diagonal).
- ClimbSpeed: The default climb speed is 5.0, but this can be modified.
- AirDashMax, DoubleJumpMax: The number of times the knight can air dash or double jump can be changed - setting this value to be negative will cause it to be treated as infinite.
- GlideFallSpeedMultiplier: Multiply the knight's maximum downward velocity by this when gliding.

Some of these can be modified in the mod menu.

## DebugMod interop
With DebugMod installed, each skill upgrade, as well as the global toggle, can be toggled in the debug mod keybinds menu (and these toggles can be bound to keys like with normal DebugMod methods).

## Randomizer interop
Each of the Skill Upgrades can be randomized in Randomizer 4's Connections page. 

There are four options:
- None - no skill upgrades will be randomized, and they will be active if and only if they are active in the mod menu.
- All - each skill upgrade will be placed.
- RandomSelection - a random selection of skill upgrades will be placed.
- EnabledSkills - the skill upgrades which are currently enabled in the mod menu will be placed. This setting ignores the global toggle.

Limitations:
- Skill upgrades are not progressive (so e.g. it is possible to find a useless extra air dash before finding any dash).
- Skill upgrades unlock the ability to toggle in the menu (so finding wall climb will have no effect if it is turned off in the mod menu).
- Skill upgrades do not give specialized logic access.
- Unrandomized skill upgrades will behave according to the global setting - so they will be available as soon as the base skill is unlocked if the skill upgrade is active in the mod menu.

Skill Upgrades will, by default, be placed in the same item group as Mothwing Cloak (even the upgrades not related to dash). Any Custom Group that affects skills differently to each other should match skill upgrades too, if intended.