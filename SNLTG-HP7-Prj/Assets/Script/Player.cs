using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameObject LanderFeed;

    [SerializeField]
    private Transform FeedOpenPosition;
    [SerializeField]
    private Transform FeedClosePosition;
    private AudioSource AudioSource;
    [SerializeField]
    private AudioClip explosionSound;
    [SerializeField]
    // private AudioClip popSound;
    private LevelManager levelManager;
    private Rigidbody2D RigidBody;
    private Animator AnimationsContainer;
    [SerializeField]
    private Transform LanderCheck;
    [SerializeField]
    private float LanderCheckRadius = 1f;
    [SerializeField]
    private bool isGoingLand = false;
    //Movement Variables
    private Vector2 Velocity;
    public float CurrentVelocity;
    private float VelocityX;
    private float VelocityY;
    //Input Variables
    private int inputUp;
    private int inputDown;
    private int inputLeft;
    private int inputRight;
    //Bools
    private bool canOpenLandFeed = false;
    private bool canCloseLandFeed = false;
    private bool isLanded = false;
    private float collisionTimer = 0f;
    [SerializeField] private float EnginePower = 2.2f;
    public FuelBar fuelBar;
    public float FuelTank = 100f;
    public float FuelTankMax = 100f;
    [SerializeField] private float VelocityLimit = 12f;
    bool isFuelEmpty = false;
    private float LandedTime = 0;
    private float LandedTimeMax = 3;
    public bool isWin { get; private set; } = false;
    public bool isLose { get; private set; } = false;

    private GameObject[] WinUI;
    private GameObject[] LoseUI;
    // [SerializeField]
    void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();
        RigidBody = GetComponent<Rigidbody2D>();
        AnimationsContainer = GetComponentInChildren<Animator>();
        WinUI = GameObject.FindGameObjectsWithTag("WinUI");
        LoseUI = GameObject.FindGameObjectsWithTag("LoseUI");
        DisableUI();
        DisableAllFlame();
        fuelBar.SetMaxFuel(FuelTankMax);
        AudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isLose || isWin)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (isLose)
                {
                    levelManager.ReloadLevel();
                }
                if (isWin)
                {
                    levelManager.LoadNextLevel();
                }
            }
        }
    }
    void FixedUpdate()
    {
        if (isLose == true || isWin == true)
        {
            return;
        }
        CheckCurrentVelocity();
        CheckFuel();
        if (!isFuelEmpty)
        {
            CheckVFx();
            CheckFlame();
            ApplyForce();
            ApplyRotation();
        }
        else
        {
            AudioSource.Stop();
            DisableAllFlame();
        }
        CheckIsGoingtoLand();
        CheckLandFeed();
        CheckWin();
    }
    //Engine Functions

    public void ApplyForce()
    {
        VelocityX = RigidBody.velocity.x;
        VelocityY = RigidBody.velocity.y;
        float RotationZ = transform.rotation.eulerAngles.z;
        float RotationZRad = RotationZ * Mathf.Deg2Rad;
        float VelocityYAdd = (float)(EnginePower * (inputUp + inputDown * 0.55) * Mathf.Cos(RotationZRad) + EnginePower * inputLeft * Mathf.Cos(RotationZRad) / 3 + EnginePower * inputRight * Mathf.Cos(RotationZRad) / 3);
        float VelocityXAdd = (float)(EnginePower * (inputUp + inputDown * 0.55) * Mathf.Sin(RotationZRad) + EnginePower * inputLeft * Mathf.Sin(RotationZRad) / 3 + EnginePower * inputRight * Mathf.Sin(RotationZRad) / 3);
        VelocityX = VelocityX - VelocityXAdd;
        VelocityY = VelocityY + VelocityYAdd;
        Velocity = new Vector2(VelocityX, VelocityY);
        RigidBody.velocity = Velocity;
        FuelTank = (float)(FuelTank - inputUp - inputDown * 0.55 - (inputLeft + inputRight) / 3);
    }
    //Fuel check
    public void CheckFuel()
    {
        fuelBar.SetFuel(FuelTank);

        if (isFuelEmpty == true)
        {
            collisionTimer += Time.deltaTime;
            if (collisionTimer > 5 && isWin == false)
            {
                Lose();
            }
        }
        if (FuelTank > 100)
        {
            FuelTank = 100;
        }
        if (FuelTank <= 0)
        {
            FuelTank = 0;
            isFuelEmpty = true;
        }
    }
    public void CheckCurrentVelocity()
    {
        CurrentVelocity = RigidBody.velocity.magnitude;

        if (CurrentVelocity > VelocityLimit)
        {
            canOpenLandFeed = false;
        }
    }
    //If landed is true more thanh 3s print win
    public void CheckWin()
    {
        if (isLanded == true)
        {
            LandedTime = LandedTime + Time.deltaTime;
            if (LandedTime > LandedTimeMax)
            {
                isWin = true;
                AudioSource.Stop();
                EnableTagObject(WinUI);
            }
        }
    }
    //Check functions
    public void CheckIsGoingtoLand()
    {
        if (Physics2D.OverlapCircle(LanderCheck.position, LanderCheckRadius, LayerMask.GetMask("LandingPad")))
        {
            isGoingLand = true;
            if (isLanded == false && CurrentVelocity < VelocityLimit)
            {
                canOpenLandFeed = true;
            }

        }
        else
        {
            isGoingLand = false;
        }
    }
    public void CheckLandFeed()
    {
        if (canOpenLandFeed && isGoingLand)
        {

            OpenLandFeed();
            isLanded = true;
            canCloseLandFeed = true;

        }
        if (canCloseLandFeed && !isGoingLand)
        {
            CloseLandFeed();
            isLanded = false;
            canOpenLandFeed = false;
        }
    }

    //Rotation Functions
    private float RotationAngle = 0;
    public void ApplyRotation()
    {
        float RotationZ = transform.rotation.eulerAngles.z;
        if (Mathf.Abs(inputLeft) - Mathf.Abs(inputRight) == 0)
        {
            if (Mathf.Abs(RotationAngle) < 0.05)
            {
                RotationAngle = 0;
            }
            RotationAngle = (float)(RotationAngle * 0.7);
            RotationZ = RotationZ + RotationAngle;
            transform.rotation = Quaternion.Euler(0, 0, RotationZ);
            return;
        }
        if (inputLeft == 1)
        {
            RotationAngle = -2;
            RotationZ = RotationZ + RotationAngle;
        }
        if (inputRight == 1)
        {
            RotationAngle = 2;
            RotationZ = RotationZ + RotationAngle;
        }
        transform.rotation = Quaternion.Euler(0, 0, RotationZ);
    }

    //Child affect functions
    private void EnableChildObject(string ChildName)
    {
        Transform Child = transform.Find(ChildName);
        if (Child != null)
        {
            Child.gameObject.SetActive(true);
        }
    }
    private void DisableChildObject(string ChildName)
    {
        Transform Child = transform.Find(ChildName);
        if (Child != null)
        {
            Child.gameObject.SetActive(false);
        }
    }
    // private void EnableUIObjects()
    // {
    //     EnableTagObject("WinUI");
    //     EnableTagObject("LoseUI");
    // }


    private void DisableTagObject(GameObject[] TagObjects)
    {
        foreach (GameObject TagObject in TagObjects)
        {
            TagObject.SetActive(false);
        }
    }
    private void DisableUI()
    {
        DisableTagObject(WinUI);
        DisableTagObject(LoseUI);
    }
    private void EnableTagObject(GameObject[] TagObjects)
    {
        foreach (GameObject TagObject in TagObjects)
        {
            TagObject.SetActive(true);
        }
    }
    //Disable all flame
    private void DisableAllFlame()
    {
        DisableChildObject("Sprite_Flame_Mid");
        DisableChildObject("Sprite_Flame_Right");
        DisableChildObject("Sprite_Flame_Left");
    }
    private void OpenLandFeed()
    {
        if (canOpenLandFeed)
            LanderFeed.transform.localPosition = Vector3.MoveTowards(LanderFeed.transform.localPosition, FeedOpenPosition.localPosition, 0.1f);
    }
    private void CloseLandFeed()
    {
        LanderFeed.transform.localPosition = Vector3.MoveTowards(LanderFeed.transform.localPosition, FeedClosePosition.localPosition, 0.1f);
    }
    //Freeze player
    public void FreezePlayer()
    {
        RigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
    }
    //Input Functions
    public void OnUP(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputUp = 1;
        }
        if (context.canceled)
        {
            inputUp = 0;
        }
    }
    public void OnLEFT(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputLeft = 1;
        }
        if (context.canceled)
        {
            inputLeft = 0;
        }
    }
    public void OnRIGHT(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputRight = 1;
        }
        if (context.canceled)
        {
            inputRight = 0;
        }
    }
    public void OnDOWN(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputDown = 1;
        }
        if (context.canceled)
        {
            inputDown = 0;
        }
    }
    //Sprite_Flame controller
    public void CheckFlame()
    {
        if (inputUp == 1 || inputDown == 1)
        {
            EnableChildObject("Sprite_Flame_Mid");
        }
        else
        {
            DisableChildObject("Sprite_Flame_Mid");
        }
        if (inputLeft == 1)
        {
            EnableChildObject("Sprite_Flame_Left");
        }
        else
        {
            DisableChildObject("Sprite_Flame_Left");
        }
        if (inputRight == 1)
        {
            EnableChildObject("Sprite_Flame_Right");
        }
        else
        {
            DisableChildObject("Sprite_Flame_Right");
        }
    }
    public void CheckVFx()
    {
        if ((inputUp + inputDown + inputLeft + inputRight > 0))
        {
            if (!AudioSource.isPlaying)
                AudioSource.Play();
        }
        else
        {
            AudioSource.Stop();
        }
    }
    //Collecting Functions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Fuel"))
        {
            // AudioSource.PlayOneShot(popSound);
            FuelTank = FuelTank + 20;
            Destroy(collision.gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rock") || collision.gameObject.CompareTag("LandingPad"))
        {
            if (CurrentVelocity > VelocityLimit)
            {
                Lose();
            }
        }
    }

    private void Lose()
    {
        AudioSource.Stop();
        isLose = true;
        AnimationsContainer.SetBool("isLose", true);
        AudioSource.PlayOneShot(explosionSound, 0.7F);
        DisableChildObject("Lander_Feed");
        if (isFuelEmpty == false)
            FreezePlayer();
        EnableTagObject(LoseUI);
    }
}
