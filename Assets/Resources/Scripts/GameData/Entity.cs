using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Entity : MonoBehaviour {

    public static int NO_DESTINATION_CELL_ID = -1;

    public Grid grid;
    public GameLogic game;

    public int entityId;
    public string displayedName = "NONE";
    public Color modelColor = Color.white;
    public int destinationCellId = NO_DESTINATION_CELL_ID;
    public int currentCellId = NO_DESTINATION_CELL_ID;
    public int currentHealth, maxHealth;
    public int currentAP, maxAP;
    public int currentMP, maxMP;

    // buffs
    public int rangeModifier;
    public int damageModifier;
    public int resistanceModifier;
    public int stunCount; // mécanique à implémenter?
    public bool isStunt { get { return stunCount > 0; } }

    public void stun() {
        stunCount++;
    }
    public void unstun() {
        stunCount = stunCount > 1 ? stunCount - 1 : 0 ;
    }


    public Queue<Action> actions = new Queue<Action>();
    public SpellInstance[] spells;
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
                Cell destCell = GameLogicClient.game.grid.GetCell(destinationCellId);
                Vector3 pos = gameObject.transform.position;
                Vector3 dest = new Vector3(destCell.x, 0, destCell.y);
                Vector3 dir = dest - pos;
                if (transform != null) {
                    transform.rotation = Quaternion.LookRotation(dir.normalized, new Vector3(0, 1, 0));
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
        currentCellId = c.cellId;
    }

    public void orderMoveToCell(int destinationCellId) {
        this.destinationCellId = destinationCellId;
    }

    public int getCurrentCell() {
        if (destinationCellId != NO_DESTINATION_CELL_ID)
            return destinationCellId;
        return currentCellId;
    }

    public void addOrder(Action o) {
        actions.Enqueue(o);
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
    public void heal(int amount) {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }
    public bool damage(int amount) { // returns true if the unit dies because of the damage
        currentHealth -= amount;
        return currentHealth <= 0;
    }
    public void modAP(int AP) {
        currentAP += AP;
        if (currentAP > maxAP)
            currentAP = maxAP;
        if (currentAP < 0)
            currentAP = 0;
    }
    public void modMP(int MP) {
        currentMP += MP;
        if (currentMP > maxMP)
            currentMP = maxMP;
        if (currentMP < 0)
            currentMP = 0;
    }
    public void modRange(int range) {
        rangeModifier += range;
    }
    public void modDamage(int damage) {
        damageModifier += damage;
    }
    public void modResistance(int resistance) {
        resistanceModifier += resistance;
    }
}
