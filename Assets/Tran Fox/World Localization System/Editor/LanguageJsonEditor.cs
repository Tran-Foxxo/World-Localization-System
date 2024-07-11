using System;
using System.Collections.Generic;
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
        string _CurrentValueContents;
        DataDictionary _LoadedLanguage;

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
                    _LoadedLanguage = null;
                    _SelectedKeySearchIndex = 0;
                }
                else
                {
                    _LanguageAsset = _CurrentLoadedAsset;
                }
            }

            if (_CurrentLoadedAsset == null) return;

            if (_LoadedLanguage == null)
            {
                if (VRCJson.TryDeserializeFromJson(_CurrentLoadedAsset.text, out DataToken result) == true)
                {
                    _LoadedLanguage = result.DataDictionary;
                }
                else
                {
                    GUILayout.Label($"JSON IS INVALID!!", EditorStyles.boldLabel);
                    return;
                }
            }

            EditorGUILayout.Space(10);

            // Get keys
            List<string> stringKeys = new List<string>();
            var dictKeys = _LoadedLanguage.GetKeys();
            for (int i = 0; i < dictKeys.Count; i++)
            {
                var dictKey = dictKeys[i];

                if (dictKey.TokenType == TokenType.String)
                    stringKeys.Add(dictKey.String);
            }

            _KeySearch = EditorGUILayout.TextField("Key Search: ", _KeySearch);

            List<string> keysAfterSearch = new List<string>();
            foreach (string key in stringKeys)
            {
                if (key.Contains(_KeySearch, System.StringComparison.CurrentCultureIgnoreCase))
                    if (_LoadedLanguage[key].TokenType == TokenType.String)
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
            if (!stringKeys.Contains(_KeySearch) && _KeySearch != "")
            {
                if (GUILayout.Button($"Add Key \"{_KeySearch}\""))
                    _LoadedLanguage.Add(_KeySearch, "");
            }
            if (keysAfterSearch.Count == 0) { return; }
            
            // Get selected key
            string currentKey = keysAfterSearch[_SelectedKeySearchIndex];

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
                    _LoadedLanguage.Remove(currentKey); 
                }
            }

            EditorGUILayout.Space(10);

            // Edtinging box
            GUILayout.Label($"KEY \"{currentKey}\":", EditorStyles.boldLabel);
            _LoadedLanguage[currentKey] = EditorGUILayout.TextArea(_LoadedLanguage[currentKey].String);

            // Saving
            if (GUILayout.Button("Save Language"))
            {
                // So for SOME reason, deleting a key leaves a blank "null" value and keeps the key.
                // So instead we will manually only save string keys & values.
                DataDictionary fixedDictionary = new DataDictionary();
                var keys = _LoadedLanguage.GetKeys();
                var values = _LoadedLanguage.GetValues();
                for (int i = 0; i < _LoadedLanguage.Count; i++)
                {
                    if (keys[i].TokenType == TokenType.String && values[i].TokenType == TokenType.String)
                    {
                        string keyString = keys[i].String;
                        string valueString = values[i].String;

                        // Remove \r's this may cause unexpected behaviour with TMP actually supporting them
                        // If you really need \r for some reason just comment out this line I guess.
                        valueString = valueString.Replace("\r", "");

                        fixedDictionary.Add(keyString, valueString);
                    }
                }

                var outToken = new DataToken(); 
                bool worked = VRCJson.TrySerializeToJson(fixedDictionary, JsonExportType.Beautify, out outToken);

                if (!worked) { Debug.LogError("[LanguageJsonEditor] UNABLE TO SAVE FOR SOME REASON???"); return; }

                System.IO.File.WriteAllText(AssetDatabase.GetAssetPath(_LanguageAsset), outToken.String);
                Debug.Log($"[LanguageJsonEditor] Saved {_LanguageAsset.name}!");
                
                EditorUtility.SetDirty(_LanguageAsset);
                _CurrentLoadedAsset = _LanguageAsset;
            }
        }
    }
}