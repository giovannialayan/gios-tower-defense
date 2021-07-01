using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseAttack : MonoBehaviour
{
    //total damage of pulse
    private float pulseDamage;

    //reference to parent tower script
    private TowerAI tower;

    //timer for pulse alpha
    private float pulseFadeTimer;

    //spriterenderer reference
    private SpriteRenderer pulseImage;

    // Start is called before the first frame update
    void Start()
    {
        pulseImage = gameObject.GetComponent<SpriteRenderer>();
        tower = transform.parent.gameObject.GetComponent<TowerAI>();
        pulseFadeTimer = .5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (pulseFadeTimer > 0)
        {
            Color c = pulseImage.color;
            c.a = pulseFadeTimer * 2;
            pulseImage.color = c;
            pulseFadeTimer -= Time.deltaTime;

            if (pulseFadeTimer <= 0)
            {
                pulseImage.color = new Color(1, 1, 1, 1);
                gameObject.SetActive(false);
            }
        }
    }

    //start the pulse attack
    public void StartPulse()
    {
        pulseFadeTimer = .5f;
    }
}
