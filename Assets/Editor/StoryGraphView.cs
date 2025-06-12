// Assets/Editor/StoryGraphView.cs
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using StoryNameSpace; // StoryNode ve Choice sınıflarınızın namespace'i

public class StoryGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(200, 150);
    private StoryGraphEditor editorWindow;

    public List<StoryGraphNode> Nodes => nodes.ToList().Cast<StoryGraphNode>().ToList();


    public StoryGraphView(StoryGraphEditor editorWindow)
    {
        this.editorWindow = editorWindow;

        // Yakınlaştırma/Uzaklaştırma (Zoom) ve sürükleme desteği
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new FreehandSelector());

        // Arka plan ızgarası
        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        // Nod oluşturma menüsü (sağ tıklama)
        nodeCreationRequest = context =>
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), new SearchWindowProvider(editorWindow));
        };
    }

    // Bağlantılar için geçerli portları belirler
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }

    // Yeni bir hikaye nodu ekler
    public StoryGraphNode CreateStoryNode(string nodeName, NodeTypes nodeType, Vector2 position, StoryNode storyNode = null)
    {
        StoryGraphNode node = new StoryGraphNode(storyNode);
        node.title = nodeName;
        node.SetPosition(new Rect(position, defaultNodeSize));
        node.StoryNodeData.id = nodeName; // ID'yi başlangıçta isimle eşitleyin
        node.StoryNodeData.nodeTypeEnum = nodeType;
        node.StoryNodeData.nodeType = nodeType.ToString();

        // Giriş Portu
        Port inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        // Çıkış Portu (Seçenekler veya NextNodeId)
        if (nodeType == NodeTypes.Choice)
        {
            // Choice nodları için her seçenek bir çıkış portu olabilir
            // Bu kısım dinamik olarak seçenekleri ekleyecek şekilde genişletilmeli
            // Örneğin:
            // node.AddChoicePort(new Choice { text = "Yeni Seçenek 1" });
            // node.AddChoicePort(new Choice { text = "Yeni Seçenek 2" });
        }
        else
        {
            // Diğer nodlar için tek bir çıkış portu
            Port outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            outputPort.portName = "Next Node";
            node.outputContainer.Add(outputPort);
        }

        // Node içeriğini güncelleyen UI elementleri ekleyin (Description, Speaker, Flags vb.)
        node.RefreshExpandedState();
        node.RefreshPorts();
        AddElement(node);
        return node;
    }

    // Mevcut StoryNode verisiyle grafiği doldurur
    public void PopulateGraph(List<StoryNode> storyNodes)
    {
        ClearGraph(); // Mevcut nodları ve bağlantıları temizle

        // Tüm nodları oluştur
        Dictionary<string, StoryGraphNode> graphNodes = new Dictionary<string, StoryGraphNode>();
        foreach (StoryNode storyNode in storyNodes)
        {
            // Varsayılan pozisyon ayarlaması, veya JSON'a pozisyonları da kaydedebilirsiniz.
            Vector2 position = new Vector2(Random.Range(100, 800), Random.Range(100, 800));
            StoryGraphNode graphNode = CreateStoryNode(storyNode.id, storyNode.nodeTypeEnum, position, storyNode);
            graphNodes[storyNode.id] = graphNode;

            // Eğer Choice nod ise, her choice için bir output port ekle
            if (storyNode.nodeTypeEnum == NodeTypes.Choice && storyNode.choices != null)
            {
                foreach (var choice in storyNode.choices)
                {
                    // Choice için özel bir output portu ekle
                    Port choiceOutputPort = graphNode.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
                    choiceOutputPort.portName = "Choice: " + choice.text;
                    // Portu benzersiz bir ID ile etiketlemek iyi olabilir, bu sayede bağlarken hangi seçeneğin portu olduğunu anlarız.
                    // choiceOutputPort.userData = choice.id; // Eğer choice'ların bir ID'si varsa
                    graphNode.outputContainer.Add(choiceOutputPort);
                }
            }
        }

        // Tüm bağlantıları oluştur
        foreach (StoryNode storyNode in storyNodes)
        {
            StoryGraphNode sourceNode = graphNodes[storyNode.id];

            // Choice nodlarındaki nextNodeId'ler için bağlantı
            if (storyNode.nodeTypeEnum == NodeTypes.Choice && storyNode.choices != null)
            {
                int choiceIndex = 0;
                foreach (Choice choice in storyNode.choices)
                {
                    if (choice.nextNodeId != null && choice.nextNodeId.Any())
                    {
                        // Sadece ilk nextNodeId'yi varsayalım, ama bunu genişletebilirsiniz
                        string targetNodeId = choice.nextNodeId.First();
                        if (graphNodes.TryGetValue(targetNodeId, out StoryGraphNode targetNode))
                        {
                            Port sourcePort = (Port)sourceNode.outputContainer.Children().ElementAt(choiceIndex); // Doğru choice portunu bul
                            Port targetPort = (Port)targetNode.inputContainer.Children().First(); // Hedef nodun input portu
                            AddElement(sourcePort.ConnectTo(targetPort));
                        }
                    }
                    choiceIndex++;
                }
            }
            // Diğer nodlardaki nextNodeId'ler için bağlantı
            else if (storyNode.nextNodeId != null && storyNode.nextNodeId.Any())
            {
                string targetNodeId = storyNode.nextNodeId.First(); // Sadece ilk nextNodeId'yi varsayalım
                if (graphNodes.TryGetValue(targetNodeId, out StoryGraphNode targetNode))
                {
                    Port sourcePort = sourceNode.outputContainer.Children().OfType<Port>().First(); // Tek çıkış portu
                    Port targetPort = targetNode.inputContainer.Children().OfType<Port>().First(); // Hedef nodun input portu
                    AddElement(sourcePort.ConnectTo(targetPort));
                }
            }
        }

        editorWindow.rootVisualElement.Add(this);
    }

    private void ClearGraph()
    {
        DeleteElements(graphElements.ToList());
    }
}