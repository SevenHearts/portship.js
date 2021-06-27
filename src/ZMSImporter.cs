using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;
using Kaitai;

[ScriptedImporter(1, "zms")]
public class ZMSImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        RoseZms zms = RoseZms.FromFile(ctx.assetPath);

        Mesh mesh = new Mesh();

        switch (zms.Version)
        {
            case RoseZms.ZmsVersion.V6:
                {
                    RoseZms.Zms6 model = (RoseZms.Zms6)zms.Data;
                    var vcount = model.VertCount;

                    if ((zms.Format & RoseZms.VertexFormat.Position) == 0)
                    {
                        ctx.LogImportError("ZMS missing required POSITION format specifier");
                        return;
                    }

                    {
                        var positions = model.Vertices;
                        var newPositions = new Vector3[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var pos = positions[i];
                            newPositions[i] = new Vector3(
                                pos.X, pos.Z, -pos.Y
                            );
                        }

                        mesh.SetVertices(newPositions);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Normal) != 0)
                    {
                        var normals = model.VertNormals;
                        var newNormals = new Vector3[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var norm = normals[i];
                            newNormals[i] = new Vector3(
                                norm.X, norm.Z, -norm.Y
                            );
                        }

                        mesh.SetNormals(newNormals);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Color) != 0)
                    {
                        var colors = model.VertColors;
                        var newColors = new Color[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var col = colors[i];
                            newColors[i] = new Color(
                                (float)col.R / 255.0f,
                                (float)col.G / 255.0f,
                                (float)col.B / 255.0f,
                                (float)col.A / 255.0f
                            );
                        }

                        mesh.SetColors(newColors);
                    }

                    // XXX This is a guess. I have no idea what this achieves, to be completel honest.
                    // XXX It might not work at all.
                    if ((zms.Format & RoseZms.VertexFormat.Tangent) != 0)
                    {
                        var tangents = model.VertTangents;
                        var newTangents = new Vector4[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var tan = tangents[i];
                            newTangents[i] = new Vector4(
                                tan.X, tan.Z, -tan.Y, 1
                            );
                        }

                        mesh.SetTangents(newTangents);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Uvmap1) != 0)
                    {
                        var uv = model.VertUv1coords;
                        var newCoords = new Vector2[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var coord = uv[i];
                            newCoords[i] = new Vector2(
                                coord.X, coord.Y
                            );
                        }

                        mesh.SetUVs(0, newCoords);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Uvmap2) != 0)
                    {
                        var uv = model.VertUv2coords;
                        var newCoords = new Vector2[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var coord = uv[i];
                            newCoords[i] = new Vector2(
                                coord.X, coord.Y
                            );
                        }

                        mesh.SetUVs(1, newCoords);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Uvmap3) != 0)
                    {
                        var uv = model.VertUv3coords;
                        var newCoords = new Vector2[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var coord = uv[i];
                            newCoords[i] = new Vector2(
                                coord.X, coord.Y
                            );
                        }

                        mesh.SetUVs(2, newCoords);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Uvmap4) != 0)
                    {
                        var uv = model.VertUv4coords;
                        var newCoords = new Vector2[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var coord = uv[i];
                            newCoords[i] = new Vector2(
                                coord.X, coord.Y
                            );
                        }

                        mesh.SetUVs(3, newCoords);
                    }

                    {
                        var faceCount = model.FaceCount;
                        if (vcount > 65535)
                        {
                            ctx.LogImportError("vertex count exceeds maximum supported (65535) - support for 32-bit indices will need to be added");
                            return;
                        }

                        ushort[] indices = new ushort[faceCount * 3];
                        var faces = model.Faces;
                        for (var i = 0; i < faceCount; i++)
                        {
                            var verts = faces[i].Verts;
                            var b = i * 3;
                            indices[b] = (ushort)verts[0];
                            indices[b + 1] = (ushort)verts[1];
                            indices[b + 2] = (ushort)verts[2];
                        }

                        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
                        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                    }

                    break;
                }
            case RoseZms.ZmsVersion.V7:
            case RoseZms.ZmsVersion.V8:
                {
                    RoseZms.Zms78 model = (RoseZms.Zms78)zms.Data;
                    var vcount = model.VertCount;

                    if ((zms.Format & RoseZms.VertexFormat.Position) == 0)
                    {
                        ctx.LogImportError("ZMS missing required POSITION format specifier");
                        return;
                    }

                    {
                        var positions = model.VertPositions;
                        var newPositions = new Vector3[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var pos = positions[i];
                            newPositions[i] = new Vector3(
                                pos.X, pos.Z, -pos.Y
                            );
                        }

                        mesh.SetVertices(newPositions);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Normal) != 0)
                    {
                        var normals = model.VertNormals;
                        var newNormals = new Vector3[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var norm = normals[i];
                            newNormals[i] = new Vector3(
                                norm.X, norm.Z, -norm.Y
                            );
                        }

                        mesh.SetNormals(newNormals);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Color) != 0)
                    {
                        var colors = model.VertColors;
                        var newColors = new Color[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var col = colors[i];
                            newColors[i] = new Color(
                                (float)col.R / 255.0f,
                                (float)col.G / 255.0f,
                                (float)col.B / 255.0f,
                                (float)col.A / 255.0f
                            );
                        }

                        mesh.SetColors(newColors);
                    }

                    // XXX This is a guess. I have no idea what this achieves, to be completel honest.
                    // XXX It might not work at all.
                    if ((zms.Format & RoseZms.VertexFormat.Tangent) != 0)
                    {
                        var tangents = model.VertTangents;
                        var newTangents = new Vector4[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var tan = tangents[i];
                            newTangents[i] = new Vector4(
                                tan.X, tan.Z, -tan.Y, 1
                            );
                        }

                        mesh.SetTangents(newTangents);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Uvmap1) != 0)
                    {
                        var uv = model.VertUv1coords;
                        var newCoords = new Vector2[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var coord = uv[i];
                            newCoords[i] = new Vector2(
                                coord.X, coord.Y
                            );
                        }

                        mesh.SetUVs(0, newCoords);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Uvmap2) != 0)
                    {
                        var uv = model.VertUv2coords;
                        var newCoords = new Vector2[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var coord = uv[i];
                            newCoords[i] = new Vector2(
                                coord.X, coord.Y
                            );
                        }

                        mesh.SetUVs(1, newCoords);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Uvmap3) != 0)
                    {
                        var uv = model.VertUv3coords;
                        var newCoords = new Vector2[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var coord = uv[i];
                            newCoords[i] = new Vector2(
                                coord.X, coord.Y
                            );
                        }

                        mesh.SetUVs(2, newCoords);
                    }

                    if ((zms.Format & RoseZms.VertexFormat.Uvmap4) != 0)
                    {
                        var uv = model.VertUv4coords;
                        var newCoords = new Vector2[vcount];
                        for (var i = 0; i < vcount; i++)
                        {
                            var coord = uv[i];
                            newCoords[i] = new Vector2(
                                coord.X, coord.Y
                            );
                        }

                        mesh.SetUVs(3, newCoords);
                    }

                    {
                        var faceCount = model.FaceCount;
                        ushort[] indices = new ushort[faceCount * 3];
                        var faces = model.Faces;
                        for (var i = 0; i < faceCount; i++)
                        {
                            var verts = faces[i].Verts;
                            var b = i * 3;
                            indices[b] = verts[0];
                            indices[b + 1] = verts[1];
                            indices[b + 2] = verts[2];
                        }

                        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
                        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                    }

                    break;
                }
            default:
                ctx.LogImportError("unknown ZMS version: " + zms.Version.ToString());
                return;
        }

        ctx.AddObjectToAsset("mesh", mesh);

        //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        //ctx.AddObjectToAsset("main obj", cube);
        //ctx.SetMainObject(cube);
    }
}
