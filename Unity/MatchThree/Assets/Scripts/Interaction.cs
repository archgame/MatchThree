/// <summary>
/// The main Interaction script.
/// Tests jewel for neighbor and sets jewel as swappable jewel in main script
/// Script should be attached to the jewel prefab and is fired when the button is pressed.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour {

    //[HideInInspector]
    public int x;
    //[HideInInspector]
    public int y;

    //TODO: add jewel press overlay/underlay

    public void JewelSelect()
    {       
        MatchThree mainScript = Camera.main.GetComponent<MatchThree>();
        if (!mainScript.isAnimating)
        {           
            if (mainScript.selected1 == null)
            {
                mainScript.ClickSound();
                mainScript.selected1 = this.gameObject;
            }
            else if(!GameObject.ReferenceEquals(mainScript.selected1,this.gameObject))
            {
                
                Interaction other = mainScript.selected1.GetComponent<Interaction>();
                if (other.x == x - 1 && other.y == y || other.x == x + 1 && other.y == y
                    || other.x == x && other.y == y - 1 || other.x == x && other.y == y + 1
                    )
                {
                    mainScript.ClickSound();
                    mainScript.selected2 = this.gameObject;
                }
                else //if something else was selected, we reset the selection
                {
                    mainScript.selected1 = null;
                    mainScript.selected2 = null;
                }
            }
        }
    }
}
