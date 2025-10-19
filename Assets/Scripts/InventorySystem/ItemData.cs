using UnityEngine;
using System.Collections.Generic;

// Bu satır sayesinde Unity editöründe sağ tıklayıp Create > Envanter > Item oluşturabileceğiz.
[CreateAssetMenu(fileName = "Yeni Item", menuName = "Envanter/Item")]
public class ItemData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    public string itemName;
    [TextArea(3, 10)] // Açıklama alanını daha büyük yapar
    public string description;
    public Sprite icon; // Bu, senin "1. kategori sabit görsel" dediğin kısım.

    [Header("Etiketler ve Özellikler")]
    public List<ItemTag> tags; // Item'ın sahip olduğu etiketler

    [Header("Bonuslar ve Gereksinimler")]
    public List<StatBonus> bonuses; // Verdiği bonus statlar
    public List<Requirement> requirements; // Kullanım için gereksinimler

    // Buraya item'ın kendine özgü başka özellikleri de eklenebilir.
    // Örneğin, bir silahsa hasar değeri, bir iksir ise iyileştirme miktarı vb.
}