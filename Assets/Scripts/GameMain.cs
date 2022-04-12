using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    public ZombieSpawnManager zombieSpawnManager;

    void Start()
    {
        this.zombieSpawnManager.Init();

        this.zombieSpawnManager.StartSpawn();
    }
}
