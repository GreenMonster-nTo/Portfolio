using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport : MonoBehaviour
{
    public GameObject transport_obj;    // 이동 오브젝트
    protected int time_tick;            // 1틱당 몇 초인지 설정
    protected bool transporting;        // 현재 게임 진행중인지
    protected bool transport_result;    // 게임 결과

    // 이동수단 시작/끝을 알리는 델리게이트
    public delegate void transport_delegate();
    public static event transport_delegate transport_start;
    public static event transport_delegate transport_end;
    public static event transport_delegate transport_pause;

    // 이동수단 시 티끌 획득 델리게이트
    public delegate void transport_get(int amount);
    public static event transport_get transport_money;
    public static event transport_get transport_mood;

    public void TransportStart()
    {
        transporting = true;
        transport_start();
    }

    // 연출용
    public void TransportPause()
    {
        transport_pause();
    }

    public void TransportEnd()
    {
        transporting = false;
        transport_obj.SetActive(false);
        transport_end();
    }

    // 결과
    public bool GetTransportResult()
    {
        return transport_result;
    }

    // 기분
    public void TransportMood(int mood)
    {
        transport_mood(mood);
    }

    // 돈
    public void TransportMoney(int money)
    {
        transport_money(money);
    }
}