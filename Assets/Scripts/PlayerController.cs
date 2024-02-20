using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform viewPoint;
    [SerializeField] float mouseSenstivity = 0.5f;
    [SerializeField] float moveSpeed = 5f, runSpeed = 8f;
    private float activeMoveSpeed;
    [SerializeField] CharacterController charCon;
    private Vector3 moveDir, movement;
    private float verticalRotStore;
    private Vector2 mouseInput;
    private Camera cam;
    [SerializeField] float jumpForce = 12f, gravityMod = 2.5f;
    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayers;
    [SerializeField] GameObject bulletImpact;
    // [SerializeField] float timeBetweenShots = 0.1f;
    private float shotCounter;
    private float maxHeat = 10f, /* heatperShot = 1f,*/ coolRate = 4f, overHeatCoolRate = 5;
    private float heatCounter;
    private bool overHeated;
    private bool canZoom;
    public Gun[] allGuns;
    private int selectedGun;
    private float muzzleDisplayTime = 0.2f;
    private float muzzleCounter;
    private bool isShooting;
    private Image zoomBtnImage;
    private float screenWidth;
    public GameObject playerHitimpact;
    public int maxHealth;
    private int currentHealth;
    public Animator anim;
    public GameObject playerModel;
    public Transform modelGunPoint, gunHolder;
    public Material[] allSkins;
    public float adsSpeed = 5f;
    public Transform adsInPoint, adsOutPoint;
    public AudioSource footstepSlow, footstepFast;
    public FixedJoystick joyStick;
    public GameObject miniMap;
    [SerializeField] private Image selfTag;
    [SerializeField] private Image otherTag;
    void Start()
    {
        currentHealth = maxHealth;
        cam = Camera.main;
        UIController.instance.weaponTempSlider.maxValue = maxHeat;
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        if (photonView.IsMine)
        {
            screenWidth = Screen.width;
            UIController.instance.shootBtn.onClick.AddListener(OnClickShootButton);
            UIController.instance.jumpBtn.onClick.AddListener(OnClickJumpButton);
            UIController.instance.cameraZoomButton.onClick.AddListener(AdsZoom);
            UIController.instance.gunSwitchingButton.onClick.AddListener(GunSwitching);
            zoomBtnImage = UIController.instance.cameraZoomButton.GetComponent<Image>();
            playerModel.SetActive(false);
            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;
            miniMap.SetActive(true);
            otherTag.gameObject.SetActive(false);
            selfTag.gameObject.SetActive(true);
        }
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
            selfTag.gameObject.SetActive(false);
            otherTag.gameObject.SetActive(true);
        }
        playerModel.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber % allSkins.Length];
    }


    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (allGuns[selectedGun].muzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter <= 0)
            {
                allGuns[selectedGun].muzleFlash.SetActive(false);
            }
        }


        if (heatCounter < 0)
        {
            heatCounter = 0;
        }
        UIController.instance.weaponTempSlider.value = heatCounter;
        Debug.Log("Grounded: " + isGrounded + " Speed " + moveDir.magnitude);
        anim.SetBool("grounded", isGrounded);
        anim.SetFloat("speed", moveDir.magnitude);
        HandleGunTemperature();
#if UNITY_EDITOR

        PlayerMove();
        ScreenRotatioin();

#elif UNITY_ANDROID       

        GetMobileTouchPositions();

#else

        Debug.Log("Any other platform");

