using UnityEngine;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public class GeneratePath : MonoBehaviour {
    private GameObject pathPiece;
    private Quaternion oldPathPieceRot;
    private GameObject endPoint;
    [SerializeField] private GameObject pathPiecePrefab;
    [SerializeField] private Transform path;

    private char dirChoice;

    // Start is called before the first frame update
    void Start() {
        pathPiece = Instantiate(pathPiecePrefab, Vector3.zero, Quaternion.identity);
        oldPathPieceRot = pathPiece.transform.rotation;
        //pathPiece.transform.parent = path;
        endPoint = pathPiece.transform.GetChild(0).gameObject;
    }

    void Update() {

    }

    private Vector3 spawnPosition;
    private char spawnRotation;
    public void Generate() {
        oldPathPieceRot = pathPiece.transform.rotation;

        SetSpawnPos(out spawnPosition, out spawnRotation);

        pathPiece = Instantiate(pathPiecePrefab, spawnPosition, oldPathPieceRot);

        PieceInfo PI = pathPiece.GetComponent<PieceInfo>();
        PI.direction = dir;

        if (dirChoice != 'F') {
            pathPiece.transform.position += pathPiece.transform.right * (pieceLength / 2);
        }

        //pathPiece.transform.parent = path;
    }

    private float pieceLength;
    private float pieceWidth;
    private int directionChoice;
    private char dir;
    Vector3 endPointPos;
    private RaycastHit hit;
    private Ray ray;
    [SerializeField] private LayerMask roadPieces;
    private void SetSpawnPos(out Vector3 spawnPos, out char spawnRot) {

        endPoint = pathPiece.transform.GetChild(0).gameObject;

        endPointPos = endPoint.transform.position;

        spawnPos = Vector3.zero;
        spawnRot = ' ';

        float newRot;

        pieceLength = pathPiece.transform.localScale.x;
        pieceWidth = pathPiece.transform.localScale.z;

        dir = ChooseDirection();

        switch (dir) {
            case 'F':
                spawnPos = endPointPos += new Vector3(pieceLength / 2, 0, 0);
                spawnRot = 'F';
                break;
            case 'L':
                spawnPos = endPointPos += new Vector3(pieceWidth / 2, 0, 0);
                spawnRot = 'L';
                break;
            case 'R':
                spawnPos = endPointPos += new Vector3(pieceWidth / 2, 0, 0);
                newRot = Mathf.Round(oldPathPieceRot.y + 90);
                spawnRot = 'R';
                break;
        }
    }

    private char ChooseDirection() {
        char finalDirection = ' ';
        Vector3 rayStartPos = endPointPos + pathPiece.transform.forward * 2;

        if (Physics.Raycast(rayStartPos, pathPiece.transform.right, 20f, roadPieces)) {

            directionChoice = (int)UnityEngine.Random.Range(0, 2);

            switch (directionChoice) {
                case 0:
                    Debug.DrawRay(rayStartPos, pathPiece.transform.forward * 20, Color.red);
                    if (!Physics.Raycast(rayStartPos, pathPiece.transform.forward, 20f, roadPieces)) {
                        finalDirection = 'R';
                    } else {
                        finalDirection = 'L';
                    }
                    break;
                case 1:
                    Debug.DrawRay(rayStartPos, -pathPiece.transform.forward * 20, Color.red);
                    if (!Physics.Raycast(rayStartPos, -pathPiece.transform.forward, 20f, roadPieces)) {
                        finalDirection = 'L';
                    } else {
                        finalDirection = 'R';
                    }
                    break;
            }

        } else if (Physics.Raycast(rayStartPos, pathPiece.transform.forward, 20f, roadPieces)) {

            directionChoice = (int)UnityEngine.Random.Range(0, 2);

            switch (directionChoice) {
                case 0:
                    Debug.DrawRay(rayStartPos, pathPiece.transform.right * 20, Color.red);
                    if (!Physics.Raycast(rayStartPos, pathPiece.transform.right, 20f, roadPieces)) {
                        finalDirection = 'F';
                    } else {
                        finalDirection = 'L';
                    }
                    break;
                case 1:
                    Debug.DrawRay(rayStartPos, -pathPiece.transform.forward * 20, Color.red);
                    if (!Physics.Raycast(rayStartPos, -pathPiece.transform.forward, 20f, roadPieces)) {
                        finalDirection = 'L';
                    } else {
                        finalDirection = 'F';
                    }
                    break;
            }
        } else if (Physics.Raycast(rayStartPos, -pathPiece.transform.forward, 20f, roadPieces)) {

            directionChoice = (int)UnityEngine.Random.Range(0, 2);

            switch (directionChoice) {
                case 0:
                    Debug.DrawRay(rayStartPos, pathPiece.transform.right * 20, Color.red);
                    if (!Physics.Raycast(rayStartPos, pathPiece.transform.right, 20f, roadPieces)) {
                        finalDirection = 'F';
                    } else {
                        finalDirection = 'R';
                    }
                    break;
                case 1:
                    Debug.DrawRay(rayStartPos, pathPiece.transform.forward * 20, Color.red);
                    if (!Physics.Raycast(rayStartPos, pathPiece.transform.forward, 20f, roadPieces)) {
                        finalDirection = 'R';
                    } else {
                        finalDirection = 'F';
                    }
                    break;
            }
        } else {
            directionChoice = (int)UnityEngine.Random.Range(0, 3);

            switch (directionChoice) {
                case 0:
                    finalDirection = 'F';
                    break;
                case 1:
                    finalDirection = 'L';
                    break;
                case 2:
                    finalDirection = 'R';
                    break;
            }
        }

        return finalDirection;
    }
}
