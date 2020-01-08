using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    class Checkpoint {
        public Vector3 player;
        public List<Vector3> mummies = new List<Vector3>();
    }


    // Inspector
    public GameObject winOverlay;
    public GameObject loseOverlay;
    public GameObject dust_effect;
    public GameObject defeat_effect;
    
    // Internal
    public bool idle;
    public Character player;
    public List<Character> mummies;
    public Stack checkpoints = new Stack();
    
    // Static
    public int size;
    int[,] verticalWall;
    int[,] horizontalWall;
    Vector3 stairPosition;
    Vector3 stairDirection;
    bool restrictedVision = false;


    void Awake() {
        size = 6;
        idle = true;
        mummies = new List<Character>();
        verticalWall = new int[size, size];
        horizontalWall = new int[size, size];
    }

    // Start is called before the first frame update
    void Start()
    {
        int n = size;
        foreach (Transform t in transform) {
            int x = (int) t.localPosition.x;
            int y = (int) t.localPosition.y;

            switch (t.tag) {
                case "Player":
                    player = t.GetComponent<Character>();
                    break;
                case "WhiteMummy":
                case "RedMummy":
                    mummies.Add(t.GetComponent<Character>());
                    break;
                case "Stair":
                    stairPosition = t.localPosition;
                    if (x == 0) stairDirection = Vector3.left;
                    if (y == 0) stairDirection = Vector3.down;
                    if (x == n) {
                        stairPosition.x--;
                        stairDirection = Vector3.right;
                    }
                    if (y == n) {
                        stairPosition.y--;
                        stairDirection = Vector3.up;
                    }
                    break;
                case "VerticalWall":
                    verticalWall[x, y] = 1;
                    break;
                case "HorizontalWall":
                    horizontalWall[x, y] = 1;
                    break;
                default:
                    Debug.Log("Unexpected game object with tag: " + t.tag);
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!idle) return;
        Vector3 direction = Vector3.zero;

        if (Input.GetKeyDown("up")) direction = Vector3.up;
        else if (Input.GetKeyDown("down")) direction = Vector3.down;
        else if (Input.GetKeyDown("left")) direction = Vector3.left;
        else if (Input.GetKeyDown("right")) direction = Vector3.right;

        if (direction != Vector3.zero)
            StartCoroutine(Action(direction));
    }

    IEnumerator Action(Vector3 direction) {

        // Player move 1 step
        if (Blocked(player.transform.localPosition, direction)) yield break;

        idle = false;
        yield return player.Move(direction, false);
        
        // Mummy move 2 step
        for (int step = 0; step < 2; step++) {
            yield return MummiesMove();

            if (MummiesCatch()) {
                yield return Lost();
                yield break;
            }

            yield return MummiesFight();
        }

        if (player.transform.localPosition == stairPosition) {
            yield return Victory();
            yield break;
        }
        
        idle = true;
    }

    // Win and lose
    IEnumerator Victory() {
        yield return player.Move(stairDirection, false);

        Destroy(player.gameObject);
        foreach (var mummy in mummies) 
            Destroy(mummy.gameObject);

        yield return new WaitForSeconds(0.5f);
        Instantiate(winOverlay, transform, true);
    }

    IEnumerator Lost() {
        Vector3 position = player.transform.localPosition;

        Destroy(player.gameObject);
        foreach (var mummy in mummies) 
            Destroy(mummy.gameObject);

        yield return RunEffect(defeat_effect, position, false);
        yield return RunEffect(dust_effect, position, true);

        yield return new WaitForSeconds(0.5f);
        Instantiate(loseOverlay, transform, true);
    }

    // Mummies    
    Vector3 WhiteTrace(Vector3 position) {
        int x = (int) player.transform.localPosition.x;
        int y = (int) player.transform.localPosition.y;
        int z = (int) player.transform.localPosition.z;
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

    Vector3 RedTrace(Vector3 position) {
        int x = (int) player.transform.localPosition.x;
        int y = (int) player.transform.localPosition.y;
        int z = (int) player.transform.localPosition.z;
        int px = (int) position.x;
        int py = (int) position.y;
        int pz = (int) position.z;

        if (y > py) {
            if (!Blocked(position, Vector3.up)) return Vector3.up;
        }
        if (y < py) {
            if (!Blocked(position, Vector3.down)) return Vector3.down;
        }
        if (x > px) return Vector3.right;
        if (x < px) return Vector3.left;
        if (y > py) return Vector3.up;
        if (y < py) return Vector3.down;

        return Vector3.zero;
    }

    IEnumerator MummiesMove() {
        List<IEnumerator> coroutines = new List<IEnumerator>();

        foreach (var mummy in mummies) {
            Vector3 next_move = mummy.tag == "WhiteMummy"
                ? WhiteTrace(mummy.transform.localPosition)
                : RedTrace(mummy.transform.localPosition);
            
            bool isBlocked = Blocked(mummy.transform.localPosition, next_move);
            
            coroutines.Add(mummy.Move(next_move, isBlocked));
        }

        yield return PromiseAll(coroutines.ToArray());
    }

    bool MummiesCatch() {
        foreach (var mummy in mummies) {
            if (mummy.transform.localPosition == player.transform.localPosition)
                return true;
        }
        return false;
    }

    IEnumerator MummiesFight() {
        // Group mummy by position
        var positions = new Dictionary<Vector3, List<Character>>();

        foreach (var mummy in mummies) {
            Vector3 key = mummy.transform.localPosition;
            if (!positions.ContainsKey(key))
                positions.Add(key, new List<Character>());
            
            positions[key].Add(mummy);
        }

        //Delete mummy and run effect
        var effects = new List<IEnumerator>();
        
        foreach (var item in positions) {
            if (item.Value.Count == 1) continue;

            // Preserve one
            item.Value.RemoveAt(0);
            foreach (var mummy in item.Value) {
                mummies.Remove(mummy);
                Destroy(mummy.gameObject);
            }
            
            effects.Add(RunEffect(dust_effect, item.Key, true));
        }

        yield return PromiseAll(effects.ToArray());
    }

    // Character vs walls
    bool Blocked(Vector3 position, Vector3 direction) {
        int x = (int)position.x;
        int y = (int)position.y;
        int n = size-1;
        
        if (direction == Vector3.up)
            return y == n || horizontalWall[x, y+1] == 1;

        if (direction == Vector3.down)
            return y == 0 || horizontalWall[x, y] == 1;

        if (direction == Vector3.left)
            return x == 0 || verticalWall[x, y] == 1;
        
        if (direction == Vector3.right)
            return x == n || verticalWall[x+1, y] == 1;
        
        return true;
    }


    // Helper functions
    IEnumerator RunEffect(GameObject effect, Vector3 position, bool clear) {
        GameObject fx = Instantiate(effect, transform);
        fx.transform.localPosition = position;
        yield return fx.GetComponent<Effect>().Run(clear);
    }
    
    IEnumerator PromiseAll(params IEnumerator[] coroutines) {
        while (true) {
            object current = null;

            foreach (IEnumerator x in coroutines) {
                if (x.MoveNext() == true)
                    current = x.Current;
            }

            if (current == null) break;
            yield return current;
        }
    }

    // Undo
    public void Save() {
        var checkpoint = new Checkpoint();
        checkpoint.player = player.transform.localPosition;
        foreach (var mummy in mummies) {
            checkpoint.mummies.Add(mummy.transform.localPosition);
        }
        checkpoints.Push(checkpoint);
    }

    public void Restore() {
        var checkpoint = (Checkpoint)checkpoints.Pop();
        
    }
}
