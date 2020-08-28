using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialMusicStart : MonoBehaviour {

    void Start() {
        GameObject.FindGameObjectWithTag("Music").GetComponent<MusicScript>().PlayMusic();
    }

}
