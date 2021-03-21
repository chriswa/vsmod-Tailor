using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;

namespace Tailor {
  class BoneNeedleItem : Item {
    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling) {
      if (byEntity.Api.Side == EnumAppSide.Server) {
        IServerPlayer player = byEntity.World.PlayerByUid((byEntity as EntityPlayer).PlayerUID) as IServerPlayer;
        // player.SendMessage(GlobalConstants.GeneralChatGroup, "OnHeldInteractStart on Server", EnumChatType.Notification);
      }
      else {
        ICoreClientAPI capi = byEntity.Api as ICoreClientAPI;
        // capi.ShowChatMessage("OnHeldInteractStart on Client");

        capi.ModLoader.GetModSystem<TailorMod>().OpenTailoringDialog();
      }
      handling = EnumHandHandling.Handled;
    }
  }
}