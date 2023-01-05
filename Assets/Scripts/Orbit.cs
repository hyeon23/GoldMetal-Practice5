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
    {   //주위 회전
        //첫번째 인자: 타겟 위치, 두번째 인자: 회전축, 세번째 인자: 회전 수치
        //RotateAround는 타겟이 이동하면 일그러지는 단점 존재
        //이동 전후의 offset 값을 update해줘야 함
        transform.position = target.position + offset;
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
        offset = transform.position - target.position;
    }
}
