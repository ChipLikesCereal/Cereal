using GorillaNetworking;
using GorillaNetworking.Store;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace CerealMenu.Cosmetx.Patches
{
    [HarmonyPatch(typeof(StoreUpdater))]
    [HarmonyPatch("Initialize", MethodType.Normal)]
    public class PostGetData
    {
        private static void Postfix() =>
            PluginCosmetx.instance.UnlockCosmetics();
    }
}
