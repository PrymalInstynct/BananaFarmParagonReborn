# Banana Farm Paragon Reborn

A Paragon upgrade for the Banana Farm, for current BTD6 (57.4), MelonLoader 0.7.3 and BTD Mod Helper 3.6.10.

Forked from [Jonyboylovespie/BananaFarmParagon](https://github.com/Jonyboylovespie/BananaFarmParagon), which stopped
loading after game updates. This version repairs it and rebalances it to sit alongside the vanilla Paragons.

## What the Paragon does

- Produces 10 crates every round. Total income is $25,000 per round at degree 1, rising evenly to $125,000 at degree 100.
- Collects its own crates and every other banana on the map.
- Keeps Banana Central's boost to Banana Research Facilities and applies it map-wide.
- Raises cash per pop map-wide: x1.25 at degree 1, rising to x2 at degree 100. Stacks with a Monkey Town.
- Costs $500,000 on Medium.

Cost and both income figures can be changed in mod settings. OP mode ignores them and gives $10,000,000 per round
and x10 cash per pop at any degree.

## Credits

Original mod by Jonyboylovespie. Art by PipDragon (texture), Keetsoona (upgrade icon) and Connor Conntroller (portrait).
Upstream has no published license.

## Build

Needs the .NET SDK (8.x works), `btd6.targets` from BTD Mod Helper in the parent directory,
and a folder holding the game's `MelonLoader/net6`, `MelonLoader/Il2CppAssemblies` and `Mods/Btd6ModHelper.dll`.

    BloonsTD6=/path/to/BloonsTD6-or-refs dotnet build -c Release -o /tmp/bfp-out

Copy `BananaFarmParagon.dll` from the output into the game's `Mods` folder.
