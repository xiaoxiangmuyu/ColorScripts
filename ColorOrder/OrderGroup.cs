﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
[System.Serializable]
public class OrderGroup:GradualOrder
{
    public List<ColorOrderBase>colorOrders=new List<ColorOrderBase>();
    
}