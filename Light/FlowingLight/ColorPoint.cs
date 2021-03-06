﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
[RequireComponent(typeof(SphereCollider))]
public class ColorPoint : MonoBehaviour
{
    Renderer curRenderer;
    ColorParent colorParent;
    PointState state;


    [HideInInspector]
    public Material mat { get; private set; }


    [ShowInInspector]
    [PropertyOrder(1)]
    public PointState State { get { return state; } }


    [ReadOnly]
    public Color originalColor;


    [PropertyOrder(2)]
    public List<string> filterTags = new List<string>();


    #region Colors
    public Color flowTextureColor { get { var temp = colorParent as OffsetMapping; if (!temp) { Debug.LogError("flowTextureColor为空"); return Color.white; } return temp.GetMappingColor(transform); } }
    //public Color hsvColor { get { return GetColorByHSV(); } }
    public Color randomColor
    {
        get
        {
            float red = Random.Range(0.0f, 1.0f);
            float green = Random.Range(0.0f, 1.0f);
            float blue = Random.Range(0.0f, 1.0f);
            return new Color(red, green, blue);
        }
    }
    #endregion


    void WorkBegin()
    {
        state = PointState.Busy;
    }
    void WorkComplete()
    {
        state = PointState.Idle;
    }
    void Awake()
    {
        curRenderer = GetComponent<Renderer>();

        if (curRenderer)
        {
            mat = curRenderer.material;

            if (!mat)
            {
                Debug.LogError("Material is null");
                return;
            }
        }
        else
        {
            Debug.LogErrorFormat("Renderer is null, name: {0}", name);
            return;
        }

        colorParent = GetComponentInParent<ColorParent>();
    }


    void OnTriggerEnter(Collider other)
    {
        TriggerBase TriggerBase = other.GetComponent<TriggerBase>();
        if (TriggerBase)
        {
            if ((TriggerBase.targetTags.Count != 0) && !FilterCompare(TriggerBase.targetTags))
            {
                return;
            }
            if ((TriggerBase.ignoreTags.Count != 0) && FilterCompare(TriggerBase.ignoreTags))
            {
                return;
            }
        }
        else
        {
            Debug.LogError("碰撞体没有TriggerBase组件");
            return;
        }
        if (TriggerBase.record != null)
        {
            if (TriggerBase.record.objParent == string.Empty)
                TriggerBase.record.objParent = transform.root.name;
            if (TriggerBase.record.objs.Exists((x) => x == gameObject.name))
                return;
            if (TriggerBase.recordTimer == 0)
            {
                TriggerBase.record.objs.Add(gameObject.name);
                TriggerBase.record.times.Add(0);
                TriggerBase.recordTimer = Time.time;
            }
            else
            {
                TriggerBase.record.times.Add(Time.time - TriggerBase.recordTimer);
                TriggerBase.record.objs.Add(gameObject.name);
            }
            mat.DOColor(Color.red, 0.5f);
            return;
        }
        if (TriggerBase.orderFile != null)
        {
            SetProcessType(TriggerBase.orderFile.colorOrders);

        }
        else
        {
            SetProcessType(TriggerBase.colorOrders);
        }
    }


    void OnTriggerExit(Collider other)
    {
        if (!isTriggerTarget(other))
            return;
        if (other.GetComponent<TriggerBase>().useExitOrder)
        {
            SetProcessType(other.GetComponent<TriggerBase>().exitOrders);
        }
    }
    bool isTriggerTarget(Collider other)
    {
        TriggerBase TriggerBase = other.GetComponent<TriggerBase>();
        if (TriggerBase)
        {
            if ((TriggerBase.targetTags.Count != 0) && !FilterCompare(TriggerBase.targetTags))
            {
                return false;
            }
            if ((TriggerBase.ignoreTags.Count != 0) && FilterCompare(TriggerBase.targetTags))
            {
                return false;
            }
        }
        else
        {
            Debug.LogError("碰撞体没有TriggerBase组件");
            return false;
        }
        return true;
    }

    bool FilterCompare(List<string> tags)
    {
        foreach (var tag in tags)
        {
            foreach (var thisTag in this.filterTags)
            {
                if (string.Equals(thisTag, tag))
                {
                    return true;
                }
            }
        }
        return false;
    }


