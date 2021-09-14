# HollowKnight.SkillUpgrades

Mod that adds toggleable upgrades and tweaks to some of the Knight's skills.

Includes the following:

- **Multiple Air Dash**: Dash more than once before landing.
- **Directional Dash**: Dash in the 8 cardinal directions.
- **Wall Climb**: Climb walls without having to jump. Hold up or down to move vertically while wall-clinging.
- **Multiple Wings**: Use wings more than once in the air.
- **Vertical Cdash**: Cdash vertically and diagonally. Hold up when releasing a ground cdash charge to cdash upwards, hold up and forward to cdash diagonal. Hold up or down when releasing a wall cdash charge to cdash diagonally up or down.
- **Downward Fireball**: Shoot fireballs downward. Hold no direction when casting to send a fireball downwards (or hold left/right to shoot normally).
- **Horizontal Dive**: Dive to the left or right. Hold left or right as well as down when casting to dive horizontally. This can lead to the player clipping out of bounds, so be careful.
- **Spiral Scream**: Scream in a circle around the knight. Hold left or right as well as up when casting to cause the wraiths/shriek to sweep a circle around the knight.

Also includes a global toggle - if this is off, all upgrades will be disabled. Each upgrade, as well as the global toggle, can be toggled individually from the mod menu. 

## Global settings
Manually editing the global settings file (while the game is closed) can let the player tweak some of the properties of the skill upgrades. These options include:
- AllowDownDiagonalDashes: if this is set to false, dashing down-diagonally is banned, and the player will dash in the direction they would if they had dashmaster on (so left/right usually, or down if down is held but neither left nor right)
- MaintainVerticalDashMomentum: By default, the knight will continue moving vertically after an upward or up-diagonal dash ends (slowed down by gravity); this behaviour can be disabled, to match how the knight stops moving horizontally with no left/right input after a regular dash
- DiagonalSuperdash: If this is disabled, the knight will only be able to cdash upward or horizontally (no diagonal).
- ClimbSpeed: The default climb speed is 5.0, but this can be modified.
- AirDashMax, DoubleJumpMax: The number of times the knight can air dash or double jump can be changed - setting this value to -1 will cause it to be treated as infinite.

Some of the skill upgrades require substantial changes to some of the knight's components to function, and these changes can't easily be reversed. In order not to conflict with other mods, it is possible to prevent those upgrades from loading entirely by setting the value in the EnabledModules section of the global settings to `null`. Any skill upgrade disabled in this way will not be able to be toggled in-game.
