using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonArchitect;

public class GameManagerScript : MonoBehaviour {

    private GameObject startRoom;
    private GameObject dungeonSnap;
    private Dungeon dungeon;
    public GameObject player;
    public Vector3 startDiff;
    public int seed;

    public int roomsEntered;

    void Start() {
        player = GameObject.Find("Player");
        dungeonSnap = GameObject.Find("DungeonSnap");
        seed = Mathf.RoundToInt(System.DateTime.Now.Millisecond);
        dungeonSnap.GetComponent<DungeonConfig>().Seed = (uint)seed;
        dungeon = dungeonSnap.GetComponent<Dungeon>();

        dungeon.Build();
        startRoom = GameObject.Find("DungeonItems").transform.GetChild(0).gameObject;
        player.transform.position = startRoom.transform.position + startDiff;
    }
}
