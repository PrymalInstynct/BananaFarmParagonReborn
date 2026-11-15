# Banana Farm Paragon Reborn

<img src="BananaFarmParagon-Portrait.png" alt="Banana Farm Paragon portrait" width="160" align="right">

A Paragon upgrade for the Banana Farm in Bloons TD 6, rebalanced to sit alongside the vanilla Paragons.

This is a fork of [Banana Farm Paragon](https://github.com/Jonyboylovespie/BananaFarmParagon) by Jonyboylovespie.
The original stopped loading after game updates and was last changed in April 2024. This fork repairs it for the
current game and reworks how the Paragon earns money.

> **Status:** version 2.1.0 builds and installs, but has not yet been played through a full match.
> Treat the numbers below as the design, not as measured results.

## What the Paragon does

**The Pinnacle Of Banana Production** merges Banana Central, Monkey-Nomics and Monkey Wall Street.

| Ability | Effect |
| --- | --- |
| Crates | Produces 10 crates every round. They never rot. |
| Collection | Pulls in its own crates and every other banana on the map, the way Monkey Wall Street does for nearby farms. |
| Research boost | Keeps Banana Central's boost to Banana Research Facilities and applies it to the whole map. |
| Pop cash | Every pop on the map is worth more cash. Stacks with a Monkey Town. |

Income and pop cash grow evenly with Paragon degree, so sacrificing more farms and investing more cash pays off:

| Degree | Income per round | Per crate | Cash per pop |
| ---: | ---: | ---: | ---: |
| 1 | $25,000 | $2,500 | x1.25 |
| 25 | $49,242 | $4,924 | x1.43 |
| 50 | $74,495 | $7,449 | x1.62 |
| 75 | $99,747 | $9,975 | x1.81 |
| 100 | $125,000 | $12,500 | x2.00 |

The upgrade costs $500,000 on Medium and scales with difficulty like any other upgrade. At degree 1 the crates
alone pay that back in about 20 rounds.

## Requirements

- Bloons TD 6 version 57.4
- [MelonLoader](https://github.com/LavaGang/MelonLoader) 0.7.3
- [BTD Mod Helper](https://github.com/gurrenm3/BTD-Mod-Helper) 3.6.10

Like every Paragon, it needs all three tier 5 Banana Farms on the map. Banana Farm is a vanilla tower without a
vanilla Paragon, so no other mod is needed to unlock it.

## Installing

There is no packaged release yet. Build the mod (see [Building](#building)) and copy `BananaFarmParagon.dll`
into the `Mods` folder inside your Bloons TD 6 install, then restart the game.

The file name is the same as the original mod's on purpose. Both mods add a Paragon to the same tower, so only
one of them should be installed.

## Settings

In game, open **Mods**, select **Banana Farm Paragon Reborn** and open its settings.

| Setting | Default | What it does |
| --- | ---: | --- |
| Banana Farm Paragon Cost | 500,000 | Upgrade cost on Medium. |
| Income Per Round at Degree 1 | 25,000 | Total value of the 10 crates at the lowest degree. |
| Income Per Round at Degree 100 | 125,000 | Total value of the 10 crates at the highest degree. |
| OP Mode of Banana Farm Paragon | Off | Ignores the income settings: $10,000,000 per round and x10 cash per pop at any degree. |
| Toggle Popup | Off | Shows a main menu popup saying whether balanced or OP mode is loaded. |

Changes take effect in the next match you start.

## What changed from the original

| | Original 1.0.7 | Reborn 2.1.0 |
| --- | --- | --- |
| Loads on the current game | No | Yes |
| Income per round | $1,000,000 at any degree | $25,000 to $125,000 by degree |
| Cost on Medium | $1,000,000 | $500,000 |
| Cash per pop, map-wide | x10 | x1.25 to x2 by degree |
| Paragon degree | No effect | Scales income and pop cash |
| Banana collection | A hidden helper tower spawned every 100 seconds | Built into the Paragon |
| Banana Central's research boost | Removed | Kept, map-wide |
| Tower range | Set to effectively infinite | Unchanged from the farm |
| OP mode | About 1.8 x 10^19 cash per banana | $10,000,000 per round |

## How it works

- The Paragon starts as a copy of Banana Central and keeps its banana-producing weapon, with the crate count and
  value replaced.
- Collection uses the game's own `CollectCashZoneModel`, copied from Monkey Wall Street and widened to the whole map.
- The pop cash bonus is Monkey Town's `MonkeyCityIncomeSupportModel` with its own mutator id, made global.
- The game's Paragon degree system only scales damage, pierce and attack speed, which do nothing for a farm, and
  Mod Helper 3.6.10 does not call its Paragon degree hooks. The mod therefore checks each Banana Farm Paragon once
  a second and rewrites its income when the degree and the income disagree. It logs a line to the MelonLoader
  console each time it does.

## Known limitations

- Not yet tested through a full match, including saving and reloading.
- A degree change may release one extra batch of crates in that round.
- The original mod's startup warning from Mod Helper (`NullReferenceException` in `ModVanillaParagon.get_Name`)
  still appears in the log. The Paragon registers correctly despite it.
- Mods that also change Paragon degrees or Banana Farm income may interact with the degree scaling.

## Building

You need the .NET SDK (8.x works) and three things from a modded Bloons TD 6 install:

- `btd6.targets`, which Mod Helper generates, placed in the directory above this repository
- the game's `MelonLoader/net6` and `MelonLoader/Il2CppAssemblies` folders
- `Mods/Btd6ModHelper.dll`

Point `BloonsTD6` at the game folder, or at a copy that holds those folders in the same layout:

    BloonsTD6=/path/to/BloonsTD6 dotnet build -c Release -o /tmp/bfp-out

The build also copies the DLL into `$BloonsTD6/Mods`.

## Credits

- Original mod: [Jonyboylovespie](https://github.com/Jonyboylovespie)
- Art in 2.1.0: portrait and upgrade icon generated with ChatGPT image generation to a purple and gold tycoon
  brief; the model texture is the game's own Banana Farm texture sheet recoloured to match.
- Art in the original mod (replaced in 2.1.0): texture by PipDragon, upgrade icon by Keetsoona, tower portrait by
  Connor Conntroller.

The original repository has no published license, so this fork does not add one. It is shared in the same
community spirit as the original, and the credits above stay with it.
