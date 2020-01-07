using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpriteAnimation = Character.SpriteAnimation;

public class Effect : MonoBehaviour
{
    public Texture2D texture;
    SpriteAnimation animation;
    SpriteRenderer renderer;

    void Awake()
    {
        animation = new SpriteAnimation(texture);
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = null;
    }

    public IEnumerator Run(bool clear = false)
    {
        yield return Animate(animation, 30);
        if (clear) renderer.sprite = null;
    }

    // Copy from Character    
    IEnumerator Animate(SpriteAnimation animation, float fps, bool inverse = false) {
        for (int i = 0; i < animation.frame.Length; i++) {
            if (!inverse)
                renderer.sprite = animation.frame[i];
            else
                renderer.sprite = animation.frame[animation.frame.Length-i-1];
            
            if (i < animation.frame.Length-1)
                yield return new WaitForSeconds(1.0f / fps);
        }
    }

}
