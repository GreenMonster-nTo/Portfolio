using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Transport_Door : MonoBehaviour, IPointerClickHandler
{
    // 문짝 애니메이션
    public Animator door_anim;

    // 문짝 닫기
    public void Door_Close()
    {
        door_anim.SetBool("open", false);
    }

    // 문짝 열기
    public void Door_Open()
    {
        door_anim.SetBool("open", true);
    }

    // 애니메이션이 끝나면 자동으로 터치 리셋
    public void TriggerOff()
    {
        door_anim.ResetTrigger("touched");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        door_anim.SetTrigger("touched");
    }
}