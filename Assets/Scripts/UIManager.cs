using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
    [SerializeField] private GameManager GM;

    [SerializeField] private GameObject menuUI;

    private bool listenerAdded = false;

    private void Awake() {

    }

    // Start is called before the first frame update
    void Start() {
        GM.onGameStateChange.AddListener(UpdateUI);
    }

    // Update is called once per frame
    void Update() {

    }

    private void UpdateUI() {
        switch (GM.currentGameState) {
            case GameManager.GameState.PLAYING:
                menuUI.SetActive(false);
                break;
        }
    }
}
