using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GnomiumTranslation
{
    public static class TranslationCore
    {
        public static Dictionary<string, string> Dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static List<KeyValuePair<string, string>> SortedDict = new List<KeyValuePair<string, string>>();
        static TMP_FontAsset ChineseFallback;
        static bool fontInitialized = false;

        static TranslationCore()
        {
            InitDict();
        }

        public static void InitDict()
        {
            try
            {
                // 使用相对于插件 DLL 的相对路径
                string dllDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string jsonPath = System.IO.Path.Combine(dllDir, "dictionary.json");
                if (System.IO.File.Exists(jsonPath))
                {
                    string json = System.IO.File.ReadAllText(jsonPath, System.Text.Encoding.UTF8);
                    
                    // 手动解析 JSON 避免引入 Newtonsoft.Json 依赖（或如果可用则使用）
                    // 为了最大兼容性，我们使用一个简单的正则解析器或引入 Newtonsoft
                    // 这里假设我们引用了 Newtonsoft.Json
                    var loadedDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    if (loadedDict != null)
                    {
                        foreach (var kvp in loadedDict)
                        {
                            Dict[kvp.Key] = kvp.Value;
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[GnomiumTranslation] Dictionary file not found at " + jsonPath);
                }

                SortedDict = new List<KeyValuePair<string, string>>(Dict);
                SortedDict.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[GnomiumTranslation] Error loading dictionary: " + ex.ToString());
            }
        }

        public static string GetDayText(string numText)
        {
            try
            {
                // 使用反射找到 PlayerController 类型，避免硬引用导致编译失败
                Type playerControllerType = Type.GetType("PlayerController, Assembly-CSharp");
                if (playerControllerType != null)
                {
                    var pc = UnityEngine.Object.FindFirstObjectByType(playerControllerType);
                    if (pc != null)
                    {
                        var field = playerControllerType.GetField("txt_day", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (field != null)
                        {
                            var txtDay = field.GetValue(pc) as TMP_Text;
                            if (txtDay != null)
                            {
                                txtDay.textWrappingMode = TextWrappingModes.NoWrap;
                                txtDay.alignment = TextAlignmentOptions.Center;
                                txtDay.overflowMode = TextOverflowModes.Overflow;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[GnomiumTranslation] Error in GetDayText: " + ex.ToString());
            }
            return "第" + numText + "天";
        }

        public static string Translate(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // 如果包含中文字符，说明已经翻译过，直接返回避免重复处理导致 \n 丢失
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"\p{IsCJKUnifiedIdeographs}"))
            {
                return text.Replace("！", "! ").Replace("，", ", ").Replace("。", ". ").Replace("？", "? ").Replace("：", ": ").Replace("（", "(").Replace("）", ")");
            }

            string cleanText = text.Trim('\u200B', ' ', '\n', '\r', '\t');
            string noNewlineText = cleanText.Replace("\n", " ").Replace("\r", "").Replace("  ", " ");
            
            string translated;
            if (Dict.TryGetValue(text, out translated)) return translated;
            if (Dict.TryGetValue(cleanText, out translated)) return translated;
            if (Dict.TryGetValue(noNewlineText, out translated)) return translated;
            
            string translatedUpper;
            if (Dict.TryGetValue(text.ToUpper(), out translatedUpper)) return translatedUpper;
            if (Dict.TryGetValue(noNewlineText.ToUpper(), out translatedUpper)) return translatedUpper;
            
            string titleCaseText = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(noNewlineText.ToLower());
            
            string translatedTitle;
            if (Dict.TryGetValue(titleCaseText, out translatedTitle)) return translatedTitle;

            // Handle dynamic keybinding prefixes like "[E] REQUEST EXTRACTION"
            if (noNewlineText.StartsWith("["))
            {
                int closeBracketIndex = noNewlineText.IndexOf(']');
                if (closeBracketIndex != -1 && closeBracketIndex < noNewlineText.Length - 1)
                {
                    string prefix = noNewlineText.Substring(0, closeBracketIndex + 1); // e.g. "[E]"
                    string restOfText = noNewlineText.Substring(closeBracketIndex + 1).TrimStart();
                    
                    string translatedRest;
                    if (Dict.TryGetValue(restOfText, out translatedRest) || 
                        Dict.TryGetValue(restOfText.ToUpper(), out translatedRest) || 
                        Dict.TryGetValue(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(restOfText.ToLower()), out translatedRest))
                    {
                        return prefix + " " + translatedRest;
                    }
                }
            }

            if (noNewlineText.Contains("This is a demo version of Burglin' Gnomes"))
                return "这是《Burglin' Gnomes》的试玩版本，包含的内容有限。\n汉化:ASwave，所有出现汉化不完善的问题请截图发送至邮箱：lhl286256218@gmail.com\n微信：aswave0 抖音：aswavecodestorm0\nDiscord： <link=\"https://discord.gg/893spUGPU\"><color=#0000FF><u>https://discord.gg/893spUGPU</u></color></link>";
            
            if (noNewlineText.Contains("This view also functions as your inventory"))
                return "此视图也可作为你的物品栏，你可以通过将物品拖出槽位来丢弃它们。";

            if (noNewlineText.Contains("Now you must find a way inside"))
                return "现在你必须找到进去的方法。窗户是个不错的选择！";

            if (noNewlineText.Contains("You can throw, smash and"))
                return "你可以使用双手（包括其他侏儒）投掷、粉碎以及操纵各种物品来为你创造优势。";

            // 特殊处理带富文本标签的长句（直接替换避免标签丢失或被部分替换污染）
            if (noNewlineText.Contains("Clonk is") && noNewlineText.Contains("containing metal"))
                return "金属材料可以从含有金属的物品中<u>收集</u>。";

            if (noNewlineText.Contains("are") && noNewlineText.Contains("made out of fabric, linen"))
                return "破布可以从由织物、亚麻和其他此类材料制成的物品中<u>收集</u>。";

            if (noNewlineText.Contains("is") && noNewlineText.Contains("containing naturally occurring"))
                return "杂石木可以从含有天然资源的物品中<u>收集</u>。这些包括：";

            if (noNewlineText.Contains("Gathering resources is done by") && noNewlineText.Contains("stolen"))
                return "收集资源是通过<u>偷窃</u>物品来完成的。成功偷取物品后，<u>Bob</u> 会将其运回你的小屋进行处理。";

            // 动态截断翻译
            if (noNewlineText.StartsWith("Clonk ", StringComparison.OrdinalIgnoreCase)) {
                return "金属材料" + noNewlineText.Substring(5);
            }
            if (noNewlineText.StartsWith("Grabble ", StringComparison.OrdinalIgnoreCase)) {
                return "杂石木" + noNewlineText.Substring(7);
            }
            if (noNewlineText.StartsWith("Scrags ", StringComparison.OrdinalIgnoreCase)) {
                return "破布" + noNewlineText.Substring(6);
            }
            if (noNewlineText.StartsWith("Plasto ", StringComparison.OrdinalIgnoreCase)) {
                return "塑料" + noNewlineText.Substring(6);
            }
            if (noNewlineText.StartsWith("Gnomium ", StringComparison.OrdinalIgnoreCase)) {
                return "地精矿" + noNewlineText.Substring(7);
            }
            if (noNewlineText.StartsWith("Fraggles ", StringComparison.OrdinalIgnoreCase)) {
                return "矿渣" + noNewlineText.Substring(8);
            }

            // 任务字符串动态替换
            if (noNewlineText.StartsWith("Day ", StringComparison.OrdinalIgnoreCase) && noNewlineText.Length > 4) {
                return "第" + noNewlineText.Substring(4).Trim() + "天";
            } else if (noNewlineText.StartsWith("Steal an item from random category", StringComparison.OrdinalIgnoreCase)) {
                return "从随机类别偷取一件物品";
            } else if (noNewlineText.StartsWith("Steal a random item", StringComparison.OrdinalIgnoreCase)) {
                return "随机偷取一件物品";
            } else if (noNewlineText.StartsWith("Steal any item from RANDOM room", StringComparison.OrdinalIgnoreCase)) {
                return "从随机房间偷取任意物品";
            } else if (noNewlineText.StartsWith("Steal any item from ", StringComparison.OrdinalIgnoreCase)) {
                string room = noNewlineText.Substring("Steal any item from ".Length);
                return "从 " + Translate(room) + " 偷取任意物品";
            } else if (noNewlineText.Equals("Steal any item", StringComparison.OrdinalIgnoreCase)) {
                return "偷取任意物品";
            } else if (noNewlineText.StartsWith("Steal ", StringComparison.OrdinalIgnoreCase)) {
                string item = noNewlineText.Substring(6);
                return "偷取 " + Translate(item);
            } else if (noNewlineText.StartsWith("Gather ", StringComparison.OrdinalIgnoreCase)) {
                string item = noNewlineText.Substring(7);
                return "收集 " + Translate(item);
            } else if (noNewlineText.StartsWith("Break ", StringComparison.OrdinalIgnoreCase)) {
                string item = noNewlineText.Substring(6);
                return "破坏 " + Translate(item);
            } else if (noNewlineText.StartsWith("Explode ", StringComparison.OrdinalIgnoreCase)) {
                string item = noNewlineText.Substring(8);
                return "炸毁 " + Translate(item);
            } else if (noNewlineText.StartsWith("Shoot ", StringComparison.OrdinalIgnoreCase)) {
                string item = noNewlineText.Substring(6);
                return "射击 " + Translate(item);
            } else if (noNewlineText.StartsWith("Stab ", StringComparison.OrdinalIgnoreCase)) {
                string item = noNewlineText.Substring(5);
                return "刺击 " + Translate(item);
            } else if (noNewlineText.StartsWith("Tase ", StringComparison.OrdinalIgnoreCase)) {
                string item = noNewlineText.Substring(5);
                return "电击 " + Translate(item);
            } else if (noNewlineText.StartsWith("Kill ", StringComparison.OrdinalIgnoreCase)) {
                string item = noNewlineText.Substring(5);
                return "杀死 " + Translate(item);
            }

            foreach (var kvp in SortedDict)
            {
                if (kvp.Key.Length > 4 && noNewlineText.Contains(kvp.Key))
                {
                    noNewlineText = noNewlineText.Replace(kvp.Key, kvp.Value);
                }
            }

            // 兜底：将全角标点符号替换为半角，防止游戏自带字体不支持全角标点而出现方块字
            noNewlineText = noNewlineText.Replace("！", "! ").Replace("，", ", ").Replace("。", ". ").Replace("？", "? ").Replace("：", ": ").Replace("（", "(").Replace("）", ")");

            return noNewlineText;
        }

        public static void OnEnableTMPText(TMP_Text textInstance)
        {
            if (textInstance == null) return;

            // 字体修复
            if (!fontInitialized)
            {
                try
                {
                    string dllDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    string fontPath = System.IO.Path.Combine(dllDir, "songti.ttf");
                    ChineseFallback = TMP_FontAsset.CreateFontAsset(
                        fontPath, 
                        0, 
                        90, 
                        9, 
                        UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 
                        1024, 
                        1024
                    );
                    
                    if (ChineseFallback != null)
                    {
                        ChineseFallback.name = "ChineseFallbackFont";
                        
                        if (TMP_Settings.fallbackFontAssets == null)
                        {
                            TMP_Settings.fallbackFontAssets = new System.Collections.Generic.List<TMP_FontAsset>();
                        }
                        
                        if (!TMP_Settings.fallbackFontAssets.Contains(ChineseFallback))
                        {
                            TMP_Settings.fallbackFontAssets.Add(ChineseFallback);
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("CreateFontAsset returned null.");
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError("Font creation failed: " + ex.ToString());
                }
                fontInitialized = true;
            }

            if (textInstance.font != null && ChineseFallback != null)
            {
                if (textInstance.font.fallbackFontAssetTable == null)
                    textInstance.font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                if (!textInstance.font.fallbackFontAssetTable.Contains(ChineseFallback))
                {
                    textInstance.font.fallbackFontAssetTable.Add(ChineseFallback);
                    textInstance.SetAllDirty();
                }
            }

            // 文本翻译
            if (!string.IsNullOrEmpty(textInstance.text))
            {
                string original = textInstance.text;
                string translated = Translate(original);
                if (original != translated)
                {
                    textInstance.text = translated;
                    
                    if (translated.Contains("<link=\"https://discord.gg/893spUGPU\">"))
                    {
                        if (textInstance.gameObject.GetComponent<GnomiumLinkHandler>() == null)
                        {
                            textInstance.gameObject.AddComponent<GnomiumLinkHandler>();
                        }
                    }

                    // 移除之前添加的硬编码大小修改（已通过富文本处理）
                }
            }
        }
        
        public static void OnEnableText(Text textInstance)
        {
            if (textInstance == null) return;
            if (!string.IsNullOrEmpty(textInstance.text))
            {
                string original = textInstance.text;
                string translated = Translate(original);
                if (original != translated)
                {
                    textInstance.text = translated;
                }
            }
        }
        
        public static void OnEnableUIElementsText(object textInstance)
        {
            // Dummy method for UIElements to prevent IL verification errors
            // UIElements TextElement usually doesn't use OnEnable anyway
        }
        
        public static string GetTranslatedString(string original)
        {
            return Translate(original);
        }
    }

    public class GnomiumLinkHandler : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            var text = GetComponent<TMP_Text>();
            if (text != null)
            {
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
                if (linkIndex != -1)
                {
                    TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
                    Application.OpenURL(linkInfo.GetLinkID());
                }
            }
        }
    }
}
