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
    public int teamId;
    public Cell destinationCell;
    public int destinationCellId {
        get {
            return destinationCell != null ? destinationCell.cellId : NO_DESTINATION_CELL_ID;
        }
    }
    public Cell currentCell;
    public int currentCellId {
        get {
            return currentCell != null ? currentCell.cellId : NO_DESTINATION_CELL_ID;
        }
    }

    public event System.EventHandler<ChangeEntityHealthEventArgs> ChangeEntityHealth;
    public event System.EventHandler<ChangeEntityActionPointsEventArgs> ChangeEntityActionPoints;
    public event System.EventHandler<ChangeEntityMovePointsEventArgs> ChangeEntityMovePoints;
    // stats
    public int maxHealth = 50;
    private int currentHealth;
    public int CurrentHealth {
        get { return currentHealth; }
        set { 
            currentHealth = value;
            //TEMP
            if (ChangeEntityHealth != null)
                ChangeEntityHealth(this, new ChangeEntityHealthEventArgs { health = currentHealth }); 
        } 
    }
    public int maxAP = 14;
    private int currentAP;
    public int CurrentAP {
        get { return currentAP; }
        set {
            currentAP = value;
            //TEMP
            if (ChangeEntityHealth != null)
                ChangeEntityActionPoints(this, new ChangeEntityActionPointsEventArgs { actionPoints = currentAP });
        }
    }
    public int maxMP = 8;
    private int currentMP;
    public int CurrentMP {
        get { return currentMP; }
        set {
            currentMP = value;
            //TEMP
            if (ChangeEntityHealth != null)
                ChangeEntityMovePoints(this, new ChangeEntityMovePointsEventArgs { movePoints = currentMP });
        }
    }

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
        stunCount = stunCount > 1 ? stunCount - 1 : 0;
    }


    public Queue<Action> actions = new Queue<Action>();
    public SpellInstance[] spells;
    public List<BuffInstance> buffs;
    public Animator animator;
    public TextMesh entityNameText;
    public Transform meshTransform;

    // Use this for initialization
    void Start() {
        animator = gameObject.GetComponent<Animator>();
        CurrentHealth = 50;
        maxHealth = 50;
        CurrentAP = 14;
        maxAP = 14;
        CurrentMP = 8;
        maxMP = 8;
        buffs = new List<BuffInstance>();
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
                    currentCell = destinationCell;
                    destinationCell = null;
                    gameObject.transform.position = dest;
                }
            }
            if (entityNameText != null) {
                entityNameText.transform.rotation = Camera.main.transform.rotation;
            }
        }
    }

    public void initSpell(string[] spellIds) {
        spells = new SpellInstance[spellIds.Length];
        for (int i = 0; i < spellIds.Length; i++) {
            spells[i] = new SpellInstance();
            spells[i].spell = DataManager.SPELL_DATA[spellIds[i]];
        }
    }

    public bool hasBuff(string buffId) {
        foreach (BuffInstance bi in buffs)
            if (bi.buff.id == buffId)
                return true;
        return false;
    }

    // should contain the buff, check first with hasBuff
    public BuffInstance getBuffInstance(string buffId) {
        return buffs.Find((x => x.buff.id == buffId));
    }

    public BuffInstance addBuffInstance(Entity origin, string buffId, int duration) {
        BuffInstance bi = new BuffInstance();
        bi.origin = origin;
        bi.target = this;
        bi.remainingDuration = duration;
        bi.buff = DataManager.BUFF_DATA[buffId];
        buffs.Add(bi);
        return bi;
    }

    // should contain the buff, check first with hasBuff
    public BuffInstance refreshBuffInstance(Entity origin, string buffId, int duration) {
        BuffInstance bi = buffs.Find((x => x.buff.id == buffId));
        bi.origin = origin;
        bi.target = this;
        bi.remainingDuration = duration;
        return bi;
    }

    // should contain the buff, check first with hasBuff
    public BuffInstance removeBuffInstance(string buffId) {
        BuffInstance bi = buffs.Find((x => x.buff.id == buffId));
        buffs.Remove(bi);
        return bi;
    }

    public void setCurrentCell(Cell c) {
        if (c != null) {
            if (currentCell != null)
                currentCell.currentEntity = null;
            transform.position = c.position;
            currentCell = c;
            c.currentEntity = this;
        }
    }

    public void orderMoveToCell(Cell destinationCell) {
        this.destinationCell = destinationCell;
    }

    public int getCurrentCellId() {
        if (destinationCellId != NO_DESTINATION_CELL_ID)
            return destinationCellId;
        return currentCellId;
    }

    public Cell getCurrentCell() {
        if (destinationCell != null)
            return destinationCell;
        return currentCell;
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
        CurrentHealth += amount;
        if (CurrentHealth > maxHealth)
            CurrentHealth = maxHealth;
    }
    public bool damage(int amount) { // returns true if the unit dies because of the damage
        CurrentHealth -= amount;
        return CurrentHealth <= 0;
    }
    public void modAP(int AP) {
        CurrentAP += AP;
        if (CurrentAP > maxAP)
            CurrentAP = maxAP;
        if (CurrentAP < 0)
            CurrentAP = 0;
    }
    public void modMP(int MP) {
        CurrentMP += MP;
        if (CurrentMP > maxMP)
            CurrentMP = maxMP;
        if (CurrentMP < 0)
            CurrentMP = 0;
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
