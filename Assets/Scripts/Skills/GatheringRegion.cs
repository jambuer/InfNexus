using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bir "Gathering Chapter" (örn: Orman) içindeki belirli bir bölgeyi 
/// (örn: Kuzey, Güney) ve o bölgedeki toplayıcılık eylemlerini tanımlar.
/// </summary>
[System.Serializable]
public class GatheringRegion
{
    [Tooltip("Bölgenin UI'da (buton üzerinde) görünecek adı (örn: Kuzey Orman)")]
    public string regionName;

    // [Tooltip("Gelecekte buton üzerinde ikon göstermek için eklenebilir")]
    // public Sprite regionIcon;

    [Tooltip("Bu bölgeye ait tüm toplayıcılık eylemlerinin (Node) listesi")]
    public List<GatheringNodeData> gatheringNodes;
}