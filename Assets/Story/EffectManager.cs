using Unity;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using static StoryNameSpace.StoryNode;
using System.Linq;

namespace StoryNameSpace
{
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance;
        public List<Effect> effects;
        public Dictionary<Effect, string> effectDictionary = new Dictionary<Effect, string>();
        public string selam;
        private void Awake()
        {
            effectDictionary.Add(new FadeOutEffect(), "FadeOutEffect");
            effectDictionary.Add(new FadeInEffect(), "FadeInEffect");
            effectDictionary.Add(new NewSceneEffect(), "NewScene");

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        public Effect GetEffectByCode(string code)
        {
            Effect dondur = effectDictionary.FirstOrDefault(x => x.Value == code).Key;
            return dondur;
        }
    }
}