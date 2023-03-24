using UnityEngine;
using System.Collections;

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
    private int spawnRotation;
    public void Generate() {
        GetSpawnPos(out spawnPosition, out spawnRotation, out dirChoice);

        pathPiece = Instantiate(pathPiecePrefab, spawnPosition, oldPathPieceRot);
        pathPiece.transform.Rotate(Vector3.up, spawnRotation);

        if (dirChoice != 'F') {
            pathPiece.transform.position += pathPiece.transform.right * (pieceLength / 2);
        }

        //pathPiece.transform.parent = path;
    }

    private float pieceLength;
    private float pieceWidth;
    private RaycastHit hit;
    private Ray ray;
    [SerializeField] private LayerMask roadPieces;
    private void GetSpawnPos(out Vector3 spawnPos, out int spawnRot, out char finalDirection) {
        finalDirection = ' ';

        endPoint = pathPiece.transform.GetChild(0).gameObject;

        oldPathPieceRot = pathPiece.transform.rotation;

        int directionChoice;

        Vector3 endPointPos = endPoint.transform.position;

        spawnPos = Vector3.zero;
        spawnRot = 0;

        pieceLength = pathPiece.transform.localScale.x;
        pieceWidth = pathPiece.transform.localScale.z;

        if (Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.right, 20f, roadPieces)) {

            directionChoice = (int)Random.Range(0, 2);

            switch (directionChoice) {
                case 0:
                    Debug.DrawRay(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.forward * 20, Color.red);
                    if (!Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.forward, 20f, roadPieces)) {
                        finalDirection = 'R';
                    } else {
                        finalDirection = 'L';
                    }
                    break;
                case 1:
                    Debug.DrawRay(endPointPos + pathPiece.transform.forward * 2, -pathPiece.transform.forward * 20, Color.red);
                    if (!Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, -pathPiece.transform.forward, 20f, roadPieces)) {
                        finalDirection = 'L';
                    } else {
                        finalDirection = 'R';
                    }
                    break;
            }

        } else if (Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.forward, 20f, roadPieces)) {

            directionChoice = (int)Random.Range(0, 2);

            switch (directionChoice) {
                case 0:
                    Debug.DrawRay(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.right * 20, Color.red);
                    if (!Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.right, 20f, roadPieces)) {
                        finalDirection = 'F';
                    } else {
                        finalDirection = 'L';
                    }
                    break;
                case 1:
                    Debug.DrawRay(endPointPos + pathPiece.transform.forward * 2, -pathPiece.transform.forward * 20, Color.red);
                    if (!Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, -pathPiece.transform.forward, 20f, roadPieces)) {
                        finalDirection = 'L';
                    } else {
                        finalDirection = 'F';
                    }
                    break;
            }
        } else if (Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, -pathPiece.transform.forward, 20f, roadPieces)) {

            directionChoice = (int)Random.Range(0, 2);

            switch (directionChoice) {
                case 0:
                    Debug.DrawRay(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.right * 20, Color.red);
                    if (!Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.right, 20f, roadPieces)) {
                        finalDirection = 'F';
                    } else {
                        finalDirection = 'R';
                    }
                    break;
                case 1:
                    Debug.DrawRay(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.forward * 20, Color.red);
                    if (!Physics.Raycast(endPointPos + pathPiece.transform.forward * 2, pathPiece.transform.forward, 20f, roadPieces)) {
                        finalDirection = 'R';
                    } else {
                        finalDirection = 'F';
                    }
                    break;
            }
        } else {
            directionChoice = (int)Random.Range(0, 3);

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

        //directionChoice = (int)Random.Range(0, 3);

        //if (directionChoice == 0) {
        //    finalDirection = 'F';
        //} else if (directionChoice == 1) {
        //    finalDirection = 'R';
        //} else if (directionChoice == 2) {
        //    finalDirection = 'L';
        //}

        switch (finalDirection) {
            case 'F':
                spawnPos = endPointPos += new Vector3(pieceLength / 2, 0, 0);
                spawnRot = (int)oldPathPieceRot.y;
                break;
            case 'L':
                spawnPos = endPointPos += new Vector3(pieceWidth / 2, 0, 0);
                spawnRot = -90;
                break;
            case 'R':
                spawnPos = endPointPos += new Vector3(pieceWidth / 2, 0, 0);
                spawnRot = 90;
                break;
        }

        Debug.Log($"direction choice: {finalDirection}");
    }
}
