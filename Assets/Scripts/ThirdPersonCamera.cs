using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 3.5f, -6f);
    public float smoothSpeed = 10f;

    [Header("Camera Obstruction")]
    [Range(0f, 1f)]
    public float obstructedOpacity = 0.25f;
    public LayerMask obstructionLayers = ~0;

    private readonly Dictionary<Renderer, OccludedRenderer> occludedRenderers = new Dictionary<Renderer, OccludedRenderer>();
    private readonly HashSet<Renderer> obstructedRenderers = new HashSet<Renderer>();
    private readonly List<Renderer> renderersToRestore = new List<Renderer>();

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 target = player.position + Vector3.up * 1.2f;
        Vector3 targetPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        transform.LookAt(target);
        UpdateObstructingRenderers(target);
    }

    private void OnDisable()
    {
        RestoreAllRenderers();
    }

    private void UpdateObstructingRenderers(Vector3 target)
    {
        obstructedRenderers.Clear();

        Vector3 toTarget = target - transform.position;
        float distance = toTarget.magnitude;
        if (distance > 0f)
        {
            RaycastHit[] hits = Physics.RaycastAll(
                transform.position,
                toTarget / distance,
                distance,
                obstructionLayers,
                QueryTriggerInteraction.Ignore);

            foreach (RaycastHit hit in hits)
            {
                Transform hitTransform = hit.collider.transform;
                if (hitTransform == player || hitTransform.IsChildOf(player))
                {
                    continue;
                }

                Renderer obstructingRenderer = hit.collider.GetComponent<Renderer>();
                if (obstructingRenderer == null)
                {
                    obstructingRenderer = hit.collider.GetComponentInParent<Renderer>();
                }

                if (obstructingRenderer != null)
                {
                    obstructedRenderers.Add(obstructingRenderer);
                    MakeTranslucent(obstructingRenderer);
                }
            }
        }

        renderersToRestore.Clear();
        foreach (KeyValuePair<Renderer, OccludedRenderer> entry in occludedRenderers)
        {
            if (entry.Key == null || !obstructedRenderers.Contains(entry.Key))
            {
                RestoreRenderer(entry.Value);
                renderersToRestore.Add(entry.Key);
            }
        }

        foreach (Renderer renderer in renderersToRestore)
        {
            occludedRenderers.Remove(renderer);
        }
    }

    private void MakeTranslucent(Renderer renderer)
    {
        if (occludedRenderers.ContainsKey(renderer))
        {
            return;
        }

        Material[] originalMaterials = renderer.sharedMaterials;
        Material[] translucentMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            Material original = originalMaterials[i];
            if (original == null)
            {
                DestroyMaterials(translucentMaterials);
                return;
            }

            bool isUrpLit = original.HasProperty("_Surface") && original.HasProperty("_BaseColor");
            bool isStandard = original.HasProperty("_Mode") && original.HasProperty("_Color");
            if (!isUrpLit && !isStandard)
            {
                DestroyMaterials(translucentMaterials);
                return;
            }

            Material translucent = new Material(original)
            {
                name = original.name + " (Camera Translucent)"
            };
            string colorProperty = isUrpLit ? "_BaseColor" : "_Color";
            Color color = original.GetColor(colorProperty);
            color.a *= obstructedOpacity;
            translucent.SetColor(colorProperty, color);
            if (isUrpLit)
            {
                translucent.SetFloat("_Surface", 1f);
                translucent.SetFloat("_Blend", 0f);
                translucent.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                translucent.SetFloat("_Mode", 3f);
                translucent.EnableKeyword("_ALPHABLEND_ON");
            }

            translucent.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            translucent.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (translucent.HasProperty("_SrcBlendAlpha"))
            {
                translucent.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            }

            if (translucent.HasProperty("_DstBlendAlpha"))
            {
                translucent.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            translucent.SetFloat("_ZWrite", 0f);
            translucent.SetOverrideTag("RenderType", "Transparent");
            translucent.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            translucent.DisableKeyword("_ALPHATEST_ON");
            translucent.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            translucent.DisableKeyword("_ALPHAMODULATE_ON");
            translucentMaterials[i] = translucent;
        }

        occludedRenderers.Add(renderer, new OccludedRenderer(renderer, originalMaterials, translucentMaterials));
        renderer.sharedMaterials = translucentMaterials;
    }

    private void RestoreRenderer(OccludedRenderer occludedRenderer)
    {
        if (occludedRenderer.renderer != null)
        {
            occludedRenderer.renderer.sharedMaterials = occludedRenderer.originalMaterials;
        }

        DestroyMaterials(occludedRenderer.translucentMaterials);
    }

    private void RestoreAllRenderers()
    {
        foreach (OccludedRenderer occludedRenderer in occludedRenderers.Values)
        {
            RestoreRenderer(occludedRenderer);
        }

        occludedRenderers.Clear();
    }

    private static void DestroyMaterials(Material[] materials)
    {
        foreach (Material material in materials)
        {
            if (material == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
        }
    }

    private sealed class OccludedRenderer
    {
        public readonly Renderer renderer;
        public readonly Material[] originalMaterials;
        public readonly Material[] translucentMaterials;

        public OccludedRenderer(Renderer renderer, Material[] originalMaterials, Material[] translucentMaterials)
        {
            this.renderer = renderer;
            this.originalMaterials = originalMaterials;
            this.translucentMaterials = translucentMaterials;
        }
    }
}
