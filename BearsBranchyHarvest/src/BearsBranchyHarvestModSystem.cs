using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace BearsBranchyHarvest
{
    /// <summary>
    /// Asset patching system that scans for leaf block assets and modifies their drops.
    /// </summary>
    public class BearsBranchyHarvestModSystem : ModSystem
    {
        #region Methods

        public override double ExecuteOrder()
        {
            return 0.96;
        }

        #endregion Methods

        #region Enums

        public enum LeafBlockType
        {
            Branchy,
            Leafy
        }

        #endregion Enums

        #region Classes

        /// <summary>
        /// A C# object representing a quantity token in drops JSON.
        /// </summary>
        public class QuantityObject
        {
            #region Fields

            public float avg;
            public float var;

            #endregion Fields

            #region Constructors

            public QuantityObject(float avg, float var)
            {
                this.avg = avg;
                this.var = var;
            }

            #endregion Constructors
        }

        /// <summary>
        /// A C# object representing a single drop preset in blocktypes JSON.
        /// </summary>
        public class DropPresetObject
        {
            #region Fields

            public string? type;
            public string code;
            public QuantityObject quantity;
            public string? tool;
            public bool lastDrop;

            #endregion Fields

            #region Constructors

            public DropPresetObject(string? type, string code, string? tool, float avg, float var)
            {
                this.type = type;
                this.code = code;
                quantity = new(avg, var);
                this.tool = tool;
                lastDrop = false;
            }

            public DropPresetObject(string? type, string code, string? tool, float avg, float var, bool lastDrop)
            {
                this.type = type;
                this.code = code;
                quantity = new(avg, var);
                this.tool = tool;
                this.lastDrop = lastDrop;
            }

            #endregion Constructors

            #region Methods

            // Converts the drop preset object to a JSON string
            public string ToJSON()
            {
                StringBuilder sb = new();

                if (type != null) {
                    _ = sb.Append(JToken.FromObject(type));
                }

                _ = sb.Append(JToken.FromObject(code));
                _ = sb.Append(JToken.FromObject(quantity));

                if (tool != null) {
                    _ = sb.Append(JToken.FromObject(tool));
                }
                _ = sb.Append(JToken.FromObject(lastDrop));

                return sb.ToString();
            }

            #endregion Methods
        }

        #endregion Classes

        #region Methods

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification(Lang.Get("bearsbranchyharvest:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification(Lang.Get("bearsbranchyharvest:hello"));
        }

        /// <summary>
        /// Hijacks the leaf block drops once they have been converted to C# objects but before they have been fully finalized. We do this here to avoid the JSON fiasco of mods patching and clobbering each other's JSON.
        /// </summary>
        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);

            // no need to run this on anything but the server, clients will download the blocks
            if (api.Side != EnumAppSide.Server) {
                return;
            }

            if (BranchyHarvestSettingsLoader.CurrentSettings == null) {
                Mod.Logger.Error(Lang.Get("bearsbranchyharvest:settings-missing-error"));
                // couldn't find settings, abort
                return;
            }

            // initialize variables out here so we can reuse the same memory addresses
            BranchyHarvestSettings settings = BranchyHarvestSettingsLoader.CurrentSettings;
            bool isBranchy;

            foreach (Block block in api.World.Blocks) {
                if (block.Code == null) {
                    continue;
                }

                isBranchy = block.Code.Path.Contains("leavesbranchy-");

                if (block.Code.Path.Contains("leaves-") || isBranchy) {
                    if (block.Drops != null) {
                        foreach (BlockDropItemStack drop in block.Drops) {
                            if (drop.Code == null) {
                                continue;
                            }

                            if (settings.AlterStickDrops) {
                                if (drop.Code.Equals("stick") || drop.Code.Equals("game:stick")) {
                                    if (isBranchy) {
                                        drop.Quantity.avg = settings.BranchyStickAverage;
                                        drop.Quantity.var = settings.BranchyStickVariance;
                                    }
                                    else {
                                        drop.Quantity.avg = settings.LeafyStickAverage;
                                        drop.Quantity.var = settings.LeafyStickVariance;
                                    }
                                }
                            }
                        }

                        if ((settings.AllowLeafyDropWithKnife && !isBranchy) || (settings.AllowBranchyDropWithKnife && isBranchy)) {
                            (bool, BlockDropItemStack) leafDrop = GetLeafBlockDropStack(block, api);

                            if (leafDrop.Item1) {
                                block.Drops = block.Drops.Prepend(leafDrop.Item2).ToArray();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the state descriptor of the leaf block and replaces it with the "placed" state.
        /// </summary>
        private string GetFriendlyLeafDropName(string blockCode)
        {
            string[] subStrings = blockCode.Split("-");
            // HACK: this is pretty fragile. Need to do some more sophisticated targeting logic to get the state
            if (subStrings[1].Contains("bambooleaves")) {
                subStrings[^1] = "placed";
            }
            else {
                subStrings[^2] = "placed";
            }
            return string.Join("-", subStrings);
        }

        /// <summary>
        /// Helper function fabricate a BlockDropItemStack designed for leaf block drops.
        /// </summary>
        private (bool, BlockDropItemStack) GetLeafBlockDropStack(Block block, ICoreAPI api)
        {
            BlockDropItemStack leafDrop = new() {
                Type = EnumItemClass.Block,
                Code = GetFriendlyLeafDropName(block.Code),
                Quantity = NatFloat.One,
                Tool = EnumTool.Knife,
                LastDrop = true
            };

            bool didSucceed = leafDrop.Resolve(api.World, "", AssetLocation.Create(leafDrop.Code));

            return (didSucceed, leafDrop);
        }
    }

        #endregion Methods
}