using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
namespace CerealMenu
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool isMenuCreated;
        GameObject? menuObj;
        GameObject? HandMenuCollider;
        List<GameObject> btnObjs = new List<GameObject>();

        public static Plugin instance;

        public float pageSwitchcooldown = 0.4f;
        public int currentCategoryIndex = -1;
        public int currentPageIndex = 0;

        List<List<List<string>>> allCategories = new List<List<List<string>>>();
        List<string> catagoryName = new List<string> { "Movement", "Utility", "Rig Mods", "Menu Settings", "Mod Settings", "Admin" };

        List<List<string>> movementPages = new List<List<string>>
        {
            new List<string> {"Back", "Speed Boost", "Fly", "Long Arms", "Platforms", "Noclip"},
            new List<string> {"TP Gun", "Joystick Fly"}
        };
        List<List<string>> utilityPages = new List<List<string>>()
        {
            new List<string> {"Back", "Anti Report", "Get PID Gun", "Mute Gun", "Report Gun", "Mute Others"},
            new List<string> {"Mute all", "Unmute all"}
        };
        List<List<string>> RigModPages = new List<List<string>>()
        {
            new List<string> {"Back", "Ghost Monke","Upside Down Head", "Backwards Head", "Fix Head", "Follow Rig"},
            new List<string> {"Hold Rig", "Freeze Rig", "Rig Gun"}
        };
        List<List<string>> menuSettingPages = new List<List<string>>()
        {
            new List<string> {"Back", "Set Menu Blue", "Set Menu Red", "Set Menu White", "Set Menu Black"},
            new List<string> {"Set Text Blue", "Set Text Red", "Set Text White"},
        };
        List<List<string>> modSettingPages = new List<List<string>>()
        {
            new List<string> {"Back", "Set Plat Blue", "Set Plat Red", "Set Plat White", "Set Plat Black"},
            new List<string> {"Set Ghost Blue", "Set Ghost Red", "Set Ghost White", "Set Ghost Black"},
            new List<string> {"Fly Speed +", "Fly Speed -"}
        };
        List<List<string>> adminPages = new List<List<string>>()
        {
            new List<string> {"Back", "kick", "twerking carti", "no carti", "cool sword", "no sword"},
            new List<string> {"travis scott", "no travis"}
        };
        public ConfigEntry<bool> SpeedBoostEnabled;
        public ConfigEntry<bool> FlyEnabled;
        public ConfigEntry<bool> LongArmsEnabled;
        public ConfigEntry<bool> IsPlatformsEnabled;
        public ConfigEntry<bool> IsNoclipEnabled;
        public ConfigEntry<bool> IsJoystickFly;
        public ConfigEntry<bool> IsLockOntoRig;
        public ConfigEntry<bool> IsHoldRig;
        public ConfigEntry<bool> IsRigGun;
        public ConfigEntry<bool> IsFreezeRig;
        public ConfigEntry<bool> IsTPGun;
        public ConfigEntry<bool> IsGetPIDGun;
        public ConfigEntry<bool> IsMuteGun;
        public ConfigEntry<bool> IsMuteEveryoneExceptGun;
        public ConfigEntry<bool> IsReportGun;


        public ConfigEntry<bool> IsSilKick;
        public ConfigEntry<bool> IsTwerkingCarti;
        public ConfigEntry<bool> IsCoolSword;
        public ConfigEntry<bool> IsTravis;

        public ConfigEntry<bool> IsGrayScreenEnabled;


        public ConfigEntry<bool> IsUpsideDownHead;
        public ConfigEntry<bool> IsBackwardsHead;

        public ConfigEntry<bool> IsAntiReportEnabled;
        public ConfigEntry<bool> IsGhostMonke;

        public ConfigEntry<Color> PlatColorSave;
        public ConfigEntry<Color> MenuColorSave;
        public ConfigEntry<Color> TextColorSave;

        public ConfigEntry<float> FlySpeedSave;
        public ConfigEntry<Color> GhostColorSave;

        public bool IsAdmin = false;

        public static bool HasGrayScreened;

        public string menuversion;
        public void EnableAdminMenu()
        {
            IsAdmin = true;

            // Prevent duplicates
            if (!catagoryName.Contains("Admin"))
            {
                catagoryName.Add("Admin");
                allCategories.Add(adminPages);
            }

            // Refresh menu if it's open
            if (isMenuCreated)
            {
                DestroyMenu();
                CreateMenu();
            }
        }


        public async Task GetVer() // all this web request is doing is getting the version from the github repo, its not downloading anything on to your computer, relax.
        {
            using HttpClient client = new HttpClient();
            try
            {
                string url = "https://raw.githubusercontent.com/Literally-Reese-Literally/Cereal/refs/heads/main/Version";
                menuversion = (await client.GetStringAsync(url)).Trim();
            }
            catch (HttpRequestException e)
            {
                Logger.LogInfo(e);
            }
        }

        void Awake()
        {
            string dirPath = Path.Combine(BepInEx.Paths.GameRootPath, "Cereal");

            // Ensure directory exists
            Directory.CreateDirectory(dirPath);

            instance = this;
            Harmony harmony = new Harmony("Lexi.Cereal.Menu");
            harmony.PatchAll();

            Logger.LogInfo("Thank you");

            PlatColorSave = Config.Bind("Settings", "PlatColorSave", Color.white, "Platform Color");
            MenuColorSave = Config.Bind("Settings", "MenuColorSave", Color.white, "Menu Color");
            TextColorSave = Config.Bind("Settings", "TextColorSave", Color.white, "Text Color");


            FlySpeedSave = Config.Bind("Settings", "FlySpeedSave", 4f, "Fly Speed");
            GhostColorSave = Config.Bind("Settings", "GhostColorSave", Color.white, "Ghost Monke Color");



            SpeedBoostEnabled = Config.Bind("Movement", "Speed Boost", false, "A movement mod that lets you go really fast");
            FlyEnabled = Config.Bind("Movement", "Fly", false, "A movement mod that lets you fly around the map like your superman");
            LongArmsEnabled = Config.Bind("Movement", "Long Arms", false, "A movement mod that makes you seem very tall");
            IsPlatformsEnabled = Config.Bind("Movement", "Platforms", false, "A movement mod that puts convient platforms under your hand when you hold grip");
            IsNoclipEnabled = Config.Bind("Movement", "Noclip", false, "A movement mod that lets you go through stuff when holding right trigger");
            IsJoystickFly = Config.Bind("Movement", "Joystick Fly", false, "A movement mod that lets you fly with your joystick");
            IsLockOntoRig = Config.Bind("RigMods", "Lock Onto Rig", false, "A mod that locks you onto a rig");
            IsHoldRig = Config.Bind("RigMods", "Hold Rig", false, "A mod that lets you hold your rig");
            IsRigGun = Config.Bind("RigMods", "Rig Gun", false, "A mod that puts your rig where a gun is pointing");
            IsFreezeRig = Config.Bind("RigMods", "Freeze Rig", false, "A mod that freezes your rig but keeps it following your player");
            IsGetPIDGun = Config.Bind("Extras", "Get PID Gun", false, "A mod that copies someones Player id");
            IsMuteGun = Config.Bind("Extras", "Mute Gun", false, "A mod that mutes someone");
            IsMuteEveryoneExceptGun = Config.Bind("Extras", "Mute Others", false, "A mod that mutes everyone except a person");
            IsReportGun = Config.Bind("Extras", "Report Gun", false, "A mod that reports someone");
            IsGrayScreenEnabled = Config.Bind("Extras", "Gray Screen All", false, "A mod that makes everyones screen gray when masterclient.");


            IsSilKick = Config.Bind("Admin", "SilKick", false, "silent kick");
            IsTwerkingCarti = Config.Bind("Admin", "Twerking Carti", false, "twerking carti");
            IsCoolSword = Config.Bind("Admin", "Cool sword", false, "cool sword");
            IsTravis = Config.Bind("Admin", "travis", false, "travis scott");


            IsTPGun = Config.Bind("Movement", "Teleport Gun", false, "A mod that teleports you where a gun is pointed");
            IsAntiReportEnabled = Config.Bind("Extras", "Anti Report", false, "A mod that kicks you from the room when someone tries to report you");
            IsGhostMonke = Config.Bind("RigMods", "Ghost Monke", false, "A mod that freezes you when pressing A");
            IsUpsideDownHead = Config.Bind("RigMods", "Upside Down Head", false, "A mod that makes your head upside down");
            IsBackwardsHead = Config.Bind("RigMods", "Backwards Head", false, "A mod that makes your head backwards");

            allCategories.Add(movementPages);
            allCategories.Add(utilityPages);
            allCategories.Add(RigModPages);
            allCategories.Add(menuSettingPages);
            allCategories.Add(modSettingPages);
            allCategories.Add(adminPages);
        }
        // DO NOT REMOVE THIS! EVER! IT IS ALWAYS REQUIRED NO MATTER WHAT! THIS IS THE RIGPATCH, REMOVING IT WILL MAKE GHOST MONKE DETECTED AND IT **WILL** BAN YOU!
        [HarmonyPatch(typeof(VRRig), "OnDisable")]
        internal class GhostPatch : MonoBehaviour
        {

            public static bool Prefix(VRRig __instance)
            {
                if (__instance == VRRig.LocalRig) { return false; }
                return true;
            }
        }
        [HarmonyPatch(typeof(VRRigJobManager), "DeregisterVRRig")]
        public static class Bullshit
        {
            public static bool Prefix(VRRigJobManager __instance, VRRig rig) => !(__instance == VRRig.LocalRig);
        }
        [HarmonyPatch(typeof(VRRig), "PostTick")]
        public static class Bullshit2
        {
            public static bool Prefix(VRRig __instance) => !__instance.isLocal || __instance.enabled;
        }
        // DO NOT REMOVE THIS! EVER! IT IS ALWAYS REQUIRED NO MATTER WHAT! THIS IS THE RIGPATCH, REMOVING IT WILL MAKE GHOST MONKE DETECTED AND IT **WILL** BAN YOU!
        void Start()
        {
            isMenuCreated = false;
            _ = GetVer();
            gameObject.AddComponent<GunLib>();

            if (NotiLib.Instance == null)
            {
                var notiObj = new GameObject("NotiLib");
                DontDestroyOnLoad(notiObj);
                notiObj.AddComponent<NotiLib>();
            }
            Console.LoadConsole();
        }
        void OnJoinedRoom()
        {
            Mods.RefreshRigCache();
        }
        void Update()
        {
            if (pageSwitchcooldown > 0)
            {
                pageSwitchcooldown -= Time.deltaTime;
            }
            if (!isMenuCreated && ControllerInputPoller.instance.leftControllerSecondaryButton && !string.IsNullOrEmpty(menuversion) && menuversion == PluginInfo.Version)
            {
                CreateMenu();
            }
            else if (isMenuCreated && !ControllerInputPoller.instance.leftControllerSecondaryButton)
            {
                DestroyMenu();
            }
            if (ControllerInputPoller.instance.leftControllerSecondaryButton && !string.IsNullOrEmpty(menuversion) && menuversion != PluginInfo.Version)
            {
                NotiLib.SendNotifacation("Your menu is out of date! Please update!");
            }

            // Here is mod code



            if (SpeedBoostEnabled.Value) Mods.SpeedBoost();
            if (FlyEnabled.Value) Mods.Fly();
            if (LongArmsEnabled.Value) Mods.LongArms();
            if (!LongArmsEnabled.Value) Mods.UnLongArms();
            if (IsPlatformsEnabled.Value) Mods.Platforms();
            if (IsGrayScreenEnabled.Value && !HasGrayScreened)
            {
                Mods.ActivateGrayAll();
                HasGrayScreened = true;
            }
            if (IsAntiReportEnabled.Value) Mods.AntiReport();
            if (!IsGrayScreenEnabled.Value && HasGrayScreened)
            {
                Mods.DeactivateGrayAll();
                HasGrayScreened = false;
            }
            if (IsMuteGun.Value) Mods.MuteGun();
            if (IsReportGun.Value) Mods.ReportGun();
            if (IsGhostMonke.Value) Mods.GhostMonke();
            if (IsBackwardsHead.Value) Mods.BackwardsHead();
            if (IsUpsideDownHead.Value) Mods.UpsideDownNeck();
            if (IsNoclipEnabled.Value) Mods.Noclip();
            if (IsJoystickFly.Value) Mods.JoystickFly();
            if (IsLockOntoRig.Value) Mods.LockOntoRig();
            if (IsHoldRig.Value) Mods.HoldRig();
            if (IsRigGun.Value) Mods.RigGun();
            if (IsFreezeRig.Value) Mods.FreezeRig();
            if (IsTPGun.Value) Mods.TPGun();
            if (IsGetPIDGun.Value) Mods.GetPID();
            if (IsMuteEveryoneExceptGun.Value) Mods.MuteEveryoneExceptGun();
            if (IsSilKick.Value) Mods.silkickgun();
            if (IsTwerkingCarti.Value) Mods.TwerkingCarti();
            if (IsCoolSword.Value) Mods.Sword();
            if (IsTravis.Value) Mods.TravisScott();
            Mods.CreatePlayerOutline();

            // DONT REMOVE
        }
        public void CreateMenu()
        {
            var player = GorillaLocomotion.GTPlayer.Instance;
            isMenuCreated = true;
            menuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            menuObj.transform.parent = player.LeftHand.controllerTransform;
            menuObj.transform.localPosition = Vector3.zero;
            menuObj.transform.localRotation = Quaternion.identity;
            menuObj.transform.localScale = new Vector3(0.03f, 0.21f, 0.45f);

            var textObject = new GameObject("MenuLabel");
            textObject.transform.SetParent(menuObj.transform);
            textObject.transform.localPosition = new Vector3(0.55f, 0f, 0.45f);
            textObject.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);

            var text = textObject.AddComponent<TextMeshPro>();
            text.text = "Cereal";
            text.fontSize = 28;
            text.alignment = TextAlignmentOptions.Center;
            if (MenuColorSave.Value == Color.white)
            {
                text.color = Color.black;
            }
            else if (MenuColorSave.Value == Color.black)
            {
                text.color = Color.white;
            }
            else
            {
                text.color = Color.white;
            }
            text.enableAutoSizing = true;
            text.rectTransform.sizeDelta = new Vector2(50f, 40f);
            text.transform.localScale = new Vector3(0.016f, 0.01f, 0.01f);


            Destroy(menuObj.GetComponent<Rigidbody>());
            Destroy(menuObj.GetComponent<Collider>());

            var rend = menuObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = MenuColorSave.Value;
            // Hand Trigger
            HandMenuCollider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            HandMenuCollider.transform.parent = player.RightHand.controllerTransform;
            HandMenuCollider.transform.localPosition = Vector3.down * 0.094f;
            HandMenuCollider.transform.localRotation = Quaternion.identity;
            HandMenuCollider.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);
            Destroy(HandMenuCollider.GetComponent<Rigidbody>());
            var rendhand = HandMenuCollider.GetComponent<Renderer>();
            rendhand.material.shader = Shader.Find("GorillaTag/UberShader");
            rendhand.material.color = Color.white;
            // Hand Trigger
            if (currentCategoryIndex == -1)
            {
                float zOffset = 0.15f;
                foreach (string catName in catagoryName)
                {
                    AddButton(zOffset, 0f, 0.2f, catName);
                    zOffset -= 0.05f;
                }

                AddButton(0.3f, 0f, 0.2f, "Disconnect");
            }
            else
            {
                List<string> currentButtons = allCategories[currentCategoryIndex][currentPageIndex];
                float zOffset2 = 0.15f;

                foreach (string btnName in currentButtons)
                {
                    AddButton(zOffset2, 0f, 0.2f, btnName);
                    zOffset2 -= 0.05f;
                }

                AddButton(0.3f, 0f, 0.2f, "Disconnect");
                // AddButton(-0.1f, 0.153f, 0.1f, "Back");
                AddButton(-0.15f, 0.06f, 0.1f, "Previous");
                AddButton(-0.15f, -0.06f, 0.1f, "Next");
            }
        }
        public void DestroyMenu()
        {
            isMenuCreated = false;
            GameObject.Destroy(menuObj);
            GameObject.Destroy(HandMenuCollider);
            DestroyAllButtons();
            Plugin.instance.Config.Save();
        }
        public void NextPage()
        {
            List<List<string>> currentCategory = allCategories[currentCategoryIndex];
            currentPageIndex = (currentPageIndex + 1) % currentCategory.Count;
            DestroyMenu();
            CreateMenu();
        }
        public void PreviousPage()
        {
            List<List<string>> currentCategory = allCategories[currentCategoryIndex];
            currentPageIndex = (currentPageIndex - 1 + currentCategory.Count) % currentCategory.Count;
            DestroyMenu();
            CreateMenu();
        }
        void AddButton(float zoffset, float yOffset, float sOffset, string btnName)
        {
            GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var player = GorillaLocomotion.GTPlayer.Instance;
            var follow = btnObj.AddComponent<FollowMenu>();
            follow.target = player.LeftHand.controllerTransform;
            follow.position = new Vector3(0.015f, yOffset, zoffset);
            follow.rotation = Quaternion.identity;
            btnObj.transform.localScale = new Vector3(0.03f, sOffset, 0.04f);
            var rend = btnObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = Color.black;
            btnObj.GetComponent<Collider>().isTrigger = true;
            btnObj.layer = 2;
            var trigger = btnObj.AddComponent<ButtonCollider>();
            trigger.btnIdentifier = btnName;
            var textObject = new GameObject("ButtonLabel");
            textObject.transform.SetParent(btnObj.transform);
            textObject.transform.localPosition = new Vector3(0.55f, 0f, 0f);
            textObject.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);

            var text = textObject.AddComponent<TextMeshPro>();
            text.text = btnName;
            text.alignment = TextAlignmentOptions.Center;
            text.color = TextColorSave.Value;
            text.enableAutoSizing = true;
            text.rectTransform.sizeDelta = new Vector2(50f, 40f);
            if (btnName == "Previous" || btnName == "Next") // btnName == "Back"
            {
                textObject.transform.localScale = new Vector3(0.02f, 0.05f, 0.04f);
            }
            else if (btnName == "Set Menu White" || btnName == "Set Ghost White" || btnName == "Upside Down Head" || btnName == "Backwards Head" || btnName == "Set Menu Black" || btnName == "Set Ghost Black")
            {
                textObject.transform.localScale = textObject.transform.localScale = new Vector3(0.007f, 0.06f, 0.37f);
            }
            else
            {
                textObject.transform.localScale = new Vector3(0.01f, 0.09f, 0.40f);
            }
            btnObjs.Add(btnObj);
        }
        void DestroyAllButtons()
        {
            foreach (GameObject btnObj in btnObjs)
            {
                Destroy(btnObj);
            }
        }
        public class FollowMenu : MonoBehaviour
        {
            public Transform target;
            public Vector3 position;
            public Quaternion rotation;

            void LateUpdate()
            {
                transform.position = target.TransformPoint(position);
                transform.rotation = target.rotation * rotation;
            }
        }

        public class ButtonCollider : MonoBehaviour
        {
            public string btnIdentifier;
            public bool isTogglable;
            public bool isToggled;

            void Start()
            {
                // Initialize toggle state based on plugin config
                switch (btnIdentifier)
                {
                    case "Speed Boost":
                        isTogglable = true;
                        isToggled = Plugin.instance.SpeedBoostEnabled.Value;
                        break;
                    case "Fly":
                        isTogglable = true;
                        isToggled = Plugin.instance.FlyEnabled.Value;
                        break;
                    case "Long Arms":
                        isTogglable = true;
                        isToggled = Plugin.instance.LongArmsEnabled.Value;
                        break;
                    case "Platforms":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsPlatformsEnabled.Value;
                        break;
                    case "Noclip":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsNoclipEnabled.Value;
                        break;
                    case "Joystick Fly":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsJoystickFly.Value;
                        break;
                    case "Follow Rig":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsLockOntoRig.Value;
                        break;
                    case "Rig Gun":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsRigGun.Value;
                        break;
                    case "Hold Rig":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsHoldRig.Value;
                        break;
                    case "Freeze Rig":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsFreezeRig.Value;
                        break;
                    case "Gray Screen All":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsGrayScreenEnabled.Value;
                        break;
                    case "TP Gun":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsTPGun.Value;
                        break;
                    case "Get PID Gun":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsGetPIDGun.Value;
                        break;
                    case "Mute Gun":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsMuteGun.Value;
                        break;
                    case "Mute Others":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsMuteEveryoneExceptGun.Value;
                        break;
                    case "Report Gun":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsReportGun.Value;
                        break;
                    case "Anti Report":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsAntiReportEnabled.Value;
                        break;
                    case "Ghost Monke":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsGhostMonke.Value;
                        break;
                    case "Upside Down Head":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsUpsideDownHead.Value;
                        break;
                    case "Backwards Head":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsBackwardsHead.Value;
                        break;
                    case "kick":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsSilKick.Value;
                        break;
                    case "twerking carti":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsTwerkingCarti.Value;
                        break;
                    case "cool sword":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsCoolSword.Value;
                        break;
                    case "travis scott":
                        isTogglable = true;
                        isToggled = Plugin.instance.IsTravis.Value;
                        break;
                    default:
                        isTogglable = false;
                        isToggled = false;
                        break;
                }

                // Set initial color
                GetComponent<Renderer>().material.color = isToggled ? Color.blue : Color.black;
            }

            void OnTriggerEnter(Collider other)
            {
                // Only respond if hand collider touches
                if (other.gameObject == Plugin.instance.HandMenuCollider)
                {
                    if (Plugin.instance.pageSwitchcooldown > 0) return;

                    if (isTogglable)
                    {
                        isToggled = !isToggled;
                        GetComponent<Renderer>().material.color = isToggled ? Color.blue : Color.black;
                    }

                    HandleButtonAction();
                    Plugin.instance.pageSwitchcooldown = 0.4f;
                }
            }

            void HandleButtonAction()
            {
                switch (btnIdentifier)
                {
                    case "Speed Boost":
                        Plugin.instance.SpeedBoostEnabled.Value = isToggled;
                        NotiLib.SendNotifacation("Makes you really fast");
                        Plugin.instance.Config.Save();
                        
                        break;
                    case "Fly":
                        Plugin.instance.FlyEnabled.Value = isToggled;
                        NotiLib.SendNotifacation("Lets You Fly");
                        Plugin.instance.Config.Save();
                        break;
                    case "Long Arms":
                        Plugin.instance.LongArmsEnabled.Value = isToggled;
                        NotiLib.SendNotifacation("Makes your arms longer");
                        Plugin.instance.Config.Save();
                        break;
                    case "Platforms":
                        Plugin.instance.IsPlatformsEnabled.Value = isToggled;
                        NotiLib.SendNotifacation("Spawns convient platforms under your hands when you press your grips");
                        Plugin.instance.Config.Save();
                        break;
                    case "Noclip":
                        Plugin.instance.IsNoclipEnabled.Value = isToggled;
                        NotiLib.SendNotifacation("Lets you go through stuff");
                        Plugin.instance.Config.Save();
                        break;
                    case "Joystick Fly":
                        Plugin.instance.IsJoystickFly.Value = isToggled;
                        NotiLib.SendNotifacation("Bark fly");
                        if (Plugin.instance.IsJoystickFly.Value)
                        Plugin.instance.Config.Save();
                        break;
                    case "Follow Rig":
                        Plugin.instance.IsLockOntoRig.Value = isToggled;
                        NotiLib.SendNotifacation("Makes you follow a rig");
                        Plugin.instance.Config.Save();
                        break;
                    case "Rig Gun":
                        Plugin.instance.IsRigGun.Value = isToggled;
                        NotiLib.SendNotifacation("Puts your rig where a gun is pointed");
                        Plugin.instance.Config.Save();
                        break;
                    case "Freeze Rig":
                        Plugin.instance.IsFreezeRig.Value = isToggled;
                        NotiLib.SendNotifacation("Freezes your rig but keeps it following your player");
                        Plugin.instance.Config.Save();
                        break;
                    case "Hold Rig":
                        Plugin.instance.IsHoldRig.Value = isToggled;
                        NotiLib.SendNotifacation("Lets you hold your rig");
                        Plugin.instance.Config.Save();
                        break;
                    case "TP Gun":
                        Plugin.instance.IsTPGun.Value = isToggled;
                        NotiLib.SendNotifacation("Teleports you where a gun is pointed");
                        Plugin.instance.Config.Save();
                        break;
                    case "Get PID Gun":
                        Plugin.instance.IsGetPIDGun.Value = isToggled;
                        NotiLib.SendNotifacation("Gets someones player id");
                        Plugin.instance.Config.Save();
                        break;
                    case "Mute Gun":
                        Plugin.instance.IsMuteGun.Value = isToggled;
                        NotiLib.SendNotifacation("Mutes Someone");
                        Plugin.instance.Config.Save();
                        break;
                    case "Mute Others":
                        Plugin.instance.IsMuteEveryoneExceptGun.Value = isToggled;
                        NotiLib.SendNotifacation("Mutes everyone except the selected person");
                        Plugin.instance.Config.Save();
                        break;
                    case "Mute all":
                        Mods.MuteAll();
                        NotiLib.SendNotifacation("Mutes all");
                        break;
                    case "Unmute all":
                        Mods.UnmuteAll();
                        NotiLib.SendNotifacation("Unmutes all");
                        break;
                    case "Report Gun":
                        Plugin.instance.IsReportGun.Value = isToggled;
                        NotiLib.SendNotifacation("Reports Someone");
                        Plugin.instance.Config.Save();
                        break;
                    case "Gray Screen All":
                        Plugin.instance.IsGrayScreenEnabled.Value = isToggled;
                        NotiLib.SendNotifacation("Makes everyones screen gray");
                        Plugin.instance.Config.Save();
                        break;
                    case "Anti Report":
                        Plugin.instance.IsAntiReportEnabled.Value = isToggled;
                        NotiLib.SendNotifacation("Disconnects you when someone tries to report you");
                        Plugin.instance.Config.Save();
                        break;
                    case "Ghost Monke":
                        Plugin.instance.IsGhostMonke.Value = isToggled;
                        NotiLib.SendNotifacation("Makes you appear frozen when you press A");
                        Plugin.instance.Config.Save();
                        break;
                    case "Upside Down Head":
                        Plugin.instance.IsUpsideDownHead.Value = isToggled;
                        Plugin.instance.Config.Save();
                        break;
                    case "Backwards Head":
                        Plugin.instance.IsBackwardsHead.Value = isToggled;
                        Plugin.instance.Config.Save();
                        break;
                    case "Fix Head":
                        Mods.FixHead();
                        break;
                    case "Set Plat Blue":
                        Plugin.instance.PlatColorSave.Value = Color.blue;
                        Plugin.instance.Config.Save();
                        break;
                    case "Set Plat Red":
                        Plugin.instance.PlatColorSave.Value = Color.red;
                        Plugin.instance.Config.Save();
                        break;
                    case "Set Plat White":
                        Plugin.instance.PlatColorSave.Value = Color.white;
                        Plugin.instance.Config.Save();
                        break;
                    case "Set Plat Black":
                        Plugin.instance.PlatColorSave.Value = Color.black;
                        Plugin.instance.Config.Save();
                        break;
                    case "Set Menu Blue":
                        Plugin.instance.MenuColorSave.Value = Color.blue;
                        Plugin.instance.Config.Save();
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Set Menu Red":
                        Plugin.instance.MenuColorSave.Value = Color.red;
                        Plugin.instance.Config.Save();
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Set Menu White":
                        Plugin.instance.MenuColorSave.Value = Color.white;
                        Plugin.instance.Config.Save();
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Set Menu Black":
                        Plugin.instance.MenuColorSave.Value = Color.black;
                        Plugin.instance.Config.Save();
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Set Text Blue":
                        Plugin.instance.TextColorSave.Value = Color.blue;
                        Plugin.instance.Config.Save();
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Set Text Red":
                        Plugin.instance.TextColorSave.Value = Color.red;
                        Plugin.instance.Config.Save();
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Set Text White":
                        Plugin.instance.TextColorSave.Value = Color.white;
                        Plugin.instance.Config.Save();
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;


                    case "Set Ghost Blue":
                        Plugin.instance.GhostColorSave.Value = Color.blue;
                        Plugin.instance.Config.Save();
                        break;
                    case "Set Ghost Red":
                        Plugin.instance.GhostColorSave.Value = Color.red;
                        Plugin.instance.Config.Save();
                        break;
                    case "Set Ghost White":
                        Plugin.instance.GhostColorSave.Value = Color.white;
                        Plugin.instance.Config.Save();
                        break;
                    case "Set Ghost Black":
                        Plugin.instance.GhostColorSave.Value = Color.black;
                        Plugin.instance.Config.Save();
                        break;
                    case "Fly Speed +":
                        Plugin.instance.FlySpeedSave.Value += 0.1f;
                        NotiLib.SendNotifacation(Plugin.instance.FlySpeedSave.Value.ToString());
                        Plugin.instance.Config.Save();
                        break;
                    case "Fly Speed -":
                        if (Plugin.instance.FlySpeedSave.Value > 0.1)
                            Plugin.instance.FlySpeedSave.Value -= 0.1f;
                        NotiLib.SendNotifacation(Plugin.instance.FlySpeedSave.Value.ToString());
                        Plugin.instance.Config.Save();
                        break;
                    case "Next":
                        Plugin.instance.NextPage();
                        break;
                    case "Previous":
                        Plugin.instance.PreviousPage();
                        break;
                    case "Movement":
                        Plugin.instance.currentCategoryIndex = 0;
                        Plugin.instance.currentPageIndex = 0;
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Utility":
                        Plugin.instance.currentCategoryIndex = 1;
                        Plugin.instance.currentPageIndex = 0;
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Rig Mods":
                        Plugin.instance.currentCategoryIndex = 2;
                        Plugin.instance.currentPageIndex = 0;
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Menu Settings":
                        Plugin.instance.currentCategoryIndex = 3;
                        Plugin.instance.currentPageIndex = 0;
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Mod Settings":
                        Plugin.instance.currentCategoryIndex = 4;
                        Plugin.instance.currentPageIndex = 0;
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "Admin":
                        if (Plugin.instance.IsAdmin)
                        {
                            Plugin.instance.currentCategoryIndex = 5;
                            Plugin.instance.currentPageIndex = 0;
                            Plugin.instance.DestroyMenu();
                            Plugin.instance.CreateMenu();
                        }
                        break;
                    case "Back":
                        Plugin.instance.currentCategoryIndex = -1;
                        Plugin.instance.currentPageIndex = 0;
                        Plugin.instance.DestroyMenu();
                        Plugin.instance.CreateMenu();
                        break;
                    case "cherry bomb":
                        Plugin.instance.IsSilKick.Value = isToggled;
                        Plugin.instance.Config.Save();
                        break;
                    case "twerking carti":
                        Plugin.instance.IsTwerkingCarti.Value = isToggled;
                        Plugin.instance.Config.Save();
                        break;
                    case "cool sword":
                        Plugin.instance.IsCoolSword.Value = isToggled;
                        Plugin.instance.Config.Save();
                        break;
                    case "no carti":
                        Mods.NoCarti();
                        break;
                    case "no sword":
                        Mods.NoSword();
                        break;
                    case "travis scott":
                        Plugin.instance.IsTravis.Value = isToggled;
                        Plugin.instance.Config.Save();
                        break;
                    case "no travis":
                        Mods.NoTravis();
                        break;
                    case "Disconnect":
                        Photon.Pun.PhotonNetwork.Disconnect();
                        break;
                }
            }
        }
    }
}