#endif

    }


    public void GetMobileTouchPositions()
    {
        PlayerMove();
        Touch touch = Input.GetTouch(0);
        if (touch.position.x < (screenWidth / 2))
        {
            joyStick.gameObject.SetActive(true);
        }
        else
        {
            joyStick.gameObject.SetActive(false);
            ScreenRotatioin();
        }
    }
    public void ScreenRotatioin()
    {
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSenstivity;
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);
        viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
    }


    public void OnClickShootButton()
    {

        if (!overHeated)
        {
            if (!UIController.instance.isPaused)
            {
                Shoot();
            }
            if (allGuns[selectedGun].isAutomatic && UIController.instance.isPaused)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0)
                {
                    Shoot();
                }
            }

        }

    }

    public void HandleGunTemperature()
    {
        if (!overHeated)
        {
            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overHeatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {

                overHeated = false;
                UIController.instance.overHeatedMsg.gameObject.SetActive(false);
            }
        }
    }
    public void AdsZoom()
    {
        canZoom = !canZoom;
        if (canZoom)
        {
            zoomBtnImage.color = Color.red;
            StartCoroutine(ZoomEffect(cam.fieldOfView, allGuns[selectedGun].adsZoom, gunHolder.position, adsInPoint.position));
        }
        else
        {
            zoomBtnImage.color = Color.white;
            StartCoroutine(ZoomEffect(cam.fieldOfView, 60f, gunHolder.position, adsOutPoint.position));
        }

    }
    private IEnumerator ZoomEffect(float startFOV, float endFov, Vector3 startPos, Vector3 endPos)
    {
        float elapsedTime = 0f;
        while (elapsedTime < adsSpeed)
        {
            cam.fieldOfView = Mathf.Lerp(startFOV, endFov, elapsedTime / adsSpeed);
            gunHolder.position = Vector3.Lerp(startPos, endPos, elapsedTime / adsSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;

        }
    }
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (MatchManager.instance.state == MatchManager.GameState.Playing)
            {
                cam.transform.position = viewPoint.transform.position;
                cam.transform.rotation = viewPoint.transform.rotation;
            }
            else
            {
                cam.transform.position = MatchManager.instance.mapCamPint.position;
                cam.transform.rotation = MatchManager.instance.mapCamPint.rotation;

            }
        }
    }

    void GunSwitching()
    {
       
            selectedGun++;
            if (selectedGun >= allGuns.Length)
            {
                selectedGun = 0;
            }
         
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
    }
    void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzleFlash.SetActive(false);
    }
    [PunRPC]
    public void SetGun(int gunToSwitch)
    {
        if (gunToSwitch < allGuns.Length)
        {
            selectedGun = gunToSwitch;
            SwitchGun();
        }
    }
    void CursorLockUnlock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (isShooting && !UIController.instance.optionScreen.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
    void PlayerMove()
    {
        moveDir = new Vector3(joyStick.Horizontal, 0, joyStick.Vertical);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
            if (!footstepFast.isPlaying && moveDir != Vector3.zero)
            {
                footstepFast.Play();
                footstepSlow.Stop();
            }
        }
        else
        {
            activeMoveSpeed = moveSpeed;
            if (!footstepSlow.isPlaying && moveDir != Vector3.zero)
            {
                footstepFast.Stop();
                footstepSlow.Play();
            }
        }
        if (moveDir == Vector3.zero || !isGrounded)
        {
            footstepSlow.Stop();
            footstepFast.Stop();
        }
        float yVel = movement.y;
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
        movement.y = yVel;
        if (charCon.isGrounded)
        {
            movement.y = 0;
        }

        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers);

        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        charCon.Move(movement * Time.deltaTime);
    }

    public void OnClickJumpButton()
    {
        if (isGrounded)
        {
            movement.y = jumpForce;
        }
        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        charCon.Move(movement * Time.deltaTime);
    }
    private void Shoot()
    {
        allGuns[selectedGun].lineRenderer.SetPosition(0, allGuns[selectedGun].muzleFlash.transform.position);
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            allGuns[selectedGun].lineRenderer.SetPosition(1, hit.point);

            if (hit.collider.gameObject.tag == "Player")
            {
                Debug.Log("hit: " + hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(playerHitimpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {

                GameObject bulletImpactObj = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletImpactObj, 10f);
            }
        }
        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
            UIController.instance.overHeatedMsg.gameObject.SetActive(true);
        }
        allGuns[selectedGun].muzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
        allGuns[selectedGun].shotSound.Stop();
        allGuns[selectedGun].shotSound.Play();
    }
    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        TakeDamage(damager, damageAmount, actor);
    }
    public void TakeDamage(string damager, int damageAmount, int actor)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damageAmount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                PlayerSpawner.instance.Die(damager);
                MatchManager.instance.UpdateStatSend(actor, 0, 1);
            }
            UIController.instance.healthSlider.value = currentHealth;
        }
    }
}
