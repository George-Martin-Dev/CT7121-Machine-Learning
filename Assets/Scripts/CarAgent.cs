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
    private Transform previousCheckpoint;
    [HideInInspector] public Transform boostCheckpoint;

    private Vector3 checkPointBackVector;
    private Vector3 startPos;

    public List<Transform> checkPoints = new List<Transform>();

    private int checkPointsPassed = 0;

    private float smoothRotationChange = 0f;
    private float startTime = 0f;

    [Tooltip("Speed of agent")]
    private float initSpeed = 100f;
    [SerializeField] private float speed = 30f;

    [Tooltip("Maximum agent velocity")]
    private float maxVelocity = 30f;

    [Tooltip("Speed of rotation")]
    [SerializeField] private float rotationSpeed = 100f;

    private void Awake() {
        GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Start() {
        startPos = transform.position;
        targetCheckpoint = checkPoints[0];
        previousCheckpoint = boostCheckpoint;
        targetCheckpoint.tag = "Target Checkpoint";
        checkPointBackVector = targetCheckpoint.transform.forward;
        rb.AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    int pathLayer = 1 << 6;
    int checkPointLayer = 1 << 7;
    private void Update() {
        //Vector3 lookRot = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //if (lookRot != Vector3.zero) {
        //    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookRot, Vector3.up), 3 * Time.deltaTime);
        //}

        startTime += Time.deltaTime;

        Vector3 groundCheckRayPos = new Vector3(transform.position.x, transform.position.y - 0.01f, transform.position.z);
        Vector3 checkPointRayPos = new Vector3(transform.position.x, transform.position.y + 0.01f, transform.position.z);

        RaycastHit checkPointHit;

        if (!Physics.Raycast(groundCheckRayPos, -Vector3.up, 3, pathLayer)) {
            Debug.DrawRay(groundCheckRayPos, -Vector3.up * 3, Color.red);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            speed = UnityEngine.Random.Range(10f, 30f);
            transform.position = startPos;
            transform.rotation = Quaternion.identity;
            checkPointsPassed = 0;
            targetCheckpoint = checkPoints[0];
            targetCheckpoint.tag = "Target Checkpoint";
            AddReward((-.25f / startTime));
            startTime = 0f;
            rb.AddForce(transform.forward * speed);
        } else {
            Debug.DrawRay(groundCheckRayPos, -Vector3.up * 3, Color.green);
        }

        //if (Physics.Raycast(checkPointRayPos, transform.forward, out checkPointHit, 30, checkPointLayer, QueryTriggerInteraction.Collide)) {
        //    Debug.DrawRay(checkPointRayPos, transform.forward * 30, Color.red);
        //    if (checkPointHit.transform.CompareTag("Target Checkpoint")) {
        //        Debug.Log("looked at checkpoint");
        //        AddReward(.05f);
        //    }
        //} else {
        //    Debug.DrawRay(checkPointRayPos, transform.forward * 30, Color.green);
        //}
    }

    private float currentDistanceToCheckpoint;
    float targetDivideValue;
    Vector3 leftRayStart;
    Vector3 leftRayEnd;
    Vector3 rightRayStart;
    Vector3 rightRayEnd;
    public override void OnActionReceived(ActionBuffers actions) {

        if (GM.frozen) {
            return;
        }

        ActionSegment<float> vectorAction = actions.ContinuousActions;



        Vector3 rotation = new Vector3(vectorAction[0], 0, vectorAction[2]).normalized;
        //Vector3 rotation = new Vector3(0, vectorAction[1], 0).normalized;

        //Debug.Log("x rot: " + transform.localRotation.x + " ###### x rot stupid: " + vectorAction[0] +
        //          "\ny rot: " + transform.localRotation.y + " ###### y rot stupid: " + vectorAction[1] +
        //          "\nz rot: " + transform.localRotation.z + " ###### z rot stupid: " + vectorAction[2]);

        speed = vectorAction[3];
        float distanceToPreviousCheckpoint = vectorAction[4];
        currentDistanceToCheckpoint = vectorAction[5];
        float lookDP = vectorAction[6];

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rotation, Vector3.up), 3 * Time.deltaTime);

        //rb.AddForce(movement * speed * initSpeed, ForceMode.Acceleration);
        rb.AddForce(transform.forward * speed * initSpeed, ForceMode.Acceleration);

        leftRayStart = new Vector3(transform.position.x - .5f, transform.position.y - .005f, transform.position.z);
        leftRayEnd = new Vector3(-2, -.8f, 2);
        rightRayStart = new Vector3(transform.position.x + .5f, transform.position.y - .005f, transform.position.z);
        rightRayEnd = new Vector3(2, -.8f, 2);
        //Vector3 backwardRayStart = new Vector3(transform.position.x, transform.position.y - .005f, transform.position.z - 1);
        //Vector3 backwardRayEnd = new Vector3(0, -.25f, -3);

        Vector3 forwardRayStart = frontOfCar.transform.position /*+ new Vector3(transform.position.x, transform.position.y + .3f, transform.position.z + 1.3f)*/;
        Vector3 forwardRayEnd = /*transform.forward + new Vector3(0, -.3f, 3)*/ new Vector3(transform.forward.x * 3, transform.forward.y - .3f, transform.forward.z * 3);

        //Debug.DrawRay(backwardRayStart, backwardRayEnd * 3, Color.red);

        bool groundLeft = Physics.Raycast(leftSensor.transform.position, leftSensor.forward, 3, pathLayer);
        bool groundRight = Physics.Raycast(rightSensor.transform.position, rightSensor.forward, 3, pathLayer);
        //bool groundBackward = Physics.Raycast(backwardRayStart, backwardRayEnd, 3);

        //bool groundForward = Physics.Raycast(forwardRayStart, forwardRayEnd, 3);

        //if (!groundForward) {
        //    Debug.DrawRay(forwardRayStart, forwardRayEnd * 3, Color.green);
        //    if (rb.velocity.magnitude > 10) {
        //        AddReward(-.01f);
        //    }
        //} else {
        //    Debug.DrawRay(forwardRayStart, forwardRayEnd * 3, Color.red);
        //}

        //float newDistance = Vector3.Distance(transform.position, targetCheckpoint.position);

        if (lookDP > 0.9) {
            AddReward(.02f);
        }

        if (!groundLeft) {
            Debug.DrawRay(leftSensor.transform.position, leftSensor.forward * 3, Color.red);
            AddReward(-.025f);
        } else {
            Debug.DrawRay(leftSensor.transform.position, leftSensor.forward * 3, Color.green);
        }

        if (!groundRight) {
            Debug.DrawRay(leftSensor.transform.position, leftSensor.forward * 3, Color.red);
            AddReward(-.025f);
        } else {
            Debug.DrawRay(leftSensor.transform.position, leftSensor.forward * 3, Color.green);
        }

        targetDivideValue = 0;

        if (currentDistanceToCheckpoint >= 0 && currentDistanceToCheckpoint < 10) {
            targetDivideValue = 100000;
        } else if (currentDistanceToCheckpoint >= 10 && currentDistanceToCheckpoint < 100) {
            targetDivideValue = 1000000;
        } else if (currentDistanceToCheckpoint >= 100 && currentDistanceToCheckpoint < 1000) {
            targetDivideValue = 10000000;
        }

        float previousDivideValue = 0;

        if (distanceToPreviousCheckpoint >= 0 && distanceToPreviousCheckpoint < 10) {
            previousDivideValue = 10000;
        } else if (distanceToPreviousCheckpoint >= 10 && distanceToPreviousCheckpoint < 100) {
            previousDivideValue = 100000;
        } else if (distanceToPreviousCheckpoint >= 100 && distanceToPreviousCheckpoint < 1000) {
            previousDivideValue = 1000000;
        }

        if (speed < 0) {
            AddReward(-.02f);
        }

        float distanceReward = distanceToPreviousCheckpoint / previousDivideValue;

        if (distanceReward > 0) {
            AddReward(distanceReward);
        }
        //AddReward(-currentDistanceToCheckpoint / targetDivideValue);

        float lookingToCheckpoint = vectorAction[5];

        //if ((lookingToCheckpoint > .9 && lookingToCheckpoint <= 1) || (lookingToCheckpoint < 1.1 && lookingToCheckpoint >= 1)) {
        //    AddReward(0.01f);
        //}

        //if (newDistance < currentDistance) {
        //    AddReward(.05f);
        //}
    }

    public override void CollectObservations(VectorSensor sensor) {

        if (GM.frozen) {
            return;
        }

        //if (targetCheckpoint == null) {
        //    sensor.AddObservation(new float[10]);
        //    return;
        //}

        sensor.AddObservation(transform.localRotation.normalized);

        sensor.AddObservation(rb.velocity.magnitude);

        sensor.AddObservation(Vector3.Distance(transform.position, previousCheckpoint.position));

        sensor.AddObservation(Vector3.Distance(transform.position, targetCheckpoint.position));

        Vector3 toCheckPoint = (targetCheckpoint.position - transform.position).normalized;

        sensor.AddObservation(Vector3.Dot(transform.forward, toCheckPoint));

        sensor.AddObservation(toCheckPoint);
    }

    private void OnTriggerEnter(Collider other) {

        if (other.transform == targetCheckpoint) {
            checkPointsPassed++;
            targetCheckpoint.tag = "Default";
            previousCheckpoint = targetCheckpoint;
            targetCheckpoint = checkPoints[checkPointsPassed];
            checkPointBackVector = targetCheckpoint.transform.forward;
            Debug.Log("Good Boi");
        }
    }

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawRay(leftSensor.transform.position, leftSensor.forward * 3);
    //    Gizmos.DrawRay(rightSensor.transform.position, rightSensor.forward * 3);
    //}
}
