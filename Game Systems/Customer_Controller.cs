using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Customer_StageInfo
{
    public float spawntime;                 // 손님 생성간격
    public float[] type_probability;        // 타입 확률
    public float stop_probability;          // 구매의사 확률
    public float[] buy_probability;         // 구매확정 시 물품구매 확률
    public Vector2 position_limit;          // 리미트 포지션
    public float money_percent;             // 소지금 퍼센티지
}

public class Customer_Controller : MonoBehaviour
{
    public const int MAX_AMOUNT = 50;               // 최대 생성 한계치
    public const float BASIC_SPAWNTIME = 3f;        // 기본 생성 시간 
    public const int BASIC_SPAWNAMOUNT = 1;         // 기본 생성 개수
    public const float BASIC_SPAWNRATE = 1f;        // 기본 생성률
    public const int BASIC_SORTINGORDER = 30;       // 기본 레이어 값

    public GameObject c_pref;                       // 손님 프리팹 오브젝트
    public Transform c_list;                        // 손님 프리팹 핸들용 부모 오브젝트
    private List<GameObject> c_objs;                // 손님 게임오브젝트 풀
    private List<Customer> c_scripts;               // 손님 스크립트 풀

    private List<int> c_activated;                  // 활성화 된 손님 인덱스 저장용

    private Customer_StageInfo stage_info;          // 스테이지 인포메이션

    private string[] nomoney_msg;                   // 돈 없음 랜덤 메세지
    private string[] dislike_msg;                   // 안좋아함 랜덤 메세지

    private float spawn_time;                       // 생성 시간
    private int spawn_amount;                       // 생성 개수
    private float spawn_rate;                       // 생성률
    private float spawner;                          // 생성 제어용 시간 저장 변수

    private WaitForSeconds spawn_timer;             // 생성 타이머
    private Coroutine spawn_coroutine;              // 생성 코루틴

    private bool spawning;                          // 생성 필요 여부
    private bool working;                           // 일과 저장용

    #region ObjectPool Settings

    // 손님 컨트롤러 초기화
    private void CustomerControllerInitialize()
    {
        spawning = false;
        spawn_time = BASIC_SPAWNTIME;
        spawn_amount = BASIC_SPAWNAMOUNT;
        spawn_rate = BASIC_SPAWNRATE;

        spawn_timer = new WaitForSeconds(0.1f);
        spawner = 0;
        customer_info = new Customer_Info();
        c_activated = new List<int>();

        nomoney_msg = new string[DataManager.manager.ui.wallet_say.nomoney.Length];
        dislike_msg = new string[DataManager.manager.ui.wallet_say.dislike.Length];
        SetCustomerMsg();
        spawn_coroutine = null;
        CustomerListInitialize();
    }

    // 손님 오브젝트 풀 초기화
    private void CustomerListInitialize()
    {
        c_objs = new List<GameObject>();
        c_scripts = new List<Customer>();

        for (int i = 0; i < MAX_AMOUNT; i++)
        {
            GameObject obj = Instantiate(c_pref, c_list);
            obj.SetActive(false);
            c_objs.Add(obj);
            c_scripts.Add(obj.GetComponent<Customer>());
        }
        for (int i = 0; i < MAX_AMOUNT; i++)
        { c_scripts[i].CustomerInitialize(i); }
    }

    // SpawnRate 설정
    private void SetSpawnRate(float rate)
    {
        if (spawn_coroutine == null) { return; }
        if (rate == 0)
        {
            Spawn();
            spawn_rate = BASIC_SPAWNRATE;
        }
        else
        { spawn_rate = rate; }
        StopCoroutine(spawn_coroutine);
        StartSpawn();
    }

    #endregion

    #region Info Setter

    // 스테이지 별 손님 설정값 설정
    public void Customer_StageInfoSet(Customer_StageInfo info)
    {
        stage_info = info;
        spawn_time = info.spawntime;
    }

    // 손님 호불호 / 돈없음 메세지 언어변경 시 호출
    public void SetCustomerMsg()
    {
        for (int i = 0; i < nomoney_msg.Length; i++)
        { nomoney_msg[i] = DataManager.manager.ui.wallet_say.GetNoMoney(i); }
        for (int i = 0; i < dislike_msg.Length; i++)
        { dislike_msg[i] = DataManager.manager.ui.wallet_say.GetDislike(i); }
    }

    #endregion

    #region Spawn System

