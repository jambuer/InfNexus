using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "QuestSystem/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    [Tooltip("Oyundaki TÜM görev (QuestData) asset'lerini buraya sürükleyin.")]
    public List<QuestData> allQuests = new List<QuestData>();
}