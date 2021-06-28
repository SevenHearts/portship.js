using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer))]
public class ROSEMaterialConfigurator : MonoBehaviour
{
    public Color color;
    public Texture2D texture;

    private MaterialPropertyBlock block;
    private MeshRenderer meshRenderer;

    void Start()
    {
        block = new MaterialPropertyBlock();
        meshRenderer = GetComponent<MeshRenderer>();
        Apply();   
    }

    private void Apply()
    {
        block.SetColor("_Color", color);
        block.SetTexture("_MainTex", texture);
        meshRenderer.SetPropertyBlock(block);
    }
}
