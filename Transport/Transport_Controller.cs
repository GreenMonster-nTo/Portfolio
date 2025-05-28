using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transport_Controller : MonoBehaviour
{
    private int transport_amount;               // 이동수단 개수
    private GameObject[] transports;            // 현재 생성된 이동수단
    private int now_transport;                  // 현재 이동수단
    private bool now_morning;                   // 현재 아침인지 (출근길인지)

    private Transport0 transport_metro;         // 지하철
    private Transport1 transport_jogging;       // 조깅
    private Transport2 transport_bus;           // 버스
    private Transport3 transport_wagon;         // 달구지
    private Transport4 transport_cannon;        // 대포타기
    private Transport5 transport_ferry;         // 뗏목
    private Transport6 transport_run;           // 담벼락달리기

    public GameObject transport_canvas;         // 이동수단 이펙트 캔버스
    private GameObject transport_result_panel;  // 이동수단 이펙트 판넬
    private GameObject[] transport_effect;      // 이동수단 이펙트 오브젝트들

    #region Effects

    // 미니게임 관련 UI 초기화
    private void TransportEffectInitialize()
    {
        transport_result_panel = transport_canvas.transform.Find("Result_Panel").gameObject;

        transport_effect = new GameObject[3];
        transport_effect[0] = transport_result_panel.transform.Find("start").gameObject;
        transport_effect[1] = transport_result_panel.transform.Find("clear").gameObject;
        transport_effect[2] = transport_result_panel.transform.Find("fail").gameObject;
    }

    // 이펙트 시작 이펙트 설정
    public void TransportStartEffect()
    {
        transport_canvas.SetActive(true);
        TransportEffect_On(0);
    }

    // 이동수단 끝 이펙트 온
    private void TransportEndEffect()
    {
        if (GetResult())
        {
            TransportEffect_On(1);
        }
        else
        {
            TransportEffect_On(2);
        }
    }

    // 이동수단 이펙트 시작
    public void TransportEffect_On(int effect)
    {
        for (int i = 0; i < transport_effect.Length; i++)
        { transport_effect[i].SetActive(false); }

        GameManager.manager.GetSoundManager().MinigameResult(effect);
        transport_result_panel.SetActive(true);
        transport_effect[effect].SetActive(true);
    }

    // 이동수단 이펙트 끝
    public void TransportEffect_Off()
    {
        for (int i = 0; i < transport_effect.Length; i++)
        { transport_effect[i].SetActive(false); }
        transport_result_panel.SetActive(false);
    }

    // 이펙트 끝 설정
    public void TransportEffectEnd()
    {
        TransportEffect_Off();
        transport_canvas.SetActive(false);
    }

    // 결과 받아오기
    private bool GetResult()
    {
        switch (now_transport)
        {
            case 0:
                // 지하철
                return transport_metro.GetTransportResult();
            case 1:
                // 조깅
                return transport_jogging.GetTransportResult();
            case 2:
                // 버스
                return transport_bus.GetTransportResult();
            case 3:
                // 사막 횡단
                return transport_wagon.GetTransportResult();
            case 4:
                // 대포
                return transport_cannon.GetTransportResult();
            case 5:
                // 나룻배
                return transport_ferry.GetTransportResult();
            case 6:
                // 담벼락 달리기
                return transport_run.GetTransportResult();
        }
        return transport_metro.GetTransportResult();
    }

    #endregion

    #region Transport

    // 초기화
    private void ControllerInitialize()
    {
        transport_amount = 7;

        transports = new GameObject[transport_amount];

        for (int i = 0; i < transport_amount; i++)
        {
            transports[i] = Instantiate(Data_Container.pamphlet_container.GetTransport(i), this.transform);
            transports[i].SetActive(false);
        }

        transport_metro = transports[0].GetComponent<Transport0>();
        transport_jogging = transports[1].transform.Find("Player").GetComponent<Transport1>();
        transport_bus = transports[2].GetComponent<Transport2>();
        transport_wagon = transports[3].transform.Find("Wagon").transform.Find("Player").GetComponent<Transport3>();
        transport_cannon = transports[4].GetComponent<Transport4>();
        transport_ferry = transports[5].transform.Find("Ferry").transform.Find("Player").GetComponent<Transport5>();
        transport_run = transports[6].transform.Find("Player").GetComponent<Transport6>();

        now_transport = 0;
        now_morning = false;
        TransportEffectInitialize();
    }

    // 선택된 이동수단 시작
    public void TransportActive(int picked)
    {
        UIManager.manager.TransportPanel_inactive();

        if (now_morning)
        {
            transport_light(0.95f);
            // 담벼락 달리기 예외처리
            if (DataManager.manager.transports.transport[picked].prefab_code == 6)
            { transport_light(DataManager.manager.transports.transport[picked].intensity); }
        }
        else
        { transport_light(DataManager.manager.transports.transport[picked].intensity); }

        transport_start(picked);
        now_transport = DataManager.manager.transports.transport[picked].prefab_code;

        if (now_transport < 0)
        { Transport_Skip(); }
        else
        {
            for (int i = 0; i < transport_amount; i++)
            {
                if (transports[i] != null)
                { transports[i].SetActive(false); }
            }
            if (transports[now_transport] != null)
            { transports[now_transport].SetActive(true); }

            Transport_Reset();
            TransportStartEffect();
        }
    }

    // 이동수단 리셋
    private void Transport_Reset()
    {
        switch (now_transport)
        {
            case 0:
                // 지하철
                transport_metro.ResetTransport(now_morning);
                break;
            case 1:
                // 조깅
                transport_jogging.ResetTransport(now_morning);
                break;
            case 2:
                // 버스
                transport_bus.ResetTransport(now_morning);
                break;
            case 3:
                // 사막 횡단
                transport_wagon.ResetTransport(now_morning);
                break;
            case 4:
                // 대포 타기
                transport_cannon.ResetTransport(now_morning);
                break;
            case 5:
                // 나룻배
                transport_ferry.ResetTransport(now_morning);
                break;
            case 6:
                // 담벼락 달리기
                transport_run.ResetTransport(now_morning);
                break;
            default:
                transport_metro.ResetTransport(now_morning);
                break;
        }
    }

    // 이동수단 시작
    private void TransportStart()
    {
        TransportEffect_Off();
        switch (now_transport)
        {
            case 0:
                // 지하철
                transport_metro.StartTransport();
                break;
            case 1:
                // 조깅
                transport_jogging.StartTransport();
                break;
            case 2:
                // 버스
                transport_bus.StartTransport();
                break;
            case 3:
                // 사막 횡단
                transport_wagon.StartTransport();
                break;
            case 4:
                // 대포 타기
                transport_cannon.StartCannon();
                break;
            case 5:
                // 나룻배
                transport_ferry.StartTransport();
                break;
            case 6:
                // 담벼락 달리기
                transport_run.StartTransport();
                break;
            default:
                transport_metro.StartTransport();
                break;
        }
    }

    // 이동수단 종료
    private void TransportEnd()
    {
        TransportEffectEnd();
        switch (now_transport)
        {
            case 0:
                // 지하철
                transport_metro.EndTransport();
                break;
            case 1:
                // 조깅
                transport_jogging.EndTransport();
                break;
            case 2:
                // 버스
                transport_bus.EndTransport();
                break;
            case 3:
                // 사막 횡단
                transport_wagon.EndTransport();
                break;
            case 4:
                // 대포 타기
                transport_cannon.EndTransport();
                break;
            case 5:
                // 나룻배
                transport_ferry.EndTransport();
                break;
            case 6:
                // 담벼락 달리기
                // transport_run.EndTransport();
                break;
            default:
                transport_metro.EndTransport();
                break;
        }
    }

    // 스케줄 스킵 이동수단(택시, 걸어가기)
    private void Transport_Skip()
    {
        if (now_morning)
        { UIManager.manager.FadeOutIn("transport_m", 0.5f); }
        else
        { UIManager.manager.FadeOutIn("transport_n", 0.5f); }
    }

    // 이동수단 호출 설정
    private void Transport_ScheduleSet(bool active, bool morning)
    {
        if (active)
        {
            now_morning = morning;
            UIManager.manager.TransportPanel_active(morning);
        }
        else
        {
            TransportEndEffect();
        }
    }

    #endregion

    #region Delegates

    // 이동수단 시작/끝을 알리는 델리게이트
    public delegate void transport_delegate_start(int index);
    public static event transport_delegate_start transport_start;

    public delegate void transport_light_set(float intensity);
    public static event transport_light_set transport_light;

    private void OnEnable() => DelegateSet();
    private void OnDisable() => DelegateDel();

    private void DelegateSet()
    {
        Data_Controller.controller_init += ControllerInitialize;
        Data_Controller.transport_set += Transport_ScheduleSet;
        UI_TransportPamphlet.selected_transport += TransportActive;

        Transport_Effect.start_effect += TransportStart;
        Transport_Effect.result_effect += TransportEnd;
    }

    private void DelegateDel()
    {
        Data_Controller.controller_init -= ControllerInitialize;
        Data_Controller.transport_set -= Transport_ScheduleSet;
        UI_TransportPamphlet.selected_transport -= TransportActive;

        Transport_Effect.start_effect -= TransportStart;
        Transport_Effect.result_effect -= TransportEnd;
    }

    #endregion
}
