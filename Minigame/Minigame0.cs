using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 양말 찾기
public class Minigame0 : Minigame, IMinigame
{
    public GameObject[] collect_pos;            // 찾은 양말 진열 포지션
    private SpriteRenderer[] collect_renderer;  // 찾은 양말 진열용 스프라이트 렌더러

    public GameObject[] socks_dragable;         // 회색 양말
    public GameObject[] socks_target;           // 타겟 양말
    private Minigame0_Socks[] target_scripts;   // 타겟 양말 스크립트
    private Minigame0_Socks[] dragable_scripts; // 논타겟 양말 스크립트

    public Sprite[] socks_color_sprite;         // 컬러 양말 스프라이트
    public Sprite[] socks_mono_sprite;          // 회색 양말 스프라이트
    public Sprite[] socks_silhouette_sprite;    // 양말 실루엣 스프라이트

    private bool[] socks_touchable;             // 터치 가능한지
    private bool[] socks_touched;               // 터치 되었는지

    private int socks_collected;                // 모은 양말 개수

    #region Initialize

    // 초기화
    public void MinigameInitialize()
    {
        DelegateSet();

        socks_touchable = new bool[socks_dragable.Length];
        socks_touched = new bool[socks_dragable.Length];

        collect_renderer = new SpriteRenderer[collect_pos.Length];
        for (int i = 0; i < collect_pos.Length; i++)
        { collect_renderer[i] = collect_pos[i].GetComponent<SpriteRenderer>(); }

        target_scripts = new Minigame0_Socks[socks_target.Length];
        dragable_scripts = new Minigame0_Socks[socks_dragable.Length];
        for (int i = 0; i < socks_target.Length; i++)
        { target_scripts[i] = socks_target[i].GetComponent<Minigame0_Socks>(); }
        for (int i = 0; i < socks_dragable.Length; i++)
        { dragable_scripts[i] = socks_dragable[i].GetComponent<Minigame0_Socks>(); }

        for (int i = 0; i < socks_target.Length; i++)
        { target_scripts[i].SocksInitialize(i, false, collect_pos[i].transform); }
        for (int i = 0; i < socks_dragable.Length; i++)
        { dragable_scripts[i].SocksInitialize(i, true, null); }
    }

    // 미니게임 액티브 되었을 때 (지속적인 초기화)
    public void MinigameActive()
    {
        socks_collected = 0;
        SocksPositionSet();
        SocksTouchableInit();
        SocksTouchedInit();
    }

    // 양말 터치 가능 여부 초기화
    private void SocksTouchableInit()
    {
        for (int i = 0; i < socks_touchable.Length; i++)
        {
            SocksTouchableSet(i);
        }
    }

    // 양말 터치되었는지 여부 초기화
    private void SocksTouchedInit()
    {
        for (int i = 0; i < socks_touched.Length; i++)
        {
            socks_touched[i] = false;
        }
    }

    // 델리게이트 해제
    public void MinigameInactive()
    {
        DelegateDel();
    }

    #endregion

    #region Game Logic

    int sprite_code;                            // 양말 스프라이트 코드 설정용
    Vector2 temp_vec;                           // 임시 벡터
    float temp_x;                               // 임시 벡터 x
    float temp_y;                               // 임시 벡터 y
    int rand;                                   // 랜덤 세팅용 int
    int count;                                  // 카운트 처리용 int

