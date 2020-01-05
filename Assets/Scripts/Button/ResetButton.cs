using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour
{
    void OnMouseUpAsButton() {
        GameGlobal.Restart();
    }
}
