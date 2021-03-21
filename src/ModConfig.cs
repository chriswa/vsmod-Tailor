namespace Tailor {
  public class ModConfig {
    public int[] clothCostPerWarmthCategory = new int[] { 1, 2, 4, 8, 12 };
    public int[] threadCostPerWarmthCategory = new int[] { 1, 2, 6, 8, 16 };
    public string[] blocklistedCategories = new string[] { "neck", "emblem", "arm", "armorhead", "armorbody", "armorlegs" };
    public string[] blocklistedItemCodeSubstrings = new string[] {
      "silver", "gold", "wood", "metal", "gem",
      "prisoner", // looks like metal
      "malefactor-mask" // unfortunately doesn't have "wood" in the code, only the english lang entry
    };
  }
}
