using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using NexgenDragon;
[AddComponentMenu("UI/Extensions/ShrinkText")]
public class ShrinkText : Text
{
    /// <summary>
    /// 当前可见的文字行数
    /// </summary>
    [SerializeField]
    private bool _singleLine = false;
    [SerializeField]
    private Color _glowColor = Color.black;
    [SerializeField]
    private float _glowSize = 1;
    ///
    [SerializeField]
    private Vector2 _shadowOffset = Vector2.zero;
    [SerializeField]
    private Color _shadowColor = Color.black;

    [SerializeField]
    private bool _useAutoArabic = true;

    public Color OutlineColor
    {
        get
        {
            return _glowColor;
        }
        set
        {
            _glowColor = value;
        }
    }

    public float OutlineSize
    {
        get
        {
            return _glowSize;
        }
        set
        {
            _glowSize = value;
        }
    }

    public Color ShadowColor
    {
        get
        {
            return _shadowColor;
        }
        set
        {
            _shadowColor = value;
        }
    }

    public Vector2 ShadowOffset
    {
        get
        {
            return _shadowOffset;
        }
        set
        {
            _shadowOffset = value;
        }
    }

    public int VisibleLines
    {
        get;
        private set;
    }

    string arabic_text = string.Empty;
    public void _UseArabicSettings(TextGenerationSettings settings)
    {
        if (string.IsNullOrEmpty(text)) return;

        var lineCount = 1;

        ArabicSupportUtils.ArabicSetting arabicSetting = new ArabicSupportUtils.ArabicSetting( );
        arabicSetting.content = m_Text;
        arabicSetting.rectSize = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        arabicSetting.spacing = lineSpacing;
        arabicSetting.preferredWidth = preferredWidth;
        arabicSetting.preferredHeight = preferredHeight;
        arabicSetting.pixelsPerUnit = pixelsPerUnit;
        arabicSetting.maxFontSize = resizeTextMaxSize;
        arabicSetting.minFontSize = resizeTextMinSize;
        arabicSetting.textGenerator = cachedTextGenerator;
        arabicSetting.textSetting = settings;
        arabicSetting.font = font;
        arabicSetting.singleLine = _singleLine;

        if (m_Text != arabic_text && _useAutoArabic)
        {
            arabic_text = ArabicSupportUtils.FixArabic(arabicSetting, ref lineCount);
            m_Text = arabic_text;
        }

        if (lineCount > 1 && _useAutoArabic)
        {
            switch (settings.textAnchor)
            {
                case TextAnchor.LowerLeft:
                    settings.textAnchor = TextAnchor.LowerRight;
                    break;
                case TextAnchor.MiddleLeft:
                    settings.textAnchor = TextAnchor.MiddleRight;
                    break;
                case TextAnchor.UpperLeft:
                    settings.textAnchor = TextAnchor.UpperRight;
                    break;
            }
        }
        cachedTextGenerator.Populate(text, settings);

    }

    private void _UseFitSettings(TextGenerationSettings settings)
    {
        settings.resizeTextForBestFit = false;
        if (!resizeTextForBestFit || string.IsNullOrEmpty(text))
        {
            cachedTextGenerator.Populate(text, settings);
            return;
        }
        

        int minSize = resizeTextMinSize;
        if(minSize > 10)
        {
            minSize -= 3;
        }
        int txtLen = text.Length;
        bool success = false;
        string finalText = string.Empty;
        for (int i = resizeTextMaxSize; i >= minSize; --i)
        {
            settings.fontSize = i;
            bool needRelayout = false;
            if (WrapText(text, out finalText, false, ref needRelayout, settings))
            {
                cachedTextGenerator.Populate(finalText, settings);
                //因为添加了换行，可能导致实际显示数目大于字符长度
                if (cachedTextGenerator.characterCountVisible >= txtLen)
                {
                    break;
                }
            }

            /*      if (_singleLine)
                  {
                      if (cachedTextGenerator.characterCountVisible == txtLen && cachedTextGenerator.lineCount == 1) break;
                  }
                  else
                  {
                      if (cachedTextGenerator.characterCountVisible == txtLen) break;
                  }  */
        }
    }

