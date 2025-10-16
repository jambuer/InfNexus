using UnityEngine;

/// <summary>
/// Oyundaki tek bir eşyanın tüm verilerini içeren ScriptableObject.
/// Şimdilik bir yer tutucu olarak görev yapar.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory System/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string itemName = "New Item";
    public Sprite itemIcon;
    [TextArea]
    public string itemDescription = "Item description here.";

    // İleride buraya eşyanın türü, değeri, giyilebilir olup olmadığı,
    // verdiği stat bonusları gibi birçok özellik eklenecek.
}