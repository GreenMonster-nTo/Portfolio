using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Inventory : UI_Panel
{
    private void OnEnable() { GameManager.game_init += InitializeInventoryPanel; }
    private void OnDisable() { GameManager.game_init -= InitializeInventoryPanel; }

    public GameObject inventory;                        // 인벤토리 오브젝트

    private const int ITEM_SLOT = 8;                    // 아이템 슬롯 개수

    public GameObject[] inventory_pages;                // 인벤토리 페이지들

    public Button[] inventory_btns;                     // 인벤토리 버튼들
    public Image[] inventory_btns_sprites;              // 인벤토리 버튼 내 스프라이트들
    private GameObject[] inventory_new_token;           // 인벤토리 카테고리 뉴 토큰

    public Sprite[] inventory_category_btn_sprites;     // 인벤토리 카테고리 버튼 스프라이트
    private Sprite[][] inventory_image_sprites;         // 인벤토리 스프라이트 전체 저장
    public Sprite[] inventory_inventory_sprites;        // 인벤토리 스프라이트
    public Sprite[] inventory_stall_sprites;            // 노점 스프라이트
    public Sprite[] inventory_costume_sprites;          // 코스튬 스프라이트

    public Sprite[] inventory_btn_sprites;              // 인벤토리 버튼 스프라이트

    public Sprite[] inventory_price_coin;               // 인벤토리 코인 스프라이트 0 : 코인 / 1 : 영씨
    public Sprite[] inventory_costume_frame;            // 인벤토리 코스튬 프레임
    public Sprite inventory_interior_frame;             // 인테리어 프레임

    private int inventory_panel;                        // 현재 인벤토리 카테고리
    private bool inventory_info;                        // 인포메이션 켰는지

    private int now_info, max_info;                     // 데이터
    private bool name_or_description;                   // 인벤토리 이름 출력 or 설명 출력

    // 인벤토리 판넬 이니셜라이즈
    private void InitializeInventoryPanel()
    {
        UI_Initialize("inventory");

        inventory_panel = 0;
        inventory_info = false;

        now_info = 0; max_info = 0;

        inventory_new_token = new GameObject[3];
        inventory_new_token[0] = inventory_btns[0].transform.Find("New").gameObject;
        inventory_new_token[1] = inventory_btns[1].transform.Find("New").gameObject;
        inventory_new_token[2] = inventory_btns[2].transform.Find("New").gameObject;

        for (int i = 0; i < inventory_new_token.Length; i++)
        { inventory_new_token[i].SetActive(false); }

        inventory_image_sprites = new Sprite[3][];
        inventory_image_sprites[0] = inventory_inventory_sprites;
        inventory_image_sprites[1] = inventory_stall_sprites;
        inventory_image_sprites[2] = inventory_costume_sprites;

        ItemPanelInitialize();
        StallPanelInitialize();
        CostumePanelInitialize();
        Inventory_Category_NewTokenSet();
    }

    // 인벤토리 판넬 열기
    public void InventoryPanel_active()
    {
        UI_Open();
        inventory.SetActive(true);
        Inventory_Category_NewTokenSet();
        Inventory_Panel(0);
    }

    // 인벤토리 판넬 닫기
    public void InventoryPanel_inactive()
    {
        Inventory_Category_NewTokenSet();
        inventory.SetActive(false);
        UI_Close();
    }

    // 인벤토리 판넬 변경
    public void Inventory_Panel(int panel)
    {
        inventory_panel = panel;
        for (int i = 0; i < inventory_pages.Length; i++)
        {
            inventory_pages[i].SetActive(false);
            inventory_btns[i].GetComponent<Image>().sprite = inventory_category_btn_sprites[1];
            inventory_btns_sprites[i].sprite = inventory_image_sprites[i][1];
        }
        Inventory_Category_PageOpen();
    }

    // 인벤토리 카테고리 페이지
    private void Inventory_Category_PageOpen()
    {
        inventory_pages[inventory_panel].SetActive(true);
        inventory_btns[inventory_panel].GetComponent<Image>().sprite = inventory_category_btn_sprites[0];
        inventory_btns_sprites[inventory_panel].sprite = inventory_image_sprites[inventory_panel][0];
        inventory_info = false;

        switch (inventory_panel)
        {
            case 0: ItemInitialize(); Item(); break;
            case 1: StallInitialize(); Stall(); break;
            case 2: CostumeInitialize(); Costume(); break;
        }
    }

    // 인벤토리 카테고리 뉴토큰
    private void Inventory_Category_NewTokenSet()
    {
        inventory_new_token[1].SetActive(Inventory_Stall_New_Check());
        inventory_new_token[2].SetActive(Inventory_Costume_New_Check());
        UIManager.manager.NewTokenSet(1, inventory_new_token[2].activeInHierarchy);
    }

    // 노점 뉴토큰 체크
    private bool Inventory_Stall_New_Check()
    {
        if (DataManager.stalls_save[DataManager.manager.stalls.stalls.Length - 1][DataManager.manager.stalls.stalls[0].Length - 1])
        { return false; }
        else
        { return true; }
    }

    // 코스튬 뉴토큰 체크
    private bool Inventory_Costume_New_Check()
    {
        int count = 0;
        for (int i = 0; i < DataManager.manager.costumes.costumes.Length; i++)
        {
            for (int j = 0; j < DataManager.manager.costumes.costumes[i].Length; j++)
            {
                if (DataManager.costumes_save[i][j].unlocked && !DataManager.costumes_save[i][j].looked)
                { count += 1; }
            }
        }
        if (count > 0)
        { return true; }
        else
        { return false; }
    }


    #region item

    private GameObject inventory_item;          // 인벤토리 아이템 페이지
    private GameObject inventory_item_info;     // 인벤토리 아이템 상세페이지

    private List<Button> item_slots;            // 인벤토리 아이템 슬롯들
    private List<GameObject> item_slot_token;   // 인벤토리 아이템 소지 양 알림 토큰
    private List<Text> item_slot_amount;        // 인벤토리 아이템 소지 양 알림

    private Image item_img;                     // 인벤토리 아이템 이미지
    private GameObject item_amount_bg;          // 인벤토리 아이템 소지 양 배경
    private Text item_amount;                   // 인벤토리 아이템 소지 양
    private Text item_description;              // 인벤토리 아이템 설명
    private Text item_info_btn_txt;             // 인벤토리 더보기 버튼 텍스트

    private GameObject item_prev_btn;           // 아이템 이전 버튼
    private GameObject item_next_btn;           // 아이템 다음 버튼

    private Button item_use_btn;                // 아이템 사용 버튼
    private Text item_use_btn_txt;              // 인벤토리 사용 버튼 텍스트

    private Sprite[] item_sprite;               // 아이템 스프라이트들

    // 인벤토리 아이템 페이지 판넬 초기화
    private void ItemPanelInitialize()
    {
        inventory_item = inventory_pages[0].transform.Find("Items").gameObject;
        inventory_item_info = inventory_pages[0].transform.Find("Item_Info").gameObject;

        item_slots = new List<Button>();
        item_slot_token = new List<GameObject>();
        item_slot_amount = new List<Text>();

        item_sprite = new Sprite[ITEM_SLOT];

        for (int i = 0; i < ITEM_SLOT; i++)
        {
            item_slots.Add(inventory_item.transform.Find("Item_" + i).GetComponent<Button>());
            if (i < ITEM_SLOT - 1)
            {
                item_slot_token.Add(item_slots[i].transform.Find("amount_img").gameObject);
                item_slot_amount.Add(item_slot_token[i].transform.Find("amount").GetComponent<Text>());
            }
        }

        item_img = inventory_item_info.transform.Find("Item_BG").Find("Item_Img").GetComponent<Image>();
        item_amount_bg = inventory_item_info.transform.Find("Amount_BG").gameObject;
        item_amount = item_amount_bg.transform.Find("amount_txt").GetComponent<Text>();
        item_description = inventory_item_info.transform.Find("Description_BG").Find("description_txt").GetComponent<Text>();
        item_info_btn_txt = inventory_item_info.transform.Find("Description_BG").Find("Info_Btn").Find("Info_txt").GetComponent<Text>();
        item_use_btn = inventory_item_info.transform.Find("Use_Btn").GetComponent<Button>();
        item_use_btn_txt = item_use_btn.transform.Find("Text").GetComponent<Text>();
        item_prev_btn = inventory_item_info.transform.Find("Prev_Btn").gameObject;
        item_next_btn = inventory_item_info.transform.Find("Next_Btn").gameObject;

        for (int i = 0; i < ITEM_SLOT; i++)
            item_sprite[i] = item_slots[i].GetComponent<Image>().sprite;

        item_use_btn.onClick.RemoveAllListeners();
        item_use_btn.onClick.AddListener(() => ItemUse(now_info));
    }

    // 인벤토리 아이템 페이지 초기화
    private void ItemInitialize()
    {
        max_info = ITEM_SLOT;
        now_info = 0;
    }

    // 인벤토리 아이템 페이지 오픈
    private void Item()
    {
        GameManager.manager.GetSoundManager().Inventory();
        inventory_item_info.SetActive(false);
        inventory_item.SetActive(true);
        inventory_info = false;

        now_info = 0;

        for (int i = 0; i < ITEM_SLOT - 1; i++)
            SetItem_Slot(i);
    }

    // 인벤토리 아이템 디테일 뷰
    public void Item_Info(int code)
    {
        GameManager.manager.GetSoundManager().Inventory();
        inventory_item_info.SetActive(true);
        inventory_item.SetActive(false);
        inventory_info = true;

        now_info = code;
        name_or_description = false;

        int i_type = GetItemType(code);
        int i_code = GetItemCode(code);

        ImageUI(item_img, item_sprite[code]);
        SetItemTextSet(i_type, i_code);

        ItemInfoBtnSet();
    }

    // 인벤토리 아이템 디테일 뷰 리프레시
    public void Item_Info_Refresh(int type, int code)
    {
        if (DataManager.items_save[type][code] <= 0)
        { Item(); }
        else
        {
            inventory_item_info.SetActive(true);
            inventory_item.SetActive(false);
            inventory_info = true;

            name_or_description = false;

            ImageUI(item_img, item_sprite[now_info]);
            SetItemTextSet(type, code);

            ItemInfoBtnSet();
        }
    }

    // 아이템 타입 리턴
    private int GetItemType(int code)
    {
        for (int i = 0; i < DataManager.manager.items.items.Length; i++)
        {
            if (code < DataManager.manager.items.items[i].Length)
                return i;
            code -= DataManager.manager.items.items[i].Length;
        }
        return -1;
    }

    // 아이템 코드 리턴
    private int GetItemCode(int code)
    {
        if (GetItemType(code) == 4)
        {
            int count = -1;
            for (int i = 0; i < DataManager.stages_save.Length - 1; i++)
            {
                for (int j = 0; j < DataManager.stages_save[i].Length; j++)
                {
                    if (DataManager.stages_save[i][j] == true) count += 1;
                }
            }
            return count;
        }
        else
        {
            for (int i = 0; i < DataManager.manager.items.items.Length; i++)
            {
                if (code < DataManager.manager.items.items[i].Length)
                    return code;
                code -= DataManager.manager.items.items[i].Length;
            }
        }

        return -1;
    }

    // 아이템 슬롯 설정
    private void SetItem_Slot(int code)
    {
        int amount = DataManager.items_save[GetItemType(code)][GetItemCode(code)];
        if (amount <= 0)
        {
            item_slots[code].GetComponent<Touch_Scale>().SetTouchable(false);
            item_slots[code].interactable = false;
            item_slot_token[code].SetActive(false);
        }
        else
        {
            item_slots[code].GetComponent<Touch_Scale>().SetTouchable(true);
            item_slots[code].interactable = true;
            item_slot_token[code].SetActive(true);
            TextUI(item_slot_amount[code], amount);
        }
    }

    // 아이템 텍스트 설정
    private void SetItemTextSet(int type, int code)
    {
        if (item_useable(type))
        {
            item_amount_bg.SetActive(true);
            item_use_btn.interactable = true;
            item_use_btn.GetComponent<Touch_Scale>().SetTouchable(true);
            TextUI(item_use_btn_txt, DataManager.manager.ui.inventory.GetUse());
            TextUI(item_amount, DataManager.items_save[type][code].ToString("00") + " / " + DataManager.manager.items.items[type][code].max_amount.ToString("00"));
        }
        else
        {
            item_amount_bg.SetActive(false);
            item_use_btn.interactable = false;
            item_use_btn.GetComponent<Touch_Scale>().SetTouchable(false);
            TextUI(item_use_btn_txt, DataManager.manager.ui.inventory.GetCantUse());
        }

        if (!name_or_description)
        {
            TextUI(item_description, DataManager.manager.items.items[type][code].GetName());
            TextUI(item_info_btn_txt, DataManager.manager.ui.inventory.GetMoreInfo());
        }
        else
        {
            TextUI(item_description, DataManager.manager.items.items[type][code].GetEffectDescription());
            TextUI(item_info_btn_txt, DataManager.manager.ui.inventory.GetBack());
        }
    }

    // 아이템 더보기 터치
    public void ItemMoreInfo()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        name_or_description = !name_or_description;
        SetItemTextSet(GetItemType(now_info), GetItemCode(now_info));
    }

    // 아이템 사용
    public void ItemUse(int code)
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        use_item(GetItemType(code), GetItemCode(code));
        if (DataManager.items_save[GetItemType(code)][GetItemCode(code)] > 0)
            Item_Info(code);
        else
            Item();
    }

    // 이전 데이터 가져오기
    private int GetPrevDataIndex(int index)
    {
        for (int i = index - 1; i >= 0; i--)
        {
            if (DataManager.items_save[GetItemType(i)][GetItemCode(i)] > 0) return i;
        }
        return -1;
    }

    // 다음 데이터 가져오기
    private int GetNextDataIndex(int index)
    {
        for (int i = index + 1; i < max_info; i++)
        {
            if (i == max_info - 1) return i;
            else
            {
                if (DataManager.items_save[GetItemType(i)][GetItemCode(i)] > 0) return i;
            }
        }
        return -1;
    }

    // 버튼 설정
    private void ItemInfoBtnSet()
    {
        if (GetPrevDataIndex(now_info) > -1)
            item_prev_btn.SetActive(true);
        else
            item_prev_btn.SetActive(false);

        if (GetNextDataIndex(now_info) > -1)
            item_next_btn.SetActive(true);
        else
            item_next_btn.SetActive(false);
    }

    // 아이템 스프라이트 받아오기
    public Sprite GetItemSprite(int index)
    {
        return item_sprite[index];
    }

    // 이전 아이템
    public void ItemPrevPage()
    {
        if (now_info > 0)
        {
            if (GetPrevDataIndex(now_info) > -1)
            {
                Item_Info(GetPrevDataIndex(now_info));
            }
        }
    }

    // 이후 아이템
    public void ItemNextPage()
    {
        if (now_info < max_info - 1)
        {
            if (GetNextDataIndex(now_info) > -1)
            {
                Item_Info(GetNextDataIndex(now_info));
            }
        }
    }

    #endregion

    #region stall

    private int STALL_SLOT;                     // 인벤토리 노점 슬롯 개수
    private GameObject inventory_stall;         // 인벤토리 노점 페이지
    private GameObject inventory_stall_info;    // 인벤토리 노점 상세페이지

    private List<Button> stall_slots;           // 인벤토리 노점 슬롯들
    private List<GameObject> stall_slot_img;    // 인벤토리 노점 슬롯 내부 이미지
    private List<GameObject> stall_slot_new;    // 인벤토리 노점 슬롯 뉴 토큰

    private Image stall_img;                    // 인벤토리 노점 이미지
    private Text stall_price;                   // 인벤토리 노점 가격
    private Text stall_description;             // 인벤토리 노점 설명

    private Button stall_buy_btn;               // 인벤토리 노점 구매 버튼
    private Text stall_buy_btn_txt;             // 인벤토리 노점 구매 버튼 텍스트
    private Button stall_back_btn;              // 인벤토리 노점 뒤로가기 버튼
    private Text stall_back_btn_txt;            // 인벤토리 노점 뒤로가기 버튼 텍스트

    private Sprite[] stall_sprite;              // 노점 스프라이트들

    // 인벤토리 노점 페이지 판넬 초기화
    private void StallPanelInitialize()
    {
        STALL_SLOT = DataManager.stalls_save.Length * DataManager.stalls_save[0].Length;

        inventory_stall = inventory_pages[1].transform.Find("Stalls").gameObject;
        inventory_stall_info = inventory_pages[1].transform.Find("Stall_Info").gameObject;

        stall_slots = new List<Button>();
        stall_slot_img = new List<GameObject>();
        stall_slot_new = new List<GameObject>();

        stall_sprite = new Sprite[STALL_SLOT];
        for (int i = 0; i < STALL_SLOT; i++)
        {
            stall_slots.Add(inventory_stall.transform.Find("Viewport").Find("Content").Find("Stall_" + i).GetComponent<Button>());
            stall_slot_img.Add(stall_slots[i].transform.Find("Image").gameObject);
            stall_slot_new.Add(stall_slots[i].transform.Find("New").gameObject);
        }

        stall_img = inventory_stall_info.transform.Find("Stall_Img").GetComponent<Image>();
        stall_price = inventory_stall_info.transform.Find("Stall_Price_BG").Find("price_txt").GetComponent<Text>();
        stall_description = inventory_stall_info.transform.Find("description_txt").GetComponent<Text>();
        stall_buy_btn = inventory_stall_info.transform.Find("Stall_Buy_Btn").GetComponent<Button>();
        stall_buy_btn_txt = stall_buy_btn.transform.Find("Text").GetComponent<Text>();
        stall_back_btn = inventory_stall_info.transform.Find("Stall_Exit_Btn").GetComponent<Button>();
        stall_back_btn_txt = stall_back_btn.transform.Find("Text").GetComponent<Text>();

        for (int i = 0; i < STALL_SLOT; i++)
        {
            stall_sprite[i] = Data_Container.stall_container.GetStallIcon(i);
            stall_slot_img[i].GetComponent<Image>().sprite = stall_sprite[i];
            stall_slot_new[i].SetActive(false);
        }

        stall_back_btn.onClick.RemoveAllListeners();
        stall_back_btn.onClick.AddListener(() => Stall());
    }

    // 인벤토리 노점 페이지 초기화
    private void StallInitialize()
    {
        max_info = STALL_SLOT;
        now_info = 0;
    }

    // 인벤토리 노점 페이지 오픈
    private void Stall()
    {
        GameManager.manager.GetSoundManager().Inventory();
        inventory_stall_info.SetActive(false);
        inventory_stall.SetActive(true);
        inventory_info = false;

        now_info = 0;

        for (int i = 0; i < STALL_SLOT; i++)
        { SetStall_Slot(i); }

        Inventory_Category_NewTokenSet();
    }

    // 인벤토리 노점 디테일 뷰
    public void Stall_Info(int code)
    {
        GameManager.manager.GetSoundManager().Inventory();
        inventory_stall_info.SetActive(true);
        inventory_stall.SetActive(false);
        inventory_info = true;

        now_info = code;
        name_or_description = false;

        int s_type = GetStallType(code);
        int s_code = GetStallCode(code);

        ImageUI(stall_img, stall_sprite[code]);
        SetStallTextSet(s_type, s_code);

        Inventory_Category_NewTokenSet();
    }

    // 노점 타입 리턴
    private int GetStallType(int code)
    {
        for (int i = 0; i < DataManager.manager.stalls.stalls.Length; i++)
        {
            if (code < DataManager.manager.stalls.stalls[i].Length)
                return i;
            code -= DataManager.manager.stalls.stalls[i].Length;
        }
        return -1;
    }

    // 노점 코드 리턴
    private int GetStallCode(int code)
    {
        for (int i = 0; i < DataManager.manager.stalls.stalls.Length; i++)
        {
            if (code < DataManager.manager.stalls.stalls[i].Length)
                return code;
            code -= DataManager.manager.stalls.stalls[i].Length;
        }
        return -1;
    }

    // 노점 슬롯 설정
    private void SetStall_Slot(int code)
    {
        stall_slot_new[code].SetActive(false);
        if (DataManager.stalls_save[GetStallType(code)][GetStallCode(code)] || DataManager.stalls_save[GetStallType(code - 1)][GetStallCode(code - 1)])
        {
            stall_slots[code].GetComponent<Touch_Scale>().SetTouchable(true);
            stall_slots[code].interactable = true;
            stall_slot_img[code].SetActive(true);

            if (!DataManager.stalls_save[GetStallType(code)][GetStallCode(code)])
            { stall_slot_new[code].SetActive(true); }
        }
        else
        {
            stall_slots[code].GetComponent<Touch_Scale>().SetTouchable(false);
            stall_slots[code].interactable = false;
            stall_slot_img[code].SetActive(false);
        }
    }

    // 노점 텍스트 설정
    private void SetStallTextSet(int type, int code)
    {
        if (!name_or_description)
            TextUI(stall_description, DataManager.manager.stalls.stalls[type][code].GetName());
        else
            TextUI(stall_description, DataManager.manager.ui.inventory.GetStallAmount() + DataManager.manager.stalls.stalls[type][code].product_display.Length);

        TextUI(stall_price, DataManager.manager.stalls.stalls[type][code].price);
        TextUI(stall_back_btn_txt, DataManager.manager.ui.inventory.GetExit());

        SetStallBuyBtn(type, code);
    }

    // 노점 구매 버튼 설정
    private void SetStallBuyBtn(int type, int code)
    {
        stall_buy_btn.onClick.RemoveAllListeners();

        // 현재 노점이 해금되어있는 경우
        if (DataManager.stalls_save[type][code])
        {
            // 사용중인 경우
            if (DataManager.manager.stall_save.now_stall == now_info)
            {
                stall_buy_btn.interactable = false;
                stall_buy_btn.GetComponent<Touch_Scale>().SetTouchable(false);
                TextUI(stall_buy_btn_txt, DataManager.manager.ui.inventory.GetNowUse());
                ImageUI(stall_buy_btn.gameObject, inventory_btn_sprites[3]);
            }
            else
            {
                stall_buy_btn.onClick.AddListener(() => ChangeStall(now_info));
                stall_buy_btn.interactable = true;
                stall_buy_btn.GetComponent<Touch_Scale>().SetTouchable(true);
                TextUI(stall_buy_btn_txt, DataManager.manager.ui.inventory.GetUse());
                ImageUI(stall_buy_btn.gameObject, inventory_btn_sprites[0]);
            }
        }
        else
        {
            // 현재 노점의 이전 노점 해금되어있을 경우
            if (DataManager.stalls_save[GetStallType(now_info - 1)][GetStallCode(now_info - 1)])
            {
                stall_buy_btn.onClick.AddListener(() => BuyStall(now_info));
                stall_buy_btn.interactable = true;
                stall_buy_btn.GetComponent<Touch_Scale>().SetTouchable(true);
                TextUI(stall_buy_btn_txt, DataManager.manager.ui.inventory.GetBuy());
                ImageUI(stall_buy_btn.gameObject, inventory_btn_sprites[2]);
            }
            else
            {
                stall_buy_btn.interactable = false;
                stall_buy_btn.GetComponent<Touch_Scale>().SetTouchable(false);
                TextUI(stall_buy_btn_txt, DataManager.manager.ui.inventory.GetCantBuy());
                ImageUI(stall_buy_btn.gameObject, inventory_btn_sprites[3]);
            }
        }
    }

    // 노점 변경
    private void ChangeStall(int code)
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        change_stall(code);
        SetStallTextSet(GetStallType(code), GetStallCode(code));
    }

    // 노점 구매
    private void BuyStall(int code)
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        if (get_money() >= DataManager.manager.stalls.stalls[GetStallType(code)][GetStallCode(code)].price)
        {
            // 구매노점 전달
            buy_stall(code);
            SetStallTextSet(GetStallType(code), GetStallCode(code));
        }
        else
        {
            MsgBox_Controller.manager.NomoneyMsg();
        }
    }

    // 노점 더보기 터치
    public void StallMoreInfo()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        name_or_description = !name_or_description;
        SetStallTextSet(GetStallType(now_info), GetStallCode(now_info));
    }

    #endregion

    #region costume

    private int INTERIOR_SLOT;                  // 인벤토리 인테리어 슬롯 개수
    private int COSTUME_SLOT;                   // 인벤토리 코스튬 슬롯 개수
    private GameObject inventory_costume;       // 인벤토리 코스튬 페이지
    private GameObject inventory_costume_info;  // 인벤토리 코스튬 상세페이지

    private List<GameObject> interior_slots;    // 인벤토리 인테리어 슬롯들
    private List<Button> interior_btns;         // 인벤토리 인테리어 슬롯 버튼
    private List<GameObject> interior_slot_img; // 인벤토리 인테리어 슬롯 내부 이미지
    private List<GameObject> interior_slot_new; // 인벤토리 인테리어 슬롯 뉴 토큰

    private List<GameObject> costume_slots;     // 인벤토리 코스튬 슬롯들
    private List<Button> costume_btns;          // 인벤토리 코스튬 슬롯 버튼    
    private List<GameObject> costume_slot_img;  // 인벤토리 코스튬 슬롯 내부 이미지
    private List<GameObject> costume_slot_new;  // 인벤토리 코스튬 슬롯 뉴 토큰

    private Image costume_img;                  // 인벤토리 코스튬 이미지
    private Text costume_description;           // 인벤토리 코스튬 설명

    private Button costume_use_btn;             // 인벤토리 코스튬 구매 버튼
    private Text costume_use_btn_txt;           // 인벤토리 코스튬 구매 버튼 텍스트
    private Button costume_back_btn;            // 인벤토리 코스튬 뒤로가기 버튼
    private Text costume_back_btn_txt;          // 인벤토리 코스튬 뒤로가기 버튼 텍스트

    private int c_type, c_code;                 // 현재 타입/코드
    private bool now_interior;                  // 인테리어 눌렀는지

    private Vector3 interior_scale;             // 인테리어 스케일
    private Vector3 costume_big_scale;          // 큰 코스튬 스케일
    private Vector3 costume_small_scale;        // 작은 코스튬 스케일

    private Vector3 interior_info_scale;        // 인테리어 세부 스케일
    private Vector3 costume_info_big_scale;     // 큰 코스튬 세부 스케일
    private Vector3 costume_info_small_scale;   // 작은 코스튬 세부 스케일

    private GameObject costume_slot_pref;       // 인벤토리 코스튬 슬롯 프리팹

    // 인벤토리 코스튬 페이지 판넬 초기화
    private void CostumePanelInitialize()
    {
        inventory_costume = inventory_pages[2].transform.Find("Costumes").gameObject;
        inventory_costume_info = inventory_pages[2].transform.Find("Costume_Info").gameObject;

        costume_slot_pref = Resources.Load<GameObject>("Prefabs/UI/Costume_Slot");

        interior_scale = new Vector3(1.5f, 1.5f, 1.5f);
        costume_big_scale = new Vector3(0.45f, 0.45f, 0.45f);
        costume_small_scale = new Vector3(1.1f, 1.1f, 1.1f);

        interior_info_scale = new Vector3(2.0f, 2.0f, 2.0f);
        costume_info_big_scale = new Vector3(0.8f, 0.8f, 0.8f);
        costume_info_small_scale = new Vector3(1.0f, 1.0f, 1.0f);

        INTERIOR_SLOT = DataManager.manager.interiors.interior_theme.Length;
        interior_slots = new List<GameObject>();
        interior_btns = new List<Button>();
        interior_slot_img = new List<GameObject>();
        interior_slot_new = new List<GameObject>();
        for (int i = 0; i < INTERIOR_SLOT; i++)
        {
            interior_slots.Add(Instantiate(costume_slot_pref, inventory_costume.transform.Find("Viewport").Find("Content")));
            interior_btns.Add(interior_slots[i].GetComponent<Button>());
            interior_slot_img.Add(interior_slots[i].transform.Find("Image").gameObject);
            interior_slot_new.Add(interior_slots[i].transform.Find("New").gameObject);
        }

        COSTUME_SLOT = 0;
        for (int i = 0; i < DataManager.manager.costumes.costumes.Length; i++)
        { COSTUME_SLOT += DataManager.manager.costumes.costumes[i].Length; }
        costume_slots = new List<GameObject>();
        costume_btns = new List<Button>();
        costume_slot_img = new List<GameObject>();
        costume_slot_new = new List<GameObject>();
        for (int i = 0; i < COSTUME_SLOT; i++)
        {
            costume_slots.Add(Instantiate(costume_slot_pref, inventory_costume.transform.Find("Viewport").Find("Content")));
            costume_btns.Add(costume_slots[i].GetComponent<Button>());
            costume_slot_img.Add(costume_slots[i].transform.Find("Image").gameObject);
            costume_slot_new.Add(costume_slots[i].transform.Find("New").gameObject);
        }

        costume_img = inventory_costume_info.transform.Find("Costume_Img").GetComponent<Image>();
        costume_description = inventory_costume_info.transform.Find("description_txt").GetComponent<Text>();
        costume_use_btn = inventory_costume_info.transform.Find("Costume_Use_Btn").GetComponent<Button>();
        costume_use_btn_txt = costume_use_btn.transform.Find("Text").GetComponent<Text>();
        costume_back_btn = inventory_costume_info.transform.Find("Costume_Exit_Btn").GetComponent<Button>();
        costume_back_btn_txt = costume_back_btn.transform.Find("Text").GetComponent<Text>();

        c_type = 0; c_code = 0;

        costume_back_btn.onClick.RemoveAllListeners();
        costume_back_btn.onClick.AddListener(() => Costume());
    }

    // 인벤토리 코스튬 페이지 초기화
    private void CostumeInitialize()
    {
        max_info = INTERIOR_SLOT + COSTUME_SLOT;
        now_info = 0;
        c_type = 0; c_code = 0;
    }
    // 인벤토리 코스튬 페이지 오픈
    private void Costume()
    {
        GameManager.manager.GetSoundManager().Inventory();
        inventory_costume_info.SetActive(false);
        inventory_costume.SetActive(true);
        inventory_info = false;

        now_info = 0;
        c_type = 0; c_code = 0;

        for (int i = 0; i < INTERIOR_SLOT; i++)
        { SetInterior_Slot(i); }
        for (int i = 0; i < COSTUME_SLOT; i++)
        { SetCostume_Slot(i); }

        Inventory_Category_NewTokenSet();
    }

    // 인벤토리 인테리어 디테일 뷰
    public void Interior_Info(int code)
    {
        GameManager.manager.GetSoundManager().Inventory();
        inventory_costume_info.SetActive(true);
        inventory_costume.SetActive(false);
        inventory_info = true;

        now_info = code;
        name_or_description = false;
        now_interior = true;

        ImageUI(costume_img, Data_Container.interior_container.GetInteriorIcon(code));
        costume_img.GetComponent<RectTransform>().localScale = interior_info_scale;
        SetInteriorTextSet(code);

        Inventory_Category_NewTokenSet();
    }

    // 인벤토리 코스튬 디테일 뷰
    public void Costume_Info(int code)
    {
        GameManager.manager.GetSoundManager().Inventory();
        inventory_costume_info.SetActive(true);
        inventory_costume.SetActive(false);
        inventory_info = true;

        now_info = code;
        name_or_description = false;
        now_interior = false;

        c_type = GetCostumeType(code);
        c_code = GetCostumeCode(code);

        if (!DataManager.costumes_save[c_type][c_code].looked)
        { DataManager.costumes_save[c_type][c_code].looked = true; }

        ImageUI(costume_img, Data_Container.costume_container.GetCostume_Icon(c_type, c_code));
        if (c_type == 0)
        { costume_img.GetComponent<RectTransform>().localScale = costume_info_big_scale; }
        else
        { costume_img.GetComponent<RectTransform>().localScale = costume_info_small_scale; }
        SetCostumeTextSet(c_type, c_code);

        Inventory_Category_NewTokenSet();
    }

    // 인테리어 슬롯 설정
    private void SetInterior_Slot(int code)
    {
        if (!DataManager.manager.interior_save.interior_theme[code].unlocked_all)
        { interior_slots[code].SetActive(false); }
        else
        {
            interior_slots[code].SetActive(true);

            // 추후 수정
            interior_slot_new[code].SetActive(false);

            ImageUI(interior_slots[code], inventory_interior_frame);
            ImageUI(interior_slot_img[code], Data_Container.interior_container.GetInteriorIcon(code));
            interior_slot_img[code].GetComponent<RectTransform>().localScale = interior_scale;
            interior_btns[code].onClick.RemoveAllListeners();
            interior_btns[code].onClick.AddListener(() => Interior_Info(code));
        }
    }

    // 코스튬 슬롯 설정
    private void SetCostume_Slot(int code)
    {
        c_type = GetCostumeType(code);
        c_code = GetCostumeCode(code);

        if (!DataManager.costumes_save[c_type][c_code].unlocked)
        { costume_slots[code].SetActive(false); }
        else
        {
            if (DataManager.costumes_save[c_type][c_code].looked)
            { costume_slot_new[code].SetActive(false); }
            else
            { costume_slot_new[code].SetActive(true); }

            if (c_type == 2 && DataManager.skills_save[4][1] < 0)
            { costume_slots[code].SetActive(false); }
            else
            {
                costume_slots[code].SetActive(true);

                ImageUI(costume_slots[code], inventory_costume_frame[c_type]);
                ImageUI(costume_slot_img[code], Data_Container.costume_container.GetCostume_Icon(c_type, c_code));
                switch (c_type)
                {
                    case 0:
                        costume_slot_img[code].GetComponent<RectTransform>().localScale = costume_big_scale;
                        break;
                    case 1:
                        costume_slot_img[code].GetComponent<RectTransform>().localScale = costume_info_big_scale;
                        break;
                    case 2:
                        costume_slot_img[code].GetComponent<RectTransform>().localScale = costume_big_scale;
                        break;
                }
                costume_btns[code].onClick.RemoveAllListeners();
                costume_btns[code].onClick.AddListener(() => Costume_Info(code));
            }
        }
    }

    // 코스튬 타입 리턴
    private int GetCostumeType(int code)
    {
        for (int i = 0; i < DataManager.manager.costumes.costumes.Length; i++)
        {
            if (code < DataManager.manager.costumes.costumes[i].Length)
            { return i; }
            code -= DataManager.manager.costumes.costumes[i].Length;
        }
        return -1;
    }

    // 코스튬 코드 리턴
    private int GetCostumeCode(int code)
    {
        for (int i = 0; i < DataManager.manager.costumes.costumes.Length; i++)
        {
            if (code < DataManager.manager.costumes.costumes[i].Length)
            { return code; }
            code -= DataManager.manager.costumes.costumes[i].Length;
        }
        return -1;
    }

    // 인테리어 텍스트 설정
    private void SetInteriorTextSet(int code)
    {
        if (!name_or_description)
        { TextUI(costume_description, DataManager.manager.interiors.interior_theme[code].GetName()); }
        else
        { TextUI(costume_description, DataManager.manager.interiors.interior_theme[code].GetDescription()); }
        TextUI(costume_back_btn_txt, DataManager.manager.ui.inventory.GetExit());
        SetInteriorBtn(code);
    }

    // 인테리어 버튼 설정
    private void SetInteriorBtn(int code)
    {
        costume_use_btn.onClick.RemoveAllListeners();

        // 해금되어있는 경우
        if (DataManager.manager.interior_save.interior_theme[code].unlocked_all)
        {
            // 사용중인 경우
            if (DataManager.manager.player_save.player_house.sleeproom.theme == code)
            {
                costume_use_btn.interactable = false;
                costume_use_btn.GetComponent<Touch_Scale>().SetTouchable(false);
                TextUI(costume_use_btn_txt, DataManager.manager.ui.inventory.GetNowUse());
                ImageUI(costume_use_btn.gameObject, inventory_btn_sprites[3]);
            }
            else
            {
                costume_use_btn.onClick.AddListener(() => ChangeInterior(now_info));
                costume_use_btn.interactable = true;
                costume_use_btn.GetComponent<Touch_Scale>().SetTouchable(true);
                TextUI(costume_use_btn_txt, DataManager.manager.ui.inventory.GetUse());
                ImageUI(costume_use_btn.gameObject, inventory_btn_sprites[0]);
            }
        }
    }

    // 코스튬 텍스트 설정
    private void SetCostumeTextSet(int type, int code)
    {
        if (!name_or_description)
        { TextUI(costume_description, DataManager.manager.costumes.costumes[type][code].GetName()); }
        else
        { TextUI(costume_description, DataManager.manager.costumes.costumes[type][code].GetDescription()); }

        TextUI(costume_back_btn_txt, DataManager.manager.ui.inventory.GetExit());
        SetCostumeBtn(type, code);
    }

    // 코스튬 버튼 설정
    private void SetCostumeBtn(int type, int code)
    {
        costume_use_btn.onClick.RemoveAllListeners();

        switch (type)
        {
            case 0:
            case 1:
                // 현재 코스튬이 해금되어있는 경우
                if (DataManager.costumes_save[type][code].unlocked)
                {
                    costume_use_btn.GetComponent<Touch_Scale>().SetTouchable(true);
                    costume_use_btn.onClick.AddListener(() => ChangeCostume(now_info));
                    costume_use_btn.interactable = true;
                    // 사용중인 경우
                    if (Costume_UseCheck(type, code))
                    {
                        // costume_use_btn.interactable = false;
                        // costume_use_btn.GetComponent<Touch_Scale>().SetTouchable(false);
                        TextUI(costume_use_btn_txt, DataManager.manager.ui.inventory.GetDenyUse());
                        ImageUI(costume_use_btn.gameObject, inventory_btn_sprites[3]);
                    }
                    else
                    {
                        TextUI(costume_use_btn_txt, DataManager.manager.ui.inventory.GetUse());
                        ImageUI(costume_use_btn.gameObject, inventory_btn_sprites[0]);
                    }
                }
                break;
            case 2:
                if (DataManager.costumes_save[type][code].unlocked)
                {
                    // 사용중인 경우
                    if (Costume_UseCheck(type, code))
                    {
                        costume_use_btn.interactable = false;
                        costume_use_btn.GetComponent<Touch_Scale>().SetTouchable(false);
                        TextUI(costume_use_btn_txt, DataManager.manager.ui.inventory.GetNowUse());
                        ImageUI(costume_use_btn.gameObject, inventory_btn_sprites[3]);
                    }
                    else
                    {
                        costume_use_btn.GetComponent<Touch_Scale>().SetTouchable(true);
                        costume_use_btn.onClick.AddListener(() => ChangeCostume(now_info));
                        costume_use_btn.interactable = true;
                        TextUI(costume_use_btn_txt, DataManager.manager.ui.inventory.GetUse());
                        ImageUI(costume_use_btn.gameObject, inventory_btn_sprites[0]);
                    }
                }
                break;
        }
    }

    // 인테리어 변경
    private void ChangeInterior(int code)
    {
        change_interior(code);
        SetInteriorTextSet(code);
    }

    // 코스튬 변경
    private void ChangeCostume(int code)
    {
        change_costume(GetCostumeType(code), GetCostumeCode(code));
        SetCostumeTextSet(GetCostumeType(code), GetCostumeCode(code));
    }

    // 코스튬 더보기 터치
    public void CostumeMoreInfo()
    {
        name_or_description = !name_or_description;
        if (now_interior)
        { SetInteriorTextSet(now_info); }
        else
        { SetCostumeTextSet(GetCostumeType(now_info), GetCostumeCode(now_info)); }
    }

    // 코스튬 매치 체크
    private bool Costume_UseCheck(int type, int code)
    {
        switch (type)
        {
            case 0:
                if (code == DataManager.manager.player_save.player_costume.player_head)
                { return true; }
                break;
            case 1:
                if (code == DataManager.manager.player_save.player_costume.player_eye)
                { return true; }
                break;
            case 2:
                if (code == DataManager.manager.player_save.player_costume.player_cat[0])
                { return true; }
                break;
        }
        return false;
    }

    #endregion

    #region Delegates

    public delegate int GetData();
    public static event GetData get_money;
    public static event GetData get_soulseed;

    public delegate void StallSet(int stall_code);
    public static event StallSet change_stall;
    public static event StallSet buy_stall;

    public delegate void CostumeSet(int costume_type, int costume_code);
    public static event CostumeSet change_costume;

    public delegate void CostumeBuy(int costume_type, int costume_code, bool mote_or_seed);
    public static event CostumeBuy buy_costume;

    public static event StallSet change_interior;
    public static event StallSet buy_interior;

    public delegate bool ItemUseable(int type);
    public static event ItemUseable item_useable;

    public delegate void UseItem(int type, int code);
    public static event UseItem use_item;

    #endregion
}
