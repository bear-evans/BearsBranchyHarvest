# Bear's Branchy Harvest
A vintage story mod allowing the harvest of leaf blocks and increasing their stick drops.

In survival shows such as Naked and Afraid, shelter commonly takes the form of a boma. Bomas are basically an enclosure surrounded by stacked and entwined sticks and branches that form a rudimentary wall. I like to build something similar as my day-one shelter in-game, but rough-hewn fences look too advanced for such a primitive build. With this mod, you can chop branchy leaf blocks off trees with a knife and use them to build such an enclosure, or patch holes in ruins to stave off animals and keep that stone-age survival feel. Additionally, when broken with anything else, the blocks will drop several sticks, making it much easier to build wattle fencing or layers of sticks for an attractive bushcraft hideaway.

# What This Mod Does
* Allows leaves and branchy leaves to drop themselves when harvested with a knife. (Individually configurable).
* Increases the number of sticks dropped by branchy leaves and leaves. (shared on/off toggle, individually configurable amounts)
* Allows fences to connect to branchy leaf blocks for better hedge fences. (Optional toggle)

# Technical Details
Despite the fact that it alters the drop table in the leaf block, Bear's Branchy Harvest is a code mod, not a JSON patch. Because of this, it is compatible with other patches and mods (including stick drop patches, though it will override the amount unless configured not to.) This mod alters the blocks after they have been loaded and patched, just after they have been processed into C# objects, so all other patches will be applied first and the mod will make an effort to intelligently insert its own drops after all your other modifications have been made.

It has out-of-the-box compatibility with all tree mods, so long as they inherit from the vanilla leaf blocks.

# Configuration
Bear's Branchy Harvest has a configuration file where you can adjust its behavior. It contains the following settings.

* **AllowBranchyDropWithKnife** *(Default true)* - If true, branchy leaf blocks can be harvested with a knife, dropping themselves.
* **AllowLeafyDropWithKnife** *(Default true)* - If true, non-branchy leaf blocks can be harvested with a knife, dropping themselves.
* **AlterStickDrops** *(Default true)* - If true, the amount of sticks dropped by branchy and non-branchy leaf blocks will be altered by the following settings. If false, the default values or values determined by JSON patches will be used.
* **BranchyStickAverage** *(Default 3)* - The average number of sticks a branchy leaf block will drop. The range of possible amounts will be the average plus and minus the variance (2 to 4 sticks by default).
* **BranchyStickVariance** *(Default 1)* - The amount by which branchy stick drops will vary.
* **LeafyStickAverage** *(Default 0.5)* - The average number of sticks a non-branchy leaf block will drop. The range of possible amounts will be the average plus and minus the variance (50% chance to drop 1 stick by default).
* **LeafyStickVariance** *(Default 0)* - The amount by which non-branchy stick drops will vary.
* **ConnectToFences** *(Default true)* - Makes branchy leaf blocks connect to fences like other solid blocks do.
* **BlockBlacklist** (default []) - Blocks with names that *contain* any of these words/strings will be excluded.
* **ModBlacklist** (default []) - Blocks from mod domains that *contain* any of these words/strings will be excluded.

# Roadmap
* Possibly a seed chance multiplier to make this a complete leaf block solution.