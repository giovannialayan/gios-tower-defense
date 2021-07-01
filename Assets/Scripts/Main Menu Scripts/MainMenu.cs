using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    //tutorial
    public Transform tutorialParent;
    private bool tutorialOn = false;
    private int tutorialPage;

    // Start is called before the first frame update
    void Start()
    {
        
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
    }

    //starts normal mode
    public void StartNormalMode()
    {
        //SceneManager.LoadScene("Normal_Level_1");
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
}