    private void ApplyShadow(IList<UIVertex> fontVerts, float unitPerPixel, int maxLoop, VertexHelper toFill, Vector2 offset, Vector2 adjustOffset)
    {
        for (int index1 = 0; index1 < maxLoop; ++index1)
        {
            int index2 = index1 & 3;
            _tmpVerts[index2] = fontVerts[index1];
            _tmpVerts[index2].position.x = _tmpVerts[index2].position.x + offset.x;
            _tmpVerts[index2].position.y = _tmpVerts[index2].position.y + offset.y;
            _tmpVerts[index2].position *= unitPerPixel;
            _tmpVerts[index2].position.x += adjustOffset.x;
            _tmpVerts[index2].position.y += adjustOffset.y;
            var glowColor = _glowColor;
            glowColor.a *= (_tmpVerts[index2].color.a / 255f);
            _tmpVerts[index2].color = glowColor;
            if (index2 == 3)
                toFill.AddUIVertexQuad(this._tmpVerts);
        }
    }

    private void ApplyShadow(IList<UIVertex> fontVerts, float unitPerPixel, int maxLoop, VertexHelper toFill, Vector2 offset)
    {
        for (int index1 = 0; index1 < maxLoop; ++index1)
        {
            int index2 = index1 & 3;
            _tmpVerts[index2] = fontVerts[index1];
            _tmpVerts[index2].position.x = _tmpVerts[index2].position.x + offset.x;
            _tmpVerts[index2].position.y = _tmpVerts[index2].position.y + offset.y;
            _tmpVerts[index2].position *= unitPerPixel;
            var glowColor = _glowColor;
            glowColor.a *= (_tmpVerts[index2].color.a / 255f);
            _tmpVerts[index2].color = glowColor;
            if (index2 == 3)
                toFill.AddUIVertexQuad(this._tmpVerts);
        }
    }

    private readonly UIVertex[] _tmpVerts = new UIVertex[4];
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (null == font) return;

        m_DisableFontTextureRebuiltCallback = true;
        TextGenerationSettings settings = GetGenerationSettings(rectTransform.rect.size);
        

        if (!ArabicSupportUtils.IsArabicString(text))
            _UseFitSettings(settings);
        else
            _UseArabicSettings(settings);

        Rect rect = rectTransform.rect;

        Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
        Vector2 zero = Vector2.zero;
        zero.x = Mathf.Lerp(rect.xMin, rect.xMax, textAnchorPivot.x);
        zero.y = Mathf.Lerp(rect.yMin, rect.yMax, textAnchorPivot.y);
        Vector2 vector2 = PixelAdjustPoint(zero) - zero;
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float num1 = 1f / pixelsPerUnit;
        

#if UNITY_2018_4_17
        int num2 = verts.Count - 4;
#else
        int num2 = verts.Count;
#endif


