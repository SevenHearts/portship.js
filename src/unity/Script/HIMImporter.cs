using Kaitai;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "him", AllowCaching = true)]
public class HIMImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var him = RoseHim.FromFile(ctx.assetPath);

        var height = (int)him.Height - 1;
        var width = (int)him.Width - 1;
        var heights = him.Heights;

        Mesh mesh = new Mesh();

        float cellSize = him.GridSize / 100.0f;

        Vector2 size;
        size.x = cellSize * width;
        size.y = cellSize * height;

        Vector2 halfSize = size / 2;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        Vector3 vertice = Vector3.zero;
        Vector2 uv = Vector3.zero;

        for (int y = 0; y < height + 1; y++)
        {
            vertice.z = y * cellSize - halfSize.y;
            uv.y = y * cellSize / size.y;

            for (int x = 0; x < width + 1; x++)
            {
                vertice.x = x * cellSize - halfSize.x;
                uv.x = x * cellSize / size.x;

                vertice.y = heights[(y * (width + 1)) + x] / 100f;

                vertices.Add(vertice);
                uvs.Add(uv);
            }
        }

        int a = 0;
        int b = 0;
        int c = 0;
        int d = 0;
        int startIndex = 0;
        int[] indexs = new int[width * height * 2 * 3];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                a = y * (width + 1) + x;
                b = (y + 1) * (width + 1) + x;
                c = b + 1;
                d = a + 1;

                startIndex = y * width * 2 * 3 + x * 2 * 3;

                indexs[startIndex] = a;
                indexs[startIndex + 1] = b;
                indexs[startIndex + 2] = c;

                indexs[startIndex + 3] = c;
                indexs[startIndex + 4] = d;
                indexs[startIndex + 5] = a;
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indexs, MeshTopology.Triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        ctx.AddObjectToAsset("mesh", mesh);
        ctx.SetMainObject(mesh);
    }
}
