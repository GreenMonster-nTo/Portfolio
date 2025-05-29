using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NUnit.Framework;

public enum product_type { normal, common, rare, epic, great, special }
public enum customer_type { change_wallet, half_wallet, long_wallet, rich }
public class UI_Journal : UI_Panel
{
    public GameObject journal;                  // 저널 오브젝트

    private const int SLOT = 6;                 // 슬롯 개수
    private const int ACTIVE_POS = 450;         // 버튼 액티브 시 X 포지션
    private const int INACTIVE_POS = 425;       // 버튼 액티브 아닐 시 X 포지션

    public GameObject[] journal_pages;          // 저널 페이지들
    public Button[] journal_btns;               // 저널 버튼들
    public GameObject[] journal_new_token;      // 저널 뉴 토큰
    public Text journal_page_txt;               // 저널 페이지 텍스트
    public GameObject[] journal_page_btn;       // 저널 페이지 버튼
    public GameObject journal_pages_btn;        // 저널 페이지 한 번에 변경하는 버튼
    public Sprite[] journal_pages_btn_sprite;   // 저널 페이지버튼 스프라이트

    private Color journal_sl_color;             // 저널 실루엣 컬러
    private Vector3 journal_slot_size;          // 저널 슬롯 사이즈
    private Vector2[] journal_slot_pivot;       // 저널 슬롯 피봇

    private int journal_category;               // 현재 저널 카테고리
    private bool journal_info;                  // 저널 인포메이숀인지

    private int now_page, max_page;             // 페이지
    private int now_info, max_info;             // 데이터
    private int max_tier;                       // 티어

    public GameObject map_new;                  // 맵 버튼 뉴토큰
    private bool[] journal_new_token_active;    // 저널 뉴토큰 액티브상태인지

    private int reward_soulseed, reward_mote, reward_sp;

    // 저널 판넬 이니셜라이즈
    private void InitializeJournalPanel()
    {
        UI_Initialize("journal");

        now_page = 0; max_page = 0; now_info = 0; max_info = 0; max_tier = 0;
        reward_soulseed = 0; reward_mote = 0; reward_sp = 0;
        journal_sl_color = new Color(255 / 255f, 215 / 255f, 0 / 255f);
        journal_slot_size = new Vector3(0.4f, 0.4f, 0.4f);
        journal_slot_pivot = new Vector2[2];
        journal_slot_pivot[0] = new Vector2(0.5f, 0.0f);
        journal_slot_pivot[1] = new Vector2(0.5f, 0.1f);
        journal_new_token_active = new bool[5];
        ProductPanelInitialize();
        CustomerPanelInitialize();
        EndingPanelInitialize();
        StagePanelInitialize();
        EventPanelInitialize();
        NewTokenInitialize();
    }

    // 저널 판넬 열기
    public void JournalPanel_active()
    {
        UI_Open();
        reward_soulseed = 0; reward_mote = 0; reward_sp = 0;
        journal.SetActive(true);
        Journal_Category(0);
    }

    // 저널 판넬 닫기
    public void JournalPanel_inactive()
    {
        journal_reward(reward_soulseed, reward_mote, reward_sp);
        reward_soulseed = 0; reward_mote = 0; reward_sp = 0;
        journal.SetActive(false);
        UI_Close();
    }

    // 맵 열기
    public void JournalMap_active()
    {
        UI_Open();
        journal.SetActive(true);
        Journal_Category(3);
    }

    // 저널 카테고리 페이지 변경
    public void Journal_Category(int category)
    {
        journal_category = category;
        for (int i = 0; i < journal_pages.Length; i++)
        {
            journal_pages[i].SetActive(false);
            CategoryBtnSet(i, true);
        }
        Journal_Category_PageOpen();
    }

    // 저널 카테고리 페이지
    private void Journal_Category_PageOpen()
    {
        journal_pages[journal_category].SetActive(true);
        CategoryBtnSet(journal_category, false);
        journal_info = false;

        switch (journal_category)
        {
            case 0: ProductInitialize(); Product(); break;
            case 1: CustomerInitialize(); Customer(); break;
            case 2: EndingInitialize(); Ending(now_page); break;
            case 3: StageInitialize(); Stage(now_page); break;
            case 4: EventInitialize(); GameEvent(); break;
        }
    }

    // 저널 보상풀 설정용
    private void Journal_Reward_Setter(int index)
    {
        switch (index)
        {
            case 0:
                reward_mote += 30;
                reward_sp += 2;
                break;
            case 1:
                reward_soulseed += 2;
                reward_sp += 10;
                break;
        }
    }

    #region product

    private GameObject journal_product;         // 저널 물품 페이지
    private GameObject journal_product_info;    // 저널 물품 상세페이지
    private Text product_percentage;            // 저널 물품 언락 퍼센테이지

    private List<GameObject> product_slots;     // 저널 물품 슬롯들
    private List<GameObject> product_token;     // 저널 물품 뉴 토큰
    private List<Text> product_slot_txt;        // 저널 물품 슬롯 텍스트
    private List<Image> product_slot_img;       // 저널 물품 슬롯 이미지

    private Text product_name;                  // 저널 물품 이름
    private Image product_img;                  // 저널 물품 이미지
    private Text product_sold_amount;           // 저널 물품 판매 양
    private Text product_price;                 // 저널 물품 가격
    private Text product_description;           // 저널 물품 설명

    private Color product_spawnable_color;      // 스폰 가능 컬러

    // 저널 물품 페이지 판넬 초기화
    private void ProductPanelInitialize()
    {
        journal_product = journal_pages[0].transform.Find("Products").gameObject;
        journal_product_info = journal_pages[0].transform.Find("Product_Info").gameObject;

        product_percentage = journal_product.transform.Find("unlock_amount").GetComponent<Text>();
        product_slots = new List<GameObject>();
        product_token = new List<GameObject>();
        product_slot_txt = new List<Text>();
        product_slot_img = new List<Image>();

        for (int i = 0; i < SLOT; i++)
        {
            product_slots.Add(journal_product.transform.Find("product_" + i).gameObject);
            product_token.Add(product_slots[i].transform.Find("new").gameObject);
            product_slot_txt.Add(journal_product.transform.Find("product_" + i + "_txt").GetComponent<Text>());
            product_slot_img.Add(journal_product.transform.Find("product_" + i).GetComponent<Image>());
        }
        product_name = journal_product_info.transform.Find("Product_Name").GetComponent<Text>();
        product_img = journal_product_info.transform.Find("Background").Find("Product_Img").GetComponent<Image>();
        product_sold_amount = journal_product_info.transform.Find("Background").Find("Sold_Amount").GetComponent<Text>();
        product_price = journal_product_info.transform.Find("Background").Find("Product_Price").GetComponent<Text>();
        product_description = journal_product_info.transform.Find("Product_Description").GetComponent<Text>();
        product_spawnable_color = new Color(180 / 255f, 144 / 255f, 150 / 255f, 255 / 255f);
    }

    // 저널 물품 페이지 초기화
    private void ProductInitialize()
    {
        max_tier = DataManager.manager.products.products.Length + 1;
        max_info = GetProductAmount();
        max_page = GetProductPage();
        now_info = 0;
        now_page = Product_CheckNewTokenPage();
        PageButtonSet(true);
    }

    // 저널 물품 페이지 오픈
    private void Product()
    {
        GameManager.manager.GetSoundManager().Journal();
        journal_product_info.SetActive(false);
        journal_product.SetActive(true);
        journal_info = false;

        TextUI(journal_page_txt, (now_page + 1) + " / " + max_page);

        if (GetProductUnlockPercentage() > 0)
        { TextUI(product_percentage, GetProductUnlockPercentage().ToString("F2") + " %"); }
        else TextUI(product_percentage, "0 %");

        for (int i = 0; i < SLOT; i++)
        {
            int type = GetPageProductTier(now_page);
            if (type < DataManager.manager.products.products.Length)
            {
                if (i + (now_page * SLOT) < GetProductSlotAmount(type - 1) + DataManager.manager.products.products[type].Length)
                {
                    int code = (i + (now_page * SLOT)) - GetProductSlotAmount(type - 1);
                    SetProduct_Slot(i, type, code, true);
                }
                else
                    SetProduct_Slot(i, false);
            }
            else
            {
                if (i + (now_page * SLOT) < GetProductSlotAmount(type - 1) + (DataManager.manager.foods.foods[0].Length * 3) + (DataManager.manager.foods.foods[1].Length * 4))
                {
                    int code = (i + (now_page * SLOT)) - GetProductSlotAmount(type - 1);
                    SetProduct_Slot(i, type, code, true);
                }
            }
        }
        PageButtonSet();
    }

