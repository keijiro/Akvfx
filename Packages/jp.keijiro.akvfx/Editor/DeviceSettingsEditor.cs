using UnityEngine;
using UnityEditor;

namespace Akvfx {

[CanEditMultipleObjects]
[CustomEditor(typeof(DeviceSettings))]
sealed class DeviceSettingsEditor : Editor
{
    SerializedProperty _autoExposure;
    SerializedProperty _exposure;

    SerializedProperty _autoWhiteBalance;
    SerializedProperty _whiteBalance;

    SerializedProperty _brightness;
    SerializedProperty _contrast;
    SerializedProperty _saturation;
    SerializedProperty _sharpness;
    SerializedProperty _gain;

    SerializedProperty _enableBlc;
    SerializedProperty _powerIs60Hz;

    SerializedProperty _maxDepth;

    static class Styles
    {
        public static GUIContent enableBlc = new GUIContent("Enable BLC");
        public static GUIContent powerIs60Hz = new GUIContent("Power is 60Hz");
    }

    void OnEnable()
    {
        _autoExposure = serializedObject.FindProperty("_autoExposure");
        _exposure = serializedObject.FindProperty("_exposure");

        _autoWhiteBalance = serializedObject.FindProperty("_autoWhiteBalance");
        _whiteBalance = serializedObject.FindProperty("_whiteBalance");

        _brightness = serializedObject.FindProperty("_brightness");
        _contrast = serializedObject.FindProperty("_contrast");
        _saturation = serializedObject.FindProperty("_saturation");
        _sharpness = serializedObject.FindProperty("_sharpness");
        _gain = serializedObject.FindProperty("_gain");

        _enableBlc = serializedObject.FindProperty("_enableBlc");
        _powerIs60Hz = serializedObject.FindProperty("_powerIs60Hz");

        _maxDepth = serializedObject.FindProperty("_maxDepth");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_autoExposure);
        if (_autoExposure.hasMultipleDifferentValues ||
            !_autoExposure.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_exposure);
            EditorGUILayout.PropertyField(_gain);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(_autoWhiteBalance);
        if (_autoWhiteBalance.hasMultipleDifferentValues ||
            !_autoWhiteBalance.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_whiteBalance);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(_brightness);
        EditorGUILayout.PropertyField(_contrast);
        EditorGUILayout.PropertyField(_saturation);
        EditorGUILayout.PropertyField(_sharpness);

        EditorGUILayout.PropertyField(_enableBlc, Styles.enableBlc);
        EditorGUILayout.PropertyField(_powerIs60Hz, Styles.powerIs60Hz);

        EditorGUILayout.PropertyField(_maxDepth);

        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("Assets/Create/Akvfx/Device Settings")]
    public static void CreateDeviceSettings()
    {
        var asset = ScriptableObject.CreateInstance<DeviceSettings>();
        ProjectWindowUtil.CreateAsset(asset, "Akvfx Settings.asset");
    }
}

}
