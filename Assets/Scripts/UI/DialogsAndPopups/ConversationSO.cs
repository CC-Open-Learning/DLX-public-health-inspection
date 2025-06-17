using System.Collections.Generic;
using UnityEngine;

namespace VARLab.PublicHealth
{
    [CreateAssetMenu(fileName = "ConversationSO", menuName = "ScriptableObjects/ConversationSO")]
    public class ConversationSO : ScriptableObject
    {
        [System.Serializable]
        public struct Dialogue
        {
            public Sprite Avatar;
            public string Speaker;
            [TextArea(3, 10)]
            public string Text;
            public AudioClip AudioClip;
            public Color TextBackgroundColour;
            public Color ImageBorderColour;
            public Color TextBorderRight;
            public Color TextBorderLeft;
            public Color TextBorderTop;
            public Color TextBorderBottom;
        }

        [Tooltip("Window Name")] public string Name;
        [Tooltip("Scenario Name")] public string Scenario;
        [Tooltip("Description")] public string ScenarioText;
        [Tooltip("Button Text")] public string ButtonText;
        [Tooltip("Start Dialogue Side")] public bool StartDialogueOnLeft = true;

        public List<Dialogue> dialogue = new List<Dialogue>();
    }
}
