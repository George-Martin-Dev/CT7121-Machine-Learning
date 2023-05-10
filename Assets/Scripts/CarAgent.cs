using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent {
    private GameManager GM;

    [SerializeField] private Rigidbody rb;

    [SerializeField] private Transform frontOfCar;

    private Transform targetCheckpoint;

    private Vector3 checkPointBackVector;
    private Vector3 startPos;

    public List<Transform> checkPoints = new List<Transform>();

    private int checkPointsPassed = 0;

    private float smoothRotationChange = 0f;
    private float startTime = 0f;

    [Tooltip("Speed of agent")]
    private float speed = 30f;

    [Tooltip("Maximum agent velocity")]
    private float maxVelocity;

    [Tooltip("Speed of rotation")]
    [SerializeField] private float rotationSpeed = 100f;

    private void Awake() {
        GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Start() {
        startPos = transform.position;
        targetCheckpoint = checkPoints[0];
        targetCheckpoint.tag = "Target Checkpoint";
        checkPointBackVector = targetCheckpoint.transform.right.normalized;
        rb.maxLinearVelocity = speed / 2;
    }

    int pathLayer = 1 << 6;
    int checkPointLayer = 1 << 7;
    private void Update() {
        Vector3.ClampMagnitude(rb.velocity, maxVelocity);

        startTime += Time.deltaTime;

        Vector3 groundCheckRayPos = new Vector3(transform.position.x, transform.position.y - 0.01f, transform.position.z);
        Vector3 checkPointRayPos = new Vector3(transform.position.x, transform.position.y + 0.01f, transform.position.z);

        RaycastHit checkPointHit;

        if (!Physics.Raycast(groundCheckRayPos, -Vector3.up, 3, pathLayer)) {
            Debug.DrawRay(groundCheckRayPos, -Vector3.up * 3, Color.red);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = startPos;
            transform.rotation = Quaternion.identity;
            checkPointsPassed = 0;
            targetCheckpoint = checkPoints[0];
            targetCheckpoint.tag = "Target Checkpoint";
            AddReward(-.05f / startTime);
            startTime = 0f;
        } else {
            Debug.DrawRay(groundCheckRayPos, -Vector3.up * 3, Color.green);
        }

        if (Physics.Raycast(checkPointRayPos, transform.forward, out checkPointHit, 30, checkPointLayer, QueryTriggerInteraction.Collide)) {
            Debug.DrawRay(checkPointRayPos, transform.forward * 30, Color.red);
            if (checkPointHit.transform.CompareTag("Target Checkpoint")) {
                Debug.Log("looked at checkpoint");
                AddReward(.05f);
            }
        } else {
            Debug.DrawRay(checkPointRayPos, transform.forward * 30, Color.green);
        }
    }

    public override void OnActionReceived(ActionBuffers actions) {

        if (GM.frozen) {
            return;
        }

        ActionSegment<float> vectorAction = actions.ContinuousActions;

        Vector3 currentRotation = transform.rotation.eulerAngles;

        float rotation = vectorAction[0];
        float speed = vectorAction[1];

        smoothRotationChange = Mathf.MoveTowards(smoothRotationChange, rotation, 2f * Time.deltaTime);
        float newRotation = currentRotation.y + smoothRotationChange * Time.deltaTime * rotationSpeed;
        transform.rotation = Quaternion.Euler(0, newRotation, 0);

        speed = UnityEngine.Random.Range(10f, 30f);

        rb.AddForce(transform.forward * speed);
    }

    private Vector3 toCheckpoint;
    public override void CollectObservations(VectorSensor sensor) {

        if (GM.frozen) {
            return;
        }

        //if (targetCheckpoint == null) {
        //    sensor.AddObservation(new float[10]);
        //    return;
        //}

        sensor.AddObservation(transform.rotation.eulerAngles.y);

        sensor.AddObservation(rb.velocity.magnitude);
    }

    private void OnTriggerExit(Collider other) {

        if (other.transform == targetCheckpoint) {
            checkPointsPassed++;
            targetCheckpoint.tag = "Default";
            targetCheckpoint = checkPoints[checkPointsPassed];
            checkPointBackVector = targetCheckpoint.transform.right.normalized;
            AddReward(.05f);
            Debug.Log("Good Boi");
        }
    }
}
