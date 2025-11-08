using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bir Image bileşenine gelişmiş görsel efektler (Blur, Saturation, Contrast)
/// uygulamak için kullanılır. 
/// "UI/AdvancedEffects" shader'ını kullanan bir Materyal gerektirir.
/// </summary>
[RequireComponent(typeof(Image))]
[AddComponentMenu("UI/UI Advanced Effects")] // Component menüsüne ekler
public class UIImageAdvancedEffects : MonoBehaviour
{
    [Header("Efekt Ayarları")]
    [Tooltip("Görüntünün ne kadar bulanık olacağı.")]
    [Range(0f, 5f)]
    public float blurAmount = 0f;

    [Tooltip("Görüntünün ne kadar solgun (0) veya canlı (2) olacağı.")]
    [Range(0f, 2f)]
    public float saturation = 1f;

    [Tooltip("Görüntünün parlaklığı.")]
    [Range(0f, 3f)]
    public float brightness = 1f;

    [Tooltip("Görüntünün kontrastı (renkler arası fark).")]
    [Range(1f, 3f)]
    public float contrast = 1f;

    [Header("Referanslar")]
    [Tooltip("Efektleri uygulayacak Materyal. Otomatik oluşturulur.")]
    [SerializeField]
    private Material effectMaterial;

    private Image targetImage;

    // Shader'daki özelliklerin ID'lerini önbelleğe al
    private static readonly int _BlurAmount = Shader.PropertyToID("_BlurAmount");
    private static readonly int _Saturation = Shader.PropertyToID("_Saturation");
    private static readonly int _Brightness = Shader.PropertyToID("_Brightness");
    private static readonly int _Contrast = Shader.PropertyToID("_Contrast");


    void OnEnable()
    {
        InitializeMaterial();
        ApplyEffects();
    }

    /// <summary>
    /// Materyalin var olduğundan ve Image'a atandığından emin olur.
    /// </summary>
    void InitializeMaterial()
    {
        targetImage = GetComponent<Image>();

        if (effectMaterial == null || effectMaterial.shader.name != "UI/AdvancedEffects")
        {
            // Shader'ı bul
            Shader effectShader = Shader.Find("UI/AdvancedEffects");
            if (effectShader == null)
            {
                Debug.LogError("UIImageAdvancedEffects: 'UI/AdvancedEffects.shader' bulunamadı! Lütfen shader dosyasını projeye ekleyin.", this);
                return;
            }

            // Bu Image için özel bir Materyal kopyası (Instance) oluştur
            effectMaterial = new Material(effectShader);
        }

        // Materyali Image'a ata
        targetImage.material = effectMaterial;
    }

    /// <summary>
    /// Değerleri Materyale (Shader'a) gönderir.
    /// </summary>
    void ApplyEffects()
    {
        if (effectMaterial == null)
        {
            InitializeMaterial();
            if (effectMaterial == null) return; // Shader bulunamadıysa çık
        }

        effectMaterial.SetFloat(_BlurAmount, blurAmount);
        effectMaterial.SetFloat(_Saturation, saturation);
        effectMaterial.SetFloat(_Brightness, brightness);
        effectMaterial.SetFloat(_Contrast, contrast);
    }

    /// <summary>
    /// Inspector'daki değerler her değiştiğinde bu fonksiyon çalışır.
    /// Efektleri anında (Editörde) görmemizi sağlar.
    /// </summary>
    private void OnValidate()
    {
        // OnValidate, Awake/Start'tan önce çalışabilir, bu yüzden materyali burada da kontrol et
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        // Eğer materyalimiz yoksa veya Image'ın materyali biz değilsek,
        // (örn: Editörde script'i yeni eklediğimizde) materyali ata.
        if (targetImage.material == null || targetImage.material.shader.name != "UI/AdvancedEffects")
        {
             // Editörde anında atama yap (Runtime için OnEnable'ı bekle)
            if (Application.isPlaying)
            {
                InitializeMaterial();
            }
            else
            {
                // Editörde ve Play modda değilken geçici olarak ayarla
                Shader effectShader = Shader.Find("UI/AdvancedEffects");
                if (effectShader)
                {
                    // Not: Bu geçici bir materyaldir, Play'e basınca OnEnable'da
                    // düzgün bir instance oluşturulur.
                    targetImage.material = new Material(effectShader);
                    effectMaterial = targetImage.material; 
                }
            }
        }
        
        ApplyEffects();
    }
}