using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameLogic {

    public static int MAX_APT = 3; // max actions per turn

    public ulong id { set; get; }

    public Dictionary<ulong, Player> playerList = new Dictionary<ulong, Player>();
    public Dictionary<int, Entity> entityList = new Dictionary<int, Entity>();
    public int lastEntityIdGenerated = -1;
    public Grid map;
    public GameObject playerPrefab;

    public GameLogic() {
        map = new Grid();
        map.initialisation(15, 15);
        playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
    }

    protected void OnDrawGizmos() {
        if (map != null) {
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

    public Entity createEntity(int entityId) {
        Entity temp = GameObject.Instantiate(playerPrefab, new Vector3(), Quaternion.identity).GetComponent<Entity>();
        temp.entityId = entityId;
        temp.setCurrentCell(map.GetCell(0));
        entityList[entityId] = temp;
        return temp;
    }

    public void removeEntity(Entity e) {
        GameObject.Destroy(e.gameObject);
    }

    public void addPlayer(Player p) {
        if (!playerList.ContainsKey(p.playerId)) {
            playerList[p.playerId] = p;
        }
    }

    public void removePlayer(Player p) {
        removeEntity(p.entity);
        playerList.Remove(p.playerId);
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
            foreach (Player p in playerList.Values) {
                if (p.entity.orders.Count > 0) {
                    Order o = p.entity.orders.Dequeue();
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
