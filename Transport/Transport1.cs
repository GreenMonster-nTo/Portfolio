using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Transport1 : Transport, IPointerClickHandler
{
    private int touch_amount;                       // 저장 될 터치 횟수

    public GameObject player_obj;                   // 플레이어 오브젝트
    public SpriteRenderer player_renderer;          // 플레이어 스프라이트 렌더러
    public Animator player_anim;                    // 플레이어 애니메이션
    public ParticleSystem player_particle;          // 플레이어 파티클

    private Vector2 player_start;                   // 플레이어 시작 위치
    private Vector2 player_end;                     // 플레이어 최대 위치
    private Vector2 player_direction_end;           // 플레이어 연출 최대 위치

    public SpriteRenderer sky_rendrerer;            // 하늘 렌더러
    public Sprite[] sky_sprites;                    // 하늘 스프라이트

    public GameObject cloud_obj;                    // 구름 오브젝트
    public SpriteRenderer cloud_renderer;           // 구름 렌더러
    public Sprite[] cloud_sprites;                  // 구름 스프라이트
    private Vector2 cloud_pos;                      // 구름 포지션

    public GameObject bg_obj;                       // 배경 오브젝트
    public SpriteRenderer bg_renderer;              // 배경 렌더러
    public Sprite[] bg_sprites;                     // 배경 스프라이트
    private Vector2 bg_pos;                         // 배경 포지션

    public GameObject[] street_obj;                 // 길 오브젝트들
    public SpriteRenderer[] street_renderer;        // 길 렌더러
    public Sprite[] street_sprites;                 // 길 스프라이트

    public SpriteRenderer[] plant_renderer;         // 수풀 렌더러
    public Sprite[] plant_sprites;                  // 수풀 스프라이트

    private Vector2[] street_start;                 // 길 시작 포지션
    private Vector2 street_end;                     // 길 끝 포지션

    private float player_speed;                     // 플레이어 이동속도
    private float bg_speed;                         // 뒷배경 이동속도
    private float street_speed;                     // 길 이동속도

    private WaitForSecondsRealtime direction_wait;  // 연출용 타이머

    private int morning_index;                      // 아침인지 체크를 위한 정수형 변수

    private bool now_transport;                     // 현재 이동중인지 체크

    #region Initialize

    private void Awake()
    {
        TransportInitialize();
        SpeedInitialize();

        direction_wait = new WaitForSecondsRealtime(0.5f);
    }

    // 이동수단 이니셜라이즈
    private void TransportInitialize()
    {
        player_start = new Vector2(4.3f, -3.44f);
        player_end = new Vector2(0, -3.44f);
        player_direction_end = new Vector2(9.5f, -3.44f);

        cloud_pos = new Vector2(2.5f, 6.5f);
        bg_pos = new Vector2(7.1f, 1.74f);

        street_start = new Vector2[2];
        street_start[0] = new Vector2(7.1f, -6.33f);
        street_start[1] = new Vector2(32.1f, -6.33f);
        street_end = new Vector2(17.9f, -6.33f);
    }

    // 움직임 속도 초기화
    private void SpeedInitialize()
    {
        player_speed = 0.03f;
        bg_speed = 0.0001f;
        street_speed = 0.001f;
    }

    #endregion

    #region Game Controller

    // 이동 전 그래픽 초기화
    public void ResetTransport(bool morning)
    {
        morning_index = morning ? 0 : 1;
        now_transport = true;
        touch_amount = 0;

        ResetResult();

        SetVectors();
        SetBackground();
        SetPlayer();

    }

    // 이동 시작
    public void StartTransport()
    {
        TransportStart();
        StartCoroutine(CloudsMove());
        StartCoroutine(PlayerMove());
        StartCoroutine(BackgroundMove());
        StartCoroutine(StreetMove());
    }

    // 결과 리셋
    private void ResetResult()
    {
        transport_result = false;
    }

    // 포지션 리셋
    private void SetVectors()
    {
        player_start.x = Mathf.Abs(player_start.x);
        player_end.x = Mathf.Abs(player_end.x);

        cloud_pos.x = Mathf.Abs(cloud_pos.x);
        bg_pos.x = Mathf.Abs(bg_pos.x);

        street_start[0].x = Mathf.Abs(street_start[0].x);
        street_start[1].x = Mathf.Abs(street_start[1].x);
        street_end.x = Mathf.Abs(street_end.x);

        var particle_shape = player_particle.shape;

        if (morning_index == 0)
        {
            player_start.x *= -1;
            player_end.x *= -1;

            cloud_pos.x *= -1;

            street_end.x *= -1;

            particle_shape.rotation = new Vector3(0, 0, -225f);
        }
        else
        {
            player_direction_end.x *= -1;
            bg_pos.x *= -1;
            street_start[0].x *= -1;
            street_start[1].x *= -1;
            particle_shape.rotation = new Vector3(0, 0, 10f);
        }
    }

    // 배경 스프라이트 리셋
    private void SetBackground()
    {
        sky_rendrerer.sprite = sky_sprites[morning_index];

        cloud_renderer.sprite = cloud_sprites[Random.Range(0, cloud_sprites.Length)];
        cloud_obj.transform.localPosition = cloud_pos;

        bg_renderer.sprite = bg_sprites[morning_index];
        bg_obj.transform.localPosition = bg_pos;

        for (int i = 0; i < 2; i++)
        {
            street_renderer[i].sprite = street_sprites[morning_index];
            plant_renderer[i].sprite = plant_sprites[morning_index];

            street_obj[i].transform.localPosition = street_start[i];
        }
    }

    // 플레이어 리셋
    private void SetPlayer()
    {
        player_obj.transform.localPosition = player_start;
        if (morning_index == 0)
        { player_renderer.flipX = false; }
        else
        { player_renderer.flipX = true; }
    }

    // 이동 끝
    public void EndTransport()
    {
        StopAllCoroutines();
        UIManager.manager.NoTouch(true);
        now_transport = false;

        TransportPause();
        StartCoroutine(TransportEndDirection());
    }

    #endregion

    #region Direction

    Vector2 end_pos;                            // 도착지점 포지션
    float speed;                                // 속도 에너지

    Vector2 cloud_temp_vec;                     // 구름 임시 이동 벡터
    Vector2 cloud_vec;                          // 구름 벡터
    Vector2 bg_temp_vec;                        // 배경 임시 이동 벡터
    Vector2 bg_vec;                             // 배경 벡터
    Vector2 street_pos;                         // 길 무한반복을 위한 현재 길 포지션
    float street_x;                             // 길 무한반복 보간용 x float

    Vector2 player_pos;                         // 플레이어 포지션 설정

    // 시작지점을 참고 해 도착지점 알아내기
    private Vector2 GetEndPos(Vector2 position)
    {
        end_pos.x = position.x * -1;
        end_pos.y = position.y;
        return end_pos;
    }

    // 플레이어 위치정보를 속도 에너지로 변경
    private float GetSpeed(float move_speed)
    {
        speed = ((Vector2)player_obj.transform.localPosition - player_start).magnitude / Time.fixedDeltaTime;
        return move_speed * speed;
    }

    // 구름 움직임
    private IEnumerator CloudsMove()
    {
        cloud_temp_vec = GetEndPos(cloud_pos);
        while (transporting)
        {
            cloud_vec.x = Mathf.Lerp(cloud_obj.transform.localPosition.x, cloud_temp_vec.x, GetSpeed(bg_speed / 12f));
            cloud_vec.y = cloud_pos.y;
            cloud_obj.transform.localPosition = cloud_vec;
            yield return null;
        }
    }

    // 뒷배경 움직임
    private IEnumerator BackgroundMove()
    {
        bg_temp_vec = GetEndPos(bg_pos);
        while (transporting)
        {
            bg_vec.x = Mathf.Lerp(bg_obj.transform.localPosition.x, bg_temp_vec.x, GetSpeed(bg_speed / 12f));
            bg_vec.y = bg_pos.y;
            bg_obj.transform.localPosition = bg_vec;
            yield return null;
        }
    }


    // 길 무한반복을 위한 틈 보간
    private Vector2 GetStreetTransform()
    {
        if (Mathf.Abs(street_obj[0].transform.localPosition.x) < Mathf.Abs(street_obj[1].transform.localPosition.x))
        { street_x = morning_index == 0 ? street_obj[0].transform.localPosition.x + (25f - GetSpeed(street_speed)) : street_obj[0].transform.localPosition.x - (25f - GetSpeed(street_speed)); }
        else
        { street_x = morning_index == 0 ? street_obj[1].transform.localPosition.x + (25f - GetSpeed(street_speed)) : street_obj[1].transform.localPosition.x - (25f - GetSpeed(street_speed)); }
        street_pos.x = street_x;
        street_pos.y = street_end.y;
        return street_pos;
    }

    // 길 움직임이 한계를 벗어났는지 체크
    private bool CheckStreetPos(Vector2 pos)
    {
        if (pos.x <= street_end.x && morning_index == 0) { return true; }
        if (pos.x >= street_end.x && morning_index == 1) { return true; }
        return false;
    }

    // 길 움직임
    private IEnumerator StreetMove()
    {
        while (transporting)
        {
            for (int i = 0; i < 2; i++)
            {
                street_obj[i].transform.localPosition = Vector2.MoveTowards(street_obj[i].transform.localPosition, street_end, GetSpeed(street_speed));
                if (CheckStreetPos(street_obj[i].transform.localPosition))
                { street_obj[i].transform.localPosition = GetStreetTransform(); }
            }
            yield return null;
        }
    }

    // 플레이어 움직임 (뒤로)
    private IEnumerator PlayerMove()
    {
        player_pos.x = morning_index == 0 ? player_start.x + 0.2f : player_start.x - 0.2f;
        player_pos.y = player_start.y;
        while (transporting)
        {
            player_obj.transform.localPosition = Vector2.MoveTowards(player_obj.transform.localPosition, player_pos, player_speed);
            player_anim.speed = GetSpeed(player_speed);
            SetParticle();
            yield return null;
        }
    }

    // 플레이어의 움직임에 따른 파티클 설정
    private void SetParticle()
    {
        var emission = player_particle.emission;
        emission.rateOverTime = GetSpeed(player_speed) * 2f;
    }

    // 이동 끝나고 나오는 연출
    private IEnumerator TransportEndDirection()
    {
        while (Vector2.Distance(player_obj.transform.localPosition, player_direction_end) > 0.3f)
        {
            player_obj.transform.localPosition = Vector2.MoveTowards(player_obj.transform.localPosition, player_direction_end, 0.3f);
            yield return null;
        }
        yield return direction_wait;
        UIManager.manager.NoTouch(false);
        TransportEnd();
    }

    #endregion

    #region Game Logic

    float pos_x;                        // 이동 거리
    Vector2 player_move_vec;            // 플레이어 이동 벡터

    // 이동 할 거리
    private float SetPosX()
    {
        pos_x = morning_index == 0 ? 0.4f : -0.4f;
        return pos_x;
    }

    // 플레이어 최대 위치 체크
    private bool CheckPlayerEndPos(Vector2 nowPos)
    {
        if (nowPos.x > player_end.x && morning_index == 0) { return true; }
        if (nowPos.x < player_end.x && morning_index == 1) { return true; }
        return false;
    }

    // 터치 시 플레이어 이동
    private void MoveForward()
    {
        player_move_vec.x = player_obj.transform.localPosition.x + SetPosX();
        player_move_vec.y = player_obj.transform.localPosition.y;
        if (CheckPlayerEndPos(player_move_vec)) { player_move_vec = player_end; }
        player_obj.transform.localPosition = player_move_vec;

        touch_amount += 1;
        if (touch_amount != 0 && touch_amount % 5 == 0)
        { TransportMoney(10); }

        if (touch_amount >= 35)
        { transport_result = true; }
    }


    #endregion

    public void OnPointerClick(PointerEventData eventData)
    {
        if (now_transport)
        { MoveForward(); }
    }
}
