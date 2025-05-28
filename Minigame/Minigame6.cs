using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame6 : Minigame, IMinigame
{
    public SpriteRenderer cup_renderer;         // 컵 스프라이트 렌더러
    public SpriteRenderer liquid_renderer;      // 음료 스프라이트 렌더러

    public Sprite[] cup_sprites;                // 컵 스프라이트
    public Sprite[] liquid_sprites;             // 음료 스프라이트

    public GameObject capsule_insert;           // 캡슐 넣을 곳

    public Sprite[] capsule_sprites;            // 캡슐 스프라이트들
    private Sprite[] liquid_animsheet;          // 음료 애니메이션 시트

    public GameObject[] capsules;               // 캡슐 오브젝트
    private SpriteRenderer[] capsule_renderer;  // 캡슐 스프라이트 렌더러
    private Minigame6_Capsule[] capsule_scripts;// 캡슐 오브젝트 스크립트
    private int[] capsule_type;                 // 캡슐 타입

    private int picked_capsule;                 // 선택된 캡슐 인덱스
    private bool capsule_in;                    // 캡슐 넣었는지
    private bool currect;                       // 맞는 캡슐 넣었는지

    private WaitForSeconds delay;               // 액체 애니메이션 출력용 딜레이

    private int random_capsule;                 // 랜덤 캡슐 설졍용 int
    private int count;                          // 정답 캡슐 삽입용 int

    #region Initialize

    // 초기화
    public void MinigameInitialize()
    {
        DelegateSet();
        capsule_type = new int[capsules.Length];
        capsule_scripts = new Minigame6_Capsule[capsules.Length];
        capsule_renderer = new SpriteRenderer[capsules.Length];
        liquid_animsheet = new Sprite[2];

        delay = new WaitForSeconds(0.3f);

        for (int i = 0; i < capsules.Length; i++)
        {
            capsule_scripts[i] = capsules[i].GetComponent<Minigame6_Capsule>();
            capsule_renderer[i] = capsules[i].GetComponent<SpriteRenderer>();
        }
        for (int i = 0; i < capsules.Length; i++)
        { capsule_scripts[i].CapsuleInitialize(i, capsule_insert.transform); }
    }

    // 액티브(지속 초기화)
    public void MinigameActive()
    {
        capsule_insert.SetActive(false);
        liquid_renderer.gameObject.SetActive(false);
        picked_capsule = Random.Range(0, capsules.Length);
        capsule_in = false;
        CupSet();
        CapsuleSet();
        AnimSheetSet();
    }

    // 선택된 캡슐알림용 컵 스프라이트 설정
    private void CupSet()
    {
        cup_renderer.sprite = cup_sprites[picked_capsule];
    }

    // 캡슐들 초기화
    private void CapsuleSet()
    {
        for (int i = 0; i < capsules.Length; i++)
        {
            capsules[i].SetActive(true);
            capsule_type[i] = RandomIndexSet(i);
            InsertRandomCapsule(i, capsule_type[i]);
            capsule_scripts[i].CapsuleActive();
        }
        CorrectCapsuleExist();
    }

    // 애니메이션 시트 설정
    private void AnimSheetSet()
    {
        liquid_animsheet[0] = liquid_sprites[picked_capsule * 2];
        liquid_animsheet[1] = liquid_sprites[(picked_capsule * 2) + 1];
    }

    // 캡슐 종류 랜덤설정
    private int RandomIndexSet(int index)
    {
        random_capsule = Random.Range(0, capsules.Length);
        for (int i = 0; i < index; i++)
        {
            if (capsule_type[i] == random_capsule)
            { random_capsule = RandomIndexSet(index); }
        }
        return random_capsule;
    }

    // 캡슐들 전체 검사 후 정답 캡슐이 없으면 삽입
    private void CorrectCapsuleExist()
    {
        count = 0;
        for (int i = 0; i < capsules.Length; i++)
        {
            if (capsule_type[i] == picked_capsule)
            { count++; }
        }
        if (count <= 0)
        {
            int rand = Random.Range(0, capsules.Length);
            InsertRandomCapsule(rand, picked_capsule);
            capsule_type[rand] = picked_capsule;
        }
    }

    // 랜덤캡슐 삽입 (8퍼센트 확률)
    private void InsertRandomCapsule(int index, int selected)
    {
        capsule_renderer[index].sprite = Random.Range(0, 26) <= 2 ? capsule_sprites[4] : capsule_sprites[selected];
    }

    // 인액티브
    public void MinigameInactive()
    {
        DelegateDel();
    }

    #endregion

    #region Direction

    // 커피머신 액티브
    private void CoffeemachineActive()
    {
        capsule_insert.SetActive(true);
        liquid_renderer.gameObject.SetActive(true);
    }

    // 음료 애니메이션
    private IEnumerator LiquidPour()
    {
        int timer = 5;
        int frame = 0;
        while (timer > 0)
        {
            if (frame == liquid_animsheet.Length)
            { frame = 0; }
            liquid_renderer.sprite = liquid_animsheet[frame++];
            timer -= 1;
            yield return delay;
        }
        MinigameEnd(currect);
    }

    #endregion

    #region Game Logic

    // 캡슐 체크
    private void CheckCapsuleRight(int index, bool insert)
    {
        capsule_in = insert;

        for (int i = 0; i < capsules.Length; i++)
        { capsule_scripts[i].SetTouchable(false); }

        GameManager.manager.GetSoundManager().CoffeeMachine();
        CoffeemachineActive();

        if (capsule_type[index] == picked_capsule)
        { currect = true; }
        else
        { currect = false; }

        MinigameTimerStop();
        StartCoroutine(LiquidPour());
    }

    // 캡슐 하나를 들어올리면 나머지는 못 들어올리게 하는 함수
    private void CapsulePickedUp(int capsule_index, bool picked_up)
    {
        for (int i = 0; i < capsules.Length; i++)
        {
            if (i == capsule_index) { continue; }
            capsule_scripts[i].SetTouchable(!picked_up);
            capsule_scripts[i].CapsulePosReturn();
        }
        if (!picked_up && !capsule_in)
        {
            for (int i = 0; i < capsules.Length; i++)
            { capsule_scripts[i].CapsulePosReturn(); }
        }
        if (capsule_in)
        {
            for (int i = 0; i < capsules.Length; i++)
            { capsule_scripts[i].SetTouchable(!picked_up); }
        }
    }

    #endregion

    // 게임 시작
    public void MinigameStart()
    {
        MinigameTimerStart();
    }

    // 게임 끝
    public void MinigameEnd(bool result)
    {
        StopAllCoroutines();
        liquid_renderer.gameObject.SetActive(false);
        if (!result)
        { cup_renderer.sprite = cup_sprites[4]; }
        SendGameResult(result);
    }

    private void DelegateSet()
    {
        Minigame6_Capsule.picked += CapsulePickedUp;
        Minigame6_Capsule.insert += CheckCapsuleRight;
    }

    private void DelegateDel()
    {
        Minigame6_Capsule.picked -= CapsulePickedUp;
        Minigame6_Capsule.insert -= CheckCapsuleRight;
    }
}
