using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellListItem : MonoBehaviour {

    private SelectSpellOverlayViewController parent;

    public Image spellImage;
    public Text spellName;
    public Text spellDescription;

    private string spellId;

    public void SetParent(SelectSpellOverlayViewController parent) {
        this.parent = parent;
    }

    public void SetSpell(string spellId) {
        this.spellId = spellId;
        if (spellId != null) {
            Data.SpellData spell = Tools.DataManager.SPELL_DATA[spellId];
            spellImage.sprite = Resources.Load<Sprite>(spell.iconPath);
            spellName.text = spell.name;
            spellDescription.text = spell.description[0] + "\n" + spell.description[1];
        }
    }

    public void SelectSpell() {
        parent.SelectSpell(spellId);
    }
}
