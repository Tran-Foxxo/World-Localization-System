using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using VRC.SDK3.Data;
using TextAsset = UnityEngine.TextAsset;

namespace TranFox.LocalizationSystem
{
    public class LanguageJsonEditor : EditorWindow
    {
        TextAsset _LanguageAsset;
        string _KeySearch = "";
        int _SelectedKeySearchIndex = 0;

        TextAsset _CurrentLoadedAsset;

        // Bodge to fix playmode issues
        List<string> _LoadedLanguageKeys;
        List<string> _LoadedLanguageValues;

        [MenuItem("Tran Fox/Language Json Editor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            LanguageJsonEditor window = (LanguageJsonEditor)EditorWindow.GetWindow(typeof(LanguageJsonEditor));
            window.name = "Language Json Editor";
            window.Show();
        }

        void OnGUI()
        {
            _LanguageAsset = (TextAsset)EditorGUILayout.ObjectField("Language Asset: ", (UnityEngine.Object)_LanguageAsset, typeof(TextAsset), false);

            if (_CurrentLoadedAsset != _LanguageAsset)
            {
                string name = _LanguageAsset != null ? _LanguageAsset.name : "null";

                bool changed = EditorUtility.DisplayDialog(
                    "[LanguageJsonEditor] Language Changed!",
                    $"Are you sure you want to change the loaded language to {name}?\n\nIf you have any unsaved changes you will lose them.",
                    "Change",
                    "Don't Change");

                if (changed)
                {
                    _CurrentLoadedAsset = _LanguageAsset;
                    _LoadedLanguageKeys = null;
                    _LoadedLanguageValues = null;
                    _SelectedKeySearchIndex = 0;
                }
                else
                {
                    _LanguageAsset = _CurrentLoadedAsset;
                }
            }

            // If no lang is loaded return
            if (_CurrentLoadedAsset == null) return;

            // Load new language
            if (_LoadedLanguageKeys == null || _LoadedLanguageValues == null)
            {
                if (VRCJson.TryDeserializeFromJson(_CurrentLoadedAsset.text, out DataToken result) == true)
                {
                    _LoadedLanguageKeys = new List<string>();
                    _LoadedLanguageValues = new List<string>();

                    Debug.Log("LOADING LANG");
                    var resultKeys = result.DataDictionary.GetKeys();
                    var resultValues = result.DataDictionary.GetValues();
                    
                    for(int i = 0; i < resultKeys.Count; i++) 
                    {
                        if (resultKeys[i].TokenType == TokenType.String && resultValues[i].TokenType == TokenType.String)
                        {
                            _LoadedLanguageKeys.Add(resultKeys[i].String);
                            _LoadedLanguageValues.Add(resultValues[i].String);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"JSON IS INVALID!!", EditorStyles.boldLabel);
                    return;
                }
            }

            EditorGUILayout.Space(10);

            // Key Search
            _KeySearch = EditorGUILayout.TextField("Key Search: ", _KeySearch);
            List<string> keysAfterSearch = new List<string>();
            foreach (string key in _LoadedLanguageKeys)
            {
                if (key.Contains(_KeySearch, System.StringComparison.CurrentCultureIgnoreCase))
                    keysAfterSearch.Add(key);
            }

            // Key selection dropdown
            _SelectedKeySearchIndex = EditorGUILayout.Popup("Selected Key: ", _SelectedKeySearchIndex, keysAfterSearch.ToArray());
            if (_SelectedKeySearchIndex >= keysAfterSearch.Count)
            {
                _SelectedKeySearchIndex = 0; // Set to 0 if over, happens when changing search
            }

            EditorGUILayout.Space(10);

            // Add Key
            if (!_LoadedLanguageKeys.Contains(_KeySearch) && _KeySearch != "")
            {
                if (GUILayout.Button($"Add Key \"{_KeySearch}\""))
                {
                    _LoadedLanguageKeys.Add(_KeySearch);
                    _LoadedLanguageValues.Add("");
                }
            }
            if (keysAfterSearch.Count == 0) { return; }
            
            // Get selected key
            string currentKey = keysAfterSearch[_SelectedKeySearchIndex];
            int currentKeyIndex = _LoadedLanguageKeys.IndexOf(currentKey);

            // Key Deletion
            if (GUILayout.Button($"Remove Key \"{currentKey}\""))
            {
                // This code spacing looks REALLY ugly but it's readable enough...
                if (EditorUtility.DisplayDialog(
                    "[LanguageJsonEditor] Key Deletion",
                    $"Are you sure you want to delete \"{currentKey}\"?",
                    "Yes",
                    "No")
                    ) 
                {
                    _LoadedLanguageKeys.RemoveAt(currentKeyIndex);
                    _LoadedLanguageValues.RemoveAt(currentKeyIndex);
                }
            }

            EditorGUILayout.Space(10);

            // Edtinging box
            GUILayout.Label($"KEY \"{currentKey}\" ({currentKeyIndex}):", EditorStyles.boldLabel);
            _LoadedLanguageValues[currentKeyIndex] = EditorGUILayout.TextArea(_LoadedLanguageValues[currentKeyIndex]);

            // Saving
            if (GUILayout.Button("Save Language"))
            {
                // Make our own DataDictionary in case VRChat weirdness lol
                DataDictionary outputDictionary = new DataDictionary();
                var keys = _LoadedLanguageKeys.ToArray();
                var values = _LoadedLanguageValues.ToArray();

                for (int i = 0; i < _LoadedLanguageKeys.Count; i++)
                {
                    string keyString = keys[i];
                    string valueString = values[i];

                    // Remove \r's this may cause unexpected behaviour with TMP actually supporting them
                    // If you really need \r for some reason just comment out this line I guess.
                    valueString = valueString.Replace("\r", "");

                    outputDictionary.Add(keyString, valueString);
                }

                bool worked = VRCJson.TrySerializeToJson(outputDictionary, JsonExportType.Beautify, out DataToken outToken);

                if (!worked) { Debug.LogError("[LanguageJsonEditor] UNABLE TO SAVE FOR SOME REASON???"); return; }

                System.IO.File.WriteAllText(AssetDatabase.GetAssetPath(_LanguageAsset), outToken.String);
                Debug.Log($"[LanguageJsonEditor] Saved {_LanguageAsset.name}!");
                
                EditorUtility.SetDirty(_LanguageAsset);
                _CurrentLoadedAsset = _LanguageAsset;
            }
        }
    }
}