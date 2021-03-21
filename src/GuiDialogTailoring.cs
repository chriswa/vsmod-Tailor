using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;

namespace Tailor {
  class GuiDialogTailoring : GuiDialog {

    // private InventoryCatalog inventory;
    private const int PADDING = 15;
    private const int STACKLIST_HEIGHT = 300;

    public override string ToggleKeyCombinationCode => null;
    private TailorMod tailorMod;
    public GuiDialogTailoring(ICoreClientAPI capi, TailorMod tailorMod) : base(capi) {
      this.tailorMod = tailorMod;
      LoadEntries();
      SetupDialog();
      FilterItems();
    }
    private List<GuiHandbookPage> allStacklistItems = new List<GuiHandbookPage>();
    private List<GuiHandbookPage> shownStacklistItems = new List<GuiHandbookPage>();
    private string filterSearch = "";
    private string filterCategory = "";

    private void SetupDialog() {

      // title bar
      ElementBounds bgBounds = ElementStdBounds.DialogBackground();
      ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithChild(bgBounds);
      SingleComposer = capi.Gui.CreateCompo("tailoring", dialogBounds)
          .AddShadedDialogBG(bgBounds)
          .AddDialogTitleBar("Tailoring", () => TryClose())
          .BeginChildElements(bgBounds);
      ElementBounds titleBarBounds = ElementStdBounds.TitleBar();
      bgBounds.WithChild(titleBarBounds);

      // search text box
      ElementBounds searchBounds = ElementBounds.FixedSize(200, 30).FixedUnder(titleBarBounds);
      bgBounds.WithChild(searchBounds);
      SingleComposer.AddTextInput(searchBounds, (string newText) => {
        filterSearch = newText;
        FilterItems();
      }, CairoFont.WhiteSmallishText(), "search");
      SingleComposer.GetTextInput("search").SetPlaceHolderText("Search...");
      // SingleComposer.FocusElement(SingleComposer.GetTextInput("search").TabIndex);

      // prep for category dropdown
      string[] characterDressTypeNames = EnumCharacterDressType.GetNames(typeof(EnumCharacterDressType));
      List<string> filteredCharacterDressTypeNames = new List<string>();
      foreach (var characterDressTypeName in characterDressTypeNames) {
        if (categoryCount.ContainsKey(characterDressTypeName.ToLowerInvariant())) {
          filteredCharacterDressTypeNames.Add(characterDressTypeName);
        }
      }
      string[] values = new string[filteredCharacterDressTypeNames.Count + 1];
      values[0] = "";
      filteredCharacterDressTypeNames.CopyTo(0, values, 1, filteredCharacterDressTypeNames.Count);
      string[] names = new string[filteredCharacterDressTypeNames.Count + 1];
      names[0] = "(Show all)";
      filteredCharacterDressTypeNames.CopyTo(0, names, 1, filteredCharacterDressTypeNames.Count);

      // category dropdown
      ElementBounds categoryDropdownBounds = ElementBounds.FixedSize(150, 30).FixedUnder(titleBarBounds).FixedRightOf(searchBounds, PADDING);
      bgBounds.WithChild(categoryDropdownBounds);
      SingleComposer.AddDropDown(values, names, 0, (value, selected) => {
        filterCategory = value;
        FilterItems();
      }, categoryDropdownBounds, "category");

      // stacklist
      ElementBounds stacklistBounds = ElementBounds.FixedSize(400, STACKLIST_HEIGHT).FixedUnder(searchBounds, PADDING);
      bgBounds.WithChild(stacklistBounds);
      ElementBounds clipBounds = stacklistBounds.ForkBoundingParent();
      ElementBounds insetBounds = stacklistBounds.FlatCopy().FixedGrow(6).WithFixedOffset(-3, -3);
      ElementBounds scrollbarBounds = insetBounds.CopyOffsetedSibling(3 + stacklistBounds.fixedWidth + 7).WithFixedWidth(20);
      SingleComposer
          .BeginClip(clipBounds)
              .AddInset(insetBounds, 3)
              .AddInteractiveElement(new GuiElementHandbookListWithTooltips(capi, stacklistBounds, (int index) => {
                OnSelectProduct((shownStacklistItems[index] as GuiHandbookItemStackPage).Stack);
              }, shownStacklistItems), "stacklist")
          .EndClip()
          .AddVerticalScrollbar((float value) => {
            GuiElementHandbookList stacklist = SingleComposer.GetHandbookStackList("stacklist");
            stacklist.insideBounds.fixedY = 3 - value;
            stacklist.insideBounds.CalcWorldBounds();
          }, scrollbarBounds, "scrollbar")
      ;
      UpdateStacklistScrollbar();

      // costLabel
      ElementBounds costLabelBounds = ElementBounds.FixedSize(300, 80).FixedUnder(clipBounds, PADDING);
      bgBounds.WithChild(costLabelBounds);
      SingleComposer.AddDynamicText("", CairoFont.WhiteSmallText(), EnumTextOrientation.Left, costLabelBounds, "costLabel");

      // craftButton
      ElementBounds craftButtonBounds = ElementBounds
          .FixedSize(0, 0)
          .FixedUnder(clipBounds, PADDING)
          .WithAlignment(EnumDialogArea.RightFixed)
          .WithFixedPadding(20, 4)
          .WithFixedAlignmentOffset(-11, 1)
      ;
      bgBounds.WithChild(craftButtonBounds);
      SingleComposer.AddSmallButton("Craft", () => {
        OnCraftButtonClicked();
        return true;
      }, craftButtonBounds, EnumButtonStyle.Normal, EnumTextOrientation.Center, "craftButton");
      var craftButton = SingleComposer.GetButton("craftButton");
      craftButton.Enabled = false;

      SingleComposer.EndChildElements(); // bgBounds
      SingleComposer.Compose();
    }
    private Dictionary<string, int> categoryCount = new Dictionary<string, int>(); // used to hide dropdown entries
    private void LoadEntries() {
      allStacklistItems.Clear();

      foreach (Item item in capi.World.Items) {
        if (item.Code != null) {
          if (item.Code.ToShortString().StartsWith("clothes-")) {
            // capi.Logger.Debug($"=== {item.Code.ToString()} - {item.Code.ToShortString()} ===");
            // capi.Logger.Debug(item.Variant.ToString());

            float warmth = item.Attributes?["warmth"].AsFloat(0) ?? 0;
            string clothescategory = item.Attributes?["clothescategory"].AsString() ?? "";
            // capi.Logger.Debug($"  {clothescategory} - {warmth}");

            if (!tailorMod.IsItemAllowed(clothescategory, item.Code.ToShortString().ToLowerInvariant())) {
              continue;
            }

            categoryCount[clothescategory] = categoryCount.ContainsKey(clothescategory) ? categoryCount[clothescategory] + 1 : 1;

            var itemStack = new ItemStack(item);
            itemStack.Attributes.SetFloat("condition", 1); // 100% condition
            GuiHandbookItemStackPage stacklistPage = new GuiHandbookItemStackPage(capi, itemStack);
            // stacklistPage.TextCache = stacklistPage.Stack.GetName() + " " + stacklistPage.Stack.GetDescription(capi.World, stacklistPage.dummySlot, false) + $" ({clothescategory})";
            allStacklistItems.Add(stacklistPage);
          }
        }
      }
    }
    private void FilterItems() {
      shownStacklistItems.Clear();
      foreach (GuiHandbookItemStackPage stacklistItem in allStacklistItems) {
        if (filterCategory != "") {
          var stacklistItemClothesCategory = stacklistItem.Stack.ItemAttributes?["clothescategory"].AsString() ?? "";
          if (stacklistItemClothesCategory != filterCategory.ToLowerInvariant()) {
            // capi.Logger.Debug($"filtering by cat: {stacklistItemClothesCategory} != {filterCategory}");
            continue;
          }
        }
        if (filterSearch != "") {
          var itemName = stacklistItem.Stack.GetName();
          if (!itemName.ToLowerInvariant().Contains(filterSearch.ToLowerInvariant())) {
            continue;
          }
        }
        shownStacklistItems.Add(stacklistItem);
      }
      GuiElementHandbookList stacklist = SingleComposer.GetHandbookStackList("stacklist");
      stacklist.CalcTotalHeight();
      UpdateStacklistScrollbar();
    }
    private void UpdateStacklistScrollbar() {
      var scrollbar = SingleComposer.GetScrollbar("scrollbar");
      scrollbar.SetHeights(
        (float)STACKLIST_HEIGHT,
        (float)SingleComposer.GetHandbookStackList("stacklist").insideBounds.fixedHeight
      );
      scrollbar.OnMouseWheel(capi, new MouseWheelEventArgs() { deltaPrecise = 999999 }); // scroll to top hax
    }
    public override void OnGuiOpened() {
      SingleComposer.GetTextInput("search").SetValue("");
      SingleComposer.GetDropDown("category").SetSelectedIndex(0);
      selectedProductItemStack = null;
      SingleComposer.GetButton("craftButton").Enabled = false;
      SingleComposer.GetDynamicText("costLabel").SetNewText("Click on an article of clothing.");
      filterCategory = "";
      filterSearch = "";
      FilterItems();
      base.OnGuiOpened();
    }
    private ItemStack selectedProductItemStack;
    private string GetNameForCode(string code) {
      var matLocation = new AssetLocation(code);
      string blockKey = matLocation.Domain + AssetLocation.LocationSeparator + "block-" + matLocation.Path;
      string blockName = Lang.GetMatching(blockKey);
      if (blockName != blockKey) { return blockName; }
      string itemName = Lang.GetMatching(matLocation.Domain + AssetLocation.LocationSeparator + "item-" + matLocation.Path);
      return itemName;
    }
    private void OnSelectProduct(ItemStack itemStack) {
      selectedProductItemStack = itemStack;
      // capi.ShowChatMessage($"Clicked on: {itemStack.GetName()} - {itemStack.Item} - {itemStack.Item.Code.ToShortString()}");
      var costs = this.tailorMod.GetMaterialCostsForProduct(itemStack);
      var warmth = itemStack.ItemAttributes?["warmth"].AsFloat(0) ?? 0;
      var costString = new StringBuilder($"{itemStack.GetName()}\n");
      foreach (var costLineItem in costs) {
        costString.Append($"{costLineItem.Value} {GetNameForCode(costLineItem.Key)}\n");
      }
      SingleComposer.GetDynamicText("costLabel").SetNewText(costString.ToString());

      bool canCraft = tailorMod.DoesPlayerEntityHaveRequiredMaterials(capi.World.Player.Entity, itemStack.Item.Code.ToString());

      SingleComposer.GetButton("craftButton").Enabled = canCraft;
    }
    private void OnCraftButtonClicked() {
      tailorMod.RequestCraftFromServer(capi, selectedProductItemStack.Item.Code.ToString());
      this.TryClose();
    }
  }
}