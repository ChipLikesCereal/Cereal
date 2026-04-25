using GorillaNetworking;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CerealMenu.Cosmetx.Patches
{
    [HarmonyPatch(typeof(CosmeticsController.CosmeticSet), "LoadFromPlayerPreferences")]
    internal class NoNull : MonoBehaviour
    {
        private static bool Prefix(CosmeticsController.CosmeticSet __instance, CosmeticsController controller)
        {
            int num = 16;
            for (int i = 0; i < num; i++)
            {
                CosmeticsController.CosmeticSlots slot = (CosmeticsController.CosmeticSlots)i;
                CosmeticsController.CosmeticItem item = controller.GetItemFromDict(PlayerPrefs.GetString(CosmeticsController.CosmeticSet.SlotPlayerPreferenceName(slot), "NOTHING"));
                __instance.items[i] = item;
            }
            return false;
        }
    }
}