    // 생성 된 손님 개수
    public int GetActiveCustomerAmount()
    {
        int amount = 0;
        for (int i = 0; i < MAX_AMOUNT; i++)
        {
            if (c_scripts[i].NowActive())
            { amount += 1; }
        }
        return amount;
    }

    // 오브젝트 풀 내 활성화 되지않은 오브젝트 인덱스 받아오기
    public int GetInactiveIndex()
    {
        for (int i = 0; i < MAX_AMOUNT; i++)
        {
            if (!c_scripts[i].NowActive()) { return i; }
        }
        return -1;
    }

    // 한 번에 전체 생성
    public void SpawnAll()
    {
        int i = 0;
        while (i < MAX_AMOUNT)
        {
            CustomerSpawn();
            i++;
        }
    }

    // 생성 시작
    public void StartSpawn()
    {
        spawning = true;
        c_activated.Clear();
        spawn_coroutine = StartCoroutine(Spawning());
    }

    // 한 번에 생성되는 양만큼 생성
    public void Spawn()
    {
        int i = 0;
        while (i < spawn_amount)
        {
            CustomerSpawn();
            i++;
        }
    }

    // 생성 요청기간동안 계속 손님 스폰하게끔 하게 만들기
    private IEnumerator Spawning()
    {
        spawner = 0;
        while (spawning)
        {
            yield return spawn_timer;
            if (!c_skill)
            {
                spawner += 0.1f;
                if (spawner >= spawn_time * spawn_rate)
                {
                    Spawn();
                    spawner = 0;
                }
            }
        }
    }

    // 생성 멈춤
    public void StopSpawn()
    {
        spawning = false;
        StopAllCoroutines();
        spawn_coroutine = null;
        c_activated.Clear();
        for (int i = 0; i < MAX_AMOUNT; i++)
        { c_scripts[i].CustomerInactive(); }
    }

    // 일 시작 시 스폰 시작
    public void Work_SpawnSet(bool work_start)
    {
        working = work_start;
        if (work_start)
        { StartSpawn(); }
        else
        { StopSpawn(); }
    }

    #endregion

    #region Customer Info Setter

    private int select_index;                       // 손님 랜덤 생성용 인덱스
    private Customer_Info customer_info;            // 손님 정보 컨테이너

    private float rich_probability;                 // 부자타입 확률
    private float probability;                      // 확률 계산용 변수
    private float type_roll;                        // 타입 랜덤 선택용 변수
    private int code_roll;                          // 코드 랜덤 선택용 변수
    private float stop_roll;                        // 물품 구매 랜덤 변수

    // 손님 생성 및 설정
    public void CustomerSpawn()
    {
        select_index = GetInactiveIndex();
        c_activated.Add(select_index);
        TypeSet();
        CodeSet(customer_info.type);
        MoneySet(customer_info.type, customer_info.code);
        PositionSet();
        SpeedSet();
        c_scripts[select_index].CustomerInfoSet(customer_info);
        SpriteSet(select_index, customer_info.type, customer_info.code);
        LayerSet();
        StopProbabilitySet(select_index);
        c_scripts[select_index].CustomerActive();
    }

    // 손님 랜덤 타입 설정
    private void TypeSet()
    {
        rich_probability = stage_info.type_probability[stage_info.type_probability.Length - 1] * Data_Container.skill_container.toutingSkill.touting_richProbability;
        rich_probability += Data_Container.totem_container.GetTotemData(3) * 0.01f;

        probability = 0;
        for (int i = 0; i < stage_info.type_probability.Length; i++)
        {
            if (i == stage_info.type_probability.Length - 1)
            { probability += rich_probability; }
            else
            { probability += stage_info.type_probability[i]; }
        }

        type_roll = Random.Range(0.0f, probability);
        probability = 0;
        for (int i = 0; i < stage_info.type_probability.Length; i++)
        {
            probability += stage_info.type_probability[i];
            if (i == stage_info.type_probability.Length - 1)
            { probability += rich_probability; }
            if (type_roll < probability)
            { customer_info.type = i; break; }
        }
    }

    // 손님 랜덤 코드 설정
    private void CodeSet(int type)
    {
        code_roll = Random.Range(0, DataManager.manager.customers.customers[type].Length);
        if (!DataManager.customers_save[type][code_roll].spawnable)
        { CodeSet(type); }
        else
        { customer_info.code = code_roll; }
    }

    // 손님 소지금 설정
    private void MoneySet(int type, int code)
    {
        customer_info.money = Random.Range(DataManager.manager.customers.customers[type][code].money[0], DataManager.manager.customers.customers[type][code].money[1] + 1);
        customer_info.money = (int)(customer_info.money * stage_info.money_percent) + Data_Container.totem_container.GetTotemData(2);
    }