        toFill.Clear( );
        var screenScale = Screen.height / 720f;
        var glowSize = _glowSize * screenScale;
        if (vector2 != Vector2.zero)
        {
            if (_glowSize > 0)
            {
                //top
                ApplyShadow(verts, num1, num2, toFill, new Vector2(0f, glowSize), vector2);
                //top right
                ApplyShadow(verts, num1, num2, toFill, new Vector2(glowSize, glowSize), vector2);
                //right
                ApplyShadow(verts, num1, num2, toFill, new Vector2(glowSize, 0f), vector2);
                //bottom right
                ApplyShadow(verts, num1, num2, toFill, new Vector2(glowSize, -glowSize), vector2);
                //bottom
                ApplyShadow(verts, num1, num2, toFill, new Vector2(0f, -glowSize), vector2);
                //bottom left
                ApplyShadow(verts, num1, num2, toFill, new Vector2(-glowSize, -glowSize), vector2);
                //left
                ApplyShadow(verts, num1, num2, toFill, new Vector2(-glowSize, 0f), vector2);
                //top left
                ApplyShadow(verts, num1, num2, toFill, new Vector2(-glowSize, glowSize), vector2);
            }

            if (_shadowOffset != Vector2.zero)
            {
                //shadow
                ApplyShadow(verts, num1, num2, toFill, _shadowOffset * screenScale, vector2);
            }

            for (int index1 = 0; index1 < num2; ++index1)
            {
                int index2 = index1 & 3;
                _tmpVerts[index2] = verts[index1];
                _tmpVerts[index2].position *= num1;
                _tmpVerts[index2].position.x += vector2.x;
                _tmpVerts[index2].position.y += vector2.y;
                if (index2 == 3)
                    toFill.AddUIVertexQuad(this._tmpVerts);
            }
        }
        else
        {
            if (_glowSize > 0)
            {
                //top
                ApplyShadow(verts, num1, num2, toFill, new Vector2(0f, glowSize));
                //top right
                ApplyShadow(verts, num1, num2, toFill, new Vector2(glowSize, glowSize));
                //right
                ApplyShadow(verts, num1, num2, toFill, new Vector2(glowSize, 0f));
                //bottom right
                ApplyShadow(verts, num1, num2, toFill, new Vector2(glowSize, -glowSize));
                //bottom
                ApplyShadow(verts, num1, num2, toFill, new Vector2(0f, -glowSize));
                //bottom left
                ApplyShadow(verts, num1, num2, toFill, new Vector2(-glowSize, -glowSize));
                //left
                ApplyShadow(verts, num1, num2, toFill, new Vector2(-glowSize, 0f));
                //top left
                ApplyShadow(verts, num1, num2, toFill, new Vector2(-glowSize, glowSize));
            }
            //shadow
            if (_shadowOffset != Vector2.zero)
            {
                ApplyShadow(verts, num1, num2, toFill, _shadowOffset * screenScale);
            }

            for (int index1 = 0; index1 < num2; ++index1)
            {
                int index2 = index1 & 3;
                _tmpVerts[index2] = verts[index1];
                _tmpVerts[index2].position *= num1;
                if (index2 == 3)
                    toFill.AddUIVertexQuad(_tmpVerts);
            }
        }
        m_DisableFontTextureRebuiltCallback = false;
        VisibleLines = cachedTextGenerator.lineCount;
    }

    private float GetWidthByPixel(TextGenerationSettings settings)
    {
        //   var max = Mathf.Max(rectTransform.sizeDelta.x,rectTransform.rect.
        return rectTransform.rect.width * pixelsPerUnit / settings.scaleFactor;
    }

    private float GetHeightByPixel(TextGenerationSettings settings)
    {
        return rectTransform.rect.height * pixelsPerUnit / settings.scaleFactor;
    }

    private float GetCharSpace(int ch)
    {
        if(ch >= '0' && ch <= '9'){
            return 1f;
        }
        else
        {
            return 0.2f;
        }
    }

    public bool WrapText(string text, out string finalText, bool keepCharCount, ref bool needRelayout, TextGenerationSettings settings)
    {
        //   font.RequestCharactersInTexture(text, settings.fontSize, settings.fontStyle);
        //   if (regionWidth < 1 || regionHeight < 1 || finalLineHeight < 1f)
        var finalLineHeight = (settings.fontSize + lineSpacing + 2);
        if (preferredWidth < 1 || preferredHeight < 1 || finalLineHeight < 1)
        {
            finalText = "";
            return false;
        }

        //   float height = (maxLines > 0) ? Mathf.Min(regionHeight, finalLineHeight * maxLines) : regionHeight;
        float height = GetHeightByPixel(settings);
        int maxLineCount = 10000; // (maxLines > 0) ? maxLines : 1000000;
        maxLineCount = Mathf.FloorToInt(Mathf.Min(maxLineCount, height / finalLineHeight) + 0.01f);

        if (maxLineCount == 0)
        {
            finalText = "";
            return false;
        }

        if (string.IsNullOrEmpty(text)) text = " ";
        //get char info
        //    Prepare(text);

        StringBuilder sb = new StringBuilder( );
        int textLength = text.Length;
        //    float remainingWidth = regionWidth;
        float remainingWidth = GetWidthByPixel(settings);
        int start = 0, offset = 0, lineCount = 1, prev = 0;
        bool lineIsEmpty = true;
        bool fits = true;
        bool eastern = false;

        // Run through all characters
        for (; offset < textLength; ++offset)
        {
            char ch = text[offset];
            if (ch > 12287)
            {
                eastern = true;
            }
            else
            {
                eastern = false;
            }

          //  if(Localization.Instance.CurrentLanguage == "th") eastern = true;

            // New line character -- start a new line
            if (ch == '\n')
            {
                if (lineCount == maxLineCount)
                    break;
                //     remainingWidth = regionWidth;
                remainingWidth = GetWidthByPixel(settings);
                // Add the previous word to the final string
                if (start < offset) sb.Append(text.Substring(start, offset - start + 1));
                else sb.Append(ch);

                lineIsEmpty = true;
                ++lineCount;
                start = offset + 1;
                prev = 0;
                continue;
            }

            // When encoded symbols such as [RrGgBb] or [-] are encountered, skip past them
            //    if (encoding && ParseSymbol(text, ref offset)) { --offset; continue; }
            if (ParseSymbol(text, ref offset))
            {
                --offset;
                continue;
            }
            // See if there is a symbol matching this text
            //     BMSymbol symbol = useSymbols ? GetSymbol(text, offset, textLength) : null;
            //获取字形的时候用默认size，然后用比例换算
            CharacterInfo info = new CharacterInfo( );
            if (!font.GetCharacterInfo(ch, out info, font.fontSize, settings.fontStyle))
            {
                font.RequestCharactersInTexture(text, font.fontSize, settings.fontStyle);
                font.GetCharacterInfo(ch, out info, font.fontSize, settings.fontStyle);
            }

            // Calculate how wide this symbol or character is going to be
            var fontScale = (float) settings.fontSize / font.fontSize;
            float glyphWidth = (info.advance + GetCharSpace(ch)) * fontScale;

            /*    if (symbol == null)
                {
                    // Find the glyph for this character
                    float w = GetGlyphWidth(ch, prev);
                    if (w == 0f) continue;
                    glyphWidth = finalSpacingX + w;
                }
                else glyphWidth = finalSpacingX + symbol.advance * fontScale; */
            // Reduce the width
            remainingWidth -= glyphWidth;

            // If this marks the end of a word, add it to the final string.
            if ((IsSpace(ch) || IsHyphen(ch)) && !eastern && start < offset)
            {
                int end = offset - start + 1;

                // Last word on the last line should not include an invisible character
                if (lineCount == maxLineCount && remainingWidth <= 0f && offset < textLength)
                {
                    char cho = text[offset];
                    if (cho < ' ' || IsSpace(cho)||IsHyphen(cho)) --end;
                }

                sb.Append(text.Substring(start, end));
                lineIsEmpty = false;
                start = offset + 1;
                prev = ch;
            }

            // Doesn't fit?
            if (Mathf.RoundToInt(remainingWidth) < 0)
            {
                // Can't start a new line
                if (lineIsEmpty || lineCount == maxLineCount)
                {
                    // This is the first word on the line -- add it up to the character that fits
                    sb.Append(text.Substring(start, Mathf.Max(0, offset - start)));
                    bool space = IsSpace(ch);
                    bool hyphen = IsHyphen(ch);
                    if (!space && !eastern)
                    {
                        fits = false;
                        finalText = "";
                        //加的优化，如果不是fit状态，不用再计算了
                        return false;
                    }

                    if (lineCount++ == maxLineCount)
                    {
                        start = offset;
                        break;
                    }

                    if(eastern)
                    {
                        if (space) ReplaceSpaceWithNewline(ref sb);
                        else if(hyphen) AddHyphenWithNewline(ref sb);
                        else EndLine(ref sb);

                    }
                    else
                    {
                        if (keepCharCount) ReplaceSpaceWithNewline(ref sb);
                        else EndLine(ref sb);
                    }



                    // Start a brand-new line
                    lineIsEmpty = true;

                    if (space)
                    {
                        start = offset + 1;
                        //    remainingWidth = regionWidth;
                        remainingWidth = GetWidthByPixel(settings);
                    }
                    else
                    {
                        start = offset;
                        //   remainingWidth = regionWidth - glyphWidth;
                        remainingWidth = GetWidthByPixel(settings) - glyphWidth;
                    }
                    prev = 0;
                }
                else
                {
                    // Revert the position to the beginning of the word and reset the line
                    lineIsEmpty = true;
                    //   remainingWidth = regionWidth;
                    if(eastern)
                    {
                         sb.Append(text.Substring(start, Mathf.Max(0, offset - start)));
                         start = offset;
                         remainingWidth = GetWidthByPixel(settings) - glyphWidth;
                         prev = 0;
                    }
                    else
                    {
                        remainingWidth = GetWidthByPixel(settings);
                        offset = start - 1;
                        prev = 0;
                    }


                    if (lineCount++ == maxLineCount) break;
                    if (keepCharCount) ReplaceSpaceWithNewline(ref sb);
                    else EndLine(ref sb);
                    continue;
                }
            }
            else prev = ch;

            // Advance the offset past the symbol
            /*    if (symbol != null)
                {
                    offset += symbol.length - 1;
                    prev = 0;
                }  */
        }

        if (start < offset) sb.Append(text.Substring(start, offset - start));
        finalText = sb.ToString( );
        if (needRelayout)
        {
            //          D.warn("asdfwef " + finalText);
            string[] strofline = finalText.Split('\n');
            StringBuilder sb2 = new StringBuilder( );
            foreach (var str in strofline)
            {
                char[] temparray = str.ToCharArray( );
                Array.Reverse(temparray);
                sb2.Append(temparray);
                sb2.Append('\n');
            }
            finalText = sb2.ToString( );
        }

        return fits && ((offset == textLength) && (lineCount <= Mathf.Min(1000, maxLineCount)));
    }

    public bool IsOneLine(string text, TextGenerationSettings settings)
    {
        int prev = 0;
        float glyphWidth = 0f;
        //     float remainingWidth = regionWidth;
        float remainingWidth = GetWidthByPixel(settings);
        text = StripSymbols(text);
        var width = cachedTextGenerator.GetPreferredWidth(text, settings);
        return width <= preferredWidth;
        // Run through all characters
        /*     for (int offset = 0; offset < text.Length; ++offset)
             {
                 char ch = text[offset];
                 float w = GetGlyphWidth(ch, prev);
                 if (w == 0f) continue;
                 glyphWidth = finalSpacingX + w;
                 remainingWidth -= glyphWidth;
                 if(remainingWidth < 0f)
                     return false;
             }
             return true;  */
    }

    static void ReplaceSpaceWithNewline(ref StringBuilder s)
    {
        int i = s.Length - 1;
        if (i > 0 && IsSpace(s[i])) s[i] = '\n';
    }
    static void AddHyphenWithNewline(ref StringBuilder s)
    {
        int i = s.Length - 1;
        if (i > 0 &&  IsHyphen(s[i])) s[i] = '\n';
    }

    static public void EndLine(ref StringBuilder s)
    {
        int i = s.Length - 1;
        if (i > 0 && (IsSpace(s[i]))) s[i] = '\n';
        else if (i > 0 && (IsHyphen(s[i]))) s.Append('\n');
        else s.Append('\n');
    }

    static public string StripSymbols(string text)
    {
        if (text != null)
        {
            for (int i = 0, imax = text.Length; i < imax;)
            {
                char c = text[i];

                if (c == '[')
                {
                    int sub = 0;
                    bool bold = false;
                    bool italic = false;
                    bool underline = false;
                    bool strikethrough = false;
                    bool ignoreColor = false;
                    int retVal = i;

                    if (ParseSymbol(text, ref retVal, null, false, ref sub, ref bold, ref italic, ref underline, ref strikethrough, ref ignoreColor))
                    {
                        text = text.Remove(i, retVal - i);
                        imax = text.Length;
                        continue;
                    }
                }
                ++i;
            }
        }
        return text;
    }

    static public bool ParseSymbol(string text, ref int index)
    {
        int n = 1;
        bool bold = false;
        bool italic = false;
        bool underline = false;
        bool strikethrough = false;
        bool ignoreColor = false;
        return ParseSymbol(text, ref index, null, false, ref n, ref bold, ref italic, ref underline, ref strikethrough, ref ignoreColor);
    }

    static public bool ParseSymbol(string text, ref int index, List<Color> colors, bool premultiply,
        ref int sub, ref bool bold, ref bool italic, ref bool underline, ref bool strike, ref bool ignoreColor)
    {
        int length = text.Length;

        if (index + 3 > length || text[index] != '<') return false;

        var endIndex = text.IndexOf('>', index);
        if (endIndex > index)
        {
            index = endIndex + 1;
            return true;
        }
        else
        {
            return false;
        }
        //if (text[index + 2] == ']')
        //{
        //    if (text[index + 1] == '-')
        //    {
        //        if (colors != null && colors.Count > 1)
        //            colors.RemoveAt(colors.Count - 1);
        //        index += 3;
        //        return true;
        //    }

        //    string sub3 = text.Substring(index, 3);

        //    switch (sub3)
        //    {
        //        case "[b]":
        //            bold = true;
        //            index += 3;
        //            return true;

        //        case "[i]":
        //            italic = true;
        //            index += 3;
        //            return true;

        //        case "[u]":
        //            underline = true;
        //            index += 3;
        //            return true;

        //        case "[s]":
        //            strike = true;
        //            index += 3;
        //            return true;

        //        case "[c]":
        //            ignoreColor = true;
        //            index += 3;
        //            return true;
        //    }
        //}

        //if (index + 4 > length) return false;

        //if (text[index + 3] == ']')
        //{
        //    string sub4 = text.Substring(index, 4);

        //    switch (sub4)
        //    {
        //        case "[/b]":
        //            bold = false;
        //            index += 4;
        //            return true;

        //        case "[/i]":
        //            italic = false;
        //            index += 4;
        //            return true;

        //        case "[/u]":
        //            underline = false;
        //            index += 4;
        //            return true;

        //        case "[/s]":
        //            strike = false;
        //            index += 4;
        //            return true;

        //        case "[/c]":
        //            ignoreColor = false;
        //            index += 4;
        //            return true;

        //        default:
        //            {
        //                char ch0 = text[index + 1];
        //                char ch1 = text[index + 2];

        //                if (IsHex(ch0) && IsHex(ch1))
        //                {
        //                    int a = (NGUIMath.HexToDecimal(ch0) << 4) | NGUIMath.HexToDecimal(ch1);
        //                    //       mAlpha = a / 255f;
        //                    index += 4;
        //                    return true;
        //                }
        //            }
        //            break;
        //    }
        //}

        //if (index + 5 > length) return false;

        //if (text[index + 4] == ']')
        //{
        //    string sub5 = text.Substring(index, 5);

        //    switch (sub5)
        //    {
        //        case "[sub]":
        //            sub = 1;
        //            index += 5;
        //            return true;

        //        case "[sup]":
        //            sub = 2;
        //            index += 5;
        //            return true;
        //    }
        //}

        //if (index + 6 > length) return false;

        //if (text[index + 5] == ']')
        //{
        //    string sub6 = text.Substring(index, 6);

        //    switch (sub6)
        //    {
        //        case "[/sub]":
        //            sub = 0;
        //            index += 6;
        //            return true;

        //        case "[/sup]":
        //            sub = 0;
        //            index += 6;
        //            return true;

        //        case "[/url]":
        //            index += 6;
        //            return true;
        //    }
        //}

        //if (text[index + 1] == 'u' && text[index + 2] == 'r' && text[index + 3] == 'l' && text[index + 4] == '=')
        //{
        //    int closingBracket = text.IndexOf(']', index + 4);

        //    if (closingBracket != -1)
        //    {
        //        index = closingBracket + 1;
        //        return true;
        //    }
        //    else
        //    {
        //        index = text.Length;
        //        return true;
        //    }
        //}

        //if (index + 8 > length) return false;

        //if (text[index + 7] == ']')
        //{
        //    Color c = ParseColor24(text, index + 1);

        //    if (EncodeColor24(c) != text.Substring(index + 1, 6).ToUpper( ))
        //        return false;

        //    if (colors != null)
        //    {
        //        c.a = colors[colors.Count - 1].a;
        //        //    if (premultiply && c.a != 1f)
        //        //        c = Color.Lerp(mInvisible, c, c.a);
        //        colors.Add(c);
        //    }
        //    index += 8;
        //    return true;
        //}

        //if (index + 10 > length) return false;

        //if (text[index + 9] == ']')
        //{
        //    Color c = ParseColor32(text, index + 1);
        //    if (EncodeColor32(c) != text.Substring(index + 1, 8).ToUpper( ))
        //        return false;

        //    if (colors != null)
        //    {
        //        //    if (premultiply && c.a != 1f)
        //        //        c = Color.Lerp(mInvisible, c, c.a);
        //        colors.Add(c);
        //    }
        //    index += 10;
        //    return true;
        //}
        //return false;
    }

    static public Color ParseColor32(string text, int offset)
    {
        int r = (NGUIMath.HexToDecimal(text[offset]) << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
        int g = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
        int b = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
        int a = (NGUIMath.HexToDecimal(text[offset + 6]) << 4) | NGUIMath.HexToDecimal(text[offset + 7]);
        float f = 1f / 255f;
        return new Color(f * r, f * g, f * b, f * a);
    }

    static public string EncodeColor32(Color c)
    {
        int i = NGUIMath.ColorToInt(c);
        return NGUIMath.DecimalToHex32(i);
    }

    static public Color ParseColor24(string text, int offset)
    {
        int r = (NGUIMath.HexToDecimal(text[offset]) << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
        int g = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
        int b = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
        float f = 1f / 255f;
        return new Color(f * r, f * g, f * b);
    }

    static public string EncodeColor24(Color c)
    {
        int i = 0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8);
        return NGUIMath.DecimalToHex24(i);
    }

    static public bool IsHex(char ch)
    {
        return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
    }

    static bool IsSpace(int ch)
    {
        return (ch == ' ' || ch == 0x200a || ch == 0x200b || ch == '\u2009');
    }
    static bool IsHyphen(int ch)
    {
        return (ch == '-' || ch == 0x2010 );
    }

    private int _lastInt = int.MinValue;
    /// 缓存int类型值避免重复设置
    public void SetIntValue(int nv)
    {
        if (_lastInt == nv)
            return;

        _lastInt = nv;
        text = _lastInt.ToString();
    }
    
    /// 缓存int类型值避免重复设置
    public void SetIntValue(int nv, string suffix)
    {
        if (_lastInt == nv)
            return;

        _lastInt = nv;
        
        if(string.IsNullOrEmpty(suffix))
            text = _lastInt.ToString();
        else
            text = _lastInt.ToString() + suffix;
    }
}

