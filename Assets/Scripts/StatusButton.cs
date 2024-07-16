using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusButton : MonoBehaviour
{

    [SerializeField] private Image rewardButton;

    [SerializeField] private Button button;

    public void StatusSprite(bool status)
    {
        if(status = true)
        {
            rewardButton.sprite = button.spriteState.selectedSprite;
        }
        else
        {
            rewardButton.sprite = button.spriteState.disabledSprite;
        }
    }


}
