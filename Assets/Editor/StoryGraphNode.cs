// Assets/Editor/StoryGraphNode.cs
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using StoryNameSpace; // StoryNode sınıfınızın namespace'i
using UnityEditor.UIElements; // PropertyField için
using System;
using System.Collections.Generic;
public class StoryGraphNode : Node
{
    public StoryNode StoryNodeData { get; private set; }

    public StoryGraphNode(StoryNode storyNode = null)
    {
        StoryNodeData = storyNode ?? new StoryNode();
        this.title = StoryNodeData.id ?? "New Node";

        // Node içeriğini doldurma
        CreateContent();
    }

    private void CreateContent()
    {
        // Description alanı
        TextField descriptionField = new TextField("Description:");
        descriptionField.bindingPath = "description"; // StoryNodeData.description'a bağla
        descriptionField.value = StoryNodeData.description;
        descriptionField.multiline = true;
        descriptionField.RegisterValueChangedCallback(evt => StoryNodeData.description = evt.newValue);
        contentContainer.Add(descriptionField);

        // Speaker alanı (Monologue nodlar için)
        if (StoryNodeData.nodeTypeEnum == NodeTypes.Monologue)
        {
            TextField speakerField = new TextField("Speaker:");
            speakerField.bindingPath = "Speaker";
            speakerField.value = StoryNodeData.Speaker;
            speakerField.RegisterValueChangedCallback(evt => StoryNodeData.Speaker = evt.newValue);
            contentContainer.Add(speakerField);
        }

        // Node tipi dropdown'ı
        EnumField nodeTypeField = new EnumField("Node Type:", StoryNodeData.nodeTypeEnum);
        nodeTypeField.RegisterValueChangedCallback(evt =>
        {
            StoryNodeData.nodeTypeEnum = (NodeTypes)evt.newValue;
            StoryNodeData.nodeType = StoryNodeData.nodeTypeEnum.ToString();
            // Node tipi değiştiğinde portları veya UI'ı güncellemeniz gerekebilir.
            RefreshPorts();
        });
        contentContainer.Add(nodeTypeField);


        // Diğer özellikler (requiredFlags, SetFlags, Choices, etc.) için PropertyField kullanabilir veya custom UI elementleri oluşturabilirsiniz.
        // Örneğin, PropertyField ile daha kolay bağlama:
        // serializedObject = new SerializedObject(StoryNodeData); // Bu, StoryNodeData bir ScriptableObject ise çalışır.
        // Eğer StoryNodeDatna sadece bir sınıf ise, her alanı manuel olarak bağlamanız gerekir.

        // Choices için dinamik UI ve Port oluşturma
        if (StoryNodeData.nodeTypeEnum == NodeTypes.Choice)
        {
            // Seçenekleri eklemek için bir Add düğmesi
            Button addChoiceButton = new Button(() => AddChoicePort(new Choice { text = "New Choice", nextNodeId = new List<string>() }));
            addChoiceButton.text = "Add Choice";
            contentContainer.Add(addChoiceButton);

            if (StoryNodeData.choices != null)
            {
                foreach (Choice choice in StoryNodeData.choices)
                {
                    AddChoicePort(choice);
                }
            }
        }
    }

    public void AddChoicePort(Choice choice)
    {
        StoryNodeData.choices = StoryNodeData.choices ?? new List<Choice>();
        if (!StoryNodeData.choices.Contains(choice))
        {
            StoryNodeData.choices.Add(choice);
        }

        // Choice text alanı
        TextField choiceTextField = new TextField("Choice Text:");
        choiceTextField.value = choice.text;
        choiceTextField.RegisterValueChangedCallback(evt => choice.text = evt.newValue);
        outputContainer.Add(choiceTextField);

        // Choice için çıkış portu
        Port choiceOutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(float));
        choiceOutputPort.portName = "->" + choice.text; // Port adını seçeneğe göre ayarla
        choiceOutputPort.userData = choice; // Seçeneğin kendisini porta bağla (bağlantı kurarken erişmek için)
        outputContainer.Add(choiceOutputPort);

        // Seçeneği sil butonu
        Button removeChoiceButton = new Button(() =>
        {
            outputContainer.Remove(choiceTextField);
            outputContainer.Remove(choiceOutputPort);
            StoryNodeData.choices.Remove(choice);
            RefreshPorts();
        });
        removeChoiceButton.text = "X";
        outputContainer.Add(removeChoiceButton);

        RefreshPorts();
    }
}