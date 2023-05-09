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
    [HideInInspector] public List<Transform> checkPoints = new List<Transform>();

    private GameObject startPiece;
    private GameObject endPiece;

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
        track = Instantiate(tracks[Int16.Parse(EventSystem.current.currentSelectedGameObject.tag)], Vector3.zero, Quaternion.Euler(0, -90, 0));
        startPiece = track.transform.GetChild(0).gameObject;
        endPiece = track.transform.GetChild(track.transform.childCount - 1).gameObject;

        car = Instantiate(carPrefab, new Vector3(startPiece.transform.position.x, startPiece.transform.position.y + 1.1f, startPiece.transform.position.z), Quaternion.Euler(90, 0, 0));

        GetCheckPoints();
    }

    /// <summary>
    /// Finds all race track checkpoints, and adds them to a list
    /// </summary>
    private void GetCheckPoints() {
        for (int i = 1; i < track.transform.childCount - 1; i++) {
            checkPoints.Add(track.transform.GetChild(i).GetChild(0));
        }
    }
}
