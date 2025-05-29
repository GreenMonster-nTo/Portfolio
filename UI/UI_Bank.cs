using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Bank : UI_Panel
{
    private int BANK_MAX_AMOUNT = 5000000;  // 은행 예금 최대치
    private int BANK_MIN_AMOUNT = 100;      // 은행 예금 최소치

    public GameObject bank;                 // 은행
    public GameObject remove_ad;            // 광고제거 판
    public GameObject bank_account;         // 은행 어카운트창
    public GameObject bank_description;     // 은행 설명창
    public GameObject bank_saving_withdraw; // 은행 입출금창

    public GameObject bank_ad_icon;         // 은행 광고제거 아이콘

    public GameObject bank_new;             // 은행 뉴 토큰

    // 은행 판넬 이니셜라이즈
    private void InitializeBankPanel()
    {
        UI_Initialize("bank");

        InitializeBank();
        InitializeBankDescription();
        InitializeBankAccount();
        InitializeLottery();
    }

    // 현재 소지금
    private void Now_Bank_Thing_Init()
    {
        now_saved_mote = 0;
        now_exchanged_mote = 0;
        now_exchanged_sp = 0;
        now_used_seed = 0;
        now_exchanged_seed = 0;
        used_mote_for_lottery = 0;
        lottery_mote = 0;
        lottery_seed = 0;
        lottery_saves.Clear();
    }

    // 현재 소지금 계산
    private void Check_Money()
    {
        int whole_bank_mote = now_saved_mote + used_mote_for_lottery + now_exchanged_mote;
        int whole_bank_seed = now_used_seed + now_exchanged_seed;

        UIManager.manager.MoneyUI(get_mote() + whole_bank_mote + lottery_mote);
        UIManager.manager.SoulSeedUI(get_soulseed() + whole_bank_seed + lottery_seed);
    }

    private int GetMote()
    {
        int whole_bank_mote = get_mote() + now_saved_mote + used_mote_for_lottery + now_exchanged_mote + lottery_mote;
        return whole_bank_mote;
    }

    private int GetSoulseed()
    {
        int whole_bank_seed = get_soulseed() + now_used_seed + now_exchanged_seed + lottery_seed;
        return whole_bank_seed;
    }

    #region Bank

    public GameObject hamster;                  // 주인 햄스터
    public Image hamster_img;                   // 주인 햄스터 이미지
    public Sprite[] hamster_normal;             // 노말 스프라이트 (0 idle, 1 changed, 2 kuji)
    public Sprite[] hamster_removead;           // 광고제거 이후 스프라이트 (0 idle, 1 changed, 2 kuji)

    private GameObject bank_content;            // 스크롤 뷰 콘텐트
    private GameObject[] soulseed_buy_slots;    // 영씨 구매 슬롯
    private Text[] soulseed_buy_slot_money_txt; // 영씨 구매 슬롯 현금가 텍스트
    private Text[] soulseed_buy_slot_seed_txt;  // 영씨 구매 슬롯 영씨 개수 텍스트

    private GameObject[] sp_buy_slots;          // SP 구매 슬롯
    private GameObject sp_seed_img;             // SP 영씨 이미지
    private Text[] sp_buy_slot_seed_txt;        // SP 구매 슬롯 영씨 가격 텍스트
    private Text[] sp_buy_slot_getsp_txt;       // SP 구매 슬롯 획득 SP 텍스트

    private GameObject[] exchange_slots;        // 환전 슬롯
    private Text[] exchange_slot_seed_txt;      // 환전 영씨 텍스트
    private Text[] exchange_slot_mote_txt;      // 환전 가격 텍스트

    public Animator remove_ad_anim;             // 광고제거 판넬 애니메이션
    public Text remove_ad_txt;                  // 광고제거 글씨

    public Text bank_account_title;             // 예금계좌 텍스트
    public Text bank_account_amount_txt;        // 예금계좌 돈 텍스트

    public GameObject ask_panel;                // 영씨 부족시 구매할건지 물어보기 판넬
    private Image ask_img;                      // 구매영씨 이미지
    private Text ask_txt;                       // 구매영씨 질문 텍스트
    private Button ask_ok_btn;                  // 오케이버튼
    private Text ask_ok_btn_txt;                // 오케이버튼 텍스트
    private Text ask_cancle_btn_txt;            // 취소버튼 텍스트

    private bool bought_something;              // 뭔가를 샀음
    private bool remove_ad_temp;                // 광고제거 (임시)

    private int now_saved_mote;                 // 예금계좌 중첩 계산용
    private int now_used_seed;                  // 예금계좌에서 쓴 영씨
    private int now_exchanged_mote;             // 현재 전환한 티끌
    private int now_exchanged_sp;               // 현재 전환한 영감
    private int now_exchanged_seed;             // 현재 전환한 영씨

    private bool spbook_adloaded;               // SP 책 광고로 이용가능한 경우

    // 환전 은행 이니셜라이즈
    private void InitializeBank()
    {
        bank_content = bank.transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;

        soulseed_buy_slots = new GameObject[DataManager.manager.purchases.soulseed.Length];
        soulseed_buy_slot_money_txt = new Text[soulseed_buy_slots.Length];
        soulseed_buy_slot_seed_txt = new Text[soulseed_buy_slots.Length];

        for (int i = 0; i < soulseed_buy_slots.Length; i++)
        {
            soulseed_buy_slots[i] = bank_content.transform.Find("Seed_" + i).gameObject;
            soulseed_buy_slot_money_txt[i] = soulseed_buy_slots[i].transform.Find("Money_txt").GetComponent<Text>();
            soulseed_buy_slot_seed_txt[i] = soulseed_buy_slots[i].transform.Find("Soulseed_txt").GetComponent<Text>();
        }

        sp_buy_slots = new GameObject[DataManager.manager.purchases.change_sp.Length];
        sp_buy_slot_seed_txt = new Text[sp_buy_slots.Length];
        sp_buy_slot_getsp_txt = new Text[sp_buy_slots.Length];

        for (int i = 0; i < sp_buy_slots.Length; i++)
        {
            sp_buy_slots[i] = bank_content.transform.Find("SP_" + i).gameObject;
            sp_buy_slot_seed_txt[i] = sp_buy_slots[i].transform.Find("Money_txt").GetComponent<Text>();
            sp_buy_slot_getsp_txt[i] = sp_buy_slots[i].transform.Find("SPBook_txt").GetComponent<Text>();
        }
        sp_seed_img = sp_buy_slots[0].transform.Find("soulseed").gameObject;

        exchange_slots = new GameObject[DataManager.manager.purchases.change_money.Length];
        exchange_slot_seed_txt = new Text[exchange_slots.Length];
        exchange_slot_mote_txt = new Text[exchange_slots.Length];

        for (int i = 0; i < exchange_slots.Length; i++)
        {
            exchange_slots[i] = bank_content.transform.Find("Change_" + i).gameObject;
            exchange_slot_seed_txt[i] = exchange_slots[i].transform.Find("Soulseed_txt").GetComponent<Text>();
            exchange_slot_mote_txt[i] = exchange_slots[i].transform.Find("Mote_txt").GetComponent<Text>();
        }

        ask_img = ask_panel.transform.Find("Ask_BuySeed").Find("Image").GetComponent<Image>();
        ask_txt = ask_panel.transform.Find("Ask_BuySeed").Find("Description").GetComponent<Text>();
        ask_ok_btn = ask_panel.transform.Find("Ask_BuySeed").Find("Btn_0").GetComponent<Button>();
        ask_ok_btn_txt = ask_ok_btn.transform.Find("Text").GetComponent<Text>();
        ask_cancle_btn_txt = ask_panel.transform.Find("Ask_BuySeed").Find("Btn_1").Find("Text").GetComponent<Text>();

        if (DataManager.manager.player_cloud_data.ad_remove)
        {
            bank_ad_icon.SetActive(false);
            remove_ad_txt.gameObject.SetActive(false);
        }
        else
        {
            bank_ad_icon.SetActive(true);
            remove_ad_txt.gameObject.SetActive(true);
        }

        spbook_adloaded = false;
    }

    // 환전 은행 액티브
    private void BankInitialize()
    {
        ask_panel.SetActive(false);

        TextUI(bank_account_title, DataManager.manager.ui.bank.GetAccount() + DataManager.manager.ui.bank.GetBankAccount());
        TextUI(bank_account_amount_txt, UIManager.manager.Money_Change(DataManager.manager.bank_save.savings + (int)DataManager.manager.bank_save.interest));

        remove_ad_anim.SetBool("touched", false);
        spbook_adloaded = GameManager.manager.CheckAdLoadable("spbook");
        sp_seed_img.SetActive(!spbook_adloaded);
        if (DataManager.manager.player_cloud_data.ad_remove)
        {
            remove_ad_anim.SetBool("bought", true);
            remove_ad_info_anim.SetBool("bought", true);
            ImageUI(hamster_img, hamster_removead[0]);
            TextUI(remove_ad_txt, DataManager.manager.ui.GetRemoveAd());
            bank_ad_icon.SetActive(false);
            remove_ad_txt.gameObject.SetActive(false);
        }
        else
        {
            remove_ad_anim.SetBool("bought", false);
            remove_ad_info_anim.SetBool("bought", false);
            ImageUI(hamster_img, hamster_normal[0]);
            TextUI(remove_ad_txt, DataManager.manager.ui.GetRemoveAd());
            bank_ad_icon.SetActive(true);
            remove_ad_txt.gameObject.SetActive(true);
        }

        for (int i = 0; i < soulseed_buy_slots.Length; i++)
        {
            TextUI(soulseed_buy_slot_money_txt[i], UIManager.manager.Money_Change(DataManager.manager.purchases.soulseed[i].price[0]) + "\\");
            TextUI(soulseed_buy_slot_seed_txt[i], DataManager.manager.purchases.soulseed[i].amount);
        }
        for (int i = 0; i < sp_buy_slots.Length; i++)
        {
            if (i == 0 && spbook_adloaded)
            {
                TextUI(sp_buy_slot_getsp_txt[i], DataManager.manager.purchases.change_sp[i].change);
                if (DataManager.manager.player_cloud_data.ad_remove)
                { TextUI(sp_buy_slot_seed_txt[i], DataManager.manager.ui.bank.GetFree()); }
                else
                { TextUI(sp_buy_slot_seed_txt[i], DataManager.manager.ui.bank.GetAd()); }
            }
            else
            {
                TextUI(sp_buy_slot_getsp_txt[i], DataManager.manager.purchases.change_sp[i].change);
                TextUI(sp_buy_slot_seed_txt[i], UIManager.manager.Money_Change(DataManager.manager.purchases.change_sp[i].soulseed));
            }
        }
        for (int i = 0; i < exchange_slots.Length; i++)
        {
            TextUI(exchange_slot_mote_txt[i], UIManager.manager.Money_Change(DataManager.manager.purchases.change_money[i].change));
            TextUI(exchange_slot_seed_txt[i], DataManager.manager.purchases.change_money[i].soulseed);
        }
        Check_Money();
    }

    // 광고 제거 버튼 클릭 시
    public void Touched_RemoveAd()
    {
        if (DataManager.manager.player_cloud_data.ad_remove)
        { remove_ad_anim.SetBool("touched", true); }
        else
        { RemoveAd_Active(); }
    }

    // 슬롯 별 영감 변경
    public void Change_SP(int code)
    {
        if (code == 0 && spbook_adloaded)
        { AskAdForSP(); }
        else
        {
            MsgBox_Controller.manager.MessageboxPanel_active("exchange_sp", GetChangeSPText(code), 1);
            if (GetSoulseed() >= DataManager.manager.purchases.change_sp[code].soulseed)
            {
                MsgBox_Controller.manager.SetMessageboxButtonText("exchange_sp", 0, DataManager.manager.ui.GetOK());
                MsgBox_Controller.manager.GetMessageboxButton("exchange_sp", 0).onClick.AddListener(() => ChangeSeedtoSP(code));
                MsgBox_Controller.manager.SetMessageboxButton("exchange_sp", 0);
            }
            else
            {
                MsgBox_Controller.manager.SetMessageboxButtonText("exchange_sp", 0, DataManager.manager.ui.GetOK());
                MsgBox_Controller.manager.SetMessageboxButton("exchange_sp", 0);
                MsgBox_Controller.manager.GetMessageboxButton("exchange_sp", 0).onClick.AddListener(() => AskBuySeed_active(code));
            }
            MsgBox_Controller.manager.SetMessageboxButtonText("exchange_sp", 1, DataManager.manager.ui.GetCancle());
            MsgBox_Controller.manager.SetMessageboxButton("exchange_sp", 1);
        }
    }

    // 영씨 -> SP 메시지박스 출력
    private string GetChangeSPText(int code)
    {
        return DataManager.manager.purchases.change_sp[code].GetMessage();
    }

    // 영씨 SP 변환
    private void ChangeSeedtoSP(int code)
    {
        now_exchanged_sp += DataManager.manager.purchases.change_sp[code].change;
        now_exchanged_seed += -DataManager.manager.purchases.change_sp[code].soulseed;

        MsgBox_Controller.manager.ChangeSuccess();
        BankInitialize();
    }

    // 광고 보고 SP변환
    private void ChangeSeedtoSP()
    {
        now_exchanged_sp += DataManager.manager.purchases.change_sp[0].change;

        MsgBox_Controller.manager.ChangeSuccess();
        BankInitialize();
    }

    // 슬롯 별 돈 변경 거시기
    public void Change_Mote(int code)
    {
        MsgBox_Controller.manager.MessageboxPanel_active("exchange_mote", GetChangeMoneyText(code), 1);
        if (GetSoulseed() >= DataManager.manager.purchases.change_money[code].soulseed)
        {
            MsgBox_Controller.manager.SetMessageboxButtonText("exchange_mote", 0, DataManager.manager.ui.GetOK());
            MsgBox_Controller.manager.GetMessageboxButton("exchange_mote", 0).onClick.AddListener(() => ChangeSeedtoMote(code));
            MsgBox_Controller.manager.SetMessageboxButton("exchange_mote", 0);
        }
        else
        {
            MsgBox_Controller.manager.SetMessageboxButtonText("exchange_mote", 0, DataManager.manager.ui.GetOK());
            MsgBox_Controller.manager.SetMessageboxButton("exchange_mote", 0);
            MsgBox_Controller.manager.GetMessageboxButton("exchange_mote", 0).onClick.AddListener(() => AskBuySeed_active(code));
        }
        MsgBox_Controller.manager.SetMessageboxButtonText("exchange_mote", 1, DataManager.manager.ui.GetCancle());
        MsgBox_Controller.manager.SetMessageboxButton("exchange_mote", 1);
    }

    // 영씨 -> 티끌 메시지박스 출력
    private string GetChangeMoneyText(int code)
    {
        return DataManager.manager.purchases.change_money[code].GetMessage();
    }

    // 영씨 티끌 변환
    private void ChangeSeedtoMote(int code)
    {
        now_exchanged_mote += DataManager.manager.purchases.change_money[code].change;
        now_exchanged_seed += -DataManager.manager.purchases.change_money[code].soulseed;

        MsgBox_Controller.manager.ChangeSuccess();
        BankInitialize();
    }

    // 영씨 구매 판넬 열기
    private void AskBuySeed_active(int index)
    {
        int set_index = index == 2 ? 2 : 0;

        ask_panel.SetActive(true);

        ImageUI(ask_img, soulseed_buy_slots[set_index].transform.Find("Image").GetComponent<Image>().sprite);
        TextUI(ask_txt, DataManager.manager.ui.messages.GetNotEnoughSoulseed() + "\n" +
        "'" + DataManager.manager.purchases.soulseed[set_index].GetName() + "'" + DataManager.manager.ui.bank.GetAsk());

        TextUI(ask_ok_btn_txt, DataManager.manager.ui.inventory.GetBuy());
        TextUI(ask_cancle_btn_txt, DataManager.manager.ui.GetCancle());

        ask_ok_btn.onClick.RemoveAllListeners();
        ask_ok_btn.onClick.AddListener(() => IAP_Manager.manager.Purchase(DataManager.manager.purchases.soulseed[index].id));
    }

    // 영씨 구매 판넬 닫기
    public void AskBuySeed_inactive()
    {
        ask_panel.SetActive(false);
    }

    // 광고보기 묻는 판넬
    public void AskAdForSP()
    {
        MsgBox_Controller.manager.SureToSeeAd("spbook");
    }

    // 구매 완료
    private void Purchased(string id)
    {
        switch (id)
        {
            case "remove_ad":
                bank_ad_icon.SetActive(false);
                Remove_Ad_Info_inactive();
                Hamster_Dance();
                BankInitialize();
                MsgBox_Controller.manager.PurchaseSuccess(DataManager.manager.purchases.remove_ad.GetName());
                break;
            default:
                string id_name = "";
                for (int i = 0; i < DataManager.manager.purchases.soulseed.Length; i++)
                {
                    if (id == DataManager.manager.purchases.soulseed[i].id)
                    { id_name = DataManager.manager.purchases.soulseed[i].GetName(); }
                }
                AskBuySeed_inactive();
                MsgBox_Controller.manager.PurchaseSuccess(id_name);
                break;
        }
    }

    #endregion

    #region Bank Description

    private Text bank_desc_title;               // 타이틀
    private Text bank_desc_description;         // 계좌 설명
    private Text bank_desc_btn_txt;             // 설명창 닫기버튼 텍스트

    // 계좌 설명창 이니셜라이즈
    private void InitializeBankDescription()
    {
        bank_desc_title = bank_description.transform.Find("Title").GetComponent<Text>();
        bank_desc_description = bank_description.transform.Find("Description").GetComponent<Text>();
        bank_desc_btn_txt = bank_description.transform.Find("Btn").Find("Text").GetComponent<Text>();
    }

    // 계좌 설명창 열기
    public void BankDescription_active()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        bank_account.SetActive(true);
        bank_description.SetActive(true);

        TextUI(bank_desc_title, DataManager.manager.ui.bank.GetAccount() + " " + DataManager.manager.ui.bank.GetBankAccount());
        TextUI(bank_desc_description, DataManager.manager.ui.bank.GetDescription());
        TextUI(bank_desc_btn_txt, DataManager.manager.ui.GetOK());
    }

    // 계좌 설명창 닫기
    public void BankDescription_inactive()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        bank_description.SetActive(false);
        bank_account.SetActive(false);
    }

    #endregion

    #region Account

    private Text bank_acc_title;                    // 타이틀
    private InputField bank_acc_amount_input;       // 인풋필드
    private Text bank_acc_amount_placeholder;       // 인풋필드 플레이스 홀더
    private Text bank_acc_txt;                      // 인풋필드 값
    private Text commission_txt;                    // 수수료 텍스트
    private Text whole_txt;                         // 총액 텍스트
    private Text no_commission_txt;                 // 수수료 제외 텍스트
    private Text no_commission_amount_txt;          // 수수료 제외 값 텍스트
    private GameObject check;                       // 체크박스 내 체크 이미지

    private Button yes_btn;                         // 입금 출금 버튼
    private Text yes_btn_txt;                       // 입금 출금 버튼 텍스트
    private Text no_btn_txt;                        // 취소 버튼 텍스트

    private bool save_or_withraw;                   // 입금 or 출금
    private int bank_amount;                        // 입금 할 양
    private int commission_amount;                  // 수수료 가격
    private int no_commission_soulseed;             // 수수료 제외하는데 필요한 영씨가격
    private bool no_commission;                     // 수수료 제외

    // 은행 예금통장 창 이니셜라이즈
    private void InitializeBankAccount()
    {
        bank_acc_title = bank_saving_withdraw.transform.Find("Title").GetComponent<Text>();
        bank_acc_amount_input = bank_saving_withdraw.transform.Find("Amount").GetComponent<InputField>();
        bank_acc_amount_placeholder = bank_acc_amount_input.transform.Find("Placeholder").GetComponent<Text>();
        bank_acc_txt = bank_acc_amount_input.transform.Find("Text").GetComponent<Text>();
        commission_txt = bank_saving_withdraw.transform.Find("Commission_txt").GetComponent<Text>();
        whole_txt = bank_saving_withdraw.transform.Find("Whole_txt").GetComponent<Text>();
        no_commission_txt = bank_saving_withdraw.transform.Find("Except_Commission_btn").Find("Text").GetComponent<Text>();
        no_commission_amount_txt = bank_saving_withdraw.transform.Find("Except_Commission_btn").Find("Exception_Amount").GetComponent<Text>();
        check = bank_saving_withdraw.transform.Find("Except_Commission_btn").Find("Checkbox").Find("Check").gameObject;

        yes_btn = bank_saving_withdraw.transform.Find("Btn_Yes").GetComponent<Button>();
        yes_btn_txt = bank_saving_withdraw.transform.Find("Btn_Yes").Find("Text").GetComponent<Text>();
        no_btn_txt = bank_saving_withdraw.transform.Find("Btn_No").Find("Text").GetComponent<Text>();

        bank_amount = 0;
        commission_amount = 0;
        no_commission_soulseed = 0;
        no_commission = false;
    }

    // 은행 예금통장
    public void Money_Savings(bool saving_or_withdraw)
    {
        BankAccount_active();

        TextUI(bank_acc_txt, null);
        save_or_withraw = saving_or_withdraw;

        bank_amount = 0;
        commission_amount = 0;
        no_commission_soulseed = 0;
        no_commission = false;

        if (saving_or_withdraw) { Saving_Money(); }
        else { Withdraw_Money(); }
    }

    // 수수료 제외 체크
    public void CheckNoCommission()
    {
        no_commission = !no_commission;
        check.SetActive(no_commission);

        if (no_commission)
        {
            if (save_or_withraw)
            {
                TextUI(commission_txt, DataManager.manager.ui.bank.GetCommission() + " : " + 0 + " (10%)");
                TextUI(whole_txt, DataManager.manager.ui.bank.GetWholeMote() + " : " + UIManager.manager.Money_Change(bank_amount));
            }
            // 출금 수수료
            else
            {
                TextUI(commission_txt, DataManager.manager.ui.bank.GetCommission() + " : " + 0 + " (15%)");
                TextUI(whole_txt, DataManager.manager.ui.bank.GetAccount() + " : " + UIManager.manager.Money_Change(DataManager.manager.bank_save.savings + (int)DataManager.manager.bank_save.interest - bank_amount));
            }
        }
        else
        { CalculateCommission(); }
    }

    // 예금
    public void Saving_Money()
    {
        save_or_withraw = true;
        TextUI(bank_acc_title, DataManager.manager.ui.bank.GetAccount() + " " + DataManager.manager.ui.bank.GetSaving());
        TextUI(bank_acc_amount_placeholder, DataManager.manager.ui.bank.GetInputfield());
        TextUI(no_commission_txt, DataManager.manager.ui.bank.GetNoCommission());
        TextUI(no_commission_amount_txt, no_commission_soulseed.ToString());

        TextUI(commission_txt, DataManager.manager.ui.bank.GetCommission() + " : " + UIManager.manager.Money_Change(commission_amount) + " (10%)");
        TextUI(whole_txt, DataManager.manager.ui.bank.GetWholeMote() + " : " + UIManager.manager.Money_Change(bank_amount));
        TextUI(yes_btn_txt, DataManager.manager.ui.bank.GetSaving());
        TextUI(no_btn_txt, DataManager.manager.ui.GetCancle());

        check.SetActive(no_commission);
    }

    // 출금
    public void Withdraw_Money()
    {
        save_or_withraw = false;
        TextUI(bank_acc_title, DataManager.manager.ui.bank.GetAccount() + " " + DataManager.manager.ui.bank.GetWithdraw());
        TextUI(bank_acc_amount_placeholder, DataManager.manager.ui.bank.GetInputfield());
        TextUI(no_commission_txt, DataManager.manager.ui.bank.GetNoCommission());
        TextUI(no_commission_amount_txt, no_commission_soulseed.ToString());

        TextUI(commission_txt, DataManager.manager.ui.bank.GetCommission() + " : " + UIManager.manager.Money_Change(commission_amount) + " (15%)");
        TextUI(whole_txt, DataManager.manager.ui.bank.GetAccount() + " : " + UIManager.manager.Money_Change(DataManager.manager.bank_save.savings + (int)DataManager.manager.bank_save.interest - (bank_amount + commission_amount)));
        // TextUI(whole_txt, DataManager.manager.ui.bank.GetWholeMote() + " : " + UIManager.manager.Money_Change(bank_amount - commission_amount));
        TextUI(yes_btn_txt, DataManager.manager.ui.bank.GetWithdraw());
        TextUI(no_btn_txt, DataManager.manager.ui.GetCancle());

        check.SetActive(no_commission);
    }

    // 입력 중 수수료 계산
    public void CalculateCommission_OnValue()
    {

    }

    // 수수료 계산
    public void CalculateCommission()
    {
        if (bank_acc_txt.text != null || bank_acc_txt.text != "")
        {
            try
            { bank_amount = int.Parse(bank_acc_txt.text); }
            catch
            {
                bank_amount = 0;
                TextUI(bank_acc_amount_input, bank_amount);
            }
        }
        // 입금 수수료
        if (save_or_withraw)
        {
            no_commission_soulseed = 1;
            if (GetMote() >= bank_amount)
            {
                commission_amount = (int)((bank_amount * 0.1f) * Data_Container.skill_container.efficiencySkill.efficiency_bankCommission);
                TextUI(commission_txt, DataManager.manager.ui.bank.GetCommission() + " : " + UIManager.manager.Money_Change(commission_amount) + " (10%)");
            }
            else
            {
                bank_amount = GetMote();
                TextUI(bank_acc_amount_input, bank_amount);
                commission_amount = (int)((bank_amount * 0.1f) * Data_Container.skill_container.efficiencySkill.efficiency_bankCommission);
                TextUI(commission_txt, DataManager.manager.ui.bank.GetCommission() + " : " + UIManager.manager.Money_Change(commission_amount) + " (10%)");
            }
            TextUI(whole_txt, DataManager.manager.ui.bank.GetWholeMote() + " : " + UIManager.manager.Money_Change(bank_amount - commission_amount));
        }
        // 출금 수수료
        else
        {
            no_commission_soulseed = 2;
            int whole_account = 0;
            if (DataManager.manager.bank_save.savings + DataManager.manager.bank_save.interest >= bank_amount)
            {
                commission_amount = (int)((bank_amount * 0.15f) * Data_Container.skill_container.efficiencySkill.efficiency_bankCommission);
                whole_account = DataManager.manager.bank_save.savings + (int)DataManager.manager.bank_save.interest - (bank_amount + commission_amount);
                TextUI(commission_txt, DataManager.manager.ui.bank.GetCommission() + " : " + UIManager.manager.Money_Change(commission_amount) + " (15%)");
            }
            else
            {
                whole_account = 0;
                bank_amount = DataManager.manager.bank_save.savings + (int)DataManager.manager.bank_save.interest;
                TextUI(bank_acc_amount_input, bank_amount);
                commission_amount = (int)((bank_amount * 0.15f) * Data_Container.skill_container.efficiencySkill.efficiency_bankCommission);
                TextUI(commission_txt, DataManager.manager.ui.bank.GetCommission() + " : " + UIManager.manager.Money_Change(commission_amount) + " (15%)");
            }
            TextUI(whole_txt, DataManager.manager.ui.bank.GetAccount() + " : " + UIManager.manager.Money_Change(whole_account));
        }

        // 필요 영씨
        no_commission_soulseed = bank_amount / 50000 < 1 ? 1 : bank_amount / 50000;
        TextUI(no_commission_amount_txt, no_commission_soulseed);
    }

    // 입금 혹은 출금하기
    public void Bank_Check()
    {
        if (save_or_withraw)
        { Bank_Save(); }
        else
        { Bank_Withdraw(); }
    }

    // 입금하기
    private void Bank_Save()
    {
        bool quitable = true;
        // 수수료 제외 선택했을 시
        if (no_commission)
        {
            if (GetSoulseed() >= no_commission_soulseed)
            {
                // 영씨 사용
                now_used_seed -= no_commission_soulseed;
                commission_amount = 0;
            }
            else
            {
                // 사용 못함
                MsgBox_Controller.manager.NoSoulseedMsg();
                quitable = false;
            }
        }
        if (quitable)
        {
            DataManager.manager.bank_save.savings += bank_amount - commission_amount;
            now_saved_mote -= bank_amount;
            BankAccount_inactive();
        }
    }

    // 출금하기
    private void Bank_Withdraw()
    {
        bool quitable = true;
        // 수수료 제외 선택했을 시
        if (no_commission)
        {
            if (GetSoulseed() >= no_commission_soulseed)
            {
                // 영씨 사용
                now_used_seed -= no_commission_soulseed;
                commission_amount = 0;
            }
            else
            {
                // 사용 못함
                MsgBox_Controller.manager.NoSoulseedMsg();
                quitable = false;
            }
        }
        if (quitable)
        {
            if (DataManager.manager.bank_save.interest - (bank_amount + commission_amount) >= 0)
            {
                DataManager.manager.bank_save.interest -= bank_amount + commission_amount;
            }
            else
            {
                int withdrawable_mote = (int)(DataManager.manager.bank_save.savings + DataManager.manager.bank_save.interest - (bank_amount + commission_amount));
                if (withdrawable_mote >= 0)
                {
                    int left_mote = (int)DataManager.manager.bank_save.interest - (bank_amount + commission_amount);
                    DataManager.manager.bank_save.interest = 0;
                    DataManager.manager.bank_save.savings += left_mote;

                    now_saved_mote += bank_amount;
                    BankAccount_inactive();
                }
                else
                {
                    int left_mote = (int)DataManager.manager.bank_save.interest - (bank_amount + commission_amount);
                    left_mote = DataManager.manager.bank_save.savings + left_mote;
                    DataManager.manager.bank_save.interest = 0;
                    DataManager.manager.bank_save.savings = 0;

                    now_saved_mote += bank_amount + left_mote;
                    BankAccount_inactive();
                }
            }
        }
    }

    // 입출금창 열기
    private void BankAccount_active()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        bank_amount = 0;
        commission_amount = 0;
        no_commission_soulseed = 0;
        no_commission = false;
        TextUI(bank_acc_amount_input, "");
        bank_account.SetActive(true);
        bank_saving_withdraw.SetActive(true);
    }

    // 입출금창 닫기
    public void BankAccount_inactive()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        BankInitialize();
        bank_saving_withdraw.SetActive(false);
        bank_account.SetActive(false);
    }

    #endregion

    #region Remove Ad

    public Animator remove_ad_info_anim;        // 광고제거 판넬 애니메이션
    private bool remove_ad_only_active;         // 광제 온리 액티브인지 체크
    public GameObject remove_ad_info;           // 광고제거 구성품 보기
    public Text[] slot_txt;                     // 슬롯 텍스트
    public Text sure_to_buy;                    // 진짜 구매할건지 묻는 텍스트
    public Text[] info_button_txt;              // 버튼 텍스트

    public Image remove_ad_givegift;            // 광고제거 선물 증정 이미지
    public Image remove_ad_sign;                // 광고제거 사인 이미지
    public Image remove_ad_benefit;             // 광고제거 베네핏 이미지

    public Sprite[] sprite_givegift;            // 광고제거 선물 증정 스프라이트
    public Sprite[] sprite_sign;                // 광고제거 사인 스프라이트
    public Sprite[] sprite_benefit;             // 광고제거 베네핏 스프라이트

    // 광고 제거'만' 액티브
    public void Remove_Ad_Only_Active()
    {
        remove_ad_only_active = true;
        UI_Open();
        panel_object.SetActive(true);
        bank.SetActive(false);
        Remove_Ad_Active();
        BankInitialize();
    }

    // 광고 제거 액티브
    private void RemoveAd_Active()
    {
        GameManager.manager.GetSoundManager().SoundEffect();
        Remove_Ad_Active();
    }

    // 광고 제거 액티브 시스템
    private void Remove_Ad_Active()
    {
        remove_ad.SetActive(true);
        remove_ad_info.SetActive(false);
        remove_ad_givegift.sprite = sprite_givegift[(int)GameManager.manager.system_language];
        remove_ad_sign.sprite = sprite_sign[(int)GameManager.manager.system_language];
        remove_ad_benefit.sprite = sprite_benefit[(int)GameManager.manager.system_language];
    }

    // 햄스터 댄스
    public void Hamster_Dance()
    {
        remove_ad_info_anim.SetBool("touched", true);
    }

    // 광고제거 info 열기
    public void Remove_Ad_Info_active()
    {
        if (DataManager.manager.player_cloud_data.ad_remove) { return; }

        GameManager.manager.GetSoundManager().SoundEffect();
        remove_ad_info.SetActive(true);
        string[] info_ask = DataManager.manager.purchases.remove_ad.GetAsk().Split('\n');

        for (int i = 0; i < slot_txt.Length; i++)
        {
            TextUI(slot_txt[i], DataManager.manager.purchases.remove_ad.GetRewardTxt().arr[i]);
        }
        TextUI(sure_to_buy, DataManager.manager.purchases.remove_ad.GetName() + info_ask[0] + "\n" + DataManager.manager.purchases.remove_ad.GetPrice() + info_ask[1]);
        TextUI(info_button_txt[0], DataManager.manager.ui.inventory.GetBuy());
        TextUI(info_button_txt[1], DataManager.manager.ui.GetCancle());
    }

    // 광고제거 info 닫기
    public void Remove_Ad_Info_inactive()
    {
        remove_ad_info.SetActive(false);
    }

    // 광고 제거 인액티브
    public void RemoveAd_inactive()
    {
        remove_ad_info_anim.SetBool("touched", false);
        remove_ad.SetActive(false);

        if (remove_ad_only_active)
        {
            BankPanel_inactive();
        }
    }

    #endregion

    #region Lottery

    private int LOTTERY_AMOUNT = 7;
    private int LOTTERY_PRICE = 100;
    private int daily_lottery;

    public Sprite mote_img;
    public Sprite seed_img;

    public GameObject lottery;                  // 복권
    private GameObject[] lottery_slot;          // 복권 슬롯 (연출용)

    public GameObject lottery_result;           // 복권 당첨내역 알림용
    private GameObject lottery_result_backlight;// 후광
    private GameObject lottery_result_txt;      // 물품 획득 텍스트
    private GameObject lottery_result_img;      // 물품 획득 이미지
    private GameObject lottery_mote_txt;        // 복권 티끌 획득 텍스트
    private Text lottery_result_btn_txt;        // 물품 획득 버튼 텍스트

    private Vector3 title_pos;                  // 타이틀 포지션
    private Vector3 item_pos;                   // 이미지 아이템 포지션
    private Vector3 mote_pos;                   // 이미지 티끌 포지션

    private float picked_probability;           // Roll
    private float now_probability;              // 당첨 확률 설정용

    private int used_mote_for_lottery;          // 복권에 쓴 돈
    private int picked_lottery;                 // 당첨 복권
    private List<int> lottery_saves;            // 당첨된 복권 인덱스 저장용

    private int lottery_mote, lottery_seed;     // 복권 당첨된 티끌, 영씨

    // 복권 UI 이니셜라이즈
    private void InitializeLottery()
    {
        lottery_slot = new GameObject[LOTTERY_AMOUNT];
        for (int i = 0; i < lottery_slot.Length; i++)
        { lottery_slot[i] = lottery.transform.Find("kuji_" + i).gameObject; }
        lottery_saves = new List<int>();

        lottery_result_backlight = lottery_result.transform.Find("Backlight").gameObject;
        lottery_result_txt = lottery_result.transform.Find("Text").gameObject;
        lottery_result_img = lottery_result.transform.Find("Image").gameObject;
        lottery_mote_txt = lottery_result.transform.Find("Mote_txt").gameObject;
        lottery_result_btn_txt = lottery_result.transform.Find("Button").Find("Text").GetComponent<Text>();

        title_pos = new Vector3(0, 230, 0);
        item_pos = new Vector3(0, 20, 0);
        mote_pos = new Vector3(-135, 20, 0);

        lottery_mote = 0;
        lottery_seed = 0;
        lottery_result.SetActive(false);
        bank_new.SetActive(true);
    }

    // 복권 초기화
    public void LotteryInitialize()
    {
        daily_lottery = LOTTERY_AMOUNT;
        for (int i = 0; i < lottery_slot.Length; i++)
        { lottery_slot[i].SetActive(true); }

        lottery_saves.Clear();

        picked_probability = 0;
        now_probability = 0;
        picked_lottery = 0;
        used_mote_for_lottery = 0;
        bank_new.SetActive(true);
    }

    // 복권 뽑기
    public void Lottery()
    {
        if (daily_lottery <= 0) return;

        if (GetMote() < LOTTERY_PRICE)
        {
            MsgBox_Controller.manager.NomoneyMsg();
        }

        else
        {
            GameManager.manager.GetSoundManager().LotteryTouch();
            daily_lottery -= 1;
            lottery_slot[daily_lottery].SetActive(false);
            used_mote_for_lottery -= LOTTERY_PRICE;

            picked_probability = Random.Range(0.0f, 1.0f);
            now_probability = 0;
            picked_lottery = 0;
            for (int i = 0; i < DataManager.manager.lottery.lottery.Length; i++)
            {
                if (now_probability <= picked_probability && picked_probability < now_probability + DataManager.manager.lottery.lottery[i].probability)
                { picked_lottery = i; break; }
                else
                { now_probability += DataManager.manager.lottery.lottery[i].probability; }
            }
            Lottery_Result(picked_lottery);
        }
    }

    // 복권 결과 출력
    private void Lottery_Result(int index)
    {
        if (index > 7)
        { GameManager.manager.GetSoundManager().LotteryWin(); }

        TextUI(lottery_result_btn_txt, DataManager.manager.ui.GetOK());
        TextUI(lottery_result_txt, DataManager.manager.lottery.lottery[index].GetName());

        switch (DataManager.manager.lottery.lottery[index].effect_type)
        {
            case "mote":
                lottery_result_txt.SetActive(true);
                lottery_result_img.SetActive(true);
                lottery_mote_txt.SetActive(true);
                lottery_result_txt.GetComponent<RectTransform>().anchoredPosition = title_pos;
                lottery_result_img.GetComponent<RectTransform>().anchoredPosition = mote_pos;
                lottery_mote += DataManager.manager.lottery.lottery[index].effect;
                TextUI(lottery_mote_txt, DataManager.manager.lottery.lottery[index].effect);
                ImageUI(lottery_result_img, mote_img);
                break;
            case "soulseed":
                lottery_result_txt.SetActive(true);
                lottery_result_img.SetActive(true);
                lottery_mote_txt.SetActive(true);
                lottery_result_txt.GetComponent<RectTransform>().anchoredPosition = title_pos;
                lottery_result_img.GetComponent<RectTransform>().anchoredPosition = mote_pos;
                lottery_seed += DataManager.manager.lottery.lottery[index].effect;
                TextUI(lottery_mote_txt, DataManager.manager.lottery.lottery[index].effect);
                ImageUI(lottery_result_img, seed_img);
                break;
            case "mood":
                lottery_result_txt.SetActive(true);
                lottery_result_img.SetActive(false);
                lottery_mote_txt.SetActive(false);
                lottery_result_txt.GetComponent<RectTransform>().anchoredPosition = item_pos;
                break;
            case "item":
                lottery_result_txt.SetActive(true);
                lottery_result_img.SetActive(true);
                lottery_mote_txt.SetActive(false);
                lottery_result_txt.GetComponent<RectTransform>().anchoredPosition = title_pos;
                lottery_result_img.GetComponent<RectTransform>().anchoredPosition = item_pos;
                ImageUI(lottery_result_img, UIManager.manager.inventory.GetItemSprite(DataManager.manager.lottery.lottery[index].effect));
                UIManager.manager.NewTokenSet(1, true);
                break;
        }

        lottery_result.SetActive(true);
        StartCoroutine(Backlight_Rotate());

        lottery_saves.Add(index);
    }

    // 후광 연출부(돌아가기)
    private IEnumerator Backlight_Rotate()
    {
        float rotate_amount = 0;
        while (lottery_result.activeInHierarchy)
        {
            lottery_result_backlight.transform.Rotate(new Vector3(0, 0, rotate_amount));
            rotate_amount = 5f * Time.unscaledDeltaTime;
            yield return null;
        }
    }

    // 복권 결과창 닫기
    public void Lottery_Result_Quit()
    {
        lottery_result.SetActive(false);
        BankInitialize();
    }

    #endregion

    // 은행 열기
    public void BankPanel_active()
    {
        remove_ad_only_active = false;
        Now_Bank_Thing_Init();
        UI_Open();
        panel_object.SetActive(true);
        bank.SetActive(true);
        remove_ad.SetActive(false);
        bank_account.SetActive(false);
        lottery_result.SetActive(false);
        BankInitialize();
    }

    // 은행 닫기
    public void BankPanel_inactive()
    {
        remove_ad.SetActive(false);
        bank_account.SetActive(false);
        panel_object.SetActive(false);
        lottery_result.SetActive(false);
        bank_new.SetActive(false);
        bank_mote(now_saved_mote + used_mote_for_lottery + now_exchanged_mote);
        bank_sp(now_exchanged_sp);
        bank_soulseed(now_used_seed + now_exchanged_seed);
        lottery_set(lottery_saves);
        Now_Bank_Thing_Init();
        UI_Close();
    }

    #region Delegates

    public delegate int MoneyCheck();
    public static event MoneyCheck get_soulseed;
    public static event MoneyCheck get_mote;

    public delegate void BankAccount(int amount);
    public static event BankAccount bank_mote;
    public static event BankAccount bank_sp;
    public static event BankAccount bank_soulseed;

    public delegate void LotterySet(List<int> lottery_code);
    public static event LotterySet lottery_set;

    private void OnEnable()
    {
        GameManager.game_init += InitializeBankPanel;
        IAP_Manager.purchased += Purchased;
        Admob_Manager.spbook_reward += ChangeSeedtoSP;
    }
    private void OnDisable()
    {
        GameManager.game_init -= InitializeBankPanel;
        IAP_Manager.purchased -= Purchased;
        Admob_Manager.spbook_reward -= ChangeSeedtoSP;
    }

    #endregion
}
