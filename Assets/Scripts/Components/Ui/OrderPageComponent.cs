using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrderPageComponent : MonoBehaviour
{
    [Header("Reference")]
    public TMP_Text orderTitleText;
    public Button orderIdButton;
    public TMP_Text orderIdText;
    public OrderPageSymbolDropdownComponent symbolDropdownComponent;
    public TMP_InputField maxLossPercentageInput;
    public TMP_InputField maxLossAmountInput;
    public OrderPageInputEntryPricesComponent inputEntryPricesComponent;
    public TMP_InputField entryTimesInput;
    public TMP_InputField stopLossInput;
    public TMP_InputField takeProfitInput;
    public OrderPageResultComponent resultComponent;
    public GameObject dataRowPrefab;
    public TMP_Dropdown marginDistributionModeDropdown;
    public GameObject marginWeightDistributionValueObject;
    public Slider marginWeightDistributionValueSlider;
    public TMP_InputField marginWeightDistributionValueInput;
    public Button calculateButton;
    public TMP_Text calculateButtonText;
    public GameObject takeProfitTypeObject;
    public TMP_Dropdown takeProfitTypeDropdown;
    public GameObject riskRewardRatioObject;
    public TMP_InputField riskRewardRatioInput;
    public Button riskRewardMinusButton;
    public Button riskRewardAddButton;
    public GameObject takeProfitTrailingCallbackPercentageObject;
    public Slider takeProfitTrailingCallbackPercentageSlider;
    public EventTrigger takeProfitTrailingCallbackPercentageSliderTrigger;
    public TMP_Text takeProfitTrailingCallbackPercentageMinText;
    public TMP_Text takeProfitTrailingCallbackPercentageMaxText;
    public TMP_InputField takeProfitTrailingCallbackPercentageInput;
    public GameObject orderTypeObject;
    public TMP_Dropdown orderTypeDropdown;
    public GameObject applyButtonObject;
    public Button placeOrderButton;
    public Button cancelOrderButton;
    public Button closePositionButton;
    public Button cancelErrorOrderButton;
    public Button closeErrorPositionButton;
    public GameObject positionInfoObject;
    public TMP_Text positionInfoAvgEntryPriceFilledText;
    public TMP_Text positionInfoActualTakeProfitPriceText;
    public TMP_Text positionInfoQuantityFilledText;
    public TMP_Text positionInfoPaidFundingAmount;
    public OrderPageThrottleParentComponent throttleParentComponent;
    public GameObject throttleObject;

    [Header("Runtime")]
    public bool restoreData;
    public bool destroySelf;
    public bool calculate;
    public bool saveToServer;
    public bool updateToServer;
    public bool deleteFromServer;
    public bool submitToServer;
    public CalculateMargin marginCalculator;
    public bool lockForEdit;
    public string orderId;
    public OrderStatusEnum orderStatus = OrderStatusEnum.UNSUBMITTED;
    public bool orderStatusError = false;
    public Tween spawnTween;
}