using System.Linq;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace BearsBranchyHarvest
{
    /// <summary>Asset patching system that scans for leaf block assets and modifies their drops.</summary>
    public class BearsBranchyHarvestModSystem : ModSystem
    {
        #region Fields

        private CompatHandler compatHandler = new();

        #endregion Fields

        #region Vintage Story Methods

        /// <summary>
        /// Hijacks the leaf block drops once they have been converted to C# objects but before they
        /// have been fully finalized. We do this here to avoid the JSON fiasco of mods patching and
        /// clobbering each other's JSON.
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

            // interate through all registered blocks, looking for leaves and noting if they are branchy
            foreach (Block block in api.World.Blocks) {
                if (block.Code == null || IsBlacklisted(block, settings)) {
                    continue;
                }

                isBranchy = block.Code.Path.Contains("leavesbranchy-");

                if (block.Code.Path.Contains("leaves-") || isBranchy) {
                    if (block.Drops != null) {
                        // stick drops are altered first
                        if (settings.AlterStickDrops || settings.AlterSeedDrops) {
                            foreach (BlockDropItemStack drop in block.Drops) {
                                if (drop.Code == null) {
                                    continue;
                                }

                                // find a stick drop and alter the drop values
                                if (drop.Code.Equals("stick") || drop.Code.Equals("game:stick")) {
                                    if (settings.AlterStickDrops) {
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
                        }
                    }

                    // leaf block knife drops are added to the front of the array so they happen
                    // first and cancel the stick, seed, and other drops
                    if ((settings.AllowLeafyDropWithKnife && !isBranchy) || (settings.AllowBranchyDropWithKnife && isBranchy)) {
                        (bool isSuccessful, BlockDropItemStack itemStack) = GetLeafBlockDropStack(block, api);

                        if (isSuccessful) {
                            block.Drops = block.Drops.Prepend(itemStack).ToArray();
                        }
                    }
                }

                // once done with drops, we engage the fence connections
                if (settings.ConnectToFences && isBranchy) {
                    if (block.Attributes["fenceConnect"].Exists) {
                        JToken? fenceConnectToken = block.Attributes["fenceConnect"].Token;

                        if (fenceConnectToken == null) {
                            Mod.Logger.Error("Can't find fence connect token.");
                            continue;
                        }

                        fenceConnectToken["north"] = true;
                        fenceConnectToken["west"] = true;
                        fenceConnectToken["east"] = true;
                        fenceConnectToken["south"] = true;
                    }
                }
            }
        }

        public override double ExecuteOrder()
        {
            return 0.96;
        }

        // Called on server and client Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            compatHandler.OnStart(api);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification(Lang.Get("bearsbranchyharvest:hello"));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            compatHandler.TryRemoveXSkillsPatch(api);
        }

        #endregion Vintage Story Methods

        #region Private Methods

        /// <summary>
        /// Removes the state descriptor of the leaf block and replaces it with the "placed" state.
        /// </summary>
        private string GetFriendlyLeafDropName(string blockCode)
        {
            string[] subStrings = blockCode.Split("-");
            // HACK: this is pretty fragile. Need to do some more sophisticated targeting logic to get the state
            if (subStrings[0].Contains("bambooleaves")) {
                subStrings[^1] = "placed";
            }
            else {
                subStrings[^2] = "placed";
            }
            return string.Join("-", subStrings);
        }

        /// <summary>
        /// Helper function to fabricate a BlockDropItemStack designed for leaf block drops.
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

            // TODO: Set this up a little better. Feels very hackish to basically create throwaway parameters.
            bool didSucceed = leafDrop.Resolve(api.World, "", AssetLocation.Create(leafDrop.Code));

            // it's rare I get to use a tuple
            return (didSucceed, leafDrop);
        }

        /// <summary>
        /// Checks if the block is blacklisted. Returns false if the block is acceptable to use,
        /// true if it is blacklisted.
        /// </summary>
        private bool IsBlacklisted(Block block, BranchyHarvestSettings settings)
        {
            foreach (string blackListItem in settings.BlockBlacklist) {
                if (block.Code.GetName().Contains(blackListItem)) {
                    return true;
                }
            }

            foreach (string blackListItem in settings.ModBlacklist) {
                if (block.Code.Domain.Contains(blackListItem)) {
                    return true;
                }
            }
            return false;
        }

        #endregion Private Methods
    }
}