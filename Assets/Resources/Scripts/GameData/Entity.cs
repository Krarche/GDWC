using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Entity : MonoBehaviour {


    public string displayedName = "NONE";
    public Color modelColor = Color.white;
    public int destinationCellId = NO_DESTINATION_CELL_ID;
    public int currentCellId = NO_DESTINATION_CELL_ID;
    public static int NO_DESTINATION_CELL_ID = -1;

    public int entityId;
    public Queue<Order> orders = new Queue<Order>();
    public Animator animator;
    public TextMesh entityNameText;
    public Transform meshTransform;


    // Use this for initialization
    void Start() {
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        if (GameLogicClient.game != null) {
            //if (meshTransform == null) {
            //    meshTransform = gameObject.transform.GetChild(1).transform;
            //}
            if (destinationCellId != NO_DESTINATION_CELL_ID) {
                Cell destCell = GameLogicClient.game.map.GetCell(destinationCellId);
                Vector3 pos = gameObject.transform.position;
                Vector3 dest = new Vector3(destCell.x, 0, destCell.y);
                Vector3 dir = dest - pos;
                if (transform != null) {
                    transform.rotation = Quaternion.LookRotation(dir.normalized, new Vector3(0,1,0));
                }
                if (dir.magnitude > 0.1f) {
                    gameObject.transform.position = pos + dir / 2;
                } else {
                    currentCellId = destinationCellId;
                    destinationCellId = NO_DESTINATION_CELL_ID;
                    gameObject.transform.position = dest;
                }
            }
            if (entityNameText != null) {
                entityNameText.transform.rotation = Camera.main.transform.rotation;
            }
        }
    }

    public void setCurrentCell(Cell c) {
        transform.position = c.position;
        currentCellId = c.id;
    }

    public void orderMoveToCell(int destinationCellId) {
        this.destinationCellId = destinationCellId;
    }

    public int getCurrentCell() {
        if (destinationCellId != NO_DESTINATION_CELL_ID)
            return destinationCellId;
        return currentCellId;
    }

    public void addOrder(Order o) {
        orders.Enqueue(o);
    }

    public void setColor(float r, float g, float b) {
        setColor(new Color(r, g, b));
    }

    public void setColor(Color c) {
        modelColor = c;
    }

    public void setDisplayedName(string name) {
        displayedName = name;
        entityNameText = gameObject.GetComponentInChildren<TextMesh>();
        if (entityNameText != null) {
            entityNameText.text = name;
        }
    }

    public void applyColor() {
        gameObject.GetComponentInChildren<SkinnedMeshRenderer>().material.color = modelColor;
    }
}
