# HollowKnight.SkillUpgrades

Mod that upgrades some skills. Includes:

- Multiple Air Dash: Dash more than once before landing.
- Directional Dash: Dash in the 8 cardinal directions.
- Wall Climb: Climb walls without having to jump. Hold up or down to move vertically while wall-clinging.
- Multiple Wings: Use wings more than once in the air.
- Vertical Cdash: Cdash vertically and diagonally. Hold up when releasing a ground cdash charge to cdash upwards, hold up and forward (or just up from a wall) to cdash diagonal.
- Downward Fireball: Shoot fireballs downward. Hold no direction when casting to send a fireball downwards (or hold left/right to shoot normally).
- Horizontal Dive: Dive to the left or right. Hold left or right as well as down when casting to dive horizontally. This can lead to the player clipping out of bounds, so be careful.
- Spiral Scream: Scream in a circle around the knight. Hold left or right as well as up when casting to cause the wraiths/shriek to sweep a circle around the knight.

Also includes a global toggle - if this is off, all upgrades will be disabled. Each upgrade, as well as the global toggle, can be toggled individually from the mod menu. The use case for this would be disabling the mod temporarily without losing the skills' individual settings (this is equivalent to toggling the mod off, though for technical reasons having a proper toggle button would be impossible).

## Global settings
Manually editing the global settings file (while the game is closed) can let the player tweak some of the properties of the skill upgrades. These options include:
- AllowDownDiagonalDashes: if this is set to false, dashing down-diagonally is banned, and the player will dash in the direction they would if they had dashmaster on (so left/right usually, or down if down is held but neither left nor right)
- MaintainVerticalDashMomentum: By default, the knight will continue moving vertically after an upward or up-diagonal dash ends (slowed down by gravity); this behaviour can be disabled, to match how the knight stops moving horizontally with no left/right input after a regular dash
- DiagonalSuperdash: If this is disabled, the knight will only be able to cdash upward or horizontally (no diagonal).
- ClimbSpeed: The default climb speed is 5.0, but this can be modified.
- AirDashMax, DoubleJumpMax: The number of times the knight can air dash or double jump can be changed - setting this value to -1 will cause it to be treated as infinite.

Some of the skills may conflict with other mods; if their value in the EnabledModules dictionary is set to null, the skill will simply not be loaded (and not be toggleable in-game). 
