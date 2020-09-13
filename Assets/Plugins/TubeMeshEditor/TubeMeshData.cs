using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// チューブメッシュデータ
/// </summary>
public class TubeMeshData : ScriptableObject
{
    /// <summary>
    /// ベジェ曲線
    /// </summary>
    [SerializeField]
    public PathCreation.PathCreator bezier = null;
    /// <summary>
    /// 半径
    /// </summary>
    [SerializeField]
    public float radius = 1f;
    /// <summary>
    /// 円を構成する辺の数
    /// </summary>
    [SerializeField]
    public int edgeCount = 16;
    /// <summary>
    /// スケール
    /// </summary>
    [SerializeField]
    public float scale = 1f;

    /// <summary>
    /// メッシュ生成
    /// </summary>
    public Mesh CreateMesh()
    {
        if (this.bezier == null)
        {
            return null;
        }

        //チューブを構成する各リングの頂点
        var ringVerts = new Vector3[this.bezier.path.NumPoints][];

        for (int i = 0, imax = ringVerts.Length; i < imax; i++)
        {
            //UVの関係上、最初と最後の頂点を重複させて持たせるため「+ 1」
            ringVerts[i] = new Vector3[this.edgeCount + 1];

            //リングの中心点
            var center = this.bezier.path.GetPoint(i) * this.scale;

            //リングの向き
            var forward = (i < imax - 1)
                        ? this.bezier.path.GetPoint(i + 1) * this.scale - center
                        : center - this.bezier.path.GetPoint(i - 1) * this.scale;

            //リングの頂点を決定
            for (int j = 0, jmax = ringVerts[i].Length; j < jmax; j++)
            {
                var angle = 360f / this.edgeCount * j;
                ringVerts[i][j] = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up * this.radius;
                ringVerts[i][j] = Quaternion.LookRotation(forward, Vector3.up) * ringVerts[i][j];
                ringVerts[i][j] += center;
            }
        }

        //メッシュの三角形配列
        var triangles = new List<int>();

        for (int i = 0, imax = ringVerts.Length - 1, offset = 0; i < imax; i++)
        {
            for (int j = 0, jmax = ringVerts[i].Length; j < jmax; j++)
            {
                /*
                ringB   B0----B1----B2---
                        |     |     |    
                ringA   A0----A1----A2---
                */

                int A0 = offset + j;
                int A1 = ((A0 + 1) % jmax) + offset;
                int B0 = A0 + jmax;
                int B1 = A1 + jmax;

                triangles.AddRange(new int[]{ A0, B0, B1 });
                triangles.AddRange(new int[]{ A0, B1, A1 });
            }

            offset += ringVerts[i].Length;
        }

        //各頂点のUV
        var uv = new Vector2[this.bezier.path.NumPoints * (this.edgeCount + 1)];
        var magnitudes = new float[this.edgeCount + 1];

        for (int i = 0, imax = ringVerts.Length; i < imax; i++)
        {
            for (int j = 0, jmax = ringVerts[i].Length; j < jmax; j++)
            {
                int k = i * (this.edgeCount + 1) + j;

                if (i > 0)
                {
                    magnitudes[j] += (ringVerts[i][j] - ringVerts[i - 1][j]).magnitude;
                }

                uv[k].x = j;
                uv[k].y = magnitudes[j];
            }
        }

        var mesh = new Mesh();
        mesh.name = this.bezier.name;
        mesh.vertices = ringVerts.SelectMany(x => x).ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
}
