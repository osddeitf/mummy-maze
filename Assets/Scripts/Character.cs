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

    class Animated {
        public Sprite[] frame;
        public Animated(Texture2D texture) {
            int size = texture.height;
            int len = texture.width / size;

            frame = new Sprite[len];
            for (int i = 0; i < len; i++) {
                frame[i] = Sprite.Create(texture, new Rect(i * size, 0, size, size), new Vector2(0, 0), 1);
            }
        }
    }


    Animated animated_up;
    Animated animated_down;
    Animated animated_left;
    Animated animated_right;
    SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        animated_up = new Animated(up);
        animated_down = new Animated(down);
        animated_left = new Animated(left);
        animated_right = new Animated(right);
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = animated_down.frame[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Face(Vector3 direction) {
        if (direction == Vector3.up) renderer.sprite = animated_up.frame[0];
        if (direction == Vector3.down) renderer.sprite = animated_down.frame[0];
        if (direction == Vector3.left) renderer.sprite = animated_left.frame[0];
        if (direction == Vector3.right) renderer.sprite = animated_right.frame[0];
    }

    public IEnumerator Move(Vector3 direction) {

        Animated animate = null;
        if (direction == Vector3.up) animate = animated_up;
        if (direction == Vector3.down) animate = animated_down;
        if (direction == Vector3.left) animate = animated_left;
        if (direction == Vector3.right) animate = animated_right;
        if (animate == null) yield break;

        int n = animate.frame.Length;
        int frame = n * 5;
        float time = 0.3f;
        float fps = time / frame;

        Vector3 momentum = direction * (60.0f / frame);
        Vector3 target = direction * 60 + transform.localPosition;

        for (int i = 0; i < frame; i++) {
            renderer.sprite = animate.frame[i / 5];
            transform.Translate(momentum);
            yield return new WaitForSeconds(fps);
        }
        
        // Fix position
        renderer.sprite = animate.frame[0];
        transform.localPosition = target;
    }
}
