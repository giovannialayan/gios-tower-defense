using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenu : MonoBehaviour
{
    //tutorial
    public Transform tutorialParent;
    private bool tutorialOn = false;
    private int tutorialPage;

    //file saving
    private StreamWriter writer = null;
    private StreamReader reader = null;

    //skills
    private string skillsFile = "saves\\skills.txt";
    private string skillTreeFile = "saves\\skilltree.txt";
    private string skillPointFile = "saves\\skillpoints.txt";
    public Transform skillsParent;
    private Dictionary<string, float> skillTreeValues;
    private Dictionary<string, int> skillTreeCosts;
    private Dictionary<string, float> skillTreeIncrement;
    private Dictionary<string, float> skillTreeMax;
    private int skillPoints = 0;
    public Text skillPointUI;

    //options
    private string options = "saves\\options.txt";
    public bool randomizeMap = false;
    public Toggle randomMapToggle;
    public Slider musicSlider;
    public AudioSource musicSource;
    public Text musicText;
    public GameObject optionsParent;
    private float sfxVolume;
    public Slider soundEffectSlider;
    public Text soundEffectText;

    //enemypedia
    private bool enemyPediaActive = false;
    private bool[] wasFocused = new bool[8];
    public Transform[] enemypedia = new Transform[8]; //0 = cur, 1 = knight, 2 = monk, 3 = assassin, 4 = paladin, 5 = armorer, 6 = succubus, 7 = warlock
    public GameObject enemypediaParent;
    private string enemypediaFile = "saves\\enemypedia.txt";

    // Start is called before the first frame update
    void Start()
    {
        //read skills and options from file
        ReadSkillsFromFile();
        ReadOptionsFromFile();

        //options
        randomMapToggle.onValueChanged.AddListener(delegate { ToggleRandomMap(randomMapToggle); });
        musicSlider.onValueChanged.AddListener(delegate { ChangeMusicVolume(musicSlider, musicSource, musicText); });
        soundEffectSlider.onValueChanged.AddListener(delegate { ChangeSFXVolume(soundEffectSlider, soundEffectText); });

        //enemypedia
        ReadEnemypediaFromFile();
        enemypedia[0].GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CalculateEnemyWave(enemypedia[0].GetChild(2).GetComponent<InputField>(), enemypedia[0].GetChild(0).GetComponent<Text>(), EnemyClass.Cur); });
        enemypedia[1].GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CalculateEnemyWave(enemypedia[1].GetChild(2).GetComponent<InputField>(), enemypedia[1].GetChild(0).GetComponent<Text>(), EnemyClass.Knight); });
        enemypedia[2].GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CalculateEnemyWave(enemypedia[2].GetChild(2).GetComponent<InputField>(), enemypedia[2].GetChild(0).GetComponent<Text>(), EnemyClass.Monk); });
        enemypedia[3].GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CalculateEnemyWave(enemypedia[3].GetChild(2).GetComponent<InputField>(), enemypedia[3].GetChild(0).GetComponent<Text>(), EnemyClass.Assassin); });
        enemypedia[4].GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CalculateEnemyWave(enemypedia[4].GetChild(2).GetComponent<InputField>(), enemypedia[4].GetChild(0).GetComponent<Text>(), EnemyClass.Paladin); });
        enemypedia[5].GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CalculateEnemyWave(enemypedia[5].GetChild(2).GetComponent<InputField>(), enemypedia[5].GetChild(0).GetComponent<Text>(), EnemyClass.Armorer); });
        enemypedia[6].GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CalculateEnemyWave(enemypedia[6].GetChild(2).GetComponent<InputField>(), enemypedia[6].GetChild(0).GetComponent<Text>(), EnemyClass.Succubus); });
        enemypedia[7].GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { CalculateEnemyWave(enemypedia[7].GetChild(2).GetComponent<InputField>(), enemypedia[7].GetChild(0).GetComponent<Text>(), EnemyClass.Warlock); });

        //skill tree
        skillTreeCosts = new Dictionary<string, int>();
        skillTreeValues = new Dictionary<string, float>();
        skillTreeIncrement = new Dictionary<string, float>();
        skillTreeMax = new Dictionary<string, float>();
        skillTreeIncrement.Add("money1", .005f);
        skillTreeIncrement.Add("money2", 10);
        skillTreeIncrement.Add("money3", -1);
        skillTreeIncrement.Add("money4", -.05f);
        skillTreeIncrement.Add("base1", 1);
        skillTreeIncrement.Add("base2", 1);
        skillTreeIncrement.Add("base3", 1);
        skillTreeIncrement.Add("enemy1", 1);
        skillTreeIncrement.Add("enemyknight", .05f);
        skillTreeIncrement.Add("enemyarmorer", .1f);
        skillTreeIncrement.Add("enemywarlock", -.01f);
        skillTreeIncrement.Add("enemymonk", -.01f);
        skillTreeIncrement.Add("enemyassassin", -.01f);
        skillTreeIncrement.Add("enemypaladin", -.1f);
        skillTreeIncrement.Add("enemysuccubus", -.01f);
        skillTreeIncrement.Add("upgraderange", -.05f);
        skillTreeIncrement.Add("upgradeattackspeed", -.05f);
        skillTreeIncrement.Add("upgradeattack", -.05f);

        skillTreeMax.Add("money3", 1);
        skillTreeMax.Add("money4", 1.25f);
        skillTreeMax.Add("enemywarlock", .05f);
        skillTreeMax.Add("enemymonk", .5f);
        skillTreeMax.Add("enemyassassin", .5f);
        skillTreeMax.Add("enemypaladin", 2);
        skillTreeMax.Add("enemysuccubus", .1f);
        skillTreeMax.Add("upgraderange", 1.25f);
        skillTreeMax.Add("upgradeattackspeed", 1.25f);
        skillTreeMax.Add("upgradeattack", 1.25f);

        ReadSkillTreeFromFile();
    }

    // Update is called once per frame
    void Update()
    {
        //go through tutorial
        if (tutorialOn && Input.anyKeyDown)
        {
            if (tutorialPage == 6)
            {
                for (int i = 0; i < 6; i++)
                {
                    tutorialParent.GetChild(i).gameObject.SetActive(false);
                }

                tutorialParent.gameObject.SetActive(false);
                tutorialOn = false;
            }
            else
            {
                tutorialParent.GetChild(tutorialPage).gameObject.SetActive(true);
                tutorialPage++;
            }
        }

        //enemypedia
        if (enemyPediaActive)
        {
            //check if it was focused previous frame because pressing enter removes focus (fun fact)

            for (int i = 0; i < enemypedia.Length; i++)
            {
                if (wasFocused[i] && Input.GetKeyDown(KeyCode.Return))
                {
                    CalculateEnemyWave(enemypedia[i].GetChild(2).GetComponent<InputField>(), enemypedia[i].GetChild(0).GetComponent<Text>(), EnemyClass.Cur);
                }

                wasFocused[i] = enemypedia[i].GetChild(2).GetComponent<InputField>().isFocused;
            }
        }
    }

    //starts endless mode
    public void StartEndlessMode()
    {
        SceneManager.LoadScene("Endless_Level");
    }

    //opens tutorial
    public void StartTutorial()
    {
        tutorialParent.gameObject.SetActive(true);
        tutorialParent.GetChild(0).gameObject.SetActive(true);
        tutorialOn = true;
        tutorialPage = 1;
    }

    //exits game
    public void QuitGame()
    {
        Application.Quit();
    }

    //open survey
    public void OpenSurvey()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSeqe-ToPxbwRR6aJFXfxxFJegxGMzvHPsUWI56XXEBgwypZ4g/viewform?usp=sf_link");
    }

    //open bug report
    public void OpenBugReport()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSdm4iQLfaBhJvp9J99qtNOtYBlxeHrv9goTkqub_1JiquIxjQ/viewform?usp=sf_link");
    }

    //sets skills from file save
    private void ReadSkillsFromFile()
    {
        //if the file is missing create it with default values
        if (!File.Exists(skillsFile))
        {
            writer = new StreamWriter(skillsFile, false);
            writer.WriteLine("orderskill=false;");
            writer.WriteLine("lightningskill=false;");
            writer.WriteLine("fireskill=false;");
            writer.WriteLine("waterskill=false;");
            writer.WriteLine("chaosskill=false;");
            writer.WriteLine("whimsicalskill=false;");
            writer.WriteLine("natureskill=false;");
            writer.WriteLine("earthskill=false;");

            if (writer != null)
            {
                writer.Close();
            }
        }

        reader = new StreamReader(skillsFile);
        string skillsText = reader.ReadToEnd();

        //lightning skill
        bool lightningSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "lightningskill=", ';'));
        skillsParent.GetChild(2).GetChild(0).gameObject.SetActive(!lightningSkillUnlocked);

        //fire skill
        bool fireSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "fireskill=", ';'));
        skillsParent.GetChild(2).GetChild(1).gameObject.SetActive(!fireSkillUnlocked);

        //whimsical skill
        bool whimsicalSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "whimsicalskill=", ';'));
        skillsParent.GetChild(2).GetChild(2).gameObject.SetActive(!whimsicalSkillUnlocked);

        //order skill
        bool orderSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "orderskill=", ';'));
        skillsParent.GetChild(2).GetChild(3).gameObject.SetActive(!orderSkillUnlocked);

        //nature skill
        bool natureSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "natureskill=", ';'));
        skillsParent.GetChild(2).GetChild(4).gameObject.SetActive(!natureSkillUnlocked);

        //earth skill
        bool earthSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "earthskill=", ';'));
        skillsParent.GetChild(2).GetChild(5).gameObject.SetActive(!earthSkillUnlocked);

        //chaos skill
        bool chaosSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "chaosskill=", ';'));
        skillsParent.GetChild(2).GetChild(6).gameObject.SetActive(!chaosSkillUnlocked);

        //water skill
        bool waterSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "waterskill=", ';'));
        skillsParent.GetChild(2).GetChild(7).gameObject.SetActive(!waterSkillUnlocked);


        if (reader != null)
        {
            reader.Close();
        }
    }

    //returns the variable contained within a line from a file
    private string GetVariableFromText(string fullText, string id, char lineEnd)
    {
        return fullText.Substring(fullText.IndexOf(id) + id.Length, fullText.Substring(fullText.IndexOf(id)).IndexOf(lineEnd) - id.Length);
    }

    //go to skills
    public void OpenSkills()
    {
        skillsParent.gameObject.SetActive(true);
    }

    //go back to main menu
    public void CloseSkills()
    {
        skillsParent.gameObject.SetActive(false);
    }

    //save options to file
    public void SaveOptionsToFile()
    {
        writer = new StreamWriter(options, false);
        writer.WriteLine("randommap=" + randomizeMap + ";");
        writer.WriteLine("musicvolume=" + musicSource.volume + ";");
        writer.WriteLine("sfxvolume=" + sfxVolume + ";");

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

        if (reader != null)
        {
            reader.Close();
        }
    }

    //toggle random map option
    public void ToggleRandomMap(Toggle toggle)
    {
        randomizeMap = toggle.isOn;
    }

    //change volume based on slider value
    private void ChangeMusicVolume(Slider slider, AudioSource audio, Text label)
    {
        audio.volume = slider.value;

        label.text = string.Format("<color=ff6600>{0}%</color> music volume", Mathf.Floor(slider.value * 100));
    }

    //change all sfx volume based on slider value
    private void ChangeSFXVolume(Slider slider, Text label)
    {
        sfxVolume = slider.value;
        label.text = string.Format("<color=ff6600>{0}%</color> sound effect volume", Mathf.Floor(slider.value * 100));
    }

    //open options
    public void OpenOptions()
    {
        optionsParent.SetActive(true);
    }

    //close options
    public void CloseOptions()
    {
        optionsParent.SetActive(false);
    }

    //open scourge-o-nomicon
    public void OpenEnemyPedia()
    {
        enemypediaParent.SetActive(true);
        enemyPediaActive = true;
    }

    //close scourge-o-nomicon
    public void CloseEnemyPedia()
    {
        enemypediaParent.SetActive(false);
        enemyPediaActive = false;
    }

    //changes wave values of input panel
    public void CalculateEnemyWave(InputField input, Text panel, EnemyClass enemyClass)
    {
        int waveNum;

        if (int.TryParse(input.text, out waveNum))
        {
            int waveNumberMod = waveNum + Mathf.FloorToInt(waveNum / 10f);

            switch (enemyClass)
            {
                case EnemyClass.Cur:
                    panel.text = string.Format("values on wave {0}\nquantity: {1}\n  health: {2}\n  attack: {3}\n   speed: {4}\n      er: {5}", 
                        waveNum, waveNumberMod * 5, waveNumberMod * 2f, waveNumberMod * 1.5f, 1.05f, waveNumberMod * 2f);
                    break;

                case EnemyClass.Knight:
                    panel.text = string.Format("values on wave {0}\nquantity: {1}\n  health: {2}\n  attack: {3}\n   speed: {4}\n      er: {5}\nbulky and packs a punch but rather slow.",
                        waveNum, waveNumberMod * 1, waveNumberMod * 5f, waveNumberMod * 3f, .525f, waveNumberMod * 10f);
                    break;

                case EnemyClass.Monk:
                    panel.text = string.Format("values on wave {0}\nquantity: {1}\n  health: {2}\n  attack: {3}\n   speed: {4}\n      er: {5}\nsmall and fast, there are a lot of them.",
                        waveNum, waveNumberMod * 10, waveNumberMod * .5f, waveNumberMod * .5f, 1.575f, waveNumberMod * 1f);
                    break;

                case EnemyClass.Assassin:
                    panel.text = string.Format("values on wave {0}\nquantity: {1}\n  health: {2}\n  attack: {3}\n   speed: {4}\n      er: {5}\nspeed and attack scale with missing health.",
                        waveNum, waveNumberMod * 3, waveNumberMod * 4f, waveNumberMod * 2f, 1f, waveNumberMod * 3.5f);
                    break;

                case EnemyClass.Paladin:
                    panel.text = string.Format("values on wave {0}\nquantity: {1}\n  health: {2}\n  attack: {3}\n   speed: {4}\n      er: {5}\nafter being hit 3 times the paladin will gain a shield that blocks all damage for 5 seconds and have increased speed.",
                        waveNum, waveNumberMod * 3, waveNumberMod * 3f, waveNumberMod * 1.5f, 1.05f, waveNumberMod * 3.5f);
                    break;

                case EnemyClass.Armorer:
                    panel.text = string.Format("values on wave {0}\nquantity: {1}\n  health: {2}\n  attack: {3}\n   speed: {4}\n      er: {5}\nthe armorer takes less damage the further away it is from the tower targeting it.",
                        waveNum, waveNumberMod * 4, waveNumberMod * 3f, waveNumberMod * 3f, 1.1f, waveNumberMod * 2.5f);
                    break;

                case EnemyClass.Succubus:
                    panel.text = string.Format("values on wave {0}\nquantity: {1}\n  health: {2}\n  attack: {3}\n   speed: {4}\n      er: {5}",
                        waveNum, waveNumberMod * 2, waveNumberMod * 3f, waveNumberMod * 4f, 1.32f, waveNumberMod * 5f);
                    break;

                case EnemyClass.Warlock:
                    panel.text = string.Format("values on wave {0}\nquantity: {1}\n  health: {2}\n  attack: {3}\n   speed: {4}\n      er: {5}\nthe warock doesnt take damage if the attack deals less than 20% of its health.",
                        waveNum, waveNumberMod * 6, waveNumberMod * 2f, waveNumberMod * 1.5f, 1.2f, waveNumberMod * 1.7f);
                    break;
            }
        }
        else
        {
            input.Select();
            input.text = "";
        }
    }

    //read enemypedia from file
    private void ReadEnemypediaFromFile()
    {
        //if the file is missing create it with default values
        if (!File.Exists(enemypediaFile))
        {
            writer = new StreamWriter(enemypediaFile, false);
            writer.WriteLine("cur=" + false + ";");
            writer.WriteLine("knight=" + false + ";");
            writer.WriteLine("monk=" + false + ";");
            writer.WriteLine("assassin=" + false + ";");
            writer.WriteLine("paladin=" + false + ";");
            writer.WriteLine("armorer=" + false + ";");
            writer.WriteLine("succubus=" + false + ";");
            writer.WriteLine("warlock=" + false + ";");

            if (writer != null)
            {
                writer.Close();
            }
        }

        reader = new StreamReader(enemypediaFile);
        string enemyText = reader.ReadToEnd();

        //cur
        bool curPanelActive = bool.Parse(GetVariableFromText(enemyText, "cur=", ';'));
        enemypedia[0].GetChild(4).gameObject.SetActive(!curPanelActive);

        //knight
        bool knightPanelActive = bool.Parse(GetVariableFromText(enemyText, "knight=", ';'));
        enemypedia[1].GetChild(4).gameObject.SetActive(!knightPanelActive);

        //monk
        bool monkPanelActive = bool.Parse(GetVariableFromText(enemyText, "monk=", ';'));
        enemypedia[2].GetChild(4).gameObject.SetActive(!monkPanelActive);

        //assassin
        bool assassinPanelActive = bool.Parse(GetVariableFromText(enemyText, "assassin=", ';'));
        enemypedia[3].GetChild(4).gameObject.SetActive(!assassinPanelActive);

        //paladin
        bool paladinPanelActive = bool.Parse(GetVariableFromText(enemyText, "paladin=", ';'));
        enemypedia[4].GetChild(4).gameObject.SetActive(!paladinPanelActive);

        //armorer
        bool armorerPanelActive = bool.Parse(GetVariableFromText(enemyText, "armorer=", ';'));
        enemypedia[5].GetChild(4).gameObject.SetActive(!armorerPanelActive);

        //succubus
        bool succubusPanelActive = bool.Parse(GetVariableFromText(enemyText, "succubus=", ';'));
        enemypedia[6].GetChild(4).gameObject.SetActive(!succubusPanelActive);

        //warlock
        bool warlockPanelActive = bool.Parse(GetVariableFromText(enemyText, "warlock=", ';'));
        enemypedia[7].GetChild(4).gameObject.SetActive(!warlockPanelActive);

        if (reader != null)
        {
            reader.Close();
        }
    }

    //go to ultimate skills
    public void GoToUltimateSkills()
    {
        skillsParent.GetChild(4).gameObject.SetActive(false);
    }

    //close ultimate skills
    public void CloseUltimateSkills()
    {
        skillsParent.GetChild(4).gameObject.SetActive(true);
    }

    //alter and save skill tree
    private void SaveSkillTree()
    {
        writer = new StreamWriter(skillTreeFile, false);

        foreach (KeyValuePair<string, int> skillCost in skillTreeCosts)
        {
            writer.WriteLine(skillCost.Key + "=" + skillCost.Value + ";");
        }

        foreach (KeyValuePair<string, float> skillValue in skillTreeValues)
        {
            writer.WriteLine(skillValue.Key + "=" + skillValue.Value + ";");
        }

        if (writer != null)
        {
            writer.Close();
        }
    }

    //read skill tree from file
    private void ReadSkillTreeFromFile()
    {
        //if the file doesnt exist create it with default values
        if (!File.Exists(skillTreeFile))
        {
            writer = new StreamWriter(skillTreeFile, false);

            //skill costs
            writer.WriteLine("money1cost=" + 1 + ";");
            writer.WriteLine("money2cost=" + 2 + ";");
            writer.WriteLine("money3cost=" + 3 + ";");
            writer.WriteLine("money4cost=" + 3 + ";");
            writer.WriteLine("base1cost=" + 1 + ";");
            writer.WriteLine("base2cost=" + 2 + ";");
            writer.WriteLine("base3cost=" + 3 + ";");
            writer.WriteLine("enemy1cost=" + 1 + ";");
            writer.WriteLine("enemyknightcost=" + 2 + ";");
            writer.WriteLine("enemymonkcost=" + 2 + ";");
            writer.WriteLine("enemyarmorercost=" + 3 + ";");
            writer.WriteLine("enemywarlockcost=" + 3 + ";");
            writer.WriteLine("enemyassassincost=" + 3 + ";");
            writer.WriteLine("enemypaladincost=" + 3 + ";");
            writer.WriteLine("enemysuccubuscost=" + 3 + ";");
            writer.WriteLine("upgraderangecost=" + 3 + ";");
            writer.WriteLine("upgradeattackspeedcost=" + 3 + ";");
            writer.WriteLine("upgradeattackcost=" + 3 + ";");

            //skill values
            writer.WriteLine("money1value=" + 0.01 + ";");
            writer.WriteLine("money2value=" + 100 + ";");
            writer.WriteLine("money3value=" + 10 + ";");
            writer.WriteLine("money4value=" + 2 + ";");
            writer.WriteLine("base1value=" + 10 + ";");
            writer.WriteLine("base2value=" + 0 + ";");
            writer.WriteLine("base3value=" + 5 + ";");
            writer.WriteLine("enemy1value=" + 0 + ";");
            writer.WriteLine("enemyknightvalue=" + 0 + ";");
            writer.WriteLine("enemymonkvalue=" + 1 + ";");
            writer.WriteLine("enemyarmorervalue=" + 0 + ";");
            writer.WriteLine("enemywarlockvalue=" + .25f + ";");
            writer.WriteLine("enemyassassinvalue=" + 0 + ";");
            writer.WriteLine("enemypaladinvalue=" + 5 + ";");
            writer.WriteLine("enemysuccubusvalue=" + .3f + ";");
            writer.WriteLine("upgraderangevalue=" + 2 + ";");
            writer.WriteLine("upgradeattackspeedvalue=" + 2 + ";");
            writer.WriteLine("upgradeattackvalue=" + 2 + ";");

            if (writer != null)
            {
                writer.Close();
            }
        }

        if (!File.Exists(skillPointFile))
        {
            writer = new StreamWriter(skillPointFile, false);

            writer.WriteLine("skillpoints=" + skillPoints + ";");

            if (writer != null)
            {
                writer.Close();
            }
        }

        //read skill points
        reader = new StreamReader(skillPointFile);
        string skillPointText = reader.ReadToEnd();

        skillPoints = int.Parse(GetVariableFromText(skillPointText, "skillpoints=", ';'));
        skillPointUI.text = "primordial orbs:   " + skillPoints;

        if (reader != null)
        {
            reader.Close();
        }

        //read skill tree
        reader = new StreamReader(skillTreeFile);
        string skillTreeText = reader.ReadToEnd();

        skillTreeCosts.Clear();
        skillTreeValues.Clear();

        //money 1
        skillTreeCosts.Add("money1cost", int.Parse(GetVariableFromText(skillTreeText, "money1cost=", ';')));
        skillTreeValues.Add("money1value", float.Parse(GetVariableFromText(skillTreeText, "money1value=", ';')));
        
        //money 2
        skillTreeCosts.Add("money2cost", int.Parse(GetVariableFromText(skillTreeText, "money2cost=", ';')));
        skillTreeValues.Add("money2value", float.Parse(GetVariableFromText(skillTreeText, "money2value=", ';')));
        
        //money 3
        skillTreeCosts.Add("money3cost", int.Parse(GetVariableFromText(skillTreeText, "money3cost=", ';')));
        skillTreeValues.Add("money3value", float.Parse(GetVariableFromText(skillTreeText, "money3value=", ';')));
        
        //money 4
        skillTreeCosts.Add("money4cost", int.Parse(GetVariableFromText(skillTreeText, "money4cost=", ';')));
        skillTreeValues.Add("money4value", float.Parse(GetVariableFromText(skillTreeText, "money4value=", ';')));
        
        //base 1
        skillTreeCosts.Add("base1cost", int.Parse(GetVariableFromText(skillTreeText, "base1cost=", ';')));
        skillTreeValues.Add("base1value", float.Parse(GetVariableFromText(skillTreeText, "base1value=", ';')));
        
        //base 2
        skillTreeCosts.Add("base2cost", int.Parse(GetVariableFromText(skillTreeText, "base2cost=", ';')));
        skillTreeValues.Add("base2value", float.Parse(GetVariableFromText(skillTreeText, "base2value=", ';')));
        
        //base 3
        skillTreeCosts.Add("base3cost", int.Parse(GetVariableFromText(skillTreeText, "base3cost=", ';')));
        skillTreeValues.Add("base3value", float.Parse(GetVariableFromText(skillTreeText, "base3value=", ';')));
        
        //enemy 1
        skillTreeCosts.Add("enemy1cost", int.Parse(GetVariableFromText(skillTreeText, "enemy1cost=", ';')));
        skillTreeValues.Add("enemy1value", float.Parse(GetVariableFromText(skillTreeText, "enemy1value=", ';')));
        
        //enemy knight
        skillTreeCosts.Add("enemyknightcost", int.Parse(GetVariableFromText(skillTreeText, "enemyknightcost=", ';')));
        skillTreeValues.Add("enemyknightvalue", float.Parse(GetVariableFromText(skillTreeText, "enemyknightvalue=", ';')));
        
        //enemy armorer
        skillTreeCosts.Add("enemyarmorercost", int.Parse(GetVariableFromText(skillTreeText, "enemyarmorercost=", ';')));
        skillTreeValues.Add("enemyarmorervalue", float.Parse(GetVariableFromText(skillTreeText, "enemyarmorervalue=", ';')));

        //enemy warlock
        skillTreeCosts.Add("enemywarlockcost", int.Parse(GetVariableFromText(skillTreeText, "enemywarlockcost=", ';')));
        skillTreeValues.Add("enemywarlockvalue", float.Parse(GetVariableFromText(skillTreeText, "enemywarlockvalue=", ';')));

        //enemy monk
        skillTreeCosts.Add("enemymonkcost", int.Parse(GetVariableFromText(skillTreeText, "enemymonkcost=", ';')));
        skillTreeValues.Add("enemymonkvalue", float.Parse(GetVariableFromText(skillTreeText, "enemymonkvalue=", ';')));

        //enemy assassin
        skillTreeCosts.Add("enemyassassincost", int.Parse(GetVariableFromText(skillTreeText, "enemyassassincost=", ';')));
        skillTreeValues.Add("enemyassassinvalue", float.Parse(GetVariableFromText(skillTreeText, "enemyassassinvalue=", ';')));

        //enemy paladin
        skillTreeCosts.Add("enemypaladincost", int.Parse(GetVariableFromText(skillTreeText, "enemypaladincost=", ';')));
        skillTreeValues.Add("enemypaladinvalue", float.Parse(GetVariableFromText(skillTreeText, "enemypaladinvalue=", ';')));

        //enemy succubus
        skillTreeCosts.Add("enemysuccubuscost", int.Parse(GetVariableFromText(skillTreeText, "enemysuccubuscost=", ';')));
        skillTreeValues.Add("enemysuccubusvalue", float.Parse(GetVariableFromText(skillTreeText, "enemysuccubusvalue=", ';')));

        //upgrade range
        skillTreeCosts.Add("upgraderangecost", int.Parse(GetVariableFromText(skillTreeText, "upgraderangecost=", ';')));
        skillTreeValues.Add("upgraderangevalue", float.Parse(GetVariableFromText(skillTreeText, "upgraderangevalue=", ';')));

        //upgrade attack speed
        skillTreeCosts.Add("upgradeattackspeedcost", int.Parse(GetVariableFromText(skillTreeText, "upgradeattackspeedcost=", ';')));
        skillTreeValues.Add("upgradeattackspeedvalue", float.Parse(GetVariableFromText(skillTreeText, "upgradeattackspeedvalue=", ';')));

        //upgrade attack
        skillTreeCosts.Add("upgradeattackcost", int.Parse(GetVariableFromText(skillTreeText, "upgradeattackcost=", ';')));
        skillTreeValues.Add("upgradeattackvalue", float.Parse(GetVariableFromText(skillTreeText, "upgradeattackvalue=", ';')));

        UpdateSkillTreeText();

        if (reader != null)
        {
            reader.Close();
        }
    }

    public bool BuySkillTree(string skill, int costIncrement)
    {
        if (skillPoints < skillTreeCosts[skill + "cost"] || (skillTreeMax.ContainsKey(skill) && skillTreeValues[skill + "value"] == skillTreeMax[skill]))
        {
            return false;
        }

        skillPoints -= skillTreeCosts[skill + "cost"];
        skillPointUI.text = "primordial orbs:   " + skillPoints;
        skillTreeCosts[skill + "cost"] += costIncrement;
        skillTreeValues[skill + "value"] += skillTreeIncrement[skill];
        UpdateSkillTreeText();

        SaveSkillPoints();
        SaveSkillTree();

        return true;
    }

    //save skill points to file
    private void SaveSkillPoints()
    {
        writer = new StreamWriter(skillPointFile, false);

        writer.WriteLine("skillpoints=" + skillPoints + ";");

        if (writer != null)
        {
            writer.Close();
        }
    }

    //update text boxes of skills
    private void UpdateSkillTreeText()
    {
        //side effects of reading the code below are: dizziness, shortness of breath, vomiting, eye watering, hemorrhaging of the brain

        //money 1
        skillsParent.GetChild(4).GetChild(4).GetChild(0).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the amount of er that is accumulated every second.\n\ncurrent:\n{0}% of max er\n\nnext level:\n{2}% of max er\n\ncost: {1}",
            skillTreeValues["money1value"] * 100, skillTreeCosts["money1cost"], (skillTreeValues["money1value"] + skillTreeIncrement["money1"]) * 100);

        //money 2
        skillsParent.GetChild(4).GetChild(4).GetChild(1).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the amount of max er that you start at.\n\ncurrent:\n{0} increase\n\nnext level:\n{2} increase\n\ncost: {1}",
            skillTreeValues["money2value"], skillTreeCosts["money2cost"], skillTreeValues["money2value"] + skillTreeIncrement["money2"]);

        //money 3
        if (skillTreeValues["money3value"] <= skillTreeMax["money3"])
        {
            skillTreeValues["money3value"] = skillTreeMax["money3"];
            skillsParent.GetChild(4).GetChild(4).GetChild(2).GetChild(0).GetComponent<Text>().text =
                string.Format("decrease the initial er cost of towers\n\ncurrent:\n{0} er\n\nMAX",
                skillTreeValues["money3value"]);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(2).GetChild(0).GetComponent<Text>().text =
                string.Format("decrease the initial er cost of towers\n\ncurrent:\n{0} er\n\nnext level:\n{2} er\n\ncost: {1}",
                skillTreeValues["money3value"], skillTreeCosts["money3cost"], skillTreeValues["money3value"] + skillTreeIncrement["money3"]);
        }

        //money 4
        if (skillTreeValues["money4value"] <= skillTreeMax["money4"])
        {
            skillTreeValues["money4value"] = skillTreeMax["money4"];
            skillsParent.GetChild(4).GetChild(4).GetChild(3).GetChild(0).GetComponent<Text>().text =
            string.Format("decrease amount the er cost of towers will increase by.\n\ncurrent:\n{0}%\n\nMAX",
            (skillTreeValues["money4value"] - 1) * 100);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(3).GetChild(0).GetComponent<Text>().text =
                string.Format("decrease amount the er cost of towers will increase by.\n\ncurrent:\n{0}%\n\nnext level:\n{2}%\n\ncost: {1}",
                (skillTreeValues["money4value"] - 1) * 100, skillTreeCosts["money4cost"], (skillTreeValues["money4value"] + skillTreeIncrement["money4"] - 1) * 100);
        }

        //base 1
        skillsParent.GetChild(4).GetChild(4).GetChild(4).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the initial health of your base\n\ncurrent:\n{0}\n\nnext level:\n{2}\n\ncost: {1}",
            skillTreeValues["base1value"], skillTreeCosts["base1cost"], skillTreeValues["base1value"] + skillTreeIncrement["base1"]);

        //base 2
        skillsParent.GetChild(4).GetChild(4).GetChild(5).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the amount your base will heal every 10 waves\n\ncurrent:\n{0}\n\nnext level:\n{2}\n\ncost: {1}",
            skillTreeValues["base2value"], skillTreeCosts["base2cost"], skillTreeValues["base2value"] + skillTreeIncrement["base2"]);

        //base 3
        skillsParent.GetChild(4).GetChild(4).GetChild(6).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the amount of max health your base gains every 10 waves.\n\ncurrent:\n{0}\n\nnext level:\n{2}\n\ncost: {1}",
            skillTreeValues["base3value"], skillTreeCosts["base3cost"], skillTreeValues["base3value"] + skillTreeIncrement["base3"]);

        //enemy 1
        skillsParent.GetChild(4).GetChild(4).GetChild(7).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the amount of er gained from scourge.\n\ncurrent:\n{0} * wave number\n\nnext level:\n{2} * wave number\n\ncost: {1}",
            skillTreeValues["enemy1value"], skillTreeCosts["enemy1cost"], skillTreeValues["enemy1value"] + skillTreeIncrement["enemy1"]);

        //enemy knight
        skillsParent.GetChild(4).GetChild(4).GetChild(8).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the number of knight waves, knights take more damage.\n\ncurrent:\n{3}% wave chance\n+{0}% damage\n\nnext level:\n10% wave chance\n+{2}% damage\n\ncost: {1}",
            skillTreeValues["enemyknightvalue"] * 100, skillTreeCosts["enemyknightcost"], (skillTreeValues["enemyknightvalue"] + skillTreeIncrement["enemyknight"]) * 100, skillTreeCosts["enemyknightcost"] == 2 ? 5 : 10);

        //enemy armorer
        skillsParent.GetChild(4).GetChild(4).GetChild(9).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the number of armorer waves, armorers take more damage when with half the range of the tower.\n\ncurrent:\n{3}% wave chance\n+{0}% damage\n\nnext level:\n10% wave chance\n+{2}% damage\n\ncost: {1}",
            skillTreeValues["enemyarmorervalue"] * 100, skillTreeCosts["enemyarmorercost"], (skillTreeValues["enemyarmorervalue"] + skillTreeIncrement["enemyarmorer"]) * 100, skillTreeCosts["enemyarmorercost"] == 3 ? 5 : 10);

        //enemy warlock
        if (skillTreeValues["enemywarlockvalue"] <= skillTreeMax["enemywarlock"])
        {
            skillTreeValues["enemywarlockvalue"] = skillTreeMax["enemywarlock"];
            skillsParent.GetChild(4).GetChild(4).GetChild(10).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the number of warlock waves, the warlock's damage threshhold is lower.\n\ncurrent:\n10% wave chance\n{0}% of max health\n\nMAX",
            skillTreeValues["enemywarlockvalue"] * 100);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(10).GetChild(0).GetComponent<Text>().text =
            string.Format("increase the number of warlock waves, the warlock's damage threshhold is lower.\n\ncurrent:\n{3}% wave chance\n{0}% of max health\n\nnext level:\n10% wave chance\n{2}% of max health\n\ncost: {1}",
            skillTreeValues["enemywarlockvalue"] * 100, skillTreeCosts["enemywarlockcost"], (skillTreeValues["enemywarlockvalue"] + skillTreeIncrement["enemywarlock"]) * 100, skillTreeCosts["enemywarlockcost"] == 3 ? 5 : 10);
        }

        //enemy monk
        if (skillTreeValues["enemymonkvalue"] <= skillTreeMax["enemymonk"])
        {
            skillTreeValues["enemymonkvalue"] = skillTreeMax["enemymonk"];
            skillsParent.GetChild(4).GetChild(4).GetChild(11).GetChild(0).GetComponent<Text>().text =
                string.Format("increase the number of monk waves, monk have less speed.\n\ncurrent:\n10% wave chance\n{0}% speed\n\nMAX",
                skillTreeValues["enemymonkvalue"] * 100);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(11).GetChild(0).GetComponent<Text>().text =
                string.Format("increase the number of monk waves, monk have less speed.\n\ncurrent:\n{3}% wave chance\n{0}% speed\n\nnext level:\n10% wave chance\n{2}% speed\n\ncost: {1}",
                skillTreeValues["enemymonkvalue"] * 100, skillTreeCosts["enemymonkcost"], (skillTreeValues["enemymonkvalue"] + skillTreeIncrement["enemymonk"]) * 100, skillTreeCosts["enemymonkcost"] == 2 ? 5 : 10);
        }

        //enemy assassin
        if (skillTreeValues["enemyassassinvalue"] <= skillTreeMax["enemyassassin"])
        {
            skillTreeValues["enemyassassinvalue"] = skillTreeMax["enemyassassin"];
            skillsParent.GetChild(4).GetChild(4).GetChild(12).GetChild(0).GetComponent<Text>().text =
                string.Format("increase the number of assassin waves, the assassin's attack based on missing health is lower.\n\ncurrent:\n10% wave chance\n-{0}% attack\n\nMAX",
                skillTreeValues["enemyassassinvalue"] * 100);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(12).GetChild(0).GetComponent<Text>().text =
                string.Format("increase the number of assassin waves, the assassin's attack based on missing health is lower.\n\ncurrent:\n{3}% wave chance\n{0}% attack\n\nnext level:\n10% wave chance\n{2}% attack\n\ncost: {1}",
                skillTreeValues["enemyassassinvalue"] * 100, skillTreeCosts["enemyassassincost"], (skillTreeValues["enemyassassinvalue"] + skillTreeIncrement["enemyassassin"]) * 100, skillTreeCosts["enemyassassincost"] == 3 ? 5 : 10);
        }

        //enemy paladin
        if (skillTreeValues["enemypaladinvalue"] <= skillTreeMax["enemypaladin"])
        {
            skillTreeValues["enemypaladinvalue"] = skillTreeMax["enemypaladin"];
            skillsParent.GetChild(4).GetChild(4).GetChild(13).GetChild(0).GetComponent<Text>().text =
                string.Format("increase the number of paladin waves, the paladin's shield lasts less time.\n\ncurrent:\n10% wave chance\n{0} seconds\n\nMAX",
                skillTreeValues["enemypaladinvalue"]);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(13).GetChild(0).GetComponent<Text>().text =
                string.Format("increase the number of paladin waves, the paladin's shield lasts less time.\n\ncurrent:\n{3}% wave chance\n{0} seconds\n\nnext level:\n10 % wave chance\n{2} seconds\n\ncost: {1}",
                skillTreeValues["enemypaladinvalue"], skillTreeCosts["enemypaladincost"], skillTreeValues["enemypaladinvalue"] + skillTreeIncrement["enemypaladin"], skillTreeCosts["enemypaladincost"] == 3 ? 5 : 10);
        }

        //enemy succubus
        if (skillTreeValues["enemysuccubusvalue"] <= skillTreeMax["enemysuccubus"])
        {
            skillTreeValues["enemysuccubusvalue"] = skillTreeMax["enemysuccubus"];
            skillsParent.GetChild(4).GetChild(4).GetChild(14).GetChild(0).GetComponent<Text>().text =
                string.Format("increase the number of succubus waves, succubuses heal less.\n\ncurrent:\n10% wave chance\n{0}% of max health\n\nMAX",
                skillTreeValues["enemysuccubusvalue"] * 100);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(14).GetChild(0).GetComponent<Text>().text =
                string.Format("increase the number of succubus waves, succubuses heal less.\n\ncurrent:\n{3}% wave chance\n{0}% of max health\n\nnext level:\n10 % wave chance\n{2}% of max health\n\ncost: {1}",
                skillTreeValues["enemysuccubusvalue"] * 100, skillTreeCosts["enemysuccubuscost"], (skillTreeValues["enemysuccubusvalue"] + skillTreeIncrement["enemysuccubus"]) * 100, skillTreeCosts["enemysuccubuscost"] == 3 ? 5 : 10);
        }

        //upgrade range
        if (skillTreeValues["upgraderangevalue"] <= skillTreeMax["upgraderange"])
        {
            skillTreeValues["upgraderangevalue"] = skillTreeMax["upgraderange"];
            skillsParent.GetChild(4).GetChild(4).GetChild(15).GetChild(0).GetComponent<Text>().text =
                string.Format("decrease amount of er range upgrade cost will increase by.\n\ncurrent:\n{0}%\n\nMAX",
                (skillTreeValues["upgraderangevalue"] - 1) * 100);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(15).GetChild(0).GetComponent<Text>().text =
                string.Format("decrease amount of er range upgrade cost will increase by.\n\ncurrent:\n{0}%\n\nnext level:\n{2}%\n\ncost: {1}",
                (skillTreeValues["upgraderangevalue"] - 1) * 100, skillTreeCosts["upgraderangecost"], (skillTreeValues["upgraderangevalue"] + skillTreeIncrement["upgraderange"] - 1) * 100);
        }

        //upgrade attack speed
        if (skillTreeValues["upgradeattackspeedvalue"] <= skillTreeMax["upgradeattackspeed"])
        {
            skillTreeValues["upgradeattackspeedvalue"] = skillTreeMax["upgradeattackspeed"];
            skillsParent.GetChild(4).GetChild(4).GetChild(16).GetChild(0).GetComponent<Text>().text =
                string.Format("decrease amount of er attack speed upgrade cost will increase by.\n\ncurrent:\n{0}%\n\nMAX",
                (skillTreeValues["upgradeattackspeedvalue"] - 1) * 100);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(16).GetChild(0).GetComponent<Text>().text =
                string.Format("decrease amount of er attack speed upgrade cost will increase by.\n\ncurrent:\n{0}%\n\nnext level:\n{2}%\n\ncost: {1}",
                (skillTreeValues["upgradeattackspeedvalue"] - 1) * 100, skillTreeCosts["upgradeattackspeedcost"], (skillTreeValues["upgradeattackspeedvalue"] + skillTreeIncrement["upgradeattackspeed"] - 1) * 100);
        }

        //upgrade attack
        if (skillTreeValues["upgradeattackvalue"] <= skillTreeMax["upgradeattack"])
        {
            skillTreeValues["upgradeattackvalue"] = skillTreeMax["upgradeattack"];
            skillsParent.GetChild(4).GetChild(4).GetChild(17).GetChild(0).GetComponent<Text>().text =
            string.Format("decrease amount of er attack upgrade cost will increase by.\n\ncurrent:\n{0}%\n\nMAX",
            (skillTreeValues["upgradeattackvalue"] - 1) * 100);
        }
        else
        {
            skillsParent.GetChild(4).GetChild(4).GetChild(17).GetChild(0).GetComponent<Text>().text =
            string.Format("decrease amount of er attack upgrade cost will increase by.\n\ncurrent:\n{0}%\n\nnext level:\n{2}%\n\ncost: {1}",
            (skillTreeValues["upgradeattackvalue"] - 1) * 100, skillTreeCosts["upgradeattackcost"], (skillTreeValues["upgradeattackvalue"] + skillTreeIncrement["upgradeattack"] - 1) * 100);
        }
    }
}
