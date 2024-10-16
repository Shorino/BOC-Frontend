using UnityEngine;
using UnityEngine.InputSystem;

public class ExitSystem : MonoBehaviour
{
    ExitComponent exitComponent;
    PromptComponent promptComponent;
    InputComponent inputComponent;

    void Start()
    {
        exitComponent = GlobalComponent.instance.exitComponent;
        promptComponent = GlobalComponent.instance.promptComponent;
        inputComponent = GlobalComponent.instance.inputComponent;

        exitComponent.onChange_exit.AddListener(QuitApplication);

        inputComponent.back.performed += ShowExitPrompt;
    }
    void ShowExitPrompt(InputAction.CallbackContext context)
    {
        if (promptComponent.active)
        {
            promptComponent.active = false;
        }
        else
        {
            promptComponent.ShowSelection(PromptConstant.EXIT, PromptConstant.EXIT_PROMPT, PromptConstant.YES, PromptConstant.NO,
            () =>
            {
                exitComponent.exit = true;
            },
            () =>
            {
                promptComponent.active = false;
            });
        }
    }
    void QuitApplication()
    {
        inputComponent.back.performed -= ShowExitPrompt;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        UnityEngine.Application.Quit();
#endif
    }
}
