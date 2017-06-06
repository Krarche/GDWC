using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogicClient : GameLogic {

    public static ulong localPlayerId = 0;
    public static GameLogicClient game;

    public GameLogicClient(): base() {
        game = this;
    }

    public const short BUTTON_TYPE_SPELL_0 = 0;
    public const short BUTTON_TYPE_SPELL_1 = 1;
    public const short BUTTON_TYPE_SPELL_2 = 2;
    public const short BUTTON_TYPE_SPELL_3 = 3;
    public const short BUTTON_TYPE_CONFIRM = 4;
    public const short BUTTON_TYPE_CANCEL = 5;

    public void buttonInput(short type) {
        switch (type) {
            case BUTTON_TYPE_SPELL_0:
                buttonSpell(type);
                break;
            case BUTTON_TYPE_SPELL_1:
                buttonSpell(type);
                break;
            case BUTTON_TYPE_SPELL_2:
                buttonSpell(type);
                break;
            case BUTTON_TYPE_SPELL_3:
                buttonSpell(type);
                break;
            case BUTTON_TYPE_CONFIRM:
                buttonConfirm();
                break;
            case BUTTON_TYPE_CANCEL:
                buttonCancel();
                break;
        }
    }

    public bool containsPlayerId(ulong playerId) {
        foreach(ulong key in playerList.Keys) {
            if (key == playerId)
                return true;
        }
        return false;
    }

    private void buttonSpell(short spellIndex) {

    }

    private void buttonCancel() {

    }

    private void buttonConfirm() {

    }
}
