using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

public class WebsocketGeneralResponsesCleanUpSystem : MonoBehaviour
{
    WebsocketComponent websocketComponent;
    OrderPagesComponent orderPagesComponent;

    void Start()
    {
        websocketComponent = GlobalComponent.instance.websocketComponent;
        orderPagesComponent = GlobalComponent.instance.orderPagesComponent;
    }
    void Update()
    {
        RetrievePositionInfo();
        SaveOrder();
        SaveThrottleOrder();
    }

    void RetrievePositionInfo()
    {
        string retrievePositionInfoString = websocketComponent.RetrieveGeneralResponses(WebsocketEventTypeEnum.RETRIEVE_POSITION_INFO.ToString());
        if (retrievePositionInfoString.IsNullOrEmpty()) return;
        General.WebsocketRetrievePositionInfoResponse response = JsonConvert.DeserializeObject<General.WebsocketRetrievePositionInfoResponse>(retrievePositionInfoString, JsonSerializerConfig.settings);

        bool orderExist = false;
        orderPagesComponent.childOrderPageComponents.ForEach(orderPageComponent =>
        {
            if (response.orderId.Equals(orderPageComponent.orderId))
            {
                orderExist = true;
            }
        });
        if (!orderExist)
        {
            websocketComponent.RemovesGeneralResponses(WebsocketEventTypeEnum.RETRIEVE_POSITION_INFO.ToString());
        }
    }
    void SaveOrder()
    {
        string saveOrderString = websocketComponent.RetrieveGeneralResponses(WebsocketEventTypeEnum.SAVE_ORDER.ToString());
        if (saveOrderString.IsNullOrEmpty()) return;
        General.WebsocketSaveOrderResponse response = JsonConvert.DeserializeObject<General.WebsocketSaveOrderResponse>(saveOrderString, JsonSerializerConfig.settings);

        bool orderExist = false;
        orderPagesComponent.childOrderPageComponents.ForEach(orderPageComponent =>
        {
            if (response.orderId.Equals(orderPageComponent.orderId))
            {
                orderExist = true;
            }
        });
        if (!orderExist)
        {
            websocketComponent.RemovesGeneralResponses(WebsocketEventTypeEnum.SAVE_ORDER.ToString());
        }
    }
    void SaveThrottleOrder()
    {
        string saveThrottleOrderString = websocketComponent.RetrieveGeneralResponses(WebsocketEventTypeEnum.SAVE_THROTTLE_ORDER.ToString());
        if (saveThrottleOrderString.IsNullOrEmpty()) return;
        General.WebsocketSaveThrottleOrderResponse response = JsonConvert.DeserializeObject<General.WebsocketSaveThrottleOrderResponse>(saveThrottleOrderString, JsonSerializerConfig.settings);

        bool orderExist = false;
        orderPagesComponent.childOrderPageComponents.ForEach(orderPageComponent =>
        {
            orderPageComponent.throttleParentComponent.orderPageThrottleComponents.ForEach(orderPageThrottleComponent =>
            {
                if (response.orderId.Equals(orderPageThrottleComponent.orderId) && response.parentOrderId.Equals(orderPageComponent.orderId))
                {
                    orderExist = true;
                }
            });
        });
        if (!orderExist)
        {
            websocketComponent.RemovesGeneralResponses(WebsocketEventTypeEnum.SAVE_THROTTLE_ORDER.ToString());
        }
    }
}
