using System;
using System.Runtime.Remoting.Messaging;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Serialization.OdinSerializer.Utilities;

namespace TranFox.LocalizationSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizationHandler : UdonSharpBehaviour
    {
        [Header("Hover over for tooltips if you aren't sure what all of these do!")]
        [Space(10)]

        [Tooltip("This language is loaded by default when the component is enabled")]
        public TextAsset PrimaryLanguage;

        [Tooltip("Language used to fall back on when a localization key is missing (Not required)")]
        public TextAsset FallbackLanguage;

        [Header("Debug Settings")]
        [Tooltip("If the localization key should be shown after fallback text")]
        public bool ShowKeyInFallbackText = false;
        [Tooltip("If missing localization keys should be logged")]
        public bool LogMissingLocalizationKeys = true;
        [Tooltip("Only show localization keys instead of the text (This overrides all other settings)")]
        public bool OnlyShowKeyName = false;

        // Setup with scene postprocess
        [HideInInspector] public LocalizedUIText[] StaticTexts;

        // Private
        DataDictionary _CurrentLang = new DataDictionary();
        DataDictionary _FallbackLang = new DataDictionary();

        // Setup and update with inital language.
        void Start()
        {
            if (PrimaryLanguage) SetupPrimaryLanguage(PrimaryLanguage.text);
            else Debug.Log("[LocalizationHandler] PrimaryLanguage is NULL! Did you forget to add it?");

            if (FallbackLanguage) SetupFallbackLanguage(FallbackLanguage.text);
            //else Debug.Log("[LocalizationHandler] FallbackLanguage is NULL! Did you forget to add it?");

            UpdateStaticTexts();
        }

        // Public method to easily switch out the current language
        public void SetupPrimaryLanguage(string json)
        {
            Debug.Log("[LocalizationHandler] Setting up primary language...");
            _CurrentLang = ParseLang(json);
        }

        // Ditto but for the fallback in case it ever needs to be changed for some reason
        public void SetupFallbackLanguage(string json)
        {
            Debug.Log("[LocalizationHandler] Setting up fallback language...");
            _FallbackLang = ParseLang(json);
        }

        // Tries to deserialize json data, returns an empty DataDictionary and logs if it fails.
        public static DataDictionary ParseLang(string text)
        {
            Debug.Log("[LocalizationHandler] Parsing language...");
            if (VRCJson.TryDeserializeFromJson(text, out DataToken result) == true)
            {
                return result.DataDictionary;
            }
            else
            {
                Debug.Log($"[LocalizationHandler] !! FAILED TO DESERIALIZE LANGUAGE JSON ALL KEYS WILL BE MISSING !!");
                return new DataDictionary();
            }
        }

        // Updates any static texts in the scene
        public void UpdateStaticTexts()
        {
            Debug.Log($"[LocalizationHandler] Setting up {StaticTexts.Length} static text components.");
            foreach (LocalizedUIText staticText in StaticTexts)
            {
                staticText.UpdateText();
            }
        }

        public string GetText(string key)
        {
            // Check for only showing keys
            if (OnlyShowKeyName)
                return key;

            // Check our current lang for key
            if (_CurrentLang.ContainsKey(key))
                return _CurrentLang[key].String;

            if (LogMissingLocalizationKeys)
                Debug.Log($"[LocalizationHandler] Key \"{key}\" doesn't exist in current language!");

            // Check fallback for a key
            if (_FallbackLang.ContainsKey(key))
            {
                string fallbackText = _FallbackLang[key].String;
                
                if (ShowKeyInFallbackText)
                    fallbackText += $"\n[Missing Key \"{key}\"]";

                return fallbackText;
            }
            
            // If it doesn't exist anywhere just give up lol
            return $"[Missing Key \"{key}\"]";
        }
    }
}