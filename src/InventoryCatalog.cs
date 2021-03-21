using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Tailor {
  public class InventoryCatalog : InventoryBase {
    public ItemSlotCatalog[] catalogSlots;
    public override float GetTransitionSpeedMul(EnumTransitionType transType, ItemStack stack) {
      return 0;
    }
    public override int Count {
      get { return 0; }
    }
    public InventoryCatalog(string inventoryID, ICoreAPI api) : base(inventoryID, api) {
      GenEmptyCatalogSlots(Count);
    }
    public InventoryCatalog(string className, string instanceID, ICoreAPI api) : base(className, instanceID, api) {
      GenEmptyCatalogSlots(Count);
    }

    public void GenEmptyCatalogSlots(int count) {
      catalogSlots = new ItemSlotCatalog[count];
      for (int i = 0; i < count; i += 1) {
        catalogSlots[i] = new ItemSlotCatalog(this);
      }
    }

    public override object ActivateSlot(int slotId, ItemSlot mouseSlot, ref ItemStackMoveOperation op) {
      /*
      // Player clicked an item from the selling list, move to buying cart
      if (slotId <= 15) {
        AddToBuyingCart(slots[slotId] as ItemSlotTrade);
        return InvNetworkUtil.GetActivateSlotPacket(slotId, op);
      }

      // Player clicked an item in the buying cart, remove it
      if (slotId <= 19) {
        ItemSlotTrade cartSlot = slots[slotId] as ItemSlotTrade;

        if (op.MouseButton == EnumMouseButton.Right) {
          // Just remove one batch on right mouse    
          if (cartSlot.TradeItem?.Stack != null) {
            cartSlot.TakeOut(cartSlot.TradeItem.Stack.StackSize);
            cartSlot.MarkDirty();
          }
        }
        else {
          cartSlot.Itemstack = null;
          cartSlot.MarkDirty();
        }

        return InvNetworkUtil.GetActivateSlotPacket(slotId, op);
      }

      // Player clicked an item on the buy slot, ignore it
      if (slotId <= 34) {
        return InvNetworkUtil.GetActivateSlotPacket(slotId, op);
      }

      // Player clicked an item in the selling cart, act like a normal slot
      if (slotId <= 39) {
        return base.ActivateSlot(slotId, mouseSlot, ref op);
      }

      return InvNetworkUtil.GetActivateSlotPacket(slotId, op);
      */
      return null; // ????????????????????????????
    }

    public override ItemSlot this[int slotId] {
      get {
        if (slotId < 0 || slotId >= Count) return null;
        return catalogSlots[slotId];
      }
      set {
        throw new NotSupportedException("nope!");
      }
    }
    public override void FromTreeAttributes(ITreeAttribute tree) {
      // pass (never used?)
    }
    public override void ToTreeAttributes(ITreeAttribute tree) {
      // pass (never used?)
    }
    protected override ItemSlot NewSlot(int slotId) {
      return new ItemSlotCatalog(this);
    }
    public override object Close(IPlayer player) {
      object p = base.Close(player);

      for (int i = 0; i < Count; i++) {
        catalogSlots[i].Itemstack = null;
      }

      return p;
    }
    public override WeightedSlot GetBestSuitedSlot(ItemSlot sourceSlot, List<ItemSlot> skipSlots = null) {
      return new WeightedSlot();
    }
    public override float GetSuitability(ItemSlot sourceSlot, ItemSlot targetSlot, bool isMerge) {
      return base.GetSuitability(sourceSlot, targetSlot, isMerge);
    }
  }
}
