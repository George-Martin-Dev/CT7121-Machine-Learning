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

    private int checkPointsPassed = 0;

    private float smoothRotationChange = 0f;

    [Tooltip("Speed of agent")]
    [SerializeField] private float moveSpeed;

    [Tooltip("Speed of rotation")]
    [SerializeField] private float rotationSpeed = 100f;

    private void Awake() {
        GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    private void Start() {
        startPos = transform.position;
        targetCheckpoint = GM.checkPoints[0];
        checkPointBackVector = targetCheckpoint.transform.right.normalized;
    }

    public override void OnActionReceived(ActionBuffers actions) {

        if (GM.frozen) {
            return;
        }

        ActionSegment<float> vectorAction = actions.ContinuousActions;

        Vector3 currentRotation = transform.rotation.eulerAngles;

        float rotation = vectorAction[0];

        smoothRotationChange = Mathf.MoveTowards(smoothRotationChange, rotation, 2f * Time.deltaTime);

        float newRotation = currentRotation.x + smoothRotationChange * Time.deltaTime * rotationSpeed;

        transform.rotation = Quaternion.Euler(0, newRotation, 0);

        rb.AddForce(transform.forward * moveSpeed);
    }

    private Vector3 toCheckpoint;
    public override void CollectObservations(VectorSensor sensor) {

        if (GM.frozen) {
            return;
        }

        if (targetCheckpoint == null) {
            sensor.AddObservation(new float[10]);
            return;
        }

        sensor.AddObservation(transform.rotation.eulerAngles.y);

        // Get direction vector from the car to the targeted checkpoint
        toCheckpoint = targetCheckpoint.position - frontOfCar.position;

        // Observe a vector that represents the direction from the car to the targeted checkpoint
        sensor.AddObservation(toCheckpoint.normalized);

        // Observe a dot product that tells if the car is pointing towards the targeted checkpoint
        sensor.AddObservation(Vector3.Dot(frontOfCar.forward.normalized, checkPointBackVector));
    }

    private void OnTriggerExit(Collider other) {

        if (other.transform == targetCheckpoint) {
            float facingDirection = Vector3.Dot(frontOfCar.forward.normalized, checkPointBackVector);
            if (facingDirection < 0) {
                checkPointsPassed++;
                targetCheckpoint = GM.checkPoints[checkPointsPassed];
                checkPointBackVector = targetCheckpoint.transform.right.normalized;
                AddReward(.5f);
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        transform.position = startPos;
        AddReward(-.5f);
    }
}
