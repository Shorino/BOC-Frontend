using UnityEngine;
using UnityEngine.EventSystems;

public class OnClick_SwitchOrderPageStatusSystem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool selective;

    InputComponent inputComponent;
    OrderPagesComponent orderPagesComponent;
    HideAllPanelComponent hideAllPanelComponent;
    bool hovering;

    void Start()
    {
        inputComponent = GlobalComponent.instance.inputComponent;
        orderPagesComponent = GlobalComponent.instance.orderPagesComponent;
        hideAllPanelComponent = GlobalComponent.instance.hideAllPanelComponent;

        inputComponent.click.performed += _ =>
        {
            if (hovering && inputComponent.drag.ReadValue<Vector2>().Equals(Vector2.zero))
            {
                hideAllPanelComponent.hideNow = "true";
                switch (orderPagesComponent.status)
                {
                    case OrderPagesStatusEnum.IMMERSIVE:
                        orderPagesComponent.status = OrderPagesStatusEnum.DETACH;
                        break;
                    case OrderPagesStatusEnum.DETACH:
                        orderPagesComponent.status = OrderPagesStatusEnum.IMMERSIVE;
                        if (selective)
                        {
                            orderPagesComponent.currentPageIndex = transform.parent.GetSiblingIndex();
                        }
                        break;
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
