using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NexgenDragon;

/// 文本控件，支持图片
[AddComponentMenu("UI/Rich Text", 10)]
public class RichText : Text
{
	// 正在被使用的图片
	private readonly List<Image> m_UsedImages = new List<Image>();

	// 图片的第一个顶点的索引
	private readonly List<int> m_ImageStartVertexIndices = new List<int>();

	// 正则取出所需要的属性
	public static readonly Regex s_ImageRegex = new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);

	// 加载精灵图片方法
	public static Func<Image, string, Sprite> OnLoadSprite;

	private UIVertex[] m_TempVerts = new UIVertex[4];

	private string arabic_text_view = string.Empty;
    //为了不破坏原有规则 缓存原始值，外部访问获取 返回该值
	private string text_cache = string.Empty;

    public bool useAutoArabic = true;
	private static readonly Vector4 Vector2One = new Vector4(1, 1, 0, 0);
	public override string text
        {
            get
            {
                return  text_cache;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
	                if (String.IsNullOrEmpty(text_cache) && String.IsNullOrEmpty(m_Text))
		                return;

                    text_cache= m_Text = "";
                    SetVerticesDirty();
                }
                else if (text_cache != value)
                {
                    text_cache = m_Text = value;
                    SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }

	public override void SetVerticesDirty()
	{
		base.SetVerticesDirty();
		UpdateArabicText();
		GenerateQuadImage();
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		UpdateText (toFill);
		UpdateQuadImages (toFill);
	}

	public void UpdatePopulateMesh()
	{
		UpdateGeometry();
	}


	public void UpdateArabicText()
    {
        if (string.IsNullOrEmpty(text)|| !ArabicSupportUtils.IsArabicString(m_Text) ||	!useAutoArabic) return;

		TextGenerationSettings settings = GetGenerationSettings(rectTransform.rect.size);
        settings.resizeTextForBestFit = false;

        var lineCount = 1;

        ArabicSupportUtils.ArabicSetting arabicSetting = new ArabicSupportUtils.ArabicSetting( );
        arabicSetting.content = m_Text;
        arabicSetting.rectSize = new Vector2(rectTransform.rect.width,rectTransform.rect.height);
        arabicSetting.spacing = lineSpacing;
        arabicSetting.preferredWidth = preferredWidth;
        arabicSetting.preferredHeight = preferredHeight;
        arabicSetting.pixelsPerUnit = pixelsPerUnit;
        arabicSetting.maxFontSize = resizeTextMaxSize;
        arabicSetting.minFontSize = resizeTextMinSize;
        arabicSetting.textGenerator = cachedTextGenerator;
        arabicSetting.textSetting = settings;
        arabicSetting.font = font;

		if(arabic_text_view!=m_Text)
		{
			arabic_text_view = ArabicSupportUtils.FixArabic(arabicSetting, ref lineCount);
			m_Text =arabic_text_view;
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

        // cachedTextGenerator.Populate(text, settings);
    }

	void UpdateText(VertexHelper toFill)
	{
		if (font == null)
			return;

		// We don't care if we the font Texture changes while we are doing our Update.
		// The end result of cachedTextGenerator will be valid for this instance.
		// Otherwise we can get issues like Case 619238.
		m_DisableFontTextureRebuiltCallback = true;

		// >>>>> KG Begin
		var orignText = m_Text;

		Vector2 extents = rectTransform.rect.size;
		var settings = GetGenerationSettings(extents);
		cachedTextGenerator.PopulateWithErrors(m_Text, settings, gameObject);

		m_Text = orignText;
		// <<<<< KG End

		// Apply the offset to the vertices
		IList<UIVertex> verts = cachedTextGenerator.verts;
		float unitsPerPixel = 1 / pixelsPerUnit;
		//Last 4 verts are always a new line... (\n)			
#if UNITY_2018_4_17
    int vertCount = verts.Count - 4;
#else
		int vertCount = verts.Count;
#endif

		/*
	      清除乱码

	        (1,1)
	      0---1
	      | \ |
	      3---2
	    (0,0)

	     */
		var roundingOffset = Vector2.zero;
		if (vertCount > 0)
		{
			roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
			roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
		}

		toFill.Clear();
		if (roundingOffset != Vector2.zero)
		{
			for (int i = 0; i < vertCount; ++i)
			{
				int tempVertsIndex = i & 3;
				m_TempVerts[tempVertsIndex] = verts[i];
				m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
				m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
				m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
				if (tempVertsIndex == 3)
				{
					if (m_TempVerts[3].uv0 == Vector4.zero && m_TempVerts[1].uv0 == Vector2One)
					{
						m_TempVerts[0].uv0 = Vector2.zero;
						m_TempVerts[1].uv0 = Vector2.zero;
						m_TempVerts[2].uv0 = Vector2.zero;
						m_TempVerts[3].uv0 = Vector2.zero;
					}
					toFill.AddUIVertexQuad(m_TempVerts);
				}
			}
		}
		else
		{
			for (int i = 0; i < vertCount; ++i)
			{
				int tempVertsIndex = i & 3;
				m_TempVerts[tempVertsIndex] = verts[i];
				m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
				if (tempVertsIndex == 3)
				{
					if (m_TempVerts[3].uv0 == Vector4.zero && m_TempVerts[1].uv0 == Vector2One)
					{
						m_TempVerts[0].uv0 = Vector2.zero;
						m_TempVerts[1].uv0 = Vector2.zero;
						m_TempVerts[2].uv0 = Vector2.zero;
						m_TempVerts[3].uv0 = Vector2.zero;
					}
					toFill.AddUIVertexQuad(m_TempVerts);
				}
			}
		}

		m_DisableFontTextureRebuiltCallback = false;
	}

	void GenerateQuadImage()
	{
		#if UNITY_EDITOR
		if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
		{
			return;
		}
		#endif

		m_ImageStartVertexIndices.Clear();
		int count = 0;
		foreach (Match match in s_ImageRegex.Matches(m_Text))
		{
			m_ImageStartVertexIndices.Add((match.Index - count*34)*4+count*4);
			count++;
			m_UsedImages.RemoveAll (image => (bool)image);
			if (m_UsedImages.Count == 0)
			{
				GetComponentsInChildren<Image>(true, m_UsedImages);
			}

			if (m_ImageStartVertexIndices.Count > m_UsedImages.Count)
			{
				var resources = new DefaultControls.Resources();
				var go = DefaultControls.CreateImage(resources);
				go.layer = gameObject.layer;
				var rt = go.transform as RectTransform;
				if (rt)
				{
					rt.SetParent(rectTransform);
					rt.localPosition = Vector3.zero;
					rt.localRotation = Quaternion.identity;
					rt.localScale = Vector3.one;
					rt.anchorMin = new Vector2(0, 1);
					rt.anchorMax = new Vector2(0, 1);
				}
				m_UsedImages.Add(go.GetComponent<Image>());
			}

			var spriteName = match.Groups[1].Value;
			var size = float.Parse(match.Groups[2].Value);
			var img = m_UsedImages[m_ImageStartVertexIndices.Count - 1];
			if (img.sprite == null || img.sprite.name != spriteName)
			{
				if(OnLoadSprite != null)
				{
					img.sprite = OnLoadSprite (img, spriteName);
				}
				else
				{
					//SpriteManager.Instance.LoadSprite (spriteName, img, string.Empty);
				}
			}
			img.raycastTarget = false;
			img.rectTransform.sizeDelta = new Vector2(size, size);
			img.preserveAspect = true;
			img.enabled = true;
		}

		for (var i = m_ImageStartVertexIndices.Count; i < m_UsedImages.Count; i++)
		{
			if (m_UsedImages[i])
			{
				m_UsedImages[i].enabled = false;
			}
		}
	}

	void UpdateQuadImages (VertexHelper toFill)
	{
		UIVertex vert = new UIVertex ();
		for (int imageIindex = 0; imageIindex < m_ImageStartVertexIndices.Count; ++imageIindex)
		{
			int startIndex = m_ImageStartVertexIndices [imageIindex];
			if (startIndex + 4 <= toFill.currentVertCount)
			{
				// 固定图片的位置
				var rt = m_UsedImages [imageIindex].rectTransform;
				var size = rt.sizeDelta;
				
				toFill.PopulateUIVertex(ref vert, startIndex + 3);
				rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2, vert.position.y + size.y / 2);
			}
		}
		m_ImageStartVertexIndices.Clear();
	}
}
