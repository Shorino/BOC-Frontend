using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class OrderPagesWebsocketResponseSystem : MonoBehaviour
{
    WebsocketComponent websocketComponent;
    OrderPagesComponent orderPagesComponent;
    PlatformComponent platformComponent;
    PromptComponent promptComponent;

    void Start()
    {
        websocketComponent = GlobalComponent.instance.websocketComponent;
        orderPagesComponent = GlobalComponent.instance.orderPagesComponent;
        platformComponent = GlobalComponent.instance.platformComponent;
        promptComponent = GlobalComponent.instance.promptComponent;
    }
    void Update()
    {
        SubmitOrderToServerResponse();
        PositionInfoUpdateResponse();
        SubmitThrottleToServerResponse();
    }

    void PositionInfoUpdateResponse()
    {
        string jsonString = websocketComponent.RetrieveGeneralResponses(WebsocketEventTypeEnum.POSITION_INFO_UPDATE);
        websocketComponent.RemovesGeneralResponses(WebsocketEventTypeEnum.POSITION_INFO_UPDATE);
        if (jsonString.IsNullOrEmpty()) return;

        General.WebsocketPositionInfoUpdateResponse response = JsonConvert.DeserializeObject
        <General.WebsocketPositionInfoUpdateResponse>(jsonString, JsonSerializerConfig.settings);

        OrderPageComponent orderPageComponent = null;
        foreach (OrderPageComponent component in orderPagesComponent.childOrderPageComponents)
        {
            if (component.orderId.Equals(response.orderId))
            {
                orderPageComponent = component;
            }
        }
        if (orderPageComponent == null) return;

        if (response.averagePriceFilled.HasValue && platformComponent.pricePrecisions.ContainsKey(orderPageComponent.symbolDropdownComponent.selectedSymbol))
        {
            orderPageComponent.positionInfoAvgEntryPriceFilledText.text = Utils.RoundNDecimal(response.averagePriceFilled.Value, platformComponent.pricePrecisions[orderPageComponent.symbolDropdownComponent.selectedSymbol]).ToString();
        }
        if (response.quantityFilled.HasValue && platformComponent.quantityPrecisions.ContainsKey(orderPageComponent.symbolDropdownComponent.selectedSymbol))
        {
            orderPageComponent.positionInfoQuantityFilledText.text = Utils.RoundNDecimal(response.quantityFilled.Value, platformComponent.quantityPrecisions[orderPageComponent.symbolDropdownComponent.selectedSymbol]).ToString();
        }
        if (response.actualTakeProfitPrice.HasValue && platformComponent.pricePrecisions.ContainsKey(orderPageComponent.symbolDropdownComponent.selectedSymbol))
        {
            orderPageComponent.positionInfoActualTakeProfitPriceText.text = Utils.RoundNDecimal(response.actualTakeProfitPrice.Value, platformComponent.pricePrecisions[orderPageComponent.symbolDropdownComponent.selectedSymbol]).ToString();
        }
        if (response.paidFundingAmount.HasValue)
        {
            orderPageComponent.positionInfoPaidFundingAmount.text = response.paidFundingAmount.Value.ToString();
        }
    }
    void SubmitOrderToServerResponse()
    {
        string jsonString = websocketComponent.RetrieveGeneralResponses(WebsocketEventTypeEnum.SUBMIT_ORDER);
        websocketComponent.RemovesGeneralResponses(WebsocketEventTypeEnum.SUBMIT_ORDER);
        if (jsonString.IsNullOrEmpty()) return;

        General.WebsocketSubmitOrderResponse response = JsonConvert.DeserializeObject
        <General.WebsocketSubmitOrderResponse>(jsonString, JsonSerializerConfig.settings);

        OrderPageComponent orderPageComponent = null;
        foreach (OrderPageComponent component in orderPagesComponent.childOrderPageComponents)
        {
            if (component.orderId.Equals(response.orderId))
            {
                orderPageComponent = component;
            }
        }
        if (orderPageComponent == null) return;

        orderPageComponent.orderStatus = response.status;
        orderPageComponent.orderStatusError = response.statusError;
        if (orderPageComponent.orderStatus == OrderStatusEnum.UNSUBMITTED)
        {
            orderPageComponent.tradingBotId = "";
            orderPageComponent.positionInfoBotInChargeDropdown.value = 0;
        }
        if (orderPageComponent.resultComponent.orderInfoDataObject != null)
        {
            orderPageComponent.resultComponent.orderInfoDataObject.transform.GetChild(3).GetComponent<TMP_Text>().text = orderPageComponent.orderStatus.ToString();
        }
        if (orderPageComponent.orderStatusError && !response.message.IsNullOrEmpty())
        {
            switch (platformComponent.activePlatform)
            {
                case PlatformEnum.BINANCE:
                case PlatformEnum.BINANCE_TESTNET:
                    Binance.WebrequestGeneralResponse errorResponse = JsonConvert.DeserializeObject<Binance.WebrequestGeneralResponse>(response.message, JsonSerializerConfig.settings);
                    if (errorResponse.code.HasValue)
                    {
                        string message = errorResponse.msg + " (Binance Error Code: " + errorResponse.code.Value + ")";
                        promptComponent.ShowPrompt(PromptConstant.ERROR, message, () =>
                        {
                            promptComponent.active = false;
                        });
                    }
                    break;
            }
        }
    }
    void SubmitThrottleToServerResponse()
    {
        string jsonString = websocketComponent.RetrieveGeneralResponses(WebsocketEventTypeEnum.SUBMIT_THROTTLE_ORDER);
        websocketComponent.RemovesGeneralResponses(WebsocketEventTypeEnum.SUBMIT_THROTTLE_ORDER);
        if (jsonString.IsNullOrEmpty()) return;

        General.WebsocketSubmitOrderResponse response = JsonConvert.DeserializeObject
        <General.WebsocketSubmitOrderResponse>(jsonString, JsonSerializerConfig.settings);

        OrderPageThrottleComponent orderPageThrottleComponent = null;
        foreach (OrderPageComponent component in orderPagesComponent.childOrderPageComponents)
        {
            foreach (OrderPageThrottleComponent throttleComponent in component.throttleParentComponent.orderPageThrottleComponents)
            {
                if (throttleComponent.orderId.Equals(response.orderId))
                {
                    orderPageThrottleComponent = throttleComponent;
                }
            }
        }
        if (orderPageThrottleComponent == null) return;

        orderPageThrottleComponent.orderStatus = response.status;
        orderPageThrottleComponent.orderStatusError = response.statusError;
        if (orderPageThrottleComponent.orderStatusError && !response.message.IsNullOrEmpty())
        {
            switch (platformComponent.activePlatform)
            {
                case PlatformEnum.BINANCE:
                case PlatformEnum.BINANCE_TESTNET:
                    Binance.WebrequestGeneralResponse errorResponse = JsonConvert.DeserializeObject<Binance.WebrequestGeneralResponse>(response.message, JsonSerializerConfig.settings);
                    if (errorResponse.code.HasValue)
                    {
                        string message = errorResponse.msg + " (Binance Error Code: " + errorResponse.code.Value + ")";
                        promptComponent.ShowPrompt(PromptConstant.ERROR, message, () =>
                        {
                            promptComponent.active = false;
                        });
                    }
                    break;
            }
        }
    }
}