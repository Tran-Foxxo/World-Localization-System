using UnityEditor.Callbacks;
using UnityEngine;
using UdonSharpEditor;

namespace TranFox.LocalizationSystem
{
    public static class LocalizationPostProcess
    {

        [PostProcessScene(-1)]
        static void GetStaticText()
        {
            var handler = GameObject.FindObjectOfType<LocalizationHandler>();
            handler.StaticTexts = GameObject.FindObjectsOfType<LocalizedUIText>();

            UdonSharpEditor.UdonSharpEditorUtility.CopyProxyToUdon(handler);
        }

        [PostProcessScene(-1)]
        static void HandlerSetter()
        {
            var handler = GameObject.FindObjectOfType<LocalizationHandler>();
            foreach (var locText in GameObject.FindObjectsOfType<LocalizedUIText>())
            {
                locText.Handler = handler;
                UdonSharpEditor.UdonSharpEditorUtility.CopyProxyToUdon(locText);
            }
        }
    }
}