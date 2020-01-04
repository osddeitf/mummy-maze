using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{

    // Inspector
    public Texture2D up;
    public Texture2D down;
    public Texture2D left;
    public Texture2D right;
    public Texture2D[] idle;

    public class SpriteAnimation {
        public Sprite[] frame;
        public SpriteAnimation(Texture2D texture) {
            int size = texture.height;
            int len = texture.width / size;

            frame = new Sprite[len];
            for (int i = 0; i < len; i++) {
                frame[i] = Sprite.Create(texture, new Rect(i * size, 0, size, size), new Vector2(0, 0), 60);
            }
        }
    }


    SpriteRenderer renderer;
    SpriteAnimation animation_up;
    SpriteAnimation animation_down;
    SpriteAnimation animation_left;
    SpriteAnimation animation_right;
    SpriteAnimation[] animation_idle;
    Coroutine idleAnimation;

    // Start is called before the first frame update
    void Start()
    {
        animation_up = new SpriteAnimation(up);
        animation_down = new SpriteAnimation(down);
        animation_left = new SpriteAnimation(left);
        animation_right = new SpriteAnimation(right);
        animation_idle = new SpriteAnimation[idle.Length];
        
        for (int i = 0; i < idle.Length; i++)
            animation_idle[i] = new SpriteAnimation(idle[i]);

        renderer = GetComponent<SpriteRenderer>();
        
        idleAnimation = StartCoroutine(Idle());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Move(Vector3 direction, bool blocked, float duration = 1.0f / 3) {
        SpriteAnimation animation = AnimationTowards(direction);
        if (animation == null) yield break;

        StopCoroutine(idleAnimation);

        if (blocked) {
            renderer.sprite = animation.frame[0];
        }
        else {
            StartCoroutine(Animate(animation, animation.frame.Length / duration));

            // Translate
            int frame = 30;
            Vector3 momentum = direction / frame;
            Vector3 target = direction + transform.localPosition;

            for (int i = 0; i < frame; i++) {
                transform.Translate(momentum);
                yield return new WaitForSeconds(duration / frame);
            }

            transform.localPosition = target;
        }

        idleAnimation = StartCoroutine(Idle());
    }

    SpriteAnimation AnimationTowards(Vector3 direction) {
        if (direction == Vector3.up) return animation_up;
        if (direction == Vector3.down) return animation_down;
        if (direction == Vector3.left) return animation_left;
        if (direction == Vector3.right) return animation_right;
        return null;
    }

    IEnumerator Animate(SpriteAnimation animation, float fps, bool inverse = false) {
        for (int i = 0; i < animation.frame.Length; i++) {
            if (!inverse)
                renderer.sprite = animation.frame[i];
            else
                renderer.sprite = animation.frame[animation.frame.Length-i-1];
            yield return new WaitForSeconds(1.0f / fps);
        }
        renderer.sprite = animation.frame[0];
    }

    IEnumerator Idle() {
        System.Random rand = new System.Random();
        yield return new WaitForSeconds(5.0f);
        while (true) {
            int index = rand.Next(idle.Length);
            yield return Animate(animation_idle[index], 20);
            yield return Animate(animation_idle[index], 20, true);
            yield return new WaitForSeconds(2.5f);
        }
    }
}
