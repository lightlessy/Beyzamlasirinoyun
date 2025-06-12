using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening; // DoTween kütüphanesini ekle

public class SceneFader : MonoBehaviour
{
    public CanvasGroup fadeCanvas; // Inspector'dan Paneldeki CanvasGroup'u ekle
    public float fadeDuration = 0.5f; // Fade süresi (saniye)

    private void Start()
    {
        // Oyun başladığında fade-out yap
        fadeCanvas.alpha = 1;
     }

    public void LoadScene(string sceneName)
    {
        // Yeni sahneye fade efektiyle geçiş
        fadeCanvas.DOFade(0, fadeDuration).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }
}
