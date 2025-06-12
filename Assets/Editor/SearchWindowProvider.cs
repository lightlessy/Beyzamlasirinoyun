// Assets/Editor/SearchWindowProvider.cs
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;
using StoryNameSpace; // NodeTypes enum'unuz için
using UnityEngine.UIElements;

public class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
{
    private StoryGraphEditor editorWindow;

    public SearchWindowProvider(StoryGraphEditor window)
    {
        editorWindow = window;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            new SearchTreeGroupEntry(new GUIContent("Story Nodes"), 1)
        };

        // Her NodeTypes enum değeri için bir seçenek ekleyin
        foreach (NodeTypes nodeType in System.Enum.GetValues(typeof(NodeTypes)))
        {
            tree.Add(new SearchTreeEntry(new GUIContent(nodeType.ToString()))
            {
                level = 2,
                userData = nodeType
            });
        }
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        NodeTypes nodeType = (NodeTypes)entry.userData;
        // Fare pozisyonunu GraphView koordinatlarına dönüştür
        Vector2 localMousePos = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);
        Vector2 graphMousePos = editorWindow.graphView.contentViewContainer.WorldToLocal(localMousePos);

        editorWindow.graphView.CreateStoryNode("New " + nodeType.ToString() + " Node", nodeType, graphMousePos);
        return true;
    }
}