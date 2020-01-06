using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public Sprite texture;
    public Sprite hoverTexture;
    public Sprite downTexture;
    SpriteRenderer render;

    // Start is called before the first frame update
    void Start()
    {
        render = GetComponent<SpriteRenderer>();
        render.sprite = texture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseEnter() {
        if (render.sprite == texture)
            render.sprite = hoverTexture;
    }

    void OnMouseExit() {
        if (render.sprite == hoverTexture)
            render.sprite = texture;
    }

    void OnMouseDown() {
        if (downTexture != null)
            render.sprite = downTexture;
    }

    void OnMouseUp() {
        if (downTexture != null)
            render.sprite = texture;
    }
}
