using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;

[assembly: ModInfo("Tailor")]

namespace Tailor {
  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class NetworkApiTailorRequest {
    public string itemCode;
  }

  [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
  public class NetworkApiTailorResponse {
    public string message;
  }
  public class TailorMod : ModSystem {

    private ICoreAPI api;
    private ModConfig config;

    public override void Start(ICoreAPI api) {
      base.Start(api);
      this.api = api;

      api.Logger.StoryEvent("[Tailor] Hello world!");

      // register network channel and message types
      api.Network.RegisterChannel("tailor")
          .RegisterMessageType(typeof(NetworkApiTailorRequest))
          .RegisterMessageType(typeof(NetworkApiTailorResponse))
      ;

      // load config file or write it with defaults
      config = api.LoadModConfig<ModConfig>("TailorConfig.json");
      if (config == null) {
        config = new ModConfig();
        api.StoreModConfig(config, "TailorConfig.json");
      }

      api.RegisterItemClass("boneneedle", typeof(BoneNeedleItem));
    }

    public override void StartServerSide(ICoreServerAPI sapi) {
      // for testing, trygive the player a boneneedle whenever they respawn
      // sapi.Event.PlayerRespawn += (IServerPlayer player) => {
      //   player.InventoryManager.TryGiveItemstack(new ItemStack(sapi.World.GetItem(new AssetLocation("tailor:boneneedle"))));
      // };
      sapi.Network.GetChannel("tailor").SetMessageHandler<NetworkApiTailorRequest>((IServerPlayer player, NetworkApiTailorRequest request) => {
        // api.Logger.Debug($"received NetworkApiTailorRequest! {request.itemCode}");
        AttemptCraft(sapi, player, request.itemCode);
      });
    }

    private GuiDialogTailoring dialog;
    public override void StartClientSide(ICoreClientAPI capi) {
      capi.Event.LevelFinalize += () => {
        dialog = new GuiDialogTailoring(capi, this);
      };

      // TODO: this doesn't ever fire! why?
      capi.Network.GetChannel("tailor").SetMessageHandler<NetworkApiTailorResponse>((NetworkApiTailorResponse response) => {
        // capi.Logger.Debug($"CLIENT received NetworkApiTailorResponse! {response.message}");
        if (response.message != "") {
          capi.ShowChatMessage($"NetworkApiTailorResponse error: {response.message}");
        }
      });
    }

    public void OpenTailoringDialog() {
      dialog.TryOpen();
    }

    public void RequestCraftFromServer(ICoreClientAPI capi, string itemCode) {
      // api.Logger.Debug($"RequestCraftFromServer: {itemCode}");
      capi.Network.GetChannel("tailor").SendPacket(new NetworkApiTailorRequest() { itemCode = itemCode });
    }

    public Dictionary<string, int> GetMaterialCostsForProduct(ItemStack itemStack) {
      float warmth = itemStack.ItemAttributes?["warmth"].AsFloat(0) ?? 0;
      string clothescategory = itemStack.ItemAttributes?["clothescategory"].AsString() ?? "";
      string itemCode = itemStack.Item.Code.ToShortString().ToLowerInvariant();

      int warmthCategory = 0;
      bool clothMustBeFine = false;
      bool threadMustBeFine = false;
      if (warmth <= 0.5) {
      }
      else if (warmth <= 1.0) {
        warmthCategory = 1;
      }
      else if (warmth <= 2.0) {
        warmthCategory = 2;
        threadMustBeFine = true;
      }
      else if (warmth <= 3.0) {
        warmthCategory = 3;
        threadMustBeFine = true;
        clothMustBeFine = true;
      }
      else { // e.g. 4.0
        warmthCategory = 4;
        threadMustBeFine = true;
        clothMustBeFine = true;
      }
      int clothCost = config.clothCostPerWarmthCategory[warmthCategory];
      int threadCost = config.threadCostPerWarmthCategory[warmthCategory];
      int leatherCost = 0;

      if (clothescategory == "foot") {
        leatherCost += 1;
        clothCost -= 1;
      }
      if (itemCode.Contains("leather") || itemCode.Contains("raw-hide") || itemCode.Contains("fur") || itemCode.Contains("upperbody-malefactor-tunic")) {
        leatherCost += clothCost;
        clothCost = 0;
      }

      if (clothCost < 0) { clothCost = 0; }

      Dictionary<string, int> costs = new Dictionary<string, int>();
      if (clothCost > 0) {
        costs.Add(/* clothMustBeFine ? "tailor:loom-fabric" : */ "game:linen-normal-down", clothCost); // TODO: require fancy stuff
      }
      if (leatherCost > 0) {
        costs.Add("game:leather", leatherCost);
      }
      if (threadCost > 0) {
        costs.Add(/* threadMustBeFine ? "tailor:spun-thread" : */ "game:flaxtwine", threadCost); // TODO: require fancy stuff
      }
      return costs;
    }

    public ItemStack CreateItemStackForProductItemCode(string productItemCode) {
      var productItem = api.World.GetItem(new AssetLocation(productItemCode));
      var productItemStack = new ItemStack(productItem);
      productItemStack.Attributes.SetFloat("condition", 1); // 100% condition
      return productItemStack;
    }

    public bool DoesPlayerEntityHaveRequiredMaterials(EntityPlayer entityPlayer, string productItemCode) {
      var costs = GetMaterialCostsForProduct(CreateItemStackForProductItemCode(productItemCode));

      foreach (var costLineItem in costs) {
        var heldCount = 0;
        // api.Logger.Debug($"looking for {costLineItem.Key} x {costLineItem.Value}");
        entityPlayer.WalkInventory((invslot) => {
          if (invslot is ItemSlotCreative || !(invslot.Inventory is InventoryBasePlayer)) return true;
          var collectibleCode = invslot.Itemstack?.Collectible?.Code?.ToString();
          if (collectibleCode != null) {
            // api.Logger.Debug($"found collectibles in player inv slot: {collectibleCode} x {invslot.StackSize}");
          }
          if (collectibleCode == costLineItem.Key) {
            heldCount += invslot.StackSize;
          }
          return true; // continue walking inventory
        });
        // api.Logger.Debug($"player has {heldCount} of {costLineItem.Value} required {costLineItem.Key}");
        if (heldCount < costLineItem.Value) {
          return false;
        }
      }

      return true;
    }

    private void AttemptCraft(ICoreServerAPI sapi, IServerPlayer player, string productItemCode) {
      var productItemStack = CreateItemStackForProductItemCode(productItemCode);

      var costs = GetMaterialCostsForProduct(productItemStack);

      bool canCraft = DoesPlayerEntityHaveRequiredMaterials(player.Entity, productItemCode);

      if (!canCraft) {
        // sapi.Logger.Debug("Server is sending NetworkApiTailorResponse: materials missing!");
        sapi.Network.GetChannel("tailor").SendPacket(new NetworkApiTailorResponse() { message = "materials missing!" });
        return;
      }

      // remove materials from player inv
      foreach (var costLineItem in costs) {
        var debt = costLineItem.Value;
        player.Entity.WalkInventory((invslot) => {
          if (invslot is ItemSlotCreative || !(invslot.Inventory is InventoryBasePlayer)) return true;
          if (invslot.Itemstack?.Collectible?.Code?.ToString() == costLineItem.Key && debt > 0) {
            var toPayFromThisSlot = Math.Min(debt, invslot.StackSize);

            invslot.Itemstack.StackSize -= toPayFromThisSlot;
            if (invslot.StackSize <= 0) { invslot.Itemstack = null; }
            invslot.MarkDirty();
            debt -= toPayFromThisSlot;
          }
          return true;
        });
      }

      if (!player.InventoryManager.TryGiveItemstack(productItemStack, true)) {
        sapi.World.SpawnItemEntity(productItemStack, player.Entity.Pos.XYZ.Add(0, 0.5, 0));
      }
      // sapi.Logger.Debug("Server is sending NetworkApiTailorResponse: SUCCESS!");
      sapi.Network.GetChannel("tailor").SendPacket(new NetworkApiTailorResponse() { message = "NO ERROR" });
    }

    public bool IsItemAllowed(string clothescategory, string itemCode) {
      if (config.blocklistedCategories.Contains(clothescategory.ToLowerInvariant())) {
        // capi.Logger.Debug($"  rejected due to category");
        return false;
      }
      foreach (var rejectedName in config.blocklistedItemCodeSubstrings) {
        if (itemCode.Contains(rejectedName)) {
          return false;
        }
      }
      return true;
    }
  }
}
