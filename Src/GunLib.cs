using UnityEngine;
using GorillaLocomotion;
using System.Collections;

namespace CerealMenu
{
    public class GunLib : MonoBehaviour
    {
        public static GameObject GunObject;
        public static Transform GunPos;

        private static LineRenderer lineRenderer;

        private static GunLib instance;

        private static Coroutine rgbCoroutine;

        private static bool isHolding = false;
        private static bool allowThisFrame = false;

        private static VRRig lockedRig = null;

        private static Vector3 lastGunPosition;
        private static Vector3 gunVelocity;

        public static readonly string[] ignoreLayers =
        {
            "Gorilla Trigger",
            "Gorilla Boundary",
            "GorillaHand",
            "GorillaObject",
            "Zone",
            "Water",
            "GorillaCosmetics",
            "GorillaParticle",
        };

        public static bool IsOverVrrig => lockedRig != null;
        public static Transform VrrigTransform => lockedRig != null ? lockedRig.transform : null;
        public static VRRig LockedRig => lockedRig;

        public static NetPlayer LockedRigOwner => lockedRig != null ? lockedRig.Creator : null;

        public static string LockedRigOwnerNick =>
            lockedRig != null && lockedRig.Creator != null ? lockedRig.Creator.NickName : null;

        public static void LetGun()
        {
            allowThisFrame = true;
        }
        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (!allowThisFrame)
            {
                DestroyGun();
                lockedRig = null;
                return;
            }

            allowThisFrame = false;

            if (GTPlayer.Instance == null || ControllerInputPoller.instance == null)
                return;

            bool holding = ControllerInputPoller.instance.rightGrab;
            Transform hand = GTPlayer.Instance.RightHand.controllerTransform;

            if (lockedRig == VRRig.LocalRig)
                lockedRig = null;

            if (holding && !isHolding)
            {
                SpawnGun();
                isHolding = true;
                lastGunPosition = hand.position;
            }

            if (holding && GunObject != null)
            {
                Ray ray = new Ray(hand.position, hand.forward);

                int mask = ~LayerMask.GetMask(ignoreLayers);

                bool hitSomething = Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    1000f,
                    mask,
                    QueryTriggerInteraction.Collide
                );

                Vector3 endPoint = ray.origin + ray.direction * 1000f;

                if (hitSomething)
                {
                    GunObject.transform.position = hit.point;
                    GunObject.transform.rotation = Quaternion.LookRotation(hit.normal);

                    endPoint = hit.point;

                    VRRig hitRig = hit.collider.GetComponentInParent<VRRig>();

                    if (hitRig != null && hitRig != VRRig.LocalRig)
                    {
                        lockedRig = hitRig;
                    }
                }
                else
                {
                    GunObject.transform.position = endPoint;
                    GunObject.transform.rotation = hand.rotation;
                }

                gunVelocity = (GunObject.transform.position - lastGunPosition) / Time.deltaTime;
                lastGunPosition = GunObject.transform.position;

                DrawLine(ray.origin, endPoint);

                GunPos = GunObject.transform;
            }

            if (!holding && isHolding)
            {
                DestroyGun();
                isHolding = false;
                lockedRig = null;
            }
        }

        private static void DrawLine(Vector3 start, Vector3 end)
        {
            if (lineRenderer == null) return;

            int segments = 60;
            float amplitude = 0.05f;
            float frequency = 14f;
            float speed = 5f;

            float movementInfluence = Mathf.Clamp(gunVelocity.magnitude * 0.01f, 0f, 0.2f);

            lineRenderer.positionCount = segments;
            Vector3 direction = (end - start).normalized;

            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
            if (perpendicular == Vector3.zero)
                perpendicular = Vector3.Cross(direction, Vector3.right);

            perpendicular.Normalize();

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                Vector3 point = Vector3.Lerp(start, end, t);

                float wave = Mathf.Sin(t * frequency + Time.time * speed) * amplitude * t;
                float motionWave = Mathf.Sin(t * frequency * 0.5f + Time.time * speed * 0.5f)
                                   * movementInfluence * t;

                point += perpendicular * (wave + motionWave);

                lineRenderer.SetPosition(i, point);
            }
        }
        public static IEnumerator RGBTheme(Renderer targetRenderer)
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

        private static void SpawnGun()
        {
            GunObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GunObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            Object.Destroy(GunObject.GetComponent<Rigidbody>());
            Object.Destroy(GunObject.GetComponent<Collider>());

            var rend = GunObject.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GUI/Text Shader");
            rend.material.color = Plugin.instance.Theme.Value;

            lineRenderer = GunObject.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 30;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;

            Material mat = new Material(Shader.Find("GUI/Text Shader"));
            if (!Plugin.instance.IsMenuRGB.Value)
            {
                mat.color = Plugin.instance.Theme.Value;
            }
            else
            {
                if (Plugin.instance.IsMenuRGB.Value)
                {
                    if (rgbCoroutine != null)
                        instance.StopCoroutine(rgbCoroutine);

                    rgbCoroutine = instance.StartCoroutine(RGBTheme(rend));
                }
            }
            lineRenderer.material = mat;
        }

        private static void DestroyGun()
        {
            if (GunObject != null)
            {
                Object.Destroy(GunObject);
                GunObject = null;
                GunPos = null;
                lineRenderer = null;
            }
        }
    }
}