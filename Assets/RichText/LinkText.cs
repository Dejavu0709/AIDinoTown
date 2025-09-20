using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class LinkText : Text, IPointerClickHandler
{
    private String _originalText;
    private readonly List<LinkInfo> Links = new List<LinkInfo>( );
    protected static readonly StringBuilder TextBuilder = new StringBuilder( );
    private LinkText mlinkText;
    public bool _showlinkColor=false;
    public bool _ShowLinkColor
    {
        get
        {
            return _showlinkColor;
        }
        set
        {
            if (_showlinkColor == value)
                return;
            _showlinkColor = value;
            SetVerticesDirty();
            SetLayoutDirty();
        }
    }
    public Color _linkColor = Color.blue;
    public Color _LinkColor
    {
        get
        {
            return _linkColor;
        }
        set
        {
            if (_linkColor == value)
                return;
            _linkColor = value;
            SetVerticesDirty();
            SetLayoutDirty();
        }
    }

    public bool _showUnderLine = false;
    public bool _ShowUnderLine
    {
        get
        {
            return _showUnderLine;
        }
        set
        {
            if (_showUnderLine == value)
                return;
            _showUnderLine = value;
            SetVerticesDirty();
            SetLayoutDirty();
        }
    }
    public class HrefClickEvent : UnityEvent<string>
    { }
    private HrefClickEvent OnHrefClick = new HrefClickEvent( );

    public HrefClickEvent onHrefClick
    {
        get
        {
            return OnHrefClick;
        }
        set
        {
            OnHrefClick = value;
        }
    }
    /// <summary>
    /// ����������
    /// </summary>
    private static readonly Regex s_HrefRegex = new Regex(@"<a (href|act)=([^>\n\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

    protected override void Awake()
    {
        base.Awake();
        mlinkText = GetComponent<LinkText>();
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        mlinkText.onHrefClick.RemoveAllListeners();
        mlinkText.onHrefClick.AddListener(OnHyperlinkTextInfo);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        mlinkText.onHrefClick.RemoveAllListeners();

    }
    protected override void OnPopulateMesh(VertexHelper toFill)
    {

        TextGenerationSettings settings = GetGenerationSettings(rectTransform.rect.size);

        if (ArabicSupportUtils.IsArabicString(text))
            _UseArabicSettings(settings);
        if (string.IsNullOrEmpty(_originalText))
        {
            m_Text = GetOutputText(m_Text);
        }else
        {
            m_Text = GetOutputText(_originalText);
        }
        
        base.OnPopulateMesh(toFill);

        var vert = new UIVertex( );

        foreach (var hrefInfo in Links)
        {
            hrefInfo.boxes.Clear( );
            hrefInfo.linefeedIndexList.Clear();
            if (hrefInfo.startIndex >= toFill.currentVertCount)
            {
                continue;
            }
            toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            hrefInfo.linefeedIndexList.Add(hrefInfo.startIndex);
            for (int i = hrefInfo.startIndex, m = hrefInfo.startIndex + hrefInfo.length; i < m; i++)
            {
                if (i >= toFill.currentVertCount)
                {
                    break;
                }
                toFill.PopulateUIVertex(ref vert, i);
                if (_showlinkColor) 
                {
                    vert.color = _linkColor;
                    toFill.SetUIVertex(vert, i);
                }

                pos = vert.position;

                if (pos.x < bounds.min.x)
                {
                    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    hrefInfo.linefeedIndexList.Add(i);
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos);
                }
            }
            hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        }
        if (_ShowUnderLine) 
        {
            DrawUnderLine(toFill);
        }  
    }
    /// <summary>
    /// ��ȡ�����ӽ�������������ı�?
    /// </summary>
    /// <returns></returns>
    protected virtual string GetOutputText(string outputText)
    {
        TextBuilder.Length = 0;
        Links.Clear( );
        var indexText = 0;
        foreach (Match match in s_HrefRegex.Matches(outputText))
        {
            TextBuilder.Append(outputText.Substring(indexText, match.Index - indexText));
            var group = match.Groups[1];
            var linkInfo = new LinkInfo
            {
                startIndex = GetStringBytes(TextBuilder.ToString()) * 4,
                length = (GetStringBytes(match.Groups[3].Value)-1) * 4 + 3,
                content = match.Groups[2].Value,
                key = group.Value
            };
            linkInfo.spaceDic = GetSpaceDic(match.Groups[3].Value, linkInfo.startIndex);
            int test = (TextBuilder.Length + match.Groups[3].Length - 1) * 4 + 3;
            // Debug.Log("yush GetOutputText "+outputText + " linkInfo.length "+linkInfo.length + " linkInfo.startIndex "+ linkInfo.startIndex + " test "+ test);
            Links.Add(linkInfo);

            TextBuilder.Append(match.Groups[3].Value);
            indexText = match.Index + match.Length;
        }
        TextBuilder.Append(outputText.Substring(indexText, outputText.Length - indexText));
        return TextBuilder.ToString( );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        foreach (var hrefInfo in Links)
        {
            var boxes = hrefInfo.boxes;
            foreach (var box in hrefInfo.boxes)
            {
                if (box.Contains(lp))
                {
                    if (OnHrefClick != null)
                        OnHrefClick.Invoke(hrefInfo.content);
                    return;
                }
            }
        }
    }
    private void DrawUnderLine(VertexHelper vh)
    {
        UIVertex vert = new UIVertex();
        List<Vector3> startPosList = new List<Vector3>();
        List<Vector3> endPosList = new List<Vector3>();
        foreach (var hrefInfo in Links)
        {
            if (hrefInfo.startIndex >= vh.currentVertCount)
            {
                continue;
            }

            float minY = float.MaxValue;
            for (int i = hrefInfo.startIndex, m = hrefInfo.startIndex + hrefInfo.length; i < m; i += 4)
            {
                if (i >= vh.currentVertCount)
                {
                    break;
                }

                if (hrefInfo.linefeedIndexList.Contains(i))
                {
                    for (int j = 0; j < startPosList.Count; j++)
                    {
                        MeshUnderLine(vh, new Vector2(startPosList[j].x, minY), new Vector2(endPosList[j].x, minY));
                    }
                    startPosList.Clear();
                    endPosList.Clear();
                }

                vh.PopulateUIVertex(ref vert, i + 3);
                startPosList.Add(vert.position);
                vh.PopulateUIVertex(ref vert, i + 2);
                if (hrefInfo.spaceDic.ContainsKey(i))
                {
                    endPosList.Add(vert.position+new Vector3((vert.position.x - startPosList[startPosList.Count-1].x)*hrefInfo.spaceDic[i],0,0));
                }
                else
                {
                    endPosList.Add(vert.position);
                }
                

                if (vert.position.y < minY)
                {
                    minY = vert.position.y;
                }
            }

            for (int j = 0; j < startPosList.Count; j++)
            {
                MeshUnderLine(vh, new Vector2(startPosList[j].x, minY), new Vector2(endPosList[j].x, minY));
            }
            startPosList.Clear();
            endPosList.Clear();

        }
    }
    private void MeshUnderLine(VertexHelper vh, Vector2 startPos, Vector2 endPos,int space = 0)
    {
        Vector2 extents = rectTransform.rect.size;
        var setting = GetGenerationSettings(extents);

        TextGenerator underlineText = new TextGenerator();
        underlineText.Populate("—�?", setting);

        IList<UIVertex> lineVer = underlineText.verts;/*new UIVertex[4];*///"_"�ĵĶ�������

        Vector3[] pos = new Vector3[4];
        pos[0] = startPos + new Vector2(-1.5f, -3f);
        pos[3] = startPos + new Vector2(-1.5f, 1f);
        pos[2] = endPos + new Vector2(1.5f, 1f);
        pos[1] = endPos + new Vector2(1.5f, -3f);


        UIVertex[] tempVerts = new UIVertex[4];
        for (int i = 0; i < 4; i++)
        {
            tempVerts[i] = lineVer[i];
            tempVerts[i].position = pos[i];
            tempVerts[i].color = _linkColor;
        }

        vh.AddUIVertexQuad(tempVerts);
    }

    private void OnHyperlinkTextInfo(string url)
    {
        Debug.Log("linktext OnHyperlinkTextInfo url:" + url);
        //WebUtils.OpenWebURL(url,false,true,false,null);
        // WebAuthManager.Instance.ShowPrivacyView(url);
    }
    private class LinkInfo
    {
        public int startIndex;
        // public int endIndex;
        public int length;
        public Dictionary<int,int> spaceDic;
        public string content;
        public string key;
        public readonly List<Rect> boxes = new List<Rect>( );
        public List<int> linefeedIndexList = new List<int>();
    }

    private string arabic_text = string.Empty;
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

        if (m_Text != arabic_text)
        {
            arabic_text = ArabicSupportUtils.FixArabic(arabicSetting, ref lineCount);
            m_Text = arabic_text;
        }

        if (lineCount > 1)
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

    private int GetStringBytes(string builder)
    {
        int result = 0;
        foreach (char c in builder)
        {
            if (c != ' ')
            {
                result++;
            }
        }

        return result;
    }
    
    private Dictionary<int, int> GetSpaceDic(string text,int startIndex)
    {
    // 获取text中space的位置和数量
        Dictionary<int, int> spaceDic = new Dictionary<int, int>();
        // int spaceCount = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                // var temp = text.Substring(0, i);
                spaceDic.Add((GetStringBytes(text.Substring(0, i))-1)*4+startIndex, 1);
                // spaceCount++;
            }
        }

        return spaceDic;
    
    }

    public override string text { get
        {
            return base.text;
        }
        set
        {
            _originalText = value;
            base.text = value;
        } 
    }
}