    Sequence ProcessOrder(List<ColorOrderBase> colorOrders)
    {
        Sequence sequence = DOTween.Sequence();
        if (colorOrders == null)
        {
            Debug.LogError("命令列表为空");
            return null;
        }
        foreach (var order in colorOrders)
        {
            if (order == null)
            {
                Debug.LogError("命令为空!");
                return null;
            }
            if (order is Interval)
            {
                Interval temp = order as Interval;
                sequence.AppendInterval(temp.during);
            }
            else if (order is CallBack)
            {
                var temp = (CallBack)order;
                sequence.AppendCallback(delegate { temp.GetCallBack(); });
            }
            else if (order is OrderGroup)
            {
                var temp = (OrderGroup)order;
                Sequence sequence1 = DOTween.Sequence();
                for (int i = 0; i < temp.playCount; i++)
                {
                    sequence1.Append(ProcessOrder(temp.colorOrders));
                }
                for (int k = 0; k < temp.playCount; k++)
                {
                    sequence.Append(sequence1);
                }
            }
            else if (order is GradualOrder)
            {
                var temp = (GradualOrder)order;
                for (int i = 0; i < temp.playCount; i++)
                {
                    sequence.Append(order.GetOrder(this));
                }
            }
            else
                Debug.LogError("error");

        }
        return sequence;
    }


    int texIndex;
    int texCounter;
    public Color GetTextureColor(int targetTexIndex=-1)
    {
        TextureMapping textureMapping = colorParent as TextureMapping;
        if (!textureMapping)
        {
            Debug.LogError(gameObject.name + "TextureMapping为空");
            return Color.white;
        }
        if(targetTexIndex!=-1)
        return textureMapping.GetMappingColor(transform, targetTexIndex);

        texCounter += 1;
        if (texCounter <= textureMapping.texChangeCount)
        {
            return textureMapping.GetMappingColor(transform, texIndex);
        }
        else
        {
            texCounter = 1;
            if (texIndex + 1 <= textureMapping.scrTexs.Count - 1)
                texIndex += 1;
            else
            {
                if (textureMapping.isTexLoop)
                    texIndex = 0;
            }

            return textureMapping.GetMappingColor(transform, texIndex);
        }
    }


    public Color GetMappingColor(int targetTexIndex=-1)
    {
        ColorMapping colorMapping = colorParent as ColorMapping;
        if (!colorMapping)
        {
            Debug.LogError(gameObject.name + "ColorMapping为空");
            return Color.white;
        }
        if(targetTexIndex!=-1)
        return colorMapping.GetMappingColor(transform, targetTexIndex);

        texCounter += 1;
        if (texCounter <= colorMapping.ColorChangeCount)
        {
            return colorMapping.GetMappingColor(transform, texIndex);
        }
        else
        {
            texCounter = 1;
            if (texIndex + 1 <= colorMapping.colors.Count - 1)
                texIndex += 1;
            else
            {
                if (colorMapping.isColorLoop)
                    texIndex = 0;
            }

            return colorMapping.GetMappingColor(transform, texIndex);
        }
    }


    float h, s, v;//for hsv effect
    public Color GetColorByHSV(Vector3 value)
    {
        //Color.RGBToHSV(mat.color, out h, out s, out v);
        if (h < 1)
            h += value.x;// hue范围是[0,360]/360，这里每次累加10
        else
            h = 0;
        Color targetColor = Color.HSVToRGB(h, value.y, value.z);
        //Debug.Log(h);
        return targetColor;
    }


    float _h, _s, _v;//for dark effect
    public Color GetDarkColor(Vector2 value)
    {
        if (Color.Equals(originalColor, new Color(0, 0, 0, 0)))
            Debug.LogError(gameObject.name + "没有记录其他颜色，无法变暗");
        Color.RGBToHSV(originalColor, out _h, out _s, out _v);
        Color targetColor = Color.HSVToRGB(_h, value.x, value.y);
        return targetColor;
    }


    public void SetProcessType(List<ColorOrderBase> colorOrders, bool forceMode = false)
    {
        if (state == PointState.Busy && !forceMode)
        {
            return;
        }
        WorkBegin();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(ProcessOrder(colorOrders));
        sequence.AppendCallback(delegate { WorkComplete(); });
    }


    [Button(ButtonSizes.Gigantic)]
    public void AddTag(string tag)
    {
        filterTags.Add(tag);
    }


}
