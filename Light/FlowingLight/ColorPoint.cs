﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
[RequireComponent(typeof(BoxCollider))]
public class ColorPoint : MonoBehaviour
{
    private Renderer curRenderer;
    public Material mat;
    protected ColorMapping colorMapping;
    [ShowInInspector]
    [SerializeField]
    protected bool isbusy;
    public Color targetColor;
    public bool IsBusy { get { return isbusy; } }
    public Color originalColor;
    public List<string> filterTags = new List<string>();
    #region Colors
    public Color mappingColor { get { return GetMappingColor(); } }
    public Color flowMappingColor { get { var temp = colorMapping as OffsetMapping; return temp.GetFlowMappingColor(transform); } }
    public Color hsvColor { get { return GetColorByHSV(); } }
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
    float h, s, v;
    #endregion
    private void WorkBegin()
    {
        isbusy = true;
    }
    private void WorkComplete()
    {
        isbusy = false;
    }
    protected virtual void Awake()
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

        colorMapping = GetComponentInParent<ColorMapping>();
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerBase TriggerBase = other.GetComponent<TriggerBase>();
        if (TriggerBase)
        {
            if ((TriggerBase.filterTags.Count != 0) && !FilterCompare(TriggerBase))
            {
                return;
            }
        }
        else
        {
            Debug.LogError("碰撞体没有TriggerBase组件");
            return;
        }
        SetProcessType(TriggerBase.colorOrders);
    }
    public void SetProcessType(List<ColorOrderBase> colorOrders)
    {
        if (isbusy)
            return;
        WorkBegin();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(ProcessOrder(colorOrders));
        sequence.AppendCallback(delegate { WorkComplete(); });
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
                sequence.AppendCallback(delegate { temp.GetCallBack(this); });
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
    public void GradualColor(Color color, float during)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(mat.DOColor(color, during));
    }
    public void ShowColorMapping(float during = 0)
    {
        colorMapping.SetColor(transform);
        if (during == 0)
            return;
        StartCoroutine(DelayFunc(during, delegate { mat.color=Color.black; }));
    }
    private IEnumerator DelayFunc(float delayTime, System.Action callback)
    {
        yield return new WaitForSeconds(delayTime);
        callback();
    }

    public void SetColor(Color targetColor, bool needResetColor = false, float CDTime = 0.1f)
    {
        if (mat)
        {
            if (mat.color != targetColor)
            {
                originalColor = mat.color;
                mat.color = targetColor;

                if (needResetColor)
                {
                    StartCoroutine(DelayFunc(CDTime, delegate { mat.color = originalColor; }));
                }
            }
        }
        else
        {
            Debug.LogError("Material is null" + gameObject.name);
        }
    }
    int texIndex;
    int texCounter;
    private Color GetMappingColor()
    {
        texCounter += 1;
        if (texCounter <= colorMapping.texChangeCount)
        {
            return colorMapping.GetMappingColor(transform, texIndex);
        }
        else
        {
            texCounter = 1;
            if (texIndex + 1 <= colorMapping.scrTexs.Count - 1)
                texIndex += 1;
            else
            {
                if(colorMapping.isTexLoop)
                texIndex=0;
            }

            return colorMapping.GetMappingColor(transform, texIndex);
        }
    }
    private Color GetColorByHSV()
    {
        //Color.RGBToHSV(mat.color, out h, out s, out v);
        if (h < 1)
            h += 0.2f;// hue范围是[0,360]/360，这里每次累加10
        else
            h = 0;
        Color targetColor = Color.HSVToRGB(h, 0.45f, 1f);
        //Debug.Log(h);
        return targetColor;
    }

    public Color GetOriginalColor()
    {
        return originalColor;
    }
    public void TurnOff()
    {
        mat.color = Color.black;
    }
    private bool FilterCompare(TriggerBase tb)
    {
        foreach (var tag in tb.filterTags)
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


}
