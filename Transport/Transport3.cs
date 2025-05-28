using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport3 : Transport
{
    private int hit_count;                          // 맞은 횟수 저장

    public GameObject sky_obj;                      // 하늘 오브젝트
    public SpriteRenderer sky_renderer;             // 하늘 스프라이트 렌더러
    public Sprite[] sky_sprites;                    // 하늘 스프라이트들

    private const float player_morning = -0.96f;    // 플레이어 포지션(아침)
    private const float player_night = 1.15f;       // 플레이어 포지션(밤)

    private const float wagon_morning = -0.37f;     // 마차 포지션(아침)
    private const float wagon_night = 0.73f;        // 마차 포지션(밤)

    public GameObject player_obj;                   // 플레이어 오브젝트
    public SpriteRenderer player_renderer;          // 플레이어 스프라이트 렌더러
    public Animator player_anim;                    // 플레이어 애니메이터

    private Vector2 player_pos;                     // 플레이어 포지션

    public GameObject wagon_obj;                    // 마차 오브젝트
    public SpriteRenderer wagon_renderer;           // 마차 스프라이트 렌더러
    public ParticleSystem wagon_particle;           // 마차 파티클

    private Vector2 wagon_start;                    // 마차 시작점
    private Vector2 wagon_direction_end;            // 마차 연출용 도착점
    private Vector2 wagon_particle_pos;             // 마차 파티클 포지션

    public GameObject bg_obj;                       // 배경 오브젝트
    public SpriteRenderer bg_renderer;              // 배경 스프라이트 렌더러
    public Sprite[] bg_sprites;                     // 배경 스프라이트
    private Vector2 bg_pos;                         // 배경 포지션

    public GameObject[] floor_obj;                  // 길 오브젝트
    public SpriteRenderer[] floor_renderer;         // 길 스프라이트 렌더러
    public Sprite[] floor_sprites;                  // 길 스프라이트

    private Vector2[] floor_start;                  // 길 시작점
    private Vector2 floor_end;                      // 길 도착점

    public GameObject obstacle_list;                // 장애물 리스트 부모
    private Vector2 obstacle_list_pos;              // 장애물 리스트 부모 포지션
    private List<GameObject> obstacles;             // 장애물들
    private List<Transport_Obstacle> obstacle_scripts;  // 장애물 스크립트들
    public GameObject obstacle_prf;                 // 장애물 프리팹
    private WaitForSeconds obstacle_wait;           // 장애물 생성 시간

    private Vector2 touch_pos;                      // 터치 포지션

    public GameObject block_obj;                    // 파리채

    private bool hit;                               // 맞았는지 체크

    private WaitForSecondsRealtime direction_wait;  // 연출용 타이머

    private Coroutine hit_coroutine;                // 맞은 코루틴

    private int morning_index;                      // 아침인지 체크를 위한 정수형 변수
    private bool now_transport;                     // 현재 이동중인지 체크

    #region Initialize

    private void Awake()
    {
        TransportInitialize();
        Obstacle_Objectpool_Initialize();
        direction_wait = new WaitForSecondsRealtime(0.5f);
        obstacle_wait = new WaitForSeconds(0.8f);
    }

    // 이동수단 이니셜라이즈
    private void TransportInitialize()
    {
        player_pos = new Vector2(-0.96f, -2.19f);
        wagon_start = new Vector2(0, -2.01f);
        wagon_direction_end = new Vector2(11.5f, -2.01f);
        bg_pos = new Vector2(12.1f, -2.3f);

        floor_start = new Vector2[2];
        floor_start[0] = new Vector2(12.1f, -6.07f);
        floor_start[1] = new Vector2(47.1f, -6.07f);
        floor_end = new Vector2(22.9f, -6.07f);

        obstacle_list_pos = new Vector2(6, 0);
        wagon_particle_pos = new Vector2(3f, -2f);
    }

    // 오브젝트 풀 초기화
    private void Obstacle_Objectpool_Initialize()
    {
        obstacles = new List<GameObject>();
        obstacle_scripts = new List<Transport_Obstacle>();

        for (int i = 0; i < 30; i++)
        {
            GameObject obj = (GameObject)Instantiate(obstacle_prf, obstacle_list.transform);
            obj.SetActive(false);
            obstacles.Add(obj);
            obstacle_scripts.Add(obj.GetComponent<Transport_Obstacle>());
        }
    }

    #endregion

    #region Game Controller

    // 이동 전 그래픽 초기화
    public void ResetTransport(bool morning)
    {
        morning_index = morning ? 0 : 1;
        now_transport = true;
        hit_count = 0;
        hit = false;

        for (int i = 0; i < obstacle_scripts.Count; i++)
        { obstacle_scripts[i].ResetObstacle(); }

        ResetResult();

        SetVectors();
        SetBackground();
        SetWagon();
        SetPlayer();
    }

    // 이동 시작
    public void StartTransport()
    {
        TransportStart();

        StartCoroutine(TouchCheck());
        StartCoroutine(BackgroundMove());
        StartCoroutine(FloorMove());
        StartCoroutine(Obstacle_Activate());
    }

    // 결과 리셋
    private void ResetResult()
    {
        transport_result = true;
    }

    // 포지션 리셋
    private void SetVectors()
    {
        wagon_direction_end.x = Mathf.Abs(wagon_direction_end.x);
        bg_pos.x = Mathf.Abs(bg_pos.x);

        floor_start[0].x = Mathf.Abs(floor_start[0].x);
        floor_start[1].x = Mathf.Abs(floor_start[1].x);
        floor_end.x = Mathf.Abs(floor_end.x);

        obstacle_list_pos.x = Mathf.Abs(obstacle_list_pos.x);
        wagon_particle_pos.x = Mathf.Abs(wagon_particle_pos.x);

        var particle_shape = wagon_particle.shape;

        if (morning_index == 0)
        {
            player_pos.x = player_morning;
            wagon_start.x = wagon_morning;
            floor_end.x *= -1;
            wagon_particle_pos.x *= -1;

            player_renderer.flipX = false;
            wagon_renderer.flipX = false;

            particle_shape.rotation = new Vector3(0, 0, -215f);
        }
        else
        {
            wagon_direction_end.x *= -1;
            player_pos.x = player_night;
            wagon_start.x = wagon_night;
            bg_pos.x *= -1;
            floor_start[0].x *= -1;
            floor_start[1].x *= -1;
            obstacle_list_pos.x *= -1;

            player_renderer.flipX = true;
            wagon_renderer.flipX = true;

            particle_shape.rotation = Vector3.zero;
        }
    }

    // 배경 스프라이트 리셋
    private void SetBackground()
    {
        sky_renderer.sprite = sky_sprites[morning_index];

        bg_renderer.sprite = bg_sprites[morning_index];
        bg_obj.transform.localPosition = bg_pos;

        for (int i = 0; i < 2; i++)
        {
            floor_renderer[i].sprite = floor_sprites[morning_index];
            floor_obj[i].transform.localPosition = floor_start[i];
        }

        obstacle_list.transform.localPosition = obstacle_list_pos;
    }

    // 플레이어 리셋
    private void SetPlayer()
    {
        player_obj.transform.position = player_pos;
        player_anim.SetBool("hit", false);
        player_anim.SetBool("block", false);

        block_obj.SetActive(false);
        block_obj.transform.localPosition = player_pos;
    }

    // 왜건 리셋
    private void SetWagon()
    {
        wagon_obj.transform.localPosition = wagon_start;
        wagon_particle.transform.localPosition = wagon_particle_pos;
    }

    // 이동 끝
    public void EndTransport()
    {
        StopAllCoroutines();

        if (hit_coroutine != null)
        { hit_coroutine = null; }

        UIManager.manager.NoTouch(true);
        now_transport = false;

        block_obj.SetActive(false);

        TransportPause();
        StartCoroutine(TransportEndDirection());
    }
    #endregion

    #region Direction

    // 시작지점 이용해 도착지점 설정
    private Vector2 GetEndPos(Vector2 position)
    {
        return new Vector2(position.x * -1, position.y);
    }

    // 뒷배경 움직임
    private IEnumerator BackgroundMove()
    {
        Vector2 moveVec = GetEndPos(bg_pos);
        while (transporting)
        {
            bg_obj.transform.localPosition = new Vector2(Mathf.Lerp(bg_obj.transform.localPosition.x, moveVec.x, 0.08f * Time.deltaTime), bg_pos.y);
            yield return null;
        }
    }

    // 길 무한반복을 위한 틈 보간
    private Vector2 GetFloorTransform()
    {
        Vector2 pos;
        float x;
        if (Mathf.Abs(floor_obj[0].transform.localPosition.x) < Mathf.Abs(floor_obj[1].transform.localPosition.x))
            x = morning_index == 0 ? floor_obj[0].transform.localPosition.x + (35f - 0.08f) : floor_obj[0].transform.localPosition.x - (35f - 0.08f);
        else
            x = morning_index == 0 ? floor_obj[1].transform.localPosition.x + (35f - 0.08f) : floor_obj[1].transform.localPosition.x - (35f - 0.08f);

        pos = new Vector2(x, floor_end.y);
        return pos;
    }

    // 길 움직임이 한계를 벗어났는지 체크
    private bool CheckFloor(Vector2 pos)
    {
        if (pos.x <= floor_end.x && morning_index == 0) return true;
        if (pos.x >= floor_end.x && morning_index == 1) return true;
        return false;
    }

    // 길 움직임
    private IEnumerator FloorMove()
    {
        while (transporting)
        {
            for (int i = 0; i < 2; i++)
            {
                floor_obj[i].transform.localPosition = Vector2.MoveTowards(floor_obj[i].transform.localPosition, floor_end, 0.08f);
                if (CheckFloor(floor_obj[i].transform.localPosition)) floor_obj[i].transform.localPosition = GetFloorTransform();
            }
            yield return null;
        }
    }

    // 이동 끝나고 나오는 연출
    private IEnumerator TransportEndDirection()
    {
        while (Vector2.Distance(wagon_obj.transform.localPosition, wagon_direction_end) > 0.3f)
        {
            wagon_obj.transform.localPosition = Vector2.MoveTowards(wagon_obj.transform.localPosition, wagon_direction_end, 0.3f);
            yield return null;
        }
        yield return direction_wait;

        UIManager.manager.NoTouch(false);
        TransportEnd();
    }

    #endregion

    #region Game Logic

    // 장애물 활성화
    private IEnumerator Obstacle_Activate()
    {
        int count = 0;
        while (transporting)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (!obstacles[i].gameObject.activeInHierarchy && !obstacle_scripts[i].GetSpawned() && count <= 5)
                {
                    obstacles[i].gameObject.SetActive(true);
                    obstacle_scripts[i].ObstacleSpawn(morning_index);
                    count++;
                }
            }
            yield return obstacle_wait;
            count = 0;
        }
    }

    // 막는 오브젝트의 각도 설정
    private Quaternion GetRotate()
    {
        Vector2 direction = touch_pos - (Vector2)this.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        angle -= 90;

        if (morning_index == 0)
        {
            angle = angle < -195f ? -195f : angle;
            angle = angle > 15f ? 15f : angle;
        }
        else
        {
            if (angle > -90) angle = angle < -15f ? -15f : angle;
            else angle = angle > -165f ? -165f : angle;
        }

        Quaternion um_rotate = Quaternion.AngleAxis(angle, Vector3.forward);

        return um_rotate;
    }

    // 막는 오브젝트 위치 변경
    private void BlockObjMove()
    {
        block_obj.SetActive(true);
        block_obj.transform.localRotation = GetRotate();
    }

    // 맞았을 때
    private void Hit()
    {
        hit_count++;
        TransportMood(-2);

        hit = true;
        player_anim.SetBool("hit", hit);
        if (hit_coroutine != null)
        { StopCoroutine(hit_coroutine); }
        hit_coroutine = StartCoroutine(HitTimer());
    }

    // 막았을 때
    private void Block()
    {
        TransportMoney(25);
    }

    // 맞은거 타이머로 초기화
    private IEnumerator HitTimer()
    {
        yield return direction_wait;
        hit = false;
        player_anim.SetBool("hit", hit);
    }

    // 터치 입력 들어오는지 체크
    private IEnumerator TouchCheck()
    {
        while (transporting)
        {
            if (Input.GetMouseButton(0))
            {
                player_anim.SetBool("block", true);
                touch_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                BlockObjMove();
            }
            if (Input.GetMouseButtonUp(0))
            {
                block_obj.SetActive(false);
                player_anim.SetBool("block", false);
            }

            yield return null;
        }
    }

    #endregion

    #region Delegates

    private void OnEnable() => DeletageSet();
    private void OnDisable() => DeletageDel();

    private void DeletageSet()
    {
        Transport_Obstacle.hit += Hit;
        Transport_Obstacle.block += Block;
    }
    private void DeletageDel()
    {
        Transport_Obstacle.hit -= Hit;
        Transport_Obstacle.block += Block;
    }

    #endregion
}
