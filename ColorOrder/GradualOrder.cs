﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
public abstract class GradualOrder : ColorOrderBase
{
    [MinValue(0)]
    [HorizontalGroup]
    [LabelText("播放次数")]
    public int playCount = 1;
}
[LabelText("颜色变化")]
public class DoColor : GradualOrder
{
    [LabelText("是否记录颜色"), ShowIf("hideGradient")]
    public bool recordColor;

    [HideIf("hideColor")]
    [BoxGroup("Color")]
    [PropertyOrder(10)]
    public Color color = Color.white;

    [HideIf("hideGradient"), BoxGroup("Color")]
    public Gradient gradient;

    [EnumToggleButtons, HideLabel]
    [BoxGroup("Color")]
    [PropertyOrder(10)]
    public ColorType colorType;

    [MinValue(0)]
    [HorizontalGroup]
    [LabelText("持续时间")]
    public float during;

    [MaxValue(1)]
    [ShowIf("showHSVInfo")]
    [PropertyOrder(1)]
    [BoxGroup("Color")]
    public Vector3 hsvValue;

    [MinValue(0)]
    [MaxValue(1)]
    [ShowIf("showDarkInfo")]
    [PropertyOrder(1)]
    [BoxGroup("Color")]
    public Vector2 darkValue;

    [ShowIf("showTextureMappingInfo")][BoxGroup("Color")]
    public int textureIndex;
    [ShowIf("showColorMappingInfo")][BoxGroup("Color")]
    public int colorIndex;


    bool hideColor { get { return colorType != ColorType.SingleColor; } }
    bool hideGradient { get { return colorType != ColorType.Gradient; } }
    bool showHSVInfo { get { return colorType == ColorType.HSV; } }
    bool showDarkInfo { get { return colorType == ColorType.Dark; } }
    bool showColorMappingInfo { get { return colorType == ColorType.ColorMapping; } }
    bool showTextureMappingInfo{ get { return colorType == ColorType.TextureMapping; } }

    public override Tween GetOrder(ColorPoint point)
    {
        Color targetColor = Color.white;
        switch (colorType)
        {
            case ColorType.SingleColor:
                {
                    targetColor = color; break;
                }
            case ColorType.TextureMapping:
                {
                    targetColor = point.GetTextureColor(textureIndex); break;
                }
            case ColorType.ColorMapping:
                {
                    targetColor = point.GetMappingColor(colorIndex); break;
                }
            case ColorType.Random:
                {
                    targetColor = point.randomColor; break;
                }
            case ColorType.FlowMapping:
                {
                    targetColor = point.flowTextureColor; break;
                }
            case ColorType.HSV:
                {
                    targetColor = point.GetColorByHSV(hsvValue); break;
                }
            case ColorType.Origin:
                {
                    targetColor = point.originalColor; break;
                }
            case ColorType.Dark:
                {
                    targetColor = point.GetDarkColor(darkValue); break;
                }

        }
        if (recordColor)
        {
            point.originalColor = targetColor;
            if (!hideGradient)
                Debug.LogError("暂不支持记录复合流光的颜色");
        }

        if (!hideGradient)
        {
            if (during == 0)
                Debug.LogError("渐进颜色持续时间不能为0" + point.gameObject.name);
            return point.mat.DOGradientColor(gradient, during);
        }
        else
            return point.mat.DOColor(targetColor, during);
        // Debug.LogError("colorType未选择!");
        // return null;
    }
}


