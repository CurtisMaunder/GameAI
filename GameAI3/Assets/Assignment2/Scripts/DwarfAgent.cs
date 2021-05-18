using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DwarfAgent : Agent {
    Rigidbody2D rBody;
    Vector2 initPos;
    private float timeSinceSpawn;
    private Arena arena;
    // Start is called before the first frame update
    void Start(){
        rBody = GetComponent<Rigidbody2D>();
        initPos = this.transform.position;
        arena = GetComponentInParent<Arena>();
    }

    void Update(){
        timeSinceSpawn += Time.deltaTime;
    }

    public Transform Target;
    public override void OnEpisodeBegin(){
        this.transform.position = initPos;
        rBody.velocity = Vector2.zero;
        timeSinceSpawn = 0f;
        arena.ResetArea();
    }

    public override void CollectObservations(VectorSensor sensor){
        //Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        //Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.y);
    }

    public float forceMultiplier = 10f;
    public override void OnActionReceived(ActionBuffers actionBuffers){
        /*
        //Actions, size = 2
        Vector2 controlSignal = Vector2.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.y = actionBuffers.ContinuousActions[1];
        rBody.velocity = controlSignal * 5f;
        */

        //Moving from continuous to discrete actions
        //Sourced from Code Monkey
        int moveX = actionBuffers.DiscreteActions[0]; //0 = Don't move | 1 = Left | 2 = Right
        int moveY = actionBuffers.DiscreteActions[1]; //0 = Don't move | 1 = Up   | 2 = Down

        Vector2 addForce = Vector2.zero;

        switch (moveX) {
            case 0: addForce.x = 0f; break;
            case 1: addForce.x = -1f; break;
            case 2: addForce.x = +1f; break;
        }

        switch (moveY) {
            case 0: addForce.y = 0f; break;
            case 1: addForce.y = -1f; break;
            case 2: addForce.y = +1f; break;
        }

        float moveSpeed = 5f;
        rBody.velocity = addForce * moveSpeed;

        //Rewards
        float distanceToTarget = Vector2.Distance(this.transform.localPosition, Target.localPosition);
        AddReward(Mathf.Exp(1 / distanceToTarget) * 0.00001f);
        transform.up = rBody.velocity;
    }

    public override void Heuristic(in ActionBuffers actionsOut){
        /*
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
        */
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        switch (Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"))) {
            case -1: discreteActions[0] = 1; break;
            case 0: discreteActions[0] = 0; break;
            case +1: discreteActions[0] = 2; break;
        }

        switch (Mathf.RoundToInt(Input.GetAxisRaw("Vertical"))) {
            case -1: discreteActions[1] = 1; break;
            case 0: discreteActions[1] = 0; break;
            case +1: discreteActions[1] = 2; break;
        }
    }

    private void OnTriggerEnter2D(Collider2D col){
        if(col.tag == "Goal"){
            SetReward(0.0f);
            AddReward(1f);
            AddReward(-(timeSinceSpawn * 0.01f));

            EndEpisode();
        }

        if(col.tag == "Treat"){
            AddReward(0.1f);
            arena.Collect(col.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D col){
        if(col.tag == "Start"){
            SetReward(0.0f);
        }
    }
}
