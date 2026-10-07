using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(TeleFix.Plugin), TeleFix.MyPluginInfo.PLUGIN_NAME, TeleFix.MyPluginInfo.PLUGIN_VERSION, TeleFix.MyPluginInfo.PLUGIN_DEV)]
[assembly: MelonGame("Iron Nest", "Iron Nest Heavy Turret Simulator")]

/*
NUMBERS FOR LINE/CHAR COUNTS
- Primary:   Chars per line: 66 | Total lines: 195 | Max Chars = 12,870
- Secondary: Chars per line: 67 | Total lines: 210 | Max Chars = 14,070
*/

namespace TeleFix
{
    public static class MyPluginInfo {
        public const string PLUGIN_NAME = "Teleprinter Fix";
        public const string PLUGIN_VERSION = "1.0.0";
        public const string PLUGIN_DEV = "emberlantern";
    }
    
    public class Plugin : MelonMod
    {
        // Choosing numbers lower than max to give wiggle room for future message lengths
        internal const int MAX_CHARS = 14000;
        internal const int NUM_REMAINING = 12000;
        internal static MelonPreferences_Entry<bool> _verboseLogging;
        internal static void Log(string msg, bool verbose = false)
        {
            if (verbose && _verboseLogging.Value) MelonLogger.Msg($"[{MyPluginInfo.PLUGIN_NAME} - Verbose] {msg}");
            if (!verbose) MelonLogger.Msg($"[{MyPluginInfo.PLUGIN_NAME}] {msg}");
        }

        public override void OnInitializeMelon()
        {
            // Plugin startup logic
            MelonLogger.Msg($"Plugin {MyPluginInfo.PLUGIN_NAME}_{MyPluginInfo.PLUGIN_VERSION} is loaded!");

            MelonPreferences_Category category = MelonPreferences.CreateCategory("TeleFix", "Teleprinter Fix");
            _verboseLogging = category.CreateEntry(
                "VerboseLogging",
                false,
                "Verbose Logging",
                "Logs every Teleprinter Fix action/check. Use for debug purposes unless you like big logs."
            );
            
            if (_verboseLogging.Value) 
                Log("Verbose Logging Active", true);
        }
    }
    [HarmonyPatch(typeof(Teleprinter), nameof(Teleprinter.TypeChunkTopDown))]
    static class TruncatePrimaryPatch
    {
        // NOTE: This method assumes that the text will NOT be a massive block o' nearly maxed-out text
        //   In that case, I believe that the end is just chopped off a little more at the end, but it shouldn't be a big deal
        //   For this printer, 12870 is the max number of characters, so having 12000 remaining at worst means 
        //   that about 181 of 195 lines remain, losing about 30ish lines. Math: 12000/67 = 181ish
        //   As long as the text isn't over an average density of roughly 61 characters per line (of 66), everything is seamless.
        static bool Prefix(Teleprinter __instance)
        {
            if (__instance._currentFullRich.CompareTo("") == 0) 
                return true;
                
            // If the character count is bigger than the max count, truncate the string down to the NUM_REMAINING value
            TMP_TextInfo info = __instance._tmp.textInfo;
            if (info.characterCount > Plugin.MAX_CHARS)
            {
                Plugin.Log("TRUNCATING", true);

                // Determine where in the string to slice (slice after a newline)
                int cutStringLength = info.characterCount - Plugin.NUM_REMAINING - 1;
                while (cutStringLength > 0 && info.characterInfo[cutStringLength].character != '\n')
                {
                    cutStringLength--;
                }

                if (cutStringLength == 0)
                {
                    Plugin.Log("Skipping Truncation, reached end of string", true);
                    return true;
                }

                // Update all the values:
                //   currentFullRich is truncated from just after the last newline to chop up until the end
                //   currentRevealedCharIndex is updated to fit the new string length
                //   prevLineNum is adjusted to move the text down to the pinter line 
                __instance._currentFullRich = __instance._currentFullRich[info.characterInfo[cutStringLength].index..];
                __instance._currentRevealedCharIndex = info.characterCount - (cutStringLength + 1);
                int moveLines = info.characterInfo[cutStringLength].lineNumber + 1;
                __instance._prevLineNum -= moveLines;

                // Adjust the paper and cursor to make the text appear in the right place
                Vector3 paperPos = __instance.paperTransform.localPosition;
                Vector3 cursorPos = __instance.typerCursor.position;
                float delta = __instance.GetLineVerticalDeltaCached(info, 0, moveLines);
                paperPos.y += delta;
                __instance.paperTransform.localPosition = paperPos;
                __instance.typerCursor.position = cursorPos;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Teleprinter), nameof(Teleprinter.TypeChunkBottomUp))]
    static class TruncateSecondaryPatch
    {
        // NOTE: This method assumes that the text will NOT be a massive block o' nearly maxed-out text
        //   In that case, I believe that the end is just chopped off a little more at the end, but it shouldn't be a big deal
        //   For this printer, 14070 is the max number of characters, so having 12000 remaining at worst means 
        //   that about 179 of 210 lines remain, losing about 25ish lines. Math: 12000/66 = 179ish
        //   As long as the text isn't over an average density of roughly 57 characters per line (of 66), everything is seamless.
        static bool Prefix(Teleprinter __instance)
        {
            if (__instance._currentFullRich.CompareTo("") == 0) 
                return true;
                
            // If the character count is bigger than the max count, truncate the string down to the NUM_REMAINING value
            TMP_TextInfo info = __instance._tmp.textInfo;
            if (info.characterCount > Plugin.MAX_CHARS)
            {
                Plugin.Log("TRUNCATING", true);

                // Determine where in the string to slice (slice after a newline)
                int cutStringLength = Plugin.NUM_REMAINING;
                while (cutStringLength < info.characterCount && info.characterInfo[cutStringLength].character != '\n')
                {
                    cutStringLength++;
                }
                

                if (cutStringLength == info.characterCount)
                {
                    Plugin.Log("Skipping Truncation, reached end of string", true);
                    return true;
                }

                // Update all the string and associated char index:
                //   currentFullRich is truncated from the start to the last newline we want to keep
                //   currentRevealedCharIndex is updated to fit the new string length
                __instance._currentFullRich = __instance._currentFullRich[..info.characterInfo[cutStringLength].index];
                __instance._currentRevealedCharIndex = cutStringLength;
                
                // Adjust the paper and cursor to make the text appear in the right place
                Vector3 paperPos = __instance.paperTransform.localPosition;
                Vector3 cursorPos = __instance.typerCursor.position;
                float delta = __instance.GetLineVerticalDeltaCached(info, info.lineCount - 1, info.characterInfo[cutStringLength].lineNumber);
                paperPos.y += delta;
                __instance.paperTransform.localPosition = paperPos;
                __instance.typerCursor.position = cursorPos;
            }

            return true;
        }
    }
}