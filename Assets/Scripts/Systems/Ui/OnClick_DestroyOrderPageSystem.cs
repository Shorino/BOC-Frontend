using UnityEngine;
using UnityEngine.EventSystems;

public class OnClick_DestroyOrderPageSystem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    InputComponent inputComponent;
    OrderPagesComponent orderPagesComponent;
    PromptComponent promptComponent;
    bool hovering;

    void Start()
    {
        inputComponent = GlobalComponent.instance.inputComponent;
        orderPagesComponent = GlobalComponent.instance.orderPagesComponent;
        promptComponent = GlobalComponent.instance.promptComponent;

        inputComponent.click.performed += _ =>
        {
            if (hovering)
            {
                if (orderPagesComponent.childOrderPageComponents.Count == 0) return;
                int pageIndex = (int)orderPagesComponent.currentPageIndex;
                if (orderPagesComponent.childOrderPageComponents[pageIndex].orderStatus == OrderStatusEnum.UNSUBMITTED)
                {
                    orderPagesComponent.childOrderPageComponents[pageIndex].destroySelf = true;
                }
                else
                {
                    switch (orderPagesComponent.childOrderPageComponents[pageIndex].orderStatus)
                    {
                        case OrderStatusEnum.SUBMITTED:
                            if (orderPagesComponent.childOrderPageComponents[pageIndex].orderStatusError)
                                orderPagesComponent.childOrderPageComponents[pageIndex].cancelErrorOrderButton.onClick.Invoke();
                            else
                                orderPagesComponent.childOrderPageComponents[pageIndex].cancelOrderButton.onClick.Invoke();
                            break;
                        case OrderStatusEnum.FILLED:
                            if (orderPagesComponent.childOrderPageComponents[pageIndex].orderStatusError)
                                orderPagesComponent.childOrderPageComponents[pageIndex].closeErrorPositionButton.onClick.Invoke();
                            else
                                orderPagesComponent.childOrderPageComponents[pageIndex].closePositionButton.onClick.Invoke();
                            break;
                    }
                    promptComponent.leftButton.onClick.AddListener(() =>
                    {
                        orderPagesComponent.childOrderPageComponents[pageIndex].destroySelf = true;
                    });
                }
            }
        };
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}