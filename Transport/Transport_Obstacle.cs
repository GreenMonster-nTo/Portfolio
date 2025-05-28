using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport_Obstacle : MonoBehaviour
{
    private const float start_min = -2f;
    private const float start_max = 8f;
    private const float end_min = -6f;
    private const float end_max = 6f;
    private const float reflection_min = -12f;
    private const float reflection_max = 12f;

    private GameObject obstacle_obj;            // 장애물 오브젝트

    private Vector2 start, end;                 // 이동 시작점, 이동 도착점
    private Vector2 reflection;                 // 튕겨나갈 위치 미리 지정

    private float speed;                        // 이동 속도(랜덤)
    private bool spawned;                       // 생성 체크

    public SpriteRenderer obstacle_renderer;    // 장애물 스프라이트 렌더러
    public Sprite[] sprites;                    // 장애물 스프라이트

    private Coroutine move_coroutine;           // 움직이는 코루틴
    private Coroutine hit_coroutine;            // 뽀개지는 애니메이션용 코루틴

    private WaitForSeconds wait;                // 애니메이션 용

    #region Initialize

    private void Awake()
    {
        ObstacleInitialize();
    }

    private void ObstacleInitialize()
    {
        obstacle_obj = this.gameObject;

        start = Vector2.zero;
        end.x = 12f;
        end.y = 6f;
        reflection.x = 0;
        reflection.y = 10.2f;

        wait = new WaitForSeconds(0.1f);
    }

    public void ResetObstacle()
    {
        ReUse();
    }

    // 생성
    public void ObstacleSpawn(int morning_index)
    {
        start.y = Random.Range(start_min, start_max);
        end.y = Random.Range(end_min, end_max);
        end.x = Mathf.Abs(end.x);

        if (morning_index == 0)
        {
            end.x *= -1;
            reflection.x = Random.Range(reflection_min, 0f);
        }
        else
        {
            reflection.x = Random.Range(0f, reflection_max);
        }

        speed = Random.Range(0.08f, 0.15f);
        obstacle_obj.transform.localPosition = start;
        obstacle_renderer.sprite = sprites[0];
        spawned = true;

        move_coroutine = StartCoroutine(Move());
    }

    // 스폰 체크
    public bool GetSpawned() => spawned;

    #endregion

    #region Directions

    Vector2 normal_direction;                   // 방향
    float normal_angle;                         // 방향 앵글
    Vector2 reverse_direction;                  // 방향 (반대)
    float reverse_angle;                        // 방향 (반대) 앵글
    int count;                                  // 플레이어 맞은 횟수

    // 바라보는 방향 설정
    private Quaternion LookAt2D()
    {
        normal_direction = end - start;
        normal_angle = Mathf.Atan2(normal_direction.y, normal_direction.x) * Mathf.Rad2Deg;
        normal_angle -= 180f + 45f;
        return Quaternion.AngleAxis(normal_angle, Vector3.forward);
    }

    // 바라보는 방향(리버스)
    private Quaternion ReverseLookAt2D()
    {
        reverse_direction = end + reflection;
        reverse_angle = Mathf.Atan2(reverse_direction.y, reverse_direction.x) * Mathf.Rad2Deg;
        reverse_angle -= 180f + 45f;
        return Quaternion.AngleAxis(reverse_angle, Vector3.forward);
    }

    // 기본 이동 조건 체크
    private bool CheckMoveReUse()
    {
        if (reflection.x < 0)
        {
            if (obstacle_obj.transform.localPosition.x <= end.x) { return true; }
        }
        else
        {
            if (obstacle_obj.transform.localPosition.x >= end.x) { return true; }
        }
        return false;
    }

    // 기본 이동
    private IEnumerator Move()
    {
        while (spawned)
        {
            obstacle_obj.transform.localPosition = Vector2.MoveTowards(obstacle_obj.transform.localPosition, end, speed);
            obstacle_obj.transform.rotation = LookAt2D();
            if (CheckMoveReUse()) { ReUse(); }
            yield return null;
        }
        StopCoroutine(move_coroutine);
        move_coroutine = null;
    }

    // 튕겨나가는 이동
    private IEnumerator ReflectionMove()
    {
        while (spawned)
        {
            obstacle_obj.transform.localPosition = Vector2.MoveTowards(obstacle_obj.transform.localPosition, reflection, speed * 1.5f);
            obstacle_obj.transform.rotation = ReverseLookAt2D();
            if (obstacle_obj.transform.localPosition.y >= reflection.y) { ReUse(); }
            yield return null;
        }
    }

    // 플레이어에 맞았을 때 뽀개지는 애니메이션
    private IEnumerator HitAnim()
    {
        count = 1;
        while (count < 3)
        {
            obstacle_renderer.sprite = sprites[count];
            yield return wait;
            count++;
        }
        ReUse();
        StopCoroutine(hit_coroutine);
        hit_coroutine = null;
    }

    #endregion

    #region Game Logic

    // 재사용 대기 상태
    private void ReUse()
    {
        spawned = false;
        obstacle_obj.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    // 플레이어와 닿음
    private void Hit_Player()
    {
        if (move_coroutine != null)
        { StopCoroutine(move_coroutine); }
        hit_coroutine = StartCoroutine(HitAnim());
        hit();
    }

    // 막는것과 닿음
    private void Hit_Block()
    {
        if (move_coroutine != null)
        { StopCoroutine(move_coroutine); }
        move_coroutine = StartCoroutine(ReflectionMove());
        block();
    }

    // 플레이어와 닿았을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "Player") { Hit_Player(); }
        if (other.name == "Block") { Hit_Block(); }
    }

    #endregion

    #region Delegates

    public delegate void Hit();
    public static event Hit hit;
    public static event Hit block;

    #endregion
}
