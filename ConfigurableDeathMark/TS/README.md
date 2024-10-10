# Configurable Death Mark

Configurable Death Mark allows you to decide what Damage Over Times (Dot) and what Debuffs can proc the Death Mark Effect.

The mod can work without R2API, but it has built in support for Custom DOTs added by DotAPI.

Everything is handled in the Configuration Files created by the mod, found under `BepInEx/config/ConfigurableDeathMark`.

## Features:

* Allows you to choose what Buffs marked as Debuffs count towards proccing Via config files.
* Works with Mods that add Debuffs.
* Allows you to choose what Dot's count towards proccing via Config Files.
* Compatible with DotAPI
* Knowing what Dot youre modifying may be hard for modded DOTs due to the DotIndex enum value not having a name.
* Automatically Detects what Dots with associated Buffs have said Buff set to IsDebuff and sets the config value for that buff as False.

## Wait, Whats up with that last feature? (Dev speak first paragraph, second is TL;DR)

I've seen certain mods that add custom DOT's via DotAPI which have associated buffs, but said Buffs have the isDebuff flag set to true. This by itself is an issue with how the Code for DeathMark works in vanilla.
By default, DeathMark checks first for the DebuffIndices (Which is an Array of BuffIndex, which is set from the Buffdefs that have the IsDebuff set to true.) Adds 1 to a number (we will call this number "x")if the body has the buff, and then Checks for DOT's, with the DOT logic, it only checks if the body's DotController has the DOT active. without checking if the previous code has already added 1 to x. Causing the DOT itself to count as "2 status effects" for the proccing of DeathMark. Causing the DeathMark to proc sooner than later.

TL;DR: Dot has buff, buff is debuff, counts as 2 stats effects instead of 1.

## Are you aware that you're releasing a mod 6 days before the DLC Survivors of the Void launches on March 1st 2022 (Otherwise known as CUM2 day)?

Yes.

## Credits:

* Borbo for the original idea.
* IHarb, RandomlyAwesome and Bubbet for helping with the ILCode.

## Changelog:

### '1.0.0'

* First Release