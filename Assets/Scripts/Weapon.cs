using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };//Melee: 근접공격, Range: 원거리 공격
    public Type type;
    public int dmg;
    public float atkRate;//공속

    public int maxAmmo;//최대 탄창
    public int curAmmo;//현재 탄창

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    //#. Prefab을 저장할 변수와 위치
    public Transform bulletPos;
    public GameObject bullet;

    public Transform bulletCasePos;
    public GameObject bulletCase;

    ////<Coroutine 선언 및 호출법>
    //public void Use()
    //{
    //    if(type == Type.Melee)
    //    {
    //        //StopCoroutine("함수명")을 통해 Coroutine이 중단된 상태에서도 해당 코루틴 중단
    //        StopCoroutine("Swing");
    //        //Coroutine 사용은 일반 함수처럼 사용하지 않고, StartCoroutine("함수명")을 사용해 호출
    //        StartCoroutine("Swing");
    //    }
    //}
    ////Use(Main Routine) -> Swing(Sub Routine) 호출 시 진행 -> Use(Main Routine)로 돌아와 다시 수행
    ////Coroutine이란 동시에 진행됨 => Use + Swing 동시 실행
    ////Coroutine 선언 및 사용법
    ////=>함수에 void를 지우고, IE를 치고, IEnumerator 열거형 함수 클래스를 활용해 사용

    //IEnumerator Swing()//Invoke 이외의 Unity 지원 개념 Coroutine
    //{   
    //    //결과를 산출하는 키워드, Coroutine은 최소 한 개 이상의 yield 필요
    //    //1
    //    yield return new WaitForSeconds(0.1f);//    원하는 시간 대기, 1 -> 1s, 0.1 -> 0.1s
    //    //2
    //    yield return null;//    1프레임 대기
    //    //3
    //    yield return null;//    1프레임 대기

    //    //yield break;//    Coroutine 탈출[주의]
    //}

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if(type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StopCoroutine("Shot");
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        yield return new WaitForSeconds(0.4f);
        meleeArea.enabled = false;
        yield return new WaitForSeconds(0.4f);
        trailEffect.enabled = false;
    }

    int bulletSpeed = 50;

    IEnumerator Shot()
    {
        //#. 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * bulletSpeed;//BulletPos 방향으로 발사

        yield return null;
        //#. 탄피 배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);//튕기듯 연출
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);//회전을 가하는 함수
        //#. 탄피 삭제
    }
}
