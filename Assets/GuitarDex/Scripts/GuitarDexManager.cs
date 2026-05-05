using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GuitarDexManager : MonoBehaviour
{
    [Header("Data")]
    public List<GuitarDexItem> items = new List<GuitarDexItem>();

    [Header("Grid")]
    public Transform gridContent;

    [Header("Detail Panel")]
    public Image rarityColorBar;
    public Image detailGraphicImage;
    public TextMeshProUGUI itemIDLabel;
    public TextMeshProUGUI itemNameLabel;
    public TextMeshProUGUI flavorTextLabel;
    public TextMeshProUGUI unlockIconLabel;
    public TextMeshProUGUI unlockLabel;
    public TextMeshProUGUI unlockConditionLabel;

    [Header("Header")]
    public RectTransform progressBarFill;
    public TextMeshProUGUI progressLabel;
    public TextMeshProUGUI coinAmountLabel;

    static Color Sage  =new Color(181/255f,207/255f,174/255f);
    static Color Aqua  =new Color(115/255f,193/255f,206/255f);
    static Color Orange=new Color(249/255f,117/255f, 75/255f);
    static Color Cream =new Color(254/255f,226/255f,176/255f);
    static Color Ink   =new Color( 10/255f, 29/255f, 39/255f);
    static Color Teal  =new Color( 17/255f, 78/255f, 85/255f);
    static Color CardBg      =new Color(223/255f,223/255f,223/255f); // ~7% darken on (240,240,240)
    static Color CardBgLocked=new Color(232/255f,232/255f,232/255f); // ~3% darken when locked

    GuitarDexItem selected;
    List<CardUI> cards=new List<CardUI>();

    void Start()
    {
        cards.Clear();
        if(gridContent!=null)
        {
            for(int i=0;i<gridContent.childCount&&i<items.Count;i++)
            {
                var cardGO=gridContent.GetChild(i).gameObject;
                var item=items[i];
                var cardUI=cardGO.GetComponent<CardUI>()??cardGO.AddComponent<CardUI>();
                cardUI.item=item;
                var cardBg=cardGO.GetComponent<UnityEngine.UI.Image>();
                if(cardBg!=null) cardBg.color=item.unlocked?CardBg:CardBgLocked;
                var graphic=cardGO.transform.Find("GraphicArea/InstrumentImage")?.GetComponent<UnityEngine.UI.Image>();
                if(graphic!=null){ graphic.color=item.unlocked?Color.white:new Color(0.55f,0.55f,0.55f,1f); graphic.preserveAspect=true; }
                var btn=cardGO.GetComponent<UnityEngine.UI.Button>();
                if(btn!=null){ var idx=i; btn.onClick.AddListener(()=>Select(items[idx])); }
                cards.Add(cardUI);
            }
        }
        RefreshProgress();
        if(items.Count>0) Select(items[0]);
    }

    public void Select(GuitarDexItem item)
    {
        selected=item;
        foreach(var c in cards) c.SetSelected(c.item==item);
        UpdateDetail(item);
    }

    void RefreshProgress()
    {
        int u=0; foreach(var i in items) if(i.unlocked) u++;
        if(progressLabel) progressLabel.text=$"{u} / {items.Count} COLLECTED";
        if(progressBarFill) progressBarFill.anchorMax=new Vector2(items.Count>0?(float)u/items.Count:0,1);
    }

    Color RarityColor(GuitarDexItem.Rarity r){
        switch(r){
            case GuitarDexItem.Rarity.Common:    return Sage;
            case GuitarDexItem.Rarity.Rare:      return Aqua;
            case GuitarDexItem.Rarity.Epic:      return Orange;
            case GuitarDexItem.Rarity.Legendary: return Cream;
            default: return Sage;
        }
    }

    void UpdateDetail(GuitarDexItem item)
    {
        bool lk=!item.unlocked;
        Color rc=RarityColor(item.rarity);
        if(rarityColorBar)    rarityColorBar.color=rc;
        if(detailGraphicImage){
            detailGraphicImage.sprite=item.graphic;
            detailGraphicImage.color=lk?new Color(0.55f,0.55f,0.55f,1f):Color.white;
            detailGraphicImage.preserveAspect=true;
        }
        if(itemIDLabel)       itemIDLabel.text=$"#{item.id:D3} · {item.type.ToString().ToUpper()}";
        if(itemNameLabel){    itemNameLabel.text=lk?"???":item.itemName; itemNameLabel.color=lk?new Color(10/255f,29/255f,39/255f,0.25f):Ink;}
        if(flavorTextLabel){  flavorTextLabel.text=lk?"Keep playing to unlock.":((char)34)+item.flavorText+((char)34); flavorTextLabel.color=lk?new Color(10/255f,29/255f,39/255f,0.2f):new Color(10/255f,29/255f,39/255f,0.65f);}
        if(unlockIconLabel)   unlockIconLabel.text=lk?"":">";
        if(unlockLabel)       unlockLabel.text=lk?"UNLOCK CONDITION":"UNLOCKED BY";
        if(unlockConditionLabel){ unlockConditionLabel.text=item.unlockCondition; unlockConditionLabel.color=lk?Orange:new Color(10/255f,29/255f,39/255f,0.8f);}
    }
}

public class CardUI : MonoBehaviour
{
    public GuitarDexItem item;
    [HideInInspector] public Image borderImage;
    public void SetSelected(bool sel)
    {
        transform.localScale=sel?Vector3.one*1.04f:Vector3.one;
    }
}
