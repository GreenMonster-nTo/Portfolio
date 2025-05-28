using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport2 : Transport
{
    private const int passenger_amount = 8;         // 최대 승객 수

    public GameObject player_obj;                   // 플레이어 오브젝트
    public Animator player_anim;                    // 플레이어 애니메이션
    private Vector2 player_direction_end;           // 플레이어 연출용 도착위치

    public GameObject background_bg;                // 배경 오브젝트
    public SpriteRenderer[] background_rendrerer;   // 배경 렌더러
    public Sprite[] background_sprites;             // 배경 스프라이트

    public Transport_Door door;                     // 문짝 관련

    private Vector2 left_end;                       // 왼쪽 배경엔딩 위치
    private Vector2 right_end;                      // 오른쪽 배경 엔딩 위치

    public GameObject passenger_list;               // 승객 부모 오브젝트
    private GameObject[] passengers_obj;            // 승객 오브젝트풀
    private Transport_Passenger[] passengers;       // 승객 오브젝트 스크립트

    private Vector2 bg_move;                        // 뒷배경 이동벡터

    private int passenger_kick_amount;              // 승객 쳐낸 횟수

    private float transport_timer;                  // 이동 타이머
    private int morning_index;                      // 아침인지 체크를 위한 정수형 변수

    private Coroutine touch_coroutine;              // 터치 코루틴

    #region Initialize

    private void Awake()
    {
        TransportInitialize();
    }

    // 이동수단 이니셜라이즈
    private void TransportInitialize()
    {
        passengers_obj = new GameObject[passenger_amount];
        for (int i = 0; i < passenger_amount; i++)
        {
            passengers_obj[i] = passenger_list.transform.Find("Passenger" + i).gameObject;
        }
        passengers = new Transport_Passenger[passenger_amount];
        for (int i = 0; i < passenger_amount; i++)
        {
            passengers[i] = passengers_obj[i].GetComponent<Transport_Passenger>();
        }
        transport_timer = 12f;
        player_direction_end = new Vector2(0, -11f);

        PassengerInitialize();
        BackgroundVectorInitialize();
    }

    // 승객 오브젝트 초기화
    private void PassengerInitialize()
    {
        for (int i = 0; i < passenger_amount; i++)
        {
            passengers[i].PassengerInitialize(2, i);
        }
    }

    // 배경 벡터 초기화
    private void BackgroundVectorInitialize()
    {
        left_end = new Vector2(65f, 3.7f);
        right_end = new Vector2(-2.5f, 3.7f);
    }

    #endregion

    #region Game Controller

    // 이동 전 그래픽 초기화
    public void ResetTransport(bool morning)
    {
        morning_index = morning ? 0 : 1;

        ResetResult();
        door.TriggerOff();
        player_obj.transform.localPosition = Vector2.zero;
        player_anim.SetBool("pressed", false);
        SetBackground();
        SetPassenger();
    }

    // 이동 시작
    public void StartTransport()
    {
        TransportStart();
        TouchTimer_Reset();
        StartCoroutine(Move_Background());
    }

    // 결과 리셋
    private void ResetResult()
    {
        passenger_kick_amount = 0;
        transport_result = false;
    }

    // 배경 벡터 리셋
    private void SetBackground()
    {
        if (morning_index == 0)
        {
            background_bg.transform.localPosition = right_end;
            bg_move = left_end;
        }
        else
        {
            background_bg.transform.localPosition = left_end;
            bg_move = right_end;
        }

        for (int i = 0; i < background_rendrerer.Length; i++)
            background_rendrerer[i].sprite = background_sprites[morning_index];

    }

    // 손님 리셋
    private void SetPassenger()
    {
        for (int i = 0; i < passenger_amount; i++)
        {
            passengers[i].PassengerReset();
        }
    }

    // 이동 끝
    public void EndTransport()
    {
        StopAllCoroutines();
        UIManager.manager.NoTouch(true);
        player_anim.SetBool("pressed", false);
        player_anim.SetBool("walk", true);

        door.Door_Open();

        StartCoroutine(Ending_Direction());
        // TransportEnd();
    }

    #endregion

    #region Direction

    // 승객 재사용
    private void Passenger_Reset(int index)
    {
        passenger_kick_amount += 1;
        if (passenger_kick_amount > 10)
        { transport_result = true; }

        // 돈 처리
        TransportMoney(5);

        // 플레이어 캐릭터 찌부상태에서 원래상테로 되돌리기 && 찌부타이머 초기화
        TouchTimer_Reset();

        if (index < 4)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i != index)
                {
                    passengers[i].PassengerIndexSet(passengers[i].GetNowIndex() - 1);
                }
            }
        }
        else
        {
            for (int i = 4; i < passenger_amount; i++)
            {
                if (i != index)
                {
                    passengers[i].PassengerIndexSet(passengers[i].GetNowIndex() - 1);
                }
            }
        }
    }

    // 찌부타이머 리셋
    private void TouchTimer_Reset()
    {
        player_anim.SetBool("pressed", false);
        if (touch_coroutine != null)
        { StopCoroutine(touch_coroutine); }
        touch_coroutine = StartCoroutine(TouchTimer());
    }

    // 찌부타이머
    private IEnumerator TouchTimer()
    {
        var wait = new WaitForSecondsRealtime(2f);
        while (transporting)
        {
            yield return wait;
            player_anim.SetBool("pressed", true);
            // 기분 처리
            TransportMood(-2);
        }
    }

    // 뒷배경 이동
    private IEnumerator Move_Background()
    {
        Vector2 move = background_bg.transform.localPosition;
        float move_time = 0.0f;
        while (transporting)
        {
            move_time += Time.deltaTime;
            background_bg.transform.localPosition = new Vector2(Mathf.Lerp(move.x, bg_move.x, move_time / transport_timer), bg_move.y);
            yield return null;
        }
    }

    // 엔딩 연출
    private IEnumerator Ending_Direction()
    {
        GameManager.manager.GetSoundManager().BusDoor();
        GameManager.manager.GetSoundManager().Walk();
        while (Vector2.Distance(player_obj.transform.localPosition, player_direction_end) > 0.5f)
        {
            player_obj.transform.localPosition = Vector2.MoveTowards(player_obj.transform.localPosition, player_direction_end, 0.1f);
            yield return null;
        }

        Transport_Direction_Ended();
    }

    // 엔딩 연출 끝나고 실질엔딩
    private void Transport_Direction_Ended()
    {
        StopAllCoroutines();

        player_anim.SetBool("walk", false);
        door.Door_Close();

        UIManager.manager.NoTouch(false);
        TransportEnd();
    }

    #endregion

    #region Delegates

    private void OnEnable() => DelegateSet();
    private void OnDisable() => DelegateDel();

    private void DelegateSet()
    {
        Transport_Passenger.passneger_out += Passenger_Reset;
    }

    public void DelegateDel()
    {
        Transport_Passenger.passneger_out -= Passenger_Reset;
    }

    #endregion
}
