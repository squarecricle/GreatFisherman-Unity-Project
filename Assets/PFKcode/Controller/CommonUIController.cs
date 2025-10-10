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
        // TODO: 阶段二实现
        // gameFlowManager.GoToFishTank();
        Debug.Log("【CommonUIController】“鱼箱”按钮被点击。");
    }
    
    // 其他通用按钮...
    // public void OnBackpackButtonClicked() { ... }
    // public void OnQuestButtonClicked() { ... }

    public void ShowHomeButton()
    {
        // TODO: 阶段二实现
        // homeButton.SetActive(true);
        // fishingButton.SetActive(false);
    }

    public void ShowFishingButton()
    {
        // TODO: 阶段二实现
        // homeButton.SetActive(false);
        // fishingButton.SetActive(true);
    }
}