using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Kaitai;

[ScriptedImporter(1, "zsc", AllowCaching = true)]
public class ZCSImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        string[] leafs = ctx.assetPath.Split('/');
        string name = leafs[leafs.Length - 1];

        GameObject g = new GameObject(name);

        RoseZsc zsc = RoseZsc.FromFile(ctx.assetPath);
        Mesh[] meshes = new Mesh[zsc.MeshCount];

        {
            var i = 0;
            foreach (string meshPath in zsc.MeshPaths)
            {
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/ROSE/" + meshPath);
                if (mesh == null)
                {
                    ctx.LogImportError("loading ZSC asset failed: no such ZMS: " + meshPath);
                    return;
                }

                meshes[i++] = mesh;
            }
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/ROSEGeneric.mat");
        if (material == null)
        {
            ctx.LogImportError("Could not find ROSE Generic material: Assets/Material/ROSEGeneric.mat");
            return;
        }

        var materials = zsc.Materials;
        {
            var i = 0;

            foreach (var objDef in zsc.Objects)
            {
                if (objDef.MeshCount == 0) continue;

                GameObject child = new GameObject((i++).ToString());
                child.transform.parent = g.transform;

                foreach (var meshDef in objDef.Meshes)
                {
                    GameObject meshObject = new GameObject((i++).ToString());
                    meshObject.transform.parent = child.transform;

                    var matDef = materials[meshDef.MaterialId];
                    var matConfig = meshObject.AddComponent<ROSEMaterialConfigurator>();
                    matConfig.color = new Color(
                        matDef.Red,
                        matDef.Green,
                        matDef.Blue,
                        (matDef.AlphaEnabled != 0) ? matDef.Alpha : 1.0f
                    );

                    var meshFilter = meshObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = meshes[meshDef.MeshId];

                    var renderer = meshObject.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = material;

                    renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

                    // TODO turn these back on when we want to try shadows :)
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;

                    foreach (var prop in meshDef.Properties)
                    {
                        switch (prop.Type)
                        {
                            case RoseZsc.MeshPropType.Position:
                                {
                                    var pos = (RoseZsc.Vec3)prop.Data;
                                    meshObject.transform.localPosition = new Vector3(
                                        pos.X / 100f, pos.Z / 100f, -pos.Y / 100f
                                    );
                                    break;
                                }
                            case RoseZsc.MeshPropType.Scale:
                                {
                                    var scale = (RoseZsc.Vec3)prop.Data;
                                    meshObject.transform.localScale = new Vector3(
                                        scale.X, scale.Z, scale.Y
                                    );
                                    break;
                                }
                            case RoseZsc.MeshPropType.Rotation:
                                {
                                    var rot = (RoseZsc.Vec4)prop.Data;
                                    meshObject.transform.rotation = new Quaternion(
                                        rot.X, rot.Y, rot.Z, rot.W
                                    );
                                    break;
                                }
                        }
                    }
                }
            }
        }

        ctx.AddObjectToAsset("main obj", g);
        ctx.SetMainObject(g);
    }
}
