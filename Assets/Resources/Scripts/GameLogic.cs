using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameLogic {

    public static int MAX_APT = 3; // max actions per turn

    public ulong gameId;

    public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();
    public GameObject playerPrefab; // temporary

    public Dictionary<int, Entity> entityList = new Dictionary<int, Entity>();
    public int lastEntityIdGenerated = -1;

    public Grid grid;
    
    public GameLogic() {
        grid = new Grid();
        grid.game = this;
        grid.initialisation(15, 15);
        playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
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

    public abstract void registerAction();

    public virtual void resolveAction(Order o) {
        Entity e = entityList[o.entityId];
        if(o is MovementOrder) {
            MovementOrder mo = (MovementOrder)o;
            e.orderMoveToCell(mo.cellId);
        }
    }

    public virtual void resolveTurn() {
        Queue<Order> fast = new Queue<Order>();
        Queue<Order> move = new Queue<Order>();
        Queue<Order> slow = new Queue<Order>();

        for (int i = 0; i < MAX_APT ; i++) {
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

}
