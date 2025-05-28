using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Transport4 : Transport
{
    private int collect_count;                      // 모은 개수 저장

    public GameObject dust;                         // 먼지
    public Transport4_Player dust_script;           // 먼지 움직임 제어용 스크립트

    private Vector2 player_direction_start_pos;     // 먼지 연출 시작 포지션
    private Vector3 player_start_rot;               // 먼지 연출 시작 로테이션

    private Vector2 player_direction_end_pos;       // 먼지 연출 엔딩 포지션
    private Vector3 player_end_rot;                 // 먼지 연출 엔딩 로테이션

    private Vector2 player_start_pos;               // 먼지 시작 포지션
    private Vector2 player_end_pos;                 // 먼지 엔딩 포지션
    private Vector3 player_play_rot;                // 먼지 플레이 로테이션

    public GameObject[] sky_obj;                    // 하늘 오브젝트
    public SpriteRenderer[] sky_renderer;           // 하늘 스프라이트 렌더러
    public Sprite[] sky_sprites;                    // 하늘 스프라이트들

    private Vector2[] sky_init_pos;                 // 하늘 초기화 시작 포지션
    private Vector2 sky_start_pos;                  // 하늘 시작 포지션
    private Vector2 sky_move_pos;                   // 하늘 이동 포지션

    public GameObject cloud_obj;                    // 구름 오브젝트

    private Vector2 cloud_start_direction_pos;      // 구름 시작 포지션
    private Vector2 cloud_end_direction_pos;        // 구름 끝 포지션
    private Vector2 cloud_start_pos;                // 구름 시작 포지션
    private Vector2 cloud_end;                      // 구름 끝 포지션

    public GameObject left_wall;                    // 대포 왼쪽 벽 오브젝트
    private Vector2 left_wall_start_pos;            // 대포 벽 시작 포지션
    private Vector2 left_wall_end_pos;              // 대포 벽 끝 포지션

    public GameObject right_wall;                   // 대포 오른쪽 벽 오브젝트
    private Vector2 right_wall_start_pos;           // 대포 벽 시작 포지션
    private Vector2 right_wall_end_pos;             // 대포 벽 끝 포지션

    public Animator cannon_anim;                    // 대포 애니메이션

    public GameObject feather_list;                 // 깃털 리스트 부모
    private Vector2 feather_list_pos;               // 깃털 리스트 부모 포지션
    private List<GameObject> feathers;              // 깃털들
    private List<Transport_Feather> feather_scripts;// 깃털 스크립트들
    public GameObject feather_prf;                  // 깃털 프리팹
    private WaitForSeconds feather_wait;            // 깃털 생성 시간
    private Coroutine feather_coroutine;            // 깃털 코루틴

    private float bg_speed;                         // 뒷배경 이동속도
    private float sky_speed;                        // 하늘 이동속도

    private WaitForSecondsRealtime direction_wait;  // 연출용 타이머

    private int morning_index;                      // 아침인지 체크를 위한 정수형 변수
    private bool now_transport;                     // 현재 이동중인지 체크

    #region Initialize

    private void Awake()
    {
        TransportInitialize();
        Feather_Objectpool_Initialize();
        direction_wait = new WaitForSecondsRealtime(0.5f);
        feather_wait = new WaitForSeconds(0.8f);
    }

    // 이동수단 이니셜라이즈
    private void TransportInitialize()
    {
        player_direction_start_pos = new Vector2(-1.04f, 0.88f);
        player_start_rot = new Vector3(0, 0, 20.5f);
        player_start_pos = new Vector2(2.5f, 0.88f);
        player_end_pos = new Vector2(8f, 0.88f);
        player_play_rot = new Vector3(0, 0, 0);
        player_direction_end_pos = new Vector2(-32.3f, 0f);
        player_end_rot = new Vector3(0, 0, -20.5f);

        sky_init_pos = new Vector2[2];
        sky_init_pos[0] = new Vector2(5.4f, 9.5f);
        sky_init_pos[1] = new Vector2(48.6f, 9.5f);
        sky_start_pos = new Vector2(-16.3f, 0f);
        sky_move_pos = new Vector2(-37.8f, 0f);

        cloud_start_direction_pos = new Vector2(28f, 12f);
        cloud_end_direction_pos = new Vector2(-73f, 12f);
        cloud_start_pos = new Vector2(0, 0);
        cloud_end = new Vector2(-45f, 0);

        feather_list_pos = new Vector2(6, 0);

        left_wall_start_pos = new Vector2(-5.4f, -9.6f);
        left_wall_end_pos = new Vector2(-11.3f, -12.5f);
        right_wall_start_pos = new Vector2(11.3f, -12.5f);
        right_wall_end_pos = new Vector2(5.4f, -9.6f);

        dust_script = dust.GetComponent<Transport4_Player>();
        feather_scripts = new List<Transport_Feather>();
    }

    // 오브젝트 풀 초기화
    private void Feather_Objectpool_Initialize()
    {
        feathers = new List<GameObject>();

        for (int i = 0; i < 30; i++)
        {
            GameObject obj = (GameObject)Instantiate(feather_prf, feather_list.transform);
            obj.SetActive(false);
            feathers.Add(obj);
            feather_scripts.Add(obj.GetComponent<Transport_Feather>());
        }
    }

    #endregion

    #region Game Controller

    // 이동 전 그래픽 초기화
    public void ResetTransport(bool morning)
    {
        morning_index = morning ? 0 : 1;
        now_transport = true;
        collect_count = 0;
        ResetResult();

        SetBackground();
        SetPlayer();
        SetFeathers();
    }

    // 이동 시작 시 호출
    public void StartCannon()
    {
        cannon_anim.SetBool("ready", true);
        Invoke("StartTransportDirection", 0.2f);
    }

    // 대포 애니메이션에서 호출
    private void StartTransportDirection()
    {
        StartCoroutine(PlayerFeatherGameDirection());
        StartCoroutine(StartFeatherGameDirection());
    }

    // 이동 시작
    private void StartTransport()
    {
        TransportStart();

        StartCoroutine(CloudsMove());
        StartCoroutine(SkyMove());
        StartCoroutine(Feather_Fly());

        dust_script.SetDragable(true);
    }

    // 결과 리셋
    private void ResetResult()
    {
        transport_result = false;
    }

    // 배경 스프라이트 리셋
    private void SetBackground()
    {
        for (int i = 0; i < sky_renderer.Length; i++)
        { sky_renderer[i].sprite = sky_sprites[morning_index]; }

        this.transform.position = Vector2.zero;
        sky_obj[0].transform.localPosition = sky_init_pos[0];
        sky_obj[1].transform.localPosition = sky_init_pos[1];
        left_wall.transform.localPosition = left_wall_start_pos;
        right_wall.transform.localPosition = right_wall_start_pos;
        feather_list.transform.localPosition = feather_list_pos;
        cloud_obj.transform.localPosition = cloud_start_direction_pos;

        cannon_anim.SetBool("ready", false);
    }

    // 플레이어 리셋
    private void SetPlayer()
    {
        dust.transform.localPosition = player_direction_start_pos;
        dust.transform.localRotation = Quaternion.Euler(player_start_rot);
        dust_script.SetFly(false);
    }

    // 깃털 리셋
    private void SetFeathers()
    {
        for (int i = 0; i < feathers.Count; i++)
        {
            feathers[i].gameObject.SetActive(false);
        }
    }

    // 깃털 활성화
    private IEnumerator Feather_Fly()
    {
        int count = 0;
        while (transporting)
        {
            for (int i = 0; i < feathers.Count; i++)
            {
                if (!feathers[i].activeInHierarchy && !feather_scripts[i].GetSpawned() && count < 5)
                {
                    feathers[i].gameObject.SetActive(true);
                    feather_scripts[i].FeatherSpawn();
                    count++;
                }
            }
            yield return feather_wait;
            count = 0;
        }
    }

    // 깃털 모음
    private void Collected()
    {
        if (collect_count < 4)
        { collect_count += 1; }
        else
        { transport_result = true; }
        TransportMoney(25);
        dust_script.Collected();
    }

    // 이동 끝
    public void EndTransport()
    {
        StopAllCoroutines();

        dust_script.SetDragable(false);
        UIManager.manager.NoTouch(true);
        now_transport = false;

        TransportPause();
        StartCoroutine(TransportEndDirection());
    }

    #endregion

    #region Direction

    // 시작부 다이렉션 스타트
    private IEnumerator StartFeatherGameDirection()
    {
        while (Vector2.Distance(sky_obj[0].transform.localPosition, sky_start_pos) > 0.1f)
        {
            sky_obj[0].transform.localPosition = Vector2.MoveTowards(sky_obj[0].transform.localPosition, sky_start_pos, 0.3f);
            sky_obj[1].transform.localPosition = Vector2.MoveTowards(sky_obj[1].transform.localPosition, sky_start_pos, 0.3f);
            cloud_obj.transform.localPosition = Vector2.MoveTowards(cloud_obj.transform.localPosition, cloud_start_pos, 0.3f);
            yield return null;
        }
        StartTransport();
    }

    // 시작부 다이렉션 플레이어 세팅
    private IEnumerator PlayerFeatherGameDirection()
    {
        dust_script.SetFly(true);
        while (Vector2.Distance(left_wall.transform.localPosition, left_wall_end_pos) > 0.1f)
        {
            dust.transform.localPosition = Vector2.MoveTowards(dust.transform.localPosition, player_start_pos, 0.01f);
            left_wall.transform.localPosition = Vector2.MoveTowards(left_wall.transform.localPosition, left_wall_end_pos, 0.3f);
            dust.transform.localRotation = Quaternion.RotateTowards(dust.transform.localRotation, Quaternion.Euler(player_play_rot), 0.35f);
            yield return null;
        }
    }

    // 구름 움직임
    private IEnumerator CloudsMove()
    {
        while (transporting)
        {
            cloud_obj.transform.localPosition = new Vector2(Mathf.Lerp(cloud_obj.transform.localPosition.x, cloud_end.x, 0.08f * Time.fixedDeltaTime), cloud_end.y);
            yield return null;
        }
    }

    // 하늘 무한반복을 위한 틈 보간
    private Vector2 GetSkyTransform()
    {
        Vector2 pos;
        float x;
        if (Mathf.Abs(sky_obj[0].transform.localPosition.x) < Mathf.Abs(sky_obj[1].transform.localPosition.x))
            x = sky_obj[0].transform.localPosition.x + (43.2f - 0.15f);
        else
            x = sky_obj[1].transform.localPosition.x + (43.2f - 0.15f);

        pos = new Vector2(x, sky_move_pos.y);
        return pos;
    }

    // 하늘 움직임이 한계를 벗어났는지 체크
    private bool CheckSky(Vector2 pos)
    {
        if (pos.x <= sky_move_pos.x) return true;
        return false;
    }

    // 하늘 움직임
    private IEnumerator SkyMove()
    {
        sky_obj[1].transform.localPosition = new Vector2(sky_obj[1].transform.localPosition.x, sky_obj[0].transform.localPosition.y);
        while (transporting)
        {
            for (int i = 0; i < 2; i++)
            {
                sky_obj[i].transform.localPosition = Vector2.MoveTowards(sky_obj[i].transform.localPosition, sky_move_pos, 0.15f);
                if (CheckSky(sky_obj[i].transform.localPosition)) sky_obj[i].transform.localPosition = GetSkyTransform();
            }
            yield return null;
        }
    }

    // 이동 끝나고 나오는 연출
    private IEnumerator TransportEndDirection()
    {
        while (Vector2.Distance(dust.transform.localPosition, player_end_pos) > 0.1f)
        {
            dust.transform.localPosition = Vector2.MoveTowards(dust.transform.localPosition, player_end_pos, 0.5f);
            right_wall.transform.localPosition = Vector2.MoveTowards(right_wall.transform.localPosition, right_wall_end_pos, 0.5f);
            dust.transform.localRotation = Quaternion.RotateTowards(dust.transform.localRotation, Quaternion.Euler(player_end_rot), 0.35f);

            yield return null;
        }
        yield return direction_wait;

        UIManager.manager.NoTouch(false);
        TransportEnd();
    }

    #endregion

    #region Delegates

    private void OnEnable() => DeletageSet();
    private void OnDisable() => DeletageDel();

    private void DeletageSet()
    {
        Transport_Feather.collect += Collected;
    }
    private void DeletageDel()
    {
        Transport_Feather.collect -= Collected;
    }

    #endregion
}
