using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginComponent : MonoBehaviour
{
    [Header("Reference")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public GameObject confirmPasswordObj;
    public Button proceedButton;
    public TMP_Text proceedButtonText;
    public Button switchButton;
    public TMP_Text switchButtonText;
    public GameObject switchButtonObj;

    [Header("Runtime")]
    [SerializeField] private LoginPageStatusEnum _loginStatus;
    public LoginPageStatusEnum loginStatus
    {
        set
        {
            _loginStatus = value;
            onChange_loginStatus.Invoke();
        }
        get { return _loginStatus; }
    }
    [HideInInspector] public UnityEvent onChange_loginStatus = new();
    public string _token;
    public string token
    {
        set
        {
            _token = value;
            onChange_token.Invoke();
        }
        get { return _token; }
    }
    [HideInInspector] public UnityEvent onChange_token = new();
    public bool logoutNow
    {
        set
        {
            onChange_logoutNow.Invoke();
        }
    }
    [HideInInspector] public UnityEvent onChange_logoutNow = new();

}