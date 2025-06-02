using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage_Controller : MonoBehaviour
{
    private Transform stall_transform;          // 노점 위치 포지션
    private Transform stall_player_transform;   // 플레이어 위치 포지션
    private GameObject[] stage_objects;         // 현재 생성된 스테이지 오브젝트들
    private GameObject[] stall_objects;         // 현재 생성된 노점 오브젝트들
    private Stage_Direction[] street_trash;     // 길에 떨어진 쓰레기 제어용

    private bool stage_changeable;              // 스테이지 변경 가능한지
    private bool stage_activatable;             // 스테이지 액티브 가능한지

    private int now_stage;                      // 현재 선택 된 스테이지
    private int next_stage;                     // 다음 선택 된 스테이지
    private int stage_type;                     // 스테이지 타입
    private int stage_code;                     // 스테이지 코드

    private int now_stall;                      // 현재 선택 된 노점
    private int stall_type;                     // 노점 타입
    private int stall_code;                     // 노점 코드

    private Customer_StageInfo customer_info;   // 손님관련 정보 전달용
    private Product_StallInfo product_info;     // 물품관련 정보 전달용

    // 초기화
    private void ControllerInitialize()
    {
        stall_transform = this.transform.Find("Stall");
        stall_player_transform = stall_transform.Find("player_pos");

        now_stage = DataManager.manager.stage_save.now_stage;
        next_stage = DataManager.manager.stage_save.next_stage;

        now_stall = DataManager.manager.stall_save.now_stall;
        stall_type = Data_Container.stall_container.GetStallType(now_stall);
        stall_code = Data_Container.stall_container.GetStallCode(now_stall);

        stage_activatable = false;
        stage_changeable = true;

        customer_info = new Customer_StageInfo();
        product_info = new Product_StallInfo();

        SetCustomerInfo();
        SetProductInfo();

        StageInitialize();
        StallInitialize();
    }

    #region Variable

    // 물품관련 정보 설정
    private void SetProductInfo()
    {
        product_info.max_amount = DataManager.manager.stalls.stalls[stall_type][stall_code].product_display.Length;
        product_info.spawntime = DataManager.manager.stalls.stalls[stall_type][stall_code].product_spawntime;
        product_info.type_probability = DataManager.manager.stalls.stalls[stall_type][stall_code].product_probability;
        product_info.product_position = DataManager.manager.stalls.stalls[stall_type][stall_code].product_display;

        customer_info.stop_probability = DataManager.manager.stalls.stalls[stall_type][stall_code].customer_stop_probability;
    }

    // 물품관련 정보 
    public Product_StallInfo GetProductStallInfo()
    { return product_info; }

    // 손님관련 정보설정
    private void SetCustomerInfo()
    {
        customer_info.spawntime = DataManager.manager.stages.stages[stage_type][stage_code].customer_spawntime;
        customer_info.type_probability = DataManager.manager.stages.stages[stage_type][stage_code].customer_spawn_probability;
        customer_info.buy_probability = DataManager.manager.stages.stages[stage_type][stage_code].customer_buy_probability;
        customer_info.position_limit = DataManager.manager.stages.stages[stage_type][stage_code].customer_position_limit;
        customer_info.money_percent = DataManager.manager.stages.stages[stage_type][stage_code].customer_money_percent;
    }

    // 손님관련 정보
    public Customer_StageInfo GetCustomerStageInfo()
    { return customer_info; }

    #endregion

    #region Stages

    // 스테이지 사운드 설정 (이동수단 시작 시 변경)
    public void Stage_BGM()
    {
        if (next_stage < 0) { return; }
        GameManager.manager.GetSoundManager().ChangeBGM(Data_Container.stage_container.GetStageBGM(Data_Container.stage_container.GetStageType(next_stage), Data_Container.stage_container.GetStageCode(next_stage)));
    }

    // 스테이지 이니셜라이즈
    private void StageInitialize()
    {
        stage_objects = new GameObject[Data_Container.stage_container.GetStageAmount()];
        street_trash = new Stage_Direction[stage_objects.Length];
        stage_type = now_stage / 6;
        stage_code = now_stage % 6;

        Stage_Instantiate();
        GameManager.manager.GetSoundManager().ChangeBGM(Data_Container.stage_container.GetStageBGM(stage_type, stage_code));
    }

    // 스테이지 생성
    private void Stage_Instantiate()
    {
        stage_objects[now_stage] = Instantiate(Data_Container.stage_container.GetStage(now_stage), this.transform);
        stage_objects[now_stage].SetActive(false);
        street_trash[now_stage] = stage_objects[now_stage].GetComponent<Stage_Direction>();
    }

    // 스테이지 자동 변경 (next_stage 설정되어있는 경우)
    private void Stage_Change()
    {
        if (next_stage < 0) { return; }

        now_stage = next_stage;
        next_stage = -1;
        now_stage_delegate(now_stage);

        stage_type = Data_Container.stage_container.GetStageType(now_stage);
        stage_code = Data_Container.stage_container.GetStageCode(now_stage);

        Stage_Save();
    }

    // 스테이지 지정 변경 (저널에서 선택 시 호출)
    private void Stage_Change(int index)
    {
        if (stage_changeable)
        { next_stage = index; }
        Stage_Save();
    }

    // 스테이지 저장
    private void Stage_Save()
    {
        DataManager.manager.stage_save.now_stage = now_stage;
        DataManager.manager.stage_save.next_stage = next_stage;
    }

    // 스테이지 액티브 설정
    private void Stage_Active()
    {
        Stage_Unlocks_Active();

        for (int i = 0; i < stage_objects.Length; i++)
        {
            if (stage_objects[i] != null)
                stage_objects[i].SetActive(false);
        }
        if (stage_objects[now_stage] != null)
            stage_objects[now_stage].SetActive(true);
        else
        {
            Stage_Instantiate();
            stage_objects[now_stage].SetActive(true);
        }

        Stage_Condition();
        stage_light(DataManager.manager.stages.stages[stage_type][stage_code].light_intensity);
    }

    // 스테이지 컨디션 받아오기
    private void Stage_Condition()
    {
        if (now_stage == 0)
        {
            if (!DataManager.manager.ending_save.ending[0].unlocked && DataManager.manager.player_save.hidden_conditions.newbie)
            { stage_condition(3, 0, true); }
        }
        else
        {
            if (DataManager.manager.stages.stages[stage_type][stage_code].stage_condition > -1)
            {
                stage_condition(3, DataManager.manager.stages.stages[stage_type][stage_code].stage_condition, true);
            }
        }
    }

    // 스테이지 인액티브 설정
    private void Stage_Inactive()
    {
        for (int i = 0; i < stage_objects.Length; i++)
        {
            if (stage_objects[i] != null)
                stage_objects[i].SetActive(false);
        }
    }

    // 스테이지 쓰레기 온오프
    public void Stage_Direction(string direction, bool active)
    {
        if (direction == "trash")
            street_trash[now_stage].Stage_Trash(active);
        else
            street_trash[now_stage].Stage_Sinkhole(active);
    }

    #region Get Info

    // 스테이지 체력 받아오기
    public int GetStageLooseHP()
    { return DataManager.manager.stages.stages[stage_type][stage_code].loose_health; }

    // 스테이지 타입 받아오기
    public int GetStageType()
    { return stage_type; }

    // 스테이지 코드 받아오기
    public int GetStageCode()
    { return stage_code; }

    // 스테이지 받아오기 (전체 코드)
    public int GetStage()
    {
        if (next_stage >= 0) return next_stage;
        return now_stage;
    }

    #endregion

    #endregion

    #region Stalls

    // 노점 이니셜라이즈
    private void StallInitialize()
    {
        stall_objects = new GameObject[Data_Container.stall_container.GetStallAmount()];
        Stall_Instantiate();
    }

    // 노점 생성
    private void Stall_Instantiate()
    {
        stall_objects[now_stall] = Instantiate(Data_Container.stall_container.GetStall(now_stall), stall_transform);
        stall_objects[now_stall].SetActive(false);
    }

    // 노점 변경
    public void Stall_InfoChange(int index)
    {
        now_stall = index;
        stall_type = Data_Container.stall_container.GetStallType(now_stall);
        stall_code = Data_Container.stall_container.GetStallCode(now_stall);

        Stall_Save();
        SetProductInfo();
    }

    // 노점 구매
    private void Stall_Buy(int index)
    {
        // 머니 충분할 경우 체크
        DataManager.stalls_save[Data_Container.stall_container.GetStallType(index)][Data_Container.stall_container.GetStallCode(index)] = true;
    }

    // 노점 저장
    private void Stall_Save()
    {
        DataManager.manager.stall_save.now_stall = now_stall;
    }

    // 노점 액티브 설정
    public void Stall_Active()
    {
        Stall_PositionSet();
        Stall_PlayerPositionSet();

        for (int i = 0; i < stall_objects.Length; i++)
        {
            if (stall_objects[i] != null)
            { stall_objects[i].SetActive(false); }
        }
        if (stall_objects[now_stall] != null)
        { stall_objects[now_stall].SetActive(true); }
        else
        {
            Stall_Instantiate();
            stall_objects[now_stall].SetActive(true);
        }
    }

    // 노점 인액티브 설정
    public void Stall_Inactive()
    {
        for (int i = 0; i < stall_objects.Length; i++)
        {
            if (stall_objects[i] != null)
            { stall_objects[i].SetActive(false); }
        }
    }

    // 노점 위치 설정
    private void Stall_PositionSet()
    {
        stall_transform.localPosition = DataManager.manager.stages.stages[stage_type][stage_code].stall_display;
    }

    // 플레이어 위치 설정
    private void Stall_PlayerPositionSet()
    {
        stall_player_transform.localPosition = DataManager.manager.stalls.stalls[stall_type][stall_code].player_display;
        stall_playerpos(stall_player_transform.position);
    }

    #endregion

    // 일 시작
    private void Work_Start()
    {
        Stage_Active();
        Stall_Active();
    }

    // 일 끝
    private void Work_End()
    {
        Stage_Inactive();
        Stage_Unlocks_Inactive();

        Stall_Inactive();
    }

    // 스테이지 해금정보 전달
    private void Stage_Unlocks_Active()
    {
        // 손님 해금 정보
        for (int i = 0; i < DataManager.manager.stages.stages[stage_type][stage_code].customer_unlock_type.Length; i++)
        {
            DataManager.customers_save[DataManager.manager.stages.stages[stage_type][stage_code].customer_unlock_type[i]][DataManager.manager.stages.stages[stage_type][stage_code].customer_unlock_code[i]].spawnable = true;
        }

        // 물품 해금 정보(한 번 해금하고 나면 지속적으로 생성)
        for (int i = 0; i < DataManager.manager.stages.stages[stage_type][stage_code].product_unlock_type.Length; i++)
        {
            if (!DataManager.products_save[DataManager.manager.stages.stages[stage_type][stage_code].product_unlock_type[i]][DataManager.manager.stages.stages[stage_type][stage_code].product_unlock_code[i]].spawnable)
                DataManager.products_save[DataManager.manager.stages.stages[stage_type][stage_code].product_unlock_type[i]][DataManager.manager.stages.stages[stage_type][stage_code].product_unlock_code[i]].spawnable = true;
        }
    }
    private void Stage_Unlocks_Inactive()
    {
        // 손님 해금 정보 (특정 스테이지 내에서만 생성하게끔 하기)
        for (int i = 0; i < DataManager.manager.stages.stages[stage_type][stage_code].customer_unlock_type.Length; i++)
        {
            DataManager.customers_save[DataManager.manager.stages.stages[stage_type][stage_code].customer_unlock_type[i]][DataManager.manager.stages.stages[stage_type][stage_code].customer_unlock_code[i]].spawnable = false;
        }
    }

    // 스테이지 변경 가능여부 정보
    public bool GetStageChangeable() => stage_changeable;
    public void SetStageChangeable(bool changable) => stage_changeable = changable;

    // 스테이지 설정 (외부 핸들링)
    public void StageWork(bool work_start)
    {
        if (work_start)
        {
            stage_activatable = true;
            stage_changeable = false;
            Stage_Change();
            Work_Start();
        }
        else
        {
            if (stage_activatable)
            {
                Work_End();
                stage_activatable = false;
            }
        }
    }

    #region Delegates

    // 현재 스테이지/노점 전달
    public delegate void Stage_Delegate(int now_stage);
    public static event Stage_Delegate now_stage_delegate;
    public static event Stage_Delegate now_stall_delegate;

    // 스테이지 별 메인 라이트 값 설정
    public delegate void Stage_Light(float intensity);
    public static event Stage_Light stage_light;


    // 노점 별 플레이어 위치 설정
    public delegate void Stall_PlayerPosSet(Vector2 position);
    public static event Stall_PlayerPosSet stall_playerpos;

    // 스테이지 버프/디버프 설정
    public delegate void Stage_ConditionSet(int type, int code, bool active);
    public static event Stage_ConditionSet stage_condition;

    private void OnEnable() => DelegateSet();
    private void OnDisable() => DelegateDel();

    private void DelegateSet()
    {
        Data_Controller.controller_init += ControllerInitialize;

        UI_Inventory.buy_stall += Stall_Buy;
        UI_Journal.stage_change += Stage_Change;
        UI_Journal.check_stage_changeable += GetStageChangeable;
    }

    private void DelegateDel()
    {
        Data_Controller.controller_init -= ControllerInitialize;

        UI_Inventory.buy_stall -= Stall_Buy;
        UI_Journal.stage_change -= Stage_Change;
        UI_Journal.check_stage_changeable -= GetStageChangeable;
    }

    #endregion

}
