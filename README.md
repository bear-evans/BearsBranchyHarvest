# Bear's Branchy Harvest
A vintage story mod allowing the harvest of leaf blocks and increasing their stick drops.

In survival shows such as Naked and Afraid, shelter commonly takes the form of a boma. Bomas are basically an enclosure surrounded by stacked and entwined sticks and branches that form a rudimentary wall. I like to build something similar as my day-one shelter in-game, but rough-hewn fences look too advanced for such a primitive build. With this mod, you can chop branchy leaf blocks off trees with a knife and use them to build such an enclosure, or patch holes in ruins to stave off animals and keep that stone-age survival feel. Additionally, when broken with anything else, the blocks will drop several sticks, making it much easier to build wattle fencing or layers of sticks for an attractive bushcraft hideaway.

# Technical Details
Despite the fact that it alters the drop table in the leaf block's JSON, Bear's Branchy Harvest is a code mod, not a JSON patch. Because of this, it is compatible with other patches (including stick drop patches, though it will override the amount unless configured not to.) This mod alters the JSON objects of the blocks after they have been loaded and patched but before they have been processed into C# block data, so all other patches will be applied first and the mod will make an effort to intelligently insert its own drops after all your other modifications have been made.

Why did I do this? Because drops are processed in order, so for the leaf blocks to drop themselves, I need to put the tool-specific drop first or they'll drop seeds and sticks and THEN themselves, leading to infinite resources. Patches, however, target drops by index, not by name, so doing this would shift the indexes of the other drops and guarantee incompatibility with any other patches for seeds or sticks or additional items.

So instead, my code loads the JSON and navigates it more intelligently using the full power of C#, targeting drops by name.

# Configuration
Bear's Branchy Harvest has a configuration file where you can adjust its behavior. It contains the following settings.

* **AllowBranchyDropWithKnife** *(Default true)* - If true, branchy leaf blocks can be harvested with a knife, dropping themselves.
* **AllowLeafyDropWithKnife** *(Default true)* - If true, non-branchy leaf blocks can be harvested with a knife, dropping themselves.
* **AlterStickDrops** *(Default true)* - If true, the amount of sticks dropped by branchy and non-branchy leaf blocks will be altered by the following settings. If false, the default values or values determined by JSON patches will be used.
* **BranchyStickAverage** *(Default 3)* - The average number of sticks a branchy leaf block will drop. The range of possible amounts will be the average plus and minus the variance (2 to 4 sticks by default).
* **BranchyStickVariance** *(Default 1)* - The amount by which branchy stick drops will vary.
* **LeafyStickAverage** *(Default 1.5)* - The average number of sticks a non-branchy leaf block will drop. The range of possible amounts will be the average plus and minus the variance (1 to 2 sticks by default).
* **LeafyStickVariance** *(Default 0.5)* -  - The amount by which non-branchy stick drops will vary.

# Roadmap
* I intend to add compatibility for Wildcraft, Floral Zones, and Biodiversity.
* Possibly a seed chance multiplier to make this a complete leaf block solution.