using UnityEngine;
using Visvang.Fish;
using Visvang.Bait;
using Visvang.Equipment;

namespace Visvang.Data
{
    /// <summary>
    /// Loads all game data from factory classes into the runtime databases.
    /// Call LoadAll() after all managers are initialized.
    /// </summary>
    public static class RuntimeDataLoader
    {
        public static void LoadAll()
        {
            LoadFish();
            LoadDips();
            LoadEquipment();
            LoadStarterInventory();
        }

        private static void LoadFish()
        {
            var db = FishDatabase.Instance;
            if (db == null) return;

            db.RegisterFish(FishDataFactory.CreateCommonCarp());
            db.RegisterFish(FishDataFactory.CreateMirrorCarp());
            db.RegisterFish(FishDataFactory.CreateLeatherCarp());
            db.RegisterFish(FishDataFactory.CreateGhostCarp());
            db.RegisterFish(FishDataFactory.CreateWildSmallCarp());
            db.RegisterFish(FishDataFactory.CreateBoknesGoldenCarp());
            db.RegisterFish(FishDataFactory.CreateBarbel());
            db.RegisterFish(FishDataFactory.CreateFlatNoseRiverBarber());
            db.RegisterFish(FishDataFactory.CreateMudfish());
            db.RegisterFish(FishDataFactory.CreateKurper());
            db.RegisterFish(FishDataFactory.CreateTilapia());
            db.RegisterFish(FishDataFactory.CreateBass());
            db.RegisterFish(FishDataFactory.CreateGraskarp());
            db.RegisterFish(FishDataFactory.CreateYellowfish());
            db.RegisterFish(FishDataFactory.CreateEel());

            Debug.Log($"[Visvang] Loaded {db.FishCount} fish species");
        }

        private static void LoadDips()
        {
            var bm = BaitManager.Instance;
            if (bm == null) return;

            // Barbel attractors
            bm.RegisterDip(DipDataFactory.CreateGarlic());
            bm.RegisterDip(DipDataFactory.CreateBloodworm());
            bm.RegisterDip(DipDataFactory.CreateDevilsFork());

            // All-rounder
            bm.RegisterDip(DipDataFactory.CreateFX());

            // Mudfish attractors (cheap/beginner)
            bm.RegisterDip(DipDataFactory.CreateCheapSweetcorn());
            bm.RegisterDip(DipDataFactory.CreateVanilla());
            bm.RegisterDip(DipDataFactory.CreateBanjo());
            bm.RegisterDip(DipDataFactory.CreatePinkSweets());

            // Mudfish repellents (premium)
            bm.RegisterDip(DipDataFactory.CreateBlackMagic());
            bm.RegisterDip(DipDataFactory.CreateSyntheticAttractor());
            bm.RegisterDip(DipDataFactory.CreateCompetitionPremium());

            Debug.Log($"[Visvang] Loaded {bm.AllDips.Count} dips");
        }

        private static void LoadEquipment()
        {
            var em = EquipmentManager.Instance;
            if (em == null) return;

            // Entry rods
            em.AddRod(RodDataFactory.CreateMakroPlasticSpecial());
            em.AddRod(RodDataFactory.CreateBentSteelRod());
            em.AddRod(RodDataFactory.CreateBudgetFibreglass());

            // Entry reel
            em.AddReel(RodDataFactory.CreateBudgetReel());

            Debug.Log("[Visvang] Loaded starter equipment");
        }

        private static void LoadStarterInventory()
        {
            var bm = BaitManager.Instance;
            if (bm == null) return;

            // Give player starter dips
            foreach (var dip in bm.AllDips)
            {
                if (dip.isStarterDip)
                    bm.PurchaseDip(dip, 10);
            }

            // Equip starter gear
            var em = EquipmentManager.Instance;
            if (em == null) return;

            var rods = em.GetOwnedRods();
            var reels = em.GetOwnedReels();

            if (rods.Count > 0) em.EquipRod(rods[0]);
            if (reels.Count > 0) em.EquipReel(reels[0]);
        }
    }
}
