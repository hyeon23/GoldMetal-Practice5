using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;

    Vector3 lookVec;
    Vector3 tauntVec;
    public bool isLook;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshes = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        anime = GetComponentInChildren<Animator>();
        nav.isStopped = true;

        StartCoroutine(Think());
    }

    void Update()
    {
        if (isDead)
        {
            StopAllCoroutines();
            return;
        }
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else
            nav.SetDestination(tauntVec);
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int randAction = Random.Range(0, 5);
        switch (randAction)
        {
            //#1. �̻��� ����
            case 0:
            case 1:
                StartCoroutine(MissileShot());
                break;
            //#2. Rock Shot ����
            case 2:
            case 3:
                StartCoroutine(RockShot());
                break;
            //#3. Taunt
            case 4:
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anime.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;
        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;

        yield return new WaitForSeconds(2.0f);
        StartCoroutine(Think());//Next Pattern
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anime.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Think());//Next Pattern
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;
        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        anime.SetTrigger("doTaunt");
        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;
        yield return new WaitForSeconds(1.0f);
        boxCollider.enabled = true;
        nav.isStopped = true;   
        isLook = true;
        StartCoroutine(Think());//Next Pattern
    }
}
