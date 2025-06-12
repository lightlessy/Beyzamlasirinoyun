using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryNameSpace {
[System.Serializable]
public class StoryNode : ISerializationCallbackReceiver
{
    public string id;                  // Node ID
    public string description;         // Açıklama metni
    public List<Choice> choices;
    public string Speaker;       // Konuşmacı

    public List<String> nextNodeId;       // Seçenekler
    public string nodeType;         // Node tipi
    public NodeTypes nodeTypeEnum; // Node tipi enum olarak
    public List<Effect> eventTrigger; // Event tetikleyici
    
    public string conditionalText;     // Şartlı metin (güç seviyesine göre değişebilir)
    public float cooldownTime;         // Soğuma süresi (Cooldown)
    public List<string> requiredFlags;
    public List<Dictionary<string,float>> requiredVariables;         // Node'u göstermek için gerekli flaglar
    public List<string> SetFlags;
    

      public void ActivateNode()
    {
        foreach (Effect code in eventTrigger)
        {
            Debug.Log(code.effectName);
            Effect effect = EffectManager.Instance.GetEffectByCode(code.effectName);
            if (effect != null)
            {
                effect.PlayEffect(code.value, code.duration);
            }
        
    }
    }
    public void OnBeforeSerialize() {}
    
    public void OnAfterDeserialize()
        {
           if (Enum.TryParse(nodeType, true, out NodeTypes parsed))
        {
            nodeTypeEnum = parsed;
        }
        else
        {
            Debug.LogWarning($"Geçersiz nodeType: {nodeType}, default Raw kullanılıyor.");
            nodeTypeEnum = NodeTypes.Raw;
        }
        }
}


[System.Serializable]
public class Choice
{
    public string text;                // Seçenek metni
    public List<string> nextNodeId;          // Sonraki node ID'si
    public HashSet<String> setFlags;             // Bu seçeneği seçtiğinde flag ayarla
    public HashSet<String> requiredFlags;         // Bu seçeneği seçmeden önce gerekli flag
    public Dictionary<string,float> requiredVariable;
 }

 
public enum NodeTypes{
    Choice,
    Monologue,
    Raw
}
  [System.Serializable]
public class StoryNodeListWrapper
{
    public List<StoryNode> nodes;
}
}

