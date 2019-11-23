﻿using System;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;

namespace TheBitCave.MMToolsExtensions.AI.Graph
{

    public class AIBrainDebuggerEditorWindow : EditorWindow
    {
    
        /// <summary>
        /// The gameobject containing the AIBrain that should be debugged.
        /// </summary>
        public GameObject aiBrainTarget;
        
        private GameObject _selectedGameObject;
        private AIBrainDebuggable _selectedBrain;
        
        private AIActionsList _actionList;
        
        private string _currentStateName;
        private string _previousStateName;

        private Vector2 _scrollPos; // Used by the scroll window

        [MenuItem("Tools/The Bit Cave/MM Extensions/AI Brain Debugger")]
        private static void Init()
        {
            var window = GetWindow<AIBrainDebuggerEditorWindow>("AI Brain Debugger", true);
            window.Show();
        }

     //   private void Awake()
     //   {
            // throw new NotImplementedException();
     //   }

        private void Update()
        {
            if (Selection.activeObject == _selectedGameObject && _selectedBrain != null) return;
            if (Selection.activeGameObject == null) return;

            if (_selectedBrain != null)
                _selectedBrain.onPerformingActions -= OnBrainPerformingActions;
            _selectedGameObject = Selection.activeGameObject;
            _selectedBrain = _selectedGameObject.GetComponent<AIBrainDebuggable>();
            if (_selectedBrain != null)
                _selectedBrain.onPerformingActions += OnBrainPerformingActions;
            aiBrainTarget = null;
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            #region styles
            
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 11
            };
            
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            var targetButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 100
            };
            
            #endregion
            
            if (_selectedBrain == null)
            {
                EditorGUI.LabelField(new Rect(0, 0, position.width, position.height), C.DEBUG_NO_AIBRAIN_COMPONENT, labelStyle);
            }
            else if (!Application.isPlaying)
            {
                EditorGUI.LabelField(new Rect(0, 0, position.width, position.height), C.DEBUG_APPLICATION_NOT_PLAYING, labelStyle);
            }
            else if (!_selectedBrain.gameObject.activeInHierarchy)
            {
                EditorGUI.LabelField(new Rect(0, 0, position.width, position.height), C.DEBUG_GAMEOBJECT_DISABLED, labelStyle);
            }
            else
            {
                EditorGUILayout.BeginVertical();
                
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(C.DEBUG_SELECTED_BRAIN_LABEL + _selectedGameObject.name, titleStyle, null);

                var label = _selectedBrain.BrainActive
                    ? C.DEBUG_ACTIVE_LABEL
                    : C.DEBUG_INACTIVE_LABEL;
                EditorGUILayout.LabelField(C.DEBUG_BRAIN_IS_LABEL + label, labelStyle, null);

                _previousStateName = _currentStateName == _selectedBrain.CurrentState.StateName
                    ? _previousStateName
                    : _currentStateName;
                _currentStateName = _selectedBrain.CurrentState.StateName;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical(GUI.skin.box);

                label = C.DEBUG_PREVIOUS_STATE_LABEL;
                EditorGUILayout.LabelField(label, titleStyle, null);
                label = _previousStateName;
                EditorGUILayout.LabelField(label, labelStyle, null);
                label = C.DEBUG_TIME_IN_STATE_LABEL + ": " + _selectedBrain.TimeInPreviousState.ToString("0.##");
                EditorGUILayout.LabelField(label, labelStyle, null);

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUI.skin.box); 
                
                label = C.DEBUG_CURRENT_STATE_LABEL;
                EditorGUILayout.LabelField(label, titleStyle, null);
                label = _currentStateName;
                EditorGUILayout.LabelField(label, labelStyle, null);
                label = C.DEBUG_TIME_IN_STATE_LABEL + ": " + _selectedBrain.TimeInThisState.ToString("0.##");
                EditorGUILayout.LabelField(label , labelStyle, null);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                label = "";
                foreach (var action in _actionList)
                {
                    if (action == null) continue;
                    label += action.GetType().Name;
                    label += ", ";
                }

                EditorGUILayout.LabelField("Performing: " + label, labelStyle, null);
                EditorGUILayout.LabelField("Target: " + _selectedBrain.Target, labelStyle, null);
                
                                
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                
                EditorGUILayout.BeginHorizontal();
                foreach (var aiState in _selectedBrain.States)
                {
                    var style = new GUIStyle(GUI.skin.button) {normal = {textColor = Color.black}};
                    foreach (var transition in _selectedBrain.CurrentState.Transitions)
                    {
                        if (transition.FalseState == aiState.StateName || transition.TrueState == aiState.StateName)
                        {
                            GUI.backgroundColor = (transition.FalseState == aiState.StateName || transition.TrueState == aiState.StateName) ?
                                new Color(0f, .5f, 1f, 1):
                                Color.white;
                        }
                    }

                    EditorGUI.BeginDisabledGroup(_selectedBrain.CurrentState.StateName == aiState.StateName);
                    if(GUILayout.Button(aiState.StateName, style)) TransitionToState(aiState.StateName);
                    EditorGUI.EndDisabledGroup();
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                #region target
                
                EditorGUILayout.BeginHorizontal();
                aiBrainTarget = EditorGUILayout.ObjectField(C.DEBUG_TARGET_LABEL, aiBrainTarget, typeof(GameObject), true) as GameObject;
                EditorGUI.BeginDisabledGroup(aiBrainTarget == null || !aiBrainTarget.scene.IsValid());
                if(GUILayout.Button(C.DEBUG_SET_TARGET_LABEL, targetButtonStyle)) _selectedBrain.Target = aiBrainTarget.transform;
                EditorGUI.EndDisabledGroup();
                EditorGUI.BeginDisabledGroup(aiBrainTarget == null);
                if (GUILayout.Button(C.DEBUG_REMOVE_TARGET_LABEL, targetButtonStyle))
                {
                    _selectedBrain.Target = null;
                    aiBrainTarget = null;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                
                #endregion
                
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }

        private void TransitionToState(string stateName)
        {
            _selectedBrain.TransitionToState(stateName);
        }
        
        private void OnBrainPerformingActions(AIActionsList actionList)
        {
            _actionList = actionList;
        }

        private void OnDisable()
        {
            aiBrainTarget = null;
        }
    }
}