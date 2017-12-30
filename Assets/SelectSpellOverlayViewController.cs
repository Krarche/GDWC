using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectSpellOverlayViewController : MonoBehaviour {

    private static GameObject spellListItemPrefab;

    public LoadoutViewController loadoutViewController;
    public RectTransform scrollViewContent;

    private int spellNumber;
    private string spellId;

    void Start() {
        if (spellListItemPrefab == null)
            spellListItemPrefab = Resources.Load<GameObject>("Prefabs/UI/SpellListItem");
        BuildSpellList();
    }

    public void DisplaySpellSelection(int spellNumber, string spellId) {
        this.spellNumber = spellNumber;
        this.spellId = spellId;
        gameObject.SetActive(true);
    }

    private void BuildSpellList() {
        Debug.Log("BuildSpellList");
        foreach (string spellId in Tools.DataManager.SPELL_DATA.Keys) {
            AddSpellToList(spellId);
        }
    }

    public void ConfirmSelection() {
        loadoutViewController.SetSpell(spellNumber, spellId);
        HideSpellSelection();
    }

    public void CancelSelection() {
        HideSpellSelection();
    }

    private void HideSpellSelection() {
        gameObject.SetActive(false);
    }

    public void SelectSpell(string spellId) {
        this.spellId = spellId;
    }

    public void AddSpellToList(string spellId) {
        Debug.Log("AddSpellToList " + spellId);
        SpellListItem spellListItem = CreateSpellListItem();
        spellListItem.SetParent(this);
        spellListItem.SetSpell(spellId);
        scrollViewContent.sizeDelta = scrollViewContent.sizeDelta + new Vector2(0, 250);
    }

    private SpellListItem CreateSpellListItem() {
        GameObject obj = GameObject.Instantiate(spellListItemPrefab);
        obj.transform.SetParent(scrollViewContent);
        return obj.GetComponent<SpellListItem>();
    }


}
