using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Transport4_Player : MonoBehaviour, IDragHandler
{
    public Animator anim;
    private Vector2 touch_pos;

    private bool dragable;

    // 애니메이션 제어
    public void SetFly(bool active)
    {
        anim.SetBool("flying", active);
    }

    // 깃털 먹었을 때
    public void Collected()
    {
        anim.SetBool("collected", true);
    }

    // 원래대로 복구
    public void Collected_End()
    {
        anim.SetBool("collected", false);
    }

    // 드래그 가능한지
    public void SetDragable(bool active)
    {
        dragable = active;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragable)
        {
            touch_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.transform.localPosition = touch_pos;
        }
    }
}
