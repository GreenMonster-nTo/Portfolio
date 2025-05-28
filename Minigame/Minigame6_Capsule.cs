using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Minigame6_Capsule : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private int capsule_index;                  // 캡슐 인덱스
    private Vector2 target_pos;                 // 캡슐커피넣는 곳
    private Vector2 capsule_pos;                // 캡슐 위치    
    private Vector2 touch_pos;                  // 터치 포지션

    private SpriteRenderer capsule_renderer;    // 캡슐 스프라이트 렌더러
    private int sorting_order;                  // 캡슐 레이어

    private Touch_Scale touch_scale;            // 터치 스케일 처리

    private bool touchable;                     // 집어들기 가능한지

    public delegate void Picked(int index, bool picked);
    public static event Picked picked;          // 캡슐 집어듬
    public static event Picked insert;          // 캡슐 커피머신에 넣음

    #region Initialize

    // 초기화
    public void CapsuleInitialize(int index, Transform target)
    {
        capsule_index = index;
        capsule_renderer = this.GetComponent<SpriteRenderer>();
        sorting_order = capsule_renderer.sortingOrder;
        target_pos = target.position;
        capsule_pos = this.transform.localPosition;
        touch_scale = this.GetComponent<Touch_Scale>();
    }

    // 캡슐 액티브
    public void CapsuleActive()
    {
        this.transform.localPosition = capsule_pos;
        capsule_renderer.sortingOrder = sorting_order;
        SetTouchable(true);
    }

    // 캡슐 터치 가능한지 설정
    public void SetTouchable(bool pick)
    {
        touchable = pick;
        touch_scale.SetTouchable(pick);
    }

    #endregion

    #region Game Logic

    // 원래 캡슐커피 자리로
    public void CapsulePosReturn()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        this.transform.localPosition = capsule_pos;
    }

    // 캡슐 움직임
    private void CapsuleMove()
    {
        touch_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = touch_pos;
        CheckCapsulePosition();
    }

    // 캡슐이 커피 넣는 곳에 닿았을 경우
    private void CheckCapsulePosition()
    {
        if (Vector2.Distance(this.transform.position, target_pos) < 1f)
        {
            this.transform.localPosition = target_pos;
            this.gameObject.SetActive(false);
            insert(capsule_index, true);
        }
    }

    #endregion

    #region Event Systems

    public void OnDrag(PointerEventData eventData)
    {
        if (!touchable) { return; }
        CapsuleMove();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (touchable)
        {
            GameManager.manager.GetSoundManager().SoundEffect();
            capsule_renderer.sortingOrder = 190;
            picked(capsule_index, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        capsule_renderer.sortingOrder = sorting_order;
        picked(capsule_index, false);
    }

    #endregion
}
