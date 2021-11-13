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

    //wave enemy chances
    private float[] enemyWaveChances = { .65f, .7f, .75f, .8f, .85f, .9f, .95f, 1f }; // 0 = cur, 1 = knight, 2 = monk, 3 = paladin, 4 = assassin, 5 = succubus, 6 = armorer, 7 = warlock

    //global enemy worth increase skill
    private float enemyWorthSkill = 0;

    //audio
    private AudioSource waveSmashSound;

    // Start is called before the first frame update
    void Awake()
    {
        //set skill tree values
        GameManager gamemanager = FindObjectOfType<GameManager>();
        float curChance = 1;
        enemyWaveChances[1] = gamemanager.SkillTree["enemyknight"] == 0 ? .05f : .1f;
        enemyWaveChances[2] = gamemanager.SkillTree["enemymonk"] == 1 ? .05f : .1f;
        enemyWaveChances[3] = gamemanager.SkillTree["enemypaladin"] == 5 ? .05f : .1f;
        enemyWaveChances[4] = gamemanager.SkillTree["enemyassassin"] == 0 ? .05f : .1f;
        enemyWaveChances[5] = gamemanager.SkillTree["enemysuccubus"] == .3f ? .05f : .1f;
        enemyWaveChances[6] = gamemanager.SkillTree["enemyarmorer"] == 0 ? .05f : .1f;
        enemyWaveChances[7] = gamemanager.SkillTree["enemywarlock"] == .01f ? .05f : .1f;

        for (int i = 1; i < enemyWaveChances.Length; i++)
        {
            curChance -= enemyWaveChances[i];
        }

        //defaults: 0 = .65, 1 = .7, 2 = .75, 3 = .8, 4 = .85, 5 = .9, 6 = .95, 7 = 1
        enemyWaveChances[0] = curChance;

        for (int i = 1; i < enemyWaveChances.Length; i++)
        {
            enemyWaveChances[i] = enemyWaveChances[i - 1] + enemyWaveChances[i];
        }

        waves = new List<Transform>();
        waveInfos = new List<Wave>();
        totalNumWaves = 1;

        //enemy worth skill
        enemyWorthSkill = gamemanager.SkillTree["enemy1"];

        //sound
        waveSmashSound = GetComponent<AudioSource>();
    }

    private void Start()
    {
        waveSmashSound.volume = FindObjectOfType<GameManager>().sfxVolume;
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

            //wave number multiplier
            int waveNumberMod = totalNumWaves + (Mathf.FloorToInt(totalNumWaves / 10f) * 5);

            //randomly choose enemy class of wave
            float m = Random.Range(0f, 1f);
            EnemyClass enemyClass = EnemyClass.Cur;

            //special enemies cant spawn before wave 10
            if (totalNumWaves < 10)
            {
                m = Random.Range(0f, enemyWaveChances[2]);
            }

            //65% - 30% chance for cur enemies
            if (m <= enemyWaveChances[0])
            {
                enemyClass = EnemyClass.Cur;
                waveInfos.Add(new Wave(waveNumberMod * 2, waveNumberMod * 3f, waveNumberMod * 1.5f, 3.05f, waveNumberMod * 2f + enemyWorthSkill * waveNumberMod, t.ToArray(), enemyClass, totalNumWaves));
            }
            //5% or 10% chance for knight
            else if (m <= enemyWaveChances[1])
            {
                enemyClass = EnemyClass.Knight;
                int numKnights = Mathf.FloorToInt(waveNumberMod / 5f);
                waveInfos.Add(new Wave(numKnights < 1 ? 1 : numKnights, waveNumberMod * 6f, waveNumberMod * 3f, 2.525f, waveNumberMod * 20f + enemyWorthSkill * waveNumberMod, t.ToArray(), enemyClass, totalNumWaves));
            }
            //5% or 10% chance for monk
            else if (m <= enemyWaveChances[2])
            {
                enemyClass = EnemyClass.Monk;
                waveInfos.Add(new Wave(waveNumberMod * 4, waveNumberMod * 1f, waveNumberMod * .5f, 3.575f, waveNumberMod * 1f + enemyWorthSkill * waveNumberMod, t.ToArray(), enemyClass, totalNumWaves));
            }
            //5% or 10% chance for paladin
            else if (m <= enemyWaveChances[3])
            {
                enemyClass = EnemyClass.Paladin;
                int numPaladins = Mathf.FloorToInt(waveNumberMod / 1.5f);
                waveInfos.Add(new Wave(numPaladins < 1 ? 1 : numPaladins, waveNumberMod * 4f, waveNumberMod * 1.5f, 3.05f, waveNumberMod * 6.5f + enemyWorthSkill * waveNumberMod, t.ToArray(), enemyClass, totalNumWaves));
            }
            //5% or 10% chance for assassin
            else if (m <= enemyWaveChances[4])
            {
                enemyClass = EnemyClass.Assassin;
                int numAssassins = Mathf.FloorToInt(waveNumberMod / 1.5f);
                waveInfos.Add(new Wave(numAssassins < 1 ? 1 : numAssassins, waveNumberMod * 4f, waveNumberMod * 2f, 3f, waveNumberMod * 6.5f + enemyWorthSkill * waveNumberMod, t.ToArray(), enemyClass, totalNumWaves));
            }
            //5% or 10% chance for succubus
            else if (m <= enemyWaveChances[5])
            {
                enemyClass = EnemyClass.Succubus;
                int numSuccubi = Mathf.FloorToInt(waveNumberMod / 2f);
                waveInfos.Add(new Wave(numSuccubi < 1 ? 1 : numSuccubi, waveNumberMod * 4f, waveNumberMod * 4f, 3.32f, waveNumberMod * 8f + enemyWorthSkill * waveNumberMod, t.ToArray(), enemyClass, totalNumWaves));
            }
            //5% or 10% chance for armorer
            else if (m <= enemyWaveChances[6])
            {
                enemyClass = EnemyClass.Armorer;
                int numArmorers = Mathf.FloorToInt(waveNumberMod / 2f);
                waveInfos.Add(new Wave(numArmorers < 1 ? 1 : numArmorers, waveNumberMod * 4f, waveNumberMod * 3f, 3.1f, waveNumberMod * 8f + enemyWorthSkill * waveNumberMod, t.ToArray(), enemyClass, totalNumWaves));
            }
            //5% or 10% chance for warlock
            else //if(m <= enemyWaveChances[7])
            {
                enemyClass = EnemyClass.Warlock;
                int numWarlocks = Mathf.FloorToInt(waveNumberMod / 1.5f);
                waveInfos.Add(new Wave(numWarlocks < 1 ? 1 : numWarlocks, waveNumberMod * 3f, waveNumberMod * 1.5f, 3.2f, waveNumberMod * 6.5f + enemyWorthSkill * waveNumberMod, t.ToArray(), enemyClass, totalNumWaves));
            }

            waves[waves.Count - 1].GetComponent<WaveHover>().SetWaveInfo(waveInfos[waves.Count - 1]);
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
            waveSmashSound.Play();
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

    //property for smash sound
    public AudioSource WaveSmashSound
    {
        get { return waveSmashSound; }
        set { waveSmashSound = value; }
    }
}
