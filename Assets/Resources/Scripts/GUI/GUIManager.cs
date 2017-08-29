using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {

    public static GUIManager gui;

    //Texts sync with Player values
    public Text healthText;
    public Text actionPointsText;
    public Text movePointsText;

    void Start()
    {
        gui = this;
    }

    public void linkWithLocalEntity(Entity localEntity) {
        localEntity.ChangeEntityHealth += player_changeHealth;
        localEntity.ChangeEntityActionPoints += player_changeActionPoints;
        localEntity.ChangeEntityMovePoints += player_changeMovePoints;
    }

    private void player_changeHealth(object sender, ChangeEntityHealthEventArgs e) {
        healthText.text = e.health.ToString();
    }

    private void player_changeActionPoints(object sender, ChangeEntityActionPointsEventArgs e) {
        actionPointsText.text = e.actionPoints.ToString();
    }

    private void player_changeMovePoints(object sender, ChangeEntityMovePointsEventArgs e) {
        movePointsText.text = e.movePoints.ToString();
    }
}

public class ChangeEntityHealthEventArgs : System.EventArgs {
    public int health { get; set; }
}

public class ChangeEntityActionPointsEventArgs : System.EventArgs {
    public int actionPoints { get; set; }
}

public class ChangeEntityMovePointsEventArgs : System.EventArgs {
    public int movePoints { get; set; }
}


