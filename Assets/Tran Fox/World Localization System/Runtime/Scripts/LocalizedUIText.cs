using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace TranFox.LocalizationSystem
{
    [ExecuteInEditMode]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizedUIText : UdonSharpBehaviour
    {
        public string LocalizationKey;
        public LocalizationHandler Handler;

        bool _Cached = false;
        TextMeshProUGUI _CachedTMP;
        Text _CachedText;

        public void UpdateText()
        {
            // Cache text components for use later if needed
            if (!_Cached)
            {
                _CachedTMP = this.GetComponent<TextMeshProUGUI>();
                _CachedText = this.GetComponent<Text>();
                _Cached = true;
            }

            // Check if handler is null (This will only happen if a handler is not in the scene)
            string localizedText = "";
            if (Handler != null)
            {
                localizedText = Handler.GetText(LocalizationKey);
            }
            else
            {
                Debug.Log($"[LocalizedUIText] LocalizationHandler is missing on \"{name}\"!! Returning.");
                return;
            }
            
            // Set text
            if (_CachedTMP != null)
                _CachedTMP.text = localizedText;
            else if (_CachedText != null)
                _CachedText.text = localizedText;
            else
                Debug.Log($"[LocalizedUIText] Couldn't find a sutible text component on \"{name}\" to apply localized text to!");
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        // Auto set LocalizationHandler in editor for users sanity
        public void Reset()
        {
            Handler = Object.FindObjectOfType<LocalizationHandler>(true);
            // UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
        }

#endif

    }
}