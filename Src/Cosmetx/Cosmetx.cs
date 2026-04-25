using BepInEx;
using GorillaNetworking;
using HarmonyLib;
using System.Reflection;

namespace CerealMenu.Cosmetx
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class PluginCosmetx : BaseUnityPlugin
    {
        public static PluginCosmetx instance;
        void Awake()
        {
            instance = this;
            HarmonyPatches.ApplyHarmonyPatches();
        }

        public void UnlockCosmetics()
        {
            MethodInfo UnlockItem = typeof(CosmeticsController).GetMethod("UnlockItem", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (CosmeticsController.CosmeticItem cosmeticItem in CosmeticsController.instance.allCosmetics)
            {
                if (!CosmeticsController.instance.concatStringCosmeticsAllowed.Contains(cosmeticItem.itemName))
                {
                    try
                    {
                        UnlockItem.Invoke(CosmeticsController.instance, new object[] { cosmeticItem.itemName, false });
                    }
                    catch { }
                }
            }

            CosmeticsController.instance.OnCosmeticsUpdated.Invoke();
        }
    }
}