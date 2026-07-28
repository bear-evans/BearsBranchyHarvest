using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using XSkills;

namespace BearsBranchyHarvest.AndFriends
{
    /// <summary>
    /// Handles compatibility between Bear's Branchy Harvest and XSkills. Namely, disables the built
    /// in leaf drop patch and instead replaces it with another one designed for Branchy Harvest.
    /// </summary>
    public class XSkillsFriendHandler : IFriendModHandler
    {
        public Harmony SharedHarmony { get => harmony; set => harmony = value; }
        private Harmony? harmony;

        public void FriendAssetFinalize(ICoreAPI api)
        {
        }

        public void FriendStartServer(ICoreAPI api)
        {
            XSkillsLeafPatchRemover(api);
            harmony?.PatchAll();
        }

        public void OnClose()
        {
            harmony?.UnpatchAll();
        }

        /// <summary>
        /// Uses the dark and forbidden magick of reflection to remove the patch that XSkills has
        /// that destroys any leaf block drops when the player is not holding a saw.
        /// </summary>
        public bool XSkillsLeafPatchRemover(ICoreAPI api)
        {
            try {
                Assembly? xskills = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name!.Equals("xskills", StringComparison.OrdinalIgnoreCase));

                if (xskills == null) {
                    api.Logger.Warning("XSkills mod detected, but unable to locate assembly. Compatibility patching failed.");
                    return false;
                }

                Type? xSkillType = xskills.GetType("XSkills.XSkillsLeavesBehavior");
                if (xSkillType == null) {
                    api.Logger.Warning("XSkills assembly found, but unable to find XSkillsLeavesBehavior. Compatibility patching failed.");
                    return false;
                }

                MethodInfo? targetMethod = xSkillType.GetMethod("GetDrops", BindingFlags.Public | BindingFlags.Instance);
                if (targetMethod == null) {
                    api.Logger.Warning("XSkills leaf behavior found, but unable to find GetDrops. Compatibility patching failed.");
                    return false;
                }

                Patches patches = Harmony.GetPatchInfo(targetMethod);
                if (patches == null) {
                    api.Logger.Warning("XSkillsLeafBehavior target method found, but no patches exist. Compatibility patching failed.");
                    return false;
                }

                Patch? targetPatch = patches.Postfixes.FirstOrDefault(p => p.PatchMethod.DeclaringType?.Name == "Patch_XSkillsLeavesBehavior_GetDrops");

                if (targetPatch == null) {
                    api.Logger.Warning("XSkillsLeafBehavior target method found, but could not find the specific patch. Compatibility patching failed.");
                    return false;
                }

                harmony!.Unpatch(targetMethod, targetPatch.PatchMethod);

                return true;
            }
            catch (Exception ex) {
                api.Logger.Error($"XSkills compatibility patching failed. {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// This patch is a replica of the original leaf patch, meant to solve the same problem but in a
    /// way that doesn't ruin Branchy Harvest.
    /// </summary>
    [HarmonyPatch(typeof(XSkillsLeavesBehavior), "GetDrops")]
    internal static class Patch_XSkillsLeavesBehavior_GetDrops
    {
        public static void Postfix(IPlayer byPlayer, ref ItemStack[] __result)
        {
            if (__result == null || __result.Length == 0) {
                return;
            }

            // if the currently held item is a knife, be cool.

            ItemStack? heldItem = byPlayer?.InventoryManager?.ActiveHotbarSlot?.Itemstack;
            if (heldItem != null && heldItem.Collectible?.Tool == EnumTool.Knife) {
                return;
            }

            // otherwise return everything but the leaf drops
            __result = __result
                .Where(stack => stack != null && stack.Collectible is not BlockLeaves)
                .ToArray();
        }
    }
}