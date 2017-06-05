using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic {

    public ulong id { set; get; }

    public List<Player> playerList = new List<Player>();

    public Grid map;
    public GameObject playerPrefab;
    public ClientSendSpellMessage currentSpell;

    public GameLogic () {
        map = new Grid();
        map.initialisation(15, 15);
        playerPrefab = Resources.Load<GameObject>("Prefabs/PlayerPrefab");
    }

    protected void OnDrawGizmos() {
		if (map != null) {
			Gizmos.color = Color.green;
			Vector3 pos = new Vector3 ();
			for (int i = 0; i < map.sizeX; i++) {
				for (int j = 0; j < map.sizeY; j++) {
					pos.x = i;
					pos.z = j;
					Gizmos.DrawSphere (pos, 0.1f);
				}
			}
		}
    }

    public Entity createEntity(int cellPositionId) {
        Cell cell = map.GetCell(cellPositionId);
        GameObject obj = GameObject.Instantiate(playerPrefab, new Vector3(), Quaternion.identity);
        obj.GetComponent<Entity>().setCurrentCell(cell);
        return obj.GetComponent<Entity>();
    }

    public void removeEntity(Entity e) {
        GameObject.Destroy(e.gameObject);
    }

    public void addPlayer(Player p) {
        playerList.Add(p);
    }

    public void removePlayer(Player p) {
        removeEntity(p.entity);
        playerList.Remove(p);
    }

    public void registerAction() {

    }

    public void resolveAction() {

    }

    public void resolveTurn() {

    }
}
