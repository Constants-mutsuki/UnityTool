using Sirenix.OdinInspector;
using UnityEngine;

namespace Darkness
{
    public class EditorConfig : ScriptableObject
    {
        [SerializeField] [LabelText("保存路径")]
        private string m_path;

        public string SavePath => m_path;

    }
}