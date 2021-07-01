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
    private string options = "saves\\options.txt";
    private StreamWriter writer = null;
    private StreamReader reader = null;

    //charts
    private bool elementChartActive = false;
    private bool towerChartActive = false;

    //audio
    public Slider musicSlider;
    public AudioSource musicSource;
    public Text musicText;

    // Start is called before the first frame update
    void Start()
    {
        //read options from file
        ReadOptionsFromFile();

        paused = false;

        waveManager.AddWaves(numWaves);

        if (randomizeMap)
        {
            //mapGenerator.BinaryTreeMazeGen();
            mapGenerator.DepthFirstSearchMazeGen();
        }

        //towerLookUp = new Transform[16, 39];

        Physics2D.queriesStartInColliders = false;

        randomMapToggle.onValueChanged.AddListener(delegate { ToggleRandomMap(randomMapToggle); });
        musicSlider.onValueChanged.AddListener(delegate { ChangeVolume(musicSlider, musicSource, musicText); });
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

        //only do stuff if the game is not paused
        if (!paused)
        {
            waveManager.MoveWaves();

            if (waveManager.WavesRemaining <= 10)
            {
                waveManager.AddWaves(numWaves);
                if (waveManager.GetWave(0).waveNumber != 1)
                {
                    baseManager.IncreaseMaxHealth(5);
                    baseManager.HealHealth(baseManager.MaxHealth * .2f);
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
            writer.WriteLine("musicvolume=" + 0.5 + ";");

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

    //change volume based on slider value
    private void ChangeVolume(Slider slider, AudioSource audio, Text label)
    {
        audio.volume = slider.value;

        label.text = string.Format("<color=ff6600>{0}%</color> music volume", Mathf.Floor(slider.value * 100));
    }
}
