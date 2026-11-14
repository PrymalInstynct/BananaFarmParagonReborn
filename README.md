# Banana Farm Paragon (resurrected)

Compatibility repair of [Jonyboylovespie/BananaFarmParagon](https://github.com/Jonyboylovespie/BananaFarmParagon)
for current BTD6 (57.4), MelonLoader 0.7.3 and BTD Mod Helper 3.6.10. Gameplay is unchanged from upstream 1.0.7.

Art credits: PipDragon (texture), Keetsoona (upgrade icon), Connor Conntroller (portrait).
Upstream has no published license.

## Build

Needs the .NET SDK (8.x works), `btd6.targets` from BTD Mod Helper in the parent directory,
and a folder holding the game's `MelonLoader/net6`, `MelonLoader/Il2CppAssemblies` and `Mods/Btd6ModHelper.dll`.

    BloonsTD6=/path/to/BloonsTD6-or-refs dotnet build -c Release -o /tmp/bfp-out

Copy `BananaFarmParagon.dll` from the output into the game's `Mods` folder.
