using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingPageInfoSystem : MonoBehaviour
{
    SettingPageComponent settingPageComponent;
    PlatformComponent platformComponent;
    OrderPagesComponent orderPagesComponent;

    Dictionary<string, TMP_Text> walletBalances = new();

    void Start()
    {
        settingPageComponent = GlobalComponent.instance.settingPageComponent;
        platformComponent = GlobalComponent.instance.platformComponent;
        orderPagesComponent = GlobalComponent.instance.orderPagesComponent;

        settingPageComponent.onChange_syncInfo.AddListener(() => UpdateInfoUI());
    }

    void UpdateInfoUI()
    {
        settingPageComponent.activePlatformText.text = platformComponent.activePlatform.ToString();
        settingPageComponent.totalOrdersText.text = orderPagesComponent.transform.childCount.ToString();
        foreach (KeyValuePair<string, double> walletBalance in platformComponent.walletBalances)
        {
            if (walletBalance.Value == 0)
            {
                if (walletBalances.ContainsKey(walletBalance.Key))
                {
                    walletBalances[walletBalance.Key].transform.parent.gameObject.SetActive(false);
                }
                continue;
            }
            if (!walletBalances.ContainsKey(walletBalance.Key))
            {
                GameObject balanceLabelObj = Instantiate(settingPageComponent.balanceLabelPrefab, settingPageComponent.InfoBodyPanel);
                balanceLabelObj.transform.GetChild(0).GetComponent<TMP_Text>().text = walletBalance.Key + " Balance: ";
                walletBalances.Add(walletBalance.Key, balanceLabelObj.transform.GetChild(1).GetComponent<TMP_Text>());
            }
            walletBalances[walletBalance.Key].transform.parent.gameObject.SetActive(true);
            walletBalances[walletBalance.Key].text = Utils.TruncTwoDecimal(walletBalance.Value).ToString();
        }
    }
}
