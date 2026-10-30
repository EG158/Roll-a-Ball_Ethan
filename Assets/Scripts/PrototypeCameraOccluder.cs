/*****************************************************************
* COMPONENT OF: Graybox walls that can block the follow camera
* REQUIRED DEPENDENCIES: Renderer with one URP/Lit material
* DESCRIPTION: Makes an obstructing wall and its visual detail transparent while preserving its solid collider.
* VERSION 1.1: Fades all renderers on the wall and its child decorations together.
*****************************************************************/

using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class PrototypeCameraOccluder : MonoBehaviour
{
    // Set the visible wall opacity while it blocks the ball; 0 is invisible and 1 is opaque.
    [SerializeField, Range(0.05f, 0.8f)] private float obstructedOpacity = 0.2f;

    // Keep the original asset unchanged so other walls retain their normal appearance.
    private Renderer[] wallRenderers;
    private Material[][] originalMaterials;
    private Material[][] transparentMaterials;
    private bool obscured;

    private void Awake()
    {
        PrepareMaterials();
    }

    // Build reusable runtime materials for the wall and its child decorations instead of allocating materials every camera frame.
    private void PrepareMaterials()
    {
        // Include child renderers so decorative panels fade with the wall they cover.
        wallRenderers = GetComponentsInChildren<Renderer>(true);
        originalMaterials = new Material[wallRenderers.Length][];
        transparentMaterials = new Material[wallRenderers.Length][];

        for (int rendererIndex = 0; rendererIndex < wallRenderers.Length; rendererIndex++)
        {
            originalMaterials[rendererIndex] = wallRenderers[rendererIndex].sharedMaterials;
            transparentMaterials[rendererIndex] = new Material[originalMaterials[rendererIndex].Length];

            for (int materialIndex = 0; materialIndex < originalMaterials[rendererIndex].Length; materialIndex++)
            {
                Material originalMaterial = originalMaterials[rendererIndex][materialIndex];
                if (originalMaterial == null)
                {
                    continue;
                }

                // A private copy lets this wall fade without changing the shared project material.
                Material transparentMaterial = new Material(originalMaterial);
                Color color = originalMaterial.color;
                color.a = obstructedOpacity;
                transparentMaterial.color = color;

                // URP requires transparent blending and depth settings as well as an alpha value.
                transparentMaterial.SetFloat("_Surface", 1f);
                transparentMaterial.SetFloat("_Blend", 0f);
                transparentMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                transparentMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                transparentMaterial.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
                transparentMaterial.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                transparentMaterial.SetFloat("_ZWrite", 0f);
                transparentMaterial.SetFloat("_AlphaToMask", 0f);
                // Draw both faces so the fade still works if the camera moves inside a thick wall.
                transparentMaterial.SetFloat("_Cull", (float)CullMode.Off);
                transparentMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                transparentMaterial.DisableKeyword("_ALPHATEST_ON");
                transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                transparentMaterial.SetOverrideTag("RenderType", "Transparent");
                transparentMaterial.renderQueue = (int)RenderQueue.Transparent;
                transparentMaterial.SetShaderPassEnabled("ShadowCaster", false);
                transparentMaterial.SetShaderPassEnabled("DepthOnly", false);
                transparentMaterial.SetShaderPassEnabled("DepthNormals", false);
                transparentMaterials[rendererIndex][materialIndex] = transparentMaterial;
            }
        }
    }

    // The camera calls this only for marked walls between its lens and the player's ball.
    public void SetObscured(bool isObscured)
    {
        if (wallRenderers == null || obscured == isObscured)
        {
            return;
        }
        obscured = isObscured;
        // Fade or restore every cached material so the entire wall assembly changes together.
        for (int rendererIndex = 0; rendererIndex < wallRenderers.Length; rendererIndex++)
        {
            wallRenderers[rendererIndex].sharedMaterials = obscured
                ? transparentMaterials[rendererIndex]
                : originalMaterials[rendererIndex];
        }
    }

    // Disabling a wall or camera should never leave a material stuck in its faded state.
    private void OnDisable()
    {
        SetObscured(false);
    }

    private void OnDestroy()
    {
        // Only these private runtime copies belong to the component; never destroy project material assets.
        if (transparentMaterials == null)
        {
            return;
        }

        foreach (Material[] rendererMaterials in transparentMaterials)
        {
            foreach (Material material in rendererMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }
    }
}
