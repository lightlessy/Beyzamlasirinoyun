// Assets/Editor/StoryGraphEditor.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements; // UI Toolkit için
using System.Collections.Generic;
using StoryNameSpace;
using UnityEditor.UIElements;

public class StoryGraphEditor : EditorWindow
{
    public StoryGraphView graphView;
    // JSON dosya yolu için Resources klasörünü belirtelim, çünkü TextAsset olarak yüklüyoruz.
    private string storyFilePath = "Assets/Story/StoryNodes.json";

    [MenuItem("Window/Story Graph Editor")]
    public static void OpenWindow()
    {
        StoryGraphEditor window = GetWindow<StoryGraphEditor>();
        window.titleContent = new GUIContent("Story Graph Editor");
    }

    private void OnEnable()
    {
        // OnEnable'da rootVisualElement'ı temizlemek önemlidir,
        // böylece pencere her açıldığında önceki içeriği tekrar eklemeyiz.
        rootVisualElement.Clear();

        CreateToolbar(); // Önce Toolbar'ı oluştur
        CreateGraphView(); // Sonra GraphView'ı oluştur
        LoadGraph(); // Grafiği yükle
    }

    private void OnDisable()
    {
        // OnDisable'da temizleme, pencere kapanırken kaynakların serbest bırakılmasına yardımcı olur.
        if (rootVisualElement != null && graphView != null)
        {
            rootVisualElement.Remove(graphView);
        }
        // Toolbar'ı da çıkarabiliriz, ancak rootVisualElement.Clear() genellikle OnEnable'da yeterlidir.
    }

    private void CreateToolbar()
    {
        Toolbar toolbar = new Toolbar();
        toolbar.style.height = 30; // Toolbar yüksekliğini ayarla
        toolbar.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f); // Biraz koyu bir arka plan

        Button saveButton = new Button(() => SaveGraph());
        saveButton.text = "Save Story";
        saveButton.style.width = 100; // Buton genişliği
        toolbar.Add(saveButton);

        Button loadButton = new Button(() => LoadGraph());
        loadButton.text = "Load Story";
        loadButton.style.width = 100; // Buton genişliği
        toolbar.Add(loadButton);

        // Flex Grow ekleyerek butonların sola hizalı kalmasını sağla,
        // gelecekteki eklemeler için boşluk bırakır.
        VisualElement spacer = new VisualElement();
        spacer.style.flexGrow = 1;
        toolbar.Add(spacer);

        rootVisualElement.Add(toolbar);
    }

    private void CreateGraphView()
    {
        graphView = new StoryGraphView(this);
        // GraphView'ın Toolbar'ın altında kalmasını ve kalan alanı kaplamasını sağlamak için
        graphView.style.flexGrow = 1; // Kalan alanı kaplaması için
        rootVisualElement.Add(graphView);

        // GraphView'ın boyutunu üst elemanına yaymasını sağlamak için (eski StretchToParentSize yerine)
        // Yeni UI Toolkit yapısıyla, flexGrow yeterli olur.
        // graphView.StretchToParentSize(); // Bu satırı kaldırabiliriz
    }

    private void SaveGraph()
    {
        StoryNodeDataSaver.SaveNodes(graphView.Nodes, storyFilePath);
        Debug.Log("Hikaye kaydedildi!");
    }

    private void LoadGraph()
    {
        List<StoryNode> loadedNodes = StoryNodeDataSaver.LoadNodes(storyFilePath);
        if (loadedNodes != null && loadedNodes.Count > 0)
        {
            graphView.PopulateGraph(loadedNodes);
            Debug.Log($"Toplam {loadedNodes.Count} nod başarıyla yüklendi ve grafiğe eklendi.");
        }
        else
        {
            Debug.LogWarning("Yüklenecek nod bulunamadı veya JSON dosyası boş.");
        }
    }
}