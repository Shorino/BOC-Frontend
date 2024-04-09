using System;

[Serializable]
public class TradingBotSetting
{
    public BotTypeEnum botType;
    public int longOrderLimit;
    public int shortOrderLimit;
    public bool autoDestroyOrder;
    public StrategySetting strategySetting;

    public TradingBotSetting(BotTypeEnum botType, int longOrderLimit, int shortOrderLimit, bool autoDestroyOrder, StrategySetting strategySetting)
    {
        this.botType = botType;
        this.longOrderLimit = longOrderLimit;
        this.shortOrderLimit = shortOrderLimit;
        this.autoDestroyOrder = autoDestroyOrder;
        this.strategySetting = strategySetting;
    }
}
[Serializable]
public class StrategySetting { }
[Serializable]
public class PremiumIndexStrategySetting : StrategySetting
{
    public double longThresholdPercentage;
    public double shortThresholdPercentage;
    public int candleLength;

    public PremiumIndexStrategySetting(double longThresholdPercentage, double shortThresholdPercentage, int candleLength)
    {
        this.longThresholdPercentage = longThresholdPercentage;
        this.shortThresholdPercentage = shortThresholdPercentage;
        this.candleLength = candleLength;
    }
}