using UnityEngine;

[ExecuteAlways]
public class OceanRippleAnimator : MonoBehaviour
{
    public Vector2 scrollSpeed = new Vector2(0.015f, 0.01f);

    private Renderer cachedRenderer;
    private MaterialPropertyBlock block;
    private Vector2 offset;

    private void OnEnable()
    {
        cachedRenderer = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (cachedRenderer == null)
            return;

        offset += scrollSpeed * Time.deltaTime;

        cachedRenderer.GetPropertyBlock(block);

        // Works for URP Lit / Standard texture offset if a texture is later assigned.
        block.SetVector("_BaseMap_ST", new Vector4(1f, 1f, offset.x, offset.y));
        block.SetVector("_MainTex_ST", new Vector4(1f, 1f, offset.x, offset.y));

        cachedRenderer.SetPropertyBlock(block);
    }
}