    // 손님 스폰포인트 설정
    private void PositionSet()
    {
        customer_info.position.x = Random.value > 0.5f ? -stage_info.position_limit.x : stage_info.position_limit.x;
        customer_info.position.y = Random.Range(-6.2f, stage_info.position_limit.y);
    }

    // 손님 속도 설정
    private void SpeedSet()
    {
        customer_info.speed = Random.Range(0.01f, 0.1f);
    }

    // 손님 스프라이트 설정
    private void SpriteSet(int index, int type, int code)
    {
        c_scripts[index].CustomerSpriteSet(Data_Container.customer_container.GetWalkSprites(type, code),
        Data_Container.customer_container.GetStopSprites(type, code),
        Data_Container.customer_container.GetBoughtWalkSprites(type, code),
        Data_Container.customer_container.GetBoughtStopSprite(type, code),
        Data_Container.customer_container.GetThreatenedWalkSprite(type, code),
        Data_Container.customer_container.GetThreatenedSprite(type, code));
    }

    // 손님 레이어 설정
    private void LayerSet()
    {
        for (int i = 0; i < c_activated.Count; i++)
        { SortingLayer(); }

        // 레이어 설정
        for (int i = 0; i < c_activated.Count; i++)
        { c_scripts[c_activated[i]].CustomerLayerSet(BASIC_SORTINGORDER + i); }
    }

    int highest, temp;
    // 손님 레이어 정렬 함수
    private void SortingLayer()
    {
        highest = 0;
        temp = 0;
        for (int i = 1; i < c_activated.Count; i++)
        {
            if (c_scripts[c_activated[highest]].GetEndPos() < c_scripts[c_activated[i]].GetEndPos())
            {
                temp = c_activated[highest];
                c_activated[highest] = c_activated[i];
                c_activated[i] = temp;
            }
        }
    }

    // 손님 멈출 확률
    private void StopProbabilitySet(int index)
    {
        stop_roll = Random.Range(0.0f, 1.0f);

        // 이 주의 노점일 경우 확률 업
        if (stop_roll <= stage_info.stop_probability)
        { c_scripts[index].CustomerStopSet(true); }
        else
        { c_scripts[index].CustomerStopSet(false); }
    }

    #endregion

    #region Customers Buy Logics

    public delegate void Buy_Products(int index, int money);
    public static event Buy_Products buy_products;

    // 물품 구매 고민 로직
    private void ThinkToBuy(int index)
    {
        if (working && c_scripts[index].NowActive())
        { buy_products(index, c_scripts[index].GetMoney()); }
    }

    // 물품 팁
    public int Tip(int index, int price)
    {
        if (c_scripts[index].GetMoney() >= price)
        {
            c_scripts[index].MoneyUse(price);
            return price;
        }
        else
        {
            c_scripts[index].MoneyUse(c_scripts[index].GetMoney());
            return c_scripts[index].GetMoney();
        }
    }

    // 물품 구매 O
    public void BuyProduct(int index, int product_price, Sprite[] product, int product_amount)
    {
        c_scripts[index].BuyProduct(product_price, product, product_amount);
    }

    // 물품 구매 X
    public void NotBuyProduct(int index, bool nomoney)
    {
        if (nomoney)
        {
            int rand = Random.Range(0, nomoney_msg.Length);
            c_scripts[index].NotBuyProduct(nomoney_msg[rand]);
        }
        else
        {
            int rand = Random.Range(0, dislike_msg.Length);
            c_scripts[index].NotBuyProduct(dislike_msg[rand]);
        }
    }

    #endregion

    // 아예 전체 삭제
    public void DestroyAll()
    {
        for (int i = 0; i < c_objs.Count; i++)
        { Destroy(c_objs[i].gameObject); }
        c_objs.Clear();
        DelegateDel();
    }

    #region Customer Info Logic

    // 손님 타입 반환
    public int GetCustomerType(int index) => c_scripts[index].GetCType();

    // 손님 코드 반환
    public int GetCustomerCode(int index) => c_scripts[index].GetCCode();

    // 손님 위치 반환
    public Vector2 GetCustomerPosition(int index) => c_scripts[index].GetPosition();