    // 양말 위치 초기화
    private void SocksPositionSet()
    {
        rand = 0;
        sprite_code = 0;
        temp_vec = Vector2.zero;

        for (int i = 0; i < socks_target.Length; i++)
        {
            temp_x = Random.Range(-1.8f, 1.8f);
            temp_y = Random.Range(-1.9f, -0.7f);
            temp_vec.x = temp_x;
            temp_vec.y = temp_y;
            socks_target[i].transform.localPosition = temp_vec;

            sprite_code = SocksSpriteCodeRandomSet(true);
            target_scripts[i].SocksActive(socks_color_sprite[sprite_code]);
            collect_renderer[i].sprite = socks_silhouette_sprite[(sprite_code / 3) > 4 ? 4 : (sprite_code / 3)];
        }
        // 드래그 가능 양말 포지션 설정
        for (int i = 0; i < socks_dragable.Length; i++)
        {
            rand = Random.Range(0, socks_target.Length);

            temp_x = socks_target[rand].transform.localPosition.x + Random.Range(-1.5f, 1.5f);
            temp_y = socks_target[rand].transform.position.y + Random.Range(-1.5f, 1.5f);
            temp_vec.x = temp_x;
            temp_vec.y = temp_y;
            socks_dragable[i].transform.localPosition = temp_vec;

            sprite_code = SocksSpriteCodeRandomSet(false);
            dragable_scripts[i].SocksActive(socks_mono_sprite[sprite_code]);
        }
    }

    // 양말 스프라이트 랜덤 설정
    private int SocksSpriteCodeRandomSet(bool target)
    {
        if (target)
        { return Random.Range(0, socks_color_sprite.Length); }
        else
        { return Random.Range(0, socks_mono_sprite.Length); }
    }

    // 오브젝트 위에 다른 양말들이 오버랩되어있는 지 체크하는 함수
    private bool isOverlapped_dragable(int code)
    {
        count = 0;
        for (int i = 0; i < socks_dragable.Length; i++)
        {
            if (Vector2.Distance(socks_dragable[code].transform.position, socks_dragable[i].transform.position) < 1.3f)
            {
                if (code > i) { count += 1; }
            }
        }
        if (count > 0) { return true; }
        return false;
    }
    private bool isOverlapped_target(int code)
    {
        count = 0;
        for (int i = 0; i < socks_dragable.Length; i++)
        {
            if (Vector2.Distance(socks_target[code].transform.position, socks_dragable[i].transform.position) < 1.3f)
            {
                count += 1;
            }
        }
        if (count > 0) { return true; }
        return false;
    }

    // 양말 터치 가능 여부 설정(타겟)
    private void SocksTouchableSet()
    {
        for (int i = 0; i < socks_target.Length; i++)
        { target_scripts[i].TouchableSet(!isOverlapped_target(i)); }
    }

    // 양말 터치 가능 여부 설정
    private void SocksTouchableSet(int index)
    {
        if (index <= 0 || !isOverlapped_dragable(index) || socks_touched[index])
        {
            dragable_scripts[index].TouchableSet(true);
            socks_touchable[index] = true;
        }
        else
        {
            dragable_scripts[index].TouchableSet(false);
            socks_touchable[index] = false;
        }
    }

    // 양말 터치되었는지 여부 설정
    private void SocksTouchedSet(int index)
    {
        socks_touched[index] = true;
    }

    // 터치 입력 들어온 양말 구분
    private bool CheckSockTouchable(bool dragable, int code)
    {
        if (dragable)
        {
            if (socks_touchable[code])
            {
                SocksTouchedSet(code);
                SocksTouchableInit();
                SocksTouchableSet();
                return true;
            }
            else { return false; }
        }
        else
        {
            if (isOverlapped_target(code))
            { return false; }
            else
            { return true; }
        }
    }

    // 양말 모음
    private void CollectSocks()
    {
        socks_collected += 1;
        if (socks_collected >= socks_target.Length)
        { MinigameEnd(true); }
    }

    #endregion

    // 게임 시작
    public void MinigameStart()
    {
        MinigameTimerStart();
    }

    // 미니게임 끝
    public void MinigameEnd(bool result)
    {
        MinigameTimerStop();
        for (int i = 0; i < socks_target.Length; i++)
        { target_scripts[i].SocksInactive(); }
        for (int i = 0; i < socks_dragable.Length; i++)
        { dragable_scripts[i].SocksInactive(); }
        SendGameResult(result);
    }

    private void DelegateSet()
    {
        Minigame0_Socks.check_touchable += CheckSockTouchable;
        Minigame0_Socks.collected += CollectSocks;
    }

    private void DelegateDel()
    {
        Minigame0_Socks.check_touchable -= CheckSockTouchable;
        Minigame0_Socks.collected -= CollectSocks;
    }

}
