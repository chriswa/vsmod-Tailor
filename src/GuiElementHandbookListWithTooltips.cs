using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Tailor {
  public class GuiElementHandbookListWithTooltips : GuiElementHandbookList {
  private ItemstackComponentBase tooltipProvider;
  private DummySlot dummySlot;
  public GuiElementHandbookListWithTooltips(
    ICoreClientAPI capi,
    ElementBounds bounds,
    Vintagestory.API.Common.Action<int> onLeftClick,
    List<GuiHandbookPage> elements = null
  ) : base(capi, bounds, onLeftClick, elements) {
    tooltipProvider = new ItemstackComponentBase(capi);
    dummySlot = new DummySlot();
  }
  public override void RenderInteractiveElements(float deltaTime) {
    base.RenderInteractiveElements(deltaTime);
    int mx = api.Input.MouseX;
    int my = api.Input.MouseY;
    bool inbounds = Bounds.ParentBounds.PointInside(mx, my);

    double posY = insideBounds.absY;

    foreach (GuiHandbookPage element in Elements) {
      if (!element.Visible) continue;

      float y = (float)(5 + Bounds.absY + posY);

      if (inbounds && mx > Bounds.absX && mx <= Bounds.absX + Bounds.InnerWidth && my >= y - 8 && my <= y + scaled(unscaledCellHeight) - 8) {

        if (mx - Bounds.absX < 60) { // only if over the icons on the left, not the text
          dummySlot.Itemstack = (element as GuiHandbookItemStackPage).Stack;
          tooltipProvider.RenderItemstackTooltip(dummySlot, api.Input.MouseX, api.Input.MouseY, deltaTime);
        }

      }

      posY += scaled(unscaledCellHeight + unscaledCellSpacing);
    }
  }
}
}