using UnityEngine;

[CreateAssetMenu(menuName="GuitarDex/Item",fileName="NewGuitarDexItem")]
public class GuitarDexItem : ScriptableObject
{
    public int id;
    public string itemName;
    public enum ItemType { Guitar, Pedal, Amp }
    public ItemType type;
    public enum Rarity { Common, Rare, Epic, Legendary }
    public Rarity rarity;
    public bool unlocked;
    [TextArea(2,4)] public string flavorText;
    public string unlockCondition;
    public Sprite graphic;
    public Color cardBgColor = Color.white;
}
