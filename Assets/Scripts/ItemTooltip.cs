using UnityEngine;
using UnityEngine.UI;
using System.Text; // Metinleri verimli bir şekilde birleştirmek için

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance;

    public Text itemNameText;
    public Text itemInfoText; // Açıklama, statlar ve gereksinimler buraya gelecek
    
    private RectTransform backgroundRectTransform;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        backgroundRectTransform = GetComponent<RectTransform>();
    }
    
    private void Start()
    {
        // Paneli, Awake() çalıştıktan ve Instance atandıktan SONRA burada gizle.
        // Bu, NullReferenceException hatasını önler.
        gameObject.SetActive(false); 
    }

    // Bilgi panelini farenin yanında göstermek için
    private void Update()
    {
        if (gameObject.activeSelf)
        {
            // Panelin pozisyonunu fare imlecine göre ayarla
            // Canvas ayarına göre (Screen Space - Overlay/Camera) burası değişebilir.
            // En basit haliyle:
            transform.position = Input.mousePosition;
            
            // Ekran kenarlarından taşmayı engellemek için küçük bir ayar eklenebilir
            // (Şimdilik basit tutuyoruz)
        }
    }

    public void ShowTooltip(ItemData item)
    {
        gameObject.SetActive(true);

        itemNameText.text = item.itemName;

        // StringBuilder kullanarak metinleri birleştirmek daha verimlidir
        StringBuilder sb = new StringBuilder();
        
        sb.AppendLine(item.description); // Açıklamayı ekle

        // Etiketleri ekle (Opsiyonel ama güzel olur)
        if (item.tags.Count > 0)
        {
            sb.AppendLine(); // Boş bir satır ekle
            sb.Append("Etiketler: ");
            for (int i = 0; i < item.tags.Count; i++)
            {
                sb.Append(item.tags[i]);
                if (i < item.tags.Count - 1) sb.Append(", ");
            }
            sb.AppendLine();
        }

        // Bonusları ekle
        if (item.bonuses.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Bonuslar:");
            foreach (var bonus in item.bonuses)
            {
                sb.AppendLine($"- {bonus.statName}: +{bonus.value}");
            }
        }

        // Gereksinimleri ekle
        if (item.requirements.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Gereksinimler:");
            foreach (var req in item.requirements)
            {
                // Burası daha sonra StatManager veya QuestManager ile kontrol edilebilir
                // Şimdilik sadece listeliyoruz
                sb.AppendLine($"- {req.requirementName} {req.requiredValue}");
            }
        }

        itemInfoText.text = sb.ToString();
        
        // İçeriğe göre panelin boyutunu ayarla (Opsiyonel, ContentSizeFitter ile daha kolay)
        // LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundRectTransform);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}