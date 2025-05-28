using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Transport_Feather : MonoBehaviour, IPointerClickHandler
{
    private const float y_min = -8f;
    private const float y_max = 8f;
    private const float x_pos = -12f;

    private GameObject feather_obj;             // 장애물 오브젝트
    private Vector2 start, end;                 // 이동 시작점, 이동 도착점
    private float speed;                        // 이동 속도(랜덤)
    private bool spawned;                       // 생성 체크

    private Coroutine move_coroutine;           // 움직이는 코루틴

    private Vector2 direction;                  // 바라보는 방향
    private float angle;                        // 바라보는 방향 float

    private void Awake()
    {
        FeatherInitialize();
    }

    // 초기화
    public void FeatherInitialize()
    {
        feather_obj = this.gameObject;

        start = Vector2.zero;
        end.x = -12f;
        end.y = 6f;
    }

    // 생성
    public void FeatherSpawn()
    {
        start.y = Random.Range(y_min, y_max);
        end.y = Random.Range(y_min, y_max);
        end.x = x_pos;

        speed = Random.Range(0.08f, 0.15f);
        feather_obj.transform.localPosition = start;

        spawned = true;

        move_coroutine = StartCoroutine(Move());
    }

    // 스폰되었는지 체크하는 함수 받아오기
    public bool GetSpawned()
    {
        return spawned;
    }

    // 바라보는 방향 설정
    private Quaternion LookAt2D()
    {
        direction = end - start;
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle -= 180f;
        return Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // 깃털 이동
    private IEnumerator Move()
    {
        while (spawned)
        {
            this.transform.localPosition = Vector2.MoveTowards(this.transform.localPosition, end, speed);
            this.transform.rotation = LookAt2D();
            if (this.transform.localPosition.x <= end.x) { ReUse(); }
            yield return null;
        }
        StopCoroutine(move_coroutine);
    }

    // 재사용 대기 상태
    private void ReUse()
    {
        spawned = false;
        this.gameObject.SetActive(false);
    }

    // 수집
    public void Collected()
    {
        ReUse();
        collect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Collected();
    }

    // 플레이어와 닿았을 경우
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Dust") Collected();
    }

    public delegate void collection();
    public static event collection collect;
}
