using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };//Melee: ��������, Range: ���Ÿ� ����
    public Type type;
    public int dmg;
    public float atkRate;//����

    public int maxAmmo;//�ִ� źâ
    public int curAmmo;//���� źâ

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    //#. Prefab�� ������ ������ ��ġ
    public Transform bulletPos;
    public GameObject bullet;

    public Transform bulletCasePos;
    public GameObject bulletCase;

    ////<Coroutine ���� �� ȣ���>
    //public void Use()
    //{
    //    if(type == Type.Melee)
    //    {
    //        //StopCoroutine("�Լ���")�� ���� Coroutine�� �ߴܵ� ���¿����� �ش� �ڷ�ƾ �ߴ�
    //        StopCoroutine("Swing");
    //        //Coroutine ����� �Ϲ� �Լ�ó�� ������� �ʰ�, StartCoroutine("�Լ���")�� ����� ȣ��
    //        StartCoroutine("Swing");
    //    }
    //}
    ////Use(Main Routine) -> Swing(Sub Routine) ȣ�� �� ���� -> Use(Main Routine)�� ���ƿ� �ٽ� ����
    ////Coroutine�̶� ���ÿ� ����� => Use + Swing ���� ����
    ////Coroutine ���� �� ����
    ////=>�Լ��� void�� �����, IE�� ġ��, IEnumerator ������ �Լ� Ŭ������ Ȱ���� ���

    //IEnumerator Swing()//Invoke �̿��� Unity ���� ���� Coroutine
    //{   
    //    //����� �����ϴ� Ű����, Coroutine�� �ּ� �� �� �̻��� yield �ʿ�
    //    //1
    //    yield return new WaitForSeconds(0.1f);//    ���ϴ� �ð� ���, 1 -> 1s, 0.1 -> 0.1s
    //    //2
    //    yield return null;//    1������ ���
    //    //3
    //    yield return null;//    1������ ���

    //    //yield break;//    Coroutine Ż��[����]
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
        //#. �Ѿ� �߻�
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * bulletSpeed;//BulletPos �������� �߻�

        yield return null;
        //#. ź�� ����
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);//ƨ��� ����
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);//ȸ���� ���ϴ� �Լ�
        //#. ź�� ����
    }
}
