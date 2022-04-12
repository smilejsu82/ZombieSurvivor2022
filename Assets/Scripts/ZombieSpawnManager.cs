using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ZombieSpawnManager : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject zombiePrefab;
    public int totalZombieCount = 10;
    public float spawnDelay = 1f;

    private List<Zombie> zombies = new List<Zombie>();

    public void Init()
    {
        this.CreateZombies();
    }

    private void CreateZombies()
    {
        for(int i = 0; i<this.totalZombieCount; i++){
            var zombieGo = Object.Instantiate<GameObject>(this.zombiePrefab, this.transform);
            zombieGo.SetActive(false);
            var zombie = zombieGo.GetComponent<Zombie>();
            zombie.dieEvent.AddListener(()=>{


                //죽는 애니메이션이 끝난 상태 
                zombie.gameObject.SetActive(false);
                zombie.transform.SetParent(this.transform);

            });
            this.zombies.Add(zombie);
        }
    }

    public void StartSpawn()
    {
        this.StartCoroutine(this.WaitForSpawn());
    }

    private IEnumerator WaitForSpawn()
    {
        while(true){
            yield return new WaitForSeconds(this.spawnDelay);

            var randIdx = UnityEngine.Random.Range(0, 3);
            var randPoint = this.spawnPoints[randIdx];
            var randPosition = randPoint.position;

            var zombie = this.zombies.First();

            zombie.Init(randPosition);

            this.zombies.Remove(zombie);

            if( this.zombies.Count == 0){
                break;
            }
        }

        Debug.Log("모든 좀비들을 방생 했습니다.");
    }
}
