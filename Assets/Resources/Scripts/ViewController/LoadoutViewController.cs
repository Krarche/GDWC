using System.Collections;
using System.Collections.Generic;
using Tools.JSON;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutViewController : MonoBehaviour {

    public LobbyViewController lobbyViewController;
    public SelectSpellOverlayViewController spellOverlayViewController;
    private string[] spellIds = new string[4];

    public Image[] spellImages;

    public void SetSpell(int spellNumber, string spellId) {
        if (spellIds[spellNumber] != spellId) {
            if (spellId != null) {
                // swap value if already used
                string currentSpellId = spellIds[spellNumber];
                spellIds[spellNumber] = null;
                for (int i = 0; i < spellIds.Length; i++) {
                    if (spellIds[i] == spellId) {
                        SetSpell(i, currentSpellId);
                    }
                }
                // set value
                spellIds[spellNumber] = spellId;
                // set spell icon
                spellImages[spellNumber].sprite = Resources.Load<Sprite>(Tools.DataManager.SPELL_DATA[spellId].iconPath);
            } else {
                // set value
                spellIds[spellNumber] = null;
                // set mystery icon
                spellImages[spellNumber].sprite = Resources.Load<Sprite>("Sprites/Spells/none");
            }
        }
    }

    public void ChangeSpell(int spellNumber) {
        spellOverlayViewController.DisplaySpellSelection(spellNumber, spellIds[spellNumber]);
    }

    private string errorMessage = "";

    public string GetErrorMessage() {
        return errorMessage;
    }

    public bool isLoadoutValid() {
        // check if spell are selected
        for (int i = 0; i < spellIds.Length; i++) {
            if (spellIds[i] == null) {
                errorMessage = "You must select all you spells before entering queue!";
                return false;
            }
        }

        // all is good
        errorMessage = "";
        return true;
    }


    public void SetLoadoutJSON(string json) {
        ObjectJSON obj = new ObjectJSON(json);
        ArrayJSON array = obj.getArrayJSON("spells");
        for (int i = 0; i < array.Length; i++) {
            SetSpell(i, array.getStringAt(i));
        }
    }

    public string GetLoadoutJSON() {
        string output = "";
        output += "\"spells\":[";
        for (int i = 0; i < spellIds.Length; i++) {
            output += "\"" + spellIds[i] + "\"";
        }
        output += "]";
        return "{" + output + "}";
    }

}
