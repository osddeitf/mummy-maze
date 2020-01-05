using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public Sprite hoverTexture;
    public Sprite downTexture;
    SpriteRenderer render;

    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
        render.sprite = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseEnter() {
        if (render.sprite == null)
            render.sprite = hoverTexture;
    }

    void OnMouseExit() {
        if (render.sprite == hoverTexture)
            render.sprite = null;
    }

    void OnMouseDown() {
        render.sprite = downTexture;
    }

    void OnMouseUp() {
        render.sprite = null;
    }
}
