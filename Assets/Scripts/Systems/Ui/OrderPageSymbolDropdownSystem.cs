using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrderPageSymbolDropdownSystem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] OrderPageSymbolDropdownComponent orderPageSymbolDropdownComponent;
    
    PlatformComponent platformComponent;
    InputComponent inputComponent;
    TMP_Dropdown dropdown;

    bool hover;

    void Start()
    {
        platformComponent = GlobalComponent.instance.platformComponent;
        inputComponent = GlobalComponent.instance.inputComponent;
        dropdown = GetComponent<TMP_Dropdown>();

        inputComponent.click.performed += _ =>
        {
            if (hover)
            {
                orderPageSymbolDropdownComponent.symbols = platformComponent.allSymbols;
                UpdateOptions();
            }
        };
        dropdown.onValueChanged.AddListener((value) =>
        {
            orderPageSymbolDropdownComponent.selectedSymbol = orderPageSymbolDropdownComponent.symbols[value];
        });
    }
    void Update()
    {
        SyncAllSymbols();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        hover = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hover = false;
    }

    public void OnSearchChanged(string value)
    {
        orderPageSymbolDropdownComponent.symbols = platformComponent.allSymbols.FindAll(symbol => symbol.Contains(value.ToUpper()));
        UpdateOptions();
    }
    void UpdateOptions()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(orderPageSymbolDropdownComponent.symbols);
        dropdown.value = orderPageSymbolDropdownComponent.symbols.IndexOf(orderPageSymbolDropdownComponent.selectedSymbol);
    }
    void SyncAllSymbols()
    {
        if (orderPageSymbolDropdownComponent.allSymbolsCount == platformComponent.allSymbols.Count) return;
        orderPageSymbolDropdownComponent.allSymbolsCount = platformComponent.allSymbols.Count;

        orderPageSymbolDropdownComponent.symbols = platformComponent.allSymbols;
        UpdateOptions();
    }
}