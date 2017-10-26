using MarkLight.Views.UI;
using UnityEngine;

public class SpellDescription : UIView {
    public string spellId {
        set {
            SPELL_ID = value;
            Data.SpellData spellData = Tools.DataManager.SPELL_DATA[SPELL_ID];
            spellDescriptionFast = spellData.description[0];
            spellDescriptionSlow = spellData.description[1];
        }
        get { return SPELL_ID; }
    }
    private string SPELL_ID;
    public string spellDescriptionFast;
    public string spellDescriptionSlow;
}
