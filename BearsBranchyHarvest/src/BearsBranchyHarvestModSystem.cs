using System;
using System.Text;
using Newtonsoft.Json.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace BearsBranchyHarvest
{
    /// <summary>
    /// Asset patching system that scans for leaf block assets and modifies their drops.
    /// </summary>
    public class BearsBranchyHarvestModSystem : ModSystem
    {
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
        /// Triggers once assets have been loaded and patched from all sources but before the assets are constructed into C# block objects. Our patches happen here.
        /// </summary>
        public override void AssetsLoaded(ICoreAPI api)
        {
            // no need to run this on anything but the server, clients will download the JSON
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

            IAsset asset;
            JToken token;
            JArray? dropArray;
            bool isPatched;

            // cycle through the list of asset addresses we want to patch
            foreach (AssetLocation assetLocation in settings.patchableAssetList) {
                asset = api.Assets.TryGet(assetLocation);

                if (asset == null) {
                    Mod.Logger.Error($"{Lang.Get("bearsbranchyharvest:patch-error-asset-missing")}{assetLocation}");
                    // The asset wasn't found. Skip it.
                    continue;
                }

                isPatched = false;

                // try to turn the JSON into a navigable object so we can traverse it, catching any errors so we can skip the patching gracefully
                try {
                    token = JToken.Parse(asset.ToText());

                    if (token is JObject leafObject) {
                        // the block code will help us sort out what logic to use. Unfortunately there's no one-size fits all patch for every leaf block.
                        string? blockCode = leafObject["code"]?.ToString();

                        // extract the drop list as an object we can traverse and alter
                        dropArray = leafObject["drops"] as JArray;
                        if (dropArray != null) {
                            // add knife drops if enabled. These have to go first in the drop array and be labeled with lastDrop
                            // so they execute first and stop stick and seed drops

                            // branchy drops are handled here, just one right now but more will come with mod compatibility
                            if (settings.AllowBranchyDropWithKnife) {
                                if (blockCode != null && blockCode.Equals("leavesbranchy")) {
                                    DropPresetObject branchyDrop = new("block", "leavesbranchy-placed-{wood}", "knife", 1f, 0f, true);
                                    dropArray.AddFirst(JToken.FromObject(branchyDrop));
                                    isPatched = true;
                                }
                            }

                            // leafy blocks have all sorts of naming schemes and I couldn't figure out a regular expression that could handle them all
                            if (settings.AllowLeafyDropWithKnife) {
                                if (blockCode != null && blockCode.Equals("leaves")) {
                                    DropPresetObject branchyDrop = new("block", "leaves-placed-{wood}", "knife", 1f, 0f, true);
                                    dropArray.AddFirst(JToken.FromObject(branchyDrop));
                                    isPatched = true;
                                }
                                if (blockCode != null && blockCode.Equals("bambooleaves")) {
                                    DropPresetObject branchyDrop = new("block", "bambooleaves-{color}-placed", "knife", 1f, 0f, true);
                                    dropArray.AddFirst(JToken.FromObject(branchyDrop));
                                    isPatched = true;
                                }
                                if (blockCode != null && blockCode.Equals("leavesnarrow")) {
                                    DropPresetObject branchyDrop = new("block", "leavesnarrow-placed-{wood}", "knife", 1f, 0f, true);
                                    dropArray.AddFirst(JToken.FromObject(branchyDrop));
                                    isPatched = true;
                                }
                            }

                            // iterate through the JSON drop array and search for seeds and sticks so we can modify them
                            foreach (JObject dropPreset in dropArray) {
                                string? dropCode = dropPreset["code"]?.ToString();

                                // alter stick drops if enabled
                                if (settings.AlterStickDrops) {
                                    if (dropCode is not null and "stick") {
                                        if (blockCode != null && blockCode.Contains("branchy")) {
                                            if (dropPreset["quantity"] is JObject dropObject) {
                                                dropObject["avg"] = settings.BranchyStickAverage;

                                                // not every quantity object has "var", so we need to do a little extra logic
                                                if (dropObject.ContainsKey("var")) {
                                                    dropObject["var"] = settings.BranchyStickVariance;
                                                }
                                                else {
                                                    dropObject.Add("var", JToken.FromObject(settings.BranchyStickVariance));
                                                }
                                                isPatched = true;
                                            }
                                        }
                                        else {
                                            if (dropPreset["quantity"] is JObject dropObject) {
                                                dropObject["avg"] = settings.LeafyStickAverage;

                                                // not every quantity object has "var", so we need to do a little extra logic
                                                if (dropObject.ContainsKey("var")) {
                                                    dropObject["var"] = settings.LeafyStickVariance;
                                                }
                                                else {
                                                    dropObject.Add("var", JToken.FromObject(settings.LeafyStickVariance));
                                                }
                                                isPatched = true;
                                            }
                                        }
                                    }
                                }

                                //    // alter seed drops if enabled
                                //    if (settings.AlterSeedDrops) {
                                //        if (dropCode != null && dropCode.Contains("treeseed-")) {
                                //            if (dropPreset["quantity"] is JObject dropObject) {
                                //                dropObject["avg"] = dropObject["avg"].ToObject<float>() * settings.SeedChanceMultiplier;
                                //                isPatched = true;
                                //            }
                                //            else if (dropPreset["quantityByType"] is dropObject) {
                                //            }
                                //        }
                                //    }
                                //}
                                //// add shears drops if enabled
                                //if (settings.AllowShearsBoostDrops) {
                                //    // work in progress
                                //}

                                if (isPatched) {
                                    // finalize altered JSON object and recommit asset
                                    Mod.Logger.Notification($"{Lang.Get("bearsbranchyharvest:patch-notification-patch-success")}{assetLocation}");
                                    asset.Data = System.Text.Encoding.UTF8.GetBytes(leafObject.ToString());
                                }
                                else {
                                    // handle the case that a provided asset can't accept any of the configured modifications, such as if we are given a dirt block or something.
                                    Mod.Logger.Notification($"{Lang.Get("bearsbranchyharvest:patch-notification-no-patch")}{assetLocation}");
                                }
                            }
                        }
                    }
                }
                catch (Exception e) {
                    // in case we run into a filesystem error or other horrible issue
                    Mod.Logger.Error($"{Lang.Get("bearsbranchyharvest:patch-error-asset-malformed")} {Lang.Get("bearsbranchyharvest:patch-bit-asset")} {assetLocation} {Lang.Get("bearsbranchyharvest:patch-bit-exception")} {e.Message}");
                }
            }
        }
    }

        #endregion Methods
}