using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PyramidButton : MonoBehaviour
{
    public int level;

    void OnMouseUpAsButton() {
        GameGlobal.Play(level);
    }
}
