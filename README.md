# vsmod-Tailor

A Vintage Story mod which allows players to craft (most) clothing items in the game. Make a Bone Needle by putting a bone in the bottom of your crafting grid, then a knife on top of it, and a stone on top of that. Right click with the Bone Needle to open the Tailoring menu. Use the controls up top to filter clothing items. Click on a clothing item to display required materials. If you have the required materials in your inventory, the Craft Button is enabled. Click to create the clothing item. It will be crafted at 100% condition.

## Material costs

Materials are based on (a) the clothing item's warmth attribute, (b) whether it has "leather", "raw-hide", or "fur" in its item code, and (c) whether it's for your feet.

Warmth 0.5 (and less) items cost 1 Linen and 1 Flax Twine.

Warmth 1.0 items cost 2 Linen and 2 Twine.

Warmth 2.0 items cost 4 Linen and 6 Twine.

Warmth 3.0 items cost 8 Linen and 8 Twine.

Warmth above 3 (e.g. 4.0) items cost 12 Linen and 16 Twine.

The mod config file allows the above values to be changed.

If the article of clothing has "leather", "raw-hide", or "fur" in its item code, it requires Leather instead of Linen, in the same amount.

If the article of clothing is for the feet, and not already leather, 1 Linen is subtracted and 1 Leather is added to the cost.

## What can be crafted?

All items with codes starting with "clothes-" are considered. The mod config file allows blocklisting categories and item code substrings. Default blocklists are provided to disallow more ridiculous recipes, such as crafting a silver crown or wooden mask.

Blocklisted Categories:

- neck
- emblem
- arm
- armorhead
- armorbody
- armorlegs

Blocklisted Item Code Substrings:

- silver
- gold
- wood
- metal
- gem
- prisoner
- malefactor-mask

