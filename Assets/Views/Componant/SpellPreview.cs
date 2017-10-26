using MarkLight.Views.UI;
using MarkLight;
using UnityEngine;

public class SpellPreview : MobileView {
    public string spellId {
        set {
            SPELL_ID = value;
            Data.SpellData spellData = Tools.DataManager.SPELL_DATA[SPELL_ID];
            spellName = spellData.name;
            iconPath = spellData.iconPath;
        }
        get { return SPELL_ID; }
    }
    private string SPELL_ID;
    public string spellName;
    public string iconPath;
}
