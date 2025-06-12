// Assets/Editor/StoryNodeDataSaver.cs
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using StoryNameSpace; // StoryNode ve Choice sınıflarınızın namespace'i
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

public static class StoryNodeDataSaver
{
    // GraphView'daki nodları JSON'a kaydet
    public static void SaveNodes(List<StoryGraphNode> graphNodes, string filePath)
    {
        List<StoryNode> storyNodesToSave = new List<StoryNode>();

        foreach (StoryGraphNode graphNode in graphNodes)
        {
            StoryNode nodeData = graphNode.StoryNodeData;

            // nextNodeId ve choices listelerini bağlantılardan güncelleyin
            nodeData.nextNodeId.Clear();
            if (nodeData.choices != null)
            {
                foreach (var choice in nodeData.choices)
                {
                    choice.nextNodeId.Clear();
                }
            }


            // Çıkış portlarından gelen bağlantıları işle
            foreach (Port outputPort in graphNode.outputContainer.Children().OfType<Port>())
            {
                foreach (Edge edge in outputPort.connections)
                {
                    StoryGraphNode targetNode = edge.input.node as StoryGraphNode;
                    if (targetNode != null)
                    {
                        if (nodeData.nodeTypeEnum == NodeTypes.Choice)
                        {
                            // Seçenek nodları için hangi choice'tan çıktığını bul
                            Choice connectedChoice = outputPort.userData as Choice; // Port'a kaydettiğimiz choice'ı al
                            if (connectedChoice != null)
                            {
                                connectedChoice.nextNodeId.Add(targetNode.StoryNodeData.id);
                            }
                        }
                        else
                        {
                            // Diğer nodlar için nextNodeId'ye ekle
                            nodeData.nextNodeId.Add(targetNode.StoryNodeData.id);
                        }
                    }
                }
            }
            storyNodesToSave.Add(nodeData);
        }

        StoryNodeListWrapper wrapper = new StoryNodeListWrapper { nodes = storyNodesToSave };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"Hikaye JSON'a kaydedildi: {filePath}");
        AssetDatabase.Refresh(); // Unity Editor'ın yeni dosyayı tanımasını sağla
    }

    // JSON'dan nodları yükle
    public static List<StoryNode> LoadNodes(string filePath)
    {
        if (File.Exists(filePath))
        {
            Debug.Log($"Hikaye dosyası bulunuyor: {filePath}");
            string json = File.ReadAllText(filePath);
            StoryNodeListWrapper wrapper = JsonUtility.FromJson<StoryNodeListWrapper>(json);
            Debug.Log($"Hikaye JSON'dan yüklendi: {filePath}");
            return wrapper.nodes;
        }
        Debug.LogWarning($"Hikaye dosyası bulunamadı: {filePath}");
        return new List<StoryNode>();
    }
}