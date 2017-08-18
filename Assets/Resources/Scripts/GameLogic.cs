using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameLogic {

    public static int MAX_TURN_NUMBER = 20;
    public static int MAX_APT = 3; // max actions per turn

    public ulong gameId;
    public int currentTurn;
    public bool allowActionRegistration;


    public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();
    public GameObject playerPrefab; // temporary

    public Dictionary<int, Entity> entityList = new Dictionary<int, Entity>();
    public int lastEntityIdGenerated = -1;

    public Grid grid;
    public TurnResolution solver;

    public GameLogic() {
        playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
        solver = new TurnResolution(this);
        grid = new Grid();
        grid.game = this;
        grid.initialisation(15, 15);
    }

    public Entity createEntity(int entityId) {
        Entity entity = GameObject.Instantiate(playerPrefab, new Vector3(), Quaternion.identity).GetComponent<Entity>();
        entity.entityId = entityId;
        entity.game = this;
        entity.grid = grid;
        entity.setCurrentCell(grid.GetCell(0));
        entityList[entityId] = entity;
        return entity;
    }

    public void removeEntity(Entity e) {
        GameObject.Destroy(e.gameObject);
    }

    public void addPlayer(Player p) {
        if (!players.ContainsKey(p.playerId)) {
            players[p.playerId] = p;
        }
    }

    public void removePlayer(Player p) {
        removeEntity(p.playerEntity);
        entityList.Remove(p.playerEntity.entityId);
        players.Remove(p.playerId);
    }


    public abstract void startNewTurn();
    public void sendActionToServer() {

    }
    public void sendActionToClient() {

    }
    public void registerAction() { // general action registration

    }
    public abstract void registerLocalAction(); // from current client
    public abstract void registerForeignAction(); // from other client
    public virtual void resolveTurn() {

    }

    public virtual void resolveActions() {
        Queue<Order> fast = new Queue<Order>();
        Queue<Order> move = new Queue<Order>();
        Queue<Order> slow = new Queue<Order>();

        for (int i = 0; i < MAX_APT; i++) {
            foreach (Player p in players.Values) {
                if (p.playerEntity.orders.Count > 0) {
                    Order o = p.playerEntity.orders.Dequeue();
                    switch (o.getPriority()) {
                        case 0:
                            fast.Enqueue(o);
                            break;
                        case 1:
                            move.Enqueue(o);
                            break;
                        default:
                            slow.Enqueue(o);
                            break;
                    }
                }
            }
            resolvePriority(fast);
            resolvePriority(move);
            resolvePriority(slow);
        }
    }

    public virtual void resolvePriority(Queue<Order> q) {
        Order o;
        while (q.Count > 0) {
            o = q.Dequeue();
            resolveAction(o);
        }
    }

    public virtual void resolveAction(Order o) {
        Entity e = entityList[o.entityId];
        if (o is MovementOrder) {
            MovementOrder mo = (MovementOrder)o;
            Cell dest = grid.GetCell(mo.cellId);
            solver.resolveMovement(e, dest);
        } else if (o is SpellOrder) {
            SpellOrder so = (SpellOrder) o;
            Cell target = grid.GetCell(so.cellId);
            SpellData spell = DataManager.SPELL_DATA[so.spellId];
            solver.resolveSpell(spell, e, target);
        }
    }

}
