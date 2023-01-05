using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };
    public Type enemyType;

    public int maxHealth;
    public int curHelath;
    public int score;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coins;
    protected Rigidbody rigid;
    protected BoxCollider boxCollider;
    protected MeshRenderer[] meshes;
    protected NavMeshAgent nav;
    protected Animator anime;

    public GameManager gameManager;

    public bool isChase;
    public bool isAttack;
    public bool isDead;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshes = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anime = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
            Invoke("ChaseStart", 2);
    }

    //�ڽ��� �θ� ��ӹ�����, �θ��� Awake�� ������� �ʰ�, �ڽ��� Awake�� ����ȴ�.
    //-Start�� �ٲٴ��� �ƴϸ� �ڽ��� Awake�� ���� �ٿ��ֱ⸦ ����� �Ѵ�.
    private void Update()
    {
        //#NavMeshAgent: AI�� Ȱ���� Nav ���
        //using UnityEngine.AI include �ʼ�
        //NavMeshAgent nav = GetComponent<NavMeshAgnet>();
        //nav.SetDestination: ������ ��ǥ ���� �Լ�
        //NavMeshAgent�� ����ϱ� ���� NavMesh�� �������� ��
        //Window -> AI -> Nevigation -> Bake ����(�� ������ Static�̿��� ��)
        if (nav.enabled && enemyType != Type.D) {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }   
    }

    private void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }

    void Targeting()
    {
        if (!isDead && enemyType != Type.D)
        {
            float targetRadius = 0f;
            float targetRange = 0f;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1.0f;
                    targetRange = 15f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayhits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayhits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anime.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;
                yield return new WaitForSeconds(1f);

                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;
                yield return new WaitForSeconds(2f);

                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantMissile = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidMissile = instantMissile.GetComponent<Rigidbody>();
                rigidMissile.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }

        anime.SetBool("isAttack", false);
        isAttack = false;
        isChase = true;

    }

    void ChaseStart()
    {
        isChase = true;
        anime.SetBool("isWalk", true);
    }

    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;//ȸ���ӵ�
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHelath -= weapon.dmg;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));

        }
        else if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHelath -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        if (!isDead)
        {
            foreach (MeshRenderer mesh in meshes)
                mesh.material.color = Color.red;

            if (curHelath > 0)
            {
                yield return new WaitForSeconds(0.1f);
                foreach (MeshRenderer mesh in meshes)
                    mesh.material.color = Color.white;
            }
            else
            {
                foreach (MeshRenderer mesh in meshes)
                    mesh.material.color = Color.gray;

                gameObject.layer = 12;
                isDead = true;
                isChase = false;
                nav.enabled = false;
                anime.SetTrigger("doDie");
                Player player = target.GetComponent<Player>();
                player.score += score;
                int randomCoin = Random.Range(0, 3);
                Instantiate(coins[randomCoin], transform.position, Quaternion.identity);

                switch (enemyType)
                {
                    case Type.A:
                        gameManager.enemyCntA--;
                        break;
                    case Type.B:
                        gameManager.enemyCntB--;
                        break;
                    case Type.C:
                        gameManager.enemyCntC--;
                        break;
                    case Type.D:
                        gameManager.enemyCntD--;
                        break;
                }

                if (isGrenade)
                {
                    reactVec = reactVec.normalized;
                    reactVec += Vector3.up * 3;
                    rigid.freezeRotation = false;
                    rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                    rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
                }
                else
                {
                    reactVec = reactVec.normalized;
                    reactVec += Vector3.up;
                    rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                }
                Destroy(gameObject, 4);
            }
        }
            

        
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHelath -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }
}