    // 저널 물품 디테일 뷰
    private void Product_Info(int type, int code)
    {
        GameManager.manager.GetSoundManager().Journal();
        if (!DataManager.products_save[type][code].looked)
        {
            DataManager.products_save[type][code].looked = true;
            Journal_Reward_Setter(0);
            Product_CheckNewToken();
        }

        journal_product_info.SetActive(true);
        journal_product.SetActive(false);
        journal_info = true;

        now_info = GetProductAmount(type - 1) + code;

        TextUI(journal_page_txt, (now_info + 1) + " / " + max_info);

        if (type < DataManager.manager.products.products.Length)
        {
            TextUI(product_name, DataManager.manager.products.products[type][code].GetName());
            ImageUI(product_img, GetProductImg(type, code));
            TextUI(product_sold_amount, DataManager.products_save[type][code].sold_amount);
            TextUI(product_price, DataManager.manager.products.products[type][code].price);
            TextUI(product_description, DataManager.manager.products.products[type][code].GetDescription());
        }
        else
        {
            TextUI(product_name, GetProductName(type, code));
            ImageUI(product_img, GetProductImg(type, code));
            TextUI(product_sold_amount, DataManager.products_save[type][code].sold_amount);
            TextUI(product_price, DataManager.manager.foods.foods[GetLunchOrDinner(code)][GetFoodStage(code)].set[GetFoodCode(code)].price);
            TextUI(product_description, DataManager.manager.foods.foods[GetLunchOrDinner(code)][GetFoodStage(code)].set[GetFoodCode(code)].GetDescription());
        }

        PageButtonSet();
        PageBothButtonSet();
    }

    // 물품 타입 리턴
    private int GetProductType(int amount)
    {
        for (int i = 0; i < max_tier; i++)
        {
            if (amount < GetProductAmount(i))
                return i;
        }
        return -1;
    }

    // 물품 코드 리턴
    private int GetProductCode(int amount)
    {
        for (int i = 0; i < max_tier; i++)
        {
            if (amount < GetProductAmount(i))
                return amount - GetProductAmount(i - 1);
        }
        return -1;
    }

