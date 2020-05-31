using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HUD : MonoBehaviour
{
    Text lifeText;
    Text coolDownText;
    PlayerController player;

    void Start()
    {
        lifeText = GameObject.FindWithTag("Life Text").GetComponent<Text>();
        coolDownText = GameObject.FindWithTag("CoolDown Text").GetComponent<Text>();

        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        HUDHandler();
    }

    void HUDHandler()
    {
        lifeText.text = "Life: " + player.health.ToString();
        coolDownText.text = "CoolDown: " + player.coolDownCounter.ToString();
    }
}
