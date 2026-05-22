using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Falldown_item : MonoBehaviour
{
    [SerializeField] private bool isFile = true;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float destroyY = -10f;

    void Update()
    {
        // 아래로 떨어지기
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        // 화면 밖으로 나가면 삭제
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    public void SetFallSpeed(float speed)
    {
        fallSpeed = speed;
    }

    public bool IsFile()
    {
        return isFile;
    }
}
