using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ActiveSkill : MonoBehaviour
{
    private Canvas activeskill_canvas;                      // 액티브 스킬 UI 캔버스
    public GameObject[] activeskill_slots;                  // 액티브 스킬 슬롯
    private Button[] activeskill_btns;                      // 액티브 스킬 버튼
    private Text[] activeskill_cooltime;                    // 액티브스킬 쿨타임 텍스트
    private GameObject[] activeskill_inactive;              // 액티브스킬 사용가능/불가능 표시 이미지
    private GameObject[] activeskill_locked;                // 액티브 스킬 잠김 표시 이미지
    private Vector2[] activeskill_position;                 // 액티브 스킬 슬롯 포지션

    public GameObject siren_obj;                            // 사이렌 오브젝트
    private Vector2[] siren_position;                       // 사이렌 포지션

    public GameObject refresh_obj;                          // 재배송 버튼 오브젝트
    private Button refresh_btn;                             // 재배송 버튼
    private Text[] refresh_txt;                             // 재배송 텍스트

    private bool[] activeskill_slot_useable;                // 액티브 스킬 사용가능 여부
    private bool[] activeskill_slot_locked;                 // 액티브 스킬 잠겼는지 여부

    private bool activeskill_useable;                       // 액티브스킬 판넬 열 수 있는 상태인지 체크
    private bool activeskill_panelopened;                   // 액티브스킬 판넬이 열려있는지

    private Coroutine coroutine;

    // 액티브스킬 초기화
    public void InitializeActiveSkillPanel()
    {
        activeskill_canvas = this.GetComponent<Canvas>();

        activeskill_useable = false;
        activeskill_panelopened = false;

        activeskill_slot_useable = new bool[activeskill_slots.Length];
        activeskill_slot_locked = new bool[activeskill_slots.Length];

        activeskill_btns = new Button[activeskill_slots.Length];
        activeskill_position = new Vector2[activeskill_slots.Length];
        activeskill_cooltime = new Text[activeskill_slots.Length];
        activeskill_inactive = new GameObject[activeskill_slots.Length];
        activeskill_locked = new GameObject[activeskill_slots.Length];

        for (int i = 0; i < activeskill_slots.Length; i++)
        {
            activeskill_btns[i] = activeskill_slots[i].GetComponent<Button>();
            activeskill_position[i] = activeskill_slots[i].transform.localPosition;
            activeskill_cooltime[i] = activeskill_slots[i].transform.Find("timer").GetComponent<Text>();
            activeskill_inactive[i] = activeskill_slots[i].transform.Find("inactive").gameObject;
            activeskill_locked[i] = activeskill_slots[i].transform.Find("lock").gameObject;
            activeskill_slots[i].transform.localPosition = Vector2.zero;
        }
        coroutine = null;

        InitializeSiren();
    }

    // 사이렌 설정
    private void InitializeSiren()
    {
        siren_position = new Vector2[2];
        siren_position[0] = new Vector2(0, 3.5f);
        siren_position[1] = new Vector2(0, 7.5f);
    }

    // 액티브스킬 타이머 설정
    public void SetActiveSkillTimer(int skill_index, int time)
    {
        activeskill_cooltime[skill_index].text = time.ToString();
    }

    // 액티브스킬 사용 가능 여부 설정
    public void SetActiveSkillUseable(int skill_index, bool useable)
    {
        activeskill_slot_useable[skill_index] = useable;
        if (activeskill_slot_locked[skill_index]) return;

        activeskill_inactive[skill_index].SetActive(!useable);
        activeskill_cooltime[skill_index].gameObject.SetActive(!useable);
        activeskill_btns[skill_index].interactable = useable;
    }

    // 액티브스킬 락/언락
    public void SetActiveSkillLock(int skill_index, bool locked)
    {
        activeskill_slot_locked[skill_index] = locked;
        activeskill_btns[skill_index].interactable = !locked;

        activeskill_inactive[skill_index].SetActive(locked);
        activeskill_cooltime[skill_index].gameObject.SetActive(!locked);
        activeskill_locked[skill_index].gameObject.SetActive(locked);

        if (!activeskill_slot_locked[skill_index])
            SetActiveSkillUseable(skill_index, activeskill_slot_useable[skill_index]);
    }

    // 액티브스킬 사용가능한지 설정
    public void ActiveSkillPanel_UseableSet(schedule now_schedule)
    {
        if (now_schedule == schedule.work)
        {
            activeskill_useable = true;
        }
        else
        {
            activeskill_useable = false;
            if (activeskill_panelopened)
                coroutine = StartCoroutine(ActiveSkillPanel_Close());
        }
    }

    // 액티브스킬 판넬 열고닫기
    public void ActiveSkillPanel_OpenClose()
    {
        if (!activeskill_useable) return;
        
        coroutine = null;
        if (!activeskill_panelopened) coroutine = StartCoroutine(ActiveSkillPanel_Open());
        else coroutine = StartCoroutine(ActiveSkillPanel_Close());
    }

    // 액티브스킬 판넬 열기
    private IEnumerator ActiveSkillPanel_Open()
    {
        activeskill_canvas.sortingOrder = 100;
        for (int i = 0; i < activeskill_slots.Length; i++)
            activeskill_slots[i].SetActive(true);

        while (!activeskill_panelopened)
        {
            for (int i = 0; i < activeskill_slots.Length; i++)
                activeskill_slots[i].transform.localPosition = Vector2.MoveTowards(activeskill_slots[i].transform.localPosition, activeskill_position[i], 2f);

            siren_obj.transform.localPosition = Vector2.MoveTowards(siren_obj.transform.localPosition, siren_position[1], 2f);

            if (activeskill_slots[activeskill_slots.Length - 1].transform.localPosition.x == activeskill_position[activeskill_position.Length - 1].x && activeskill_slots[activeskill_slots.Length - 1].transform.localPosition.y == activeskill_position[activeskill_position.Length - 1].y)
            {
                activeskill_panelopened = !activeskill_panelopened;
                StopCoroutine(coroutine);
            }
            yield return null;
        }
    }

    // 액티브스킬 판넬 닫기
    private IEnumerator ActiveSkillPanel_Close()
    {
        activeskill_canvas.sortingOrder = 4;
        while (activeskill_panelopened)
        {
            for (int i = 0; i < activeskill_slots.Length; i++)
                activeskill_slots[i].transform.localPosition = Vector2.MoveTowards(activeskill_slots[i].transform.localPosition, Vector2.zero, 2f);

            siren_obj.transform.localPosition = Vector2.MoveTowards(siren_obj.transform.localPosition, siren_position[0], 2f);

            if (activeskill_slots[activeskill_slots.Length - 1].transform.localPosition.x == 0 && activeskill_slots[activeskill_slots.Length - 1].transform.localPosition.y == 0)
            {
                for (int i = 0; i < activeskill_slots.Length; i++)
                    activeskill_slots[i].SetActive(false);
                activeskill_panelopened = !activeskill_panelopened;
                StopCoroutine(coroutine);
            }
            yield return null;
        }
    }

    // 액티브스킬 실행
    public void Activeskill_Active(int code)
    {
        active(code);
    }

    public delegate void Skill_Active(int code);
    public static event Skill_Active active;
}
