using UnityEngine;

public class CommonUIController : MonoBehaviour
{
    // 需要关联场景中的GameFlowManager
    // public GameFlowManager gameFlowManager;
    
    // 需要关联导航栏上的按钮
    // public GameObject homeButton;
    // public GameObject fishingButton;

    public void OnFishTankButtonClicked()
    {
        GameManager.Instance?.GoToFishTank();
        Debug.Log("【CommonUIController】“鱼箱”按钮被点击，已通知 GameManager。");
    }
    
    // 其他通用按钮...
    // public void OnBackpackButtonClicked() { ... }
    // public void OnQuestButtonClicked() { ... }

    public void ShowHomeButton()
    {
        GameManager.Instance?.GoToHome();
    }

    public void ShowFishingButton()
    {
        Debug.LogWarning("【CommonUIController】ShowFishingButton 已废弃，请改用具体导航方法。");
    }
}