    // 손님 상태 변경
    public void SetCustomers(string effect, float effect_data)
    {
        switch (effect)
        {
            case "state":
                if (effect_data == 0)
                {
                    for (int i = 0; i < MAX_AMOUNT; i++)
                    {
                        if (c_scripts[i].NowActive())
                        { c_scripts[i].CustomerStateSet(1); }
                    }
                }
                else
                {
                    for (int i = 0; i < MAX_AMOUNT; i++)
                    {
                        if (c_scripts[i].NowActive())
                        { c_scripts[i].CustomerStateSet(2); }
                    }
                }
                break;
            case "spawnrate":
                SetSpawnRate(effect_data);
                break;
        }
    }

    #endregion

    #region Customer Skill Logic

    public static event Buy_Products customer_beaten;       // 손님 맞음!

    public delegate void MoneyView();
    public static event MoneyView customer_moneyview;

    private int c_skill_index = -1;                 // 사용중 스킬 인덱스
    private bool c_skill;                           // 스킬 온오프

    // 액티브스킬 사용 시 처리부
    public void UseActiveSkill(int index, bool active)
    {
        c_skill = active;

        if (!active)
        { c_skill_index = -1; }
        else
        { c_skill_index = index; }

        for (int i = 0; i < MAX_AMOUNT; i++)
        {
            if (c_scripts[i].NowActive())
            { c_scripts[i].Skill_Check(active); }
        }

        switch (index)
        {
            case 1:
                for (int i = 0; i < MAX_AMOUNT; i++)
                {
                    if (c_scripts[i].NowActive())
                    { c_scripts[i].Fevertime(active); }
                }
                break;
            case 4:
                if (active)
                {
                    for (int i = 0; i < MAX_AMOUNT; i++)
                    {
                        if (c_scripts[i].NowActive() && c_scripts[i].GetBeatable())
                        { c_scripts[i].PickableSet(active); }
                    }
                }
                else
                {
                    for (int i = 0; i < MAX_AMOUNT; i++)
                    {
                        if (c_scripts[i].NowActive())
                        { c_scripts[i].PickableSet(active); }
                    }
                }
                break;
            case 10:
                if (active)
                {
                    for (int i = 0; i < MAX_AMOUNT; i++)
                    {
                        if (c_scripts[i].NowActive() && c_scripts[i].GetCanNotice())
                        { c_scripts[i].PickableSet(active); }
                    }
                }
                else
                {
                    for (int i = 0; i < MAX_AMOUNT; i++)
                    {
                        if (c_scripts[i].NowActive())
                        { c_scripts[i].PickableSet(active); }
                    }
                }
                break;
        }
    }

    // 액티브스킬 견 후 선택된 인덱스 스킬 처리
    private void AcitveSkillPicked(int index)
    {
        switch (c_skill_index)
        {
            case 1:
                break;
            case 4:
                customer_beaten(index, c_scripts[index].GetMoney());
                c_scripts[index].Beat();
                break;
            case 10:
                customer_moneyview();
                c_scripts[index].Notice();
                break;
        }
    }

    // 손님 매혹
    public void Customer_Charm(int amount)
    {
        if (amount >= GetActiveCustomerAmount())
        {
            for (int i = 0; i < c_activated.Count; i++)
            {
                if (c_scripts[i].NowActive())
                { c_scripts[i].ForcefulBuy(); }
            }
        }
        else
        {
            int count = 0;
            for (int i = 0; i < MAX_AMOUNT; i++)
            {
                if (c_scripts[i].NowActive())
                {
                    c_scripts[i].ForcefulBuy();
                    count += 1;
                    if (count > amount) { break; }
                }
            }
        }
    }

    #endregion

    #region Delegates

    private void OnEnable() => DelegateSet();
    private void OnDisable() => DelegateDel();

    // 델리게이트 설정
    private void DelegateSet()
    {
        Data_Controller.controller_init += CustomerControllerInitialize;

        UI_LanguageSelect.language_changed += SetCustomerMsg;
        Customer.stopped += ThinkToBuy;
        Customer.picked += AcitveSkillPicked;
        Effect_Customer_ForcefulBuy.customer_forced_end += ThinkToBuy;
        GameManager.game_ending += DelegateDel;
    }

    // 델리게이트 해제
    private void DelegateDel()
    {
        Data_Controller.controller_init -= CustomerControllerInitialize;

        UI_LanguageSelect.language_changed -= SetCustomerMsg;
        Customer.stopped -= ThinkToBuy;
        Customer.picked -= AcitveSkillPicked;
        Effect_Customer_ForcefulBuy.customer_forced_end -= ThinkToBuy;
        GameManager.game_ending -= DelegateDel;
    }

    #endregion
}