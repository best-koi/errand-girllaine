using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

public class WorkerMeleeHitboxes : MonoBehaviour
{
    //When WorkerDecisions cancels its token source, WorkerMeleeHitboxes's token source is then cancelled
    CancellationTokenSource cancelTokenSource;

    [Header("All Attacks")]

    Transform workerTransform;
    
    //Stat manager script here
    //WorkerMeleeHitboxData hitboxData;

    //Adjustable parameters
    float workerWidth;
    float workerHeight;

    //See WorkerDecisionLoop for timing explanation
    public void SetAttributes(
        Transform initWorkerTransform,
        float initWorkerWidth,
        float initWorkerHeight)
    {
        workerTransform = initWorkerTransform;

        workerWidth = initWorkerWidth;
        workerHeight = initWorkerHeight;
    }

    public void SetCancelTokenSource(CancellationTokenSource newSource)
    {
        cancelTokenSource = newSource;
    }

    public async Task WaitFrames(int frames, bool concurrentToCancellable)
    {
        //Wait for a certain number of frames to pass; used for animation and attack timing
        for (int i = 0; i < frames; i++)
        {
            if (cancelTokenSource.Token.IsCancellationRequested)
            {
                CancelMove(concurrentToCancellable: concurrentToCancellable);
                return;
            }

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
    }

    private void CancelMove(bool concurrentToCancellable)
    {
        //Cancel the melee hitboxes method
        //Return if running concurrently to a cancellable method in WorkerDecisions (e.g. one from Worker1Mvmt), throw an exception if not running concurrently to any cancellable method in WorkerDecisions
        if (!concurrentToCancellable) { throw new TaskCanceledException(); }
    }

    public async Task PunchHitboxes(
        bool concurrentToCancellable,

        float direction,

        float PunchRange,
        Vector2 PunchHitboxSize,
        int PunchDurationFrames)

    {
        //Stat manager script here
        //hitboxData.SetCurrentAttack(WorkerDecision.Punch);

        /*
        //Get a reference to the Box Collider and activate it
        BoxCollider2D hitbox = workerTransform.GetComponent<BoxCollider2D>();
        hitbox.enabled = true;
        */

        //Set Box Collider's size
        //hitbox.size = PunchHitboxSize;

        workerTransform.GetComponent<Animator>().SetTrigger("Attack");

        for (int i = 0; i < PunchDurationFrames; i++)
        {
            if (cancelTokenSource.Token.IsCancellationRequested)
            {
                CancelMove(concurrentToCancellable);
                return;
            }

            //Activate hitbox
            //hitbox.enabled = true;

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();

            //Deactivate hitbox
            //hitbox.enabled = false;
        }

        //Disable hitbox
        //hitbox.enabled = false;
    }

    public async void DashAttackHitboxes(
        bool concurrentToCancellable,

        float direction,

        float DashAttackRange,
        Vector2 DashAttackHitboxSize,
        int DashAttackDurationFrames)
    {
        //Stat manager script here
        //hitboxData.SetCurrentAttack(WorkerDecision.Punch);

        //Get a reference to the Box Collider and activate it
        /*
        BoxCollider2D hitbox = workerTransform.GetComponent<BoxCollider2D>();
        hitbox.enabled = true;

        //Set Box Collider's size
        hitbox.size = DashAttackHitboxSize;
        */

        workerTransform.GetComponent<Animator>().SetTrigger("Attack");

        for (int i = 0; i < DashAttackDurationFrames; i++)
        {
            if (cancelTokenSource.Token.IsCancellationRequested)
            {
                CancelMove(concurrentToCancellable);
                return;
            }

            //Activate hitbox
            //hitbox.enabled = true;

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();

            //Deactivate hitbox
            //hitbox.enabled = false;
        }

        //Disable hitbox
        //hitbox.enabled = false;
    }

    //Moves and reorients the sword according to the handAngle, and handDist
    private void UpdateHand(Vector2 handCenter)
    {
        //hitboxTransform.position = handCenter + handDist * new Vector2(Mathf.Cos(DegToRad(handAngle)), Mathf.Sin(DegToRad(handAngle)));
    }

    //Converts degrees to radians for trig funcs
    private float DegToRad(float deg)
    {
        return deg * (Mathf.PI / 180);
    }
}