public class NGUIMath
{
    static public int HexToDecimal(char ch)
    {
        switch (ch)
        {
            case '0':
                return 0x0;
            case '1':
                return 0x1;
            case '2':
                return 0x2;
            case '3':
                return 0x3;
            case '4':
                return 0x4;
            case '5':
                return 0x5;
            case '6':
                return 0x6;
            case '7':
                return 0x7;
            case '8':
                return 0x8;
            case '9':
                return 0x9;
            case 'a':
            case 'A':
                return 0xA;
            case 'b':
            case 'B':
                return 0xB;
            case 'c':
            case 'C':
                return 0xC;
            case 'd':
            case 'D':
                return 0xD;
            case 'e':
            case 'E':
                return 0xE;
            case 'f':
            case 'F':
                return 0xF;
        }
        return 0xF;
    }

    static public int ColorToInt(Color c)
    {
        int retVal = 0;
        retVal |= Mathf.RoundToInt(c.r * 255f) << 24;
        retVal |= Mathf.RoundToInt(c.g * 255f) << 16;
        retVal |= Mathf.RoundToInt(c.b * 255f) << 8;
        retVal |= Mathf.RoundToInt(c.a * 255f);
        return retVal;
    }

    static public string DecimalToHex32(int num)
    {
        return num.ToString("X8");
    }

    static public string DecimalToHex24(int num)
    {
        num &= 0xFFFFFF;
        return num.ToString("X6");
    }
}