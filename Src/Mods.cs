using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Linq;
using UnityEngine;
using static Bindings;
using static UnityEngine.GraphicsBuffer;
using Object = UnityEngine.Object;

namespace CerealMenu
{
    public class Mods : MonoBehaviour
    {
        public static bool HasGhostMonked = false;
        private static bool prevRightPrimary = false;

        public static bool HasCreated = false;

        public static bool HasCreatedLOR = false;

        public static bool IsLeftPlat = false;
        public static bool IsRightPlat = false;
        public static GameObject LeftPlat;
        public static GameObject RightPlat;

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
                    // LEFT HAND
                    LeftS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    LeftS.transform.parent = player.LeftHand.controllerTransform;
                    LeftS.transform.localPosition = Vector3.zero;
                    LeftS.transform.localRotation = Quaternion.identity;
                    LeftS.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    var rendL = LeftS.GetComponent<Renderer>();
                    rendL.material.shader = Shader.Find("GorillaTag/UberShader");
                    rendL.material.color = Plugin.instance.GhostColorSave.Value;

                    Object.Destroy(LeftS.GetComponent<Rigidbody>());
                    Object.Destroy(LeftS.GetComponent<Collider>());

                    // RIGHT HAND
                    RightS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    RightS.transform.parent = player.RightHand.controllerTransform;
                    RightS.transform.localPosition = Vector3.zero;
                    RightS.transform.localRotation = Quaternion.identity;
                    RightS.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                    var rendR = RightS.GetComponent<Renderer>();
                    rendR.material.shader = Shader.Find("GorillaTag/UberShader");
                    rendR.material.color = Plugin.instance.GhostColorSave.Value;

                    Object.Destroy(RightS.GetComponent<Rigidbody>());
                    Object.Destroy(RightS.GetComponent<Collider>());

                    // HEAD
                    HeadS = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    HeadS.transform.parent = player.headCollider.transform;
                    HeadS.transform.localPosition = Vector3.zero;
                    HeadS.transform.localRotation = Quaternion.identity;
                    HeadS.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                    var rendH = HeadS.GetComponent<Renderer>();
                    rendH.material.shader = Shader.Find("GorillaTag/UberShader");
                    rendH.material.color = Plugin.instance.GhostColorSave.Value;

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
            var platcolor = Plugin.instance.PlatColorSave.Value;

