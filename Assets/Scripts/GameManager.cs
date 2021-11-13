using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    //pause bool
    private bool paused;
    private bool optionsActive;
    private bool restarting = false;

    //pause screen
    public GameObject pauseParent;

    //wave manager
    public WaveManager waveManager;

    //base manager
    public BaseManager baseManager;

    //currency manager
    public CurrencyManager currencyManager;

    //number of waves to create
    public int numWaves = 10;

    //should the map be randomized
    public bool randomizeMap = false;
    public Toggle randomMapToggle;
    public MapGenerator mapGenerator;

    //private Transform[,] towerLookUp;

    //save files
    private string options = "saves\\options.ldata";
    private StreamWriter writer = null;
    private StreamReader reader = null;

    //charts
    private bool elementChartActive = false;
    private bool towerChartActive = false;

    //audio
    public Slider musicSlider;
    public AudioSource musicSource;
    public Text musicText;
    public AudioSource aha;
    public Slider soundEffectSlider;
    public Text soundEffectText;
    public float sfxVolume { get; set; }

    //skill tree
    private int skillPoints = 0;
    private string skillTreeFile = "saves\\skilltree.ldata";
    private string skillPointFile = "saves\\skillpoints.ldata";
    private Dictionary<string, float> skillTreeValues;
    private int skillPointCounter = 0;
    public Image skillPointImage;
    private Text skillPointNotifText;
    private bool skillNotificationOn = false;

    //tower ui active
    public bool otherTowerUIActive { get; set; }

    //dont show map pref (this just has to be here for saving options to file)
    private bool dontShowMapPref;

    // Start is called before the first frame update
    void Awake()
    {
        //read options from file
        ReadOptionsFromFile();

        //towerLookUp = new Transform[16, 39];

        Physics2D.queriesStartInColliders = false;

        randomMapToggle.onValueChanged.AddListener(delegate { ToggleRandomMap(randomMapToggle); });
        musicSlider.onValueChanged.AddListener(delegate { ChangeMusicVolume(musicSlider, musicSource, musicText); });
        soundEffectSlider.onValueChanged.AddListener(delegate { ChangeSFXVolume(soundEffectSlider, soundEffectText); });

        skillTreeValues = new Dictionary<string, float>();
        ReadSkillTreeFromFile();
    }

    private void Start()
    {
        paused = false;
        Time.timeScale = 1;

        waveManager.AddWaves(numWaves);

        if (randomizeMap)
        {
            //mapGenerator.BinaryTreeMazeGen();
            mapGenerator.DepthFirstSearchMazeGen();
        }
        else
        {
            mapGenerator.GeneratePresetMap(Random.Range(0, 10));
        }

        skillPointNotifText = skillPointImage.transform.GetChild(0).GetComponent<Text>();

        otherTowerUIActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (restarting)
        {
            return;
        }

        //toggle pausing
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //turn off charts if any are on before pausing
            if (elementChartActive)
            {
                ToggleChart(0);
            }

            if (towerChartActive)
            {
                ToggleChart(1);
            }

            if (!optionsActive)
            {
                TogglePause();
            }
            else
            {
                ToggleOptions();
            }
        }

        //toggle element chart
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (towerChartActive)
            {
                ToggleChart(1);
            }

            ToggleChart(0);
        }

        //toggle tower chart
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (elementChartActive)
            {
                ToggleChart(0);
            }

            ToggleChart(1);
        }

        //skill point notification fade out
        if (skillNotificationOn)
        {
            Color spc = skillPointImage.color;
            spc.a -= Time.deltaTime;

            if (spc.a <= 0)
            {
                skillNotificationOn = false;
            }

            skillPointImage.color = spc;
            skillPointNotifText.color = spc;
        }

        //only do stuff if the game is not paused
        if (!paused)
        {
            waveManager.MoveWaves();

            if (waveManager.WavesRemaining <= 10)
            {
                waveManager.AddWaves(numWaves);
                if (waveManager.GetWave(0).waveNumber != 1)
                {
                    //increase max health and heal
                    baseManager.IncreaseMaxHealth(skillTreeValues["base3"]);
                    baseManager.HealHealth(skillTreeValues["base2"]);

                    //gain skill points
                    skillPointCounter++;
                    AddAndSaveSkillPoints(skillPointCounter);
                    SkillPointNotification(skillPointCounter);
                    aha.PlayDelayed(.5f);
                }

            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                waveManager.WaveSpeed = 180;
            }

            ////permanent godmode for debugging
            //if (Input.GetKeyDown(KeyCode.G))
            //{
            //    baseManager.IncreaseMaxHealth(999999999989);
            //}

            ////increase money for debugging
            //if (Input.GetKeyDown(KeyCode.C))
            //{
            //    currencyManager.CurrentCurrency += currencyManager.CurrentCurrency * 10;
            //}
        }

        if (!baseManager.IsAlive && !paused)
        {
            pauseParent.SetActive(true);
            pauseParent.transform.GetChild(1).gameObject.SetActive(false);
            pauseParent.transform.GetChild(2).gameObject.SetActive(true);
            pauseParent.transform.GetChild(3).gameObject.SetActive(true);
            pauseParent.transform.GetChild(4).gameObject.SetActive(true);
            pauseParent.transform.GetChild(8).gameObject.SetActive(true);
            paused = true;
            Time.timeScale = 0;

            TowerAI[] towers = FindObjectsOfType<TowerAI>();
            foreach (TowerAI tower in towers)
            {
                if (tower.TowerInfoOn)
                {
                    tower.ToggleTowerInfo();
                    otherTowerUIActive = false;
                }
            }
        }
        else if (!baseManager.IsAlive && paused)
        {

        }
    }

    //toggle pausing the game
    public void TogglePause()
    {
        //dont allow player to unpause after they die
        if (paused && baseManager.IsAlive)
        {
            paused = false;
            pauseParent.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            paused = true;
            pauseParent.SetActive(true);
            Time.timeScale = 0;

            TowerAI[] towers = FindObjectsOfType<TowerAI>();
            foreach (TowerAI tower in towers)
            {
                if (tower.TowerInfoOn)
                {
                    tower.ToggleTowerInfo();
                    otherTowerUIActive = false;
                }
            }
        }
    }

    //restart the game
    public void RestartGame()
    {
        restarting = true;

        pauseParent.transform.GetChild(1).gameObject.SetActive(true);
        pauseParent.transform.GetChild(2).gameObject.SetActive(true);
        pauseParent.transform.GetChild(3).gameObject.SetActive(true);
        pauseParent.transform.GetChild(4).gameObject.SetActive(true);
        pauseParent.transform.GetChild(8).gameObject.SetActive(false);
        pauseParent.SetActive(false);
        paused = false;
        Time.timeScale = 1;

        SceneManager.LoadScene("Endless_Level");
    }

    //go to main menu
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    //go to options menu
    public void ToggleOptions()
    {
        if (!optionsActive)
        {
            pauseParent.transform.GetChild(1).gameObject.SetActive(false);
            pauseParent.transform.GetChild(2).gameObject.SetActive(false);
            pauseParent.transform.GetChild(3).gameObject.SetActive(false);
            pauseParent.transform.GetChild(4).gameObject.SetActive(false);
            pauseParent.transform.GetChild(5).gameObject.SetActive(true);
            pauseParent.transform.GetChild(6).gameObject.SetActive(true);
            pauseParent.transform.GetChild(7).gameObject.SetActive(true);
            optionsActive = true;
        }
        else
        {
            pauseParent.transform.GetChild(1).gameObject.SetActive(true);
            pauseParent.transform.GetChild(2).gameObject.SetActive(true);
            pauseParent.transform.GetChild(3).gameObject.SetActive(true);
            pauseParent.transform.GetChild(4).gameObject.SetActive(true);
            pauseParent.transform.GetChild(5).gameObject.SetActive(false);
            pauseParent.transform.GetChild(6).gameObject.SetActive(false);
            pauseParent.transform.GetChild(7).gameObject.SetActive(false);
            optionsActive = false;
        }
    }

    //public Transform[,] TowerLookUp
    //{
    //    get { return towerLookUp; }
    //    set { towerLookUp = value; }
    //}

    //toggle random map option and save to file
    public void ToggleRandomMap(Toggle toggle)
    {
        randomizeMap = toggle.isOn;
    }

    //returns the variable contained within a line from a file
    private string GetVariableFromText(string fullText, string id, char lineEnd)
    {
        return fullText.Substring(fullText.IndexOf(id) + id.Length, fullText.Substring(fullText.IndexOf(id)).IndexOf(lineEnd) - id.Length);
    }

    //property for paused
    public bool IsPaused
    {
        get { return paused; }
    }

    //proprty for restarting
    public bool IsRestarting
    {
        get { return restarting; }
    }

    //base manager property
    public BaseManager BaseManager
    {
        get { return baseManager; }
        set { baseManager = value; }
    }

    //save options to file
    public void SaveOptionsToFile()
    {
        writer = new StreamWriter(options, false);
        writer.WriteLine("randommap=" + randomizeMap + ";");
        writer.WriteLine("musicvolume=" + musicSource.volume + ";");
        writer.WriteLine("sfxvolume=" + sfxVolume + ";");
        writer.WriteLine("dontshowmappref=" + dontShowMapPref + ';');

        if (writer != null)
        {
            writer.Close();
        }
    }

    //read and apply options from file
    private void ReadOptionsFromFile()
    {
        //if the file is missing create it with default values
        if (!File.Exists(options))
        {
            writer = new StreamWriter(options, false);
            writer.WriteLine("randommap=" + false + ";");
            writer.WriteLine("musicvolume=" + 0.25 + ";");
            writer.WriteLine("sfxvolume=" + 0.25 + ";");
            writer.WriteLine("dontshowmappref=" + false + ';');

            if (writer != null)
            {
                writer.Close();
            }
        }

        reader = new StreamReader(options);
        string optionsText = reader.ReadToEnd();

        //random map
        randomizeMap = bool.Parse(GetVariableFromText(optionsText, "randommap=", ';'));
        randomMapToggle.isOn = randomizeMap;

        //music volume
        musicSlider.value = float.Parse(GetVariableFromText(optionsText, "musicvolume=", ';'));
        musicSource.volume = musicSlider.value;
        musicText.text = "<color=ff6600>" + Mathf.Floor(musicSlider.value * 100) + "%</color> music volume";

        //sound effect volume
        soundEffectSlider.value = float.Parse(GetVariableFromText(optionsText, "sfxvolume=", ';'));
        sfxVolume = soundEffectSlider.value;
        soundEffectText.text = "<color=ff6600>" + Mathf.Floor(musicSlider.value * 100) + "%</color> sound effect volume";
        aha.volume = sfxVolume;

        //dont show map pref
        dontShowMapPref = bool.Parse(GetVariableFromText(optionsText, "dontshowmappref=", ';'));

        if (reader != null)
        {
            reader.Close();
        }
    }

    //toggle displaying charts
    private void ToggleChart(int chart)
    {
        //element chart
        if (chart == 0)
        {
            elementChartActive = !elementChartActive;
            pauseParent.SetActive(elementChartActive);
            pauseParent.transform.GetChild(9).gameObject.SetActive(elementChartActive);
            paused = elementChartActive;
        }
        //tower chart
        else
        {
            towerChartActive = !towerChartActive;
            pauseParent.SetActive(towerChartActive);
            pauseParent.transform.GetChild(10).gameObject.SetActive(towerChartActive);
            paused = towerChartActive;
        }
    }

    //change music volume based on slider value
    private void ChangeMusicVolume(Slider slider, AudioSource audio, Text label)
    {
        audio.volume = slider.value;

        label.text = string.Format("<color=ff6600>{0}%</color> music volume", Mathf.Floor(slider.value * 100));
    }

    //change all sfx volume based on slider value
    private void ChangeSFXVolume(Slider slider, Text label)
    {
        sfxVolume = slider.value;

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (EnemyAI enemy in enemies)
        {
            enemy.DeathSoundVolume = sfxVolume;
        }

        TowerAI[] towers = FindObjectsOfType<TowerAI>();
        foreach (TowerAI tower in towers)
        {
            tower.ShootSound.volume = sfxVolume;
        }

        waveManager.WaveSmashSound.volume = sfxVolume;

        aha.volume = sfxVolume;

        label.text = string.Format("<color=ff6600>{0}%</color> sound effect volume", Mathf.Floor(sfxVolume * 100));
    }

    //save skill points to file
    private void AddAndSaveSkillPoints(int points)
    {
        skillPoints += points;

        writer = new StreamWriter(skillPointFile, false);

        writer.WriteLine("skillpoints=" + skillPoints + ";");

        if (writer != null)
        {
            writer.Close();
        }
    }

    //read skill tree from file
    private void ReadSkillTreeFromFile()
    {
        if (!File.Exists(skillTreeFile))
        {
            skillTreeValues.Add("money1", 0.01f);
            skillTreeValues.Add("money2", 100);
            skillTreeValues.Add("money3", 10);
            skillTreeValues.Add("money4", 2);
            skillTreeValues.Add("base1", 10);
            skillTreeValues.Add("base2", 0);
            skillTreeValues.Add("base3", 5);
            skillTreeValues.Add("enemy1", 0);
            skillTreeValues.Add("enemyknight", 0);
            skillTreeValues.Add("enemymonk", 1);
            skillTreeValues.Add("enemyarmorer", 0);
            skillTreeValues.Add("enemywarlock", .25f);
            skillTreeValues.Add("enemyassassin", 0);
            skillTreeValues.Add("enemypaladin", 5);
            skillTreeValues.Add("enemysuccubus", .3f);
            skillTreeValues.Add("upgraderange", 2);
            skillTreeValues.Add("upgradeattackspeed", 2);
            skillTreeValues.Add("upgradeattack", 2);
        }
        else
        {
            //read skill tree
            reader = new StreamReader(skillTreeFile);
            string skillTreeText = reader.ReadToEnd();
            
            skillTreeValues.Clear();

            skillTreeValues.Add("money1", float.Parse(GetVariableFromText(skillTreeText, "money1value=", ';')));
            skillTreeValues.Add("money2", float.Parse(GetVariableFromText(skillTreeText, "money2value=", ';')));
            skillTreeValues.Add("money3", float.Parse(GetVariableFromText(skillTreeText, "money3value=", ';')));
            skillTreeValues.Add("money4", float.Parse(GetVariableFromText(skillTreeText, "money4value=", ';')));
            skillTreeValues.Add("base1", float.Parse(GetVariableFromText(skillTreeText, "base1value=", ';')));
            skillTreeValues.Add("base2", float.Parse(GetVariableFromText(skillTreeText, "base2value=", ';')));
            skillTreeValues.Add("base3", float.Parse(GetVariableFromText(skillTreeText, "base3value=", ';')));
            skillTreeValues.Add("enemy1", float.Parse(GetVariableFromText(skillTreeText, "enemy1value=", ';')));
            skillTreeValues.Add("enemyknight", float.Parse(GetVariableFromText(skillTreeText, "enemyknightvalue=", ';')));
            skillTreeValues.Add("enemymonk", float.Parse(GetVariableFromText(skillTreeText, "enemymonkvalue=", ';')));
            skillTreeValues.Add("enemyarmorer", float.Parse(GetVariableFromText(skillTreeText, "enemyarmorervalue=", ';')));
            skillTreeValues.Add("enemywarlock", float.Parse(GetVariableFromText(skillTreeText, "enemywarlockvalue=", ';')));
            skillTreeValues.Add("enemyassassin", float.Parse(GetVariableFromText(skillTreeText, "enemyassassinvalue=", ';')));
            skillTreeValues.Add("enemypaladin", float.Parse(GetVariableFromText(skillTreeText, "enemypaladinvalue=", ';')));
            skillTreeValues.Add("enemysuccubus", float.Parse(GetVariableFromText(skillTreeText, "enemysuccubusvalue=", ';')));
            skillTreeValues.Add("upgraderange", float.Parse(GetVariableFromText(skillTreeText, "upgraderangevalue=", ';')));
            skillTreeValues.Add("upgradeattackspeed", float.Parse(GetVariableFromText(skillTreeText, "upgradeattackspeedvalue=", ';')));
            skillTreeValues.Add("upgradeattack", float.Parse(GetVariableFromText(skillTreeText, "upgradeattackvalue=", ';')));

            if (reader != null)
            {
                reader.Close();
            }
        }

        if (!File.Exists(skillPointFile))
        {
            skillPoints = 0;
        }
        else
        {
            //read skill tree
            reader = new StreamReader(skillPointFile);
            string skillPointText = reader.ReadToEnd();

            skillPoints = int.Parse(GetVariableFromText(skillPointText, "skillpoints=", ';'));

            if (reader != null)
            {
                reader.Close();
            }
        }
    }

    //property for skill tree values
    public Dictionary<string, float> SkillTree
    {
        get { return skillTreeValues; }
    }

    //skill point gain notifier
    private void SkillPointNotification(int points)
    {
        skillPointNotifText.text = "+" + points;
        skillPointImage.color = new Color(1, 1, 1, 1);
        skillPointNotifText.color = new Color(1, 1, 1, 1);
        skillNotificationOn = true;
    }
}
