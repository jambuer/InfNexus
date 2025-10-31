using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;



/// <summary>
/// GameValidator'da kontrol edilen gereksinim listelerini (Requirement)
/// fiilen harcamak (tüketmek) için kullanılan merkezi bir Singleton sınıfı.
/// Örn: Bir binanın kilidini açmak veya bir perk satın almak için gereken maliyetleri öder.
/// </summary>
public class GameCostConsumer : Singleton<GameCostConsumer>
{
    /// <summary>
    /// Verilen gereksinim listesini harcar (eğer karşılanıyorsa).
    /// Önce GameValidator ile TÜM gereksinimlerin karşılandığını doğrular.
    /// </summary>
    /// <param name="requirements">Harcanacak gereksinimlerin listesi</param>
    /// <param name="logSuccess">Başarılı harcama logu atılsın mı?</param>
    /// <returns>Tüm harcamalar başarılıysa true döner</returns>
    public bool ConsumeRequirements(List<Requirement> requirements, bool logSuccess = true)
    {
        if (requirements == null || requirements.Count == 0)
        {
            // Harcanacak bir şey yoksa, başarılı kabul edilir.
            return true;
        }

        // 1. Önce Validator'a TÜM gereksinimlerin karşılanıp karşılanmadığını sor.
        // Bu, 100 Altın ve 5 Odun gereken bir işlemde, 100 Altını alıp
        // "Odun yok" diyerek işlemi yarıda bırakmasını engeller.
        if (!GameValidator.Instance.AreRequirementsMet(requirements))
        {
            Debug.LogError("GameCostConsumer: Harcama yapılamadı! Gereksinimler karşılanmıyor.");
            return false;
        }

        // 2. Gereksinimler tamsa, şimdi harcama işlemini yap.
        // Validator zaten null check yaptığı için burada tekrar yapmaya gerek yok.
        // (Ancak daha güvenli kod için eklenebilir)
        foreach (var req in requirements)
        {
            // Not: 'Stat', 'Level', 'Quest' ve 'Perk' gereksinimleri bir 'maliyet' değildir,
            // onlar bir 'durum' kontrolüdür. Bu yüzden switch-case'de sadece harcanabilir
            // kalemleri (Item, Gold, People, Kaynaklar) işliyoruz.
            switch (req.reqType)
            {
                case RequirementType.Item:
                    ItemData itemData = ItemManager.Instance.GetItemByName(req.requirementName);
                    if (itemData != null)
                    {
                        Inventory.Instance.RemoveItem(itemData, req.requiredValue); // requiredValue (int)
                    }
                    else
                    {
                        Debug.LogWarning($"GameCostConsumer: '{req.requirementName}' isminde eşya harcanamadı (ItemManager'da bulunamadı).");
                    }
                    break;
                case RequirementType.Gold:
                    CurrencyManager.Instance.SpendGold(req.requiredValue); // requiredValue (double)
                    break;
                case RequirementType.NexusCoin:
                    CurrencyManager.Instance.SpendNexusCoin(req.requiredValue); // requiredValue (double)
                    break;
                case RequirementType.People:
                    // 'People' için SpendPeople kullanarak kişi sayısını azalt
                    CurrencyManager.Instance.SpendPeople(req.requiredValue); // requiredValue (double)
                    break;

                // Kaynakları harcama (Nadiren kullanılır ama mümkün)
                case RequirementType.Health:
                    ResourceManager.Instance.ModifyHealth(-(float)req.requiredValue);
                    break;
                case RequirementType.Energy:
                    ResourceManager.Instance.ModifyEnergy(-(float)req.requiredValue);
                    break;
                case RequirementType.Mana:
                    ResourceManager.Instance.ModifyMana(-(float)req.requiredValue);
                    break;

                // Bu tipler 'maliyet' değil, 'kilit' olduğu için atlanır:
                // case RequirementType.Level:
                // case RequirementType.Stat:
                // case RequirementType.Quest:
                // case RequirementType.Perk:
                //     break;
            }
        }

        if (logSuccess)
        {
            Debug.Log("GameCostConsumer: Gereksinimler başarıyla harcandı.");
        }
        return true;
    }
}