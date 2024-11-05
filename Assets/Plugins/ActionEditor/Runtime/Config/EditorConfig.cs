using Sirenix.OdinInspector;
using UnityEngine;

namespace Darkness
{
    public static class EditorConfig
    {
        [SerializeField] [LabelText("保存路径")]
        private static string m_path = "Asset/Simple";

        public static string SavePath => m_path;

    }
}