using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameGlobal
{
    public static int level = 0;

    public static void Play(int lv) {
        level = lv;
        Restart();
    }

    public static void Restart() {
        SceneManager.LoadScene("Gameplay");
    }

    public static void MainMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    public static void WorldMap() {
        SceneManager.LoadScene("WorldMap");
    }
}
