using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
    // Object that holds all race track pieces
    [SerializeField] private GameObject[] tracks;
    private GameObject track;
    [HideInInspector]

    private GameObject startPiece;
    private GameObject endPiece;

    private Transform boostCheckpoint;

    [SerializeField] private GameObject carPrefab;
    private GameObject car;

    public UnityEvent onGameStateChange = new UnityEvent();

    [HideInInspector] public bool frozen = true;

    [HideInInspector]
    public enum GameState {
        MENU,
        PLAYING,
        FROZEN
    }

    public GameState currentGameState;

    [HideInInspector]
    public GameState CurrentGameState {
        get { return currentGameState; }
        set {
            if (currentGameState != value) {
                currentGameState = value;
                onGameStateChange.Invoke();
            }
        }
    }

    private void GameStateChange() {
        if (currentGameState == GameState.PLAYING) {
            frozen = false;
        }
    }

    private void Awake() {
        currentGameState = GameState.MENU;
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start() {
        onGameStateChange.AddListener(GameStateChange);
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update() {

    }

    public void ChooseTrack() {
        CurrentGameState = GameState.PLAYING;

        int trackNumber = Int16.Parse(EventSystem.current.currentSelectedGameObject.tag);
        Vector3 spawnPos;
        Vector3 previousSpawnPos = Vector3.zero;

        for (int i = 0; i < 12; i++) {
            if (i == 0) {
                spawnPos = Vector3.zero;
            } else {
                spawnPos = new Vector3(previousSpawnPos.x + 400, previousSpawnPos.y, previousSpawnPos.z);
            }
            track = Instantiate(tracks[trackNumber], spawnPos, Quaternion.Euler(0, -90, 0));
            previousSpawnPos = track.transform.position;
            startPiece = track.transform.GetChild(0).gameObject;
            endPiece = track.transform.GetChild(track.transform.childCount - 1).gameObject;
            boostCheckpoint = startPiece.transform.GetChild(0);

            if (i == 0) {
                car = Instantiate(carPrefab, new Vector3(startPiece.transform.position.x, startPiece.transform.position.y + 3.5f, startPiece.transform.position.z), Quaternion.Euler(90, 0, 0));
            } else {
                Vector3 startSectionPos = track.transform.GetChild(i + 8).position;
                Quaternion startSectionRot = track.transform.GetChild(i + 8).rotation;
                car = Instantiate(carPrefab, new Vector3(startSectionPos.x, startSectionPos.y + 3.5f, startSectionPos.z), startSectionRot);
            }

            GetCheckPoints();
        }
    }

    /// <summary>
    /// Finds all race track checkpoints, and adds them to a list
    /// </summary>
    private void GetCheckPoints() {
        for (int i = 1; i < track.transform.childCount - 1; i++) {
            CarAgent carAgent = car.GetComponent<CarAgent>();
            GameObject checkPoint = track.transform.GetChild(i).GetChild(0).gameObject;
            checkPoint.name = "checkPoint_" + i;
            carAgent.checkPoints.Add(track.transform.GetChild(i).GetChild(0));
            carAgent.boostCheckpoint = boostCheckpoint;
        }
    }
}
