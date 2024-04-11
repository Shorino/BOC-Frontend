using System;
using System.Collections.Generic;

[Serializable]
public class Depth
{
    public List<BarData> asks;
    public List<BarData> bids;
    public Depth() { }
    public Depth(List<BarData> asks, List<BarData> bids)
    {
        this.asks = asks;
        this.bids = bids;
    }
}
[Serializable]
public class BarData
{
    public double price;
    public double quantity;
    public BarData() { }
    public BarData(List<string> array)
    {
        price = double.Parse(array[0]);
        quantity = double.Parse(array[1]);
    }
}
[Serializable]
public class DepthResult
{
    public double totalQuantityOnAskSide;
    public double totalQuantityOnBidSide;
    public double totalAmountOnAskSide;
    public double totalAmountOnBidSide;
    public double impactAskPrice;
    public double impactBidPrice;
    public double impactAskPricePercentage;
    public double impactBidPricePercentage;
    public double impactAskRatio;
    public double impactBidRatio;

    public DepthResult(double totalQuantityOnAskSide, double totalQuantityOnBidSide, double totalAmountOnAskSide, double totalAmountOnBidSide,
    double impactAskPrice, double impactBidPrice, double impactAskPricePercentage, double impactBidPricePercentage, double impactAskRatio, double impactBidRatio)
    {
        this.totalQuantityOnAskSide = totalQuantityOnAskSide;
        this.totalQuantityOnBidSide = totalQuantityOnBidSide;
        this.totalAmountOnAskSide = totalAmountOnAskSide;
        this.totalAmountOnBidSide = totalAmountOnBidSide;
        this.impactAskPrice = impactAskPrice;
        this.impactBidPrice = impactBidPrice;
        this.impactAskPricePercentage = impactAskPricePercentage;
        this.impactBidPricePercentage = impactBidPricePercentage;
        this.impactAskRatio = impactAskRatio;
        this.impactBidRatio = impactBidRatio;
    }
}