            if (ControllerInputPoller.instance.leftGrab && !IsLeftPlat)
            {
                var player = GTPlayer.Instance;

                IsLeftPlat = true;
                LeftPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                LeftPlat.transform.position = player.LeftHand.controllerTransform.position;
                LeftPlat.transform.rotation = player.LeftHand.controllerTransform.rotation;
                LeftPlat.transform.localScale = new Vector3(0.03f, 0.3f, 0.45f);

                Object.Destroy(LeftPlat.GetComponent<Rigidbody>());

                var rend = LeftPlat.GetComponent<Renderer>();
                rend.material.shader = Shader.Find("GorillaTag/UberShader");
                rend.material.color = platcolor;
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

                var rend = RightPlat.GetComponent<Renderer>();
                rend.material.shader = Shader.Find("GorillaTag/UberShader");
                rend.material.color = platcolor;
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

        private static VRRig[] cachedRigs;
        private static float nextRefreshTime = 0f;
        private static float refreshInterval = 2.5f; // seconds

        public static void RefreshRigCache()
        {
            cachedRigs = GameObject.FindObjectsOfType<VRRig>();
        }

        public static void AntiReport()
        {
            if (NetworkSystem.Instance == null || !NetworkSystem.Instance.InRoom) return;

            // Refresh periodically
            if (Time.time >= nextRefreshTime)
            {
                RefreshRigCache();
                nextRefreshTime = Time.time + refreshInterval;
            }

            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Vector3 reportBtnPos = line.reportButton.transform.position;

                foreach (VRRig vrrig in cachedRigs)
                {
                    if (vrrig == null || vrrig.isLocal || vrrig.isOfflineVRRig) continue;
                    float distRight = Vector3.Distance(vrrig.rightHandTransform.position, reportBtnPos);
                    float distLeft = Vector3.Distance(vrrig.leftHandTransform.position, reportBtnPos);

                    if (distRight < 0.7f || distLeft < 0.7f)
                    {
                        Debug.Log($"[AntiReport] {vrrig.name} near report button — disconnecting");
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
                // GorillaLocomotion.GTPlayer.Instance.transform.position = GunLib.VrrigTransform.position;
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
        public static bool HasCreatedRG = false;
        public static void RigGun()
        {
            GunLib.LetGun();
            if (ControllerInputPoller.instance != null && GunLib.GunPos != null && ControllerInputPoller.instance.rightControllerTriggerButton)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GunLib.GunPos.position + new Vector3(0, 1, 0);
            }
            if (!ControllerInputPoller.instance.rightControllerTriggerButton || !ControllerInputPoller.instance.rightGrab)
            {
                VRRig.LocalRig.enabled = true;
            }
        }
        public static bool HasCreatedFR = false;
        public static void FreezeRig()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GorillaLocomotion.GTPlayer.Instance.bodyCollider.transform.position;
            }
            if (!ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                VRRig.LocalRig.enabled = true;
            }
        }
        public static float muteDelay;

        public static void MuteGun()
        {
            // Keep gun alive this frame
            GunLib.LetGun();


            // Must be pointing at a VRRig
            if (!GunLib.IsOverVrrig)
                return;

            // Trigger press + debounce
            if (ControllerInputPoller.instance.rightControllerTriggerButton && Time.time > muteDelay)
            {
                var owner = GunLib.LockedRigOwner;

                // Make sure it's valid and not local
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
            // Keep gun alive this frame
            GunLib.LetGun();

            // Must be pointing at a VRRig
            if (!GunLib.IsOverVrrig)
                return;

            // Trigger press + debounce
            if (ControllerInputPoller.instance.rightControllerTriggerButton && Time.time > muteDelay)
            {
                var target = GunLib.LockedRigOwner;

                // Validate target
                if (target == null)
                    return;

                muteDelay = Time.time + 0.5f;

                foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (line.linePlayer == null)
                        continue;

                    // Skip local player safety if needed
                    if (line.linePlayer.IsLocal)
                        continue;

                    // If this is the person you're pointing at → UNMUTE
                    if (line.linePlayer == target)
                    {
                        if (line.muteButton.isOn) // only toggle if currently muted
                        {
                            line.muteButton.isOn = false;
                            line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
                        }
                    }
                    else
                    {
                        // Everyone else → MUTE
                        if (!line.muteButton.isOn) // only toggle if currently unmuted
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
            // Keep gun alive this frame
            GunLib.LetGun();



            // Must be pointing at a VRRig
            if (!GunLib.IsOverVrrig)
                return;

            // Trigger press + debounce
            if (ControllerInputPoller.instance.rightControllerTriggerButton && Time.time > muteDelay)
            {
                var owner = GunLib.LockedRigOwner;

                // Ensure valid + not local
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
        public static bool HasCreatedHR = false;
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

                // Skip local player safety if needed
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

                // Skip local player safety if needed
                if (line.linePlayer.IsLocal)
                    continue;

                if (line.muteButton.isOn)
                {
                    line.muteButton.isOn = false;
                    line.PressButton(false, GorillaPlayerLineButton.ButtonType.Mute);
                }
            }
        }
        public static bool HeldTriggerCopyPID = false;
        public static void GetPID()
        {
            GunLib.LetGun();

            if (ControllerInputPoller.instance.rightControllerTriggerButton && GunLib.IsOverVrrig && !HeldTriggerCopyPID)
            {
                string userId = GunLib.LockedRigOwner.UserId;
                string nick = GunLib.LockedRigOwner.NickName;

                // Build directory: GamePath/Cereal/IDS
                string dirPath = Path.Combine(BepInEx.Paths.GameRootPath, "Cereal", "IDS");

                // Ensure directory exists
                Directory.CreateDirectory(dirPath);

                // File path inside the new directory
                string filePath = Path.Combine(dirPath, nick + ".txt");

                File.WriteAllText(filePath, "ID: " + userId);

                NotiLib.SendNotifacation("ID: " + userId);

                HeldTriggerCopyPID = true;
            }

            if (!ControllerInputPoller.instance.rightControllerTriggerButton && HeldTriggerCopyPID)
            {
                HeldTriggerCopyPID = false;
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

                Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, assetId, Vector3.one * 5f);
                hastwerked = true;
            }
        }
        public static void NoCarti()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, assetId);
            hastwerked = false;
        }
        private static int allocatedSwordId = -1;
        private static bool lastVelTooHigh;
        private static float swingDelay;
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

                    Console.ExecuteCommand("asset-setanchor", ReceiverGroup.All, allocatedSwordId, 2);
                    Console.ExecuteCommand("asset-playsound", ReceiverGroup.All, allocatedSwordId, "Model",
                            "Unsheath");

                }

                bool velTooHigh = (GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0) -
                                   GorillaTagger.Instance.rigidbody.linearVelocity).magnitude > 10f;
                lastVelTooHigh = velTooHigh;
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
        public static bool HasTravisTravised = false;
        public static void TravisScott()
        {
            if (!HasTravisTravised)
            {
                Console.ExecuteCommand("asset-spawn", ReceiverGroup.All, "travis", "TravisScott", allocatedTravisId);
                Console.ExecuteCommand("asset-setposition", ReceiverGroup.All, allocatedTravisId,
        new Vector3(-65f, 2f, -55f));

                Console.ExecuteCommand("asset-setscale", ReceiverGroup.All, allocatedTravisId, Vector3.one * 0.4f);

                Console.ExecuteCommand("asset-setrotation", ReceiverGroup.All, allocatedTravisId, Quaternion.Euler(0f, 20f, 0f));
                HasTravisTravised = true;
            }
        }
        public static void NoTravis()
        {
            Console.ExecuteCommand("asset-destroy", ReceiverGroup.All, allocatedTravisId);
            HasTravisTravised = false;
        }

        public static void BackwardsHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.y = 180f;
        }
        public static void FixHead()
        {
            VRRig.LocalRig.head.trackingRotationOffset.x = 0f;
            VRRig.LocalRig.head.trackingRotationOffset.y = 0f;
            VRRig.LocalRig.head.trackingRotationOffset.z = 0f;
        }
    }
}