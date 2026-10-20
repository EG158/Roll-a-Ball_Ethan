/*****************************************************************
* COMPONENT OF: Graybox walls that can block the follow camera
* REQUIRED DEPENDENCIES: Renderer with one URP/Lit material
* DESCRIPTION: Makes only the obstructing wall transparent, preserving its solid collider.
* VERSION 1.0: Uses a private transparent material and restores the original when unobstructed.
*****************************************************************/

using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Renderer))]
public class PrototypeCameraOccluder : MonoBehaviour
{
    // Set the visible wall opacity while it blocks the ball; 0 is invisible and 1 is opaque.
    [SerializeField, Range(0.05f, 0.8f)] private float obstructedOpacity = 0.2f;

    // Keep the original asset unchanged so other walls retain their normal appearance.
    private Renderer wallRenderer;
    private Material originalMaterial;
    private Material transparentMaterial;
    private bool obscured;

    private void Awake()
    {
        PrepareMaterials();
    }

    // Build a reusable runtime material instead of allocating a new one every camera frame.
    private void PrepareMaterials()
    {
        wallRenderer = GetComponent<Renderer>();
        originalMaterial = wallRenderer.sharedMaterial;
        transparentMaterial = new Material(originalMaterial);
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
    }

    // The camera calls this only for marked walls between its lens and the player's ball.
    public void SetObscured(bool isObscured)
    {
        if (wallRenderer == null || obscured == isObscured)
        {
            return;
        }
        obscured = isObscured;
        wallRenderer.sharedMaterial = obscured ? transparentMaterial : originalMaterial;
    }

    // Disabling a wall or camera should never leave a material stuck in its faded state.
    private void OnDisable()
    {
        SetObscured(false);
    }

    private void OnDestroy()
    {
        // Only the private runtime copy belongs to this component; never destroy the asset.
        if (transparentMaterial != null)
        {
            Destroy(transparentMaterial);
        }
    }
}
