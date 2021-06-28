using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEditor.AssetImporters;
using Kaitai;

[ScriptedImporter(1, "rosemaps", AllowCaching = true)]
public class RosemapsImporter : ScriptedImporter
{
    [SerializeField]
    string[] importNames;

    [System.Serializable]
    private class STB
    {
        [System.Serializable]
        public class RoseMap
        {
            public string name;
            public string designation;
            public string objectTable;
            public string constructionTable;
            public int startX;
            public int startY;
        }

        public RoseMap[] stb;
    }

    private class Vector2IntComparer : IComparer<Vector2Int>
    {
        public int Compare(Vector2Int x, Vector2Int y)
        {
            return (x.x == y.x) ? x.y - y.y : x.x - y.x;
        }
    }

    private static string ToAssetPath(string rosePath) {
        return "Assets/ROSE/" + Regex.Replace(rosePath.ToLower(), @"\\+", "/");
    }

    public override void OnImportAsset(AssetImportContext ctx)
    {
        var stb = JsonUtility.FromJson<STB>(File.ReadAllText(ctx.assetPath));

        var dedupe = new SortedDictionary<string, int>();

        var enabledNamesSet = new SortedSet<string>(importNames);

        string terrainMaterialPath = "Assets/Material/ROSE/ROSETerrain.mat";
        ctx.DependsOnArtifact(terrainMaterialPath);
        Material terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(terrainMaterialPath);

        foreach (STB.RoseMap mapDef in stb.stb)
        {
            if (mapDef.name != null && mapDef.name != "")
            {
                string id = mapDef.name;
                int count = 0;
                dedupe.TryGetValue(id, out count);
                count += 1;
                dedupe[id] = count;
                if (count > 1)
                {
                    id += " " + count.ToString();
                }

                if (!enabledNamesSet.Contains(id)) continue;

                GameObject mapObject = new GameObject(id);

                string zoneFile = ToAssetPath(mapDef.designation);
                ctx.DependsOnSourceAsset(zoneFile);
                var zon = RoseZon.FromFile(zoneFile);

                string objectFile = ToAssetPath(mapDef.objectTable);
                ctx.DependsOnArtifact(objectFile);
                var objects = AssetDatabase.LoadAllAssetsAtPath(objectFile);

                string constFile = ToAssetPath(mapDef.constructionTable);
                ctx.DependsOnArtifact(constFile);
                var constructions = AssetDatabase.LoadAllAssetsAtPath(constFile);

                var objectLookup = new SortedDictionary<uint, GameObject>();
                foreach (var obj in objects) {
                    if (!(obj is GameObject)) continue;

                    var gameObj = (GameObject)obj;
                    if (gameObj.transform.parent == null)
                    {
                        uint objId;
                        if (!uint.TryParse(gameObj.name, out objId)) continue;
                        objectLookup.Add(objId, gameObj);
                    }
                }

                var constructionLookup = new SortedDictionary<uint, GameObject>();
                foreach (var obj in constructions) {
                    if (!(obj is GameObject)) continue;

                    var gameObj = (GameObject)obj;
                    if (gameObj.transform.parent == null)
                    {
                        uint objId;
                        if (!uint.TryParse(gameObj.name, out objId)) continue;
                        constructionLookup.Add(objId, gameObj);
                    }
                }

                // Enumerate all of the tiles that 'might' exist.
                var ifoTiles = new SortedDictionary<Vector2Int, RoseIfo>(new Vector2IntComparer());
                var himTiles = new SortedDictionary<Vector2Int, Mesh>(new Vector2IntComparer());

                string[] foundTiles = AssetDatabase.FindAssets("_", new string[] { Path.GetDirectoryName(zoneFile) });
                foreach (string found in foundTiles)
                {
                    string foundPath = AssetDatabase.GUIDToAssetPath(found);
                    string foundExt = Path.GetExtension(foundPath);
                    if (string.IsNullOrEmpty(foundExt)) continue;
                    string foundName = Path.GetFileNameWithoutExtension(foundPath);

                    Match match = Regex.Match(foundName, @"^([0-9]+)_([0-9]+)$");
                    if (!match.Success) continue;

                    int x = int.Parse(match.Groups[1].Value);
                    int y = int.Parse(match.Groups[2].Value);

                    if (Path.GetExtension(foundPath) == ".ifo")
                    {
                        var tileCoord = new Vector2Int(x, y);

                        ctx.DependsOnSourceAsset(foundPath);
                        ifoTiles[tileCoord] = RoseIfo.FromFile(foundPath);

                        var himPath = Path.ChangeExtension(foundPath, ".him");
                        ctx.DependsOnArtifact(himPath);
                        Object[] himobjs = AssetDatabase.LoadAllAssetsAtPath(himPath);
                        himTiles[tileCoord] = (Mesh)himobjs[0];
                    }
                }

                if (ifoTiles.Count == 0)
                {
                    ctx.LogImportWarning("ZON file has no IFO files: " + zoneFile);
                    continue;
                }

                // Extract some of the basic info from the ZON file
                var heightmapOffset = new Vector2();
                var tileTextures = new List<Texture2D>();

                foreach (var zonBlock in zon.Blocks)
                {
                    switch (zonBlock.Id)
                    {
                        case RoseZon.ZoneBlock.BlockType.BasicInfo:
                            {
                                var info = (RoseZon.ZoneBlock.BlockBasicInfo)zonBlock.Data;
                                heightmapOffset.x = (float)info.XCount;
                                heightmapOffset.y = (float)info.YCount;
                                break;
                            }
                        case RoseZon.ZoneBlock.BlockType.TextureList:
                            {
                                var info = (RoseZon.ZoneBlock.BlockTextureList)zonBlock.Data;
                                foreach (var texData in info.Textures)
                                {
                                    // Poorly designed file format lol.
                                    if (texData.Data == "end") continue;

                                    string texPath = Path.ChangeExtension(ToAssetPath(texData.Data), ".png");
                                    ctx.DependsOnArtifact(texPath);
                                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                                    if (tex == null)
                                    {
                                        ctx.LogImportWarning("Could not find tile texture: " + texPath);
                                    }
                                    tileTextures.Add(tex);
                                }
                                break;
                            }
                    }
                }

                var heightMapRoot = new GameObject(mapObject.name + ".height");
                heightMapRoot.transform.parent = mapObject.transform;

                var tile_i = 0;

                foreach (var tileCoord in ifoTiles.Keys)
                {
                    var tile_id = tile_i++;
                    var ifo = ifoTiles[tileCoord];
                    var tileObject = new GameObject(id + " " + tileCoord.x.ToString() + "_" + tileCoord.y.ToString());

                    tileObject.transform.parent = mapObject.transform;

                    // Make the heightmap gameobject
                    {
                        var heightObject = new GameObject(tileObject.name + ".heightmap");
                        heightObject.transform.parent = heightMapRoot.transform;

                        var meshFilter = heightObject.AddComponent<MeshFilter>();
                        meshFilter.mesh = himTiles[tileCoord];

                        var meshRenderer = heightObject.AddComponent<MeshRenderer>();
                        meshRenderer.sharedMaterial = terrainMaterial;

                        heightObject.transform.position = new Vector3(
                            (tileCoord.x - heightmapOffset.x) * 160f,
                            0,
                            (tileCoord.y - heightmapOffset.y) * 160f
                        );
                    }

                    var ents = 0;
                    foreach (var ifoBlock in ifo.Blocks)
                    {
                        SortedDictionary<uint, GameObject> lookup = objectLookup;
                        RoseIfo.BlockInfo.EntityList entities = null;

                        switch (ifoBlock.Type)
                        {
                            case RoseIfo.BlockInfo.BlockType.Buildings:
                                {
                                    lookup = constructionLookup;
                                    goto case RoseIfo.BlockInfo.BlockType.Decorations;
                                }
                            case RoseIfo.BlockInfo.BlockType.Decorations:
                                {
                                    entities = (RoseIfo.BlockInfo.EntityList)ifoBlock.Data;

                                    foreach (var entDef in entities.Entities)
                                    {
                                        GameObject obj = lookup[entDef.ObjId];
                                        if (obj == null)
                                        {
                                            ctx.LogImportWarning(
                                                "TIL "
                                                + tileCoord.x + "_" + tileCoord.y
                                                + " in ZON " + zoneFile
                                                + " referenced invalid (null) entity ID: "
                                                + entDef.ObjId.ToString()
                                            );
                                            continue;
                                        }

                                        var objPos = entDef.Position;
                                        var objRot = entDef.Rotation;
                                        var objSca = entDef.Scale;

                                        var inst = Instantiate<GameObject>(
                                            obj,
                                            new Vector3(objPos.X / 100f, objPos.Z / 100f, -objPos.Y / 100f),
                                            new Quaternion(objRot.X, objRot.Z, objRot.Y, objRot.W),
                                            tileObject.transform
                                        );

                                        inst.transform.localScale = new Vector3(
                                            objSca.X, objSca.Y, objSca.Z
                                        );

                                        inst.name = (ents++).ToString();
                                        inst.isStatic = true;
                                    }
                                    break;
                                }
                        }
                    }
                }

                ctx.AddObjectToAsset(id, mapObject);
            }
        }
    }
}
