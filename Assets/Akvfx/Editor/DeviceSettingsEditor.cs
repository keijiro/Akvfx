using UnityEngine;
using UnityEditor;

namespace Akvfx
{
    sealed class DeviceSettingsEditor
    {
        [MenuItem("Assets/Create/Akvfx/Device Settings")]
        public static void CreateDeviceSettings()
        {
            var asset = ScriptableObject.CreateInstance<DeviceSettings>();

            AssetDatabase.CreateAsset(asset, "Assets/Akvfx Settings.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}
