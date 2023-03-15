using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class WorkerMovement : MonoBehaviour
{
    //When WorkerDecisions cancels its token source, WorkerMovement's token source is then cancelled
    CancellationTokenSource cancelTokenSource;

    Transform player;
    Transform workerTransform;
    Rigidbody2D rb;

    //Adjustable parameters
    float workerHeight;
    float workerWidth;

    //In-script control

    //A layer mask is essentially a collection of layers; instead of checking whether an obj is touching multiple layers, you could instead check if the obj is touching a layer mask.
    //A contact filter can check whether a Collider2D is in contact with (or has a condition that satisfies) any number of different things, e.g. a layer mask, a certain depth, or an angle
    //Contact filters tell the worker whether a player, wall, or either (player + wall = target) has been detected
    ContactFilter2D primaryContactFilter;

    //Secondary layer mask & contact filter in case an attack has different results depening on what's detected (e.g. VertRush)
    ContactFilter2D secondaryContactFilter;

    //Get all attributes from an initialization function
    public void SetAttributes(
        Transform initPlayer,
        Transform initWorkerTransform,
        float initWorkerHeight,
        float initWorkerWidth)
    {
        player = initPlayer;
        workerTransform = initWorkerTransform;
        rb = workerTransform.GetComponent<Rigidbody2D>();

        workerHeight = initWorkerHeight;
        workerWidth = initWorkerWidth;
    }

    public void SetCancelTokenSource(CancellationTokenSource newSource)
    {
        cancelTokenSource = newSource;
    }

    public async Task FreezeSecs(
        float duration)
    {
        //Freeze the worker for startup and endlag

        //Redundantly ensure that the worker is still and floats if in the air just in case
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;

        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            //Cancel the freeze if WorkerDecisions cancels it
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
    }

    public async Task DodgeAwayConstantSpeed(
        float DodgeAwayMaxSpeed,
        float DodgeAwayMaxMvmtDurationSecs)
    {
        //Instantly move in a straight line away from the player

        float mvmtEndTime = Time.time + DodgeAwayMaxMvmtDurationSecs;

        Vector2 direction = (workerTransform.position - player.position).normalized;

        rb.velocity = direction * DodgeAwayMaxSpeed;

        while (Time.time < mvmtEndTime)
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
    }

    public async Task DodgeAwayDecelerate(
        float DodgeAwayDeceleration,
        float DodgeAwayDecelMinSpeed)
    {
        //Slow down to an abrupt stop (stop moving before your vel would have naturally reached 0)

        //Decelerate smoothly to 0
        while (Mathf.Abs(rb.velocity.magnitude) > DodgeAwayDecelMinSpeed)
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            rb.velocity -= rb.velocity * DodgeAwayDeceleration;
            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
        rb.velocity = Vector2.zero;
    }

    public async Task DodgeBehindConstantSpeed(
        float DodgeBehindMaxSpeed,
        float DodgeBehindMaxMvmtDurationSecs)
    {
        //Instantly move in a straight line away from the player

        float mvmtEndTime = Time.time + DodgeBehindMaxMvmtDurationSecs;

        Vector2 direction = (player.position - workerTransform.position).normalized;

        rb.velocity = direction * DodgeBehindMaxSpeed;

        while (Time.time < mvmtEndTime)
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
    }

    public async Task DodgeBehindDecelerate(
        float DodgeBehindDeceleration,
        float DodgeBehindDecelMinSpeed)
    {
        //Slow down to an abrupt stop (stop moving before your vel would have naturally reached 0)

        //Decelerate smoothly to 0
        while (Mathf.Abs(rb.velocity.magnitude) > DodgeBehindDecelMinSpeed)
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            rb.velocity -= rb.velocity * DodgeBehindDeceleration;
            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
        rb.velocity = Vector2.zero;
    }

    public async Task<float> DashAttackAccelerate(
        LayerMask primaryLayerMask,

        float DashAttackMaxSpeed,
        float DashAttackAcceleration,
        float DashAttackMaxMvmtDurationSecs,
        float DashAttackDetectionRange,
        float DashAttackDetectorTimeStartupSecs)
    {
        //Move (accelerate if not at max speed, stay at max speed if at or over max speed) in a straight line until the player is seen by primary detector raycast or the worker runs for the max amt of time (mvmtDuration) or the dist from the worker to a border is too low ("too low" = attack range).
        //2 results can happen from this attack: worker runs for max amt of time and then punches, or worker finds player or border early and punches early
        //Primary detector only activates after a startup so that the worker doesn't punch right away

        float mvmtEndTime = Time.time + DashAttackMaxMvmtDurationSecs;

        //Targets that stop movement for primary attack: player, border
        primaryContactFilter.SetLayerMask(primaryLayerMask);
        //targetDetected becomes 1 or more if a target is within the worker's detection range
        int targetDetectedPrimary = 0;
        RaycastHit2D[] targetsPrimary = new RaycastHit2D[5];

        //direction is a Vector2 w/ len 1 pointing from the worker to the player; it only tells the worker which direction to travel in
        Vector2 direction = (player.position - workerTransform.position).normalized;

        //Worker immediately decelerates and punches if a player or border is detected in the primary detector or the movement duration ends
        //Primary detector is a short range raycast from the worker w/ length DashAttackPrimaryDetectionRange; its direction is set at the beginning of the move and remains constant
        float startupEndTime = Time.time + DashAttackDetectorTimeStartupSecs;
        while ((Time.time < mvmtEndTime) && (targetDetectedPrimary == 0))
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            if (Mathf.Abs(rb.velocity.magnitude) < DashAttackMaxSpeed)
            {
                rb.velocity += DashAttackMaxSpeed * DashAttackAcceleration * direction;
            }
            else
            {
                rb.velocity = DashAttackMaxSpeed * direction;
            }

            //Detection hitbox and secondary checker activate after DashAttackDetectionHitboxTimeStartup seconds.
            if (Time.time > startupEndTime)
            {
                targetDetectedPrimary = Physics2D.Raycast(
                    origin: workerTransform.position,
                    direction: direction,
                    primaryContactFilter, targetsPrimary,
                    distance: DashAttackDetectionRange);
            }

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
        return direction.x / Mathf.Abs(direction.x);
    }

    public async Task DashAttackDecelerate(
        float DashAttackDeceleration,
        float DashAttackDecelMinSpeed)
    {
        //Slow down, then smoothly stop (stop right around when the slowdown would have naturally decelerated to 0 vel)

        //Decelerate smoothly to 0
        while (Mathf.Abs(rb.velocity.magnitude) > DashAttackDecelMinSpeed)
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            rb.velocity -= rb.velocity * DashAttackDeceleration;
            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
        rb.velocity = Vector2.zero;
    }

    public async Task StandardApproachAccelerate(
        LayerMask primaryLayerMask,

        float SAMaxSpeed,
        float SAAcceleration,
        float SAMaxMvmtDurationSecs,
        float SADetectionRange)
    {
        //Abruptly accelerate to max speed, then continuously move towards player at max speed

        float mvmtEndTime = Time.time + SAMaxMvmtDurationSecs;

        //Targets that stop the move: player
        primaryContactFilter.SetLayerMask(primaryLayerMask);
        //targetDetected becomes 1 if a player is detected in the worker's attack range
        int targetDetected = 0;
        //Nothing will be done with the colliders in targets; this var fills a necessary parameter in the OverlapBox function
        RaycastHit2D[] targets = new RaycastHit2D[5];

        //Direction that the worker is currently moving in
        Vector2 vectDirection;

        //Velocity magnitude that will be increased as the worker accelerates
        float acceleratingVelMagnitude = 0;

        while ((Time.time < mvmtEndTime) && (targetDetected == 0))
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            //After every iteration, reset direction; direction is always twds the player
            vectDirection = (player.position - workerTransform.position).normalized;

            //After every iteration, increase the vel. You can't increase rb.velocity directly as in the other rushes bc doing so does not remove the velocity already present (e.g. if the worker is moving horizontally and the player moves a bit below the horizontal, the worker's vel won't go completely twds the player bc there's still a component existing going directly horizontal). rb.velocity must be reset every frame to ensure that 100% of the vector is going twds the player at all times (no components anywhere else) 
            acceleratingVelMagnitude += SAMaxSpeed * SAAcceleration;

            if (acceleratingVelMagnitude < SAMaxSpeed)
            {
                rb.velocity = vectDirection * acceleratingVelMagnitude;
            }
            else
            {
                rb.velocity = vectDirection * SAMaxSpeed;
            }
            targetDetected = Physics2D.Raycast(
                origin: workerTransform.position,
                direction: vectDirection,
                primaryContactFilter, targets,
                distance: SADetectionRange);

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
    }

    public async Task StandardApproachDecelerate(
        float SADeceleration,
        float SADecelMinSpeed)
    {
        //Decelerate, then snap to 0 vel when your vel is below the deceleration phase's minimum speed
        //Deceleration should be nearly identical to acceleration
        while (rb.velocity.magnitude > SADecelMinSpeed)
        {
            //Cancel the attack if the decision loop wants to cancel it
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            rb.velocity -= rb.velocity.normalized * SADeceleration;
            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
        rb.velocity = Vector2.zero;
    }

    public async Task<float> CircleAroundAccelerate(
        float CAMaxSpeed,
        float CAAcceleration,
        float CAMaxMvmtDurationSecs)
    {
        //Smoothly acc to max speed, then continuously in a circle around player
        //Circle constantly updates as player moves

        float mvmtEndTime = Time.time + CAMaxMvmtDurationSecs;

        //Decides whether worker will move in a circle clockwise or counterclockwise
        float circleDirection = Random.Range(-1.0f, 1.0f);

        //Direction that the worker is currently moving in
        Vector2 vectDirection = Vector2.zero;

        //Velocity magnitude that will be increased as the worker accelerates
        float acceleratingVelMagnitude = 0;

        while (Time.time < mvmtEndTime)
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            //After every iteration, reset direction; direction is always perpendicular to the vector twds the player
            vectDirection = (circleDirection * Vector2.Perpendicular(player.position - workerTransform.position)).normalized;

            //After every iteration, increase the vel. You can't increase rb.velocity directly as in the other rushes bc doing so does not remove the velocity already present (e.g. if the worker is moving horizontally and the player moves a bit below the horizontal, the worker's vel won't go completely twds the player bc there's still a component existing going directly horizontal). rb.velocity must be reset every frame to ensure that 100% of the vector is going twds the player at all times (no components anywhere else) 
            acceleratingVelMagnitude += CAMaxSpeed * CAAcceleration;

            if (acceleratingVelMagnitude < CAMaxSpeed)
            {
                rb.velocity = vectDirection * acceleratingVelMagnitude;
            }
            else
            {
                rb.velocity = vectDirection * CAMaxSpeed;
            }

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }

        //The decel phase needs to use the same circleDirection as the accel phase
        return circleDirection;
    }

    public async Task CircleAroundDecelerate(
        float circleDirection,

        float CADeceleration,
        float CADecelMinSpeed)
    {
        //Direction that the worker is currently moving in
        Vector2 vectDirection = Vector2.zero;

        float deceleratingVelMagnitude = rb.velocity.magnitude;

        //Decelerate, then snap to 0 vel when your vel is below the deceleration phase's minimum speed
        //Deceleration should be nearly identical to acceleration
        //While decelerating, be sure to keep circular movement
        while (rb.velocity.magnitude > CADecelMinSpeed)
        {
            if (cancelTokenSource.Token.IsCancellationRequested) { throw new TaskCanceledException(); }

            //After every iteration, reset direction; direction is always perpendicular to the vector twds the player
            vectDirection = (circleDirection * Vector2.Perpendicular(player.position - workerTransform.position)).normalized;

            //After every iteration, decrease the vel
            deceleratingVelMagnitude -= CADeceleration;

            rb.velocity = vectDirection * deceleratingVelMagnitude;

            float tickEnd = Time.time + Time.fixedDeltaTime;
            while (Time.time < tickEnd)
                await Task.Yield();
        }
        rb.velocity = Vector2.zero;
    }
}
