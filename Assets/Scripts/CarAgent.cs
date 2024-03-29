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
    [SerializeField] private Transform leftSensor;
    [SerializeField] private Transform rightSensor;

    [SerializeField] private Transform targetCheckpoint;
    [SerializeField] private Transform previousCheckpoint;
    [HideInInspector] public Transform boostCheckpoint;

    private Vector3 checkPointBackVector;
    private Vector3 startPos;
    private Vector3 initStartPos;

    public List<Transform> checkPoints = new List<Transform>();

    private int checkPointsPassed = 0;

    private float smoothRotationChange = 0f;
    private float startTime = 0f;

    [Tooltip("Speed of agent")]
    private float initSpeed = 100f;
    public float speed = 10f;

    [Tooltip("Maximum agent velocity")]
    private float maxVelocity = 10f;

    [Tooltip("Speed of rotation")]
    [SerializeField] private float rotationSpeed = 100f;

    private void Awake() {
        GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Start() {

        startPos = transform.position;
        initStartPos = transform.position;
        targetCheckpoint = checkPoints[0];
        previousCheckpoint = boostCheckpoint;
        targetCheckpoint.tag = "Target Checkpoint";
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    private int pathLayer = 1 << 6;
    private int wallsLayer = 1 << 8;
    private int checkPointLayer = 1 << 7;
    private float wallsLeftFloat;
    private float wallsRightFloat;
    private void FixedUpdate() {
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);

        startTime += Time.deltaTime;
    }

    private void Update() {
        float velocity = rb.velocity.magnitude;
        float dot = Vector3.Dot((targetCheckpoint.position - transform.position).normalized, transform.forward);
        AddReward(Mathf.Abs(rb.velocity.magnitude) * dot * .1f);

        if (velocity < .1f) {
            AddReward(-0.1f);
        }
    }

    private float currentDistanceToCheckpoint;
    [SerializeField] private bool pastCheckpoint = true;
    private bool wallsLeft;
    private bool wallsRight;
    /// <summary>
    /// Takes and alters values passed in to it by sensor observations.
    /// vectorAction[0] = x rotation
    /// vectorAction[1] = y rotation
    /// vectorAction[2] = z rotation
    /// vectorAction[3] = velocity magnitude (speed)
    /// vectorAction[4] = distance from previous checkpoint
    /// vectorAction[5] = distance to next checkpoint
    /// vectorAction[6] = look direction of car, relative to next checkpoint
    /// </summary>
    /// <param name="actions">Values stored by sensor observations in the overriden 'AddObservations' function</param>
    public override void OnActionReceived(ActionBuffers actions) {

        if (GM.frozen) {
            return;
        }

        ActionSegment<float> vectorAction = actions.ContinuousActions;

        Vector3 rotation = new Vector3(vectorAction[0], 0, vectorAction[2]).normalized;

        speed = vectorAction[3];
        speed = Mathf.Abs(speed);
        float distanceToPreviousCheckpoint = vectorAction[4];
        currentDistanceToCheckpoint = vectorAction[5];
        float lookDP = vectorAction[6];
        float lWallHit = vectorAction[7];
        float rWallHit = vectorAction[8];

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rotation, Vector3.up), 3 * Time.deltaTime);

        rb.AddForce(transform.forward * speed * initSpeed, ForceMode.Acceleration);

        float previousDivideValue = 0;

        if (distanceToPreviousCheckpoint >= 0 && distanceToPreviousCheckpoint < 10) {
            previousDivideValue = 1000;
        } else if (distanceToPreviousCheckpoint >= 10 && distanceToPreviousCheckpoint < 100) {
            previousDivideValue = 10000;
        } else if (distanceToPreviousCheckpoint >= 100 && distanceToPreviousCheckpoint < 1000) {
            previousDivideValue = 100000;
        }
    }

    public override void OnEpisodeBegin() {
        base.OnEpisodeBegin();

        checkPointsPassed = 0;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        //transform.position = startPos;
        transform.position = initStartPos;
        for (int i = 0; i < checkPoints.Count; i++) {
            Collider checkPointCollider = checkPoints[i].GetComponent<Collider>();

            if (!checkPointCollider.enabled) {
                checkPointCollider.enabled = true;
            }
        }
        transform.rotation = Quaternion.identity;
        targetCheckpoint.tag = "Target Checkpoint";
        targetCheckpoint = boostCheckpoint;
        
        previousCheckpoint = boostCheckpoint;
        startTime = 0f;
        rb.AddForce(transform.forward * speed);
    }

    public override void CollectObservations(VectorSensor sensor) {
        if (GM.frozen) {
            return;
        }

        sensor.AddObservation(transform.localRotation.normalized);

        sensor.AddObservation(rb.velocity.magnitude);

        sensor.AddObservation(Vector3.Distance(transform.position, previousCheckpoint.position));

        sensor.AddObservation(Vector3.Distance(transform.position, targetCheckpoint.position));

        Vector3 toCheckPoint = (targetCheckpoint.position - transform.position).normalized;

        sensor.AddObservation(Vector3.Dot((targetCheckpoint.position - transform.position).normalized, transform.forward));

        wallsLeft = Physics.Raycast(leftSensor.transform.position, leftSensor.forward, 10, wallsLayer);
        wallsRight = Physics.Raycast(rightSensor.transform.position, rightSensor.forward, 10, wallsLayer);

        if (wallsLeft) {
            wallsLeftFloat = 1f;
        } else {
            wallsLeftFloat = 0f;
        }

        if (wallsRight) {
            wallsRightFloat = 1f;
        } else {
            wallsRightFloat = 0f;
        }

        sensor.AddObservation(wallsLeftFloat);
        sensor.AddObservation(wallsRightFloat);
    }

    private bool checkPointDisabled;
    private void OnTriggerEnter(Collider other) {
        if (other.transform == targetCheckpoint) {
            AddReward(10);
            startPos = new Vector3(targetCheckpoint.transform.position.x, targetCheckpoint.transform.position.y + 3.5f, targetCheckpoint.transform.position.z);
            targetCheckpoint.tag = "Default";
            previousCheckpoint = targetCheckpoint;
            if (previousCheckpoint.GetComponent<Collider>() != null) {
                previousCheckpoint.GetComponent<Collider>().enabled = false;
            }
            checkPointsPassed++;
            targetCheckpoint = checkPoints[checkPointsPassed];
            previousCheckpoint = checkPoints[checkPointsPassed - 1];
            Debug.Log("Checkpoint Passed");
        }
    }
}
