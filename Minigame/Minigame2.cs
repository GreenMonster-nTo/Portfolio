using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame2 : Minigame, IMinigame
{
    public GameObject dustmo_cup;           // 먼지모 컵 오브젝트
    public GameObject water_mask;           // 물 마스크 오브젝트
    public GameObject bottle;               // 물병 오브젝트
    private Minigame2_Bottle bottle_script; // 물병 스크립트

    public Sprite[] dustmo_sprite;          // 먼지모 스프라이트
    public Sprite[] cup_sprite;             // 흘러넘친 물 스프라이트 (컵)
    public Sprite[] floor_sprite;           // 흘러넘친 물 스프라이트 (바닥)

    private Vector2 mask_min;               // 물 마스크 min 포지션
    private Vector2 mask_max;               // 물 마스크 max 포지션

    private GameObject dustmo;              // 먼지모 오브젝트
    private SpriteRenderer dustmo_renderer; // 먼지모 스프라이트 렌더러
    private Vector2 dustmo_startpos;        // 먼지모 위치정보(최소)
    private Vector2 dustmo_endpos;          // 먼지모 위치정보(최대)
    private Vector2 dustmo_movement;        // 먼지모 움직임

    private GameObject overflow_cup;        // 흘러넘친 물 (컵)
    private SpriteRenderer cup_renderer;    // 흘러넘친 물 (컵) 스프라이트 렌더러
    private GameObject overflow_floor;      // 흘러넘친 물 (바닥)
    private SpriteRenderer floor_renderer;  // 흘러넘친 물 (바닥) 스프라이트 렌더러

    private float full_amount;              // 목표랑
    private float error_amount;             // 오차값
    private float now_amount;               // 현재 물 양

    private bool now_pour;                  // 현재 물 붓는중인지?
    private bool overflow_start;            // 애니메이션 중복 막기용


    // 코루틴들
    private Coroutine dustmo_movement_coroutine;
    private Coroutine dustmo_position_coroutine;
    private Coroutine water_pouring_coroutine;
    private Coroutine water_overflow_coroutine;

    private WaitForSeconds dustmo_move_wait;
    private WaitForSeconds water_overflow_wait;

    #region Initialize

    // 초기화
    public void MinigameInitialize()
    {
        DelegateSet();
        error_amount = 2f;

        mask_min.x = 0;
        mask_min.y = -3.35f;
        mask_max.x = 0;
        mask_max.y = 0.08f;
        dustmo_startpos.x = 0;
        dustmo_startpos.y = 0.7f;
        dustmo_endpos.x = 0;
        dustmo_endpos.y = 1.9f;

        dustmo = dustmo_cup.transform.Find("Dustmo").gameObject;
        overflow_cup = dustmo_cup.transform.Find("Overflow_Cup").gameObject;
        overflow_floor = dustmo_cup.transform.Find("Overflow_Floor").gameObject;

        dustmo_move_wait = new WaitForSeconds(0.5f);
        water_overflow_wait = new WaitForSeconds(0.2f);

        dustmo_renderer = dustmo.GetComponent<SpriteRenderer>();
        cup_renderer = overflow_cup.GetComponent<SpriteRenderer>();
        floor_renderer = overflow_floor.GetComponent<SpriteRenderer>();

        bottle_script = bottle.GetComponent<Minigame2_Bottle>();
        bottle_script.BottleInitialize(dustmo_cup.transform);
    }

    // 미니게임 액티브 되었을 때 (지속적인 초기화)
    public void MinigameActive()
    {
        full_amount = Random.Range(15, 25);
        now_amount = 0;
        now_pour = false;
        overflow_start = false;

        overflow_cup.SetActive(false);
        overflow_floor.SetActive(false);
        bottle_script.BottleActive();

        water_mask.transform.localPosition = mask_min;
        dustmo.transform.localPosition = dustmo_startpos;
        dustmo_renderer.sprite = dustmo_sprite[0];

        StartCoroutine(DustmoFlowing());
        dustmo_movement_coroutine = StartCoroutine(DustmoMovementSet());
    }

    // 델리게이트 해제
    public void MinigameInactive()
    {
        DelegateDel();
    }

    #endregion

    #region Game Direction

    float y_position;                       // 마스크 포지션 설정용
    float x_pos;                            // 먼지모 움직임 설정용
    float y_pos;                            // 먼지모 움직임 설정용
    Vector2 dustmo_vec;                     // 먼지모 움직임 랜덤 벡터
    Vector2 mask_vec;                       // 마스크 랜덤 벡터
    Vector2 dustmo_water_vec;               // 먼지모 물 양에 따른 랜덤 벡터
    int count;                              // 연출용 카운트

    // 물의 마스크 포지션 설정 계산식
    private float GetMaskPosition()
    {
        y_position = mask_min.y - mask_max.y;
        return y_position * (now_amount / (full_amount + error_amount));
    }

    // 물 스프라이트 설정
    private void WaterSpriteSet(bool overflow)
    {
        if (overflow) { water_overflow_coroutine = StartCoroutine(WaterOverflow()); }
        else
        {
            mask_vec.x = mask_min.x;
            mask_vec.y = mask_min.y - GetMaskPosition();
            water_mask.transform.localPosition = mask_vec;
        }
    }

    // 먼지모 물 양에 따른 포지션 설정 계산식
    private float GetDustmoPosition()
    {
        y_position = dustmo_endpos.y - dustmo_startpos.y;
        y_position = y_position * (now_amount / (full_amount + error_amount));
        y_position = y_position >= dustmo_endpos.y ? dustmo_endpos.y : y_position;
        return y_position;
    }

    // 먼지모 물 양에 따른 애니메이션
    private IEnumerator DustmoPosition()
    {
        while (now_pour)
        {
            dustmo_water_vec.x = dustmo.transform.localPosition.x;
            dustmo_water_vec.y = dustmo_startpos.y + GetDustmoPosition();
            dustmo.transform.localPosition = dustmo_water_vec;
            yield return null;
        }
        StopCoroutine(dustmo_position_coroutine);
        dustmo_position_coroutine = null;
    }

    // 먼지모 랜덤 움직임 계산식
    private Vector2 DustmoRandomMovement()
    {
        x_pos = Random.Range(dustmo.transform.localPosition.x - 0.3f, dustmo.transform.localPosition.x + 0.3f);
        y_pos = Random.Range(dustmo.transform.localPosition.y - 0.3f, dustmo.transform.localPosition.y + 0.3f);
        x_pos = x_pos < -0.3f ? -0.3f : x_pos;
        x_pos = x_pos > 0.3f ? 0.3f : x_pos;
        y_pos = y_pos < dustmo_startpos.y ? dustmo_startpos.y : y_pos;
        y_pos = y_pos > dustmo_startpos.y + GetDustmoPosition() ? dustmo_startpos.y + GetDustmoPosition() : y_pos;
        y_pos = y_pos > dustmo_endpos.y ? dustmo_endpos.y : y_pos;
        dustmo_vec.x = x_pos;
        dustmo_vec.y = y_pos;

        return dustmo_vec;
    }

    // 먼지모 랜덤 움직임 설정
    private IEnumerator DustmoMovementSet()
    {
        while (this.gameObject.activeInHierarchy)
        {
            dustmo_movement = DustmoRandomMovement();
            yield return dustmo_move_wait;
        }
        StopCoroutine(dustmo_movement_coroutine);
        dustmo_movement_coroutine = null;
    }

    // 먼지모 기본 움직임
    private IEnumerator DustmoFlowing()
    {
        while (this.gameObject.activeInHierarchy)
        {
            dustmo.transform.localPosition = Vector2.MoveTowards(dustmo.transform.localPosition, dustmo_movement, 0.001f);
            yield return null;
        }
    }

    // 물 넘침 연출
    private IEnumerator WaterOverflow()
    {
        count = 0;
        // 먼지모 놀란 표정으로 설정
        dustmo_renderer.sprite = dustmo_sprite[2];
        overflow_cup.SetActive(true);
        bottle_script.SetMoveable(false);

        while (count < 5)
        {
            if (count < 3)
            { cup_renderer.sprite = cup_sprite[count]; }
            else if (count >= 3)
            {
                overflow_floor.SetActive(true);
                floor_renderer.sprite = floor_sprite[count - 3];
            }
            count++;
            yield return water_overflow_wait;
        }
        StopCoroutine(water_overflow_coroutine);
        water_overflow_coroutine = null;
        CheckGameEnd(false);
    }

    #endregion

    #region Game Logic

    // 물 붓는 로직
    private void Pouring(bool pouring, float water_amount)
    {
        if (pouring)
        {
            now_pour = true;
            water_pouring_coroutine = StartCoroutine(PouringWater(water_amount));
            dustmo_position_coroutine = StartCoroutine(DustmoPosition());
        }
        else { now_pour = false; }
        CheckWaterAmountError();
    }

    // 물 붓기
    private IEnumerator PouringWater(float water_amount)
    {
        while (now_pour)
        {
            now_amount += water_amount;
            CheckWaterAmountError();
            yield return null;
        }
        StopCoroutine(water_pouring_coroutine);
        water_pouring_coroutine = null;
    }

    // 물 오차 체크
    private void CheckWaterAmountError()
    {
        if (now_amount < full_amount - error_amount)
        { WaterSpriteSet(false); }
        else if (now_amount >= full_amount - error_amount && now_amount <= full_amount + error_amount)
        {
            WaterSpriteSet(false);
            if (!now_pour) { CheckGameEnd(true); }
        }
        else if (now_amount > full_amount + error_amount)
        {
            if (!overflow_start)
            {
                MinigameTimerStop();
                WaterSpriteSet(true);
                overflow_start = true;
            }
        }
    }

    // 게임 끝나는지 체크
    private void CheckGameEnd(bool currect)
    {
        if (currect)
        {
            bottle_script.SetMoveable(false);
            // 먼지모 웃는 표정으로 설정
            dustmo_renderer.sprite = dustmo_sprite[1];
        }
        MinigameEnd(currect);
    }

    #endregion

    // 게임 시작
    public void MinigameStart()
    {
        MinigameTimerStart();
    }

    // 게임 끝
    public void MinigameEnd(bool result)
    {
        MinigameTimerStop();
        SendGameResult(result);
    }

    private void DelegateSet()
    {
        Minigame2_Bottle.pouring += Pouring;
    }

    private void DelegateDel()
    {
        Minigame2_Bottle.pouring -= Pouring;
    }
}
