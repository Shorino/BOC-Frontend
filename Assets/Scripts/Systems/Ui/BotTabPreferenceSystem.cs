using UnityEngine;
using WebSocketSharp;

public class BotTabPreferenceSystem : MonoBehaviour
{
    BotTabComponent botTabComponent;
    ProfileComponent profileComponent;
    SettingPageComponent settingPageComponent;

    void Start()
    {
        botTabComponent = GlobalComponent.instance.botTabComponent;
        profileComponent = GlobalComponent.instance.profileComponent;
        settingPageComponent = GlobalComponent.instance.settingPageComponent;

        botTabComponent.onChange_updatePreferenceUI.AddListener(UpdateUIFromProfile);

        DefineOnUIChangedListeners();
    }

    void UpdateUIFromProfile()
    {
        PreferenceBot preferenceBot = profileComponent.activeProfile.preference.bot;
        botTabComponent.updatingUIFromProfile = true;

        botTabComponent.longOrderLimitInput.text = preferenceBot.longOrderLimit.ToString();
        botTabComponent.shortOrderLimitInput.text = preferenceBot.shortOrderLimit.ToString();
        botTabComponent.autoDestroyOrderToggle.isOn = preferenceBot.autoDestroyOrder;
        botTabComponent.botTypeDropdown.value = (int)preferenceBot.botType;
        botTabComponent.premiumIndexSetting_longThresholdPercentage.text = preferenceBot.premiumIndex.longThresholdPercentage.ToString();
        botTabComponent.premiumIndexSetting_shortThresholdPercentage.text = preferenceBot.premiumIndex.shortThresholdPercentage.ToString();
        botTabComponent.premiumIndexSetting_averageCandleLength.text = preferenceBot.premiumIndex.averageCandleLength.ToString();
        botTabComponent.premiumIndexSetting_reverseCandleBuffer.text = preferenceBot.premiumIndex.reverseCandleBuffer.ToString();
        botTabComponent.premiumIndexSetting_reverseCandleConfirmation.text = preferenceBot.premiumIndex.reverseCandleConfirmation.ToString();
        botTabComponent.mcdxSetting_timeframeDropdown.value = (int)preferenceBot.mcdx.timeframe;
        botTabComponent.mcdxSetting_averageCandleLength.text = preferenceBot.mcdx.averageCandleLength.ToString();
        botTabComponent.mcdxSetting_fomoCandleConfirmation.text = preferenceBot.mcdx.fomoCandleConfirmation.ToString();

        botTabComponent.updatingUIFromProfile = false;
    }
    void DefineOnUIChangedListeners()
    {
        botTabComponent.longOrderLimitInput.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.longOrderLimit.ToString();
                botTabComponent.longOrderLimitInput.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.longOrderLimit.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.longOrderLimit = int.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.shortOrderLimitInput.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.shortOrderLimit.ToString();
                botTabComponent.shortOrderLimitInput.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.shortOrderLimit.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.shortOrderLimit = int.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.autoDestroyOrderToggle.onValueChanged.AddListener(value =>
        {
            if (profileComponent.activeProfile == null) return;
            profileComponent.activeProfile.preference.bot.autoDestroyOrder = value; // BUG: sometimes will null
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.botTypeDropdown.onValueChanged.AddListener(value =>
        {
            profileComponent.activeProfile.preference.bot.botType = (BotTypeEnum)value;
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.premiumIndexSetting_longThresholdPercentage.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.premiumIndex.longThresholdPercentage.ToString();
                botTabComponent.premiumIndexSetting_longThresholdPercentage.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.premiumIndex.longThresholdPercentage.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.premiumIndex.longThresholdPercentage = float.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.premiumIndexSetting_shortThresholdPercentage.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.premiumIndex.shortThresholdPercentage.ToString();
                botTabComponent.premiumIndexSetting_shortThresholdPercentage.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.premiumIndex.shortThresholdPercentage.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.premiumIndex.shortThresholdPercentage = float.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.premiumIndexSetting_averageCandleLength.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.premiumIndex.averageCandleLength.ToString();
                botTabComponent.premiumIndexSetting_averageCandleLength.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.premiumIndex.averageCandleLength.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.premiumIndex.averageCandleLength = int.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.premiumIndexSetting_reverseCandleBuffer.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.premiumIndex.reverseCandleBuffer.ToString();
                botTabComponent.premiumIndexSetting_reverseCandleBuffer.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.premiumIndex.reverseCandleBuffer.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.premiumIndex.reverseCandleBuffer = int.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.premiumIndexSetting_reverseCandleConfirmation.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.premiumIndex.reverseCandleConfirmation.ToString();
                botTabComponent.premiumIndexSetting_reverseCandleConfirmation.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.premiumIndex.reverseCandleConfirmation.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.premiumIndex.reverseCandleConfirmation = int.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.mcdxSetting_timeframeDropdown.onValueChanged.AddListener(value =>
        {
            profileComponent.activeProfile.preference.bot.mcdx.timeframe = (TimeframeEnum)value;
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.mcdxSetting_averageCandleLength.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.mcdx.averageCandleLength.ToString();
                botTabComponent.mcdxSetting_averageCandleLength.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.mcdx.averageCandleLength.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.mcdx.averageCandleLength = int.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
        botTabComponent.mcdxSetting_fomoCandleConfirmation.onEndEdit.AddListener(value =>
        {
            if (value.IsNullOrEmpty())
            {
                value = profileComponent.activeProfile.preference.bot.mcdx.fomoCandleConfirmation.ToString();
                botTabComponent.mcdxSetting_fomoCandleConfirmation.text = value;
            }
            if (profileComponent.activeProfile.preference.bot.mcdx.fomoCandleConfirmation.ToString() == value) return;
            profileComponent.activeProfile.preference.bot.mcdx.fomoCandleConfirmation = int.Parse(value);
            settingPageComponent.updatePreferenceToServer = true;
        });
    }
}