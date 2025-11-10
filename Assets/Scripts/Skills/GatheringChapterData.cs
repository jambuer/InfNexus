using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bir "Chapter" (Bölüm) tanımlar ve o bölüme ait tüm 
/// toplayıcılık eylemlerini (GatheringNodeData) listeler.
/// </summary>
[CreateAssetMenu(fileName = "GatheringChapter_", menuName = "Game/Gathering Chapter")]
public class GatheringChapterData : ScriptableObject
{
    [Tooltip("Bölüm ID'si (örn: chapter_1)")]
    public string chapterID;
    
    [Tooltip("UI'da (Dropdown'da) görünecek bölüm adı (örn: Chapter 1: Orman)")]
    public string chapterName;

    [Tooltip("Bu bölüme ait tüm bölgelerin (Kuzey, Güney vb.) listesi")]
    public List<GatheringRegion> regions;
}