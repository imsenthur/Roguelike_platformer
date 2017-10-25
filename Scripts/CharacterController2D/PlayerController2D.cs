using UnityEngine;
using CLUTCHINPUT;
using CLUTCH;
using PC2D;

/// <summary>
/// This class is a simple example of how to build a controller that interacts with PlatformerMotor2D.
/// </summary>
[RequireComponent(typeof(PlatformerMotor2D))]
public class PlayerController2D : MonoBehaviour
{
    private PlatformerAnimation2D animator;
    private PlatformerMotor2D _motor;
    private bool _restored = true;
    private bool _enableOneWayPlatforms;
    private bool _oneWayPlatformsAreWalls;

    private float attacktimer = 0;
    private bool attacking = false;
    private float attackcd = 0.1f;

    public Collider2D attacktrigger;

    public GameObject visual;
    public Transform firepoint;
    public Vector2 firepointposition;
    public GameObject bullet;
    public GameObject handchild;
    public float firerate = 0;
    float timetofire = 0;

    // Use this for initialization
    void Start()
    {
        _motor = GetComponent<PlatformerMotor2D>();
        animator = GetComponent<PlatformerAnimation2D>();
        attacktrigger.enabled = false;
    }

    // before enter en freedom state for ladders
    void FreedomStateSave(PlatformerMotor2D motor)
    {
        if (!_restored) // do not enter twice
            return;

        _restored = false;
        _enableOneWayPlatforms = _motor.enableOneWayPlatforms;
        _oneWayPlatformsAreWalls = _motor.oneWayPlatformsAreWalls;
    }
    // after leave freedom state for ladders
    void FreedomStateRestore(PlatformerMotor2D motor)
    {
        if (_restored) // do not enter twice
            return;

        _restored = true;
        _motor.enableOneWayPlatforms = _enableOneWayPlatforms;
        _motor.oneWayPlatformsAreWalls = _oneWayPlatformsAreWalls;
    }

    public void ShootUpdate()
    {

        if (InputManager.GetAxis(GameInput.VERTICAL) < -1 * PC2D.Globals.INPUT_THRESHOLD)
        {
            if (visual.transform.localScale.x == 1)
                handchild.transform.rotation = Quaternion.Euler(0, 0, -1 * 45);
            else handchild.transform.rotation = Quaternion.Euler(0, 0, 45);
        }

        else if (InputManager.GetAxis(GameInput.VERTICAL) > PC2D.Globals.INPUT_THRESHOLD)
        {
            if (visual.transform.localScale.x == 1)
                handchild.transform.rotation = Quaternion.Euler(0, 0, 45);
            else handchild.transform.rotation = Quaternion.Euler(0, 0, -1 * 45);
        }

        else if (InputManager.GetAxis(GameInput.VERTICAL) == 0)
        {
            handchild.transform.rotation = Quaternion.Euler(0, 0, 0);
        }




        if (firerate == 0)
        {
            if (InputManager.GetButton(GameInput.FIRE))
            {

                //playerpos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
                StartShooting();

            }
        }
        else
        {
            if (InputManager.GetButton(GameInput.FIRE) && Time.time > timetofire)
            {

                timetofire = Time.time + 1 / firerate;
                //playerpos = new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.y);
                StartShooting();
            }
            else if (!InputManager.GetButton(GameInput.FIRE))
            {

            }
        }
    }

    public void StartShooting()
    {
        firepointposition = new Vector2(firepoint.position.x, firepoint.position.y);
        GameObject bulle = Instantiate(bullet, firepoint.position, firepoint.rotation);
        bulle.GetComponent<MoveForward>().localscale = visual.transform.localScale.x;
        bulle.GetComponent<MoveForward>().hand = handchild.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // use last state to restore some ladder specific values
        if (_motor.motorState != PlatformerMotor2D.MotorState.FreedomState)
        {
            // try to restore, sometimes states are a bit messy because change too much in one frame
            FreedomStateRestore(_motor);
        }

        // Jump?
        // If you want to jump in ladders, leave it here, otherwise move it down
        if (InputManager.GetButton(GameInput.JUMP))
        {
            _motor.Jump();
            _motor.DisableRestrictedArea();
        }

        _motor.jumpingHeld = InputManager.GetButton(GameInput.JUMP);

        // XY freedom movement
        if (_motor.motorState == PlatformerMotor2D.MotorState.FreedomState)
        {
            // _motor.normalizedXMovement = Input.GetAxis(PC2D.Input.HORIZONTAL);
            // _motor.normalizedYMovement = Input.GetAxis(PC2D.Input.VERTICAL);

            _motor.normalizedXMovement = InputManager.GetAxis(GameInput.HORIZONTAL);
            _motor.normalizedYMovement = InputManager.GetAxis(GameInput.VERTICAL);

            return; // do nothing more
        }

        // X axis movement
        if (Mathf.Abs(InputManager.GetAxis(GameInput.HORIZONTAL)) > PC2D.Globals.INPUT_THRESHOLD)
        {
            _motor.normalizedXMovement = InputManager.GetAxis(GameInput.HORIZONTAL);
        }
        else
        {
            _motor.normalizedXMovement = 0;
        }

        if (InputManager.GetAxis(GameInput.VERTICAL) != 0)
        {
            bool up_pressed = InputManager.GetAxis(GameInput.VERTICAL) > 0;
            if (_motor.IsOnLadder())
            {
                if (
                    (up_pressed && _motor.ladderZone == PlatformerMotor2D.LadderZone.Top)
                    ||
                    (!up_pressed && _motor.ladderZone == PlatformerMotor2D.LadderZone.Bottom)
                 )
                {
                    // do nothing!
                }
                // if player hit up, while on the top do not enter in freeMode or a nasty short jump occurs
                else
                {
                    // example ladder behaviour

                    _motor.FreedomStateEnter(); // enter freedomState to disable gravity
                    _motor.EnableRestrictedArea();  // movements is retricted to a specific sprite bounds

                    // now disable OWP completely in a "trasactional way"
                    FreedomStateSave(_motor);
                    _motor.enableOneWayPlatforms = false;
                    _motor.oneWayPlatformsAreWalls = false;

                    // start XY movement
                    _motor.normalizedXMovement = InputManager.GetAxis(GameInput.HORIZONTAL);
                    _motor.normalizedYMovement = InputManager.GetAxis(GameInput.VERTICAL);
                }
            }
        }
        else if (InputManager.GetAxis(GameInput.VERTICAL) < -PC2D.Globals.FAST_FALL_THRESHOLD)
        {
            _motor.fallFast = false;
        }

        if (InputManager.GetButton(GameInput.FIRE) && !attacking)
        {
            //_motor.Dash();
            //ShootUpdate();
            Meleeattack();
        }

        if (attacking)
        {
            if (attacktimer > 0)
            {
                attacktimer -= Time.deltaTime;
            }
            else
            {
                attacking = false;
                attacktrigger.enabled = false;
            }
        }
    }

    void Meleeattack()
    {
        attacking = true;
        attacktimer = attackcd;

        attacktrigger.enabled = true;


    }

}
