using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class WorkerDecisionLoop : MonoBehaviour
{
    WorkerDecisions workerDecisions; // Script containing worker decisions

    CancellationTokenSource cancelTokenSource; // Can cancel the current decision

    [SerializeField] bool tempVar; //Used for testing

    //Timing: WorkerMeleeHitboxes and WorkerMovement initialize themselves, then WorkerDecisions gets a reference to hbxs and mvmt while initializing, then workerDecisionLoop gets a reference to decs while initializing.
    //Awake is used instead of Start bc when a level loads for the first time, I plan to instantiate the worker in the level, then disable them until the player initiates the fight (rather than waiting until the player reaches the worker room before instantiating the worker). That way, the worker can load together and initialize itself with the rest of the level at the beginning and the player won't have to experience any repeated loading time or potential drop in performance if I were to instead load the worker in as soon as the player initiates the fight every time.
    //Awake is called when the worker is instantiated whether the worker is enabled or not, allowing me to load in the worker in a disabled state. Start and OnEnable only run when the worker is enabled.
    async void Awake()
    {
        //Wait for WorkerMovement to initialize before getting a reference to it
        float waitEnd = Time.time + 1.5f;
        while (Time.time < waitEnd)
            await Task.Yield();

        workerDecisions = GetComponent<WorkerDecisions>();

        StartMakingDecisions();
    }

    //DecisionLoop calls a decision func from WorkerDecisions, then (a)waits for it to finish before calling the next one

    async void DecisionLoop()
    {
        Debug.Log("Decision loop started");

        //The worker's first decision after a stoppage; this should eventually be randomly chosen so that every time the worker starts decisioning again after stopping, the worker starts their decisions differently every time
        WorkerDecision nextDecisionChoice = WorkerDecision.StandardApproach;

        //Main decision loop
        while (!cancelTokenSource.Token.IsCancellationRequested)
        {
            switch (nextDecisionChoice)
            {
                case WorkerDecision.StandardApproach:
                    Debug.Log("Current decision: StandardApproach");
                    try { nextDecisionChoice = await workerDecisions.StandardApproach(); }
                    catch (TaskCanceledException)
                    {
                        Debug.Log("Standard Approach cancelled");
                        break;
                    }
                    break;
                case WorkerDecision.Punch:
                    Debug.Log("Current decision: Punch");
                    try { nextDecisionChoice = await workerDecisions.Punch(); }
                    catch (TaskCanceledException)
                    {
                        Debug.Log("Punch cancelled");
                        break;
                    }
                    break;
                case WorkerDecision.Block:
                    //Debug.Log("Current decision: Block");
                    try { nextDecisionChoice = await workerDecisions.Block(); }
                    catch (TaskCanceledException)
                    {
                        Debug.Log("Block cancelled");
                        break;
                    }
                    break;
                case WorkerDecision.DodgeAway:
                    Debug.Log("Current decision: Dodge Away");
                    try { nextDecisionChoice = await workerDecisions.DodgeAway(); }
                    catch (TaskCanceledException)
                    {
                        Debug.Log("Dodge Away cancelled");
                        break;
                    }
                    break;
                case WorkerDecision.DodgeBehind:
                    Debug.Log("Current decision: Dodge Behind");
                    try { nextDecisionChoice = await workerDecisions.DodgeBehind(); }
                    catch (TaskCanceledException)
                    {
                        Debug.Log("Dodge Behind cancelled");
                        break;
                    }
                    break;
                case WorkerDecision.DashAttack:
                    Debug.Log("Current decision: Dash Attack");
                    try { nextDecisionChoice = await workerDecisions.DashAttack(); }
                    catch (TaskCanceledException)
                    {
                        Debug.Log("Dash Attack cancelled");
                        break;
                    }
                    break;
                case WorkerDecision.StandMenacingly:
                    Debug.Log("Current decision: Stand Menacingly");
                    try { nextDecisionChoice = await workerDecisions.StandMenacingly(); }
                    catch (TaskCanceledException)
                    {
                        Debug.Log("Stand Menacingly cancelled");
                        break;
                    }
                    break;
                case WorkerDecision.CircleAround:
                    Debug.Log("Current decision: Circle Around");
                    try { nextDecisionChoice = await workerDecisions.CircleAround(); }
                    catch (TaskCanceledException)
                    {
                        Debug.Log("Circle Around cancelled");
                        break;
                    }
                    break;
                default:
                    throw new TaskCanceledException("Error: decision doesn't exist");
            }
        }
        
        Debug.Log("decision loop ended");
    }

    //Cancel the current decision and don't let the worker try to decision again (until StartMakingDecisions is called)
    void StopMakingDecisions()
    {
        cancelTokenSource.Cancel();
    }

    //Make the worker start decisioning again
    //To start the main decision loop again after it's cancelled, create a new (i.e. non-cancelled) cancellationTokenSource and call decisionLoop
    void StartMakingDecisions()
    {
        cancelTokenSource = new CancellationTokenSource();
        
        workerDecisions.SetCancelTokenSource(cancelTokenSource);

        DecisionLoop();
    }

    //Temporarily using the player's input to manually cancel an decision; remove this method, tempVar, and the Player Input component of the worker1 game obj after testing
    
    void OnTest()
    {
        if (tempVar)
        {
            StopMakingDecisions();
        }
        else
        {
            StartMakingDecisions();
        }
        tempVar = !tempVar;
    }


    void OnDestroy()
    {
        StopMakingDecisions();
    }

}

public enum WorkerDecision
{
    StandardApproach,
    Punch,
    Block,
    DodgeAway,
    DodgeBehind,
    DashAttack,
    StandMenacingly,
    CircleAround,
}