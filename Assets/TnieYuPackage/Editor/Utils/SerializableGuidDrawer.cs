#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace TnieYuPackage.Utils
{
    [CustomPropertyDrawer(typeof(SerializableGuid))]
    public class SerializableGuidDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 24f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty valueProperty = property.FindPropertyRelative("value");

            Rect fieldRect = position;
            fieldRect.width -= ButtonWidth + Spacing;

            Rect buttonRect = position;
            buttonRect.x = fieldRect.xMax + Spacing;
            buttonRect.width = ButtonWidth;

            // Hiển thị string như bình thường
            EditorGUI.PropertyField(fieldRect, valueProperty, label);

            GUIContent icon = EditorGUIUtility.IconContent("Refresh");

            if (GUI.Button(buttonRect, icon, GUIStyle.none))
            {
                valueProperty.stringValue = Guid.NewGuid().ToString();

                property.serializedObject.ApplyModifiedProperties();
                GUI.changed = true;
            }

            EditorGUI.EndProperty();
        }
    }
}
#endif