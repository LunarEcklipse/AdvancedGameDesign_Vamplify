using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FadeObjectsObstructingCamera : MonoBehaviour
{
    public static List<GameObject> fadeForObjects = new();

    private List<FadeableObject> objectsHit = new();

    private LayerMask layerMask;
    private Transform target;
    
    private Camera cam;
    [SerializeField][Range(0.0f, 1.0f)] private float fadedAlpha = 0.33f;
    [SerializeField] private Vector3 targetPositionOffset = new(0.0f, -0.5f, 0.0f);
    [SerializeField] private bool retainShadows = true;
    
    [SerializeField] private float fadeSpeed = 1.0f;
    [SerializeField] private int maxRaycastHits = 10;

    [Header("Read Only Data")]
    [SerializeField] List<FadeableObject> objectsBlockingView = new();
    private Dictionary<FadeableObject, Coroutine> runningCoroutines = new();

    private RaycastHit[] raycastHits;

    /* *** STATIC STUFF *** */
    public static void AddObjectToFadeFor(GameObject obj)
    {
        if (!fadeForObjects.Contains(obj))
        {
            fadeForObjects.Add(obj);
        }
    }
    public static void RemoveObjectToFadeFor(GameObject obj)
    {
        if (fadeForObjects.Contains(obj))
        {
            fadeForObjects.Remove(obj);
        }
    }
    public static void ClearObjectsToFadeFor()
    {
        fadeForObjects.Clear();
    }
    private IEnumerator FadeObjectOut(FadeableObject fadeObject)
    {
        foreach (Material material in fadeObject.Materials)
        {
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0.0f);
            material.SetFloat("_Surface", 1.0f);

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            material.SetShaderPassEnabled("DepthOnly", false);
            material.SetShaderPassEnabled("ShadowCaster", retainShadows);

            material.SetOverrideTag("RenderType", "Transparent");

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        float time = 0.0f;
        while (fadeObject.Materials[0].color.a > fadedAlpha)
        {
            for (int i = 0; i < fadeObject.Materials.Count; i++)
            {
                if (fadeObject.Materials[i].HasProperty("_Color"))
                {
                    fadeObject.Materials[i].color = new Color(fadeObject.Materials[i].color.r, fadeObject.Materials[i].color.g, fadeObject.Materials[i].color.b, Mathf.Lerp(fadeObject.initialAlpha, fadedAlpha, time * fadeSpeed));
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        if (runningCoroutines.ContainsKey(fadeObject))
        {
            StopCoroutine(runningCoroutines[fadeObject]);
            runningCoroutines.Remove(fadeObject);
        }
    }
    private IEnumerator FadeObjectIn(FadeableObject fadeObject)
    {
        float time = 0.0f;
        while (fadeObject.Materials[0].color.a <= fadeObject.initialAlpha)
        {
            for (int i = 0; i < fadeObject.Materials.Count; i++)
            {
                if (fadeObject.Materials[i].HasProperty("_Color")) // Change to _Color if does not work as intended.
                {
                    fadeObject.Materials[i].color = new Color(fadeObject.Materials[i].color.r, fadeObject.Materials[i].color.g, fadeObject.Materials[i].color.b, Mathf.Lerp(fadedAlpha, fadeObject.initialAlpha, time * fadeSpeed));
                }
            }
            time += Time.deltaTime;
            yield return null;
        }

        foreach(Material material in fadeObject.Materials)
        {
            material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetFloat("_ZWrite", 1);
            material.SetFloat("_Surface", 0);

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

            material.SetShaderPassEnabled("DepthOnly", true);
            material.SetShaderPassEnabled("ShadowCaster", true);

            material.SetOverrideTag("RenderType", "Opaque");

            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        if (runningCoroutines.ContainsKey(fadeObject))
        {
            StopCoroutine(runningCoroutines[fadeObject]);
            runningCoroutines.Remove(fadeObject);
        }
    }
    private IEnumerator CheckForObjects()
    {
        while (true)
        {
            objectsHit.Clear();
            Ray personalTransformRay = new(cam.transform.position, (target.transform.position + targetPositionOffset - cam.transform.position).normalized);
            int hits = Physics.RaycastNonAlloc(personalTransformRay, raycastHits, Vector3.Distance(cam.transform.position, target.transform.position + targetPositionOffset), layerMask);
            if (hits > 0)
            {
                for (int i = 0; i < hits; i++)
                {
                    FadeableObject obj = GetFadeableObjectFromHit(raycastHits[i]);
                    if (obj != null && !objectsHit.Contains(obj)) objectsHit.Add(obj);
                }
            }
            foreach(GameObject fadeForObj in fadeForObjects)
            {
                if (fadeForObj == null) continue;
                Ray ray = new(cam.transform.position, (fadeForObj.transform.position + targetPositionOffset - cam.transform.position).normalized);
                hits = Physics.RaycastNonAlloc(ray, raycastHits, Vector3.Distance(cam.transform.position, fadeForObj.transform.position + targetPositionOffset), layerMask);
                if (hits > 0)
                {
                    for (int i = 0; i < hits; i++)
                    {
                        FadeableObject obj = GetFadeableObjectFromHit(raycastHits[i]);
                        if (obj != null && !objectsHit.Contains(obj)) objectsHit.Add(obj);
                    }
                }
            }
            // Remake the list with all null objects removed
            fadeForObjects = fadeForObjects.Where(obj => obj != null).ToList();
            foreach(FadeableObject fadeObject in objectsHit)
            {
                if (fadeObject == null) continue;
                if (fadeObject != null && !objectsBlockingView.Contains(fadeObject))
                {
                    if (runningCoroutines.ContainsKey(fadeObject))
                    {
                        if (runningCoroutines[fadeObject] != null)
                        {
                            if (!objectsBlockingView.Contains(fadeObject)) StopCoroutine(runningCoroutines[fadeObject]);
                        }

                        runningCoroutines.Remove(fadeObject);
                    }
                    // If the object is already on objectsBLockingView we can skip this.
                    if (!objectsBlockingView.Contains(fadeObject))
                    {
                        runningCoroutines.Add(fadeObject, StartCoroutine(FadeObjectOut(fadeObject)));
                        objectsBlockingView.Add(fadeObject);
                    }
                }
            }
            objectsHit = objectsHit.Where(obj => obj != null).ToList();

            FadeObjectsNoLongerBeingHit();
            ClearRaycastHits();

            yield return null;
        }
    }
    private void FadeObjectsNoLongerBeingHit()
    {
        List<FadeableObject> objectsToRemove = new(objectsBlockingView.Count);

        foreach(FadeableObject obj in objectsBlockingView)
        {
            bool objectIsBeingHit = false;
            for (int i = 0; i < raycastHits.Length; i++)
            {
                FadeableObject hitObject = GetFadeableObjectFromHit(raycastHits[i]);
                if (hitObject != null && obj == hitObject)
                {
                    objectIsBeingHit = true;
                    break;
                }
            }

            if (!objectIsBeingHit)
            {
                if (runningCoroutines.ContainsKey(obj))
                {
                    if (runningCoroutines[obj] != null)
                    {
                        StopCoroutine(runningCoroutines[obj]);
                    }
                    runningCoroutines.Remove(obj);
                }

                runningCoroutines.Add(obj, StartCoroutine(FadeObjectIn(obj)));
                objectsToRemove.Add(obj);
            }
        }

        foreach(FadeableObject obj in objectsToRemove)
        {
            objectsBlockingView.Remove(obj);
        }
    }
    private FadeableObject GetFadeableObjectFromHit(RaycastHit rayHit) { return rayHit.collider != null ? rayHit.collider.GetComponent<FadeableObject>() : null; }
    private void ClearRaycastHits() { System.Array.Clear(raycastHits, 0, raycastHits.Length); }
    private void Awake()
    {
        raycastHits = new RaycastHit[maxRaycastHits]; // By doing this this way, we can set the size of the array in the inspector.
    }
    
    // Start is called before the first frame update
    void Start()
    {
        layerMask = LayerMask.GetMask("WallBlocker");
        target = this.transform;
        cam = Camera.main;
        CombatManager.Instance.GenerateNewRoom.AddListener(OnGenerateNewRoom);
        StartCoroutine(CheckForObjects());
    }

    private void OnDestroy()
    {
        CombatManager.Instance.GenerateNewRoom.RemoveListener(OnGenerateNewRoom);
    }

    /* *** EVENT HANDLERS *** */
    private void OnGenerateNewRoom()
    {
        // Clear the list of objects we're tracking for fading.
        FadeObjectsObstructingCamera.ClearObjectsToFadeFor();
        objectsBlockingView.Clear(); // Everything on this list is getting destroyed so we don't need anything here anymore.
    }
    private void OnCombatStart()
    {
        // Get a list of all combatants in the scene.
    }
}
