using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkingLabelScript : MonoBehaviour {

    public int tick;
    public float timeOn;
    public float timeOff;
    public bool onState;
    public Text text;

    void Start() {
        tick = 0;
        onState = true;
        text = GetComponent<Text>();
    }

    void Update() {
        tick++;
        if (onState) {
            text.enabled = true;
            if (tick > timeOn * 50) {
                onState = false;
                tick = 0;
            }
        } else {
            text.enabled = false;
            if (tick > timeOff * 50) {
                onState = true;
                tick = 0;
            }
        }
    }

    private void OnEnable() {
        tick = 0;
    }
}
