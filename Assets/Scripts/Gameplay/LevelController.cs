using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    class GameState {
        public int size;
        public bool idle;
        public Character player;
        public Character mummy;

        // Static
        public int[,] verticalWall;
        public int[,] horizontalWall;
        public Vector3 stairPosition;
        public Vector3 stairDirection = Vector3.left;
        public bool restrictedVision = false;

        public GameState(int size) {
            this.size = size;
            this.idle = true;
            verticalWall = new int[size, size];
            horizontalWall = new int[size, size];
        }
    }

    // Inspector
    public GameObject winOverlay;
    public GameObject loseOverlay;
    public GameObject dust_effect;
    public GameObject defeat_effect;
    
    // Internal
    Effect dustFx;
    Effect defeatFx;
    GameState state;

    // Start is called before the first frame update
    void Start()
    {
        int n = 6;
        state = new GameState(n);

        foreach (Transform t in transform) {
            int x = (int) t.localPosition.x;
            int y = (int) t.localPosition.y;

            switch (t.tag) {
                case "Player":
                    state.player = t.GetComponent<Character>();
                    break;
                case "Bot":
                    state.mummy = t.GetComponent<Character>();
                    break;
                case "Stair":
                    state.stairPosition = t.localPosition;
                    if (x == 0) state.stairDirection = Vector3.left;
                    if (y == 0) state.stairDirection = Vector3.down;
                    if (x == n) state.stairDirection = Vector3.right;
                    if (y == n) state.stairDirection = Vector3.up;
                    break;
                case "VerticalWall":
                    state.verticalWall[x, y] = 1;
                    break;
                case "HorizontalWall":
                    state.horizontalWall[x, y] = 1;
                    break;
                default:
                    Debug.Log("Unexpected game object with tag: " + t.tag);
                    break;
            }
        }

        dustFx = ((GameObject)Instantiate(dust_effect, transform)).GetComponent<Effect>();
        defeatFx = ((GameObject)Instantiate(defeat_effect, transform)).GetComponent<Effect>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!state.idle) return;

        Vector3 direction = Vector3.zero;

        if (Input.GetKeyDown("up")) direction = Vector3.up;
        else if (Input.GetKeyDown("down")) direction = Vector3.down;
        else if (Input.GetKeyDown("left")) direction = Vector3.left;
        else if (Input.GetKeyDown("right")) direction = Vector3.right;

        if (direction != Vector3.zero)
            StartCoroutine(Action(direction));
    }

    IEnumerator Action(Vector3 direction) {

        Character player = state.player;
        Character mummy = state.mummy;

        // Player move 1 step
        if (Blocked(player.transform.localPosition, direction)) yield break;

        state.idle = false;
        yield return player.Move(direction, false);
        
        // Mummy move 2 step
        for (int i = 0; i < 2; i++) {
            Vector3 next_move = Trace(mummy.transform.localPosition);
            bool isBlocked = Blocked(mummy.transform.localPosition, next_move);
            yield return mummy.Move(next_move, isBlocked);
        }

        // Lose
        if (player.transform.localPosition == mummy.transform.localPosition) {

            dustFx.transform.position = mummy.transform.position;
            defeatFx.transform.position = mummy.transform.position;
            
            Destroy(mummy.gameObject);
            Destroy(player.gameObject);

            StartCoroutine(defeatFx.Run(false));
            yield return StartCoroutine(dustFx.Run(true));

            Instantiate(loseOverlay, transform, true);
        }
        else if (player.transform.localPosition == state.stairPosition) {
            yield return player.Move(state.stairDirection, false);
            
            Destroy(mummy.gameObject);
            Destroy(player.gameObject);
            
            Instantiate(winOverlay, transform, true);
        }

        state.idle = true;
    }

    bool Blocked(Vector3 position, Vector3 direction) {
        int x = (int)position.x;
        int y = (int)position.y;
        
        if (direction == Vector3.up)
            return y + 1 == state.size || state.horizontalWall[x, y+1] == 1;
        
        if (direction == Vector3.right)
            return x + 1 == state.size || state.verticalWall[x+1, y] == 1;

        if (direction == Vector3.down)
            return y == 0 || state.horizontalWall[x, y] == 1;

        if (direction == Vector3.left)
            return x == 0 || state.verticalWall[x, y] == 1;
        
        return true;
    }

    // Mummy trace
    Vector3 Trace(Vector3 position) {
        int x = (int) state.player.transform.localPosition.x;
        int y = (int) state.player.transform.localPosition.y;
        int z = (int) state.player.transform.localPosition.z;
        int px = (int) position.x;
        int py = (int) position.y;
        int pz = (int) position.z;

        if (x > px) {
            if (!Blocked(position, Vector3.right)) return Vector3.right;
        }
        if (x < px) {
            if (!Blocked(position, Vector3.left)) return Vector3.left;
        }
        if (y > py) return Vector3.up;
        if (y < py) return Vector3.down;
        if (x > px) return Vector3.right;
        if (x < px) return Vector3.left;

        return Vector3.zero;
    }
}
