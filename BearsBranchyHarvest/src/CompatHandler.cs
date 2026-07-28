using System;
using System.Collections.Generic;
using System.IO;
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

        public List<IFriendModHandler> GetFriendMods(ICoreAPI api)
        {
            List<IFriendModHandler> friends = [];

            if (api.ModLoader.IsModEnabled("xskills")) {
                IFriendModHandler? xskillsHandler = GetXSkillsFriendAssembly(api);
                if (xskillsHandler != null) {
                    friends.Add(xskillsHandler);
                    xskillsHandler.SharedHarmony = harmony!;
                }
            }
            return friends;
        }

        public IFriendModHandler? GetXSkillsFriendAssembly(ICoreAPI api)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

#if DEBUG
            string[] resourceNames = assembly.GetManifestResourceNames();
            foreach (string resourceName in resourceNames) {
                api.Logger.Debug(resourceName);
            }
#endif

            using Stream? stream = assembly.GetManifestResourceStream("BearsBranchyHarvest.BranchyHarvestAndFriends.dll");
            if (stream == null) {
                api.Logger.Warning("XSkills detected, but Branchy Harvest was unable to load the compatibility DLL.");
                return null;
            }
            byte[] assemblyData = new byte[stream.Length];
            _ = stream.Read(assemblyData, 0, assemblyData.Length);

            Assembly compatAssembly = Assembly.Load(assemblyData);

            Type? handlerType = compatAssembly.GetType("BearsBranchyHarvest.AndFriends.XSkillsFriendHandler");
            if (handlerType == null) {
                api.Logger.Warning($"Failed to find XSkillsFriendHandler type.");
                return null;
            }

            if (Activator.CreateInstance(handlerType) is not IFriendModHandler handler) {
                api.Logger.Warning($"Failed to create an instance of {handlerType}");
                return null;
            }

            return handler;
        }

        #endregion Public Methods
    }
}