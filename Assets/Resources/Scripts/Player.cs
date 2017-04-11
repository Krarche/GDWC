using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public static List<Player> playerList = new List<Player>();
    public static int localPlayer = -1;

    public int playerId = 0;
    public int destinationCellId = NO_DESTINATION_CELL_ID;
    public int currentCellId = NO_DESTINATION_CELL_ID;
    public static int NO_DESTINATION_CELL_ID = -1;

    // Use this for initialization
    void Start()
    {
        playerId = playerList.Count;
        playerList.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (destinationCellId != NO_DESTINATION_CELL_ID)
        {
            Cell destCell = GameLogic.main.map.GetCell(destinationCellId);
            Vector3 pos = gameObject.transform.position;
            Vector3 dest = new Vector3(destCell.x, 0, destCell.y);
            Vector3 dir = dest - pos;
            if (dir.magnitude > 0.1f)
            {
                gameObject.transform.position = pos + dir / 2;
            }
            else
            {
                currentCellId = destinationCellId;
                destinationCellId = NO_DESTINATION_CELL_ID;
                gameObject.transform.position = dest;
            }
        }
    }

    public void orderMoveToCell(int destinationCellId)
    {
        this.destinationCellId = destinationCellId;
    }

    public int getCurrentCell()
    {
        if (destinationCellId != NO_DESTINATION_CELL_ID)
            return destinationCellId;
        return currentCellId;
    }


}
