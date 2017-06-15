using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type : int {
    NORMAL, TREE, ROCK, GAP, COUNT
}

public class CellType : GameData {
    public Mesh model;
    public Material material;

    public CellType() {
        model = (Mesh)Resources.Load("Meshes/smoothCube", typeof(Mesh));
    }

    public CellType(Type t) {
        switch (t) {
            case Type.NORMAL:
                model = (Mesh)Resources.Load("Meshes/smoothCube", typeof(Mesh));
                break;
            case Type.TREE:
                break;
            case Type.ROCK:
                break;
            case Type.GAP:
                model = null;
                break;
            case Type.COUNT:
                break;
        }
    }
}