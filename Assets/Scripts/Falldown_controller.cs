using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Falldown_controller : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float tiltSensitivity = 3f;

    [Header("Boundary Settings")]
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;

    [Header("Collision Settings")]
    [SerializeField] private float catchRadius = 1f;

    private Falldown_Manager gameManager;
    private float targetX;

    void Start()
    {
        gameManager = FindObjectOfType<Falldown_Manager>();

        // 자이로스코프 활성화
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }

        targetX = transform.position.x;
    }

    void Update()
    {
        float tilt;

#if UNITY_EDITOR
        // 에디터: 키보드 입력
        tilt = Input.GetAxis("Horizontal") * tiltSensitivity;
#else
            // 모바일: 가속도계
            tilt = Input.acceleration.x * tiltSensitivity;
#endif

        // 목표 위치 계산
        targetX += tilt * moveSpeed * Time.deltaTime;
        targetX = Mathf.Clamp(targetX, minX, maxX);

        // 부드럽게 이동
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * 10f);
        transform.position = newPosition;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Falldown_item item = collision.GetComponent<Falldown_item>();
        if (item != null)
        {
            gameManager.OnItemCaught(item.IsFile());
            Destroy(collision.gameObject);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(minX, transform.position.y, 0),
                        new Vector3(minX, transform.position.y + 2, 0));
        Gizmos.DrawLine(new Vector3(maxX, transform.position.y, 0),
                        new Vector3(maxX, transform.position.y + 2, 0));
    }
}
