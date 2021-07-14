using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    private Dictionary<string, bool> challengeDict;

    //save file
    private string skillsFile = "saves\\skills.txt";
    private StreamWriter writer = null;
    private StreamReader reader = null;

    //challenge ui
    private Dictionary<string, Transform> challengeImages; // 0 = order, 1 = lightning, 2 = whimsical, 3 = nature, 4 = earth, 5 = chaos, 6 = fire, 7 = water
    private string[] challengeElementNames = { "orderskill", "lightningskill", "whimsicalskill", "natureskill", "earthskill", "chaosskill", "fireskill", "waterskill" };
    private Queue<Transform> challengeImagesInUse;
    private float challengeUITimer;
    private bool moveDown;
    private float targetY = 70f;

    //external challenge counters to be tracked in here
    public int chaosChallengeCounter { get; set; }
    public List<EnemyAI> earthChallengeCounter { get; set; }

    //enemypedia found
    private string enemypediaFile = "saves\\enemypedia.txt";
    public Transform enemypediaFoundParent;
    private Dictionary<string, bool> enemypediaDict;
    private Dictionary<string, Transform> enemypediaImages; // 0 = cur, 1 = knight, 2 = monk, 3 = assassin, 4 = paladin, 5 = armorer, 6 = succubus, 7 = warlock
    private string[] enemypediaEnemyNames = { "cur", "knight", "monk", "assassin", "paladin", "armorer", "succubus", "warlock" };

    private void Awake()
    {
        //challenge variables
        challengeDict = new Dictionary<string, bool>();

        ReadSkillsFromFile();

        challengeImages = new Dictionary<string, Transform>();
        challengeImagesInUse = new Queue<Transform>();
        moveDown = false;

        for (int i = 0; i < 8; i++)
        {
            challengeImages.Add(challengeElementNames[i], transform.GetChild(i).GetComponent<Transform>());
        }

        //enemypedia variables
        enemypediaDict = new Dictionary<string, bool>();
        enemypediaImages = new Dictionary<string, Transform>();

        ReadEnemypediaFromFile();

        for (int i = 0; i < 8; i++)
        {
            enemypediaImages.Add(enemypediaEnemyNames[i], enemypediaFoundParent.GetChild(i).GetComponent<Transform>());
        }

        //challenge counters
        chaosChallengeCounter = 0;
        earthChallengeCounter = new List<EnemyAI>();
    }

    private void Update()
    {
        //move challenge get signifiers onto the screen and off the screen one at a time
        if (challengeImagesInUse.Count > 0)
        {
            //use a queue for fifo
            if (!moveDown)
            {
                //move first element up until it reaches the target y
                challengeImagesInUse.Peek().localPosition = new Vector3(0, challengeImagesInUse.Peek().localPosition.y + targetY * Time.deltaTime, 0);

                if (Vector3.Distance(challengeImagesInUse.Peek().localPosition, new Vector3(0, targetY, 0)) < .5f)
                {
                    moveDown = true;
                }
            }
            //have it stay there for 2 seconds
            else if (challengeUITimer <= challengeImagesInUse.Count * 4 - 3)
            {
                //move down until its off screen then remove it from the queue so the next ui element can go
                challengeImagesInUse.Peek().localPosition = new Vector3(0, challengeImagesInUse.Peek().localPosition.y - targetY * Time.deltaTime, 0);

                if (challengeUITimer <= challengeImagesInUse.Count * 4 - 4)
                {
                    moveDown = false;

                    challengeImagesInUse.Dequeue().localPosition = Vector3.zero;
                }
            }

            challengeUITimer -= Time.deltaTime;
        }

        //order challenge test
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SaveChallengeState("orderskill");
        }

        //lightning challenge test
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestChallengeUI("lightningskill");
        }

        //whimsical challenge test
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestChallengeUI("whimsicalskill");
        }

        //nature challenge test
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestChallengeUI("natureskill");
        }

        //earth challenge test
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestChallengeUI("earthskill");
        }

        //chaos challenge test
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TestChallengeUI("chaosskill");
        }

        //fire challenge test
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TestChallengeUI("fireskill");
        }

        //water challenge test
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            TestChallengeUI("waterskill");
        }
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
        if (challengeDict.ContainsKey("lightningskill"))
        {
            challengeDict["lightningskill"] = lightningSkillUnlocked;
        }
        else
        {
            challengeDict.Add("lightningskill", lightningSkillUnlocked);
        }

        //fire skill
        bool fireSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "fireskill=", ';'));
        if (challengeDict.ContainsKey("fireskill"))
        {
            challengeDict["fireskill"] = fireSkillUnlocked;
        }
        else
        {
            challengeDict.Add("fireskill", fireSkillUnlocked);
        }

        //whimsical skill
        bool whimsicalSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "whimsicalskill=", ';'));
        if (challengeDict.ContainsKey("whimsicalskill"))
        {
            challengeDict["whimsicalskill"] = whimsicalSkillUnlocked;
        }
        else
        {
            challengeDict.Add("whimsicalskill", whimsicalSkillUnlocked);
        }

        //order skill
        bool orderSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "orderskill=", ';'));
        if (challengeDict.ContainsKey("orderskill"))
        {
            challengeDict["orderskill"] = orderSkillUnlocked;
        }
        else
        {
            challengeDict.Add("orderskill", orderSkillUnlocked);
        }

        //nature skill
        bool natureSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "natureskill=", ';'));
        if (challengeDict.ContainsKey("natureskill"))
        {
            challengeDict["natureskill"] = natureSkillUnlocked;
        }
        else
        {
            challengeDict.Add("natureskill", natureSkillUnlocked);
        }

        //earth skill
        bool earthSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "earthskill=", ';'));
        if (challengeDict.ContainsKey("earthskill"))
        {
            challengeDict["earthskill"] = earthSkillUnlocked;
        }
        else
        {
            challengeDict.Add("earthskill", earthSkillUnlocked);
        }

        //chaos skill
        bool chaosSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "chaosskill=", ';'));
        if (challengeDict.ContainsKey("chaosskill"))
        {
            challengeDict["chaosskill"] = chaosSkillUnlocked;
        }
        else
        {
            challengeDict.Add("chaosskill", chaosSkillUnlocked);
        }

        //water skill
        bool waterSkillUnlocked = bool.Parse(GetVariableFromText(skillsText, "waterskill=", ';'));
        if (challengeDict.ContainsKey("waterskill"))
        {
            challengeDict["waterskill"] = waterSkillUnlocked;
        }
        else
        {
            challengeDict.Add("waterskill", waterSkillUnlocked);
        }


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

    //save challenge state to file and update the dictionary
    public void SaveChallengeState(string challengeName)
    {
        if (challengeDict[challengeName])
        {
            return;
        }

        challengeUITimer += 4;
        challengeImagesInUse.Enqueue(challengeImages[challengeName]);

        challengeDict[challengeName] = true;

        writer = new StreamWriter(skillsFile, false);

        foreach (KeyValuePair<string, bool> entry in challengeDict)
        {
            writer.WriteLine(entry.Key + "=" + entry.Value + ";");
        }

        if (writer != null)
        {
            writer.Close();
        }
    }

    private void TestChallengeUI(string challengeName)
    {
        if (challengeImagesInUse.Contains(challengeImages[challengeName]))
        {
            return;
        }

        challengeUITimer += 4;
        challengeImagesInUse.Enqueue(challengeImages[challengeName]);
    }

    //property to set external challenge trackers
    public Dictionary<string, bool> ChallengeDictionary
    {
        get { return challengeDict; }
    }

    public void SaveEnemypediaToFile(string enemy)
    {
        if (enemypediaDict[enemy])
        {
            return;
        }

        challengeUITimer += 4;
        challengeImagesInUse.Enqueue(enemypediaImages[enemy]);

        enemypediaDict[enemy] = true;

        writer = new StreamWriter(enemypediaFile, false);

        foreach (KeyValuePair<string, bool> entry in enemypediaDict)
        {
            writer.WriteLine(entry.Key + "=" + entry.Value + ";");
        }

        if (writer != null)
        {
            writer.Close();
        }
    }

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
        if (enemypediaDict.ContainsKey("cur"))
        {
            enemypediaDict["cur"] = curPanelActive;
        }
        else
        {
            enemypediaDict.Add("cur", curPanelActive);
        }

        //knight
        bool knightPanelActive = bool.Parse(GetVariableFromText(enemyText, "knight=", ';'));
        if (enemypediaDict.ContainsKey("knight"))
        {
            enemypediaDict["knight"] = knightPanelActive;
        }
        else
        {
            enemypediaDict.Add("knight", knightPanelActive);
        }

        //monk
        bool monkPanelActive = bool.Parse(GetVariableFromText(enemyText, "monk=", ';'));
        if (enemypediaDict.ContainsKey("monk"))
        {
            enemypediaDict["monk"] = monkPanelActive;
        }
        else
        {
            enemypediaDict.Add("monk", monkPanelActive);
        }

        //assassin
        bool assassinPanelActive = bool.Parse(GetVariableFromText(enemyText, "assassin=", ';'));
        if (enemypediaDict.ContainsKey("assassin"))
        {
            enemypediaDict["assassin"] = assassinPanelActive;
        }
        else
        {
            enemypediaDict.Add("assassin", assassinPanelActive);
        }

        //paladin
        bool paladinPanelActive = bool.Parse(GetVariableFromText(enemyText, "paladin=", ';'));
        if (enemypediaDict.ContainsKey("paladin"))
        {
            enemypediaDict["paladin"] = paladinPanelActive;
        }
        else
        {
            enemypediaDict.Add("paladin", paladinPanelActive);
        }

        //armorer
        bool armorerPanelActive = bool.Parse(GetVariableFromText(enemyText, "armorer=", ';'));
        if (enemypediaDict.ContainsKey("armorer"))
        {
            enemypediaDict["armorer"] = armorerPanelActive;
        }
        else
        {
            enemypediaDict.Add("armorer", armorerPanelActive);
        }

        //succubus
        bool succubusPanelActive = bool.Parse(GetVariableFromText(enemyText, "succubus=", ';'));
        if (enemypediaDict.ContainsKey("succubus"))
        {
            enemypediaDict["succubus"] = succubusPanelActive;
        }
        else
        {
            enemypediaDict.Add("succubus", succubusPanelActive);
        }

        //warlock
        bool warlockPanelActive = bool.Parse(GetVariableFromText(enemyText, "warlock=", ';'));
        if (enemypediaDict.ContainsKey("warlock"))
        {
            enemypediaDict["warlock"] = warlockPanelActive;
        }
        else
        {
            enemypediaDict.Add("warlock", warlockPanelActive);
        }

        if (reader != null)
        {
            reader.Close();
        }
    }

    public Dictionary<string, bool> Enemypedia
    {
        get { return enemypediaDict; }
    }

    //USE THIS TRIGGER ACHIEVEMENTS INSTEAD, IT IS WAY BETTER BUT WAY MORE COMPLICATED
    //https://stackoverflow.com/questions/42034245/unity-eventmanager-with-delegate-instead-of-unityevent
}
