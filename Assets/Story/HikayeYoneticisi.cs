using UnityEngine;
using System.IO;
using System.Collections.Generic;
using StoryNameSpace;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using JetBrains.Annotations;
using System.Linq;
using UnityEditor.Experimental.GraphView;
public class HikayeYoneticisi : MonoBehaviour
{

    public GameObject rawPanelPrefab, monologuePanelPrefab, charachterImagePrefab;
    public GameObject buttonPrefab; // Buton prefab'ı
    public GameObject SplitterPrefab; // Splitter prefab'ı
    public GameObject SpeakerImagePrefab;
    public GameObject diyalogPaneliPrefab;
    public Canvas canvas; // Canvas'ı referans olarak alıyoruz
    public List<StoryNode> storyNodes;  // Hikaye düğmeleri
    [SerializeField]
    private StoryNode currentNode;
    public TextAsset storyFile; // Hikaye dosyası
    private RectTransform canvasRect;
    [SerializeField]
    private List<GameObject> nodeObjects = new();
    public List<String> flags = new(); // Sahip olunan flaglar
    
    
    void Start()
    {
        canvasRect = canvas.GetComponent<RectTransform>();
        if (storyFile != null)
        {
            // JSON dosyasını TextAsset üzerinden oku
            string json = storyFile.text;
            storyNodes = JsonUtility.FromJson<StoryNodeListWrapper>(json).nodes;
            currentNode = storyNodes[0]; // Başlangıç düğümünü al
            DisplayStory();
        }
        else
        {
            Debug.LogError("Hikaye dosyası atanmamış!");
        }

    }
    public bool isButtonTouched = false;
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            isButtonTouched = IsPointerOverButtonElement();
        }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            if (!isButtonTouched && currentNode.nodeTypeEnum != NodeTypes.Choice)
            {
                Debug.Log("BASILDI!");
                PassNode();
            }
        }
    }

    private bool IsPointerOverButtonElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.GetTouch(0).position
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<Button>() != null) // Eğer tıklanan UI bir Button bileşeni içeriyorsa
            {
                return true;
            }
        }
        return false;
    }

    void DisplayStory()
    {
        if (currentNode == null)
        {
            Debug.LogError("Şu anki düğüm boş!");
            return;
        }
        if(currentNode.SetFlags != null && currentNode.SetFlags.Count > 0)
        {
            foreach (var flag in currentNode.SetFlags)
            {
                if (!flags.Contains(flag))
                {
                    flags.Add(flag);
                }
            }
        }
        switch (currentNode.nodeTypeEnum)
        {
            case NodeTypes.Choice:
                DisplayChoiceNode();
                break;
            case NodeTypes.Monologue:
                DisplayMonologueNode();
                break;
            case NodeTypes.Raw:
                DisplayRawTextNode();
                break;
            default:
                Debug.LogError("Bilinmeyen düğüm tipi!");
                break;
        }
        currentNode.ActivateNode();
    }

    [System.Serializable]
    public class StoryNodeListWrapper
    {
        public List<StoryNode> nodes;
    }

    void DisplayChoiceNode()
    {
        //* Soruyu Ekranda Göster
        GameObject diyalogPaneli = Instantiate(diyalogPaneliPrefab);
        diyalogPaneli.transform.SetParent(canvas.transform, false);
        RectTransform diyalogPaneliRect = diyalogPaneli.GetComponent<RectTransform>();
        diyalogPaneliRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
        diyalogPaneliRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
        diyalogPaneliRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
        float diyalogPaneliHeight = canvasRect.rect.height * 0.14f;
        diyalogPaneliRect.sizeDelta = new Vector2(canvasRect.rect.width, diyalogPaneliHeight);
        diyalogPaneliRect.anchoredPosition = new Vector2(0, -canvasRect.rect.height * 0.5f + canvasRect.rect.height * 0.12f + diyalogPaneliHeight / 2 + 2);

        TextMeshProUGUI DisplayText = diyalogPaneli.GetComponentInChildren<TextMeshProUGUI>();

        // TMP'yi, butonun merkezine yerleştiriyoruz
        if (DisplayText != null)
        {
            RectTransform textRect = DisplayText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
            textRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
            textRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
            textRect.anchoredPosition = Vector2.zero;  // Text'i butonun ortasına yerleştiriyoruz
            DisplayText.fontSize = Mathf.Min(diyalogPaneliRect.rect.width * 0.15f, diyalogPaneliHeight * 0.12f); // Font boyutunu butona göre ayarlıyoruz
            textRect.sizeDelta = new Vector2(diyalogPaneliRect.rect.width * 0.95f, diyalogPaneliHeight * 0.9f);
            DisplayText.enableAutoSizing = false; // Yazının otomatik boyutlandırılmasını sağlıyoruz
            DisplayText.alignment = TextAlignmentOptions.Center;  // Yazının ortalanmasını sağlıyoruz
            DisplayText.text = currentNode.description; // Buton metnini ayarlıyoruz 
        }


        //*Seçim Butonlarını Oluştur
        CreateButtons();

        //* Butonlar ile Metin Arasında Split oluştur
        GameObject Splitter = Instantiate(SplitterPrefab);
        Splitter.transform.SetParent(canvas.transform, false);
        RectTransform SplitterRect = Splitter.GetComponent<RectTransform>();
        SplitterRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
        SplitterRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
        SplitterRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
        SplitterRect.sizeDelta = new Vector2(canvasRect.rect.width, 2);
        SplitterRect.anchoredPosition = new Vector2(0, -canvasRect.rect.height * 0.5f + canvasRect.rect.height * 0.12f + 1);

        //* Karakter resmini ekranda göster
        GameObject SpeakerImage = Instantiate(SpeakerImagePrefab);
        SpeakerImage.transform.SetParent(canvas.transform, false);
        RectTransform SpeakerImageRect = SpeakerImage.GetComponent<RectTransform>();
        SpeakerImageRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
        SpeakerImageRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
        SpeakerImageRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
        SpeakerImageRect.sizeDelta = new Vector2(canvasRect.rect.width * 0.22f, canvasRect.rect.height * 0.26f);
        SpeakerImageRect.anchoredPosition = new Vector2(
        -canvasRect.rect.width * 0.5f + canvasRect.rect.width * 0.1f,
        -canvasRect.rect.height * 0.5f + diyalogPaneliHeight + canvasRect.rect.height * 0.25f + 2);

        //* Seçim Paneli ve Karakter Resmini Mesaj Arayüzüne ekle
        nodeObjects.Add(diyalogPaneli);
        nodeObjects.Add(Splitter);
        nodeObjects.Add(SpeakerImage);
    }
    void DisplayMonologueNode()
    {
        //* Monolog metnini ekranda göster
        GameObject monologuePanel = Instantiate(monologuePanelPrefab);
        monologuePanel.transform.SetParent(canvas.transform, false);
        RectTransform monologuePanelRect = monologuePanel.GetComponent<RectTransform>();
        monologuePanelRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
        monologuePanelRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
        monologuePanelRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
        monologuePanelRect.sizeDelta = new Vector2(canvasRect.rect.width * 0.78f, canvasRect.rect.height * 0.2f);
        monologuePanelRect.anchoredPosition = new Vector2(canvasRect.rect.width * 0.11f, -canvasRect.rect.height * (0.5f - 0.1f));
        TextMeshProUGUI monologuePanelText = monologuePanel.GetComponentInChildren<TextMeshProUGUI>();
        RectTransform monologuePanelTextRect = monologuePanelText.GetComponent<RectTransform>();
        monologuePanelText.text = currentNode.description;
        monologuePanelTextRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
        monologuePanelTextRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
        monologuePanelTextRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
        monologuePanelTextRect.anchoredPosition = Vector2.zero;  // Text'i butonun ortasına yerleştiriyoruz
        monologuePanelTextRect.sizeDelta = new Vector2(monologuePanelRect.rect.width * 0.95f, monologuePanelRect.rect.height * 0.9f);
        monologuePanelText.fontSize = Mathf.Min(monologuePanelRect.rect.width * 0.15f, monologuePanelRect.rect.height * 0.12f); // Font boyutunu butona göre ayarlıyoruz
        monologuePanelText.enableAutoSizing = false; // Yazının otomatik boyutlandırılmasını sağlıyoruz
        monologuePanelText.alignment = TextAlignmentOptions.TopLeft;  // Yazının ortalanmasını sağlıyoruz

        //* Monolog Karakterinin Resmini Ekranda Göster
        GameObject PlayerImage = Instantiate(charachterImagePrefab);
        PlayerImage.transform.SetParent(canvas.transform, false);
        RectTransform PlayerImageRect = PlayerImage.GetComponent<RectTransform>();
        PlayerImageRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
        PlayerImageRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
        PlayerImageRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
        PlayerImageRect.sizeDelta = new Vector2(canvasRect.rect.width * 0.22f, canvasRect.rect.height * 0.26f);
        PlayerImageRect.anchoredPosition = new Vector2(-canvasRect.rect.width * 0.5f + canvasRect.rect.width * 0.11f, -canvasRect.rect.height * 0.5f + canvasRect.rect.height * 0.13f);
        //TODO: Splitter ile Ayır

        //* Monolog Paneli ve Karakter Resmini Mesaj Arayüzüne ekle
        nodeObjects.Add(monologuePanel);
        nodeObjects.Add(PlayerImage);
    }
    void DisplayRawTextNode()
    {
        //* RawText metnini ekranda göster
        GameObject rawPanel = Instantiate(rawPanelPrefab);
        rawPanel.transform.SetParent(canvas.transform, false);
        RectTransform rawPanelRect = rawPanel.GetComponent<RectTransform>();
        rawPanelRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
        rawPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        rawPanelRect.pivot = new Vector2(0.5f, 0.5f);
        rawPanelRect.anchoredPosition = new Vector2(0, 0);
        rawPanelRect.sizeDelta = new Vector2(canvasRect.rect.width * 0.8f, canvasRect.rect.height * 0.2f);
        TextMeshProUGUI rawPanelText = rawPanel.GetComponentInChildren<TextMeshProUGUI>();
        RectTransform rawPanelTextRect = rawPanelText.GetComponent<RectTransform>();
        rawPanelText.text = currentNode.description;
        rawPanelTextRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
        rawPanelTextRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
        rawPanelTextRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
        rawPanelTextRect.anchoredPosition = Vector2.zero;  // Text'i butonun ortasına yerleştiriyoruz
        rawPanelTextRect.sizeDelta = new Vector2(rawPanelRect.rect.width * 0.95f, rawPanelRect.rect.height * 0.9f);
        rawPanelText.fontSize = Mathf.Min(rawPanelRect.rect.width * 0.15f, rawPanelRect.rect.height * 0.12f); // Font boyutunu butona göre ayarlıyoruz
        rawPanelText.enableAutoSizing = false; // Yazının otomatik boyutlandırılmasını sağlıyoruz
        rawPanelText.alignment = TextAlignmentOptions.TopLeft;  // Yazının ortalanmasını sağlıyoruz

        //* RawPaneli Mesaj Arayüzüne ekle
        nodeObjects.Add(rawPanel);
    }

    void CreateButtons()
    {
        int buttonCount = currentNode.choices.Count; // Buton sayısını burada belirleyebilirsiniz
        // Canvas'ın RectTransform'ını alalım
        float spacing = canvasRect.rect.width * 0.001f; // Butonlar arasındaki boşluk (%5 ekran genişliği kadar)

        // Ekranın alt kısmının %30'luk yüksekliğini alıyoruz
        float screenHeight = canvasRect.rect.height;
        float buttonHeight = screenHeight * 0.12f;

        // Butonlar için yatay alanı eşit olarak paylaştırıyoruz
        float buttonWidth = (canvasRect.rect.width - spacing) / buttonCount;

        // Butonları oluşturma işlemi
        for (int i = 0; i < buttonCount; i++)
        {

            //* Buton prefab'ını oluştur
            GameObject button = Instantiate(buttonPrefab);
            button.transform.SetParent(canvas.transform, false);
            //* Butonları Mesaj Arayüzüne ekle
            nodeObjects.Add(button);
            // Butonun RectTransform'ını alalım
            RectTransform buttonRect = button.GetComponent<RectTransform>();

            // Butonun boyutlarını ayarlayalım
            buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            // Anchor noktalarını ayarlayalım: Butonların alt kısımda olmasını sağlıyoruz
            buttonRect.anchorMin = new Vector2(0, 0); // Alt sola
            buttonRect.anchorMax = new Vector2(0, 0); // Alt sola
            buttonRect.pivot = new Vector2(0.5f, 0.5f); // Butonun tam ortası

            // Yatayda eşit olarak yerleştiriyoruz
            buttonRect.anchoredPosition = new Vector2(i * (buttonWidth + spacing) + (buttonWidth / 2), buttonHeight / 2);

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            int index = i;
            var trigger = button.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Add(new EventTrigger.Entry { eventID = EventTriggerType.PointerUp, callback = new EventTrigger.TriggerEvent() });
            trigger.triggers[0].callback.AddListener((_) => PassNode(index));

            // TMP'yi, butonun merkezine yerleştiriyoruz
            if (buttonText != null)
            {
                RectTransform textRect = buttonText.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 0.5f);  // Merkez
                textRect.anchorMax = new Vector2(0.5f, 0.5f);  // Merkez
                textRect.pivot = new Vector2(0.5f, 0.5f);     // Merkez
                textRect.anchoredPosition = Vector2.zero;  // Text'i butonun ortasına yerleştiriyoruz
                buttonText.fontSize = Mathf.Min(buttonWidth * 0.3f, buttonHeight * 0.24f); // Font boyutunu butona göre ayarlıyoruz
                buttonText.enableAutoSizing = false; // Yazının otomatik boyutlandırılmasını sağlıyoruz
                buttonText.alignment = TextAlignmentOptions.Center;  // Yazının ortalanmasını sağlıyoruz
                buttonText.text = currentNode.choices[i].text; // Buton metnini ayarlıyoruz 
            }
        }

    }
    public void PassNode(int choiceIndex = -1)
    {
        ClearNode();
        List<StoryNode> nextNodes = new List<StoryNode>();
        Debug.Log("nextNodes.Count");
        switch (currentNode.nodeTypeEnum)
        {
            case NodeTypes.Choice:
                nextNodes = storyNodes.FindAll(x => currentNode.choices[choiceIndex].nextNodeId.Contains(x.id));
                break;
            case NodeTypes.Monologue:
                nextNodes = storyNodes.FindAll(x => currentNode.nextNodeId.Contains(x.id));
                break;
            case NodeTypes.Raw:
                nextNodes = storyNodes.FindAll(x => currentNode.nextNodeId.Contains(x.id));
                break;
            default:
                Debug.LogError("Bilinmeyen düğüm tipi!");
                break;
        }
        for (int i = 0; i < nextNodes.Count; i++)
        {
            bool TumunuIceriyorMu = nextNodes[i].requiredFlags.All(x => flags.Contains(x));
            if (TumunuIceriyorMu){
            currentNode = nextNodes[i];
            break;
            }
        }
        DisplayStory();
    }
    void ClearNode()
    {
        foreach (GameObject Obje in nodeObjects)
        {
            Destroy(Obje);
        }
        nodeObjects.Clear();
    }
}






