using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WebSocketSharp;
using MongoDB.Bson;
using DG.Tweening;
using System;

public class OrderPageSystem : MonoBehaviour
{
    OrderPageComponent orderPageComponent;
    WebsocketComponent websocketComponent;
    PromptComponent promptComponent;
    LoginComponent loginComponent;
    PlatformComponent platformComponent;
    ProfileComponent profileComponent;
    GetBalanceComponent getBalanceComponent;
    ClosePositionPromptComponent closePositionPromptComponent;

    bool? lockForEdit = null;
    OrderStatusEnum? orderStatus = null;
    bool calculateButtonPressed = false;

    void Start()
    {
        orderPageComponent = GetComponent<OrderPageComponent>();
        websocketComponent = GlobalComponent.instance.websocketComponent;
        promptComponent = GlobalComponent.instance.promptComponent;
        loginComponent = GlobalComponent.instance.loginComponent;
        platformComponent = GlobalComponent.instance.platformComponent;
        profileComponent = GlobalComponent.instance.profileComponent;
        getBalanceComponent = GlobalComponent.instance.getBalanceComponent;
        closePositionPromptComponent = GlobalComponent.instance.closePositionPromptComponent;

        if (orderPageComponent.orderId.IsNullOrEmpty())
            orderPageComponent.orderId = ObjectId.GenerateNewId().ToString();
        orderPageComponent.orderIdButton.onClick.AddListener(() =>
        {
            GUIUtility.systemCopyBuffer = orderPageComponent.orderId.ToString();
        });
        orderPageComponent.calculateButton.onClick.AddListener(() =>
        {
            if (orderPageComponent.lockForEdit)
            {
                orderPageComponent.lockForEdit = false;
                orderPageComponent.deleteFromServer = true;
            }
            else
            {
                orderPageComponent.calculate = true;
                calculateButtonPressed = true;
            }
        });
        orderPageComponent.placeOrderButton.onClick.AddListener(() =>
        {
            promptComponent.ShowSelection(PromptConstant.NOTICE, PromptConstant.PLACE_ORDER_PROMPT,
                PromptConstant.YES_PROCEED, PromptConstant.NO,
            () =>
            {
                orderPageComponent.placeOrderButton.interactable = false;
                orderPageComponent.submitToServer = true;
                promptComponent.active = false;
            },
            () =>
            {
                promptComponent.active = false;
            });
        });
        orderPageComponent.cancelOrderButton.onClick.AddListener(() =>
        {
            promptComponent.ShowSelection(PromptConstant.NOTICE, PromptConstant.CANCEL_ORDER_PROMPT,
                PromptConstant.YES_PROCEED, PromptConstant.NO,
            () =>
            {
                orderPageComponent.cancelOrderButton.interactable = false;
                orderPageComponent.submitToServer = true;
                promptComponent.active = false;
            },
            () =>
            {
                promptComponent.active = false;
            });
        });
        orderPageComponent.closePositionButton.onClick.AddListener(() =>
        {
            closePositionPromptComponent.show = orderPageComponent;
        });
        orderPageComponent.cancelErrorOrderButton.onClick.AddListener(() =>
        {
            promptComponent.ShowSelection(PromptConstant.NOTICE, PromptConstant.CANCEL_ORDER_PROMPT,
                PromptConstant.YES_PROCEED, PromptConstant.NO,
            () =>
            {
                orderPageComponent.cancelErrorOrderButton.interactable = false;
                orderPageComponent.submitToServer = true;
                promptComponent.active = false;
            },
            () =>
            {
                promptComponent.active = false;
            });
        });
        orderPageComponent.closeErrorPositionButton.onClick.AddListener(() =>
        {
            promptComponent.ShowSelection(PromptConstant.NOTICE, PromptConstant.CLOSE_POSITION_PROMPT,
                PromptConstant.YES_PROCEED, PromptConstant.NO,
            () =>
            {
                orderPageComponent.closeErrorPositionButton.interactable = false;
                orderPageComponent.submitToServer = true;
                promptComponent.active = false;
            },
            () =>
            {
                promptComponent.active = false;
            });
        });
        orderPageComponent.riskRewardRatioInput.onSubmit.AddListener(value => orderPageComponent.updateTakeProfitPrice = true);
        orderPageComponent.riskRewardMinusButton.onClick.AddListener(() =>
        {
            float rrr = float.Parse(orderPageComponent.riskRewardRatioInput.text);
            rrr -= 0.01f;
            orderPageComponent.riskRewardRatioInput.text = rrr.ToString();

            orderPageComponent.updateTakeProfitPrice = true;
        });
        orderPageComponent.riskRewardAddButton.onClick.AddListener(() =>
        {
            float rrr = float.Parse(orderPageComponent.riskRewardRatioInput.text);
            rrr += 0.01f;
            orderPageComponent.riskRewardRatioInput.text = rrr.ToString();

            orderPageComponent.updateTakeProfitPrice = true;
        });

        orderPageComponent.onChange_addToServer.AddListener(AddToServer);
        orderPageComponent.onChange_updateToServer.AddListener(UpdateToServer);
        orderPageComponent.onChange_deleteFromServer.AddListener(DeleteFromServer);
        orderPageComponent.onChange_submitToServer.AddListener(SubmitToServer);
        orderPageComponent.onChange_updateTakeProfitPrice.AddListener(UpdateTakeProfitPrice);

        orderPageComponent.orderIdText.text = "Order Id: " + orderPageComponent.orderId.ToString();

        RestoreFromProfilePreference();
    }
    void Update()
    {
        StartCoroutine(CalculateMargin());
        UpdateUiInteractableStatus();
    }
    void OnDestroy()
    {
        orderPageComponent.rectTransform.DOKill();
    }

