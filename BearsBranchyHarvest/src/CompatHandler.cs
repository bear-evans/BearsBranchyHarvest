using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;

namespace BearsBranchyHarvest
{
    /// <summary>Handler for any cross-mod compatibility/incompatibility resolving.</summary>
    public class CompatHandler
    {
        #region Properties

        public static string HarmonyID {
            get => "bearsbranchyharvest";
        }

        #endregion Properties

        #region Fields

        private Harmony? harmony;

        #endregion Fields

        #region Public Methods

        /// <summary>Initializes a harmony instance to handle patching and unpatching.</summary>
        public void OnStart(ICoreAPI api)
        {
            harmony = new Harmony(HarmonyID);
        }

        /// <summary>Checks for any mods that require compatibility and patch them.</summary>
        public void TryRemoveXSkillsPatch(ICoreAPI api)
        {
            if (api.ModLoader.IsModEnabled("xskills")) {
                _ = XSkillsLeafPatchRemover(api);
            }
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

        #endregion Public Methods
    }
}