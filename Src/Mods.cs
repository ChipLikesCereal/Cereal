using BepInEx;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using static Bindings;
using static GTPosRotConstraintManager;
using Object = UnityEngine.Object;

namespace CerealMenu
{

    public class Mods : MonoBehaviour
    {

        public static bool HasGhostMonked = false;
        private static bool prevRightPrimary = false;

        public static Mods instance;

        private Coroutine rgbCoroutine;
        public IEnumerator RGBTheme(Renderer targetRenderer)
        {
            float speed = 2f;

            while (true)
            {
                float t = Time.time * speed;

                float r = Mathf.Sin(t) * 0.5f + 0.5f;
                float g = Mathf.Sin(t + 2f) * 0.5f + 0.5f;
                float b = Mathf.Sin(t + 4f) * 0.5f + 0.5f;

                Color rgb = new Color(r, g, b);

                if (targetRenderer != null)
                    targetRenderer.material.color = rgb;

                yield return null;
            }
        }
        void Awake()
        {
            instance = this;
        }

        public static bool HasCreated = false;

        public static bool IsLeftPlat = false;
        public static bool IsRightPlat = false;
        public static GameObject LeftPlat;
        public static GameObject RightPlat;
        // i lowk got called a skidder for having these plat gameobjects, CAN A PERSON NOT HAVE PLAT GAMEOBJECTS??

        public static GameObject LeftS;
        public static GameObject RightS;
        public static GameObject HeadS;

        public static void SpeedBoost()
        {
            GTPlayer.Instance.maxJumpSpeed = 8f;
            GTPlayer.Instance.jumpMultiplier = 5.3f;
        }
        public static void CreatePlayerOutline()
        {
            if (VRRig.LocalRig.enabled == false)
            {
                if (!HasCreated)
                {
                    var player = GTPlayer.Instance;
                    // left hand
                    LeftS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    LeftS.transform.parent = player.LeftHand.controllerTransform;
                    LeftS.transform.localPosition = Vector3.zero;
                    LeftS.transform.localRotation = Quaternion.identity;
                    LeftS.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    var rendL = LeftS.GetComponent<Renderer>();
                    rendL.material.shader = Shader.Find("GorillaTag/UberShader");
                    rendL.material.color = Plugin.instance.Theme.Value;

                    Object.Destroy(LeftS.GetComponent<Rigidbody>());
                    Object.Destroy(LeftS.GetComponent<Collider>());

                    // right hand
                    RightS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    RightS.transform.parent = player.RightHand.controllerTransform;
                    RightS.transform.localPosition = Vector3.zero;
                    RightS.transform.localRotation = Quaternion.identity;
                    RightS.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    var rendR = RightS.GetComponent<Renderer>();
                    rendR.material.shader = Shader.Find("GorillaTag/UberShader");
                    rendR.material.color = Plugin.instance.Theme.Value;

                    Object.Destroy(RightS.GetComponent<Rigidbody>());
                    Object.Destroy(RightS.GetComponent<Collider>());

                    // head
                    HeadS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    HeadS.transform.parent = player.headCollider.transform;
                    HeadS.transform.localPosition = Vector3.zero;
                    HeadS.transform.localRotation = Quaternion.identity;
                    HeadS.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                    var rendH = HeadS.GetComponent<Renderer>();
                    rendH.material.shader = Shader.Find("GorillaTag/UberShader");
                    rendH.material.color = Plugin.instance.Theme.Value;

                    Object.Destroy(HeadS.GetComponent<Rigidbody>());
                    Object.Destroy(HeadS.GetComponent<Collider>());
                    HasCreated = true;
                }
            }
            else
            {
                if (LeftS != null) Object.Destroy(LeftS);
                if (RightS != null) Object.Destroy(RightS);
                if (HeadS != null) Object.Destroy(HeadS);
                HasCreated = false;
            }
        }
        public static void Fly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GTPlayer.Instance.transform.position +=
                    GTPlayer.Instance.headCollider.transform.forward *
                    Plugin.instance.FlySpeedSave.Value;

                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static void LongArms()
        {
            GTPlayer.Instance.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
        }

