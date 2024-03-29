using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public class OrderPageSpawnOrderBasedOnServerSignalSystem : MonoBehaviour
{
    WebsocketComponent websocketComponent;
    OrderPagesComponent orderPagesComponent;
    RetrieveOrdersComponent retrieveOrdersComponent;
    PlatformComponent platformComponent;
    HideAllPanelComponent hideAllPanelComponent;

    void Start()
    {
        websocketComponent = GlobalComponent.instance.websocketComponent;
        orderPagesComponent = GlobalComponent.instance.orderPagesComponent;
        retrieveOrdersComponent = GlobalComponent.instance.retrieveOrdersComponent;
        platformComponent = GlobalComponent.instance.platformComponent;
        hideAllPanelComponent = GlobalComponent.instance.hideAllPanelComponent;
    }
    void Update()
    {
        string spawnOrderString = websocketComponent.RetrieveGeneralResponses(WebsocketEventTypeEnum.SPAWN_ORDER.ToString());
        if (spawnOrderString.IsNullOrEmpty()) return;
        General.WebsocketSpawnOrderResponse response = JsonConvert.DeserializeObject<General.WebsocketSpawnOrderResponse>(spawnOrderString, JsonSerializerConfig.settings);
        websocketComponent.RemovesGeneralResponses(WebsocketEventTypeEnum.SPAWN_ORDER.ToString());

        if (response.order == null) // destroy
        {
            foreach (OrderPageComponent orderPageComponent in orderPagesComponent.childOrderPageComponents)
            {
                if (orderPageComponent.orderId == response.orderId)
                {
                    orderPageComponent.destroySelf = true;
                    return;
                }
            }
        }
        else // spawn
        {
            #region Set order pages status and page index
            orderPagesComponent.status = OrderPagesStatusEnum.IMMERSIVE;
            orderPagesComponent.currentPageIndex = orderPagesComponent.transform.childCount;
            #endregion

            #region Add into retrieveOrdersComponent.ordersFromServer[platformComponent.tradingPlatform]
            if (retrieveOrdersComponent.ordersFromServer.ContainsKey(platformComponent.tradingPlatform))
            {
                if (!retrieveOrdersComponent.ordersFromServer[platformComponent.tradingPlatform].TryAdd(response.orderId, response.order))
                {
                    retrieveOrdersComponent.ordersFromServer[platformComponent.tradingPlatform][response.orderId] = response.order;
                }
            }
            #endregion

            #region Instantiate order page object
            GameObject orderPageObject = Instantiate(orderPagesComponent.orderPagePrefab);
            orderPageObject.transform.SetParent(orderPagesComponent.transform, false);
            OrderPageComponent orderPageComponent = orderPageObject.GetComponent<OrderPageComponent>();
            orderPageComponent.orderId = response.orderId;
            orderPageComponent.restoreData = true;
            #endregion

            #region Hide all tabs
            hideAllPanelComponent.hideNow = "true";
            #endregion
        }
    }
}