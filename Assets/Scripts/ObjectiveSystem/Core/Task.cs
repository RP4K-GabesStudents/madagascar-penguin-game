using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine;

namespace ObjectiveSystem.Core
{
    [CreateAssetMenu(menuName = "ObjectiveSystem/Task", fileName = "New Task")]
    public class Task : ScriptableObject
    {
        public readonly List<ITaskConditional> CompletionTasks = new ();// if true, player succeeded
        public readonly List<ITaskConditional> FailedTasks = new (); // if true, player failed
        
        public readonly List<IReward> Rewards = new ();
        
        public string Name => name;

        public event Action TaskUpdated;

        public void RebuiltText()
        {
            //format: {C0} in {F0} without {F1} << pretext
            // after: kill 2 guys in 1 second without being seen << curtext
            StringBuilder sb = new();
            for (var i = 0; i < preText.Length; i++)
            {
                var t = preText[i];
                if (t.Equals('{'))
                {
                    i++;
                    if (i >= preText.Length) return;
                    List<ITaskConditional> curList;
                    int id = 0;
                    
                    switch ((byte)preText[i] | 32)
                    {
                        case 'c':
                            curList = CompletionTasks;
                            break;
                        case 'f':
                            curList = FailedTasks;
                            break;
                        default:
                            Debug.LogError($"something scary has happened with the text builder {name}", this);
                            return;
                    }

                    StringBuilder num = new();
                    while (++i <= preText.Length && preText[i] != '}')
                        num.Append(preText[i]);

                    id = int.Parse(num.ToString());
                    
                    sb.Append(CompletionTasks[id].CurrentText);
                }
                else sb.Append(t);
            }
        }

        private void OnValidate() => RebuiltText();

        [SerializeField, TextArea] private string preText;
        [SerializeField, ReadOnly]private string _curText;

        public string CurrentText
        {
            get
            {
                if (string.IsNullOrEmpty(_curText))
                    RebuiltText();
                return _curText;
            }
        }
        
        
    }
}