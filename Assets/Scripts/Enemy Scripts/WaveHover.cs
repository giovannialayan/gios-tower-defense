using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WaveHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject waveUI;
    private Wave waveInfo;
    private Text waveUIText;

    // Start is called before the first frame update
    void Start()
    {
        waveUI = transform.GetChild(1).gameObject;
        waveUIText = transform.GetChild(1).GetChild(0).GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        waveUI.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        waveUI.SetActive(false);
    }

    public void SetWaveInfo(Wave info)
    {
        waveUI = transform.GetChild(1).gameObject;
        waveUIText = transform.GetChild(1).GetChild(0).GetComponent<Text>();

        waveInfo = info;
        waveUIText.text = waveInfo.GetWaveInfo();
    }
}
