﻿using System.Collections.Generic;
using UnityEngine;

public class RetrieveOrdersComponent : MonoBehaviour
{
    public Dictionary<PlatformEnum, Dictionary<string, General.WebsocketRetrieveOrdersData>> ordersFromServer = new();
    public Dictionary<PlatformEnum, Dictionary<string, General.WebsocketRetrieveQuickOrdersData>> quickOrdersFromServer = new();
    public bool destroyOrders = false;
    public bool instantiateOrders = false;
    public bool updateOrderStatus = false;
}