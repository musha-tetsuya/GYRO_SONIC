using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PipeMeshEditor : MonoBehaviour
{
    [SerializeField]
    private PathCreation.PathCreator pathCreator = null;

    [SerializeField]
    private float radius = 1f;

    [SerializeField]
    private int sidesCount = 16;

    [SerializeField]
    private float scale = 1f;

    [SerializeField]
    private List<Ring> rings = new List<Ring>();

    [SerializeField]
    private MeshFilter meshFilter = null;

    [SerializeField]
    private float ringDistance = 1f;

    [SerializeField]
    private Transform cameraPosition = null;

    [SerializeField]
    private Transform cameraAngle = null;

    [SerializeField]
    private float spd = 1f;

    [SerializeField]
    private float angleSpd = 1f;

    [SerializeField]
    private GameObject obstaclePrefab = null;

    private float time = 0f;

    private void Start()
    {
        this.cameraPosition.localPosition = this.pathCreator.path.GetPointAtTime(0f) * scale;
        this.cameraPosition.forward = this.pathCreator.path.GetDirection(0f);

        for (int i = 5; i < 95; i++)
        {
            if (UnityEngine.Random.Range(0,4) == 0)
            {
                var o = Instantiate(this.obstaclePrefab);
                var p = this.pathCreator.path.GetPointAtTime(i * 0.01f) * scale;
                var v = this.pathCreator.path.GetDirection(i * 0.01f);
                o.transform.localPosition = p;
                o.transform.forward = v;

                var a = o.transform.localEulerAngles;
                a.z += UnityEngine.Random.Range(0, 360f);
                o.transform.localEulerAngles = a; 
            }
        }
    }

    private void Update()
    {
        this.cameraPosition.localPosition = this.pathCreator.path.GetPointAtTime(this.time, PathCreation.EndOfPathInstruction.Stop) * scale;
        this.cameraPosition.forward = this.pathCreator.path.GetDirection(this.time, PathCreation.EndOfPathInstruction.Stop);
        this.time += Time.deltaTime * 0.01f * this.spd;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            var angle = this.cameraAngle.localEulerAngles;
            angle.z -= this.angleSpd;
            this.cameraAngle.localEulerAngles = angle;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            var angle = this.cameraAngle.localEulerAngles;
            angle.z += this.angleSpd;
            this.cameraAngle.localEulerAngles = angle;
        }
    }

    private void CreateMesh()
    {
        this.rings.Clear();

        /*int k = 0;
        int kmax = this.pathCreator.path.NumPoints;

        while (k < kmax)
        {
            var center = this.pathCreator.path.GetPoint(k) * scale;

            var forward = (k < kmax - 1)
                        ? this.pathCreator.path.GetPoint(k + 1) * scale - center
                        : center - this.pathCreator.path.GetPoint(k - 1) * scale;
        }*/



        for (int i = 0, imax = this.pathCreator.path.NumPoints; i < imax; i++)
        {
            var center = this.pathCreator.path.GetPoint(i) * scale;

            var forward = (i < imax - 1)
                        ? this.pathCreator.path.GetPoint(i + 1) * scale - center
                        : center - this.pathCreator.path.GetPoint(i - 1) * scale;

            this.rings.Add(new Ring(center, forward, radius, sidesCount));
        }

        var mesh = new Mesh();

        mesh.Clear();

        mesh.name = "pipe-mesh";

        /*
        mesh.vertices = new Vector3[]
        {
            new Vector3(0,0,0),
            new Vector3(0,0,1),
            new Vector3(1,0,1),
            new Vector3(1,0,0),
        };

        mesh.triangles = new int[]
        {
            0,1,2,
            0,2,3
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),
        };
        */

        mesh.vertices = this.rings.SelectMany(x => x.points).ToArray();

        var triangles = new List<int>();

        for (int i = 0, imax = this.rings.Count - 1, offset = 0; i < imax; i++)
        {
            for (int j = 0, jmax = this.rings[i].points.Length; j < jmax; j++)
            {
                int a = offset + j;
                int b = ((a + 1) % jmax) + offset;
                int c = a + jmax;
                int d = b + jmax;

                triangles.AddRange(new int[]
                {
                    a, c, d,
                    a, d, b
                });
            }

            offset += this.rings[i].points.Length;
        }

        mesh.triangles = triangles.ToArray();

        var uvs = new List<Vector2>();
        var magnitudes = new List<float>();

        for (int i = 0, imax = this.rings.Count; i < imax; i++)
        {
            for (int j = 0, jmax = this.rings[i].points.Length; j < jmax; j++)
            {
                float x = j;
                float y = i;

                if (i > 0)
                {
                    magnitudes[j] += (this.rings[i].points[j] - this.rings[i - 1].points[j]).magnitude;
                }
                else
                {
                    magnitudes.Add(0);
                }

                y = magnitudes[j];

                uvs.Add(new Vector2(x, y));
            }
        }

        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();

        mesh.RecalculateNormals();

        this.meshFilter.mesh = mesh;
    }

    private void SaveMesh()
    {
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(this.meshFilter.sharedMesh, "Assets/" + this.meshFilter.sharedMesh.name + ".asset");
#endif
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < this.rings.Count; i++)
        {
            for (int j = 0; j < sidesCount; j++)
            {
                Gizmos.DrawSphere(this.rings[i].points[j], this.radius * 0.1f);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PipeMeshEditor))]
    private class CustomInspector : Editor
    {
        private new PipeMeshEditor target => base.target as PipeMeshEditor;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create Mesh"))
            {
                this.target.CreateMesh();
            }

            if (GUILayout.Button("Save Mesh"))
            {
                this.target.SaveMesh();
            }
        }
    }
#endif

    [Serializable]
    public class Ring
    {
        public Vector3 center = Vector3.zero;

        public Vector3 forward = Vector3.forward;

        public float radius = 1f;

        public int sidesCount = 16;

        public Vector3[] points = null;

        public Ring(Vector3 center, Vector3 forward, float radius, int sidesCount)
        {
            this.center = center;
            this.forward = forward;

            this.points = new Vector3[sidesCount + 1];

            for (int i = 0; i < sidesCount; i++)
            {
                var angle = 360f / sidesCount * i;

                this.points[i] = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up * radius;

                this.points[i] = Quaternion.LookRotation(forward, Vector3.up) * this.points[i];

                this.points[i] += center;
            }

            this.points[sidesCount] = this.points[0];
        }
    }
}
