using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class SceneLoader : MonoBehaviour
{
     public CanvasGroup fadeCanvas; // Inspector'dan Paneldeki CanvasGroup'u ekle
    public float fadeDuration = 0.5f; // Fade süresi (saniye)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
                fadeCanvas.alpha = 0;
                fadeCanvas.DOFade(1, fadeDuration);
    }

     
}
