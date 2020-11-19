using UnityEngine;
using UnityEditor;

namespace Akvfx {

[CanEditMultipleObjects]
[CustomEditor(typeof(DeviceController))]
sealed class DeviceControllerEditor : Editor
{
    SerializedProperty _deviceSettings;

    void OnEnable()
      => _deviceSettings = serializedObject.FindProperty("_deviceSettings");

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(_deviceSettings);
        serializedObject.ApplyModifiedProperties();
    }
}

}
