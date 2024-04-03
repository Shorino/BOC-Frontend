using UnityEngine;

public class SpawnQuickOrderSystem : MonoBehaviour
{
    SpawnQuickOrderComponent spawnQuickOrderComponent;
    QuickTabComponent quickTabComponent;

    void Start()
    {
        spawnQuickOrderComponent = GlobalComponent.instance.spawnQuickOrderComponent;
        quickTabComponent = GlobalComponent.instance.quickTabComponent;

        spawnQuickOrderComponent.onChange_quickOrderToSpawn.AddListener(SpawnQuickOrder);
    }

    void SpawnQuickOrder(General.WebsocketGetQuickOrderResponse response)
    {
        if (quickTabComponent.spawnedQuickOrderObjects.ContainsKey(response.id)) return;

        GameObject quickOrderDataRowObject = Instantiate(quickTabComponent.quickOrderDataRowPrefab);
        quickOrderDataRowObject.transform.SetParent(quickTabComponent.orderInfoTransform, false);
        QuickOrderDataRowComponent quickOrderDataRowComponent = quickOrderDataRowObject.GetComponent<QuickOrderDataRowComponent>();
        quickTabComponent.spawnedQuickOrderObjects.TryAdd(response.id, quickOrderDataRowObject);

        quickOrderDataRowComponent.orderId = response.id;
        quickOrderDataRowComponent.symbolText.text = response.symbol.ToUpper();
        string positionSide = response.isLong ? "LONG" : "SHORT";
        Color positionSideColor = response.isLong ? OrderConfig.DISPLAY_COLOR_GREEN : Color.red;
        quickOrderDataRowComponent.positionSideText.text = positionSide;
        quickOrderDataRowComponent.positionSideText.color = positionSideColor;
        quickOrderDataRowComponent.entryPriceText.text = Utils.RoundTwoDecimal(response.entryPrice).ToString();
        quickOrderDataRowComponent.atrTimeframeText.text = OrderConfig.TIMEFRAME_ARRAY[(int)response.atrTimeframe];
    }
}