        public static void UnLongArms()
        {
            GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        public static void ActivateGrayAll()
        {
            if (PhotonNetwork.InRoom)
                GreyZoneManager.Instance.ActivateGreyZoneAuthority();
        }

        public static void DeactivateGrayAll()
        {
            if (PhotonNetwork.InRoom)
                GreyZoneManager.Instance.DeactivateGreyZoneAuthority();
        }

        public static void GhostMonke()
        {
            bool current = ControllerInputPoller.instance.rightControllerPrimaryButton;

            if (current && !prevRightPrimary)
            {
                HasGhostMonked = !HasGhostMonked;

                if (HasGhostMonked)
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = false;

                }
                else
                {
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }

            prevRightPrimary = current;
        }

        public static void Platforms()
        {
            var platcolor = Plugin.instance.Theme.Value;

            if (ControllerInputPoller.instance.leftGrab && !IsLeftPlat)
            {
                var player = GTPlayer.Instance;

                IsLeftPlat = true;
                LeftPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftPlat.transform.position = player.LeftHand.controllerTransform.position;
                LeftPlat.transform.rotation = player.LeftHand.controllerTransform.rotation;
                LeftPlat.transform.localScale = new Vector3(0.03f, 0.3f, 0.45f);

                Object.Destroy(LeftPlat.GetComponent<Rigidbody>());

                if (!Plugin.instance.IsInvisPlat.Value)
                {
                    var rend = LeftPlat.GetComponent<Renderer>();
                    rend.material.shader = Shader.Find("GorillaTag/UberShader");
                    rend.material.color = platcolor;
                    if (Plugin.instance.IsMenuRGB.Value)
                    {
                        if (Mods.instance.rgbCoroutine != null)
                            Mods.instance.StopCoroutine(Mods.instance.rgbCoroutine);

                        Mods.instance.rgbCoroutine = Mods.instance.StartCoroutine(Mods.instance.RGBTheme(rend));
                    }
                }
            }

            if (ControllerInputPoller.instance.rightGrab && !IsRightPlat)
            {
                var player = GTPlayer.Instance;

                IsRightPlat = true;
                RightPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                RightPlat.transform.position = player.RightHand.controllerTransform.position;
                RightPlat.transform.rotation = player.RightHand.controllerTransform.rotation;
                RightPlat.transform.localScale = new Vector3(0.03f, 0.3f, 0.45f);

                Object.Destroy(RightPlat.GetComponent<Rigidbody>());

                if (!Plugin.instance.IsInvisPlat.Value)
                {
                    var rend = RightPlat.GetComponent<Renderer>();
                    rend.material.shader = Shader.Find("GorillaTag/UberShader");
                    rend.material.color = platcolor;
                    if (Plugin.instance.IsMenuRGB.Value)
                    {
                        if (Mods.instance.rgbCoroutine != null)
                            Mods.instance.StopCoroutine(Mods.instance.rgbCoroutine);

                        Mods.instance.rgbCoroutine = Mods.instance.StartCoroutine(Mods.instance.RGBTheme(rend));
                    }
                }
            }

            if (!ControllerInputPoller.instance.leftGrab && IsLeftPlat)
            {
                Object.Destroy(LeftPlat);
                IsLeftPlat = false;
            }

            if (!ControllerInputPoller.instance.rightGrab && IsRightPlat)
            {
                Object.Destroy(RightPlat);
                IsRightPlat = false;
            }
        }

        public static void JoystickFly()
        {
            GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
            GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity, ForceMode.Acceleration);

            Vector2 joyl = ControllerInputPoller.instance.leftControllerPrimary2DAxis;
            Vector2 joyr = ControllerInputPoller.instance.rightControllerPrimary2DAxis;

            if (joyl.magnitude > 0.1f)
            {
                GTPlayer.Instance.transform.position +=
                    GorillaTagger.Instance.bodyCollider.transform.forward *
                    (Time.deltaTime * joyl.y * Plugin.instance.FlySpeedSave.Value * 15) +
                    GorillaTagger.Instance.bodyCollider.transform.right *
                    (Time.deltaTime * joyl.x * Plugin.instance.FlySpeedSave.Value * 15);
            }

            if (joyr.magnitude > 0.1f)
            {
                GTPlayer.Instance.transform.position +=
                    GorillaTagger.Instance.bodyCollider.transform.up *
                    (Time.deltaTime * joyr.y * Plugin.instance.FlySpeedSave.Value * 15);
            }
        }
        public static bool noclipBool = false;
        public static void Noclip()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                if (noclipBool == false)
                {
                    noclipBool = true;
                    foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    {
                        meshCollider.enabled = false;
                    }
                }
            }
            else
            {
                if (noclipBool)
                {
                    noclipBool = false;
                    foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    {
                        meshCollider.enabled = true;
                    }
                }
            }
        }

        public static void AntiReport()
        {
            if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom) return;

            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Vector3 reportBtnPos = line.reportButton.transform.position;

                foreach (VRRig vrrig in VRRigCache.ActiveRigs)
                {
                    if (vrrig == null || vrrig.isLocal || vrrig.isOfflineVRRig) continue;
                    float distRight = Vector3.Distance(vrrig.rightHandTransform.position, reportBtnPos);
                    float distLeft = Vector3.Distance(vrrig.leftHandTransform.position, reportBtnPos);

                    if (distRight < 0.7f || distLeft < 0.7f)
                    {
                        PhotonNetwork.Disconnect();
                        return;
                    }
                }
            }
        }
        public static void LockOntoRig()
        {
            GunLib.LetGun();
            if (ControllerInputPoller.instance.rightControllerTriggerButton && GunLib.IsOverVrrig && GunLib.GunPos != null && ControllerInputPoller.instance != null)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GunLib.VrrigTransform.position;
  
                
                
            }
            if (!ControllerInputPoller.instance.rightControllerTriggerButton)
            {
                VRRig.LocalRig.enabled = true;
            }
            if (!ControllerInputPoller.instance.rightGrab)
            {
                VRRig.LocalRig.enabled = true;
            }
        }
        public static void RigGun()
        {
            GunLib.LetGun();
            if (ControllerInputPoller.instance != null && GunLib.GunPos != null && ControllerInputPoller.instance.rightControllerTriggerButton)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GunLib.GunPos.position + new Vector3(0, 0.7f, 0);
            }
            if (!ControllerInputPoller.instance.rightControllerTriggerButton || !ControllerInputPoller.instance.rightGrab)
            {
                VRRig.LocalRig.enabled = true;
            }
        }
        public static void FreezeRig()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GTPlayer.Instance.bodyCollider.transform.position + new Vector3(0, 0.2f, 0);
            }
            if (!ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                VRRig.LocalRig.enabled = true;
            }
        }
        public static float muteDelay;

        public static void MuteGun()
        {

            GunLib.LetGun();



            if (!GunLib.IsOverVrrig)
                return;


            if (ControllerInputPoller.instance.rightControllerTriggerButton && Time.time > muteDelay)
            {
                var owner = GunLib.LockedRigOwner;

                if (owner != null && !owner.IsLocal)
                {
                    foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines
                             .Where(l => l.linePlayer == owner))
                    {
                        muteDelay = Time.time + 0.5f;

                        line.muteButton.isOn = !line.muteButton.isOn;
                        line.PressButton(line.muteButton.isOn, GorillaPlayerLineButton.ButtonType.Mute);
                    }
                }
            }
        }
        public static void MuteEveryoneExceptGun()
        {

            GunLib.LetGun();


            if (!GunLib.IsOverVrrig)
                return;


            if (ControllerInputPoller.instance.rightControllerTriggerButton && Time.time > muteDelay)
            {
                var target = GunLib.LockedRigOwner;


                if (target == null)
                    return;

                muteDelay = Time.time + 0.5f;

                foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (line.linePlayer == null)
                        continue;

                    if (line.linePlayer.IsLocal)
                        continue;


                    if (line.linePlayer == target)
                    {
                        if (line.muteButton.isOn) 
                        {
                            line.muteButton.isOn = false;
                            line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
                        }
                    }
                    else
                    {
                        if (!line.muteButton.isOn) 
                        {
                            line.muteButton.isOn = true;
                            line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
                        }
                    }
                }
            }
        }

        public static void ReportGun()
        {
            GunLib.LetGun();



            if (!GunLib.IsOverVrrig)
                return;

            if (ControllerInputPoller.instance.rightControllerTriggerButton && Time.time > muteDelay)
            {
                var owner = GunLib.LockedRigOwner;

                if (owner != null && !owner.IsLocal)
                {
                    GorillaPlayerScoreboardLine.ReportPlayer(
                        owner.UserId,
                        GorillaPlayerLineButton.ButtonType.Toxicity,
                        owner.NickName
                    );

                    muteDelay = Time.time + 0.2f;
                }
            }
        }
        public static bool HasShot = false;
        public static void TPGun()
        {
            GunLib.LetGun();
            if (ControllerInputPoller.instance.rightControllerTriggerButton && !HasShot)
            {
                GorillaLocomotion.GTPlayer.Instance.transform.position = GunLib.GunPos.position;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                HasShot = true;
            }
            if (!ControllerInputPoller.instance.rightControllerTriggerButton && HasShot)
            {
                HasShot = false;
            }
        }
        public static void HoldRig()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position;
            }
            if (!ControllerInputPoller.instance.rightGrab)
            {
                VRRig.LocalRig.enabled = true;
            }
            
        }
        public static void MuteAll()
        {
            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer == null)
                    continue;

                if (line.linePlayer.IsLocal)
                    continue;

                    if (!line.muteButton.isOn) 
                    {
                        line.muteButton.isOn = true;
                        line.PressButton(true, GorillaPlayerLineButton.ButtonType.Mute);
                    }
            }
        }
        public static void UnmuteAll()
        {
            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer == null)
                    continue;

                if (line.linePlayer.IsLocal)
                    continue;

                if (line.muteButton.isOn)
                {
                    line.muteButton.isOn = false;
                    line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
                }
            }
        }
        public static bool HeldTriggerGetPID = false;
        public static void GetPID()
        {
            GunLib.LetGun();

            if (ControllerInputPoller.instance.rightControllerTriggerButton && GunLib.IsOverVrrig && !HeldTriggerGetPID)
            {
                string userId = GunLib.LockedRigOwner.UserId;
                string nick = GunLib.LockedRigOwner.NickName;

                string dirPath = Path.Combine(BepInEx.Paths.GameRootPath, "Cereal", "IDS");
                Directory.CreateDirectory(dirPath);

                string filePath = Path.Combine(dirPath, nick + ".txt");

                File.WriteAllText(filePath, "ID: " + userId);

                NotiLib.SendNotification("ID: " + userId, 2000);

                HeldTriggerGetPID = true;
            }

            if (!ControllerInputPoller.instance.rightControllerTriggerButton && HeldTriggerGetPID)
            {
                HeldTriggerGetPID = false;
            }
        }
        public static void UpsideDownNeck()
        {
            VRRig.LocalRig.head.trackingRotationOffset.z = 180f;
        }
        public static void silkickgun()
        {
            GunLib.LetGun();
            
            if (ControllerInputPoller.instance.rightControllerTriggerButton && GunLib.IsOverVrrig)
            {
                Console.ExecuteCommand("silkick", ReceiverGroup.All,
                    GunLib.LockedRig.Creator.UserId);
            }
        }
        public static int assetId;
        public static bool hastwerked = false;
        public static void TwerkingCarti()
        {
            if (!hastwerked)
            {
                assetId = Console.GetFreeAssetID();
                Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "carti",
                assetId);

                Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, assetId,
                new Vector3(-76f, 1.7f, -80f));

                Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, assetId, Quaternion.Euler(0f, 40f, 0f));

                if (!Plugin.instance.IsBigAssets.Value)
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 5f);
                else
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 10f);
                hastwerked = true;
            }
        }
        public static void NoCarti()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
            hastwerked = false;
        }
        private static int allocatedSwordId = -1;
        private static bool HasSpawnedSword = false;
        private static bool HasPlayed = false;

        public static void Sword()
        {
            if (!HasSpawnedSword)
            {
                if (allocatedSwordId < 0)
                {
                    allocatedSwordId = Console.GetFreeAssetID();
                    Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "console.main1", "Sword",
                            allocatedSwordId);
                    if (Plugin.instance.IsBigAssets.Value)
                        Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedSwordId, Vector3.one * 5);

                    Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedSwordId, 2);
                    Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedSwordId, "Model",
                            "Unsheath");

                }

                HasSpawnedSword = true;

            }
            if (ControllerInputPoller.instance.rightControllerTriggerButton)
            {
                if (!HasPlayed)
                {
                    Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedSwordId, "Model",
            "Slash");
                    HasPlayed = true;
                }
            }
            if (!ControllerInputPoller.instance.rightControllerTriggerButton && HasPlayed)
            {
                HasPlayed = false;
            }

        }
        public static void NoSword()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedSwordId);
            allocatedSwordId = -1;
            HasSpawnedSword = false;
        }
        private static int allocatedTravisId;
        public static bool HasTravisTravised = false; // great bool btw
        public static void TravisScott() // traviski skoot 
        {
            if (!HasTravisTravised)
            {
                Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "travis", "TravisScott", allocatedTravisId);
                Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, allocatedTravisId,
        new Vector3(-65f, 2f, -55f));
                if (!Plugin.instance.IsBigAssets.Value)
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedTravisId, Vector3.one * 0.4f);
                else
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedTravisId, Vector3.one * 3.5f);

                Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, allocatedTravisId, Quaternion.Euler(0f, 20f, 0f));
                HasTravisTravised = true;
            }
        }
        public static void NoTravis()
        {
            HasTravisTravised = false;
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedTravisId);
        }
        public static int phoneid;
        public static bool HasCreatedPhone = false;
        public static string Video = "";
        public static void Samsung()
        {
            if (!HasCreatedPhone)
            {
                phoneid = Console.GetFreeAssetID();

                Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "samsungphone", phoneid);

                Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, phoneid, 1);

                Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, phoneid,
                        new Vector3(-0.075f, 0.1f, 0f));

                Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, phoneid,
                        Quaternion.Euler(80f, 90f, 180f));
                if (!Plugin.instance.IsBigAssets.Value)
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, phoneid, Vector3.one * 0.3f);
                else
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, phoneid, Vector3.one * 5f);

                Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, phoneid, "VideoPlayer", Video);

                Console.ExecuteCommand("asset-destroycolliders", ReceiverGroup.All, phoneid);
                HasCreatedPhone = true;
            }
        }
        public static void NoSamsung()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, phoneid);
            HasCreatedPhone = false;
        }
        // hamburbur yum
        public static void AdminGrabAll()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                Console.ExecuteCommand("tp", ReceiverGroup.Others, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position);
            }
        }
        private static int KormakurId;
        private static bool HasSignSigned = false; // has travis travised
        public static void KormakurFemboys() // okay to justify this void THE SIGN IMPLIES THAT KORMAKUR IS A FEMBOY and im not weird (trust)
        {
            if (!HasSignSigned)
            {
                KormakurId = Console.GetFreeAssetID();
                Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "KormakurSign",
                        KormakurId);

                Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, KormakurId, 2);

                Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, KormakurId,
                        new Vector3(0.29f, -0.2f, -0.1272f));

                Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, KormakurId,
                        Quaternion.Euler(355f, 275f, 265f));
                if (!Plugin.instance.IsBigAssets.Value)
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, KormakurId, Vector3.one);
                else
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, KormakurId, Vector3.one * 5);
                HasSignSigned = true;
            }
        }
        public static void NoSign()
        {
            HasSignSigned = false;
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, KormakurId);
        }
        private static int Axeid;
        public static bool HasAxeAxed = false;
        public static void Axe()
        {
            if (!HasAxeAxed)
            {
                Axeid = Console.GetFreeAssetID();
                Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets", "Axe",
                    Axeid);

                Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, Axeid, 2);

                Console.ExecuteCommand("asset-setlocalposition", ReceiverGroup.All, Axeid,
                    new Vector3(0.05f, 0.03f, 0f));

                Console.ExecuteCommand("asset-setlocalrotation", ReceiverGroup.All, Axeid,
                    Quaternion.Euler(0f, 0f, 90f));
                if (!Plugin.instance.IsBigAssets.Value)
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, Axeid, Vector3.one * 5);
                else
                    Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, Axeid, Vector3.one * 10);
                HasAxeAxed = true;
            }
        }
        public static void NoAxe()
        {
            HasAxeAxed = false;
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, Axeid);
        }
        private static int TvID;
        private static int sofaAssetId;
        public static bool Hastvtved;
        public static void SkidTV()
        {
            if (!Hastvtved)
            {
                TvID = Console.GetFreeAssetID();
                sofaAssetId = Console.GetFreeAssetID();

                Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets",
                        "TV", TvID);

                Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "consolehamburburassets",
                        "sofa", sofaAssetId);

                Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, TvID,
                        new Vector3(-57.1f, 5.6f, -37f));

                Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, sofaAssetId,
                        new Vector3(-51.8f, 4.2f, -37.4f));

                Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, TvID,
                        Quaternion.Euler(270f, 0f, 0f));

                Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, sofaAssetId,
                        Quaternion.Euler(270f, 270f, 0f));

                Console.ExecuteCommand("asset-setvideo", ReceiverGroup.All, TvID, "VideoPlayer",
                        Video);
                Hastvtved = true;
            }
        }
        public static void NoTv()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, TvID);
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, sofaAssetId);
            Hastvtved = false;
        }
        public static void NoIndicator()
        {
            Console.ExecuteCommand("nocone", ReceiverGroup.All, true);
        }
        public static void ShowIndicator()
        {
            Console.ExecuteCommand("nocone", ReceiverGroup.All, false);
        }
        public static void BackwardsHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.y = 180f;
        }
        public static void GroundHelper()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                GorillaTagger.Instance.rigidbody.AddForce(new Vector3(0, -8f, 0), ForceMode.Acceleration);
                SpeedBoost();
                if (ControllerInputPoller.instance.rightControllerPrimaryButton)
                {
                    Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();

                    Vector3 vel = rb.linearVelocity;

                    vel.y = 0f;

                    if (vel.sqrMagnitude > 0.001f)
                    {
                        Vector3 moveDir = vel.normalized;

                        rb.MovePosition(rb.position + moveDir * 3.9f * Time.deltaTime * GTPlayer.Instance.scale);
                    }
                }
            }
        }
        public static void AmplifiedMonke()
        {
            Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
            Vector3 vel = rb.linearVelocity;
            if (vel.sqrMagnitude > 0.001f)
            {
                Vector3 moveDir = vel.normalized;
                rb.MovePosition(rb.position + moveDir * 4.2f * Time.deltaTime * GTPlayer.Instance.scale);
            }
        }
        public static readonly string[] ignoreLayers = { "Gorilla Trigger", "Gorilla Boundary", "GorillaHand", "GorillaObject", "Zone", "Water", "GorillaCosmetics", "GorillaParticle", };
        public static LineRenderer webLineLeft;
        public static LineRenderer webLineRight;

        private static bool leftActive;
        private static bool rightActive;

        private static bool leftLocked;
        private static bool rightLocked;

        private static Vector3 leftAnchor;
        private static Vector3 rightAnchor;

        private static float springForce = 40f;
        private static float dampening = 6f;

        private static float leftLength;
        private static float rightLength;

        private static Vector3 lastLeftPos;
        private static Vector3 lastRightPos;

        private static Vector3 leftHandVel;
        private static Vector3 rightHandVel;
        public static void WebSlingers()
        {
            Transform left = GTPlayer.Instance.LeftHand.controllerTransform;
            Transform right = GTPlayer.Instance.RightHand.controllerTransform;

            bool leftGrab = ControllerInputPoller.instance.leftGrab;
            bool rightGrab = ControllerInputPoller.instance.rightGrab;

            Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
            if (rb == null) return;

            int ignoreMask = ~0;

            foreach (string layerName in ignoreLayers)
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (layer != -1)
                    ignoreMask &= ~(1 << layer);
            }

            leftHandVel = (left.position - lastLeftPos) / Time.deltaTime;
            rightHandVel = (right.position - lastRightPos) / Time.deltaTime;

            lastLeftPos = left.position;
            lastRightPos = right.position;

            if (webLineLeft == null)
            {
                GameObject obj = new GameObject("WebLeftHand");
                webLineLeft = obj.AddComponent<LineRenderer>();
                webLineLeft.positionCount = 2;
                webLineLeft.startWidth = 0.02f;
                webLineLeft.endWidth = 0.02f;
                webLineLeft.material = new Material(Shader.Find("Sprites/Default"));
            }

            if (webLineRight == null)
            {
                GameObject obj = new GameObject("WebRightHand");
                webLineRight = obj.AddComponent<LineRenderer>();
                webLineRight.positionCount = 2;
                webLineRight.startWidth = 0.02f;
                webLineRight.endWidth = 0.02f;
                webLineRight.material = new Material(Shader.Find("Sprites/Default"));
            }

            if (leftGrab && !leftLocked)
            {
                if (Physics.Raycast(left.position, left.forward, out RaycastHit hitL, Mathf.Infinity, ignoreMask))
                {
                    leftLocked = true;
                    leftActive = true;
                    leftAnchor = hitL.point;
                    leftLength = Vector3.Distance(GTPlayer.Instance.transform.position, leftAnchor);
                }
            }

            if (!leftGrab)
            {
                leftLocked = false;
                leftActive = false;
                webLineLeft.enabled = false;
            }

            if (rightGrab && !rightLocked)
            {
                if (Physics.Raycast(right.position, right.forward, out RaycastHit hitR, Mathf.Infinity, ignoreMask))
                {
                    rightLocked = true;
                    rightActive = true;
                    rightAnchor = hitR.point;
                    rightLength = Vector3.Distance(GTPlayer.Instance.transform.position, rightAnchor);
                }
            }

            if (!rightGrab)
            {
                rightLocked = false;
                rightActive = false;
                webLineRight.enabled = false;
            }

            Vector3 playerPos = GTPlayer.Instance.transform.position;

            if (leftActive)
            {
                Vector3 toAnchor = leftAnchor - playerPos;
                float dist = toAnchor.magnitude;
                Vector3 dir = toAnchor.normalized;

                if (dist > leftLength)
                {
                    Vector3 projected = Vector3.Project(rb.velocity, dir);
                    if (Vector3.Dot(projected, dir) > 0)
                        rb.velocity -= projected;
                }

                Vector3 tangential = Vector3.Cross(dir, Vector3.Cross(rb.velocity, dir));
                rb.AddForce(tangential, ForceMode.Acceleration);

                float pull = Vector3.Dot(leftHandVel, -dir);
                if (pull > 0)
                    rb.AddForce(dir * pull * 50f, ForceMode.Acceleration);

                webLineLeft.enabled = true;
                webLineLeft.SetPosition(0, left.position);
                webLineLeft.SetPosition(1, leftAnchor);
            }

            if (rightActive)
            {
                Vector3 toAnchor = rightAnchor - playerPos;
                float dist = toAnchor.magnitude;
                Vector3 dir = toAnchor.normalized;

                if (dist > rightLength)
                {
                    Vector3 projected = Vector3.Project(rb.velocity, dir);
                    if (Vector3.Dot(projected, dir) > 0)
                        rb.velocity -= projected;
                }

                Vector3 tangential = Vector3.Cross(dir, Vector3.Cross(rb.velocity, dir));
                rb.AddForce(tangential, ForceMode.Acceleration);

                float pull = Vector3.Dot(rightHandVel, -dir);
                if (pull > 0)
                    rb.AddForce(dir * pull * 50f, ForceMode.Acceleration);

                webLineRight.enabled = true;
                webLineRight.SetPosition(0, right.position);
                webLineRight.SetPosition(1, rightAnchor);
            }
        }
        public static float startX = -1f;
        public static float startY = -1f;

        public static float subThingy;
        public static float subThingyZ;

        public static Vector3 lastPosition = Vector3.zero;

        public static void MessUpRig()
        {
            VRRig.LocalRig.head.trackingRotationOffset.y = 90;
            VRRig.LocalRig.head.trackingRotationOffset.x = 12;
            VRRig.LocalRig.leftHand.trackingPositionOffset.z = 0.2f;
            VRRig.LocalRig.rightHand.trackingPositionOffset.z = 0.2f;
            SetBodyPatch(true, 4);
        }
        public static void FixRig()
        {
            VRRig.LocalRig.head.trackingRotationOffset.x = 0;
            VRRig.LocalRig.head.trackingRotationOffset.y = 0;
            VRRig.LocalRig.head.trackingRotationOffset.z = 0;
            VRRig.LocalRig.leftHand.trackingPositionOffset.z = 0f;
            VRRig.LocalRig.rightHand.trackingPositionOffset.z = 0f;
            DisableRecRoomBody();
        }
        public static void TorsoPatch_VRRigLateUpdate() =>
    VRRig.LocalRig.transform.rotation *= Quaternion.Euler(0f, Time.time * 180f % 360f, 0f);
        public static void SetBodyPatch(bool enabled, int mode = 0)
        {
            Plugin.TorsoPatch.enabled = enabled;
            Plugin.TorsoPatch.mode = mode;

            if (!enabled && recBodyRotary != null)
                Object.Destroy(recBodyRotary);
        }
        public static GameObject recBodyRotary;
        public static void RecRoomTorso()
        {
            SetBodyPatch(true, 5);

            if (recBodyRotary == null)
                recBodyRotary = new GameObject("cereal_recBodyRotary");

            recBodyRotary.transform.rotation = Quaternion.Lerp(recBodyRotary.transform.rotation, Quaternion.Euler(0f, GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * 6.5f);
        }
        public static void RecRoomRig()
        {
            SetBodyPatch(true, 3);

            if (recBodyRotary == null)
                recBodyRotary = new GameObject("cereal_recBodyRotary");

            recBodyRotary.transform.rotation = Quaternion.Lerp(recBodyRotary.transform.rotation, Quaternion.Euler(0f, GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * 6.5f);
        }
        public static void RealLooking()
        {
            SetBodyPatch(true, 6);

            if (recBodyRotary == null)
                recBodyRotary = new GameObject("cereal_recBodyRotary");

            recBodyRotary.transform.rotation = Quaternion.Lerp(recBodyRotary.transform.rotation, Quaternion.Euler(0f, GorillaTagger.Instance.headCollider.transform.rotation.eulerAngles.y, 0f), Time.deltaTime * 6.5f);
        }
        public static void DisableRecRoomBody()
        {
            SetBodyPatch(false);
        }

        public static readonly int TransparentFX = LayerMask.NameToLayer("TransparentFX");
        public static readonly int IgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int Zone = LayerMask.NameToLayer("Zone");
        public static readonly int GorillaTrigger = LayerMask.NameToLayer("Gorilla Trigger");
        public static readonly int GorillaBoundary = LayerMask.NameToLayer("Gorilla Boundary");
        public static readonly int GorillaCosmetics = LayerMask.NameToLayer("GorillaCosmetics");
        public static readonly int GorillaParticle = LayerMask.NameToLayer("GorillaParticle");
        public static int NoInvisLayerMask() =>
        ~(1 << TransparentFX | 1 << IgnoreRaycast | 1 << Zone | 1 << GorillaTrigger | 1 << GorillaBoundary |
          1 << GorillaCosmetics | 1 << GorillaParticle);
        public static void AdminLaser()
        {
            Vector3 dir = ControllerInputPoller.instance.rightControllerPrimaryButton
                      ? VRRig.LocalRig.rightHandTransform.right
                      : -VRRig.LocalRig.leftHandTransform.right;

            Vector3 startPos =
                    (ControllerInputPoller.instance.rightControllerPrimaryButton
                             ? VRRig.LocalRig.rightHandTransform.position
                             : VRRig.LocalRig.leftHandTransform.position) + dir * 0.1f;
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                try
                {
                    Physics.Raycast(startPos + dir / 3f, dir, out RaycastHit Ray, 512f, NoInvisLayerMask());
                    VRRig gunTarget = Ray.collider.GetComponentInParent<VRRig>();
                    if (gunTarget && !gunTarget.isLocal)
                        Console.ExecuteCommand("silkick", ReceiverGroup.All,
                                gunTarget.Creator.UserId);
                }
                catch { }
                Console.ExecuteCommand("laser", ReceiverGroup.All, true, ControllerInputPoller.instance.rightControllerPrimaryButton);
            }
        }

    }
}