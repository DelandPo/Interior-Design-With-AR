using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sliderScript : MonoBehaviour {

    public Animator ContentPanel;
    public Animator gearImage;

    public void ToggleMenu()
    {
        bool isHidden = ContentPanel.GetBool("isHidden");
        ContentPanel.SetBool("isHidden", !isHidden);
        gearImage.SetBool("isHidden", !isHidden);
    }
	
}
