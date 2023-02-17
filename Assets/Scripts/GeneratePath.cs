using UnityEngine;
using System.Collections;

public class GeneratePath : MonoBehaviour {
    private GameObject pathPiece;
    private GameObject endPoint;
    [SerializeField] private GameObject pathPiecePrefab;

    // Start is called before the first frame update
    void Start() {
        pathPiece = Instantiate(pathPiecePrefab, Vector3.zero, Quaternion.identity);
        endPoint = pathPiece.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update() {

    }

    private Vector3 spawnPosition;
    private Vector3 spawnRotation;
    public void Generate() {
        GetSpawnPos(out spawnPosition, out spawnRotation);

        pathPiece = Instantiate(pathPiecePrefab, spawnPosition, Quaternion.Euler(spawnRotation));
    }

    private void GetSpawnPos(out Vector3 spawnPos, out Vector3 spawnRot) {
        endPoint = pathPiece.transform.GetChild(0).gameObject;

        float pieceLength;
        float pieceWidth;

        Vector3 endPointWorldPos = Vector3.zero;

        spawnPos = Vector3.zero;
        spawnRot = Vector3.zero;

        float directionChoice;

        pieceLength = pathPiece.transform.localScale.x;
        pieceWidth = pathPiece.transform.localScale.z;

        directionChoice = (int)Random.Range(0, 3);

        if (directionChoice == 0) {
            spawnRot = Vector3.zero;
            spawnPos = endPoint.transform.position + new Vector3());
        } else if (directionChoice == 1) {
            spawnPos = endPoint.transform.TransformPoint(new Vector3(pieceLength / 2, 0, 0));
            spawnRot = new Vector3(0, 90, 0);
        } else if (directionChoice == 2) {
            spawnPos = endPoint.transform.TransformPoint(new Vector3(-pieceLength / 2, 0, 0));
            spawnRot = new Vector3(0, 90, 0);
        }

        Debug.Log($"direction choice: {directionChoice}");
    }
}
