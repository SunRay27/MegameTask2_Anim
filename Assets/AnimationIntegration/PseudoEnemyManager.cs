using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PseudoEnemyManager : MonoBehaviour
{
    public static PseudoEnemyManager Instance;

    List<Enemy> enemies = new List<Enemy>();


    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            DestroyImmediate(gameObject);
    }

    public void RegisterEnemy(Enemy newEnemy)
    {
        enemies.Add(newEnemy);
    }

    public Enemy GetClosestEnemy(Vector3 toPosition)
    {
        float minDistance = float.MaxValue;
        int minIndex = -1;

        for (int i = 0; i < enemies.Count; i++)
        {
            //better to remove enemy from list and spawn new one, but I use only one enemy for testing purposes, so it has the flag - <bool Dead>
            if (enemies[i].Dead)
                continue;

            float sqrDistance = (enemies[i].transform.position - toPosition).sqrMagnitude;
            if (sqrDistance <  minDistance)
            {
                minIndex = i;
                minDistance = sqrDistance;
            }
        }

        if (minIndex == -1)
            return null;

        return enemies[minIndex];

    }
}
