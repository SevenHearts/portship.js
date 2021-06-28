using Kaitai;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "zsc", AllowCaching = true)]
public class ZCSImporter : ScriptedImporter
{
    private static Material loadMaterial(string name)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/ROSE/" + name + ".mat");
        if (material == null)
        {
            throw new System.Exception("Could not find ROSE Generic material: Assets/Material/ROSE/" + name + ".mat");
        }
        return material;
    }

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
                string meshPathClean = "Assets/ROSE/" + meshPath.Replace('\\', '/');
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPathClean);

                if (mesh == null)
                {
                    ctx.LogImportError("loading ZSC asset failed: no such ZMS: " + meshPathClean);
                }

                meshes[i++] = mesh;
            }
        }

        var materials = zsc.Materials;
        Material[] roseMaterials = new Material[zsc.MaterialCount];
        Texture2D[] textures = new Texture2D[zsc.MaterialCount];

        {
            var i = 0;
            foreach (var mat in materials)
            {
                string matPath = "Assets/ROSE/" + mat.Path.ToLower().Substring(0, mat.Path.Length - 4).Replace('\\', '/') + ".png";
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(matPath);

                if (texture == null)
                {
                    ctx.LogImportWarning("failed to find ROSE texture: " + matPath + "\nwhen importing ZSC: " + ctx.assetPath);
                }

                var id = i++;

                string matName = "ROSEUnlit";

                if (mat.TwoSided != 0) matName += "2";	
                if (mat.ZWriteEnabled != 0) matName += "W";	
                if (mat.ZTestEnabled != 0) matName += "Z";
                if (mat.AlphaEnabled != 0)
                {
                    matName += (mat.AlphaTestEnabled != 0) ? "T" : "A";
                }

                roseMaterials[id] = loadMaterial(matName);
                textures[id] = texture;
            }
        }

        {
            var outer_i = 0;

            foreach (var objDef in zsc.Objects)
            {
                var outer_id = outer_i++;
                if (objDef.MeshCount == 0) continue;

                GameObject child = new GameObject(outer_id.ToString());
                child.transform.parent = g.transform;

                var inner_i = 0;
                var parents = new ushort[objDef.MeshCount];
                var meshObjects = new GameObject[objDef.MeshCount + 1];
                meshObjects[0] = child;

                foreach (var meshDef in objDef.Meshes)
                {
                    var inner_id = inner_i++;
                    GameObject meshObject = new GameObject(inner_id.ToString());
                    meshObjects[inner_i] = meshObject;

                    var matDef = materials[meshDef.MaterialId];
                    var matConfig = meshObject.AddComponent<ROSEMaterialConfigurator>();
                    matConfig.color = new Color(
                        matDef.Red,
                        matDef.Green,
                        matDef.Blue,
                        (matDef.AlphaEnabled != 0) ? matDef.Alpha : 1.0f
                    );
                    matConfig.texture = textures[meshDef.MaterialId];

                    var meshFilter = meshObject.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = meshes[meshDef.MeshId];

                    // This is already added by the ROSEMaterialConfigurator
                    var renderer = meshObject.GetComponent<MeshRenderer>();
                    renderer.sharedMaterial = roseMaterials[meshDef.MaterialId];

                    renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

                    // TODO turn these back on when we want to try shadows :)
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;

                    parents[inner_id] = 0;

                    GameObject parent = child;
                    Vector3 localPosition = meshObject.transform.localPosition;
                    Quaternion localRotation = meshObject.transform.localRotation;
                    Vector3 localScale = meshObject.transform.localScale;

                    foreach (var prop in meshDef.Properties)
                    {
                        
                        switch (prop.Type)
                        {
                            case RoseZsc.MeshPropType.Parent:
                                {
                                    var parentId = (ushort)prop.Data;
                                    parent = meshObjects[parentId];
                                    break;
                                }
                            case RoseZsc.MeshPropType.Position:
                                {
                                    var pos = (RoseZsc.Vec3)prop.Data;
                                    localPosition = new Vector3(
                                        pos.X / 100f, pos.Z / 100f, -pos.Y / 100f
                                    );
                                    break;
                                }
                            case RoseZsc.MeshPropType.Scale:
                                {
                                    var scale = (RoseZsc.Vec3)prop.Data;
                                    localScale = new Vector3(
                                        scale.X, scale.Z, scale.Y
                                    );
                                    break;
                                }
                            case RoseZsc.MeshPropType.Rotation:
                                {
                                    var rot = (RoseZsc.Vec4)prop.Data;
                                    localRotation = new Quaternion(
                                        rot.X, rot.Y, rot.Z, rot.W
                                    );
                                    break;
                                }
                        }
                    }

                    meshObject.transform.SetParent(parent.transform);
                    meshObject.transform.localPosition = localPosition;
                    meshObject.transform.localRotation = localRotation;
                    meshObject.transform.localScale = localScale;
                }
            }
        }

        ctx.AddObjectToAsset("main obj", g);
        ctx.SetMainObject(g);
    }
}
