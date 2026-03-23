using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Visvang.Core;

namespace Visvang.Bait
{
    /// <summary>
    /// Manages bait and dip inventory, selection, and purchase.
    /// </summary>
    public class BaitManager : MonoBehaviour
    {
        public static BaitManager Instance { get; private set; }

        [Header("Available Items")]
        [SerializeField] private List<BaitData> allBaits = new List<BaitData>();
        [SerializeField] private List<DipData> allDips = new List<DipData>();

        [Header("Inventory")]
        [SerializeField] private List<BaitInventoryItem> baitInventory = new List<BaitInventoryItem>();
        [SerializeField] private List<DipInventoryItem> dipInventory = new List<DipInventoryItem>();

        [Header("Active Selection")]
        [SerializeField] private BaitData selectedBait;
        [SerializeField] private DipData selectedDip;

        public BaitData SelectedBait => selectedBait;
        public DipData SelectedDip => selectedDip;
        public List<BaitInventoryItem> BaitInventory => baitInventory;
        public List<DipInventoryItem> DipInventory => dipInventory;

        public List<DipData> AllDips => allDips;

        public event System.Action<BaitData> OnBaitSelected;
        public event System.Action<DipData> OnDipSelected;

        public void RegisterDip(DipData dip)
        {
            if (!allDips.Contains(dip))
                allDips.Add(dip);
        }

        public void RegisterBait(BaitData bait)
        {
            if (!allBaits.Contains(bait))
                allBaits.Add(bait);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SelectBait(BaitData bait)
        {
            selectedBait = bait;
            OnBaitSelected?.Invoke(bait);
        }

        public void SelectDip(DipData dip)
        {
            selectedDip = dip;
            OnDipSelected?.Invoke(dip);

            // Apply to pap system
            PapSystem.Instance?.ApplyDip(dip);
        }

        public bool PurchaseBait(BaitData bait, int quantity)
        {
            var existing = baitInventory.FirstOrDefault(b => b.bait == bait);
            if (existing != null)
            {
                existing.quantity += quantity;
            }
            else
            {
                baitInventory.Add(new BaitInventoryItem { bait = bait, quantity = quantity });
            }
            return true;
        }

        public bool PurchaseDip(DipData dip, int quantity)
        {
            var existing = dipInventory.FirstOrDefault(d => d.dip == dip);
            if (existing != null)
            {
                existing.quantity += quantity;
            }
            else
            {
                dipInventory.Add(new DipInventoryItem { dip = dip, quantity = quantity });
            }
            return true;
        }

        public void ConsumeBait()
        {
            if (selectedBait == null) return;

            var item = baitInventory.FirstOrDefault(b => b.bait == selectedBait);
            if (item != null)
            {
                item.quantity--;
                if (item.quantity <= 0)
                    baitInventory.Remove(item);
            }
        }

        public void ConsumeDip()
        {
            if (selectedDip == null) return;

            var item = dipInventory.FirstOrDefault(d => d.dip == selectedDip);
            if (item != null)
            {
                item.quantity--;
                if (item.quantity <= 0)
                    dipInventory.Remove(item);
            }
        }

        public List<DipData> GetDipsByCategory(DipCategory category)
        {
            return allDips.Where(d => d.category == category).ToList();
        }

        public List<DipData> GetBarbelDips()
        {
            return allDips.Where(d => d.AttractsBarbelStrongly()).ToList();
        }

        public List<DipData> GetAntiMudfishDips()
        {
            return allDips.Where(d => d.RepelsMudfish()).ToList();
        }

        public string GetDipWarning(DipData dip)
        {
            if (dip.AttractsBarbelStrongly())
                return "Warning: This dip attracts barbel. Prepare for chaos, my bru!";
            if (dip.AttractsMudfishStrongly())
                return "Eish, this one brings the mudfish. You've been warned.";
            return null;
        }
    }

    [System.Serializable]
    public class BaitInventoryItem
    {
        public BaitData bait;
        public int quantity;
    }

    [System.Serializable]
    public class DipInventoryItem
    {
        public DipData dip;
        public int quantity;
    }
}
