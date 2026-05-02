using BepInEx;
using BepInEx.Configuration;
using GorillaLocomotion;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Networking;
using UnityEngine.Windows;

namespace CerealMenu
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance { get; private set; }
        public static bool isPcMenu;
        public static bool hasGivenNotif = false;
        public static int Pages = 0;
        private bool isMenuCreated;
        private GameObject menuObj;
        private GameObject menuPrefab;
        private AssetBundle menuBundle;

        private Vector3 menuForwardOffset = new Vector3(0.08f, 0f, 0f);
        private void LoadMenuAssetBundle()
        {
            var assembly = Assembly.GetExecutingAssembly();

            string resourceName = "CerealMenu.EmbedResources.menuobject";

            if (menuBundle == null)
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Logger.LogError($"rsrc not fdound");
                        return;
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        menuBundle = AssetBundle.LoadFromMemory(ms.ToArray());
                    }
                }
            }

            if (menuBundle == null)
            {
                Logger.LogError("asset bundle failed to load");
                return;
            }

            menuPrefab = menuBundle.LoadAsset<GameObject>("Assets/MenuObject.prefab");

            if (menuPrefab == null)
                Logger.LogError("prefab not found");
        }
        private void LoadSound()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                string resourceName = "CerealMenu.EmbedResources.buttonpress.ogg";

                using Stream stream = assembly.GetManifestResourceStream(resourceName);

                if (stream == null)
                {
                    Logger.LogError("sound rsrc not found");
                    return;
                }

                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);

                var tempClip = WavOrOggToAudioClip(data);

                if (tempClip != null)
                {
                    cachedClip = tempClip;
                    soundReady = true;
                }
            }
            catch (Exception e)
            {
                Logger.LogError("failed loading embedded sound " + e);
            }
        }
        private AudioClip WavOrOggToAudioClip(byte[] data)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "cereal_click.ogg");
            File.WriteAllBytes(tempPath, data);

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.OGGVORBIS);
            var op = www.SendWebRequest();

            while (!op.isDone) { }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Logger.LogError(www.error);
                return null;
            }

            return DownloadHandlerAudioClip.GetContent(www);
        }

        public GameObject HandMenuCollider;
        public List<GameObject> btnObjs = new List<GameObject>();
        public float globalClickCooldown = 0f;
        public int currentCategoryIndex = -1;
        public int currentPageIndex = 0;

        public AudioSource audioSource;
        private AudioClip cachedClip;
        private bool soundReady;
        private Coroutine rgbCoroutine;
        public string menuversion;

        public ConfigEntry<bool> SpeedBoostEnabled, FlyEnabled, LongArmsEnabled, IsPlatformsEnabled, IsNoclipEnabled,
                                 IsJoystickFly, IsGroundHelper, IsAmplifiedMonke, IsWebSlingers, 
                                 IsLockOntoRig, IsHoldRig, IsRigGun, IsFreezeRig, IsTPGun, IsGetPIDGun,
                                 IsMuteGun, IsMuteEveryoneExceptGun, IsReportGun, IsSilKick, IsTwerkingCarti,
                                 IsCoolSword, IsTravis, IsPhone, IsAdminGrab, IsKormakur,
                                 IsAxe, IsBigAssets, IsTv, IsUpsideDownHead, IsBackwardsHead,
                                 IsAntiReportEnabled, IsGhostMonke, IsMenuRGB, IsInvisPlat, IsFunnyRig, IsRecroomTorso, IsRecroomRig, IsRealisticLooking, ShowHandCollider, AdminLaser;

        public ConfigEntry<Color> Theme;
        public ConfigEntry<float> FlySpeedSave;
        public bool IsAdmin = false;
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
        [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
        public class TorsoPatch
        {
            private static Quaternion frozenRotation;
            private static bool hasFrozenRotation = false;
            public static event Action VRRigLateUpdate;
            public static bool enabled;
            public static int mode = 0;
            private static float storedTorsoYaw;
            private static bool hasStoredYaw = false;

            public static void Postfix(VRRig __instance)
            {
                if (__instance.isLocal)
                {
                    if (enabled)
                    {
                        Quaternion rotation = Quaternion.identity;
                        switch (mode)
                        {
                            case 0:
                                rotation = Quaternion.Euler(0f, Time.time * 180f % 360, 0f);
                                break;
                            case 1:
                                rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                                break;
                            case 2:
                                rotation = Quaternion.Euler(0f, GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles.y + 180f, 0f);
                                break;
                            case 3:
                                rotation = Quaternion.Euler(0f, Mods.recBodyRotary.transform.rotation.eulerAngles.y, 0f);
                                break;
                            case 4:
                                if (!hasFrozenRotation)
                                {
                                    frozenRotation = __instance.transform.rotation;
                                    hasFrozenRotation = true;
                                }

                                rotation = frozenRotation;
                                break;
                            case 5:
                                {
                                    Transform a = GTPlayer.Instance.LeftHand.controllerTransform;
                                    Transform b = GTPlayer.Instance.RightHand.controllerTransform;

                                    Vector3 pos = __instance.transform.position;

                                    Vector3 dirA = a.position - pos;
                                    Vector3 dirB = b.position - pos;

                                    dirA.y = 0f;
                                    dirB.y = 0f;

                                    dirA.Normalize();
                                    dirB.Normalize();

                                    Vector3 currentForward = __instance.transform.forward;
                                    currentForward.y = 0f;
                                    currentForward.Normalize();


                                    float dot = Vector3.Dot(dirA, dirB);

                                    Vector3 blendedDir;

                                    if (dot < -0.5f)
                                    {
                                        float dotA = Vector3.Dot(currentForward, dirA);
                                        float dotB = Vector3.Dot(currentForward, dirB);

                                        blendedDir = (dotA > dotB) ? dirA : dirB;
                                    }
                                    else
                                    {
                                        blendedDir = dirA + dirB;
                                    }

                                    if (blendedDir.sqrMagnitude > 0.0001f)
                                    {
                                        blendedDir.Normalize();

                                        float angle = Vector3.SignedAngle(currentForward, blendedDir, Vector3.up);

                                        float maxAngle = 100f;
                                        float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);

                                        Quaternion clampedRot = Quaternion.AngleAxis(clampedAngle, Vector3.up);
                                        Vector3 finalForward = clampedRot * currentForward;

                                        rotation = Quaternion.LookRotation(finalForward, Vector3.up);
                                    }

                                    break;
                                }
                            case 6:
                                {
                                    float deadzone = 40f;
                                    float baseSpeed = 60f;
                                    float maxSpeed = 960f;

                                    float headYaw = GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles.y;

                                    Transform lefthand = GTPlayer.Instance.LeftHand.controllerTransform;
                                    Transform righthand = GTPlayer.Instance.RightHand.controllerTransform;
                                    Vector3 pos = __instance.transform.position;

                                    Vector3 dirA = lefthand.position - pos;
                                    Vector3 dirB = righthand.position - pos;

                                    dirA.y = 0f;
                                    dirB.y = 0f;

                                    float handYaw = headYaw;

                                    if (dirA.sqrMagnitude > 0.001f && dirB.sqrMagnitude > 0.001f)
                                    {
                                        dirA.Normalize();
                                        dirB.Normalize();

                                        Vector3 blended = dirA + dirB;

                                        if (blended.sqrMagnitude > 0.001f)
                                        {
                                            blended.Normalize();
                                            handYaw = Quaternion.LookRotation(blended, Vector3.up).eulerAngles.y;
                                        }
                                    }

                                    float handWeight = 0.15f;

                                    float targetYaw = Mathf.LerpAngle(headYaw, handYaw, handWeight);

                                    if (!hasStoredYaw)
                                    {
                                        storedTorsoYaw = targetYaw;
                                        hasStoredYaw = true;
                                    }

                                    float delta = Mathf.DeltaAngle(storedTorsoYaw, targetYaw);
                                    float absDelta = Mathf.Abs(delta);

                                    if (absDelta > deadzone)
                                    {
                                        float excess = absDelta - deadzone;

                                        float speed = Mathf.Min(maxSpeed, baseSpeed * (excess / 30f));

                                        storedTorsoYaw += Mathf.Sign(delta) * speed * Time.deltaTime;
                                    }

                                    rotation = Quaternion.Euler(0f, storedTorsoYaw, 0f);
                                    break;
                                }
                        }
                            if (mode != 4)
                            {
                                hasFrozenRotation = false;
                            }
                            if (mode != 6)
                            {
                            hasStoredYaw = false;
                            }

                        __instance.transform.rotation = rotation;
                            __instance.head.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                            __instance.leftHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                            __instance.rightHand.MapMine(__instance.scaleFactor, __instance.playerOffsetTransform);
                        }

                        VRRigLateUpdate?.Invoke();
                    }
                }
            }

        void Awake()
        {
            LoadMenuAssetBundle();
            instance = this;
            string dirPath = Path.Combine(BepInEx.Paths.GameRootPath, "Cereal");
            Directory.CreateDirectory(dirPath);

            Harmony harmony = new Harmony(PluginInfo.GUID);
            harmony.PatchAll();

            Theme = Config.Bind("Settings", "Theme", Color.white, "");
            FlySpeedSave = Config.Bind("Settings", "FlySpeedSave", 4f, "");

            IsMenuRGB = Config.Bind("Settings", "Rgb Mode", false, "");
            IsInvisPlat = Config.Bind("Settings", "Invis Plats", false, "");
            ShowHandCollider = Config.Bind("Settings", "Show Hand Collider", true, "");

            SpeedBoostEnabled = Config.Bind("Movement", "Speed Boost", false, "");
            FlyEnabled = Config.Bind("Movement", "Fly", false, "");
            LongArmsEnabled = Config.Bind("Movement", "Long Arms", false, "");
            IsPlatformsEnabled = Config.Bind("Movement", "Platforms", false, "");
            IsNoclipEnabled = Config.Bind("Movement", "Noclip", false, "");
            IsJoystickFly = Config.Bind("Movement", "Joystick Fly", false, "");
            IsGroundHelper = Config.Bind("Movement", "Ground Helper", false, "");
            IsAmplifiedMonke = Config.Bind("Movement", "Amplified Monke", false, "");
            IsWebSlingers = Config.Bind("Movement", "Web Slingers", false, "");
            IsTPGun = Config.Bind("Movement", "Teleport Gun", false, "");

            IsGhostMonke = Config.Bind("Rig", "Ghost Monke", false, "");
            IsLockOntoRig = Config.Bind("Rig", "Lock Rig", false, "");
            IsHoldRig = Config.Bind("Rig", "Hold Rig", false, "");
            IsRigGun = Config.Bind("Rig", "Rig Gun", false, "");
            IsFreezeRig = Config.Bind("Rig", "Freeze Rig", false, "");
            IsUpsideDownHead = Config.Bind("Rig", "Upside Down Head", false, "");
            IsBackwardsHead = Config.Bind("Rig", "Backwards Head", false, "");
            IsFunnyRig = Config.Bind("Rig", "Funny Rig", false, "");
            IsRecroomTorso = Config.Bind("Rig", "Recroom Torso", false, "");
            IsRecroomRig = Config.Bind("Rig", "Recroom Rig", false, "");
            IsRealisticLooking = Config.Bind("Rig", "Realistic Looking", false, "");

            IsGetPIDGun = Config.Bind("Utility", "Get PID Gun", false, "");
            IsMuteGun = Config.Bind("Utility", "Mute Gun", false, "");
            IsMuteEveryoneExceptGun = Config.Bind("Utility", "Mute Others", false, "");
            IsReportGun = Config.Bind("Utility", "Report Gun", false, "");
            IsAntiReportEnabled = Config.Bind("Utility", "Anti Report", false, "");

            IsSilKick = Config.Bind("Admin", "SilKick", false, "");
            IsTwerkingCarti = Config.Bind("Admin", "Twerking Carti", false, "");
            IsCoolSword = Config.Bind("Admin", "Cool Sword", false, "");
            IsTravis = Config.Bind("Admin", "Travis Scott", false, "");
            IsPhone = Config.Bind("Admin", "Phone", false, "");
            IsAdminGrab = Config.Bind("Admin", "Grab All", false, "");
            IsKormakur = Config.Bind("Admin", "Kormakur", false, "");
            IsAxe = Config.Bind("Admin", "Axe", false, "");
            IsBigAssets = Config.Bind("Admin", "Big Assets", false, "");
            IsTv = Config.Bind("Admin", "TV", false, "");
            AdminLaser = Config.Bind("Admin", "Laser", false, "");

            audioSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            LoadSound();
        }

        void Start()
        {
            _ = Getver();
            gameObject.AddComponent<GunLib>();
            if (NotiLib.Instance == null)
            {
                var notiObj = new GameObject("NotiLib");
                DontDestroyOnLoad(notiObj);
                notiObj.AddComponent<NotiLib>();
            }
            Console.LoadConsole();
        }

        public void EnableAdminMenu()
        {
            IsAdmin = true;
            if (isMenuCreated) RefreshMenu();
        }
        void Update()
        {
            if (globalClickCooldown > 0) globalClickCooldown -= Time.deltaTime;
            if (ControllerInputPoller.instance.leftControllerSecondaryButton && !isMenuCreated)
            {
                CreateMenu();
            }
            if (!ControllerInputPoller.instance.leftControllerSecondaryButton && isMenuCreated)
            {
                DestroyMenu(false);
            }

            if (SpeedBoostEnabled.Value) Mods.SpeedBoost();
            if (FlyEnabled.Value) Mods.Fly();
            if (LongArmsEnabled.Value) Mods.LongArms();
            if (IsPlatformsEnabled.Value) Mods.Platforms();
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
            if (IsAntiReportEnabled.Value) Mods.AntiReport();
            if (IsTwerkingCarti.Value) Mods.TwerkingCarti();
            if (IsCoolSword.Value) Mods.Sword();
            if (IsTravis.Value) Mods.TravisScott();
            if (IsPhone.Value) Mods.Samsung();
            if (IsAdminGrab.Value) Mods.AdminGrabAll();
            if (IsKormakur.Value) Mods.KormakurFemboys();
            if (IsAxe.Value) Mods.Axe();
            if (IsTv.Value) Mods.SkidTV();
            if (IsGroundHelper.Value) Mods.GroundHelper();
            if (IsAmplifiedMonke.Value) Mods.AmplifiedMonke();
            if (IsWebSlingers.Value) Mods.WebSlingers();
            if (IsFunnyRig.Value) Mods.MessUpRig();
            if (IsRecroomTorso.Value) Mods.RecRoomTorso();
            if (IsRecroomRig.Value) Mods.RecRoomRig();
            if (IsRealisticLooking.Value) Mods.RealLooking();
            if (AdminLaser.Value) Mods.AdminLaser();
            Mods.CreatePlayerOutline();
            if (menuObj != null)
            {
                Vector3 parentScale = GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform.lossyScale;

                Vector3 desired = new Vector3(1.24208f, 13.04792f, 15.86129f);

                menuObj.transform.localScale = new Vector3(
                    desired.x / parentScale.x,
                    desired.y / parentScale.y,
                    desired.z / parentScale.z
                );
            }
        }

        public void CreateMenu()
        {
            var player = GorillaLocomotion.GTPlayer.Instance;
            isMenuCreated = true;

            if (menuPrefab != null)
            {
                menuObj = Instantiate(menuPrefab);
            }
            else
            {
                menuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                menuObj.transform.localScale = new Vector3(0.03f, 0.21f, 0.45f);
            }

            menuObj.transform.parent = player.LeftHand.controllerTransform;
            menuObj.transform.localPosition = menuForwardOffset;
            menuObj.transform.localRotation = Quaternion.identity;

            Transform backBtn = menuObj.transform.Find("Back");
            Transform forwardBtn = menuObj.transform.Find("Forwards");

            GameObject backObj = backBtn != null ? backBtn.gameObject : null;
            GameObject forwardObj = forwardBtn != null ? forwardBtn.gameObject : null;

            Transform Disconnect = menuObj.transform.Find("Disconnect");

            GameObject DisconnectObj = Disconnect != null ? Disconnect.gameObject : null;
            Transform Home = menuObj.transform.Find("Home");
            GameObject HomeObj = Home != null ? Home.gameObject : null;

            void SetupNavButton(GameObject obj)
            {
                if (obj == null) return;

                obj.layer = 2;

                var rb = obj.GetComponent<Rigidbody>() ?? obj.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;

                var col = obj.GetComponent<Collider>();
                if (col != null) col.isTrigger = true;

                var rend = obj.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.shader = Shader.Find("GorillaTag/UberShader");
                    rend.material.color = Theme.Value;

                    if (IsMenuRGB.Value)
                    {
                        StartCoroutine(RGBTheme(rend));
                    }
                }

                var bc = obj.GetComponent<ButtonCollider>() ?? obj.AddComponent<ButtonCollider>();

                bc.OnPressed = null;
            }

            SetupNavButton(backObj);
            SetupNavButton(forwardObj);
            SetupNavButton(DisconnectObj);
            SetupNavButton(HomeObj);

            if (backObj != null)
            {
                backObj.GetComponent<ButtonCollider>().OnPressed = () =>
                {
                    currentPageIndex = Mathf.Max(0, currentPageIndex - 1);
                    RefreshMenu();
                };
            }

            if (forwardObj != null)
            {
                forwardObj.GetComponent<ButtonCollider>().OnPressed = () =>
                {
                    currentPageIndex = Mathf.Min(Pages - 1, currentPageIndex + 1);
                    RefreshMenu();
                };
            }
            if (DisconnectObj != null)
            {
                DisconnectObj.GetComponent<ButtonCollider>().OnPressed = () =>
                {
                    PhotonNetwork.Disconnect();
                };
            }
            if (HomeObj != null)
            {
                HomeObj.GetComponent<ButtonCollider>().OnPressed = () =>
                {
                    SwitchPage(-1, 0);
                };
            }

            Transform titleTransform = menuObj.transform.Find("Title");

            if (titleTransform != null)
            {
                var text = titleTransform.GetComponent<TextMeshPro>();
                if (text != null)
                {
                    text.text = currentCategoryIndex == -1 ? "Cereal" + " [" + (currentPageIndex + 1) + "]" : GetCategoryName(currentCategoryIndex) + " [" + (currentPageIndex + 1) + "]";
                    text.color = Theme.Value == Color.white ? Color.black : Color.white;

                    if (text.fontSharedMaterial != null)
                    {
                        text.fontSharedMaterial.shader = Shader.Find("TextMeshPro/Distance Field");
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Prefab not found");
            }
            Transform BackText = menuObj.transform.Find(":3");
            if (BackText != null)
            {
                var text = BackText.GetComponent<TextMeshPro>();
                if (text.fontSharedMaterial != null)
                {
                    text.fontSharedMaterial.shader = Shader.Find("TextMeshPro/Distance Field");
                }
            }

            if (menuObj.GetComponent<Rigidbody>()) Destroy(menuObj.GetComponent<Rigidbody>());
            if (menuObj.GetComponent<Collider>()) Destroy(menuObj.GetComponent<Collider>());

            var rend = menuObj.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.shader = Shader.Find("GorillaTag/UberShader");
                rend.material.color = Theme.Value;
                if (IsMenuRGB.Value) rgbCoroutine = StartCoroutine(RGBTheme(rend));
            }

            HandMenuCollider = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            HandMenuCollider.transform.parent = player.RightHand.controllerTransform;
            HandMenuCollider.transform.localPosition = Vector3.down * 0.094f;
            HandMenuCollider.layer = 2;
            HandMenuCollider.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);
            Destroy(HandMenuCollider.GetComponent<Rigidbody>());

            if (ShowHandCollider.Value)
            {
                var rendhand = HandMenuCollider.GetComponent<Renderer>();
                rendhand.material.shader = Shader.Find("GorillaTag/UberShader");
                rendhand.material.color = Color.white;
            }

            float zOffset = 0.06f;
            float step = 0.05f;

            if (currentCategoryIndex == -1)
            {
                Pages = 2;
                if (currentPageIndex == 0)
                {
                    AddButton(zOffset, 0f, 0.2f, "Movement", () => SwitchPage(0, 0)); zOffset -= step;
                    AddButton(zOffset, 0f, 0.2f, "Utility", () => SwitchPage(1, 0)); zOffset -= step;
                    AddButton(zOffset, 0f, 0.2f, "Rig Mods", () => SwitchPage(2, 0)); zOffset -= step;
                    AddButton(zOffset, 0f, 0.2f, "Settings", () => SwitchPage(3, 0)); zOffset -= step;
                }
                else
                {
                    AddButton(zOffset, 0f, 0.2f, "Important", () => SwitchPage(4, 0)); zOffset -= step;
                    AddButton(zOffset, 0f, 0.2f, "Fun", () => SwitchPage(5, 0)); zOffset -= step;
                    if (IsAdmin)
                    {
                        AddButton(zOffset, 0f, 0.2f, "Admin", () => SwitchPage(6, 0)); zOffset -= step;
                    }
                }
            }
            else
            {
                // AddButton(zOffset, 0f, 0.2f, "⌂", () => SwitchPage(-1, 0));
                // zOffset -= step;

                switch (currentCategoryIndex)
                {
                    case 0:
                        Pages = 3;
                        if (currentPageIndex == 0)
                        {
                            AddToggleButton(ref zOffset, step, "Speed Boost", SpeedBoostEnabled);
                            AddToggleButton(ref zOffset, step, "Fly", FlyEnabled);
                            AddToggleButton(ref zOffset, step, "Platforms", IsPlatformsEnabled);
                            AddToggleButton(ref zOffset, step, "Joystick Fly", IsJoystickFly);
                        }
                        else if (currentPageIndex == 1)
                        {
                            AddToggleButton(ref zOffset, step, "Long Arms", LongArmsEnabled, () => Mods.UnLongArms());
                            AddToggleButton(ref zOffset, step, "Ground Helper", IsGroundHelper);
                            AddToggleButton(ref zOffset, step, "Amplified Monke", IsAmplifiedMonke);
                            AddToggleButton(ref zOffset, step, "Noclip", IsNoclipEnabled);
                        }
                        else
                        {
                            AddToggleButton(ref zOffset, step, "Web Slingers", IsWebSlingers);
                            AddToggleButton(ref zOffset, step, "Teleport Gun", IsTPGun);
                        }
                            break;

                    case 1:
                        Pages = 2;
                        if (currentPageIndex == 0)
                        {
                            AddToggleButton(ref zOffset, step, "Get PID Gun", IsGetPIDGun);
                            AddToggleButton(ref zOffset, step, "Mute Gun", IsMuteGun);
                            AddToggleButton(ref zOffset, step, "Mute Others", IsMuteEveryoneExceptGun);
                            AddToggleButton(ref zOffset, step, "Report Gun", IsReportGun);
                        }
                        else
                        {
                            AddButton(zOffset, 0f, 0.2f, "Mute All", () => Mods.MuteAll()); zOffset -= step;
                            AddButton(zOffset, 0f, 0.2f, "Unmute All", () => Mods.UnmuteAll()); zOffset -= step;
                        }
                        break;

                    case 2:
                        Pages = 3;
                        if (currentPageIndex == 0)
                        {
                            AddToggleButton(ref zOffset, step, "Ghost Monke", IsGhostMonke);
                            AddToggleButton(ref zOffset, step, "Lock Rig", IsLockOntoRig);
                            AddToggleButton(ref zOffset, step, "Hold Rig", IsHoldRig);
                            AddToggleButton(ref zOffset, step, "Rig Gun", IsRigGun);
                        }
                        else if (currentPageIndex == 1)
                        {
                            AddToggleButton(ref zOffset, step, "Freeze Rig", IsFreezeRig);
                            AddToggleButton(ref zOffset, step, "Upside Down Head", IsUpsideDownHead, () => Mods.FixRig());
                            AddToggleButton(ref zOffset, step, "Backwards Head", IsBackwardsHead, () => Mods.FixRig());
                            AddToggleButton(ref zOffset, step, "funny rig", IsFunnyRig, () => Mods.FixRig());
                        }
                        else
                        {
                            AddToggleButton(ref zOffset, step, "Recroom Torso", IsRecroomTorso, () => Mods.FixRig());
                            AddToggleButton(ref zOffset, step, "Recroom Rig", IsRecroomRig, () => Mods.FixRig());
                            AddToggleButton(ref zOffset, step, "Realistic Looking", IsRealisticLooking, () => Mods.FixRig());
                        }
                        break;

                    case 3:
                        Pages = 3;
                        if (currentPageIndex == 0)
                        {
                            AddToggleButton(ref zOffset, step, "Invis Plats", IsInvisPlat);
                            AddToggleButton(ref zOffset, step, "Menu RGB", IsMenuRGB);
                            AddButton(zOffset, 0f, 0.2f, "Theme Gray", () => { Theme.Value = Color.gray; RefreshMenu(); }); zOffset -= step;
                            AddButton(zOffset, 0f, 0.2f, "Theme Black", () => { Theme.Value = Color.black; RefreshMenu(); }); zOffset -= step;
                        }
                        else if (currentPageIndex == 1)
                        {
                            AddButton(zOffset, 0f, 0.2f, "Theme Blue", () => { Theme.Value = Color.lightSkyBlue; RefreshMenu(); }); zOffset -= step;
                            AddButton(zOffset, 0f, 0.2f, "Theme Red", () => { Theme.Value = Color.red; RefreshMenu(); }); zOffset -= step;
                            AddToggleButton(ref zOffset, step, "Show Hand Collider", ShowHandCollider);
                            AddButton(zOffset, 0f, 0.2f, "Fly Speed +", () => { FlySpeedSave.Value += 0.1f; NotiLib.SendNotification(FlySpeedSave.Value.ToString("0.0"), 2000); });
                        }
                        else
                        {
                            AddButton(zOffset, 0f, 0.2f, "Fly Speed -", () => { FlySpeedSave.Value -= 0.1f; NotiLib.SendNotification(FlySpeedSave.Value.ToString("0.0"), 2000); });
                        }
                            break;
                    case 4:
                        Pages = 1;
                        AddButton(zOffset, 0f, 0.2f, "Reauthenticate", () => MothershipAuthenticator.Instance.BeginLoginFlow()); zOffset -= step;
                        AddToggleButton(ref zOffset, step, "Anti Report", IsAntiReportEnabled);
                        break;
                    case 5:
                        Pages = 1;
                        AddButton(zOffset, 0f, 0.2f, "Unlock all cosmetics (CS)", () => Cosmetx.PluginCosmetx.instance.ActivateCosmetx()); zOffset -= step;
                        break;
                    case 6:
                        Pages = 4;
                        if (currentPageIndex == 0)
                        {
                            AddToggleButton(ref zOffset, step, "Silent Kick Gun", IsSilKick);
                            AddToggleButton(ref zOffset, step, "Admin Laser", AdminLaser);
                            AddToggleButton(ref zOffset, step, "Travis Scott", IsTravis, () => Mods.NoTravis());
                            AddToggleButton(ref zOffset, step, "Tv", IsTv, () => Mods.NoTv());
                        }
                        else if (currentPageIndex == 1)
                        {
                            AddToggleButton(ref zOffset, step, "Phone", IsPhone, () => Mods.NoSamsung());
                            AddToggleButton(ref zOffset, step, "Twerking Carti", IsTwerkingCarti, () => Mods.NoCarti());
                            AddToggleButton(ref zOffset, step, "Grab All", IsAdminGrab);
                            AddToggleButton(ref zOffset, step, "Roblox Sword", IsCoolSword, () => Mods.NoSword());
                        }
                        else if (currentPageIndex == 2)
                        {
                            AddToggleButton(ref zOffset, step, "Kormakur sign", IsKormakur, () => Mods.NoSign());
                            AddButton(zOffset, 0f, 0.2f, "Vid Hell", () => Mods.Video = "https://github.com/ChipLikesCereal/testvid/raw/refs/heads/main/GirlHell1999.mp4"); zOffset -= step;
                            AddButton(zOffset, 0f, 0.2f, "Vid OCD", () => Mods.Video = "https://github.com/ChipLikesCereal/testvid/raw/refs/heads/main/OCD.mp4"); zOffset -= step;
                            AddButton(zOffset, 0f, 0.2f, "Vid Kitty", () => Mods.Video = "https://github.com/ChipLikesCereal/testvid/raw/refs/heads/main/OCD.mp4"); zOffset -= step;
                        }
                        else
                        {
                            AddButton(zOffset, 0f, 0.2f, "Vid AMV", () => Mods.Video = "https://github.com/ChipLikesCereal/testvid/raw/refs/heads/main/testvid.mp4"); zOffset -= step;
                        }
                            break;
                }

                float navY = 0.08f;
            }

            foreach (GameObject btnObj in btnObjs)
            {
                if (btnObj != null)
                {
                    btnObj.layer = 2;
                    btnObj.GetComponent<Collider>().isTrigger = true;
                }
            }
        }

        private string GetCategoryName(int index) => index switch { 0 => "Movement", 1 => "Utility", 2 => "Rig Mods", 3 => "Settings", 4 => "Important", 5 => "Fun", 6 => "Admin", _ => "Cereal" };
        private void SwitchPage(int cat, int page) { currentCategoryIndex = cat; currentPageIndex = page; RefreshMenu(); }
        public void RefreshMenu() { DestroyMenu(true); CreateMenu(); currentPageIndex = Mathf.Clamp(currentPageIndex, 0, Mathf.Max(0, Pages - 1));}

        public void DestroyMenu(bool refresh)
        {
            isMenuCreated = false;
            if (menuObj != null)
            {
                if (!refresh)
                {
                    menuObj.transform.SetParent(null);
                    Rigidbody rb = menuObj.AddComponent<Rigidbody>();
                    rb.isKinematic = false; rb.useGravity = true;
                    rb.AddForce(Vector3.down * 0.5f, ForceMode.Impulse);
                }
                if (IsMenuRGB.Value && rgbCoroutine != null) StopCoroutine(rgbCoroutine);
                Destroy(menuObj, refresh ? 0 : 5f);
            }
            if (HandMenuCollider != null) Destroy(HandMenuCollider);
            foreach (var b in btnObjs) { if (!refresh && b != null) { b.GetComponent<FollowMenu>().target = null;b.AddComponent<Rigidbody>().AddForce(Vector3.down * 0.5f, ForceMode.Impulse); Destroy(b, 5f); } else Destroy(b); }
            btnObjs.Clear();
            Config.Save();
        }

        void AddToggleButton(ref float z, float step, string name, ConfigEntry<bool> entry, Action onDisable = null, Action onEnable = null)
        {
            AddButton(z, 0f, 0.2f, entry.Value ? $"[ON] {name}" : name, () => {
                entry.Value = !entry.Value;
                if (entry.Value) onEnable?.Invoke();
                else onDisable?.Invoke();
                RefreshMenu();
            });
            z -= step;
        }

        void AddButton(float z, float y, float s, string name, Action act)
        {
            GameObject btn = null;
            if (menuBundle != null)
            {
                GameObject buttonPrefab = menuBundle.LoadAsset<GameObject>("Assets/Button.prefab");

                if (buttonPrefab != null)
                {
                    btn = Instantiate(buttonPrefab);
                }
                else
                {
                    Logger.LogError("prefab not found");
                }
            }

            if (btn == null)
            {
                btn = GameObject.CreatePrimitive(PrimitiveType.Cube);
                btn.transform.localScale = new Vector3(0.03f, s, 0.04f);
            }

            var f = btn.GetComponent<FollowMenu>() ?? btn.AddComponent<FollowMenu>();
            f.target = GorillaLocomotion.GTPlayer.Instance.LeftHand.controllerTransform;
            f.position = new Vector3(0.015f, y, z) + menuForwardOffset;
            f.rotationOffset = Quaternion.Euler(90f, 0f, 0f);

            var renderer = btn.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.shader = Shader.Find("GorillaTag/UberShader");
                if (!IsMenuRGB.Value)
                    btn.GetComponent<Renderer>().material.color = Theme.Value;
                else
                {
                    StartCoroutine(RGBTheme(btn.GetComponent<Renderer>()));
                }
            }

            var collider = btn.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            btn.layer = 2;

            var c = btn.GetComponent<ButtonCollider>() ?? btn.AddComponent<ButtonCollider>();
            c.OnPressed = act;

            TextMeshPro t = null;
            Transform textTransform = btn.transform.Find("Text");

            if (textTransform != null)
            {
                t = textTransform.GetComponent<TextMeshPro>();
            }

            if (t == null)
            {
                var tObj = new GameObject("Text");
                tObj.transform.SetParent(btn.transform);
                tObj.transform.localPosition = new Vector3(0.55f, 0f, 0f);
                tObj.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);

                t = tObj.AddComponent<TextMeshPro>();
            }
            t.text = name;
            t.color = Color.white;


            btnObjs.Add(btn);
        }

        public void PlayClickSound() { if (soundReady && cachedClip != null) audioSource.PlayOneShot(cachedClip); }
       
        async Task Getver() { try { menuversion = (await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/ChipLikesCereal/Cereal/refs/heads/main/Version")).Trim(); } catch { } }
        public IEnumerator RGBTheme(Renderer r) { while (true) { float t = Time.time * 2f; r.material.color = new Color(Mathf.Sin(t) * 0.5f + 0.5f, Mathf.Sin(t + 2f) * 0.5f + 0.5f, Mathf.Sin(t + 4f) * 0.5f + 0.5f); yield return null; } }

        public class FollowMenu : MonoBehaviour
        {
            public Transform target;
            public Vector3 position;
            public Quaternion rotationOffset = Quaternion.identity;

            void LateUpdate()
            {
                if (target)
                {
                    transform.position = target.TransformPoint(position);
                    transform.rotation = target.rotation * rotationOffset;
                }
            }
        }
        public class ButtonCollider : MonoBehaviour { public Action OnPressed; public void Press() { if (Plugin.instance.globalClickCooldown > 0) return; Plugin.instance.globalClickCooldown = 0.4f; Plugin.instance.PlayClickSound(); OnPressed?.Invoke(); } private void OnTriggerEnter(Collider other) { if (other.gameObject == Plugin.instance.HandMenuCollider) Press(); } }
    }
}