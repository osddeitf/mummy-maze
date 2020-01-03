using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    // Map configuration
    class Configuration {
        public int size = 6;
        public Vector2 explorer = new Vector2(2, 2);
        public Vector2 mummy = new Vector2(2, 3);
        public Vector2[] verticalWall = {
            new Vector2(1, 5),
            new Vector2(5, 4),
            new Vector2(2, 4),
            new Vector2(5, 3),
            new Vector2(4, 3),
            new Vector2(5, 1),
            new Vector2(3, 1),
        };
        public Vector2[] horizontalWall = {
            new Vector2(2, 1),
            new Vector2(2, 2),
            new Vector2(3, 2),
            new Vector2(1, 3),
            new Vector2(4, 3),
            new Vector2(1, 4),
            new Vector2(3, 4),
            new Vector2(3, 5),
            new Vector2(4, 5)
        };
        // public Vector2 stair = new Vector2(0, 3);
        // public Vector3 stairDirection = Vector3.left;
        
        public Vector2 stair = new Vector2(4, 0);
        public Vector3 stairDirection = Vector3.down;
        
        public bool restrictedVision = false;
    }

    // Inspector
    public GameObject mummy;
    public GameObject explorer;
    public GameObject wall_v;
    public GameObject wall_h;
    public GameObject stair_up;
    public GameObject stair_down;
    public GameObject stair_left;
    public GameObject stair_right;
    
    // Internal
    Character bot;
    Character player;
    Configuration config;
    int[,] verticalWall;
    int[,] horizontalWall;
    bool idle = true;


    GameObject Spawn(GameObject prefab, Vector2 coordinate) {
        Vector3 position = transform.position + new Vector3(coordinate.x*60, coordinate.y*60, 0);
        return (GameObject)Instantiate(prefab, position, Quaternion.identity, transform);
    }

    // Start is called before the first frame update
    void Start()
    {
        config = new Configuration();
        int n = config.size;

        // Wall mask
        verticalWall = new int[n, n];
        horizontalWall = new int[n, n];

        foreach (Vector2 v in config.verticalWall)
            verticalWall[(int)v.x, (int)v.y] = 1;
        
        foreach (Vector2 v in config.horizontalWall)
            horizontalWall[(int)v.x, (int)v.y] = 1;

        // Wall
        for (int y = n-1; y >= 0; y--) {
            for (int x = 0; x < n; x++) {
                if (verticalWall[x, y] == 1) Spawn(wall_v, new Vector2(x, y));
                if (horizontalWall[x, y] == 1) Spawn(wall_h, new Vector2(x, y));
            }
        }

        // Stair
        if (config.stair.x == 0)
            Spawn(stair_left, config.stair);
        else if (config.stair.y == 0)
            Spawn(stair_down, config.stair);
        else if (config.stair.x == config.size)
            Spawn(stair_right, config.stair);
        else if (config.stair.y == config.size)
            Spawn(stair_up, config.stair);        

        // Characters
        bot = Spawn(mummy, config.mummy).GetComponent<Character>();
        player = Spawn(explorer, config.explorer).GetComponent<Character>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!idle) return;

        Vector3 direction = Vector3.zero;

        if (Input.GetKeyDown("up") && Move(ref config.explorer, Vector3.up))
            direction = Vector3.up;
        else if (Input.GetKeyDown("down") && Move(ref config.explorer, Vector3.down))
            direction = Vector3.down;
        else if (Input.GetKeyDown("left") && Move(ref config.explorer, Vector3.left))
            direction = Vector3.left;
        else if (Input.GetKeyDown("right") && Move(ref config.explorer, Vector3.right))
            direction = Vector3.right;

        if (direction != Vector3.zero) StartCoroutine(Action(direction));
    }

    IEnumerator Action(Vector3 direction) {
        idle = false;

        // Player move 1 step
        yield return StartCoroutine(player.Move(direction));
        
        // Mummy move 2 step
        for (int i = 0; i < 2; i++) {
            Vector3 next_move = Trace();
            if (Blocked(config.mummy, next_move))
                bot.Face(next_move);
            else {
                Move(ref config.mummy, next_move);
                yield return StartCoroutine(bot.Move(next_move));
            }
        }

        // Lose
        if (config.explorer == config.mummy) {
            Destroy(player.gameObject);
            idle = false;
            yield break;
        }
        
        if (config.explorer == config.stair) {
            yield return StartCoroutine(player.Move(config.stairDirection));
            yield return StartCoroutine(player.Move(config.stairDirection));
        }

        idle = true;
    }

    bool Blocked(Vector2 position, Vector3 direction) {
        int x = (int)position.x;
        int y = (int)position.y;
        
        if (direction == Vector3.up)
            return y + 1 == config.size || horizontalWall[x, y+1] == 1;
        
        if (direction == Vector3.right)
            return x + 1 == config.size || verticalWall[x+1, y] == 1;

        if (direction == Vector3.down)
            return y == 0 || horizontalWall[x, y] == 1;

        if (direction == Vector3.left)
            return x == 0 || verticalWall[x, y] == 1;
        
        return true;
    }

    bool Move(ref Vector2 position, Vector3 direction) {
        if (Blocked(position, direction)) return false;
        position.x += direction.x;
        position.y += direction.y;
        return true;
    }

    // Mummy trace
    Vector3 Trace() {
        if (config.explorer.x > config.mummy.x) {
            if (!Blocked(config.mummy, Vector3.right)) return Vector3.right;
        }
        if (config.explorer.x < config.mummy.x) {
            if (!Blocked(config.mummy, Vector3.left)) return Vector3.left;
        }
        if (config.explorer.y > config.mummy.y) return Vector3.up;
        if (config.explorer.y < config.mummy.y) return Vector3.down;
        if (config.explorer.x > config.mummy.x) return Vector3.right;
        if (config.explorer.x < config.mummy.x) return Vector3.left;
        return Vector3.zero;
    }
}
