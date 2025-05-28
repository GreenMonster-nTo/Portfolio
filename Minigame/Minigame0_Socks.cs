using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Minigame0_Socks : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private int code;                       // 양말 구분자
    private bool dragable;                  // 드래그 가능 양말?
    private float auto_move;                // 오토이동 스피드

    private Transform target_transform;     // 양말 목적지

    private Touch_Scale touch_scale;        // 터치스케일 설정하는 스크립트
    private SpriteRenderer socks_renderer;  // 스프라이트 렌더러

    private Vector2 touch_pos;              // 현재 터치 포지션
    private bool touched;                   // 터치 했는지 저장 (지속적으로 초기화)

    private PolygonCollider2D collider2d;   // 콜리더 2D

    public delegate bool Check(bool dragable, int code);
    public static event Check check_touchable;

    public delegate void Collect();
    public static event Collect collected;

    #region Initialize

    // 양말 초기화 함수
    public void SocksInitialize(int socks_code, bool socks_dragable, Transform target)
    {
        code = socks_code;
        dragable = socks_dragable;
        auto_move = 0.5f;

        target_transform = target;
        socks_renderer = this.GetComponent<SpriteRenderer>();
        touch_scale = this.GetComponent<Touch_Scale>();
    }

    // 양말 액티브 함수
    public void SocksActive(Sprite socks_sprite)
    {
        SocksInactive();

        socks_renderer.sprite = socks_sprite;
        touch_pos = Vector2.zero;
        touched = false;
        TouchableSet(false);

        collider2d = this.gameObject.AddComponent<PolygonCollider2D>();
    }

    // 양말 인액티브 함수
    public void SocksInactive()
    {
        Destroy(collider2d);
        collider2d = null;
    }

    // 터치가능한지 설정
    public void TouchableSet(bool active)
    {
        touch_scale.SetTouchable(active);
    }

    #endregion

    // 오토 이동
    private IEnumerator AutoMove()
    {
        touched = true;

        while (touched && this.transform.localPosition != target_transform.localPosition)
        {
            this.transform.localPosition = Vector2.MoveTowards(this.transform.localPosition, target_transform.localPosition, auto_move);
            yield return null;
        }
        GameManager.manager.GetSoundManager().Collect();
        collected();
        StopAllCoroutines();
    }

    // 터치 이동
    private void Move()
    {
        touch_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = touch_pos;
    }

    #region EventSystems

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragable) { return; }

        if (check_touchable(dragable, code))
        { Move(); }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (touched) { return; }

        if (check_touchable(dragable, code))
        {
            GameManager.manager.GetSoundManager().Socks();
            TouchableSet(true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (touched) { return; }

        if (check_touchable(dragable, code))
        {
            if (!dragable && touch_scale.GetTouchable())
            {
                TouchableSet(false);
                StartCoroutine(AutoMove());
            }
        }
    }

    #endregion
}
