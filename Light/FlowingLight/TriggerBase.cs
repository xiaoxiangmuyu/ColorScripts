﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
public class TriggerBase : SerializedMonoBehaviour
{
    public bool useExitOrder;
    [HideInInspector]
    public RecordAsset record;
    [HideInInspector]
    public float recordTimer;



    [SerializeField]
    [ShowInInspector]
    private bool hideDataOperation = true;
    [HideIf("hideDataOperation")]
    [InlineEditor]
    public OrderData orderFile;
    [HorizontalGroup("Tags")]
    public List<string> targetTags = new List<string>(); // 影响的飞机的标签
    [HorizontalGroup("Tags")]
    public List<string> ignoreTags = new List<string>();
    [LabelText("命令序列")]
    public List<ColorOrderBase> colorOrders;
    [ShowIf("useExitOrder")]
    [LabelText("退出命令序列")]
    public List<ColorOrderBase> exitOrders;

    protected virtual void Awake()
    {

    }
    void Start()
    {
        if (record != null)
        {
            record.Clear();
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
    [Button(ButtonSizes.Gigantic)]
    [HideIf("hideDataOperation")]
    public void ReadOrderData(OrderData data)
    {
        if (!data)
        {
            Debug.LogError("存储文件为空");
            return;
        }
        colorOrders.Clear();
        // for (int i = 0; i < data.colorOrders.Count; i++)
        // {
        //     if (data.colorOrders[i] is DoColor)
        //     {
        //         DoColor temp = new DoColor();
        //         //temp=data.colorOrders[i] as DoColor;
        //         colorOrders.Add(temp);
        //     }
        // }
        foreach(var order in data.colorOrders)
        {
            colorOrders.Add(order);
        }
        Debug.Log("读取成功");
    }
    [Button(ButtonSizes.Gigantic)]
    [HideIf("hideDataOperation")]
    public void WriteData(OrderData data)
    {
        if (!data)
        {
            Debug.LogError("存储文件为空");
            return;
        }
        data.colorOrders.Clear();
        foreach (var order in colorOrders)
        {
            data.colorOrders.Add(order);
        }
        Debug.Log("写入成功");

    }
    DOTweenAnimation animation;
    public void SpeedAnimation()
    {
        if (animation == null)
        {
            animation = GetComponent<DOTweenAnimation>();
        }
        animation.duration += 50f;
        Debug.Log("trigger");
    }
}
