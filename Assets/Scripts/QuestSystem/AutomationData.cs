// AutomationData.cs (DÜZELTİLMİŞ HALİ)
using System;
using System.Collections.Generic;

[Serializable]
public class AutomationData
{
  	public bool canBeAutomated = false;

    // [YENİ] Artık merkezi List<Requirement> yapısını kullanıyor
  	public List<Requirement> unlockRequirements; 

  	public int totalCompletionsToUnlock = 0;
  	public List<AutomationTier> automationTiers;
}