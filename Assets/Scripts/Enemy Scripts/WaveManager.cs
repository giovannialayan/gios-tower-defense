using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    //array of wave block transforms
    private List<Transform> waves;

    //array of wave information
    private List<Wave> waveInfos;

    //wave block prefab reference
    public GameObject waveBlock;

    //wave block parent object
    public Transform waveParent;

    //spawn manager reference
    public SpawnManager spawnManager;

    //wave blocks speed
    private float blockSpeed = 3.6f;

    //total waves throughout game
    private int totalNumWaves;

    // Start is called before the first frame update
    void Awake()
    {
        waves = new List<Transform>();
        waveInfos = new List<Wave>();
        totalNumWaves = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //adds waves to the game and handles showing them
    public void AddWaves(int wavesAdd)
    {
        //set up waveInfos to start editing at the add point if needed
        int prevLen = waveInfos.Count;

        //create new wave blocks
        for(int i = 0; i < wavesAdd; i++)
        {
            waves.Add(Instantiate(waveBlock, waveParent).transform);
            int waveNum = i + 1;

            //randomly set enemy types
            int n = Random.Range(1, 9);
            List<ElementTypes> t = new List<ElementTypes>();
            for(int j = 0; j < n; j++)
            {
                ElementTypes nextType = (ElementTypes)Random.Range(0, 8);

                if (!t.Contains(nextType))
                {
                    t.Add(nextType);
                }
            }

            waveInfos.Add(new Wave(waveNum * 5, waveNum * 2f, waveNum * 1.5f, waveNum * 1.05f, waveNum * 2f, t.ToArray(), totalNumWaves));
            waves[waveNum - 1].GetComponent<WaveHover>().SetWaveInfo(waveInfos[waveNum - 1]);
            totalNumWaves++;
        }

        //set the correct x pos for all waves x = i*110
        for(int i = 0; i < waves.Count; i++)
        {
            waves[i].localPosition = new Vector3((i + 1) * 110, 0, 0);
            waves[i].GetComponentInChildren<Text>().text = string.Format("{0}", waveInfos[i].waveNumber);
        }
    }

    //move the waves and activate them when they reach the end
    public void MoveWaves()
    {
        //move waves to the left
        for (int i = 0; i < waves.Count; i++)
        {
            waves[i].localPosition = new Vector3(waves[i].localPosition.x - (blockSpeed * Time.deltaTime), 0, 0);
        }

        //activate the next wave if it reaches the end then destroy the wave object and remove it from the lists
        if (waves[0].localPosition.x <= 0)
        {
            spawnManager.StartSpawning(waveInfos[0]);
            //Debug.Log(string.Format("wave spawned with {0}e, {1}h, {2}a, {3}s, {4}w", waveInfos[0].enemies, waveInfos[0].health, waveInfos[0].attack, waveInfos[0].speed, waveInfos[0].worth, waveInfos[0].elementTypes));

            Destroy(waves[0].gameObject);
            waves.RemoveAt(0);
            waveInfos.RemoveAt(0);
            blockSpeed = 3.6f;
        }
    }

    //get the information of a certain wave
    public Wave GetWave(int index)
    {
        return waveInfos[index];
    }

    //get waves remaining
    public int WavesRemaining
    {
        get { return waveInfos.Count; }
    }

    //set speed of blocks
    public float WaveSpeed
    {
        get { return blockSpeed; }
        set { blockSpeed = value; }
    }

    public SpawnManager SpawnMan
    {
        get { return spawnManager; }
        set { spawnManager = value; }
    }
}
