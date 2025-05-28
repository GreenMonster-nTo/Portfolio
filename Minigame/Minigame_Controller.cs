using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame_Controller : MonoBehaviour
{
    private int minigame_amount;                    // 미니게임 개수
    private GameObject[] minigames;                 // 미니게임 오브젝트들
    private IMinigame[] minigame_scripts;           // 미니게임 스크립트들

    public GameObject minigame_panel;               // 미니게임 판넬 오브젝트 (터치 밴 판넬)

    private int preselect_minigame;                 // 이전 선택되었던 게임
    private int select_minigame;                    // 현재 선택된 게임
    private int minigame_appearance;                // 미니게임 등장 횟수

    #region Initialize

    // 이니셜라이즈
    private void MinigameInitialize()
    {
        minigame_amount = 10;
        minigames = new GameObject[minigame_amount];
        minigame_scripts = new IMinigame[minigame_amount];
        for (int i = 0; i < minigame_amount; i++)
        { minigames[i] = Instantiate(Resources.Load<GameObject>("Prefabs/Minigame/panel" + i), this.transform); }
        for (int i = 0; i < minigame_amount; i++)
        { minigame_scripts[i] = minigames[i].GetComponent<IMinigame>(); }
        for (int i = 0; i < minigame_amount; i++)
        {
            minigame_scripts[i].MinigameInitialize();
            minigames[i].SetActive(false);
        }
        minigame_panel.SetActive(false);
        Selected_Init();
    }

    #endregion

    #region Minigame Select

    // 미니게임 선택
    private void SelectMinigame()
    {
        minigame_panel.SetActive(true);
        RandomSelect();
        minigame_appearance += 1;
        MinigameActive();
    }

    // 미니게임 랜덤 선택
    private void RandomSelect()
    {
        select_minigame = Random.Range(0, minigames.Length);
        if (!CheckMinigameLocked())
        { RandomSelect(); }
    }

    // 미니게임 잠겼는지 확인
    private bool CheckMinigameLocked()
    {
        // 미니겜 연속 등장일 경우
        if (preselect_minigame != -1 && select_minigame == preselect_minigame) { return false; }
        // 도시락싸기 미니겜일 경우
        if (select_minigame == 5)
        {
            if (DataManager.skills_save[4][3] >= 0)
            { return true; }
            else
            { return false; }
        }
        return true;
    }

    // 선택되었던 게임 초기화
    private void Selected_Init()
    {
        minigame_appearance = 0;
        preselect_minigame = -1;
        select_minigame = -1;
    }

    #endregion

    #region Minigame Controller

    // 미니게임 시작
    private void MinigameActive()
    {
        minigame_active();
        UIManager.manager.MinigameStart();
        preselect_minigame = select_minigame;

        minigames[select_minigame].SetActive(true);
        minigame_scripts[select_minigame].MinigameActive();
    }

    // 미니게임 이펙트 끝나고 진짜 시작
    private void MinigameStart()
    {
        UIManager.manager.MinigameEffect_Off();
        minigame_scripts[select_minigame].MinigameStart();
    }

    // 미니게임 끝
    public void MinigameResult(bool result)
    {
        // 미니게임 결과에 따른 이펙트
        UIManager.manager.MinigameEffect_On(result ? 1 : 2);
    }

    // 미니게임 결과에 따른 정보 설정
    public void MinigameEnd()
    {
        UIManager.manager.MinigameEffect_Off();

        for (int i = 0; i < minigames.Length; i++)
        { minigames[i].SetActive(false); }

        UIManager.manager.MinigameEnd();
        minigame_inactive();
    }

    #endregion

    // 스케줄 변경 시 초기화
    public void MinigameScheduleInitialize()
    {
        Selected_Init();
        minigame_panel.SetActive(false);
    }

    // 스케줄 받아와서 미니게임 오픈 할 함수
    public void MinigameScheduleOpen()
    {
        if (minigame_appearance < 2)
        { SelectMinigame(); }
    }

    #region Delegates

    public delegate void Minigame_UI();
    public static Minigame_UI minigame_active;
    public static Minigame_UI minigame_inactive;

    private void OnEnable() => DelegateSet();
    private void OnDisable() => DelegateDel();

    private void DelegateSet()
    {
        Data_Controller.controller_init += MinigameInitialize;
        Minigame.game_result += MinigameResult;
        Minigame_Effect.start_effect += MinigameStart;
        Minigame_Effect.result_effect += MinigameEnd;
    }

    private void DelegateDel()
    {
        for (int i = 0; i < minigame_amount; i++)
        { minigame_scripts[i].MinigameInactive(); }
        Data_Controller.controller_init -= MinigameInitialize;
        Minigame.game_result -= MinigameResult;
        Minigame_Effect.start_effect -= MinigameStart;
        Minigame_Effect.result_effect -= MinigameEnd;
    }

    #endregion
}