    void RestoreFromProfilePreference()
    {
        if (orderPageComponent.instantiateWithData) return;

        Preference preference = profileComponent.activeProfile.preference;
        if (!preference.order.symbol.IsNullOrEmpty())
        {
            orderPageComponent.symbolDropdownComponent.selectedSymbol = preference.order.symbol.ToUpper();
        }
        orderPageComponent.maxLossPercentageInput.text = preference.order.lossPercentage == 0 ? "" : preference.order.lossPercentage.ToString();
        orderPageComponent.maxLossAmountInput.text = preference.order.lossAmount == 0 ? "" : preference.order.lossAmount.ToString();
        orderPageComponent.marginDistributionModeDropdown.value = (int)preference.order.marginDistributionMode;
        orderPageComponent.marginWeightDistributionValueCustomSlider.SetValue(preference.order.marginWeightDistributionValue);
        orderPageComponent.takeProfitTypeDropdown.value = (int)preference.order.takeProfitType;
        orderPageComponent.riskRewardRatioInput.text = preference.order.riskRewardRatio.ToString();
        orderPageComponent.takeProfitQuantityPercentageCustomSlider.SetValue(preference.order.takeProfitQuantityPercentage);
        orderPageComponent.takeProfitTrailingCallbackPercentageCustomSlider.SetValue(preference.order.takeProfitTrailingCallbackPercentage);
        orderPageComponent.orderTypeDropdown.value = (int)preference.order.orderType;
    }
    IEnumerator CalculateMargin()
    {
        if (!orderPageComponent.calculate) yield break;
        orderPageComponent.calculate = false;

        #region Update button
        orderPageComponent.calculateButton.interactable = false;
        orderPageComponent.lockForEdit = true;
        #endregion

        if (calculateButtonPressed)
        {
            #region Prepare data
            // get input
            string walletUnit = platformComponent.marginAssets[orderPageComponent.symbolDropdownComponent.selectedSymbol.ToUpper()];
            float maxLossPercentage = orderPageComponent.maxLossPercentageInput.text.IsNullOrEmpty() ? float.NaN :
                float.Parse(orderPageComponent.maxLossPercentageInput.text);
            float amountToLoss = orderPageComponent.maxLossAmountInput.text.IsNullOrEmpty() ? float.NaN :
                float.Parse(orderPageComponent.maxLossAmountInput.text);
            int entryTimes = orderPageComponent.entryTimesInput.text.IsNullOrEmpty() ? 0 :
                int.Parse(orderPageComponent.entryTimesInput.text);
            List<float> entryPrices = new List<float>();
            orderPageComponent.inputEntryPricesComponent.entryPriceInputs.ForEach(input =>
            {
                if (input.text != "") entryPrices.Add(float.Parse(input.text));
            });
            float stopLossPrice = orderPageComponent.stopLossInput.text.IsNullOrEmpty() ? float.NaN :
                float.Parse(orderPageComponent.stopLossInput.text);
            float takeProfitPrice = orderPageComponent.takeProfitInput.text.IsNullOrEmpty() ? float.NaN :
                float.Parse(orderPageComponent.takeProfitInput.text);
            float riskRewardRatio = orderPageComponent.riskRewardRatioInput.text.IsNullOrEmpty() ? profileComponent.activeProfile.preference.order.riskRewardRatio : float.Parse(orderPageComponent.riskRewardRatioInput.text);

            // validate input
            if (walletUnit.IsNullOrEmpty())
            {
                ShowPrompt("Wallet unit not available.");
                yield break;
            }
            if (maxLossPercentage.Equals(float.NaN) && amountToLoss.Equals(float.NaN))
            {
                ShowPrompt("Either one of the loss percentage or loss amount must have value.");
                yield break;
            }
            if (entryPrices.Count <= 0)
            {
                ShowPrompt("Entry price(s) cannot be empty.");
                yield break;
            }
            if (stopLossPrice.Equals(float.NaN))
            {
                ShowPrompt("Stop loss price cannot be empty.");
                yield break;
            }
            if (!((entryPrices[0] > stopLossPrice && entryPrices[^1] > stopLossPrice) || (entryPrices[0] < stopLossPrice && entryPrices[^1] < stopLossPrice)))
            {
                ShowPrompt("Entry price(s) and stop loss price not valid.");
                yield break;
            }
            if (!takeProfitPrice.Equals(float.NaN))
            {
                if (!((entryPrices[0] > stopLossPrice && entryPrices[^1] > stopLossPrice && entryPrices[0] < takeProfitPrice && entryPrices[^1] < takeProfitPrice)
                  || (entryPrices[0] < stopLossPrice && entryPrices[^1] < stopLossPrice && entryPrices[0] > takeProfitPrice && entryPrices[^1] > takeProfitPrice)))
                {
                    ShowPrompt("Entry price(s) and take profit price not valid.");
                    yield break;
                }
            }

            platformComponent.walletBalances = new();
            getBalanceComponent.getBalance = true;
            yield return new WaitUntil(() => platformComponent.walletBalances.ContainsKey(walletUnit));
            float currentWalletBalance = platformComponent.walletBalances[walletUnit];

            yield return new WaitUntil(() => platformComponent.fees.ContainsKey(orderPageComponent.symbolDropdownComponent.selectedSymbol)
            && platformComponent.fees[orderPageComponent.symbolDropdownComponent.selectedSymbol].HasValue);
            float feeRate = platformComponent.fees[orderPageComponent.symbolDropdownComponent.selectedSymbol].Value;
            #endregion

            #region Create calculator instance
            orderPageComponent.marginCalculator = new CalculateMargin(
                currentWalletBalance,
                maxLossPercentage,
                amountToLoss,
                entryTimes,
                entryPrices,
                stopLossPrice,
                (TakeProfitTypeEnum)orderPageComponent.takeProfitTypeDropdown.value,
                riskRewardRatio,
                (int)orderPageComponent.takeProfitQuantityPercentageCustomSlider.slider.value,
                orderPageComponent.takeProfitTrailingCallbackPercentageCustomSlider.slider.value,
                feeRate,
                platformComponent.quantityPrecisions[orderPageComponent.symbolDropdownComponent.selectedSymbol],
                platformComponent.pricePrecisions[orderPageComponent.symbolDropdownComponent.selectedSymbol],
                orderPageComponent.marginDistributionModeDropdown.value == 1,
                orderPageComponent.marginWeightDistributionValueCustomSlider.slider.value
            );
            #endregion
        }

        #region Prepare game object for display
        orderPageComponent.resultComponent.pricesDataObjects.ForEach(obj => Destroy(obj));
        orderPageComponent.resultComponent.pricesDataObjects.Clear();
        orderPageComponent.resultComponent.quantitiesDataObjects.ForEach(obj => Destroy(obj));
        orderPageComponent.resultComponent.quantitiesDataObjects.Clear();
        if (orderPageComponent.resultComponent.orderInfoDataObject == null)
        {
            orderPageComponent.resultComponent.orderInfoDataObject = Instantiate(orderPageComponent.dataRowPrefab, orderPageComponent.resultComponent.orderInfoParent);
        }
        if (orderPageComponent.resultComponent.totalWinLossAmountDataObject == null)
        {
            orderPageComponent.resultComponent.totalWinLossAmountDataObject = Instantiate(orderPageComponent.dataRowPrefab, orderPageComponent.resultComponent.totalWinLossAmountParent);
        }
        if (orderPageComponent.resultComponent.balanceDataObject == null)
        {
            orderPageComponent.resultComponent.balanceDataObject = Instantiate(orderPageComponent.dataRowPrefab, orderPageComponent.resultComponent.balanceParent);
        }
        #endregion

        #region Assign value into game object for display
        TMP_Text temp;
        #region Order title
        string direction = orderPageComponent.marginCalculator.isLong ? "LONG" : "SHORT";
        Color directionColor = orderPageComponent.marginCalculator.isLong ? OrderConfig.DISPLAY_COLOR_GREEN : OrderConfig.DISPLAY_COLOR_RED;
        orderPageComponent.orderTitleText.text = orderPageComponent.symbolDropdownComponent.selectedSymbol + ": " + direction;
        orderPageComponent.orderTitleText.color = directionColor;
        #endregion
        #region Order info
        orderPageComponent.resultComponent.orderInfoDataObject.transform.GetChild(0).GetComponent<TMP_Text>().text = orderPageComponent.symbolDropdownComponent.selectedSymbol;
        orderPageComponent.resultComponent.orderInfoDataObject.transform.GetChild(1).gameObject.SetActive(false);
        temp = orderPageComponent.resultComponent.orderInfoDataObject.transform.GetChild(2).GetComponent<TMP_Text>();
        temp.text = direction;
        temp.color = directionColor;
        orderPageComponent.resultComponent.orderInfoDataObject.transform.GetChild(3).gameObject.SetActive(false);
        orderPageComponent.resultComponent.orderInfoDataObject.transform.GetChild(4).GetComponent<TMP_Text>().text = orderPageComponent.orderStatus.ToString();
        orderPageComponent.resultComponent.orderInfoDataObject.transform.GetChild(5).gameObject.SetActive(false);
        #endregion
        #region Prices & Quantities
        List<float> tpPrices = (TakeProfitTypeEnum)orderPageComponent.takeProfitTypeDropdown.value == TakeProfitTypeEnum.TRAILING ?
        orderPageComponent.marginCalculator.takeProfitTrailingPrices :
        orderPageComponent.marginCalculator.takeProfitPrices;
        List<float> tpPricePercentages = (TakeProfitTypeEnum)orderPageComponent.takeProfitTypeDropdown.value == TakeProfitTypeEnum.TRAILING ?
        orderPageComponent.marginCalculator.takeProfitTrailingPricePercentages :
        orderPageComponent.marginCalculator.takeProfitPricePercentages;
        for (int i = 0; i < orderPageComponent.marginCalculator.entryPrices.Count; i++)
        {
            #region Prices
            GameObject entryPriceDataObject = Instantiate(orderPageComponent.dataRowPrefab, orderPageComponent.resultComponent.pricesParent);
            entryPriceDataObject.transform.GetChild(0).GetComponent<TMP_Text>().text = orderPageComponent.marginCalculator.entryPrices[i].ToString();
            entryPriceDataObject.transform.GetChild(1).gameObject.SetActive(false);
            entryPriceDataObject.transform.GetChild(2).GetComponent<TMP_Text>().text = orderPageComponent.marginCalculator.avgEntryPrices[i].ToString();
            entryPriceDataObject.transform.GetChild(3).gameObject.SetActive(false);
            temp = entryPriceDataObject.transform.GetChild(4).GetComponent<TMP_Text>();
            temp.text = tpPrices[i].ToString();
            temp.color = OrderConfig.DISPLAY_COLOR_GREEN;
            temp = entryPriceDataObject.transform.GetChild(5).GetComponent<TMP_Text>();
            temp.text = Utils.RoundTwoDecimal(tpPricePercentages[i]).ToString() + "%";
            temp.color = OrderConfig.DISPLAY_COLOR_GREEN;
            orderPageComponent.resultComponent.pricesDataObjects.Add(entryPriceDataObject);
            #endregion
            #region Quantities
            GameObject listedWinLossAmountDataObject = Instantiate(orderPageComponent.dataRowPrefab,
                orderPageComponent.resultComponent.quantitiesParent);
            listedWinLossAmountDataObject.transform.GetChild(0).GetComponent<TMP_Text>().text = orderPageComponent.marginCalculator.quantities[i].ToString();
            listedWinLossAmountDataObject.transform.GetChild(1).gameObject.SetActive(false);
            listedWinLossAmountDataObject.transform.GetChild(2).GetComponent<TMP_Text>().text = orderPageComponent.marginCalculator.cumQuantities[i].ToString();
            listedWinLossAmountDataObject.transform.GetChild(3).gameObject.SetActive(false);
            temp = listedWinLossAmountDataObject.transform.GetChild(4).GetComponent<TMP_Text>();
            temp.text = Utils.RoundTwoDecimal(orderPageComponent.marginCalculator.stopLossAmounts[i]).ToString();
            temp.color = OrderConfig.DISPLAY_COLOR_RED;
            temp = listedWinLossAmountDataObject.transform.GetChild(5).GetComponent<TMP_Text>();
            temp.text = "Fee: " + Utils.RoundNDecimal(orderPageComponent.marginCalculator.fees[i], 6).ToString();
            temp.color = OrderConfig.DISPLAY_COLOR_RED;
            orderPageComponent.resultComponent.quantitiesDataObjects.Add(listedWinLossAmountDataObject);
            #endregion
        }
        #endregion
        #region Total win loss amount
        temp = orderPageComponent.resultComponent.totalWinLossAmountDataObject.transform.GetChild(0).GetComponent<TMP_Text>();
        temp.text = orderPageComponent.marginCalculator.stopLossPrice.ToString();
        temp.color = OrderConfig.DISPLAY_COLOR_RED;
        temp = orderPageComponent.resultComponent.totalWinLossAmountDataObject.transform.GetChild(1).GetComponent<TMP_Text>();
        temp.text = Utils.RoundTwoDecimal(orderPageComponent.marginCalculator.stopLossPercentage).ToString() + "%";
        temp.color = OrderConfig.DISPLAY_COLOR_RED;
        temp = orderPageComponent.resultComponent.totalWinLossAmountDataObject.transform.GetChild(2).GetComponent<TMP_Text>();
        temp.text = Utils.RoundTwoDecimal(orderPageComponent.marginCalculator.totalLossAmount).ToString();
        temp.color = OrderConfig.DISPLAY_COLOR_RED;
        temp = orderPageComponent.resultComponent.totalWinLossAmountDataObject.transform.GetChild(3).GetComponent<TMP_Text>();
        temp.text = "Fee: " + Utils.RoundNDecimal(orderPageComponent.marginCalculator.totalFee, 6).ToString();
        temp.color = OrderConfig.DISPLAY_COLOR_RED;
        temp = orderPageComponent.resultComponent.totalWinLossAmountDataObject.transform.GetChild(4).GetComponent<TMP_Text>();
        temp.text = Utils.RoundTwoDecimal(orderPageComponent.marginCalculator.totalWinAmount).ToString();
        temp.color = OrderConfig.DISPLAY_COLOR_GREEN;
        temp = orderPageComponent.resultComponent.totalWinLossAmountDataObject.transform.GetChild(5).GetComponent<TMP_Text>();
        temp.text = "Diff: " + Utils.RoundTwoDecimal(orderPageComponent.marginCalculator.winLossDiff).ToString();
        temp.color = orderPageComponent.marginCalculator.winLossDiff > 0 ? OrderConfig.DISPLAY_COLOR_GREEN : OrderConfig.DISPLAY_COLOR_RED;
        #endregion
        #region Balance
        orderPageComponent.resultComponent.balanceDataObject.transform.GetChild(0).GetComponent<TMP_Text>().text = Utils.TruncTwoDecimal(orderPageComponent.marginCalculator.balance).ToString();
        orderPageComponent.resultComponent.balanceDataObject.transform.GetChild(1).gameObject.SetActive(false);
        temp = orderPageComponent.resultComponent.balanceDataObject.transform.GetChild(2).GetComponent<TMP_Text>();
        temp.text = Utils.TruncTwoDecimal(orderPageComponent.marginCalculator.balanceAfterLoss).ToString();
        temp.color = OrderConfig.DISPLAY_COLOR_RED;
        temp = orderPageComponent.resultComponent.balanceDataObject.transform.GetChild(3).GetComponent<TMP_Text>();
        temp.text = "-" + Utils.RoundTwoDecimal(Utils.RateToPercentage(orderPageComponent.marginCalculator.balanceDecrementRate)).ToString() + " %";
        temp.color = OrderConfig.DISPLAY_COLOR_RED;
        temp = orderPageComponent.resultComponent.balanceDataObject.transform.GetChild(4).GetComponent<TMP_Text>();
        temp.text = Utils.TruncTwoDecimal(orderPageComponent.marginCalculator.balanceAfterFullWin).ToString();
        temp.color = OrderConfig.DISPLAY_COLOR_GREEN;
        temp = orderPageComponent.resultComponent.balanceDataObject.transform.GetChild(5).GetComponent<TMP_Text>();
        temp.text = "+" + Utils.RoundTwoDecimal(Utils.RateToPercentage(orderPageComponent.marginCalculator.balanceIncrementRate)).ToString() + " %";
        temp.color = OrderConfig.DISPLAY_COLOR_GREEN;
        #endregion
        #endregion

        #region Update button after finish calculated
        if (orderPageComponent.orderStatus == OrderStatusEnum.UNSUBMITTED)
        {
            orderPageComponent.calculateButton.interactable = true;
        }
        #endregion

        #region Save order when calculate button is pressed
        if (calculateButtonPressed)
        {
            calculateButtonPressed = false;
            orderPageComponent.addToServer = true;
        }
        #endregion
    }
    void ShowPrompt(string message, bool goToEditMode = true)
    {
        promptComponent.ShowPrompt(PromptConstant.ERROR, message, () =>
        {
            promptComponent.active = false;
            if (goToEditMode)
            {
                orderPageComponent.lockForEdit = false;
                orderPageComponent.calculateButton.interactable = true;
            }
        });
    }
    void UpdateUiInteractableStatus()
    {
        UpdateUiInteractableStatusBasedOnLockForEdit();
        UpdateUiInteractableStatusBasedOnOrderStatus();
    }
    void UpdateUiInteractableStatusBasedOnLockForEdit()
    {
        if (lockForEdit == orderPageComponent.lockForEdit) return;
        lockForEdit = orderPageComponent.lockForEdit;
        orderPageComponent.symbolDropdownComponent.dropdown.interactable = !lockForEdit.Value;
        orderPageComponent.maxLossPercentageInput.interactable = !lockForEdit.Value;
        orderPageComponent.inputEntryPricesComponent.entryPriceInputs.ForEach(input => input.interactable = !lockForEdit.Value);
        orderPageComponent.inputEntryPricesComponent.entryPriceCloseButtons.ForEach(button => button.interactable = !lockForEdit.Value);
        orderPageComponent.inputEntryPricesComponent.addButtons.interactable = !lockForEdit.Value;
        orderPageComponent.maxLossAmountInput.interactable = !lockForEdit.Value;
        orderPageComponent.entryTimesInput.interactable = !lockForEdit.Value;
        orderPageComponent.stopLossInput.interactable = !lockForEdit.Value;
        orderPageComponent.takeProfitInput.interactable = !lockForEdit.Value;
        orderPageComponent.marginDistributionModeDropdown.interactable = !lockForEdit.Value;
        orderPageComponent.marginWeightDistributionValueCustomSlider.slider.interactable = !lockForEdit.Value;
        orderPageComponent.marginWeightDistributionValueCustomSlider.input.interactable = !lockForEdit.Value;
        if (lockForEdit.Value) orderPageComponent.calculateButtonText.text = "Edit Order";
        else orderPageComponent.calculateButtonText.text = "Calculate";
        if (orderPageComponent.orderStatus == OrderStatusEnum.UNSUBMITTED) orderPageComponent.orderTypeDropdown.interactable = lockForEdit.Value;
        orderPageComponent.placeOrderButton.interactable = lockForEdit.Value;
        orderPageComponent.cancelOrderButton.interactable = lockForEdit.Value;
        orderPageComponent.closePositionButton.interactable = lockForEdit.Value;
        orderPageComponent.cancelErrorOrderButton.interactable = lockForEdit.Value;
        orderPageComponent.resultComponent.gameObject.SetActive(lockForEdit.Value);
        orderPageComponent.takeProfitTypeObject.SetActive(lockForEdit.Value);
        orderPageComponent.riskRewardRatioObject.SetActive(orderPageComponent.lockForEdit && orderPageComponent.takeProfitTypeDropdown.value > (int)TakeProfitTypeEnum.NONE);
        orderPageComponent.takeProfitQuantityPercentageCustomSlider.gameObject.SetActive(orderPageComponent.lockForEdit && orderPageComponent.takeProfitTypeDropdown.value > (int)TakeProfitTypeEnum.NONE);
        orderPageComponent.takeProfitTrailingCallbackPercentageCustomSlider.gameObject.SetActive(orderPageComponent.lockForEdit && orderPageComponent.takeProfitTypeDropdown.value == (int)TakeProfitTypeEnum.TRAILING);
        orderPageComponent.orderTypeObject.SetActive(lockForEdit.Value);
        orderPageComponent.applyButtonObject.SetActive(lockForEdit.Value);
    }
    void UpdateUiInteractableStatusBasedOnOrderStatus()
    {
        if (orderStatus == orderPageComponent.orderStatus) return;
        orderStatus = orderPageComponent.orderStatus;
        bool isFilled = orderStatus.Equals(OrderStatusEnum.FILLED);
        orderPageComponent.positionInfoObject.SetActive(isFilled);
        orderPageComponent.throttleObject.SetActive(isFilled);
        if (orderStatus != OrderStatusEnum.FILLED) orderPageComponent.positionInfoPaidFundingAmount.text = "0";
    }
    void AddToServer()
    {
        if (orderPageComponent.marginCalculator == null) return;
        websocketComponent.generalRequests.Add(
            new General.WebsocketAddOrderRequest(
            loginComponent.token,
            orderPageComponent.orderId,
            orderPageComponent.symbolDropdownComponent.selectedSymbol,
            orderPageComponent.marginCalculator,
            (OrderTypeEnum)orderPageComponent.orderTypeDropdown.value
        ));
    }
    public void UpdateToServer()
    {
        websocketComponent.generalRequests.Add(
            new General.WebsocketUpdateOrderRequest(
            loginComponent.token,
            orderPageComponent.orderId,
            orderPageComponent.marginCalculator,
            (OrderTypeEnum)orderPageComponent.orderTypeDropdown.value,
            orderPageComponent.tradingBotId
        ));
    }
    void DeleteFromServer()
    {
        foreach (OrderPageThrottleComponent orderPageThrottleComponent in orderPageComponent.throttleParentComponent.orderPageThrottleComponents)
        {
            Destroy(orderPageThrottleComponent.gameObject);
        }

        websocketComponent.generalRequests.Add(
            new General.WebsocketDeleteOrderRequest(
            loginComponent.token,
            orderPageComponent.orderId
        ));
    }
    void SubmitToServer()
    {
        websocketComponent.generalRequests.Add(
            new General.WebsocketSubmitOrderRequest(
            loginComponent.token,
            orderPageComponent.orderId,
            orderPageComponent.quantityToClose
        ));
    }
    public void UpdateTakeProfitPrice()
    {
        #region Calculate and get latest take profit prices
        if (orderPageComponent.takeProfitTypeDropdown.value > (int)TakeProfitTypeEnum.NONE)
        {
            orderPageComponent.marginCalculator.RecalculateTakeProfitPrices(
                (TakeProfitTypeEnum)orderPageComponent.takeProfitTypeDropdown.value,
                float.Parse(orderPageComponent.riskRewardRatioInput.text),
                (int)orderPageComponent.takeProfitQuantityPercentageCustomSlider.slider.value,
                orderPageComponent.takeProfitTrailingCallbackPercentageCustomSlider.slider.value);
        }
        List<float> tpPrices = (TakeProfitTypeEnum)orderPageComponent.takeProfitTypeDropdown.value == TakeProfitTypeEnum.TRAILING ?
        orderPageComponent.marginCalculator.takeProfitTrailingPrices :
        orderPageComponent.marginCalculator.takeProfitPrices;
        List<float> tpPricePercentages = (TakeProfitTypeEnum)orderPageComponent.takeProfitTypeDropdown.value == TakeProfitTypeEnum.TRAILING ?
        orderPageComponent.marginCalculator.takeProfitTrailingPricePercentages :
        orderPageComponent.marginCalculator.takeProfitPricePercentages;
        #endregion

        #region Update value in game object for display
        TMP_Text temp;
        for (int i = 0; i < orderPageComponent.marginCalculator.entryPrices.Count; i++)
        {
            Transform entryPriceDataTransform = orderPageComponent.resultComponent.pricesParent.GetChild(i + 1);
            temp = entryPriceDataTransform.GetChild(4).GetComponent<TMP_Text>();
            temp.text = tpPrices[i].ToString();
            temp = entryPriceDataTransform.GetChild(5).GetComponent<TMP_Text>();
            temp.text = Utils.RoundTwoDecimal(tpPricePercentages[i]).ToString() + "%";
        }
        orderPageComponent.resultComponent.totalWinLossAmountDataObject.transform.GetChild(4).GetComponent<TMP_Text>().text = Utils.RoundTwoDecimal(orderPageComponent.marginCalculator.totalWinAmount).ToString();
        temp = orderPageComponent.resultComponent.totalWinLossAmountDataObject.transform.GetChild(5).GetComponent<TMP_Text>();
        temp.text = "Diff: " + Utils.RoundTwoDecimal(orderPageComponent.marginCalculator.winLossDiff).ToString();
        temp.color = orderPageComponent.marginCalculator.winLossDiff > 0 ? OrderConfig.DISPLAY_COLOR_GREEN : OrderConfig.DISPLAY_COLOR_RED;
        orderPageComponent.resultComponent.balanceDataObject.transform.GetChild(4).GetComponent<TMP_Text>().text = Utils.TruncTwoDecimal(orderPageComponent.marginCalculator.balanceAfterFullWin).ToString();
        orderPageComponent.resultComponent.balanceDataObject.transform.GetChild(5).GetComponent<TMP_Text>().text = "+" + Utils.RoundTwoDecimal(Utils.RateToPercentage(orderPageComponent.marginCalculator.balanceIncrementRate)).ToString() + " %";
        #endregion

        orderPageComponent.updateToServer = true;
    }
}
