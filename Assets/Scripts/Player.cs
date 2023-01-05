using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Camera followCamera;
    public GameManager manager;

    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    float hAxis;
    float vAxis;

    bool wDown;
    bool jDown;
    bool iDown;
    bool sDown1;//Swap Down
    bool sDown2;
    bool sDown3;
    bool fDown;//Fire
    bool rDown;//Reload
    bool gDown;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isFireReady = true;
    bool isReload;
    bool isBorder;
    bool isDamage;
    bool isShop;
    bool isDead;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anime;
    Rigidbody rigid;
    MeshRenderer[] meshs;

    public float speed;
    public float jumpPower;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenades;
    public GameObject grenadeObj;  

    GameObject nearObject;
    public Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay;

    public AudioSource jumpSound;//소리넣는법

    private void Awake()
    {
        //자식의 Component를 가져오는 것
        anime = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>();//GetComponen S이다.//모든 자식 Component를 가져옴

        PlayerPrefs.SetInt("MaxScore", 12345); //유니티에서 제공하는 간단 Save 기능
        Debug.Log(PlayerPrefs.GetInt("MaxScore"));
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interaction();
    }

    private void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;//회전속도
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }

    void GetInput()
    {

        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButton("Fire2");
        rDown = Input.GetButtonDown("Reload");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || !isFireReady || isReload || isDead)
            moveVec = Vector3.zero;

        if (!isBorder)
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

        anime.SetBool("isRun", moveVec != Vector3.zero);
        anime.SetBool("isWalk", wDown);
    }

    void Turn()
    {   //#1. 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //#2. 마우스에 의한 회전
        if (fDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);//스크린에서 월드로 Ray를 쏘는 함수
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))//out: return처럼 반환 값을 주어진 변수에 저장하는 키워드
            {
                //floor에 닿으면 rayHit의 결과를 rayHit에 저장해줌
                Vector3 nextVec = rayHit.point - transform.position;//rayHit.point는 Ray가 뻗어나가 최초로 닿은 점
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isSwap && !isShop && !isDead) //#.Ver2 && !isDodge
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anime.SetBool("isJump", true);
            anime.SetTrigger("doJump");
            isJump = true;

            jumpSound.Play();//So Simple
        }
    }

    void Grenade()
    {
        if (hasGrenades == 0)
            return;

        if(gDown && !isReload && !isSwap && !isDodge && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);
                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null)
            return;
        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.atkRate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap && !isShop && !isDead)
        {
            equipWeapon.Use();
            anime.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (ammo == 0)
            return;
        if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isShop && !isDead)
        {
            anime.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 3f);//장전 시간
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isShop && !isDead)
        {
            dodgeVec = moveVec;
            speed *= 2f;
            anime.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.6f);
        }
    }

    void DodgeOut()
    {
        isDodge = false;
        speed *= 0.5f;
    }

    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) &&  !isJump && !isDodge && !isShop && !isDead)
        {
            if (equipWeapon != null)//초기 값 null 처리
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anime.SetTrigger("doSwap");
            
            isSwap = true;
            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interaction()
    {
        if (iDown && nearObject != null && !isJump && !isDodge && !isShop && !isDead)
        {
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;
                Destroy(nearObject);
            }
            else if(nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }  
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
            anime.SetBool("isJump", false);
            isJump = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades)
                        hasGrenades = maxHasGrenades;
                    break;

            }
            Destroy(other.gameObject);
        }
        else if(other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAtk = other.name == "Boss Melee Area";

                StartCoroutine(OnDamage(isBossAtk));
            }
            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }

        if (isBossAtk)
            rigid.AddForce(transform.forward * (-25), ForceMode.Impulse);

        if (health <= 0 && !isDead)
            OnDie();

        yield return new WaitForSeconds(1f);//무적시간

        isDamage = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }

        if (isBossAtk)
            rigid.velocity = Vector3.zero;   
    }

    public void OnDie()
    {
        anime.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
        else if(other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
            
    }
}
