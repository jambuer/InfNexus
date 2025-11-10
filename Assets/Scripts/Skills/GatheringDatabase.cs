using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Oyundaki tüm GatheringChapterData'ları tutan ana veritabanı.
/// UI script'lerinin bölümleri bulması için kullanılır.
/// </summary>
[CreateAssetMenu(fileName = "GatheringDatabase", menuName = "Game/Gathering Database")]
public class GatheringDatabase : ScriptableObject
{
    [Tooltip("Oyundaki tüm Gathering Chapter (Bölüm) asset'lerini buraya sürükleyin")]
    public List<GatheringChapterData> allChapters;
}