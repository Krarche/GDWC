using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

    public Grid map;
    public static Material mat;




	// Use this for initialization
	void Start () {
        map = new Grid();
        map.initialisation(6, 4);
        map.printGrid();
        mat = (Material)Resources.Load("Materials/defaultMat", typeof(Material));
        GenerateMesh();
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    void GenerateMesh() {
        int vertexCount = 0;
        int triangleCount = 0;
        int UVCount = 0;
        for (int x=0; x < map.sizeX; x++) {
            for (int y = 0; y < map.sizeY; y++) {
                Cell c = map.cells[x, y];
                Mesh m = c.model;
                vertexCount += m.vertices.Length;
                triangleCount += m.triangles.Length;
                UVCount += m.uv.Length;
            }
        }
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[triangleCount];
        Vector2[] uv = new Vector2[UVCount];
        vertexCount = 0;
        triangleCount = 0;
        UVCount = 0;
        for (int x = 0; x < map.sizeX; x++) {
            for (int y = 0; y < map.sizeY; y++) {
                Cell c = map.cells[x, y];
                Mesh m = c.model;
                for (int i = 0; i < m.vertices.Length; i++) {
                    vertices[vertexCount + i] = m.vertices[i];
                    vertices[vertexCount + i].x += x;
                    vertices[vertexCount + i].z += y;
                }
                for (int i = 0; i < m.triangles.Length; i++) {
                    triangles[triangleCount + i] = m.triangles[i] + vertexCount;
                }
                for (int i = 0; i < m.uv.Length; i++) {
                    uv[UVCount + i] = m.uv[i];
                }
                triangleCount += m.triangles.Length;
                vertexCount += m.vertices.Length;
                UVCount += m.uv.Length;
            }
        }
        Mesh mesh;
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Generated Grid";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
    }

    /*private void OnRenderObject() {
        Vector3 pos = new Vector3();
        Quaternion angle = Quaternion.identity;
        for (int i = 0; i < map.sizeX; i++) {
            for (int j = 0; j < map.sizeY; j++) {
                pos.x = i;
                pos.z = j;
                Graphics.DrawMeshNow(map.cells[i, j].model, pos, angle, 0);
            }
        }
    }*/

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Vector3 pos = new Vector3();
        for (int i = 0; i < map.sizeX; i++) {
            for (int j = 0; j < map.sizeY; j++) {
                pos.x = i;
                pos.z = j;
                Gizmos.DrawSphere(pos, 0.1f);
            }
        }
    }
}
