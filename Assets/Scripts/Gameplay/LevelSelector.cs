using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    public GameObject[] levels;

    // Start is called before the first frame update
    void Start()
    {
        if (GameGlobal.level < levels.Length) {
            Instantiate(levels[GameGlobal.level], transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
