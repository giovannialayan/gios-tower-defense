using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFinder : MonoBehaviour
{
    private TowerAI tower;

    //variables for specifying which enemy to target
    //private List<EnemyAI> currentEnemyList;
    //private EnemyAI currentEnemyScript;
    //private Transform currentEnemyTransform;

    // Start is called before the first frame update
    void Start()
    {
        tower = transform.parent.gameObject.GetComponent<TowerAI>();
        //currentEnemyList = new List<EnemyAI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //get enemy in range
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.tag == "enemy")
        {
            tower.EnemyInRange = collision.transform;
        }
    }

    //WHAT IS THE PURPOSE OF THE CODE BELOW
    //this is meant to be used to target the enemy closest to the end.
    //it works until a tower has a constant stream of enemies around it,
    //then it stops attacking, resetting the attackspeed when a new target
    //is selected didnt fix it \__(-_-)__/

    //void Update()
    //{
    //    if (currentEnemyList.Count > 0)
    //    {
    //        //if the list only has one enemy just choose that one
    //        if (currentEnemyList.Count == 1)
    //        {
    //            currentEnemyScript = currentEnemyList[0];
    //            currentEnemyTransform = currentEnemyList[0].gameObject.transform;
    //        }
    //        else
    //        {
    //            //find the enemy closest to the end if there is more than one enemy
    //            for (int i = 0; i < currentEnemyList.Count; i++)
    //            {
    //                if (currentEnemyList[i] != null && currentEnemyList[i].DistToEnd() < currentEnemyScript.DistToEnd())
    //                {
    //                    currentEnemyScript = currentEnemyList[i];
    //                    currentEnemyTransform = currentEnemyList[i].gameObject.transform;
    //                }
    //                //get rid of destroyed enemies
    //                else if (currentEnemyList[i] == null)
    //                {
    //                    currentEnemyList.RemoveAt(i);
    //                    i--;
    //                }
    //            }
    //        }

    //        tower.ChangeTarget(currentEnemyTransform);
    //    }
    //}

    ////get enemy in range
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.transform.tag == "enemy")
    //    {
    //        currentEnemyList.Add(collision.transform.gameObject.GetComponent<EnemyAI>());
    //    }
    //}

    ////remove enemy from list when they exit range
    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.transform.tag == "enemy")
    //    {
    //        currentEnemyList.Remove(collision.transform.gameObject.GetComponent<EnemyAI>());
    //    }
    //}
}
