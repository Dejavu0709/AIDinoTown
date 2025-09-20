using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static partial class ArabicSupportUtils
{
    public static bool IsArabicChar(this char value)
    {
        bool range1 = value >= '\u0600' && value <= '\u06FF';
        bool range2 = value >= '\u0750' && value <= '\u077F';
        bool range3 = value >= '\u08A0' && value <= '\u08FF';
        bool range4 = value >= '\uFB50' && value <= '\uFDFF';
        bool range5 = value >= '\uFE70' && value <= '\uFEFF';
        if (range1 || range2 || range3 || range4 || range5)
            return true;
        return false;
    }

    public static bool IsArabicString(string content)
    {
        for (int i = 0; i < content.Length; i++)
        {
            if (content[i].IsArabicChar( ))
                return true;
        }
        return false;
    }

    public static string FixArabic(ArabicSetting arabicSetting, ref int lineCount)
    {
        // if (arabicSetting.content.Contains("10355084"))
        // {
        //     Debug.Log("");
        // }
        if (arabicSetting.content.Contains("\n"))
            arabicSetting.content = arabicSetting.content.Replace('\n', ' ');
        if (arabicSetting.content.Contains("\r"))
            arabicSetting.content = arabicSetting.content.Replace('\r', ' ');

        var char_array = arabicSetting.content.TrimEnd( ).TrimStart( ).ToCharArray( );

        var words = new List<WordInfo>( );

        char_array.AnalysisSentence(0, char_array.Length - 1, words);
        WordInfo[] array = words.ToArray( );

        List<WordInfo[]> worldLines;
        array.FixeMutileLine(arabicSetting, ref lineCount, out worldLines);

        var concat_string = String.Empty;
        if (!arabicSetting.singleLine && lineCount > 1 && worldLines != null)
        {
            concatBuilder.Length = 0;
            for (int i = 0; i < worldLines.Count; i++)
            {
                worldLines[i].ReverseLine( );
                concatBuilder.Append(worldLines[i].Concat( ));
            }
            concat_string = concatBuilder.ToString( );
        }
        else
        {
            array.ReverseLine( );
            concat_string = array.Concat( );
        }
        var endLine = concat_string[concat_string.Length - 1] == '\n';
        return endLine? concat_string.Remove(concat_string.Length - 1) : concat_string;
    }

    private static string Concat(this WordInfo[] words)
    {
        fixBuilder.Length = 0;
        var tag = string.Empty;
        for (int i = 0; i < words.Length; i++)
        {
            var word = words[i];
            if (!string.IsNullOrEmpty(word.tag))
            {
                if (string.IsNullOrEmpty(tag))
                {
                    tag = word.tag;
                    fixBuilder.Append(tag);
                }
                else if (tag != word.tag)
                {
                    var end_tag = string.Empty;
                    if (AnalysisWithTag(FuncTag, tag, out end_tag))
                    {
                        fixBuilder.Append(end_tag);
                        tag = word.tag;
                        fixBuilder.Append(tag);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(tag))
            {
                var end_tag = string.Empty;
                if (AnalysisWithTag(FuncTag, tag, out end_tag))
                {
                    fixBuilder.Append(end_tag);
                    tag = string.Empty;
                }
            }
            if (word.content.Length>0 &&(int) word.content[0] != 0) //有可能出现ascll 值为0的字符。
            // { }
            // else
                fixBuilder.Append(word.content);

            if (!string.IsNullOrEmpty(tag) && i == words.Length - 1)
            {
                var end_tag = string.Empty;
                if (AnalysisWithTag(FuncTag, tag, out end_tag))
                {
                    fixBuilder.Append(end_tag);
                    tag = string.Empty;
                }
            }
        }
        fixBuilder.Append('\n');
        return fixBuilder.ToString( );
    }

    private static void AnalysisSentence(this char[] char_array, int begin, int over, List<WordInfo> words, string tag = null)
    {
        var temp_b = char_array[begin];
        var temp_c = char_array[over];
        for (int i = begin; i <= over; i++)
        {
            var start_index = i;
            var cur_type = char_array[start_index].GetCharType( );

            var create_word = true;
            if (cur_type == CharType.TAG)
            {
                if (char_array.SplitByTag(start_index, ref i, words))
                    create_word = false;
            }
            else
            {
                create_word = char_array.SplitBySpace(start_index, ref i);
            }

            if (create_word)
            {
                var word = char_array.WordGenerator(start_index, i, tag);
                words.Add(word);
            }
        }
    }
    //特 ：解决表情宽度问题
    private static float GetRegexQuadImage(string content)
    {
        var isMatch = RichText.s_ImageRegex.IsMatch(content);
        if (isMatch)
        {
            var match = RichText.s_ImageRegex.Match(content);
            return float.Parse(match.Groups[2].Value);
        }
        return 0;
    }

    private static WordInfo WordGenerator(this char[] char_array, int start_index, int end_index, string tag)
    {
        var cur_type = char_array[start_index].GetCharType( );

        var copy = new char[end_index - start_index + 1];
        Array.Copy(char_array, start_index, copy, 0, copy.Length);
        if (cur_type == CharType.RTL)
        {
            copy = ArabicFixerTool.FixLine(copy);
        }
        fixBuilder.Length = 0;
        fixBuilder.Append(copy);
        var word = fixBuilder.ToString( );

        var wordInfo = new WordInfo( )
        {
            content = word,
            tag = tag,
            chart_type = cur_type,
            width = GetRegexQuadImage(word)
        };
        return wordInfo;
    }

    private static bool SplitBySpace(this char[] array, int start_index, ref int end_index)
    {
        var i = start_index;
        var cur_type = array[i].GetCharType( );

        for (; i < array.Length; i++)
        {
            var next = ' ';
            if (i + 1 < array.Length) next = array[i + 1];

            var next_type = next.GetCharType( );
            if (next_type != cur_type) break;
        }
        end_index = i;
        return true;
    }

    private static bool SplitByTag(this char[] array, int start_index, ref int end_index, List<WordInfo> words)
    {
        end_index = start_index;
        var cur_ch = array[start_index];

        char analysisTag;

        if (cur_ch == '<')
        {
            var close_index = array.IndexOf('>', start_index);
            if (close_index != -1)
            {
                var start_tag = array.SubString(start_index, close_index);

                var end_tag = string.Empty;
                if (AnalysisWithTag(FuncTag, start_tag, out end_tag))
                {
                    var end_tag_index = array.IndexOf(end_tag, start_index);
                    if (end_tag_index != -1)
                    {
                        var temp_words = new List<WordInfo>( );
                        array.AnalysisSentence(close_index + 1, end_tag_index - 1, temp_words, start_tag);

                        words.AddRange(temp_words);
                        end_index = end_tag_index + end_tag.Length - 1;
                        return true;
                    }
                }
                end_index = close_index;
            }
        }
        else if (AnalysisWithTag<char>(CloseTag, cur_ch, out analysisTag))
        {
            array[start_index] = analysisTag;
        }
        return false;
    }

    private static void ReverseLine(this WordInfo[] array)
    {
        int start_rtl = -1, start_ltr = -1, start_tag = -1;

        for (int i = 0; i < array.Length; i++)
        {
            var current = array[i];

            if (current.chart_type != CharType.LTR && current.chart_type != CharType.RTL &&
                current.chart_type != CharType.TAG && i != array.Length - 1)
            {
                continue;
            }
            //处理arabic
            if (current.chart_type == CharType.RTL && start_rtl == -1 && i != array.Length - 1) start_rtl = i;
            if (current.chart_type == CharType.LTR && start_ltr == -1 && i != array.Length - 1) start_ltr = i;
            if (current.chart_type == CharType.TAG && start_tag == -1 && i != array.Length - 1) start_tag = i;

            if (start_rtl != -1 && current.chart_type != CharType.RTL)
            {
                Array.Reverse(array, start_rtl, i - start_rtl);
                MoveToFront(array, start_rtl, i - 1);
                start_rtl = -1;
            }

            if (start_ltr != -1 && current.chart_type != CharType.LTR)
            {
                MoveToFront(array, start_ltr, i - 1);
                start_ltr = -1;
            }

            if (start_tag != -1 && current.chart_type != CharType.TAG)
            {
                MoveToFront(array, start_tag, i - 1);
                start_tag = -1;
            }

            if (i == array.Length - 1)
            {
                if (current.chart_type == CharType.RTL)
                {
                    if (start_rtl != -1)
                    {
                        Array.Reverse(array, start_rtl, i - start_rtl + 1);
                    }
                    MoveToFront(array, start_rtl != -1 ? start_rtl : i, i);
                }
                else if (current.chart_type == CharType.LTR)
                {
                    MoveToFront(array, start_ltr != -1 ? start_ltr : i, i);
                }
                else if (current.chart_type == CharType.TAG)
                {
                    MoveToFront(array, start_tag != -1 ? start_tag : i, i);
                }
                else if (current.chart_type == CharType.PTT)
                {
                    MoveToFront(array, i, i);
                }
                start_rtl = start_ltr = start_tag = -1;
            }
        }
    }

    private static void MoveToFront<T>(T[] array, int index, int end)
    {
        int i = index;
        while (i <= end)
        {
            var temp = array[end];
            Array.Copy(array, 0, array, 1, end);
            array[0] = temp;
            i++;
        }
    }

    private static CharType GetCharType(this char value)
    {
        if (IsArabicChar(value))
            return CharType.RTL;
        else if (value == ' ')
            return CharType.SPL;
        else if (CloseTag.Contains(value))
            return CharType.TAG;
        else if (char.IsPunctuation(value) && value != '%')
            return CharType.PTT;
        else
            return CharType.LTR;
    }

    private static bool AnalysisWithTag<T>(T[] array, T source, out T target)
    {
        var index = -1;
        if (source is string)
        {
            var temp = source as string;
            if (temp.Contains('='))
            {
                temp = temp.Substring(0, temp.IndexOf('=') + 1);
            }
            index = Array.FindIndex(array, tag => tag.Equals(temp));
        }
        else
            index = Array.FindIndex(array, tag => tag.Equals(source));

        target = default(T);
        if (index == -1) return false;
        index = index % 2 == 0 ? index + 1 : index - 1;
        target = array[index];
        return true;
    }

    private static int IndexOf(this char[] array, char source, int start_index)
    {
        for (int i = start_index; i < array.Length; i++)
        {
            if (array[i] == source)
                return i;
        }
        return -1;
    }

    private static int IndexOf(this char[] array, string source, int start_index)
    {
        for (int i = start_index; i < array.Length; i++)
        {
            for (int j = 0; j < source.Length; j++)
            {
                if (source[j] != array[i + j])
                {
                    break;
                }
                if (j == source.Length - 1)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private static List<int> IndexOf(this string content, char split)
    {
        var index = content.IndexOf(split);
        List<int> temp = null;
        if (index != -1)
        {
            if (temp == null) temp = new List<int>( );
            temp.Add(index);
        }
        return temp;
    }

    private static string SubString(this char[] array, int start_index, int end_index)
    {
        fixBuilder.Length = 0;
        for (int i = start_index; i <= end_index; i++)
        {
            fixBuilder.Append(array[i]);
        }
        return fixBuilder.ToString( );
    }

    private static Vector2 GetViewFiexedSize(ArabicSetting settings)
    {
        var height = settings.rectSize.y * settings.pixelsPerUnit / settings.textSetting.scaleFactor;
        var width = settings.rectSize.x * settings.pixelsPerUnit / settings.textSetting.scaleFactor;
        return new Vector2(width, height);
    }

    private static float GetGlyphWidth(this WordInfo word, TextGenerationSettings settings, Font font)
    {
        var glyphWidth = word.width;
        if (glyphWidth != 0) return glyphWidth;

        for (int i = 0; i < word.content.Length; i++)
        {
            var ch = word.content[i];
            CharacterInfo info = new CharacterInfo( );
            if (!font.GetCharacterInfo(ch, out info, font.fontSize, settings.fontStyle))
            {
                //TODO 优化，提前获取
                font.RequestCharactersInTexture(word.content, font.fontSize, settings.fontStyle);
                font.GetCharacterInfo(ch, out info, font.fontSize, settings.fontStyle);
            }

            var fontScale = (float) settings.fontSize / font.fontSize;
            glyphWidth += info.advance * fontScale;
        }

        word.width = glyphWidth;
        return glyphWidth;
    }

    private static void FixeMutileLine(this WordInfo[] words, ArabicSetting arabicSetting, ref int line, out List<WordInfo[]> worldLines)
    {
        worldLines = null;

        var fixedSize = GetViewFiexedSize(arabicSetting);
        for (int i = arabicSetting.maxFontSize; i >= arabicSetting.minFontSize; --i)
        {
            arabicSetting.textSetting.fontSize = i;
            if (words.WrapText(i == arabicSetting.minFontSize, fixedSize, arabicSetting, ref line, out worldLines))
                break;
        }
    }

    private static bool WrapText(this WordInfo[] words, bool isEnd, Vector2 fixedSize, ArabicSetting arabicSetting, ref int line, out List<WordInfo[]> worldLines)
    {
        worldLines = null;
        line = 0;

        var finalLineHeight = (arabicSetting.textSetting.fontSize + arabicSetting.spacing);
        if (arabicSetting.preferredWidth < 1 || arabicSetting.preferredHeight < 1 || finalLineHeight < 1)
        {
            arabicSetting.content = "";
            return false;
        }

        var maxLineCount = 10000;
        maxLineCount = Mathf.FloorToInt(Mathf.Min(maxLineCount, fixedSize.y / finalLineHeight) + 0.01f);
        if (maxLineCount == 0)
        {
            arabicSetting.content = "";
            return false;
        }

        if (fixedSize.x <= 0 || fixedSize.y <= 0)
        {
            arabicSetting.content = "";
            return false;
        }

        var remainingWidth = fixedSize.x;
        var lineCount = 0;
        for (int i = 0; i < words.Length; i++)
        {
            var word_w = words[i].GetGlyphWidth(arabicSetting.textSetting, arabicSetting.font);
            var word_super = word_w >= fixedSize.x;
            remainingWidth -= word_w;
            if (remainingWidth <= 0 || i == words.Length - 1)
            {
                if (remainingWidth < 0 && !word_super) i--;

                remainingWidth = fixedSize.x;
                lineCount++;
                if (lineCount >= words.Length)
                {
                    // Debug.LogError("");
                    return false; //出现单词超过显示宽度情况，直接返回
                }
            }
        }
        if (lineCount > maxLineCount && !isEnd) return false;

        line = lineCount;
        remainingWidth = fixedSize.x;
        if (line > 1)
        {
            worldLines = new List<WordInfo[]>( );
            var start = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var word_w = words[i].GetGlyphWidth(arabicSetting.textSetting, arabicSetting.font);
                var word_super = word_w >= fixedSize.x;
                remainingWidth -= word_w;
                if (remainingWidth <= 0 || i == words.Length - 1)
                {
                    if (remainingWidth < 0 && !word_super) i--;

                    if (i < 0) break;

                    remainingWidth = fixedSize.x;
                    if (i - start > 0)
                    {
                        try
                        {
                            var array = new WordInfo[i - start + 1];
                            Array.Copy(words, start, array, 0, array.Length);
                            worldLines.Add(array);
                        }
                        catch (System.Exception)
                        {
                            throw new Exception("generate font is wrong: i:" + i + " start:" + start + " words:" + words.Length + "!");
                        }

                    }
                    start = i + 1;
                }
            }
        }
        return true;
    }

}