using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitButton : MonoBehaviour
{
    void OnMouseUpAsButton() {
        Application.Quit();
    }
}
