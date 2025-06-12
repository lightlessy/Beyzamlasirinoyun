using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
public class UIManager : MonoBehaviour

{
    public static UIManager Instance { get; private set; }
    public RectTransform mainMenuPanel;
    public RectTransform optionsPanel;
     public RectTransform CreditsPanel;
    public float transitionTime = 0.1f;
      private float screenWidth;

    void Start()
    {
        Instance = this;
        screenWidth = Screen.width;
        optionsPanel.anchoredPosition = new Vector2(screenWidth, 0); // Ayarlar menüsünü sağda başlat
        CreditsPanel.anchoredPosition = new Vector2(screenWidth, 0); // Credits menüsünü sağda başlat
    }

    public void OpenCreditsPanel(){
        CreditsPanel.DOAnchorPos(new Vector2(0, 0), transitionTime); 
        mainMenuPanel.DOAnchorPos(new Vector2(-screenWidth, 0), transitionTime); // Credits menüsünü aç
    }
    public void CloseCreditsPanel(){
        mainMenuPanel.DOAnchorPos(new Vector2(0, 0), transitionTime);
        CreditsPanel.DOAnchorPos(new Vector2(screenWidth, 0), transitionTime); // Credits menüsünü kapat
    } 
   public void OpenSettings(){
        optionsPanel.DOAnchorPos(new Vector2(0, 0), transitionTime); 
        mainMenuPanel.DOAnchorPos(new Vector2(-screenWidth, 0), transitionTime); // Ayarlar menüsünü aç
    }
    public void CloseSettings(){
        mainMenuPanel.DOAnchorPos(new Vector2(0, 0), transitionTime);
        optionsPanel.DOAnchorPos(new Vector2(screenWidth, 0), transitionTime); // Ayarlar menüsünü kapat
    }
}
