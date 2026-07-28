using HarmonyLib;
using Vintagestory.API.Common;

namespace BearsBranchyHarvest
{
    /// <summary>An interface for any compatibility handlers from mod support libraries.</summary>
    public interface IFriendModHandler
    {
        #region Properties

        /// <summary>
        /// A reference to the shared Harmony instance used by all Bear's Branchy Harvest mod handlers.
        /// </summary>
        Harmony SharedHarmony { get; set; }

        #endregion Properties

        #region Public Methods

        /// <summary>Called for each friend mod during AssetsFinalize.</summary>
        void FriendAssetFinalize(ICoreAPI api);

        /// <summary>Called for each friend mod during OnStartServer.</summary>
        void FriendStartServer(ICoreAPI api);

        /// <summary>Called for each friend mod during Dispose.</summary>
        void OnClose();

        #endregion Public Methods
    }
}