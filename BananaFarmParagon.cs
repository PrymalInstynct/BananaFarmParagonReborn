using System;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Api.Helpers;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using MelonLoader;
using ModHelperData = BananaFarmParagon.ModHelperData;

[assembly: MelonInfo(typeof(BananaFarmParagon.Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.Author)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BananaFarmParagon
{
    public class Main : BloonsTD6Mod
    {
        private int frame;
        private bool syncErrorLogged;

        public override void OnNewGameModel(GameModel gameModel)
        {
            var upgrade = gameModel.GetParagonUpgradeForTowerId(TowerType.BananaFarm);
            if (upgrade != null)
            {
                upgrade.cost = CostHelper.CostForDifficulty(Settings.ParagonCost, gameModel);
            }

            // Settings can change between matches, so re-tune the degree 1 model for each new match.
            foreach (var towerModel in gameModel.towers)
            {
                if (Balance.IsFarmParagon(towerModel))
                {
                    Balance.Apply(towerModel, 1);
                }
            }
        }

        /// <summary>
        /// The game's degree mutator only scales damage, pierce and attack speed, none of which matter to a farm.
        /// Once a second, bring each farm paragon's income in line with its current degree.
        /// </summary>
        public override void OnUpdate()
        {
            if (++frame % 60 != 0) return;

            try
            {
                var inGame = InGame.instance;
                if (inGame == null || inGame.bridge == null) return;

                foreach (var tower in inGame.GetTowers())
                {
                    SyncDegree(tower);
                }
            }
            catch (Exception e)
            {
                if (syncErrorLogged) return;
                syncErrorLogged = true;
                ModHelper.Warning<Main>("Could not sync the Banana Farm Paragon's degree: " + e);
            }
        }

        private static void SyncDegree(Tower tower)
        {
            var current = tower.towerModel;
            if (current == null || !Balance.IsFarmParagon(current)) return;

            var paragon = tower.GetTowerBehavior<ParagonTower>();
            if (paragon == null) return;

            var degree = paragon.GetCurrentDegree();
            if (degree < 1) return;

            var root = tower.rootModel.Cast<TowerModel>();
            if (Balance.Matches(root, degree)) return;

            var updated = root.Duplicate();
            Balance.Apply(updated, degree);
            tower.UpdateRootModel(updated);

            ModHelper.Msg<Main>($"Banana Farm Paragon degree {degree}: " +
                                $"${Balance.IncomePerRound(degree):N0} per round, " +
                                $"x{Balance.PopCashMultiplier(degree):0.00} cash per pop.");
        }

        public override void OnMainMenu()
        {
            var mode = Settings.ParagonTowerOp ? "OP" : "balanced";
            var message = $"The {mode} version of the banana farm paragon has been loaded.";
            ModHelper.Msg<Main>(message);
            if (Settings.TogglePopup)
            {
                PopupScreen.instance.ShowOkPopup(message);
            }
        }
    }

    public class Settings : ModSettings
    {
        public static readonly ModSettingBool ParagonTowerOp = new(false)
        {
            displayName = "OP Mode of Banana Farm Paragon",
            description = "Ignores the balanced income settings: $10,000,000 per round and 10x cash per pop at any degree.",
            button = true
        };

        public static readonly ModSettingBool TogglePopup = new(false)
        {
            displayName = "Toggle Popup",
            description = "Toggles the popup on main menu that tells you what version of the banana farm paragon you are on",
            button = true
        };

        public static readonly ModSettingInt ParagonCost = new(500000)
        {
            displayName = "Banana Farm Paragon Cost",
            description = "Cost on Medium difficulty.",
            min = 0
        };

        public static readonly ModSettingInt BaseIncome = new(25000)
        {
            displayName = "Income Per Round at Degree 1",
            min = 0
        };

        public static readonly ModSettingInt MaxIncome = new(125000)
        {
            displayName = "Income Per Round at Degree 100",
            min = 0
        };
    }

    /// <summary>
    /// The numbers that depend on paragon degree, and how they are written onto a tower model.
    /// </summary>
    public static class Balance
    {
        public const int CratesPerRound = 10;
        public const float BasePopCash = 1.25f;
        public const float MaxPopCash = 2f;

        private const float OpIncome = 10000000;
        private const float OpPopCash = 10;

        public static bool IsFarmParagon(TowerModel towerModel) =>
            towerModel.isParagon && towerModel.baseId == TowerType.BananaFarm;

        private static float Progress(int degree) => Math.Clamp((degree - 1) / 99f, 0f, 1f);

        private static float Lerp(float from, float to, float progress) => from + (to - from) * progress;

        public static float IncomePerRound(int degree) => Settings.ParagonTowerOp
            ? OpIncome
            : Lerp(Settings.BaseIncome, Settings.MaxIncome, Progress(degree));

        public static float PopCashMultiplier(int degree) => Settings.ParagonTowerOp
            ? OpPopCash
            : Lerp(BasePopCash, MaxPopCash, Progress(degree));

        private static float CrateValue(int degree) => MathF.Round(IncomePerRound(degree) / CratesPerRound);

        public static void Apply(TowerModel towerModel, int degree)
        {
            var weapon = towerModel.GetAttackModel().weapons[0];
            weapon.GetBehavior<EmissionsPerRoundFilterModel>().count = CratesPerRound;

            var cash = weapon.projectile.GetBehavior<CashModel>();
            cash.minimum = CrateValue(degree);
            cash.maximum = CrateValue(degree);

            towerModel.GetBehavior<MonkeyCityIncomeSupportModel>().incomeModifier = PopCashMultiplier(degree);
        }

        public static bool Matches(TowerModel towerModel, int degree)
        {
            var weapon = towerModel.GetAttackModel().weapons[0];
            var cash = weapon.projectile.GetBehavior<CashModel>();
            var popCash = towerModel.GetBehavior<MonkeyCityIncomeSupportModel>();

            return weapon.GetBehavior<EmissionsPerRoundFilterModel>().count == CratesPerRound &&
                   Math.Abs(cash.minimum - CrateValue(degree)) < 0.5f &&
                   Math.Abs(cash.maximum - CrateValue(degree)) < 0.5f &&
                   Math.Abs(popCash.incomeModifier - PopCashMultiplier(degree)) < 0.001f;
        }
    }

    public class Paragon
    {
        public class BananaFarmParagon : ModVanillaParagon
        {
            public override string BaseTower => "BananaFarm-500";
        }

        public class BananaFarmParagonUpgrade : ModParagonUpgrade<BananaFarmParagon>
        {
            public override int Cost => 500000;

            public override string Description =>
                "Crates of bananas every round that collect themselves, along with every other banana on the map. " +
                "All Banana Research Facilities are boosted and every pop is worth more cash. " +
                "Income grows with Paragon degree.";

            public override string DisplayName => "The Pinnacle Of Banana Production";

            public override string Icon => "BananaFarmParagon-Icon";
            public override string Portrait => "BananaFarmParagon-Portrait";

            public override void ApplyUpgrade(TowerModel towerModel)
            {
                var gameModel = Game.instance.model;
                var weapon = towerModel.GetAttackModel().weapons[0];

                // Crates wait to be collected instead of rotting, as in the original mod.
                // A lifespan of 0 expires each crate the moment it spawns, so give them a long one instead.
                var age = weapon.projectile.GetBehavior<AgeModel>();
                ModHelper.Msg<Main>($"Vanilla crate age: lifespan {age.Lifespan}s, rounds {age.rounds}, " +
                                    $"useRoundTime {age.useRoundTime}; weapon rate {weapon.Rate}, " +
                                    $"fireWithoutTarget {weapon.fireWithoutTarget}");
                age.Lifespan = 9999;

                // Banana Central's boost to Banana Research Facilities, now map-wide.
                var central = towerModel.GetBehavior<BananaCentralBuffModel>();
                central.isGlobalRange = true;

                // Monkey Wall Street's banana collection, now map-wide.
                var zone = gameModel.GetTowerFromId("BananaFarm-005").GetBehavior<CollectCashZoneModel>().Duplicate();
                ModHelper.Msg<Main>($"Wall Street collection: attract {zone.attractRange}, collect {zone.collectRange}, " +
                                    $"lerp {zone.lerpMultiplier}, filter '{zone.towerFilter}', towerRange {zone.useTowerRange}, " +
                                    $"camoOnly {zone.onlyAttractsCamo}, wait {zone.minEmitWaitTime}");
                zone.name = "CollectCashZoneModel_BananaFarmParagon";
                zone.attractRange = 9999;
                zone.useTowerRange = false;
                zone.onlyAttractsCamo = false;
                zone.animateTower = false;
                zone.rotate = false;
                towerModel.AddBehavior(zone);

                // Monkey Town's bonus cash per pop, now map-wide. Its own mutator id lets it stack with a real Monkey Town.
                var popCash = gameModel.GetTowerFromId("MonkeyVillage-004").GetBehavior<MonkeyCityIncomeSupportModel>().Duplicate();
                var cash = weapon.projectile.GetBehavior<CashModel>();
                ModHelper.Msg<Main>($"Vanilla values: Banana Central {weapon.GetBehavior<EmissionsPerRoundFilterModel>().count} x " +
                                    $"${cash.minimum}-{cash.maximum}, research buff x{central.multiplier}, " +
                                    $"Monkey Town pop cash x{popCash.incomeModifier}");
                popCash.name = "MonkeyCityIncomeSupportModel_BananaFarmParagon";
                popCash.uniqueMutatorId = "BananaFarmParagonIncome";
                popCash.isGlobal = true;
                popCash.showBuffIcon = false;
                towerModel.AddBehavior(popCash);

                Balance.Apply(towerModel, 1);
            }

            public class BananaFarmParagonDisplay : ModTowerDisplay<BananaFarmParagon>
            {
                public override string BaseDisplay => GetDisplay(TowerType.BananaFarm, 4, 0, 0);

                public override bool UseForTower(int[] tiers) => IsParagon(tiers);

                public override void ModifyDisplayNode(UnityDisplayNode node)
                {
                    SetMeshTexture(node, "texture1");
                }
            }
        }
    }
}
