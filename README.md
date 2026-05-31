# Bear's Branchy Harvest
A vintage story mod allowing the harvest of leaf blocks and increasing their stick drops.

In survival shows such as Naked and Afraid, shelter commonly takes the form of a boma. Bomas are basically an enclosure surrounded by stacked and entwined sticks and branches that form a rudimentary wall. I like to build something similar as my day-one shelter in-game, but rough-hewn fences look too advanced for such a primitive build. With this mod, you can chop branchy leaf blocks off trees with a knife and use them to build such an enclosure, or patch holes in ruins to stave off animals and keep that stone-age survival feel. Additionally, when broken with anything else, the blocks will drop several sticks, making it much easier to build wattle fencing or layers of sticks for an attractive bushcraft hideaway.

# Technical Details
Bear's Branchy Harvest is a code mod, not a JSON patch. Because of this, it is compatible with other patches (including stick drop patches, though it will override the amount unless configured not to.) This mod alters the JSON objects of the blocks after they have been loaded and patched but before they have been processed into C# block data.

# Configuration
Bear's Branchy Harvest has a configuration file where you can adjust its behavior. It contains the following settings.

* **AllowBranchyDropWithKnife** *(Default true)* - If true, branchy leaf blocks can be harvested with a knife, dropping themselves.
* **AllowLeafyDropWithKnife** *(Default true)* - If true, non-branchy leaf blocks can be harvested with a knife, dropping themselves.
* **AlterStickDrops** *(Default true)* - If true, the amount of sticks dropped by branchy and non-branchy leaf blocks will be altered by the following settings. If false, preexisting stick drops are left as-is.
* **BranchyStickAverage** *(Default 3)* - The average number of sticks a branchy leaf block will drop. The range of possible amounts will be the average plus and minus the variance (2 to 4 sticks by default).
* **BranchyStickVariance** *(Default 1)* - The amount by which branchy stick drops will vary.
* **LeafyStickAverage** *(Default 1.5)* - The average number of sticks a non-branchy leaf block will drop. The range of possible amounts will be the average plus and minus the variance (1 to 2 sticks by default).
* **LeafyStickVariance** *(Default 0.5)* -  - The amount by which non-branchy stick drops will vary.