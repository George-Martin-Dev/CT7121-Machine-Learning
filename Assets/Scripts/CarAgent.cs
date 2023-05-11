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
    private float maxVelocity = 30f;

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
        rb.AddForce(transform.forward * speed);
    }

    int pathLayer = 1 << 6;
    int checkPointLayer = 1 << 7;
    private void Update() {
        transform.rotation = Quaternion.LookRotation(new Vector3(rb.velocity.x, 0, rb.velocity.z), Vector3.up);

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
            AddReward(-.05f / startTime - rb.velocity.magnitude);
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

    public override void OnActionReceived(ActionBuffers actions) {

        if (GM.frozen) {
            return;
        }

        ActionSegment<float> vectorAction = actions.ContinuousActions;

        Vector3 movement = new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]);

        speed = vectorAction[3];

        float currentDistance = Vector3.Distance(frontOfCar.transform.position, targetCheckpoint.position);

        rb.AddForce(movement * speed, ForceMode.Acceleration);

        if (speed > 5) {
            AddReward(0.02f);
        }

        //Vector3 leftRayStart = new Vector3(transform.position.x - .5f, transform.position.y - .005f, transform.position.z);
        //Vector3 leftRayEnd = new Vector3(-2, -.25f, 0);
        //Vector3 rightRayStart = new Vector3(transform.position.x + .5f, transform.position.y - .005f, transform.position.z);
        //Vector3 rightRayEnd = new Vector3(2, -.25f, 0);
        //Vector3 backwardRayStart = new Vector3(transform.position.x, transform.position.y - .005f, transform.position.z - 1);
        //Vector3 backwardRayEnd = new Vector3(0, -.25f, -3);

        Vector3 forwardRayStart = frontOfCar.transform.position /*+ new Vector3(transform.position.x, transform.position.y + .3f, transform.position.z + 1.3f)*/;
        Vector3 forwardRayEnd = /*transform.forward + new Vector3(0, -.3f, 3)*/ new Vector3(transform.forward.x * 3, transform.forward.y - .3f, transform.forward.z * 3);

        //Debug.DrawRay(leftRayStart, leftRayEnd * 3, Color.red);
        //Debug.DrawRay(rightRayStart, rightRayEnd * 3, Color.red);      
        //Debug.DrawRay(backwardRayStart, backwardRayEnd * 3, Color.red);

        //bool groundLeft = Physics.Raycast(leftRayStart, leftRayEnd, 3);
        //bool groundRight = Physics.Raycast(rightRayStart, rightRayEnd, 3);
        //bool groundBackward = Physics.Raycast(backwardRayStart, backwardRayEnd, 3);

        bool groundForward = Physics.Raycast(forwardRayStart, forwardRayEnd, 3);

        if (!groundForward) {
            Debug.DrawRay(forwardRayStart, forwardRayEnd * 3, Color.green);
            Debug.Log("NoHitFloor");
            if (speed > 10) {
                AddReward(-.5f);
            }
        } else {
            Debug.Log("HitFloor");
            Debug.DrawRay(forwardRayStart, forwardRayEnd * 3, Color.red);
        }

        float newDistance = Vector3.Distance(frontOfCar.transform.position, targetCheckpoint.position);

        if (newDistance < currentDistance) {
            AddReward(.05f);
        }
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

        Vector3 toCheckPoint = (targetCheckpoint.position - frontOfCar.position).normalized;

        sensor.AddObservation(toCheckPoint);

        sensor.AddObservation(Vector3.Dot(frontOfCar.forward.normalized, targetCheckpoint.transform.right.normalized));

        sensor.AddObservation(Vector3.Distance(frontOfCar.transform.position, targetCheckpoint.position));
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

    //private void OnDrawGizmos() {
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y + .3f, transform.position.z + 1.3f), new Vector3(0, -.3f, 3));
    //    //Gizmos.DrawRay(new Vector3(transform.position.x - .5f, transform.position.y - .01f, transform.position.z), new Vector3(-2, -.5f, 0));
    //    //Gizmos.DrawRay(new Vector3(transform.position.x + .5f, transform.position.y - .01f, transform.position.z), new Vector3(2, -.5f, 0));
    //    //Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y - .01f, transform.position.z + 1), new Vector3(0, -.5f, 3));
    //    //Gizmos.DrawRay(new Vector3(transform.position.x, transform.position.y - .01f, transform.position.z - 1), new Vector3(0, -.5f, -3));
    //}
}
