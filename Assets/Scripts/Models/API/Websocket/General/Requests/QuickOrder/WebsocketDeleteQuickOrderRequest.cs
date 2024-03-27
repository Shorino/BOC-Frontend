using System;

namespace General
{
    [Serializable]
    public class WebsocketDeleteQuickOrderRequest : WebsocketGeneralRequest
    {
        public WebsocketOrderIdRequest orderRequest;

        public WebsocketDeleteQuickOrderRequest(string token,
            string orderId) : base(WebsocketEventTypeEnum.DELETE_QUICK_ORDER, token)
        {
            orderRequest = new(orderId);
        }
    }
}