using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Tailor {
  public class ItemSlotCatalog : ItemSlot {

    public override bool DrawUnavailable {
      get {
        return false;
      }
      set { }
    }

    public void SetItem(Item item) {
      this.itemstack = new ItemStack(item);
    }

    public ItemSlotCatalog(InventoryBase inventory) : base(inventory) {
    }

    public override bool CanTake() {
      return false;
    }

    public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge) {
      return false;
    }

    public override bool CanHold(ItemSlot sourceSlot) {
      return false;
    }


    public override bool TryFlipWith(ItemSlot itemSlot) {
      return false;
    }

    protected override void FlipWith(ItemSlot withSlot) {
      return;
    }

    public override void ActivateSlot(ItemSlot sourceSlot, ref ItemStackMoveOperation op) {
    }

    public override int TryPutInto(ItemSlot sinkSlot, ref ItemStackMoveOperation op) {
      return 0;
    }

    public override int TryPutInto(IWorldAccessor world, ItemSlot sinkSlot, int quantity = 1) {
      return 0;
    }

    public override string GetStackDescription(IClientWorldAccessor world, bool extendedDebugInfo) {
      // return Lang.Get("Price: {0} gears\nDemand: {1}", TradeItem.Price, TradeItem.Stock) + "\n\n" + base.GetStackDescription(world, extendedDebugInfo);
      return "ItemSlotCatalog.GetStackDescription";
    }
  }
}