    // 저널 물품 전체 개수
    private int GetProductAmount()
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
            amount += DataManager.products_save[i].Length;
        return amount;
    }

    // 저널 물품 티어별 개수
    private int GetProductAmount(int tier)
    {
        int amount = 0;
        for (int i = 0; i < tier + 1; i++)
            amount += DataManager.products_save[i].Length;
        return amount;
    }

    // 저널 물품 티어별 할당 슬롯 개수
    private int GetProductSlotAmount(int tier)
    {
        int amount = 0;
        for (int i = 0; i < tier + 1; i++)
            amount += GetProductPage(i) * SLOT;
        return amount;
    }

    // 저널 물품 페이지 전체 개수
    private int GetProductPage()
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
            amount += GetProductPage(i);
        return amount;
    }

    // 저널 티어 최대 페이지
    private int GetProductPage(int tier)
    {
        int amount = 0;

        if (DataManager.products_save[tier].Length % SLOT == 0)
        {
            if (DataManager.products_save[tier].Length < SLOT)
                amount = DataManager.products_save[tier].Length / SLOT - 1;
            else
                amount = DataManager.products_save[tier].Length / SLOT;
        }
        else
            amount = DataManager.products_save[tier].Length / SLOT + 1;
        return amount;
    }

    // 저널 페이지 가져오기
    private int GetProductPage(int tier, int code)
    {
        int amount = 0;
        int code_page = 0;
        for (int i = 0; i < tier; i++)
            amount += GetProductPage(i);

        if (code > SLOT)
            code_page = code / SLOT;
        amount += code_page;
        return amount;
    }

    // 저널 페이지로 티어 받아오기
    private int GetPageProductTier(int page)
    {
        int amount = 0;

        for (int i = 0; i < max_tier; i++)
        {
            amount += GetProductPage(i);
            if (page < amount) return i;
        }
        return -1;
    }

    // 저널 물품 해금 퍼센티지 계산
    private float GetProductUnlockPercentage()
    {
        int unlocked = 0;
        for (int i = 0; i < max_tier; i++)
        {
            for (int j = 0; j < DataManager.products_save[i].Length; j++)
            {
                if (DataManager.products_save[i][j].unlocked) unlocked += 1;
            }
        }
        float percent = (float)unlocked / (float)GetProductAmount();
        return percent * 100;
    }

    // 저널 물품 이름 문자열 받아오기
    private string GetProductName(int type, int code)
    {
        string product_name = null;

        if (DataManager.products_save[type][code].unlocked)
        {
            if (type < DataManager.manager.products.products.Length)
            { product_name = DataManager.manager.products.products[type][code].GetName(); }
            else
            { product_name = DataManager.manager.foods.foods[GetLunchOrDinner(code)][GetFoodStage(code)].set[GetFoodCode(code)].GetName(); }
        }
        else product_name = "???";

        return product_name;
    }

    // 저널 물품 이미지 받아오기
    private Sprite GetProductImg(int type, int code)
    {
        if (type < DataManager.manager.products.products.Length)
        {
            if (DataManager.products_save[type][code].unlocked)
            {
                return Data_Container.product_container.GetJournalSprite(type, code);
            }
            else
            {
                return Data_Container.product_container.GetSilhouette(type, code);
            }
        }
        else
        {
            if (DataManager.products_save[type][code].unlocked)
            {
                return Data_Container.pamphlet_container.GetFood(GetLunchOrDinner(code), GetFoodStage(code), GetFoodCode(code));
            }
            else
            {
                return Data_Container.pamphlet_container.GetFoodSilhouette(GetLunchOrDinner(code), GetFoodStage(code), GetFoodCode(code));
            }
        }
    }

    // 저널 페이지 슬롯 설정
    private void SetProduct_Slot(int index, bool active)
    {
        product_slots[index].SetActive(active);
        product_slot_txt[index].gameObject.SetActive(active);
    }
    private void SetProduct_Slot(int index, int type, int code, bool active)
    {
        SetProduct_Slot(index, active);
        ImageUI(product_slots[index], GetProductImg(type, code));
        TextUI(product_slot_txt[index], GetProductName(type, code));

        // 흑백 실루엣
        if (!DataManager.products_save[type][code].spawnable && !DataManager.products_save[type][code].unlocked)
            UIManager.manager.ImageColorUI(product_slot_img[index], product_spawnable_color);
        else
            UIManager.manager.ImageColorUI(product_slot_img[index], Color.white);

        // 뉴 토큰
        if (DataManager.products_save[type][code].unlocked && !DataManager.products_save[type][code].looked)
            product_token[index].SetActive(true);
        else
            product_token[index].SetActive(false);

        product_slots[index].GetComponent<EventTrigger>().triggers.Clear();

        if (DataManager.products_save[type][code].unlocked)
        {
            product_slots[index].GetComponent<Touch_Scale>().SetTouchable(true);

            EventTrigger.Entry pointer_click = new EventTrigger.Entry();
            pointer_click.eventID = EventTriggerType.PointerClick;
            pointer_click.callback.AddListener((data) => Product_Info(type, code));
            product_slots[index].GetComponent<EventTrigger>().triggers.Add(pointer_click);
        }
        else
            product_slots[index].GetComponent<Touch_Scale>().SetTouchable(false);
    }

    // 음식 데이터 점심인지 저녁인지
    private int GetLunchOrDinner(int code)
    {
        if (code - (DataManager.manager.foods.foods[0].Length * 3) < 0)
        { return 0; }
        else
        { return 1; }
    }

    // 음식 데이터 스테이지 가져오기
    private int GetFoodStage(int code)
    {
        if (GetLunchOrDinner(code) == 0)
        { return code / 3; }
        else
        { return (code - (DataManager.manager.foods.foods[0].Length * 3)) / 4; }
    }

    // 음식 데이터 코드 가져오기
    private int GetFoodCode(int code)
    {
        if (GetLunchOrDinner(code) == 0)
        { return code % 3; }
        else
        { return (code - (DataManager.manager.foods.foods[0].Length * 3)) % 4; }
    }

    // 뉴 토큰 있는지 체크
    private void Product_CheckNewToken()
    {
        int count = 0;
        for (int i = 0; i < DataManager.products_save.Length; i++)
        {
            for (int j = 0; j < DataManager.products_save[i].Length; j++)
            {
                if (DataManager.products_save[i][j].unlocked && !DataManager.products_save[i][j].looked)
                    count += 1;
            }
        }
        if (count > 0)
            NewTokenSet(0, true);
        else
            NewTokenSet(0, false);
    }

    // 저널 뉴 토큰 활성화 페이지 가져오기
    private int Product_CheckNewTokenPage()
    {
        for (int i = 0; i < DataManager.products_save.Length; i++)
        {
            for (int j = 0; j < DataManager.products_save[i].Length; j++)
            {
                if (DataManager.products_save[i][j].unlocked && !DataManager.products_save[i][j].looked)
                {
                    return GetProductPage(i, j);
                }
            }
        }
        return 0;
    }

    // 저널 물품 언락
    private void SetProduct_Unlock(int type, int code)
    {
        DataManager.products_save[type][code].sold_amount += 1;
        if (!DataManager.products_save[type][code].unlocked)
        {
            DataManager.products_save[type][code].unlocked = true;
            NewTokenSet(0, true);
        }
    }

    #endregion

    #region customer

    private GameObject journal_customer;        // 저널 손님 페이지
    private GameObject journal_customer_info;   // 저널 손님 상세페이지
    private Text customer_percentage;           // 저널 손님 언락 퍼센테이지

    private List<GameObject> customer_slots;    // 저널 손님 슬롯들
    private List<GameObject> customer_token;    // 저널 손님 뉴 토큰
    private List<Text> customer_slot_txt;       // 저널 손님 슬롯 텍스트

    private Text customer_name;                 // 저널 손님 이름
    private Image customer_img;                 // 저널 손님 이미지
    private Text customer_bought_amount;        // 저널 손님 구매 양
    private Image customer_unlock_product;      // 저널 손님의 해금 물품 이미지
    private GameObject customer_product_img;    // 저널 손님의 해금 물품 이미지 담아줄 포스트잇
    private Text customer_description;          // 저널 손님 설명

    // 저널 손님 페이지 판넬 초기화
    private void CustomerPanelInitialize()
    {
        journal_customer = journal_pages[1].transform.Find("Customers").gameObject;
        journal_customer_info = journal_pages[1].transform.Find("Customer_Info").gameObject;

        customer_percentage = journal_customer.transform.Find("unlock_amount").GetComponent<Text>();
        customer_slots = new List<GameObject>();
        customer_token = new List<GameObject>();
        customer_slot_txt = new List<Text>();

        for (int i = 0; i < SLOT; i++)
        {
            customer_slots.Add(journal_customer.transform.Find("customer_" + i).gameObject);
            customer_token.Add(customer_slots[i].transform.Find("new").gameObject);
            customer_slot_txt.Add(journal_customer.transform.Find("customer_" + i + "_txt").GetComponent<Text>());
        }

        customer_name = journal_customer_info.transform.Find("Customer_Name").GetComponent<Text>();
        customer_img = journal_customer_info.transform.Find("Background").Find("Customer_Img").GetComponent<Image>();
        customer_bought_amount = journal_customer_info.transform.Find("Background").Find("Bought_Amount").GetComponent<Text>();
        customer_product_img = journal_customer_info.transform.Find("Background").Find("Unlock_Product").gameObject;
        customer_unlock_product = customer_product_img.transform.Find("Product_Img").GetComponent<Image>();
        customer_description = journal_customer_info.transform.Find("Customer_Description").GetComponent<Text>();
    }

    // 저널 손님 페이지 초기화
    private void CustomerInitialize()
    {
        max_tier = DataManager.manager.customers.customers.Length;
        max_info = GetCustomerAmount();
        max_page = GetCustomerPage();
        now_info = 0;
        now_page = Customer_CheckNewTokenPage();
        PageButtonSet(true);
    }

    // 저널 손님 페이지 오픈
    private void Customer()
    {
        GameManager.manager.GetSoundManager().Journal();
        journal_customer_info.SetActive(false);
        journal_customer.SetActive(true);
        journal_info = false;

        TextUI(journal_page_txt, (now_page + 1) + " / " + max_page);

        if (GetCustomerUnlockPercentage() > 0)
            TextUI(customer_percentage, GetCustomerUnlockPercentage().ToString("F2") + " %");
        else
            TextUI(customer_percentage, "0 %");

        for (int i = 0; i < SLOT; i++)
        {
            int type = GetPageCustomerTier(now_page);
            if (i + (now_page * SLOT) < GetCustomerSlotAmount(type - 1) + DataManager.manager.customers.customers[type].Length)
            {
                int code = (i + (now_page * SLOT)) - GetCustomerSlotAmount(type - 1);
                SetCustomer_Slot(i, type, code, true);
            }
            else
            { SetCustomer_Slot(i, false); }
        }
        PageButtonSet();
    }

    // 저널 손님 디테일 뷰
    private void Customer_Info(int type, int code)
    {
        GameManager.manager.GetSoundManager().Journal();
        if (!DataManager.customers_save[type][code].looked)
        {
            DataManager.customers_save[type][code].looked = true;
            Journal_Reward_Setter(0);
            Customer_CheckNewToken();
        }

        journal_customer_info.SetActive(true);
        journal_customer.SetActive(false);
        journal_info = true;

        now_info = GetCustomerAmount(type - 1) + code;

        TextUI(journal_page_txt, (now_info + 1) + " / " + max_info);
        TextUI(customer_name, DataManager.manager.customers.customers[type][code].GetName());

        ImageUI(customer_img, GetCustomerImg(type, code));

        if ((type == 2 && code != 13 && code != 15) || (type == 0 && (code == 0 || code == 50)))
        { UIManager.manager.UIPivotSet(customer_img, journal_slot_pivot[1]); }
        else
        { UIManager.manager.UIPivotSet(customer_img, journal_slot_pivot[0]); }

        TextUI(customer_bought_amount, DataManager.customers_save[type][code].bought_amount);

        SetCustomerUnlockProductImg(type, code);

        TextUI(customer_description, DataManager.manager.customers.customers[type][code].GetDescription());
        PageButtonSet();
        PageBothButtonSet();
    }

    // 손님 타입 리턴
    private int GetCustomerType(int amount)
    {
        for (int i = 0; i < max_tier; i++)
        {
            if (amount < GetCustomerAmount(i))
                return i;
        }
        return -1;
    }

    // 손님 코드 리턴
    private int GetCustomerCode(int amount)
    {
        for (int i = 0; i < max_tier; i++)
        {
            if (amount < GetCustomerAmount(i))
                return amount - GetCustomerAmount(i - 1);
        }
        return -1;
    }

    // 저널 손님 전체 개수
    private int GetCustomerAmount()
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
            amount += DataManager.manager.customers.customers[i].Length;
        return amount;
    }

    // 저널 손님 티어별 개수
    private int GetCustomerAmount(int tier)
    {
        int amount = 0;
        for (int i = 0; i < tier + 1; i++)
            amount += DataManager.manager.customers.customers[i].Length;
        return amount;
    }

    // 저널 손님 티어별 할당 슬롯 개수
    private int GetCustomerSlotAmount(int tier)
    {
        int amount = 0;
        for (int i = 0; i < tier + 1; i++)
            amount += GetCustomerPage(i) * SLOT;
        return amount;
    }

    // 저널 물품 페이지 전체 개수
    private int GetCustomerPage()
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
            amount += GetCustomerPage(i);
        return amount;
    }

    // 저널 티어 최대 페이지
    private int GetCustomerPage(int tier)
    {
        int amount = 0;

        if (DataManager.manager.customers.customers[tier].Length % SLOT == 0)
        {
            if (DataManager.manager.customers.customers[tier].Length < SLOT)
                amount = DataManager.manager.customers.customers[tier].Length / SLOT - 1;
            else
                amount = DataManager.manager.customers.customers[tier].Length / SLOT;
        }
        else
            amount = DataManager.manager.customers.customers[tier].Length / SLOT + 1;
        return amount;
    }

    // 저널 페이지 가져오기
    private int GetCustomerPage(int tier, int code)
    {
        int amount = 0;
        int code_page = 0;
        for (int i = 0; i < tier; i++)
            amount += GetCustomerPage(i);

        if (code > SLOT)
            code_page = code / SLOT;
        amount += code_page;
        return amount;
    }

    // 저널 페이지로 티어 받아오기
    private int GetPageCustomerTier(int page)
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
        {
            amount += GetCustomerPage(i);
            if (page < amount) return i;
        }
        return -1;
    }

    // 저널 물품 해금 퍼센티지 계산
    private float GetCustomerUnlockPercentage()
    {
        int unlocked = 0;
        for (int i = 0; i < max_tier; i++)
        {
            for (int j = 0; j < DataManager.customers_save[i].Length; j++)
            {
                if (DataManager.customers_save[i][j].unlocked) unlocked += 1;
            }
        }
        float percent = (float)unlocked / (float)GetCustomerAmount();
        return percent * 100;
    }

    // 저널 물품 이름 문자열 받아오기
    private string GetCustomerName(int type, int code)
    {
        string customer_name = null;
        if (DataManager.customers_save[type][code].unlocked)
            customer_name = DataManager.manager.customers.customers[type][code].GetName();
        else
            customer_name = "???";
        return customer_name;
    }

    // 저널 물품 이미지 받아오기
    private Sprite GetCustomerImg(int type, int code)
    {
        if (DataManager.customers_save[type][code].unlocked)
        {
            if (type == 3 && code == 10)
            {
                return Data_Container.customer_container.GetThreatenedSprite(type, code);
            }
            return Data_Container.customer_container.GetStopSprites(type, code)[0];
        }
        else
        {
            return Data_Container.customer_container.GetSilhouette(type, code);
        }
    }

    // 판매 물품 설정
    private void SetCustomerUnlockProductImg(int type, int code)
    {
        int product_type = DataManager.manager.customers.customers[type][code].product_grade;
        int product_code = DataManager.manager.customers.customers[type][code].product_code;
        if (DataManager.manager.customers.customers[type][code].product_code > -1)
        {
            customer_product_img.SetActive(true);
            ImageUI(customer_unlock_product, GetProductImg(product_type, product_code));

            if (DataManager.products_save[product_type][product_code].unlocked)
                customer_unlock_product.color = Color.white;
            else
                customer_unlock_product.color = journal_sl_color;
        }
        else customer_product_img.SetActive(false);
    }

    // 저널 페이지 슬롯 설정
    private void SetCustomer_Slot(int index, bool active)
    {
        customer_slots[index].SetActive(active);
        customer_slot_txt[index].gameObject.SetActive(active);
    }
    private void SetCustomer_Slot(int index, int type, int code, bool active)
    {
        SetCustomer_Slot(index, active);
        SlotSizePivotSet(index, type, code);
        ImageUI(customer_slots[index], GetCustomerImg(type, code));
        TextUI(customer_slot_txt[index], GetCustomerName(type, code));

        if (DataManager.customers_save[type][code].unlocked && !DataManager.customers_save[type][code].looked)
            customer_token[index].SetActive(true);
        else
            customer_token[index].SetActive(false);

        customer_slots[index].GetComponent<EventTrigger>().triggers.Clear();

        if (DataManager.customers_save[type][code].unlocked)
        {
            customer_slots[index].GetComponent<Touch_Scale>().SetTouchable(true);

            EventTrigger.Entry pointer_click = new EventTrigger.Entry();
            pointer_click.eventID = EventTriggerType.PointerClick;
            pointer_click.callback.AddListener((data) => Customer_Info(type, code));
            customer_slots[index].GetComponent<EventTrigger>().triggers.Add(pointer_click);
        }
        else
            customer_slots[index].GetComponent<Touch_Scale>().SetTouchable(false);
    }

    // 슬롯 사이즈 & 피봇 설정
    private void SlotSizePivotSet(int index, int type, int code)
    {
        // Size
        if (type == 0 && (code == 29 || code == 33 || code == 41))
        { UIManager.manager.UISizeSet(customer_slots[index], journal_slot_size * 0.8f); }
        else
        { UIManager.manager.UISizeSet(customer_slots[index], journal_slot_size); }
        // Pivot
        if ((type == 2 && code != 11 && code != 13 && code != 15) || (type == 0 && (code == 0 || code == 30 || code == 35 || code == 50)))
        { UIManager.manager.UIPivotSet(customer_slots[index], journal_slot_pivot[1]); }
        else
        { UIManager.manager.UIPivotSet(customer_slots[index], journal_slot_pivot[0]); }

        customer_slots[index].GetComponent<Touch_Scale>().InitializeScale();
    }

    // 뉴 토큰 있는지 체크
    private void Customer_CheckNewToken()
    {
        int count = 0;
        for (int i = 0; i < DataManager.customers_save.Length; i++)
        {
            for (int j = 0; j < DataManager.customers_save[i].Length; j++)
            {
                if (DataManager.customers_save[i][j].unlocked && !DataManager.customers_save[i][j].looked)
                    count += 1;
            }
        }

        if (count > 0)
            NewTokenSet(1, true);
        else
            NewTokenSet(1, false);
    }

    // 저널 뉴 토큰 활성화 페이지 가져오기
    private int Customer_CheckNewTokenPage()
    {
        for (int i = 0; i < DataManager.customers_save.Length; i++)
        {
            for (int j = 0; j < DataManager.customers_save[i].Length; j++)
            {
                if (DataManager.customers_save[i][j].unlocked && !DataManager.customers_save[i][j].looked)
                {
                    return GetCustomerPage(i, j);
                }
            }
        }
        return 0;
    }

    // 저널 손님 언락
    private void SetCustomer_Unlock(int type, int code)
    {
        DataManager.customers_save[type][code].bought_amount += 1;
        if (!DataManager.customers_save[type][code].unlocked)
        {
            DataManager.customers_save[type][code].unlocked = true;
            NewTokenSet(1, true);

            int p_type = DataManager.manager.customers.customers[type][code].product_grade;
            int p_code = DataManager.manager.customers.customers[type][code].product_code;
            if (p_type != -1 && p_code != -1 && !DataManager.products_save[p_type][p_code].spawnable)
            {
                UIManager.manager.GetProduct_Open(Data_Container.product_container.GetSilhouette(p_type, p_code));
                DataManager.products_save[p_type][p_code].spawnable = true;
            }
        }
    }

    #endregion

    #region ending

    private GameObject[] journal_ending;        // 저널 엔딩 페이지
    private Button[][] ending_slot;             // 저널 엔딩 슬롯들
    private GameObject[][] ending_token;        // 저널 엔딩 뉴 토큰

    private GameObject ending_balloon;          // 저널 엔딩 말풍선
    private RectTransform ending_balloon_rect;  // 엔딩 말풍선 렉트트랜스폼
    private RectTransform ending_name_rect;     // 엔딩 이름 들어갈 텍스트 렉트트랜스폼
    private Text ending_name_txt;               // 엔딩 이름 들어갈 텍스트

    private Text greencard_name;                // 영주권 플레이어 이름 칸
    private Text greencard_date;                // 영주권 구매한 날짜 칸

    private int picked_ending;                  // 터치한 엔딩 인덱스 저장용

    // 저널 엔딩 페이지 초기화
    private void EndingPanelInitialize()
    {
        journal_ending = new GameObject[2];
        for (int i = 0; i < journal_ending.Length; i++)
            journal_ending[i] = journal_pages[2].transform.Find("Ending_" + i).gameObject;

        ending_slot = new Button[2][];
        ending_slot[0] = new Button[25];
        ending_slot[1] = new Button[11];
        ending_token = new GameObject[2][];
        ending_token[0] = new GameObject[25];
        ending_token[1] = new GameObject[11];
        for (int i = 0; i < ending_slot[0].Length; i++)
        {
            ending_slot[0][i] = journal_ending[0].transform.Find("End_" + i).GetComponent<Button>();
            ending_token[0][i] = ending_slot[0][i].transform.Find("new").gameObject;
        }
        for (int i = 0; i < ending_slot[1].Length; i++)
        {
            ending_slot[1][i] = journal_ending[1].transform.Find("End_" + (i + ending_slot[0].Length)).GetComponent<Button>();
            ending_token[1][i] = ending_slot[1][i].transform.Find("new").gameObject;
        }

        ending_balloon = journal_pages[2].transform.Find("description_balloon").gameObject;
        ending_balloon_rect = ending_balloon.GetComponent<RectTransform>();
        ending_name_rect = ending_balloon.transform.Find("Ending_Name").GetComponent<RectTransform>();
        ending_name_txt = ending_balloon.transform.Find("Ending_Name").GetComponent<Text>();

        greencard_name = ending_slot[0][0].transform.Find("name").GetComponent<Text>();
        greencard_date = ending_slot[0][0].transform.Find("date").GetComponent<Text>();
    }

    // 저널 엔딩 페이지 초기화
    private void EndingInitialize()
    {
        max_tier = DataManager.manager.endings.endings.Length;
        max_info = GetEndingAmount();
        max_page = 2;
        now_info = 0; now_page = 0;
        picked_ending = -1;
        PageButtonSet(false);
        PageBothButtonSet();
    }

    // 저널 엔딩 페이지 오픈
    private void Ending(int page)
    {
        GameManager.manager.GetSoundManager().Journal();
        for (int i = 0; i < max_page; i++) { journal_ending[i].SetActive(false); }
        journal_ending[page].SetActive(true);
        now_page = page;
        ending_balloon.SetActive(false);
        for (int i = 0; i < GetEndingAmount(page); i++)
        { SetEnding_Slot(i); }
    }

    // 저널 엔딩 타입 리턴
    private int GetEndingType(int amount)
    {
        for (int i = 0; i < max_tier; i++)
        {
            if (DataManager.manager.endings.endings[i].Length > amount)
            { return i; }
            else { amount -= DataManager.manager.endings.endings[i].Length; }
        }
        return -1;
    }

    // 저널 엔딩 코드 리턴
    private int GetEndingCode(int amount)
    {
        for (int i = 0; i < max_tier; i++)
        {
            for (int j = 0; j < DataManager.manager.endings.endings[i].Length; j++)
            {
                if (DataManager.manager.endings.endings[i][j].code == amount) { return j; }
            }
        }
        return -1;
    }
    // 저널 엔딩 전체 개수
    private int GetEndingAmount()
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
        { amount += DataManager.manager.endings.endings[i].Length; }
        return amount;
    }

    // 저널 엔딩 페이지 별 개수
    private int GetEndingAmount(int page)
    {
        if (page == 0)
        { return ending_slot[0].Length; }
        else if (page == 1)
        { return ending_slot[1].Length; }
        else
        { return 0; }
    }

    // 저널 엔딩 슬롯 설정
    private void SetEnding_Slot(int slot_index)
    {
        int type = GetEndingType((now_page * GetEndingAmount(now_page - 1)) + slot_index);
        int code = GetEndingCode((now_page * GetEndingAmount(now_page - 1)) + slot_index);
        bool active = DataManager.endings_save[type][code].unlocked;

        if (slot_index == 0)
        {
            greencard_name.gameObject.SetActive(active);
            greencard_date.gameObject.SetActive(active);
            if (active)
            {
                TextUI(greencard_name, DataManager.manager.player_save.player_info.player_name);
                TextUI(greencard_date, DataManager.manager.player_save.hidden_conditions.greencard_date);
            }
        }

        ending_token[now_page][slot_index].SetActive(false);
        ending_slot[now_page][slot_index].GetComponent<Touch_Scale>().SetTouchable(active);
        if (active)
        {
            if (!DataManager.endings_save[type][code].looked)
            { ending_token[now_page][slot_index].SetActive(true); }
            ending_slot[now_page][slot_index].image.sprite = Data_Container.ending_container.GetEndingIcon(type, code);
        }
        else
        { ending_slot[now_page][slot_index].image.sprite = Data_Container.ending_container.GetEndingIconSilhouette(type, code); }
    }

    // 엔딩 스티커 클릭 시 처리부
    public void EndingTouched(int index)
    {
        int type = GetEndingType(index);
        int code = GetEndingCode(index);
        if (DataManager.endings_save[type][code].unlocked && !DataManager.endings_save[type][code].looked)
        {
            DataManager.manager.SetEndingJournalData(type, code);
            Journal_Reward_Setter(1);
            Ending_CheckNewToken();
            Ending(now_page);
        }
        if (picked_ending == index && ending_balloon.activeInHierarchy)
        { ending_balloon.SetActive(false); }
        else
        {
            if (DataManager.endings_save[type][code].unlocked)
            {
                string description = "";
                try { description = DataManager.manager.endings.endings[type][code].GetDescription(); }
                catch { }
                if (description == "")
                { ending_name_txt.text = DataManager.manager.endings.endings[type][code].code.ToString("00") + ". '" + DataManager.manager.endings.endings[type][code].GetName(); }
                else
                {
                    description = description.Replace("_playername_", DataManager.manager.player_save.player_info.player_name);
                    ending_name_txt.text = DataManager.manager.endings.endings[type][code].code.ToString("00") + ". '" + DataManager.manager.endings.endings[type][code].GetName() + "'\n" + description;
                }
            }
            else
            { ending_name_txt.text = DataManager.manager.endings.endings[type][code].code.ToString("00") + ". " + "'???'\n" + DataManager.manager.endings.endings[type][code].GetBalloonDescription(); }
            ending_balloon_rect.anchoredPosition = DataManager.manager.endings.endings[type][code].balloon_pos;
            ending_balloon_rect.localScale = DataManager.manager.endings.endings[type][code].balloon_rot;
            ending_name_rect.localScale = DataManager.manager.endings.endings[type][code].balloon_rot;
            ending_balloon.SetActive(true);
        }
        picked_ending = index;
    }

    // 뉴 토큰 있는지 체크
    private void Ending_CheckNewToken()
    {
        int count = 0;
        for (int i = 0; i < DataManager.endings_save.Length; i++)
        {
            for (int j = 0; j < DataManager.endings_save[i].Length; j++)
            {
                if (DataManager.endings_save[i][j].unlocked && !DataManager.endings_save[i][j].looked)
                    count += 1;
            }
        }

        if (count > 0)
            NewTokenSet(2, true);
        else
            NewTokenSet(2, false);
    }

    // 저널 엔딩 언락
    private void SetEnding_Unlock(int type, int code)
    {
        if (!DataManager.endings_save[type][code].looked)
        {
            NewTokenSet(2, true);
        }
    }

    #endregion

    #region stage

    private GameObject[] journal_stage;         // 저널 스테이지 페이지
    private Button[][] stage_slot;              // 저널 스테이지 슬롯
    private GameObject[][] stage_token;         // 저널 물품 뉴 토큰
    private int[][] stage_code;                 // 스테이지 코드 저장
    private bool[][] stage_touchable;           // 스테이지 터치 가능한지

    private GameObject stage_balloon;           // 스테이지 조건 설명용 말풍선 오브젝트
    private RectTransform stage_balloon_rect;   // 스테이지 조건 설명용 말풍선 rect
    private Text stage_desc_text;               // 스테이지 조건 설명용 텍스트
    private RectTransform stage_desc_rect;      // 스테이지 조건 설명용 텍스트 rect
    private int picked_stage;                   // 선택된 스테이지

    private int now_stage_code;                 // 현재 스테이지 코드
    private int next_stage_code;                // 다음 스테이지 코드

    private GameObject stage_marker_now;        // 스테이지 맵 마커 (현재 맵 선택)
    private GameObject stage_marker_next;       // 스테이지 맵 마커 (다음 맵 선택)

    private bool stage_changeable;              // 스테이지 변경 가능한 일과인가

    private GameObject journal_totem;           // 토템 부모오브젝트
    private GameObject[] journal_totems;        // 토템들
    private RectTransform[] journal_totems_rect;// 토템들 rect

    private GameObject totem_balloon;           // 토템 설명용 말풍선 오브젝트
    private RectTransform totem_balloon_rect;   // 토템 설명용 말풍선 rect
    private Text totem_desc_text;               // 토템 설명용 텍스트
    private RectTransform totem_desc_rect;      // 토템 텍스트 rect
    private int picked_totem;                   // 선택된 토템

    private Vector2 totem_balloon_pos;          // 토템 말풍선 포지션 설정용
    private Vector3 rect_normal;                // X (1)
    private Vector3 rect_flip;                  // X (-1)

    // 저널 스테이지 페이지 초기화
    private void StagePanelInitialize()
    {
        journal_stage = new GameObject[2];
        for (int i = 0; i < journal_stage.Length; i++)
            journal_stage[i] = journal_pages[3].transform.Find("Map_" + i).gameObject;

        stage_slot = new Button[2][];
        stage_token = new GameObject[2][];
        for (int i = 0; i < journal_stage.Length; i++)
        {
            stage_slot[i] = new Button[9];
            stage_token[i] = new GameObject[9];
            for (int j = 0; j < stage_slot[i].Length; j++)
            {
                if (j < DataManager.manager.stages.stages[i].Length)
                {
                    stage_slot[i][j] = journal_stage[i].transform.Find("stage_" + j).GetComponent<Button>();
                    stage_token[i][j] = stage_slot[i][j].transform.Find("New").gameObject;
                }
                else
                {
                    stage_slot[i][j] = journal_stage[i].transform.Find("hidden_" + (j - DataManager.manager.stages.stages[i].Length)).GetComponent<Button>();
                    stage_token[i][j] = stage_slot[i][j].transform.Find("New").gameObject;
                }
            }
        }
        stage_code = new int[2][];
        for (int i = 0; i < stage_code.Length; i++)
        {
            stage_code[i] = new int[9];
            for (int j = 0; j < stage_code[i].Length; j++)
            {
                if (j < DataManager.manager.stages.stages[i].Length)
                { stage_code[i][j] = (i * 6) + j; }
                else
                {
                    if (i == 0) { stage_code[i][j] = 12 + (j - 6); }
                    else { stage_code[i][j] = 14 + (j - 6); }
                }
            }
        }
        stage_code[0][8] = 17;

        stage_touchable = new bool[3][];
        for (int i = 0; i < stage_touchable.Length; i++)
        { stage_touchable[i] = new bool[6]; }

        journal_totem = journal_pages[3].transform.Find("totem").gameObject;
        journal_totems = new GameObject[6];
        for (int i = 0; i < journal_totems.Length; i++)
        { journal_totems[i] = journal_totem.transform.Find(i.ToString()).gameObject; }
        journal_totems_rect = new RectTransform[6];
        for (int i = 0; i < journal_totems_rect.Length; i++)
        { journal_totems_rect[i] = journal_totems[i].GetComponent<RectTransform>(); }

        stage_balloon = journal_pages[3].transform.Find("stage_balloon").gameObject;
        stage_balloon_rect = stage_balloon.GetComponent<RectTransform>();
        stage_desc_text = stage_balloon.transform.Find("description").GetComponent<Text>();
        stage_desc_rect = stage_desc_text.transform.GetComponent<RectTransform>();

        totem_balloon = journal_totem.transform.Find("totem_balloon").gameObject;
        totem_balloon_rect = totem_balloon.GetComponent<RectTransform>();
        totem_desc_text = totem_balloon.transform.Find("description").GetComponent<Text>();
        totem_desc_rect = totem_desc_text.transform.GetComponent<RectTransform>();

        stage_marker_now = journal_pages[3].transform.Find("now_mapmarker").gameObject;
        stage_marker_next = journal_pages[3].transform.Find("next_mapmarker").gameObject;

        stage_changeable = true;

        picked_stage = -1;
        picked_totem = -1;

        totem_balloon_pos = Vector2.zero;
        rect_normal = new Vector3(1, 1, 1);
        rect_flip = new Vector3(-1, 1, 1);
    }

    // 저널 스테이지 페이지 초기화
    private void StageInitialize()
    {
        max_tier = DataManager.manager.stages.stages.Length;
        max_info = GetStageAmount();
        max_page = 2;

        now_page = Stage_CheckNewTokenPage();
        now_info = 0;

        for (int i = 0; i < journal_totems.Length; i++)
        { journal_totems[i].SetActive(false); }
        for (int i = 0; i < stage_touchable.Length; i++)
        {
            for (int j = 0; j < stage_touchable[i].Length; j++)
            { stage_touchable[i][j] = DataManager.stages_save[i][j]; }
        }
        now_stage_code = DataManager.manager.stage_save.now_stage;
        next_stage_code = DataManager.manager.stage_save.next_stage;

        totem_balloon.SetActive(false);
        picked_totem = 0;

        PageButtonSet(false);
        PageBothButtonSet();
        stage_changeable = check_stage_changeable();
    }

    // 저널 스테이지 페이지 오픈
    private void Stage()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        for (int i = 0; i < max_page; i++) { journal_stage[i].SetActive(false); }
        journal_stage[now_page].SetActive(true);

        for (int i = 0; i < stage_slot[now_page].Length; i++)
        { SetStage_Slot(i); }
        for (int i = 0; i < journal_totems.Length; i++)
        { journal_totems[i].SetActive(DataManager.manager.ending_save.totem[i]); }

        stage_balloon.SetActive(false);
        totem_balloon.SetActive(false);

        SetStage_MapMarker();
    }

    // 저널 스테이지 페이지 오픈
    private void Stage(int page)
    {
        GameManager.manager.GetSoundManager().Journal();
        for (int i = 0; i < max_page; i++) { journal_stage[i].SetActive(false); }
        journal_stage[page].SetActive(true);
        now_page = page;

        for (int i = 0; i < stage_slot[page].Length; i++)
        { SetStage_Slot(i); }
        for (int i = 0; i < journal_totems.Length; i++)
        { journal_totems[i].SetActive(DataManager.manager.ending_save.totem[i]); }

        stage_balloon.SetActive(false);
        totem_balloon.SetActive(false);

        SetStage_MapMarker();
    }

    // 저널 스테이지 타입 리턴
    private int GetStageType(int amount)
    {
        return amount / 6;
    }

    // 저널 스테이지 코드 리턴
    private int GetStageCode(int amount)
    {
        return amount % 6;
    }

    // 저널 스테이지 전체 개수
    private int GetStageAmount()
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
        { amount += DataManager.manager.stages.stages[i].Length; }
        return amount;
    }

    // 저널 스테이지 페이지 받아오기
    private int GetStagePage(int code)
    {
        for (int i = 0; i < stage_code.Length; i++)
        {
            for (int j = 0; j < stage_code[i].Length; j++)
            {
                if (stage_code[i][j] == code)
                { return i; }
            }
        }
        return -1;
    }

    // 저널 스테이지 버튼인덱스 받아오기
    private int GetStageIndex(int code)
    {
        for (int i = 0; i < stage_code.Length; i++)
        {
            for (int j = 0; j < stage_code[i].Length; j++)
            {
                if (stage_code[i][j] == code)
                { return j; }
            }
        }
        return -1;
    }

    // 저널 스테이지 슬롯 설정
    private void SetStage_Slot(int slot_index)
    {
        int type = GetStageType(stage_code[now_page][slot_index]);
        int code = GetStageCode(stage_code[now_page][slot_index]);

        stage_slot[now_page][slot_index].GetComponent<Touch_Scale>().SetTouchable(stage_touchable[type][code]);
        if (stage_touchable[type][code])
        { stage_slot[now_page][slot_index].image.sprite = Data_Container.stage_container.GetStageIcon(type, code); }
        else
        { stage_slot[now_page][slot_index].image.sprite = Data_Container.stage_container.GetStageIconSilhouette(type, code); }

        if (DataManager.stages_save[type][code] && !DataManager.manager.stage_save.touched[stage_code[now_page][slot_index]])
        { stage_token[now_page][slot_index].SetActive(true); }
        else
        { stage_token[now_page][slot_index].SetActive(false); }
    }

    // 저널 스테이지 맵 마커 설정
    private void SetStage_MapMarker()
    {
        if (next_stage_code < 0 || next_stage_code == now_stage_code)
        { stage_marker_next.SetActive(false); }
        else
        {
            if (GetStagePage(next_stage_code) != now_page)
            {
                stage_marker_next.SetActive(false);
            }
            else
            {
                stage_marker_next.SetActive(true);
                UIManager.manager.UIAnchoredPosSet(stage_marker_next, stage_slot[GetStagePage(next_stage_code)][GetStageIndex(next_stage_code)].gameObject);
            }
        }

        if (GetStagePage(now_stage_code) != now_page)
        {
            stage_marker_now.SetActive(false);
        }
        else
        {
            stage_marker_now.SetActive(true);
            UIManager.manager.UIAnchoredPosSet(stage_marker_now, stage_slot[GetStagePage(now_stage_code)][GetStageIndex(now_stage_code)].gameObject);
        }
    }

    // 맵 버튼 터치 시
    public void StageTouched(int index)
    {
        int type = GetStageType(index);
        int code = GetStageCode(index);
        if (stage_touchable[type][code])
        {
            stage_balloon.SetActive(false);
            StageChange(index);
        }
        else
        {
            if (picked_stage == index && stage_balloon.activeInHierarchy)
            { stage_balloon.SetActive(false); }
            else
            {
                stage_balloon_rect.anchoredPosition = DataManager.manager.stages.stages[type][code].balloon_pos;
                if (DataManager.manager.stages.stages[type][code].balloon_flip)
                {
                    stage_balloon_rect.localScale = rect_flip;
                    stage_desc_rect.localScale = rect_flip;
                }
                else
                {
                    stage_balloon_rect.localScale = rect_normal;
                    stage_desc_rect.localScale = rect_normal;
                }
                stage_desc_text.text = DataManager.manager.stages.stages[type][code].GetName() + ".\n" + DataManager.manager.stages.stages[type][code].GetDescription();
                stage_balloon.SetActive(true);
            }
            picked_stage = index;
        }
    }

    // 맵 변경
    public void StageChange(int code)
    {
        if (!DataManager.manager.stage_save.touched[code])
        {
            DataManager.manager.stage_save.touched[code] = true;
            Journal_Reward_Setter(0);
            Stage_CheckNewToken();
        }

        if (stage_changeable)
        {
            stage_change(code);
            next_stage_code = code;
        }
        else
        {
            MsgBox_Controller.manager.MessageboxPanel_active("map", DataManager.manager.ui.messages.GetCannotMapChange(), 0);
            MsgBox_Controller.manager.SetMessageboxButtonText("map", 0, DataManager.manager.ui.GetOK());
            MsgBox_Controller.manager.SetMessageboxButton("map", 0);
        }
        Stage();
    }

    // 토템 터치 시
    public void Totem_Touched(int code)
    {
        if (picked_totem == code && totem_balloon.activeInHierarchy)
        {
            totem_balloon.SetActive(false);
        }
        else
        {
            if (journal_totems_rect[code].anchoredPosition.x - 180 > 0)
            {
                totem_balloon_rect.localScale = rect_flip;
                totem_desc_rect.localScale = rect_normal;

                totem_balloon_pos.x = journal_totems_rect[code].anchoredPosition.x - 180;
                totem_balloon_pos.y = totem_balloon_rect.anchoredPosition.y;
                totem_balloon_rect.anchoredPosition = totem_balloon_pos;
                totem_desc_text.text = DataManager.manager.totems.totem[code].GetName() + ". " + DataManager.manager.totems.totem[code].GetDescription();
            }
            else
            {
                totem_balloon_rect.localScale = rect_normal;
                totem_desc_rect.localScale = rect_flip;

                totem_balloon_pos.x = journal_totems_rect[code].anchoredPosition.x + 180;
                totem_balloon_pos.y = totem_balloon_rect.anchoredPosition.y;
                totem_balloon_rect.anchoredPosition = totem_balloon_pos;
                totem_desc_text.text = DataManager.manager.totems.totem[code].GetName() + ". " + DataManager.manager.totems.totem[code].GetDescription();
            }
            totem_balloon.SetActive(true);
        }
        picked_totem = code;
    }

    // 스테이지 페이지 받아오기
    private int GetStagePage(int type, int code)
    {
        if (type < 2) { return type; }
        else
        {
            return code < 2 || code == 5 ? 0 : 1;
        }
    }

    // 뉴 토큰 있는지 체크
    private void Stage_CheckNewToken()
    {
        int count = 0;
        for (int i = 0; i < DataManager.stages_save.Length; i++)
        {
            for (int j = 0; j < DataManager.stages_save[i].Length; j++)
            {
                if (DataManager.stages_save[i][j] && !DataManager.manager.stage_save.touched[(i * 6) + j])
                    count += 1;
            }
        }
        if (count > 0)
        {
            map_new.SetActive(true);
            NewTokenSet(3, true);
        }
        else
        {
            map_new.SetActive(false);
            NewTokenSet(3, false);
        }
    }

    // 저널 뉴 토큰 활성화 페이지 가져오기
    private int Stage_CheckNewTokenPage()
    {
        for (int i = 0; i < DataManager.stages_save.Length; i++)
        {
            for (int j = 0; j < DataManager.stages_save[i].Length; j++)
            {
                if (DataManager.stages_save[i][j] && !DataManager.manager.stage_save.touched[(i * 6) + j])
                {
                    return GetStagePage(i, j);
                }
            }
        }
        return 0;
    }

    // 저널 맵 언락
    private void SetStage_Unlock(int stage_code, int active)
    {
        if (active == 0)
        {
            if (!DataManager.manager.stage_save.touched[stage_code])
            {
                DataManager.manager.stage_save.touched[stage_code] = true;
            }
        }

        int count = 0;
        for (int i = 0; i < DataManager.manager.stages.stages.Length; i++)
        {
            for (int j = 0; j < DataManager.manager.stages.stages[i].Length; j++)
            {
                if (DataManager.stages_save[i][j] && !DataManager.manager.stage_save.touched[(i * 6) + j])
                { count += 1; }
            }
        }

        for (int i = 0; i < 3; i++)
        { DataManager.products_save[DataManager.manager.products.products.Length][(stage_code * 3) + i].spawnable = true; }

        if (count <= 0)
        {
            map_new.SetActive(false);
            NewTokenSet(3, false);
        }
        else
        {
            map_new.SetActive(true);
            NewTokenSet(3, true);
        }
    }

    #endregion

    #region gameEvent

    private GameObject journal_event;           // 저널 이벤트 페이지
    private GameObject journal_event_info;      // 저널 이벤트 상세페이지

    private Text event_percentage;              // 저널 이벤트 언락 퍼센테이지

    private List<GameObject> event_slots;       // 저널 이벤트 슬롯들
    private List<GameObject> event_token;       // 저널 이벤트 뉴 토큰
    private List<Image> event_slot_img;         // 저널 이벤트 슬롯 이미지
    private List<Text> event_slot_txt;          // 저널 이벤트 슬롯 텍스트

    private Text event_name;                    // 저널 이벤트 이름
    private Image event_img;                    // 저널 이벤트 이미지
    private Text event_description;             // 저널 이벤트 설명

    // 저널 이벤트 페이지 초기화
    private void EventPanelInitialize()
    {
        journal_event = journal_pages[4].transform.Find("Events").gameObject;
        journal_event_info = journal_pages[4].transform.Find("Event_Info").gameObject;

        event_percentage = journal_event.transform.Find("unlock_amount").GetComponent<Text>();
        event_slots = new List<GameObject>();
        event_token = new List<GameObject>();
        event_slot_img = new List<Image>();
        event_slot_txt = new List<Text>();

        for (int i = 0; i < 2; i++)
        {
            event_slots.Add(journal_event.transform.Find("slot_" + i).gameObject);
            event_token.Add(event_slots[i].transform.Find("New").gameObject);
            event_slot_img.Add(event_slots[i].transform.Find("Event_Img").GetComponent<Image>());
            event_slot_txt.Add(event_slots[i].transform.Find("Event_Name").GetComponent<Text>());
        }
        event_name = journal_event_info.transform.Find("Event_Name").GetComponent<Text>();
        event_img = journal_event_info.transform.Find("Event_Img").GetComponent<Image>();
        event_description = journal_event_info.transform.Find("Event_Description").GetComponent<Text>();
    }

    // 저널 이벤트 페이지 초기화
    private void EventInitialize()
    {
        max_tier = DataManager.manager.events.events.Length;
        max_info = GetEventAmount();
        max_page = GetEventPage();
        now_info = 0;
        now_page = Event_CheckNewTokenPage();
        PageButtonSet(true);
    }

    // 저널 이벤트 페이지 오픈
    private void GameEvent()
    {
        GameManager.manager.GetSoundManager().Journal();
        journal_event.SetActive(true);
        journal_event_info.SetActive(false);
        journal_info = false;

        TextUI(journal_page_txt, (now_page + 1) + " / " + max_page);

        if (GetEventUnlockPercentage() > 0)
            TextUI(event_percentage, GetEventUnlockPercentage().ToString("F2") + " %");
        else TextUI(event_percentage, "0 %");

        for (int i = 0; i < 2; i++)
        {
            int type = GetPageEventTier(now_page);
            if (i + (now_page * 2) < GetEventSlotAmount(type - 1) + DataManager.manager.events.events[type].Length)
            {
                int code = (i + (now_page * 2)) - GetEventSlotAmount(type - 1);
                SetEvent_Slot(i, type, code, true);
            }
            else
            { SetEvent_Slot(i, false); }
        }
        PageButtonSet();
    }

    // 저널 이벤트 디테일 뷰
    private void GameEvent_Info(int type, int code)
    {
        GameManager.manager.GetSoundManager().Journal();
        if (!DataManager.events_save[type][code].looked)
        {
            DataManager.events_save[type][code].looked = true;
            Journal_Reward_Setter(0);
            Event_CheckNewToken();
        }

        journal_event.SetActive(false);
        journal_event_info.SetActive(true);
        journal_info = true;

        now_info = GetEventAmount(type - 1) + code;

        TextUI(journal_page_txt, (now_info + 1) + " / " + max_info);

        TextUI(event_name, DataManager.manager.events.events[type][code].GetName());
        ImageUI(event_img, GetEventImg(type, code));

        // 추후 수정
        string description_txt = DataManager.manager.events.events[type][code].GetDescription();
        description_txt = description_txt.Replace("_bill_", get_data_str("_bill_"));
        TextUI(event_description, description_txt);

        PageButtonSet();
        PageBothButtonSet();
    }

    // 이벤트 타입 리턴
    private int GetEventType(int amount)
    {
        for (int i = 0; i < max_tier; i++)
        {
            if (amount < GetEventAmount(i))
            { return i; }
        }
        return -1;
    }

    // 이벤트 코드 리턴
    private int GetEventCode(int amount)
    {
        for (int i = 0; i < max_tier; i++)
        {
            if (amount < GetEventAmount(i))
            { return amount - GetEventAmount(i - 1); }
        }
        return -1;
    }

    // 저널 이벤트 전체 개수
    private int GetEventAmount()
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
        { amount += DataManager.manager.events.events[i].Length; }
        return amount;
    }

    // 저널 이벤트 티어별 개수
    private int GetEventAmount(int tier)
    {
        int amount = 0;
        for (int i = 0; i < tier + 1; i++)
        { amount += DataManager.manager.events.events[i].Length; }
        return amount;
    }

    // 저널 이벤트 티어별 할당 슬롯 개수
    private int GetEventSlotAmount(int tier)
    {
        int amount = 0;
        for (int i = 0; i < tier + 1; i++)
        { amount += GetEventPage(i) * 2; }
        return amount;
    }

    // 저널 이벤트 페이지 전체 개수
    private int GetEventPage()
    {
        int amount = 0;
        for (int i = 0; i < max_tier; i++)
        { amount += GetEventPage(i); }
        return amount;
    }

    // 저널 이벤트 최대 페이지
    private int GetEventPage(int tier)
    {
        int amount = 0;

        if (DataManager.manager.events.events[tier].Length % 2 == 0)
        {
            if (DataManager.manager.events.events[tier].Length < 2)
            { amount = DataManager.manager.events.events[tier].Length / 2 - 1; }
            else
            { amount = DataManager.manager.events.events[tier].Length / 2; }
        }
        else
        { amount = DataManager.manager.events.events[tier].Length / 2 + 1; }
        return amount;
    }

    // 저널 페이지 가져오기
    private int GetEventPage(int tier, int code)
    {
        int amount = 0;
        int code_page = 0;
        for (int i = 0; i < tier; i++)
        { amount += GetEventPage(i); }

        if (code > 2)
        { code_page = code / 2; }
        amount += code_page;
        return amount;
    }

    // 저널 페이지로 티어 받아오기
    private int GetPageEventTier(int page)
    {
        int amount = 0;

        for (int i = 0; i < max_tier; i++)
        {
            amount += GetEventPage(i);
            if (page < amount) return i;
        }
        return -1;
    }

    // 저널 이벤트 해금 퍼센티지 계산
    private float GetEventUnlockPercentage()
    {
        int unlocked = 0;
        for (int i = 0; i < max_tier; i++)
        {
            for (int j = 0; j < DataManager.events_save[i].Length; j++)
            {
                if (DataManager.events_save[i][j].appearance > 0) unlocked += 1;
            }
        }
        float percent = (float)unlocked / (float)GetEventAmount();
        return percent * 100;
    }

    // 저널 물품 이름 문자열 받아오기
    private string GetEventName(int type, int code)
    {
        string event_name = null;

        if (DataManager.events_save[type][code].appearance > 0)
        {
            event_name = DataManager.manager.events.events[type][code].GetName();
        }
        else { event_name = "???"; }

        return event_name;
    }

    // 저널 물품 이미지 받아오기
    private Sprite GetEventImg(int type, int code)
    {
        return Data_Container.event_container.GetPickedSprite(type, code);
    }

    // 저널 페이지 슬롯 설정
    private void SetEvent_Slot(int index, bool active)
    {
        event_slots[index].SetActive(active);
    }
    private void SetEvent_Slot(int index, int type, int code, bool active)
    {
        SetEvent_Slot(index, active);
        ImageUI(event_slot_img[index], GetEventImg(type, code));
        TextUI(event_slot_txt[index], GetEventName(type, code));

        if (DataManager.events_save[type][code].appearance > 0 && !DataManager.events_save[type][code].looked)
        { event_token[index].SetActive(true); }
        else
        { event_token[index].SetActive(false); }

        event_slots[index].GetComponent<EventTrigger>().triggers.Clear();

        if (DataManager.events_save[type][code].appearance > 0)
        {
            event_slot_img[index].gameObject.SetActive(true);

            event_slots[index].GetComponent<Touch_Scale>().SetTouchable(true);

            EventTrigger.Entry pointer_click = new EventTrigger.Entry();
            pointer_click.eventID = EventTriggerType.PointerClick;
            pointer_click.callback.AddListener((data) => GameEvent_Info(type, code));
            event_slots[index].GetComponent<EventTrigger>().triggers.Add(pointer_click);
        }
        else
        {
            event_slot_img[index].gameObject.SetActive(false);
            event_slots[index].GetComponent<Touch_Scale>().SetTouchable(false);
        }
    }

    // 뉴 토큰 있는지 체크
    private void Event_CheckNewToken()
    {
        int count = 0;
        for (int i = 0; i < DataManager.events_save.Length; i++)
        {
            for (int j = 0; j < DataManager.events_save[i].Length; j++)
            {
                if (DataManager.events_save[i][j].appearance > 0 && !DataManager.events_save[i][j].looked)
                    count += 1;
            }
        }
        if (count > 0)
            NewTokenSet(4, true);
        else
            NewTokenSet(4, false);
    }

    // 저널 뉴 토큰 활성화 페이지 가져오기
    private int Event_CheckNewTokenPage()
    {
        for (int i = 0; i < DataManager.events_save.Length; i++)
        {
            for (int j = 0; j < DataManager.events_save[i].Length; j++)
            {
                if (DataManager.events_save[i][j].appearance > 0 && !DataManager.events_save[i][j].looked)
                {
                    return GetEventPage(i, j);
                }
            }
        }
        return 0;
    }

    // 저널 이벤트 등장 횟수 설정
    private void SetEvent_Unlock(int type, int code)
    {
        if (DataManager.events_save[type][code].appearance < 5000)
        {
            DataManager.events_save[type][code].appearance += 1;
            NewTokenSet(4, true);
        }
    }

    #endregion

    // 카테고리 버튼 활성화 비활성화 설정
    private void CategoryBtnSet(int index, bool active)
    {
        journal_btns[index].interactable = active;
        if (active)
            journal_btns[index].GetComponent<RectTransform>().anchoredPosition = new Vector2(ACTIVE_POS, journal_btns[index].GetComponent<RectTransform>().anchoredPosition.y);
        else
            journal_btns[index].GetComponent<RectTransform>().anchoredPosition = new Vector2(INACTIVE_POS, journal_btns[index].GetComponent<RectTransform>().anchoredPosition.y);
    }

    // 저널 카테고리별 페이지 호출
    private void Category_Open()
    {
        switch (journal_category)
        {
            case 0: Product(); break;
            case 1: Customer(); break;
            case 2: Ending(now_page == 0 ? 1 : 0); break;
            case 3: Stage(now_page == 0 ? 1 : 0); break;
            case 4: GameEvent(); break;
        }
    }

    // 저널 카테고리별 상세페이지 호출
    private void Category_Info_Open(int info_amount)
    {
        switch (journal_category)
        {
            case 0: Product_Info(GetProductType(info_amount), GetProductCode(info_amount)); break;
            case 1: Customer_Info(GetCustomerType(info_amount), GetCustomerCode(info_amount)); break;
            case 4: GameEvent_Info(GetEventType(info_amount), GetEventCode(info_amount)); break;
        }
    }

    // 이전 데이터 받아오기
    private int GetPrevData()
    {
        int prev = -1;
        switch (journal_category)
        {
            case 0:
                for (int data = now_info - 1; data > -1; data--)
                {
                    if (DataManager.products_save[GetProductType(data)][GetProductCode(data)].unlocked)
                    {
                        prev = data; break;
                    }
                }
                break;
            case 1:
                for (int data = now_info - 1; data > -1; data--)
                {
                    if (DataManager.customers_save[GetCustomerType(data)][GetCustomerCode(data)].unlocked)
                    {
                        prev = data; break;
                    }
                }
                break;
            case 4:
                for (int data = now_info - 1; data > -1; data--)
                {
                    if (DataManager.events_save[GetEventType(data)][GetEventCode(data)].appearance > 0)
                    {
                        prev = data; break;
                    }
                }
                break;
        }
        return prev;
    }

    // 다음 데이터 받아오기
    private int GetNextData()
    {
        int next = -1;
        switch (journal_category)
        {
            case 0:
                for (int data = now_info + 1; data < max_info; data++)
                {
                    if (DataManager.products_save[GetProductType(data)][GetProductCode(data)].unlocked)
                    {
                        next = data; break;
                    }
                }
                break;
            case 1:
                for (int data = now_info + 1; data < max_info; data++)
                {
                    if (DataManager.customers_save[GetCustomerType(data)][GetCustomerCode(data)].unlocked)
                    {
                        next = data; break;
                    }
                }
                break;
            case 4:
                for (int data = now_info + 1; data < max_info; data++)
                {
                    if (DataManager.events_save[GetEventType(data)][GetEventCode(data)].appearance > 0)
                    {
                        next = data; break;
                    }
                }
                break;
        }
        return next;
    }

    // 페이지(데이터) 변경 버튼 [ 이전 ]
    public void PrevPage()
    {
        if (!journal_info)
        {
            if (now_page > 0)
            {
                now_page -= 1;
                Category_Open();
            }
        }
        else
        {
            if (now_info > 0)
            {
                now_info = GetPrevData();
                Category_Info_Open(now_info);
            }
        }
    }

    // 페이지(데이터) 변경 버튼 [ 이후 ]
    public void NextPage()
    {
        if (!journal_info)
        {
            if (now_page < max_page - 1)
            {
                now_page += 1;
                Category_Open();
            }
        }
        else
        {
            if (now_info < max_info - 1)
            {
                now_info = GetNextData();
                Category_Info_Open(now_info);
            }
        }
    }

    // 페이지 변경 버튼 [ 둘 다 ]
    public void PageChange()
    {
        if (journal_category == 2 || journal_category == 3)
        {
            if (now_page == 0)
            { ImageUI(journal_pages_btn, journal_pages_btn_sprite[1]); }
            else
            { ImageUI(journal_pages_btn, journal_pages_btn_sprite[0]); }
        }
        else
        { journal_pages_btn.SetActive(false); }
        Category_Open();
    }

    // 페이지 버튼 액티브
    private void PageButtonSet()
    {
        if (!journal_info)
        {
            if (now_page <= 0)
            {
                journal_page_btn[0].SetActive(false);
                journal_page_btn[1].SetActive(true);
            }
            else if (now_page >= max_page - 1)
            {
                journal_page_btn[0].SetActive(true);
                journal_page_btn[1].SetActive(false);
            }
            else
            {
                journal_page_btn[0].SetActive(true);
                journal_page_btn[1].SetActive(true);
            }
        }
        else
        {
            if (now_info <= 0 || GetPrevData() < 0)
            { journal_page_btn[0].SetActive(false); }
            else
            { journal_page_btn[0].SetActive(true); }

            if (now_info >= max_info - 1 || GetNextData() < 0)
            { journal_page_btn[1].SetActive(false); }
            else
            { journal_page_btn[1].SetActive(true); }
        }
    }

    // 페이지 버튼 (뒤로가기) 설정
    private void PageBothButtonSet()
    {
        journal_pages_btn.SetActive(true);
        switch (journal_category)
        {
            case 2:
            case 3:
                ImageUI(journal_pages_btn, journal_pages_btn_sprite[0]);
                break;
            case 0:
            case 1:
            case 4:
                ImageUI(journal_pages_btn, journal_pages_btn_sprite[1]);
                break;
        }
    }

    // 페이지 버튼 액티브 설정
    private void PageButtonSet(bool active)
    {
        journal_page_btn[0].SetActive(active);
        journal_page_btn[1].SetActive(active);
        journal_page_txt.gameObject.SetActive(active);
        journal_pages_btn.SetActive(!active);
    }

    // 저널 뉴 토큰 이니셜라이즈
    private void NewTokenInitialize()
    {
        Product_CheckNewToken();
        Customer_CheckNewToken();
        Ending_CheckNewToken();
        Stage_CheckNewToken();
        Event_CheckNewToken();
    }

    // 저널 뉴 토큰 설정
    private void NewTokenSet(int what_new, bool active)
    {
        journal_new_token_active[what_new] = active;
        journal_new_token[what_new].SetActive(active);

        int count = 0;
        for (int i = 0; i < journal_new_token_active.Length; i++)
        {
            if (journal_new_token_active[i])
            { count += 1; }
        }

        if (count > 0)
        { UIManager.manager.NewTokenSet(0, true); }
        else
        { UIManager.manager.NewTokenSet(0, false); }
    }

    // 뉴 토큰 설정
    private void NewTokenSetter(string what_new, int type, int code)
    {
        switch (what_new)
        {
            case "product": SetProduct_Unlock(type, code); break;
            case "customer": SetCustomer_Unlock(type, code); break;
            case "ending": SetEnding_Unlock(type, code); break;
            case "stage": SetStage_Unlock(type, code); break;
            case "event": SetEvent_Unlock(type, code); break;
        }
    }

    #region Delegates

    public delegate void Journal_StageChange(int data);
    public static event Journal_StageChange stage_change;

    public delegate bool Journal_CheckChangeable();
    public static event Journal_CheckChangeable check_stage_changeable;

    public delegate string Journal_DataString(string change_data);
    public static event Journal_DataString get_data_str;

    public delegate void Journal_Reward(int soulseed, int mote, int sp);
    public static event Journal_Reward journal_reward;

    private void OnEnable() => DelegateSet();
    private void OnDisable() => DelegateDel();

    private void DelegateSet()
    {
        GameManager.game_init += InitializeJournalPanel;
        Data_Controller.journal_unlock += NewTokenSetter;
        UI_ItemShop.special_bought += NewTokenSetter;
    }

    private void DelegateDel()
    {
        GameManager.game_init -= InitializeJournalPanel;
        Data_Controller.journal_unlock -= NewTokenSetter;
        UI_ItemShop.special_bought -= NewTokenSetter;
    }

    #endregion
}