using System;
using System.Collections.Generic;
using UnityEngine;


namespace StoryNameSpace
{


    [System.Serializable]
    public class Effect
    {
        public string effectName;
        public string description;
        public string value;
        public float duration;
        public virtual void PlayEffect(string value, float duration) { }
    }

    [System.Serializable]
    public class FadeOutEffect : Effect
    {
        public new string effectName = "FadeOut";
        public override void PlayEffect(string value, float duration)
        {
            Debug.Log("Fade Out effect is playing with value: " + value + " and duration: " + duration);
            // Buraya fade out kodunu ekleyebilirsin.
        }
    }

    [System.Serializable]
    public class FadeInEffect : Effect
    {
        public override void PlayEffect(string value, float duration)
        {
            Debug.Log("Fade In effect is playing with value: " + value + " and duration: " + duration);
            // Buraya fade out kodunu ekleyebilirsin.
        }
    }
    [System.Serializable]
    public class NewSceneEffect : Effect
    {
        public override void PlayEffect(string value, float duration)
        {
            Debug.Log("New Scene effect is playing with value: " + value + " and duration: " + duration);
        }
    }

}

