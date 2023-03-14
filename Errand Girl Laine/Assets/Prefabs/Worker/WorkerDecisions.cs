using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class WorkerDecisions : MonoBehaviour
{
    //Decision loop cancels its own cancelTokenSource. Since that's the one that WorkerDecisions uses, WorkerDecisions's token source is then cancelled
    CancellationTokenSource cancelTokenSource;

    WorkerMovement WorkerMovement;
    WorkerMeleeHitboxes WorkerMeleeHitboxes;

    Transform player;
    Transform workerTransform;

    [Header("All Scripts")]
    [SerializeField] float workerWidth;
    [SerializeField] float workerHeight;

    [Header("Animation")]
    [SerializeField] bool facePlayer;

    [Header("All Decisions")]
    [SerializeField] float CloseRangeDecisionProximity; //If player is within close range decision proximity, make a close range decision. If not, make a long range decision

    [Header("Standard Approach")]
    //Startup phase
    [SerializeField] [Range(0, 10)] float SAStartupSecs;
    //Accelerate phase
    [SerializeField] float SAMaxSpeed;
    [SerializeField] [Range(0f, 1f)] float SAAcceleration; //worker should accelerate nearly instantly
    [SerializeField] float SAMaxMvmtDurationSecs; //max amt of time worker spends moving during the move; if worker doesn't come within attack range of the player, they make their next decision after this duration ends
    [SerializeField] float SADetectionRange; //WorkerMovement controls detection hitboxes ("detectors") to tell the worker when to start decelerating, while WorkerMeleeHitboxes control the actual sizes of the attack hitboxes; detection range ≠ attack range
    //Decelerate phase
    [SerializeField] [Range(0f, 1f)] float SADeceleration; //worker should decelerate nearly instantly
    [SerializeField] float SADecelMinSpeed; //min speed worker can travel at during deceleration phase before they instantly stop
    //Endlag phase
    [SerializeField] [Range(0f, 1f)] float SAEndlagSecs;

    [Header("Punch")]
    //Startup phase
    [SerializeField] [Range(0, 10)] float PunchStartupSecs;
    //Attack phase
    [SerializeField] float PunchRange;
    [SerializeField] Vector2 PunchHitboxSize;
    [SerializeField] int PunchDurationFrames;
    //Endlag phase
    [SerializeField] [Range(0, 10)] float PunchEndlagSecs;
    //Make next decision
    [SerializeField] int ConsecPunchCounter = 0;
    [SerializeField] int MaxConsecPunches = 0; //If worker consecutively punches more than the max consecutive punches, they can no longer punch until they've made a different move

    [Header("Block")]
    //Startup phase
    [SerializeField] [Range(0, 10)] float BlockStartupFrames;
    [SerializeField] [Range(0, 5)] float BlockMaxDuration; //Worker can't block longer than this duration
    //Endlag phase
    [SerializeField] [Range(0, 10)] float BlockEndlagSecs;

    [Header("Dodge Away")]
    //Startup phase
    [SerializeField] [Range(0, 10)] float DodgeAwayStartupSecs;
    //Accelerate phase
    [SerializeField] float DodgeAwayMaxSpeed;
    [SerializeField] float DodgeAwayMaxMvmtDurationSecs;
    //Decelerate phase
    [SerializeField] [Range(0f, 1f)] float DodgeAwayDeceleration;
    [SerializeField] float DodgeAwayDecelMinSpeed;
    //Endlag phase
    [SerializeField] [Range(0f, 1f)] float DodgeAwayEndlagSecs;

    [Header("Dodge Behind")]
    //Startup phase
    [SerializeField] [Range(0, 10)] float DodgeBehindStartupSecs;
    //Accelerate phase
    [SerializeField] float DodgeBehindMaxSpeed;
    [SerializeField] float DodgeBehindMaxMvmtDurationSecs;
    //Decelerate phase
    [SerializeField] [Range(0f, 1f)] float DodgeBehindDeceleration;
    [SerializeField] float DodgeBehindDecelMinSpeed;
    //Endlag
    [SerializeField] [Range(0f, 1f)] float DodgeBehindEndlagSecs;

    [Header("Dash Attack Mvmt")]
    //Startup phase
    [SerializeField] [Range(0, 10)] float DashAttackStartupSecs;
    //Accelerate phase
    [SerializeField] float DashAttackMaxSpeed;
    [SerializeField] [Range(0f, 1f)] float DashAttackAcceleration;
    [SerializeField] float DashAttackMaxMvmtDurationSecs;
    [SerializeField] float DashAttackDetectionRange;
    [SerializeField] float DashAttackDetectorStartupTimeSecs;
    //Decelerate phase
    [SerializeField] [Range(0f, 1f)] float DashAttackDeceleration;
    [SerializeField] [Range(0f, 1f)] float DashAttackDecelMinSpeed;

    [Header("Dash Attack Post-Mvmt")]
    //Attack phase
    [SerializeField] float DashAttackRange;
    [SerializeField] Vector2 DashAttackHitboxSize;
    [SerializeField] int DashAttackDurationFrames;
    //Endlag phase
    [SerializeField] [Range(0,10)] float DashAttackEndlagSecs;

    //A layer mask is essentially a collection of layers; instead of checking whether an obj is touching multiple layers, you could instead check if the obj is touching a layer mask.
    //A contact filter can check whether a Collider2D is in contact with (or has a condition that satisfies) any number of different things, e.g. a layer mask, a certain depth, or an angle
    //Contact filters tell the worker whether a player, wall, or either (player + wall = target) has been detected
    //Secondary layer mask & contact filter in case an attack has different results depening on what's detected (e.g. VertRush)
    //Usage of layer mask and contact filter found in WorkerMovement

    [Header("Stand Menacingly")]
    //Startup phase
    [SerializeField] [Range(0, 10)] float SMStartupSecs;
    //Endlag phase
    [SerializeField] [Range(0, 10)] float SMEndlagSecs;

    [Header("Circle Around")]
    //Startup phase
    [SerializeField] [Range(0, 10)] float CAStartupSecs;
    //Accelerate phase
    [SerializeField] float CAMaxSpeed;
    [SerializeField] [Range(0f, 1f)] float CAAcceleration; //worker should accelerate smoothly
    [SerializeField] float CAMaxMvmtDurationSecs; //max amt of time worker spends moving during the move
    //Decelerate phase
    [SerializeField] [Range(0f, 1f)] float CADeceleration; //worker should decelerate smoothly
    [SerializeField] float CADecelMinSpeed; //min speed worker can travel at during deceleration phase before they instantly stop
    //Endlag phase
    [SerializeField] [Range(0f, 1f)] float CAEndlagSecs;

    //Movable circle for circular detector visualizer
    [Header("Circular Detector Visualizer")]
    [SerializeField] [Range(-10, 10)] float CDV_XOffset;
    [SerializeField] [Range(-10, 10)] float CDV_YOffset;
    [SerializeField] [Range(0, 20)] float CDV_Radius;

    //See WorkerDecisionLoop for timing explanation
    async void Awake()
    {
        //It's possible that you may need to wait for mvmt & hboxes to initialize before getting references to them
        await Task.Delay(500);

        workerTransform = transform.GetChild(0);

        player = GameObject.FindGameObjectWithTag("Player1").GetComponent<Transform>();

        WorkerMovement = GetComponent<WorkerMovement>();
        WorkerMovement.SetAttributes(
            initPlayer: player,
            initWorkerTransform: workerTransform,
            initWorkerWidth: workerWidth,
            initWorkerHeight: workerHeight);

        WorkerMeleeHitboxes = GetComponent<WorkerMeleeHitboxes>();
        WorkerMeleeHitboxes.SetAttributes(
            initWorkerTransform: workerTransform,
            initWorkerWidth: workerWidth,
            initWorkerHeight: workerHeight);
    }

    public void SetCancelTokenSource(CancellationTokenSource newSource)
    {
        cancelTokenSource = newSource;

        WorkerMovement.SetCancelTokenSource(newSource);
        WorkerMeleeHitboxes.SetCancelTokenSource(newSource);
    }

    private void Update()
    {
        if (facePlayer)
        {
            if (player.position.x < workerTransform.position.x)
            {
                workerTransform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                workerTransform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    public async Task<WorkerDecision> StandardApproach()
    {
        //Startup phase
        await WorkerMovement.FreezeSecs(
                duration: SAStartupSecs);

        //Move (accelerate if not at max speed, stay at max speed if at or over max speed) in the direction of the player until the player is within attack range or the worker jogs for the max amt of time or the dist from the worker to the wall is too low ("too low" = attack range).

        // Face the player while following them
        facePlayer = true;

        //Wait for acceleration phase to end before starting deceleration phase; acc phase ends when player is detected
        await WorkerMovement.StandardApproachAccelerate(
            primaryLayerMask: LayerMask.GetMask("Player1"),

            SAMaxSpeed: SAMaxSpeed,
            SAAcceleration: SAAcceleration,
            SAMaxMvmtDurationSecs: SAMaxMvmtDurationSecs,
            SADetectionRange: SADetectionRange);

        //Wait for decel phase to end before starting endlag
        await WorkerMovement.StandardApproachDecelerate(
            SADeceleration: SADeceleration,
            SADecelMinSpeed: SADecelMinSpeed);

        // Stop facing the player during endlag
        facePlayer = false;

        //Endlag phase
        await WorkerMovement.FreezeSecs(
            duration: SAEndlagSecs);

        //Make next decision
        //return StandardApproachMakeNextDecision();
        return MakeNextDecisionTest();
    }

    private WorkerDecision MakeNextDecisionTest()
    {
        Vector2 distVect = player.position - workerTransform.position;
        //If the player is within close range of the worker, randomly make a close range decision. Otherwise, randomly make a long range decision
        if ((Mathf.Abs(distVect.x) <= workerWidth / 2 + CloseRangeDecisionProximity) && (Mathf.Abs(distVect.y) <= workerHeight / 2 + CloseRangeDecisionProximity))
        {
            if (Random.value < .33f)
                return WorkerDecision.Punch;
            else if (Random.value < .66f)
                return WorkerDecision.DodgeBehind;
            else
                return WorkerDecision.DodgeAway;
        }
        else
        {
            if (Random.value < .33f)
                return WorkerDecision.StandardApproach;
            else if (Random.value < .66f)
                return WorkerDecision.DashAttack;
            else
                return WorkerDecision.CircleAround;
        }
    }

    private WorkerDecision StandardApproachMakeNextDecision()
    {
        Vector2 distVect = player.position - workerTransform.position;
        //If the player is within close range of the worker, randomly make a close range decision. Otherwise, randomly make a long range decision
        if (Mathf.Abs(distVect.x) <= workerWidth / 2 + CloseRangeDecisionProximity)
        {
            switch(Random.value) //random float btwn 0 & 1
            {
                //Equal chance to punch, block, dodge away, or dodge behind
                case float x when 0 <= x && x <= .25f:
                    return WorkerDecision.Punch;
                case float x when .25f <= x && x <= .5f:
                    return WorkerDecision.Block;
                case float x when .5f <= x && x <= .75f:
                    return WorkerDecision.DodgeAway;
                case float x when .75f <= x && x <= 1:
                    return WorkerDecision.DodgeBehind;
                default:
                    return WorkerDecision.StandMenacingly;
            }
        }
        else
        {
            switch (Random.value) //random float btwn 0 & 1
            {
                //Equal chance to standard approach, dash attack, ankle breaker, stand menacingly, or circle around
                case float x when 0 <= x && x <= .2f:
                    return WorkerDecision.StandardApproach;
                case float x when .2f <= x && x <= .4f:
                    return WorkerDecision.DashAttack;
                case float x when .4f <= x && x <= .6f:
                    return WorkerDecision.DodgeAway;
                case float x when .6f <= x && x <= .8f:
                    return WorkerDecision.StandMenacingly;
                case float x when .6f <= x && x <= .8f:
                    return WorkerDecision.CircleAround;
                default:
                    return WorkerDecision.StandMenacingly;
            }
        }
    }

    public async Task<WorkerDecision> Punch()
    {
        //Remain in place and punch a single circular hitbox

        //Increment punch counter
        ++ConsecPunchCounter;

        // Face the player during the punch startup
        facePlayer = true;

        //Startup phase
        await WorkerMovement.FreezeSecs(
            duration: PunchStartupSecs);

        // Keep facing the same direction while punching
        facePlayer = false;

        //direction is either -1 or 1 depending on if the player is to the left (-1) or right (+1) of worker
        float xDifference = player.transform.position.x - workerTransform.position.x;
        float direction = xDifference / Mathf.Abs(xDifference);

        //Attack phase
        await WorkerMeleeHitboxes.PunchHitboxes(
            concurrentToCancellable: false,
            direction: direction,
            PunchRange: PunchRange,
            PunchHitboxSize: PunchHitboxSize,
            PunchDurationFrames: PunchDurationFrames);

        //Endlag phase
        await WorkerMovement.FreezeSecs(
                duration: PunchEndlagSecs);

        //Make next decision
        WorkerDecision punchNextDecision = MakeNextDecisionTest(); //PunchMakeNextDecision();

        //If the next chosen decision isn't a punch, reset the consecutive punch counter
        if (punchNextDecision != WorkerDecision.Punch)
            ConsecPunchCounter = 0;

        return punchNextDecision;
            
    }

    private WorkerDecision PunchMakeNextDecision()
    {
        Vector2 distVect = player.position - workerTransform.position;
        //If the player is within close range of the worker, randomly make a close range decision. Otherwise, randomly make a long range decision
        if (Mathf.Abs(distVect.x) <= workerWidth / 2 + CloseRangeDecisionProximity)
        {
            switch (Random.value) //random float btwn 0 & 1
            {
                // Equal chance to punch, block, dodge away, and dodge behind. If max consec punches has been reached, worker can no longer punch
                case float x when 0 <= x && x <= .25f:
                    if (ConsecPunchCounter < MaxConsecPunches)
                        return WorkerDecision.Punch;
                    else
                        return WorkerDecision.DodgeBehind;
                case float x when .25f <= x && x <= .5f:
                    return WorkerDecision.Block;
                case float x when .5f <= x && x <= .75f:
                    return WorkerDecision.DodgeAway;
                case float x when .75f <= x && x <= 1:
                    return WorkerDecision.DodgeBehind;
                default:
                    return WorkerDecision.StandMenacingly;
            }
        }
        else
        {
            switch (Random.value) //random float btwn 0 & 1
            {
                //Equal chance to standard approach, dash attack, ankle breaker, stand menacingly, or circle around
                case float x when 0 <= x && x <= .2f:
                    return WorkerDecision.StandardApproach;
                case float x when .2f <= x && x <= .4f:
                    return WorkerDecision.DashAttack;
                case float x when .4f <= x && x <= .6f:
                    return WorkerDecision.DodgeAway;
                case float x when .6f <= x && x <= .8f:
                    return WorkerDecision.StandMenacingly;
                case float x when .8f <= x && x <= 1:
                    return WorkerDecision.CircleAround;
                default:
                    return WorkerDecision.StandMenacingly;
            }
        }
    }

    public async Task<WorkerDecision> Block()
    {
        //Remain in place and block for a certain period of time

        //Face player during startup
        facePlayer = true;

        //Startup phase
        await WorkerMovement.FreezeSecs(
            duration: BlockStartupFrames);

        //Don't face player while blocking
        facePlayer = false;

        //Block phase
        //Enemy stat manager script here

        //Endlag phase
        await WorkerMovement.FreezeSecs(
                duration: BlockEndlagSecs);

        //Choose next attack
        //return BlockMakeNextDecision();
        return MakeNextDecisionTest();
    }

    private WorkerDecision BlockMakeNextDecision()
    {
        Vector2 distVect = player.position - workerTransform.position;
        //If the player is within close range of the worker, randomly make a close range decision. Otherwise, randomly make a long range decision
        if (Mathf.Abs(distVect.x) <= workerWidth / 2 + CloseRangeDecisionProximity)
        {
            switch (Random.value) //random float btwn 0 & 1
            {
                // 2/3 chance to punch, 1/6 chance to dodge away, 1/6 chance to dodge behind
                case float x when 0 <= x && x <= .66f:
                    return WorkerDecision.Punch;
                case float x when .66f <= x && x <= .83f:
                    return WorkerDecision.DodgeAway;
                case float x when .83f <= x && x <= 1:
                    return WorkerDecision.DodgeBehind;
                default:
                    return WorkerDecision.StandMenacingly;
            }
        }
        else
        {
            switch (Random.value) //random float btwn 0 & 1
            {
                case float x when 0 <= x && x <= .2f:
                    return WorkerDecision.StandardApproach;
                case float x when .2f <= x && x <= .4f:
                    return WorkerDecision.DashAttack;
                case float x when .4f <= x && x <= .6f:
                    return WorkerDecision.DodgeAway;
                case float x when .6f <= x && x <= .8f:
                    return WorkerDecision.StandMenacingly;
                case float x when .8f <= x && x <= 1:
                    return WorkerDecision.CircleAround;
                default:
                    return WorkerDecision.StandMenacingly;
            }
        }
    }

    public async Task<WorkerDecision> DodgeAway()
    {
        //Face the player during startup
        facePlayer = true;

        //Startup phase
        await WorkerMovement.FreezeSecs(
            duration: DodgeAwayStartupSecs);

        //Don't change face direction while moving
        facePlayer = false;

        //Move through the player by disabling collision with player
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player1"), LayerMask.NameToLayer("Enemy"), true);

        //Instantly move in a straight line away from the player, keep that speed for a bit
        await WorkerMovement.DodgeAwayConstantSpeed(
            DodgeAwayMaxSpeed: DodgeAwayMaxSpeed,
            DodgeAwayMaxMvmtDurationSecs: DodgeAwayMaxMvmtDurationSecs);

        //Decelerate to a stop
        await WorkerMovement.DodgeAwayDecelerate(
            DodgeAwayDeceleration: DodgeAwayDeceleration,
            DodgeAwayDecelMinSpeed: DodgeAwayDecelMinSpeed);

        //Endlag phase
        await WorkerMovement.FreezeSecs(
            duration: DodgeAwayEndlagSecs);

        //Become physically tangible to the player again after endlag
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player1"), LayerMask.NameToLayer("Enemy"), false);

        return MakeNextDecisionTest();
    }

    public async Task<WorkerDecision> DodgeBehind()
    {
        //Face the player during startup
        facePlayer = true;

        //Startup phase
        await WorkerMovement.FreezeSecs(
            duration: DodgeBehindStartupSecs);

        //Don't change face direction while moving
        facePlayer = false;

        //Move through the player by disabling collision with player
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player1"), LayerMask.NameToLayer("Enemy"), true);

        //Instantly move in a straight line away from the player, keep that speed for a bit
        await WorkerMovement.DodgeBehindConstantSpeed(
            DodgeBehindMaxSpeed: DodgeBehindMaxSpeed,
            DodgeBehindMaxMvmtDurationSecs: DodgeBehindMaxMvmtDurationSecs);

        //Decelerate to a stop
        await WorkerMovement.DodgeBehindDecelerate(
            DodgeBehindDeceleration: DodgeBehindDeceleration,
            DodgeBehindDecelMinSpeed: DodgeBehindDecelMinSpeed);

        //Endlag phase
        await WorkerMovement.FreezeSecs(
            duration: DodgeBehindEndlagSecs);

        //Become physically tangible to the player again after endlag
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player1"), LayerMask.NameToLayer("Enemy"), false);

        return MakeNextDecisionTest();
    }

    public async Task<WorkerDecision> DashAttack()
    {
        //Face player during startup
        facePlayer = true;

        //Startup phase
        await WorkerMovement.FreezeSecs(
                duration: DashAttackStartupSecs);

        //Don't change face direction while moving
        facePlayer = false;

        //Move (accelerate if not at max speed, stay at max speed if at or over max speed) in the direction of the player until the player is within attack range or the worker jogs for the max amt of time or the dist from the worker to the wall is too low ("too low" = attack range).

        //Wait for acceleration phase to end before starting deceleration phase; acc phase ends when player is detected
        float direction = await WorkerMovement.DashAttackAccelerate(
            primaryLayerMask: LayerMask.GetMask("Player"),

            DashAttackMaxSpeed: DashAttackMaxSpeed,
            DashAttackAcceleration: DashAttackAcceleration,
            DashAttackMaxMvmtDurationSecs: DashAttackMaxMvmtDurationSecs,
            DashAttackDetectionRange: DashAttackDetectionRange,
            DashAttackDetectorTimeStartupSecs: DashAttackDetectorStartupTimeSecs);

        //Start attack and decel at the same time
        WorkerMeleeHitboxes.DashAttackHitboxes(
            concurrentToCancellable: true,
            direction: direction,
            DashAttackRange: DashAttackRange,
            DashAttackHitboxSize: DashAttackHitboxSize,
            DashAttackDurationFrames: DashAttackDurationFrames);

        await WorkerMovement.DashAttackDecelerate(
            DashAttackDeceleration: DashAttackDeceleration,
            DashAttackDecelMinSpeed: DashAttackDecelMinSpeed);

        //Endlag phase
        await WorkerMovement.FreezeSecs(
            duration: DashAttackEndlagSecs);

        //Make next decision
        return MakeNextDecisionTest();
    }

    public async Task<WorkerDecision> StandMenacingly()
    {
        //Face player continuously throughout move
        facePlayer = true;

        //Startup phase
        await WorkerMovement.FreezeSecs(
            duration: SMStartupSecs);

        //Attack phase
        // attack code here

        //Endlag phase
        await WorkerMovement.FreezeSecs(
            duration: SMEndlagSecs);

        //Stop looking at player after move ends
        facePlayer = false;

        return MakeNextDecisionTest();
    }

    public async Task<WorkerDecision> CircleAround()
    {
        //Face the player thorughout move
        facePlayer = true;

        //Startup phase
        await WorkerMovement.FreezeSecs(
                duration: CAStartupSecs);

        //Circle around the player for a predestined amt of time; player gets flanked, but move has a fixed duration every time, making it punishable

        //Wait for acceleration phase to end before starting deceleration phase
        float circleDirection = await WorkerMovement.CircleAroundAccelerate(
            CAMaxSpeed: CAMaxSpeed,
            CAAcceleration: CAAcceleration,
            CAMaxMvmtDurationSecs: CAMaxMvmtDurationSecs);

        //Wait for decel phase to end before starting endlag
        await WorkerMovement.CircleAroundDecelerate(
            circleDirection: circleDirection,

            CADeceleration: CADeceleration,
            CADecelMinSpeed: CADecelMinSpeed);

        //Endlag phase
        await WorkerMovement.FreezeSecs(
            duration: CAEndlagSecs);

        //Stop looking at player after move ends
        facePlayer = false;

        //Make next decision
        //return CircleAroundMakeNextDecision();
        return MakeNextDecisionTest();
    }

    /*
    private WorkerDecision DashAttackMakeNextDecision()
    {
        Vector2 distVect = player.position - workerTransform.position;
        //If the player is within close range of the worker, randomly make a close range decision. Otherwise, randomly make a long range decision
        if (Mathf.Abs(distVect.x) <= workerWidth / 2 + CloseRangeDecisionProximity)
        {
            //Guarantee to dodge away
            return WorkerDecision.DodgeAway;
        }
        else
        {
            switch (Random.value) //random float btwn 0 & 1
            {
                //Equal chance to standard approach, dash attack, ankle breaker, stand menacingly, or circle around
                case float x when 0 <= x && x <= .2f:
                    return WorkerDecision.StandardApproach;
                case float x when .2f <= x && x <= .4f:
                    return WorkerDecision.DashAttack;
                case float x when .4f <= x && x <= .6f:
                    return WorkerDecision.AnkleBreaker;
                case float x when .6f <= x && x <= .8f:
                    return WorkerDecision.StandMenacingly;
                case float x when .6f <= x && x <= .8f:
                    return WorkerDecision.CircleAround;
                default:
                    return WorkerDecision.StandMenacingly;
            }
        }
    }
    */

    //Check if the player is within a list of detected colliders/raycast hits
    private bool PlayerIn(Collider2D[] colTargetList = null, RaycastHit2D[] rayTargetList = null)
    {
        if (colTargetList != null)
        {
            foreach (Collider2D col in colTargetList)
            {
                if (col != null && col.CompareTag("Player")) { return true; }
            }
            return false;
        }
        else if (rayTargetList != null && rayTargetList.Length != 0)
        {
            foreach (RaycastHit2D hit in rayTargetList)
            {
                if (hit.collider != null && hit.collider.CompareTag("Player")) { return true; }
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    //Movable circle for detector visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            center: transform.position + (CDV_XOffset * Vector3.right) + (CDV_YOffset * Vector3.up),
            radius: CDV_Radius);
    }

}
