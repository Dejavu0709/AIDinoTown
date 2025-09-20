using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static partial class ArabicSupportUtils
{
	
    private static StringBuilder fixBuilder = new StringBuilder( );
    private static StringBuilder concatBuilder = new StringBuilder( );

    private static string[] FuncTag = {
        "<i>",       "</i>",
        "<b>",       "</b>",
        "<size=",    "</size>",
        "<color=",   "</color>",
        "<a href=",  "</a>"
    };
    private static char[] CloseTag = 
    {
        '{',     '}',
        '[',     ']',
        '(',     ')',
        '<',     '>'
    };

    private enum CharType
    {
        RTL,  LTR, SPL, PTT,  TAG
    }

    private struct WordInfo
    {
        public string content;
        public string tag;
        public CharType chart_type;
        public float width;
    }

    public struct ArabicSetting
    {
        public string content;
        public Vector2 rectSize;
        public float spacing;
        public float preferredWidth;
        public float preferredHeight;
        public float pixelsPerUnit;
        public int maxFontSize;
        public int minFontSize;
        public TextGenerator textGenerator;
        public TextGenerationSettings textSetting;
        public Font font;
        public bool singleLine;
    }
}