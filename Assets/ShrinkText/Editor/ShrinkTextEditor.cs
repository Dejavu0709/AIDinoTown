using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
namespace NexgenDragon
{
    [CustomEditor(typeof(ShrinkText), true)]
    [CanEditMultipleObjects]
    public class ShrinkTextEditor : UnityEditor.UI.TextEditor
    {
        SerializedProperty m_SingleLine;
        SerializedProperty m_GlowColor;
        SerializedProperty m_GlowSize;
        SerializedProperty m_ShadowOffset;
        SerializedProperty m_UseAutoArabic;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_SingleLine = serializedObject.FindProperty("_singleLine");
            m_GlowColor = serializedObject.FindProperty("_glowColor");
            m_GlowSize = serializedObject.FindProperty("_glowSize");
            m_ShadowOffset = serializedObject.FindProperty("_shadowOffset");
            m_UseAutoArabic= serializedObject.FindProperty("_useAutoArabic");
        }

        public override void OnInspectorGUI()
        {
         //   serializedObject.Update();
            EditorGUILayout.PropertyField(m_SingleLine);
            EditorGUILayout.PropertyField(m_GlowColor);
            EditorGUILayout.PropertyField(m_GlowSize);
            EditorGUILayout.PropertyField(m_ShadowOffset);
            EditorGUILayout.PropertyField(m_UseAutoArabic);
         //   AppearanceControlsGUI();
         //   RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}