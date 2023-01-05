 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target;
    public float orbitSpeed;
    Vector3 offset;
    
    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {   //���� ȸ��
        //ù��° ����: Ÿ�� ��ġ, �ι�° ����: ȸ����, ����° ����: ȸ�� ��ġ
        //RotateAround�� Ÿ���� �̵��ϸ� �ϱ׷����� ���� ����
        //�̵� ������ offset ���� update����� ��
        transform.position = target.position + offset;
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
        offset = transform.position - target.position;
    }
}
