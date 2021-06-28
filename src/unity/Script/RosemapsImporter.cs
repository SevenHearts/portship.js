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

    public override void OnImportAsset(AssetImportContext ctx)
    {
        var stb = JsonUtility.FromJson<STB>(File.ReadAllText(ctx.assetPath));

        var dedupe = new SortedDictionary<string, int>();

        var enabledNamesSet = new SortedSet<string>(importNames);

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

                string zoneFile = "Assets/ROSE/" + Regex.Replace(mapDef.designation.ToLower(), "\\\\+", "/");
                ctx.DependsOnSourceAsset(zoneFile);
                var zon = RoseZon.FromFile(zoneFile);

                string objectFile = "Assets/ROSE/" + Regex.Replace(mapDef.objectTable.ToLower(), "\\\\+", "/");
                ctx.DependsOnArtifact(objectFile);
                var objects = AssetDatabase.LoadAllAssetsAtPath(objectFile);

                string constFile = "Assets/ROSE/" + Regex.Replace(mapDef.constructionTable.ToLower(), "\\\\+", "/");
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
                        ctx.DependsOnSourceAsset(foundPath);
                        ifoTiles[new Vector2Int(x, y)] = RoseIfo.FromFile(foundPath);
                    }
                }

                if (ifoTiles.Count == 0)
                {
                    ctx.LogImportWarning("ZON file has no IFO files: " + zoneFile);
                    continue;
                }

                foreach (var tileCoord in ifoTiles.Keys)
                {
                    var ifo = ifoTiles[tileCoord];
                    var tileObject = new GameObject(id + " " + tileCoord.x.ToString() + "_" + tileCoord.y.ToString());

                    tileObject.transform.parent = mapObject.transform;

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
