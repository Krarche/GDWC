using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type : int {
    NORMAL, TREE, ROCK, GAP, COUNT
}

public class CellType : GameData {
    public string name;
    public string modelPath;
    public Mesh modelMesh;
    public Material material;

    public CellType() {
        modelMesh = (Mesh)Resources.Load("Meshes/smoothCube", typeof(Mesh));
    }

    public CellType(Type t) {
        switch (t) {
            case Type.NORMAL:
                modelMesh = (Mesh)Resources.Load("Meshes/smoothCube", typeof(Mesh));
                break;
            case Type.TREE:
                break;
            case Type.ROCK:
                break;
            case Type.GAP:
                modelMesh = null;
                break;
            case Type.COUNT:
                break;
        }
    }
}