using HarmonyLib;
using Vintagestory.API.Common;

namespace BearsBranchyHarvest
{
    /// <summary>An interface for any compatibility handlers from mod support libraries.</summary>
    public interface IFriendModHandler
    {
        Harmony SharedHarmony { get; set; }

        void FriendAssetFinalize(ICoreAPI api);

        void FriendStartServer(ICoreAPI api);

        void OnClose();
    }
}