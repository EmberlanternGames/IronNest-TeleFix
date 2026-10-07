# TeleFix

## ***WARNING: Installing mods will disqualify you from leaderboards. Using the mod will do the same. You have been warned.***

## What is this?

There is a bug with the teleprinters that causes it to bug out and become nonresponsive after around 16,383 
  characters in the string. This mod fixes that bug by truncating the strings when they get to a certain length. 

## Requirements

MelonLoader 0.7.3 or newer. Go to https://melonwiki.xyz/ to get the installer and point it at Iron Nest's 
  executable. Then place the mod dll in the `Mods` folder in your Iron Nest install location.

Alternatively, you may also use this with BepInEx - get it from the [bleeding edge page](https://builds.
  bepinex.dev/projects/bepinex_be), the "BepInEx Unity (IL2CPP) for Windows (x64) games" version - but only 
  if you install the [MLLoader mod](https://www.nexusmods.com/ironnest/mods/26) alongside it and place the 
  mod in `MLLoader/Mods`. The `MLLoader` folder is also where you'll find the appropriate `UserData` folder 
  for config settings.

- Note: It will take a while to launch the game when you first load after installing MelonLoader, don't exit 
  if it looks frozen, it's just generating files and will be back shortly. This also happens with game updates.

## Installation

1. Extract the mod into the game folder. If all goes right, the mod file should end up at 
   `[Iron Nest Directory]/Mods/TeleFix.dll`
2. Configure: Check the config file at `[Iron Nest Directory]/UserData/MelonPreferences.cfg` - Enable any 
   Bonfire features you desire and configure them appropriately
3. Test: Load the game and make sure the mod works by testing a feature you activated. Example: If you have 
   BreakGuns enabled, the powder charge delivery system should be limited to [maxCharges] and the elevators 
   for the guns will halt at [maxAngle] degrees.

## Bug Reporting

Please include the following:
- What you were doing that broke things?
- Add `BepInEx/LogOutput.log` with VerboseLogging enabled in the config.

Please submit to the bugs to the Bugs tab in Nexus.

## AI Disclosure

This code is 100% written by a fleshy meatbag (me), but Claude was enlisted to dissect the problem to gain 
  understanding on how to truncate properly (it's not as simple as cutting at character 12000, that's for sure).