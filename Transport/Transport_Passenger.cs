using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Transport_Passenger : MonoBehaviour, IPointerClickHandler
{
    private int where_passenger;                    // 어느 미니게임 승객인가?
    private const int RIGHT = 4;                    // 우측에 배치 될 인덱스(기준점)
    private SpriteRenderer passenger_rendrerer;     // 승객의 스프라이트렌더러
    private Touch_Scale touch_scale;                // 승객 터치스케일
    private SoundSystem passenger_sound;            // 승객 사운드시스템

    private int passenger_index;                    // 지정 인덱스
    private int now_index;                          // 현재 위치(몇 번째인지)
    private int touch_amount;                       // 지정 터치 횟수

    private int touch_count;                        // 현재 터치 횟수
    private bool touchable;                         // 터치 가능한지

    private int rotate_angle;                       // 회전 방향
    private int rotate_count;                       // 회전 정보

    private Vector2 in_pos;                         // 들어가는 방향
    private Vector2 out_pos;                        // 나가는 방향

    private Coroutine moveout_coroutine;            // MoveOut 코루틴

    #region Initialize

    int rand;                                       // 랜덤 설정용
    int type;                                       // 타입 설정용
    int code;                                       // 코드 설정용

    // 손님 초기화
    public void PassengerInitialize(int where, int index)
    {
        where_passenger = where;
        passenger_index = index;
        now_index = index;
        passenger_rendrerer = this.gameObject.GetComponent<SpriteRenderer>();
        passenger_sound = this.GetComponent<SoundSystem>();
        touch_scale = this.GetComponent<Touch_Scale>();
        passenger_sound.sound_where = "passenger_" + where + "_" + index;

        SetTouchable(false);
    }

    // 손님 리셋
    public void PassengerReset()
    {
        now_index = passenger_index;
        SetTouchable(false);
        PassengerSet();
    }

    // 현재 인덱스 호출
    public int GetNowIndex()
    {
        return now_index;
    }

    // 현재 인덱스(위치) 설정
    public void PassengerIndexSet(int index)
    {
        now_index = index;
        SetTouchable(false);
        PassengerPositionSet();
        StartCoroutine(Move_In());
    }

    // 승객 포지션/정보 설정
    private void PassengerSet()
    {
        rand = Random.Range(0, 2) == 0 ? -1 : 1;
        out_pos.x = Random.Range(-6.2f, 6.2f);
        out_pos.y = -10.5f * rand;
        if (where_passenger == 0)
        { touch_amount = Random.Range(2, 6); }
        else
        { touch_amount = Random.Range(1, 3); }
        touch_count = 0;

        if (now_index == 0 || now_index == RIGHT)
        { SetTouchable(true); }

        PassengerPositionSet();
        this.transform.localPosition = in_pos;
        this.transform.localRotation = Quaternion.identity;

        PassengerSpriteSet();
        this.gameObject.SetActive(true);
    }

    // 승객 포지션 설정
    private void PassengerPositionSet()
    {
        if (passenger_index >= RIGHT)
        {
            passenger_rendrerer.flipX = false;
            in_pos.x = 1.4f + ((now_index - RIGHT) * 1.4f);
            in_pos.y = 0;
        }
        else
        {
            passenger_rendrerer.flipX = true;
            in_pos.x = -1.4f + (now_index * -1.4f);
            in_pos.y = 0;
        }
    }

    // 승객 스프라이트 랜덤 설정
    private void PassengerSpriteSet()
    {
        type = Random.Range(0, 2);
        code = Random.Range(0, DataManager.manager.customers.customers[type].Length);
        passenger_rendrerer.sortingOrder = now_index >= RIGHT ? 50 - (now_index - RIGHT) : 50 - now_index;
        if (DataManager.customers_save[type][code].spawnable == false)
        { PassengerSpriteSet(); }
        else
        { passenger_rendrerer.sprite = Data_Container.customer_container.GetStopSprites(type, code)[0]; }
    }

    #endregion

    #region Direction

    // 화면 밖으로 나갔는지
    private void Check_Outside()
    {
        if (this.transform.localPosition.y <= -10.5f || this.transform.localPosition.y >= 10.5f)
        {
            if (moveout_coroutine != null)
            { StopCoroutine(moveout_coroutine); }
            moveout_coroutine = null;
            Move_Last();
        }
    }

    // 안쪽 방향으로 땡겨야하는지 아닌지?
    private bool Check_Inside()
    {
        return passenger_index >= RIGHT ? this.transform.localPosition.x > in_pos.x : this.transform.localPosition.x < in_pos.x;
    }

    // 화면 밖으로 이동
    private IEnumerator Move_Out()
    {
        while (!touchable && now_index == -1)
        {
            this.transform.localPosition = Vector2.MoveTowards(this.transform.localPosition, out_pos, 1.0f);
            this.transform.localRotation = Quaternion.Euler(0, 0, rotate_count * rotate_angle);
            rotate_count += 15;
            Check_Outside();
            yield return null;
        }
    }

    // 플레이어방향으로 이동
    private IEnumerator Move_In()
    {
        while (Check_Inside())
        {
            this.transform.localPosition = Vector2.MoveTowards(this.transform.localPosition, in_pos, 0.5f);
            yield return null;
        }
        if (now_index == 0 || now_index == RIGHT)
        { SetTouchable(true); }
    }

    // 최후순위로 이동
    private void Move_Last()
    {
        if (passenger_index < RIGHT)
        { PassengerIndexSet(3); }
        else
        { PassengerIndexSet(7); }
        PassengerSet();
        passneger_out(passenger_index);
    }

    #endregion

    #region Game Logic

    // 터치 가능/불가능 설정
    private void SetTouchable(bool able)
    {
        touchable = able;
        touch_scale.SetTouchable(touchable);
    }

    // 터치 입력
    private void Touched()
    {
        touch_count += 1;
        if (touch_count >= touch_amount)
        {
            now_index = -1;
            SetTouchable(false);
            moveout_coroutine = StartCoroutine(Move_Out());
        }
    }

    #endregion

    public delegate void passenger(int index);
    public static event passenger passneger_out;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (touchable)
        { Touched(); }
    }
}
