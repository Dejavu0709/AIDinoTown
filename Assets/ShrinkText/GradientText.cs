
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/GradientText")]
public class GradientText : BaseMeshEffect
{
    public bool isVertical;
    public AnimationCurve curve;
    public Color32 leftTopColor = new Color32(248, 217, 44, 255);
    public Color32 rightTopColor = new Color32(255, 255, 255, 255);
    public Color32 leftBottomColor = new Color32(22, 217, 44, 255);
    public Color32 rightBottomColor = new Color32(22, 255, 255, 255);
    List<UIVertex> vertexs = new List<UIVertex>();
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }
        int count = vh.currentVertCount;
        if (count == 0) return;

        while (vertexs.Count < count)
        {
            vertexs.Add(new UIVertex());
        }
        
        for (int i = 0; i < count; i++)
        {
            UIVertex vertex = new UIVertex();
            vh.PopulateUIVertex(ref vertex, i);
            vertexs[i] = vertex;
        }

        float topY = vertexs[0].position.y;
        float bottomY = vertexs[0].position.y;
        float leftX = vertexs[0].position.x;
        float rightX = vertexs[0].position.x;
        for (int i = 1; i < count; i++)
        {
            float y = vertexs[i].position.y;
            if (y > topY)
            {
                topY = y;
            }
            else if (y < bottomY)
            {
                bottomY = y;
            }

            float x = vertexs[i].position.x;
            if (x > rightX)
            {
                rightX = x;
            }
            else if (x < leftX)
            {
                leftX = x;
            }

        }
        float height = topY - bottomY;
        float width = rightX - leftX;

        for (int i = 0; i < count; i++)
        {
            UIVertex vertex = vertexs[i];

   
            float weight = curve.Evaluate((vertex.position.y - bottomY) / height);
            float weightVertical = curve.Evaluate((vertex.position.x - leftX) / width);
            Color32 color1 = Color32.Lerp(leftBottomColor, leftTopColor, weight);
            Color32 color2 = Color32.Lerp(rightBottomColor, rightTopColor, weight);
            //  Color32 color = Color32.Lerp(color1, color2, (vertex.position.x - leftX) / width);
            Color32 colorVertical = Color32.Lerp(rightBottomColor, leftBottomColor, weightVertical);
            if (isVertical)
            {
                vertex.color = colorVertical;
            }
            else 
            { 
                vertex.color = color1;
            }
            
            vh.SetUIVertex(vertex, i);
        }
    }


}
