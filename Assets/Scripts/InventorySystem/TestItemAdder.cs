using UnityEngine;

public class TestItemAdder : MonoBehaviour
{
    [Header("Eklenecek Item")]
    public ItemData itemToAdd; // Buraya editörden "Odun"u sürükle
    public int amountToAdd = 1;

    // Bu fonksiyonu Buton'un OnClick() event'inden çağıracağız.
    public void AddTestItem()
    {
        if (itemToAdd != null)
        {
            Inventory.Instance.AddItem(itemToAdd, amountToAdd);
            Debug.Log(itemToAdd.itemName + " eklendi (Test Butonu)");
        }
        else
        {
            Debug.LogError("TestItemAdder'a eklenecek item atanmamış!");
        }
    }
}