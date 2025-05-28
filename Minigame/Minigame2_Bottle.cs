using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Minigame2_Bottle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Sprite bottle_sprite;                    // 물병 스프라이트
    public Sprite[] bottle_pouring_sprite;          // 물 부을 때 스프라이트

    private SpriteRenderer bottle_renderer;         // 물병 렌더러
    private AudioSource bottle_sound;               // 물병 사운드
    private Transform cup_transform;                // 컵 위치

    private Vector2 bottle_pos;                     // 기본 물병 포지션
    private Vector2 touch_pos;                      // 터치 포지션

    private bool moveable;                          // 움직일 수 있는지
    private bool now_pouring;                       // 먼지모 컵에 물 붓고있는지
    private Touch_Scale touch_scale;                // 터치 스케일 스크립트

    private WaitForSeconds delay;                   // 기다리기

    public delegate void Pouring(bool now_pour, float amount);
    public static event Pouring pouring;

    #region Initialize

    // 물병 초기화 함수
    public void BottleInitialize(Transform cup)
    {
        bottle_pos = this.transform.localPosition;
        bottle_renderer = this.transform.GetComponent<SpriteRenderer>();
        bottle_sound = this.transform.GetComponent<AudioSource>();
        touch_scale = this.transform.GetComponent<Touch_Scale>();

        delay = new WaitForSeconds(0.2f);
        cup_transform = cup;
    }

    // 물병 액티브 함수
    public void BottleActive()
    {
        this.transform.localPosition = bottle_pos;

        SetMoveable(true);
        touch_scale.SetTouchable(true);
        now_pouring = false;
        bottle_renderer.sprite = bottle_sprite;
    }

    // 물병 인액티브 함수
    public void BottleInactive()
    {
        StopAllCoroutines();
    }

    // 움직일 수 있는지 설정
    public void SetMoveable(bool active)
    {
        moveable = active;
        touch_scale.SetTouchable(false);
    }

    #endregion

    #region Game Direction

    int frame;                                      // 애니메이션 프레임

    // 물병 스프라이트 변경 or 애니메이션
    private IEnumerator BottleAnim()
    {
        frame = 0;
        while (now_pouring)
        {
            if (frame == bottle_pouring_sprite.Length) { frame = 0; }
            bottle_renderer.sprite = bottle_pouring_sprite[frame++];
            yield return delay;
        }
        bottle_renderer.sprite = bottle_sprite;
        StopAllCoroutines();
    }

    #endregion

    #region Game Logic

    // 먼지모 컵에 닿았는지 체크
    private void CheckPouring()
    {
        if (Vector2.Distance(this.transform.localPosition, cup_transform.localPosition) < 5f)
        {
            if (!now_pouring)
            {
                bottle_sound.Play();
                now_pouring = true;
                StartCoroutine(BottleAnim());
                pouring(now_pouring, 0.1f);
            }
        }
        else
        {
            if (now_pouring)
            {
                bottle_sound.Stop();
                now_pouring = false;
                StartCoroutine(BottleAnim());
                pouring(now_pouring, 0.1f);
            }
        }
    }

    // 움직임 제어 함수
    private void Move()
    {
        touch_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = touch_pos;
    }

    #endregion

    #region Event Systems

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!moveable)
        {
            touch_scale.ScaleSetter(false);
            this.transform.localPosition = bottle_pos;
            CheckPouring();
            return;
        }
        Move();
        CheckPouring();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.localPosition = bottle_pos;
        CheckPouring();
    }

    #endregion
}
