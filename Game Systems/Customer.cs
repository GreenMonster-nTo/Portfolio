using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Customer_Info
{
    public int type;
    public int code;
    public int money;
    public Vector2 position;
    public float speed;
}

public class Customer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private int c_index;                        // 구분자
    private int c_type;                         // 타입
    private int c_code;                         // 코드
    private int c_money;                        // 소지금

    private bool active;                        // 액티브 여부

    public GameObject c_obj;                    // 게임 오브젝트
    private Vector2 end_pos;                    // 도착 포지션
    private float stop_pos;                     // 멈출 포지션 (x position)
    private int c_state;                        // 손님 상태 (0:멈춤, 1:이동, 2:도망)
    private bool c_bought;                      // 구매 여부
    private bool c_stop;                        // 멈춰서 고민할 지 여부
    private bool c_stopped;                     // 멈췄었음
    private bool c_run;                         // 도망갈지 여부

    public SpriteRenderer c_renderer;           // 스프라이트 렌더러
    public BoxCollider2D c_collider;            // 충돌처리용 콜리더
    public SoundSystem c_sound;                 // 사운드
    public Touch_Scale c_touchscale;            // 터치스케일
    private Vector3 basic_scale;                // 원래크기
    private Vector3 long_scale;                 // 장지갑 크기

    private float move_speed;                   // 손님 기본 이동속도
    private float run_speed;                    // 손님 도망 이동속도

    public GameObject[] p_objs;                 // 구매 물품 게임오브젝트
    public SpriteRenderer[] p_renderers;        // 구매 물품 스프라이트 렌더러

    public GameObject c_canvas;                 // 손님 말풍선 및 소지금 표시용 캔버스
    public GameObject canvas_space;             // 손님 표시용 캔버스 스페이스
    public Text notice_txt;                     // 손님 소지금 표시용 텍스트
    public GameObject nobuy_obj;                // 손님 미구매 표시용 말풍선
    public Text nobuy_txt;                      // 손님 미구매 표시용 텍스트
    public GameObject arrow_obj;                // 선택 화살표 오브젝트
    public GameObject forcefulbuy_effect;       // 강제구매 이펙트 오브젝트
    public Effect_Customer_ForcefulBuy forceful_buy_script;

    private Vector3 basic_pos;                  // 손님 표시용 캔버스 스페이스 일반 지갑들 위치
    private Vector3 long_pos;                   // 손님 표시용 캔버스 스페이스 장지갑 위치

    private Coroutine move_coroutine;           // 움직임 처리 코루틴
    private Coroutine anim_coroutine;           // 애니메이션 처리 코루틴

    private int sorting_order;                  // 손님 정렬용 변수
    private Sprite[] move_sprites;              // 움직임 애니메이션 스프라이트
    private Sprite[] stop_sprites;              // 멈춤(고민) 애니메이션 스프라이트
    private Sprite bought_stop;                 // 구매 후 멈춤 스프라이트
    private Sprite[] bought_sprites;            // 구매 후 움직임 애니메이션 스프라이트
    private Sprite beaten_stop;                 // 맞은 지갑 멈춤 스프라이트
    private Sprite[] beaten_sprites;            // 맞은 지갑 움직임 애니메이션 스프라이트
    private Color visible_color;                // 보이게 하기
    private Color invisible_color;              // 안 보이게 하기

    private WaitForSeconds stop_delay;          // 멈춤 애니메이션 설정용 딜레이
    private WaitForSeconds move_delay;          // 움직임 애니메이션 설정용 딜레이
    private WaitForSeconds run_delay;           // 도망 애니메이션 설정용 딜레이

    private WaitForSeconds product_delay;       // 물품 애니메이션 설정용 딜레이
    private WaitForSeconds think_to_buy;        // 구매 고민시간 타이머
    private WaitForSeconds wait_for_buy;        // 구매 대기시간 타이머

    private bool beaten;                        // 맞았는지 체크
    private bool noticed;                       // 눈썰미 썼는지 체크

    private bool pickable;                      // 선택 가능한지

    #region Activations

    // 완전 초기화 (1회 시행)
    public void CustomerInitialize(int index)
    {
        c_index = index;
        c_run = false;
        c_state = 0;
        run_speed = 0.5f;
        active = false;
        c_sound.sound_where = "customer_" + index;
        basic_scale = new Vector3(0.32f, 0.32f, 0.32f);
        long_scale = new Vector3(0.35f, 0.35f, 0.35f);
        end_pos = Vector2.zero;
        basic_pos = new Vector3(0, 4.27f, 0);
        long_pos = new Vector3(0, 2.93f, 0);
        move_sprites = new Sprite[2];
        stop_sprites = new Sprite[2];
        bought_stop = null;
        bought_sprites = new Sprite[2];
        beaten_stop = null;
        beaten_sprites = new Sprite[2];
        visible_color = new Color(1, 1, 1, 1);
        invisible_color = new Color(1, 1, 1, 0);
        stop_delay = new WaitForSeconds(0.5f);
        run_delay = new WaitForSeconds(0.02f);
        product_delay = new WaitForSeconds(0.2f);
        think_to_buy = new WaitForSeconds(1.5f);
        wait_for_buy = new WaitForSeconds(0.5f);
        beaten = false;
        noticed = false;

        forceful_buy_script.Customer_Forcefulbuy_Init(index);
    }

    public void CustomerActive()
    {
        active = true;
        c_obj.SetActive(active);
        c_canvas.SetActive(active);
        canvas_space.SetActive(active);
        notice_txt.gameObject.SetActive(!active);
        nobuy_obj.SetActive(!active);

        c_bought = !active;
        c_stopped = !active;
        c_run = !active;
        beaten = !active;
        noticed = !active;
        pickable = !active;

        c_touchscale.SetTouchable(active);

        c_state = 1;
        SetCustomerSprite();
        move_coroutine = StartCoroutine(Move());
    }

    public void CustomerInactive()
    {
        active = false;
        nobuy_obj.SetActive(active);
        notice_txt.gameObject.SetActive(active);
        canvas_space.SetActive(active);
        c_canvas.SetActive(active);
        c_obj.SetActive(active);

        for (int i = 0; i < p_objs.Length; i++)
        {
            p_renderers[i].color = visible_color;
            p_renderers[i].sprite = null;
            p_objs[i].SetActive(active);
        }

        c_touchscale.SetTouchable(active);

        c_state = 0;
        StopAllCoroutines();
    }

    #endregion

    #region Info Setter

    public void CustomerInfoSet(Customer_Info info)
    {
        CustomerTypeSet(info.type, info.code, info.money);
        CustomerPosSet(info.position, info.speed);
    }

    // 손님 타입, 코드 설정
    public void CustomerTypeSet(int type, int code, int money)
    {
        if (type == 2)
        {
            c_obj.transform.localScale = long_scale;
            canvas_space.transform.localPosition = long_pos;
        }
        else
        {
            c_obj.transform.localScale = basic_scale;
            canvas_space.transform.localPosition = basic_pos;
        }
        c_type = type;
        c_code = code;
        c_money = money;
    }

    // 손님 포지션 설정
    public void CustomerPosSet(Vector2 start_pos, float speed)
    {
        c_obj.transform.localPosition = start_pos;
        end_pos.x = -start_pos.x;
        end_pos.y = start_pos.y;
        move_speed = speed;
        move_delay = new WaitForSeconds(0.01f / move_speed);
        c_renderer.flipX = start_pos.x < 0 ? true : false;
    }

    // 손님 레이어 설정
    public void CustomerLayerSet(int layer)
    {
        c_renderer.sortingOrder = layer;
        sorting_order = layer;
    }

    // 손님 스프라이트 설정
    public void CustomerSpriteSet(Sprite[] walk, Sprite[] stop, Sprite[] boughtWalk, Sprite boughtStop, Sprite[] beatenWalk, Sprite beatenStop)
    {
        move_sprites = walk;
        stop_sprites = stop;
        bought_sprites = boughtWalk;
        bought_stop = boughtStop;
        beaten_sprites = beatenWalk;
        beaten_stop = beatenStop;
    }

    // 손님 물품구매 여부 설정
    public void CustomerStopSet(bool stop)
    {
        c_stop = stop;
    }

    // 손님 상태 설정
    public void CustomerStateSet(int state)
    {
        c_state = state;
        if (state == 2)
        { c_run = true; }
    }

    #endregion

    #region Info Get

    // 생성 여부
    public bool NowActive() => active;

    // 포지션 받아오기
    public Vector2 GetPosition() => c_obj.transform.localPosition;

    // 포지션 y 받아오기
    public float GetEndPos() => end_pos.y;

    // 타입
    public int GetCType() => c_type;

    // 코드
    public int GetCCode() => c_code;

    // 돈
    public int GetMoney() => c_money;

    // 팰 수 있는지
    public bool GetBeatable() => !beaten;

    // 눈썰미 사용가능한지
    public bool GetCanNotice() => !noticed;

    // 멈춤 여부 받아오기
    public bool GetStopable() => c_stop;

    #endregion

    #region Movements

    // 화면 밖으로 나갔는지 체크
    private bool IsOutside()
    {
        if ((end_pos.x > 0 && c_obj.transform.localPosition.x >= end_pos.x) || (end_pos.x < 0 && c_obj.transform.localPosition.x <= end_pos.x))
        { return true; }
        return false;
    }

    // 화면 중앙인지 체크
    private bool IsMiddle()
    {
        if (c_obj.transform.localPosition.x >= stop_pos - 0.1f && c_obj.transform.localPosition.x <= stop_pos + 0.1f)
        { return true; }
        return false;
    }

    // 멈춤 처리 함수
    private void Stop()
    {
        c_state = 0;
        c_stopped = true;
        SetCustomerSprite();
        StartCoroutine(ThinkToBuy());
    }

    // 움직임 처리 함수
    private IEnumerator Move()
    {
        while (active)
        {
            if (c_state == 1)
            { c_obj.transform.localPosition = Vector2.MoveTowards(c_obj.transform.localPosition, end_pos, move_speed * Time.timeScale); }
            else if (c_state == 2)
            { c_obj.transform.localPosition = Vector2.MoveTowards(c_obj.transform.localPosition, end_pos, run_speed * Time.timeScale); }

            if (IsOutside())
            { CustomerInactive(); }
            if (c_stop && !c_stopped && IsMiddle() && c_state != 0 && !c_run)
            { Stop(); }
            yield return null;
        }
    }

    #endregion

    #region Sprite And Animation

    // 손님 스프라이트 처리
    private void SetCustomerSprite()
    {
        if (anim_coroutine != null)
        { StopCoroutine(anim_coroutine); }

        if (beaten)
        {
            switch (c_state)
            {
                case 0:
                    c_renderer.sprite = beaten_stop;
                    break;
                case 2:
                    anim_coroutine = StartCoroutine(Animation(run_delay, beaten_sprites));
                    break;
            }
        }
        else
        {
            switch (c_state)
            {
                case 0:
                    if (c_bought)
                    { c_renderer.sprite = bought_stop; }
                    else
                    { anim_coroutine = StartCoroutine(Animation(stop_delay, stop_sprites)); }
                    break;
                case 1:
                    if (c_bought)
                    { anim_coroutine = StartCoroutine(Animation(move_delay, bought_sprites)); }
                    else
                    { anim_coroutine = StartCoroutine(Animation(move_delay, move_sprites)); }
                    break;
                case 2:
                    if (c_bought)
                    { anim_coroutine = StartCoroutine(Animation(run_delay, bought_sprites)); }
                    else
                    { anim_coroutine = StartCoroutine(Animation(run_delay, move_sprites)); }
                    break;
            }
        }
    }

    int c_frame;            // 애니메이션 프레임 제어용
    // 손님 애니메이션
    private IEnumerator Animation(WaitForSeconds delay, Sprite[] sprites)
    {
        c_frame = 0;
        while (active)
        {
            if (c_frame == sprites.Length) { c_frame = 0; }
            c_renderer.sprite = sprites[c_frame++];
            yield return delay;
        }
    }

    #endregion

    #region Buy Logic

    public delegate void Customer_Del(int index);
    public static event Customer_Del stopped;

    // 물품 구매 고민로직
    private IEnumerator ThinkToBuy()
    {
        yield return think_to_buy;
        stopped(c_index);
    }

    // 손님 소지금 처리
    public void MoneyUse(int price)
    {
        c_money -= price;
        notice_txt.text = c_money.ToString();
    }

    // 구매 처리부
    public void BuyProduct(int price, Sprite[] product, int product_amount)
    {
        c_bought = true;
        MoneyUse(price);
        p_objs[product_amount].SetActive(true);
        if (product.Length > 1)
        { StartCoroutine(PAnimation(product, p_renderers[product_amount])); }
        else
        { p_renderers[product_amount].sprite = product[0]; }
        StartCoroutine(AfterThinking());
    }

    // 구매포기
    public void NotBuyProduct(string say)
    {
        c_bought = false;
        nobuy_obj.SetActive(true);
        nobuy_txt.text = say;
        StartCoroutine(AfterThinking());
    }

    // 구매 물품 애니메이션
    private IEnumerator PAnimation(Sprite[] buy_sprites, SpriteRenderer renderer)
    {
        int frame = 0;
        while (active)
        {
            if (frame == buy_sprites.Length) { frame = 0; }
            renderer.sprite = buy_sprites[frame++];
            yield return product_delay;
        }
    }

    // 고민 이후 처리부
    private IEnumerator AfterThinking()
    {
        SetCustomerSprite();
        yield return wait_for_buy;
        nobuy_obj.SetActive(false);
        c_state = 1;
        SetCustomerSprite();
    }

    #endregion

    #region Skill Setter

    public static event Customer_Del picked;

    // 스킬 온오프 체크 
    public void Skill_Check(bool active)
    {
        if (active)
        { c_state = 0; }
        else
        { c_state = !c_run ? 1 : 2; }
    }

    // 선택가능 설정
    public void PickableSet(bool choose)
    {
        pickable = choose;
        if (pickable)
        {
            arrow_obj.SetActive(true);
            c_renderer.sortingLayerID = SortingLayer.NameToID("Pickable");
        }
        else
        {
            arrow_obj.SetActive(false);
            c_renderer.sortingLayerID = SortingLayer.NameToID("Default");
        }
        SetCustomerSprite();
    }

    // 선택함
    private void Picked()
    {
        picked(c_index);
    }

    // 피버타임 일시정지, 재실행
    public void Fevertime(bool fevertime)
    {
        if (fevertime)
        {
            c_renderer.color = invisible_color;
            for (int i = 0; i < p_renderers.Length; i++)
            { p_renderers[i].color = invisible_color; }
        }
        else
        {
            c_renderer.color = visible_color;
            for (int i = 0; i < p_renderers.Length; i++)
            { p_renderers[i].color = visible_color; }

        }
    }

    // 맞기
    public void Beat()
    {
        StartCoroutine(Beaten());
    }

    // 맞았을 경우 설정
    private IEnumerator Beaten()
    {
        nobuy_obj.SetActive(false);
        beaten = true;
        c_state = 0;
        SetCustomerSprite();
        yield return new WaitForSeconds(1.0f);
        c_run = true;
        c_state = 2;
        SetCustomerSprite();
    }

    // 소지금 표시
    public void Notice()
    {
        noticed = true;
        notice_txt.gameObject.SetActive(true);
        notice_txt.text = c_money.ToString();
    }

    // 강제 구매
    public void ForcefulBuy()
    {
        nobuy_obj.SetActive(false);
        c_state = 0;
        c_stopped = true;
        SetCustomerSprite();
        forcefulbuy_effect.SetActive(true);
    }

    #endregion

    // 터치
    public void OnPointerDown(PointerEventData eventData)
    {
        if (pickable)
        { Picked(); }
    }

    // 터치 뗐을 때
    public void OnPointerUp(PointerEventData eventData)
    {}
}
