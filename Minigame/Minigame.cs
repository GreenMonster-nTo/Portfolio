using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minigame : MonoBehaviour
{
    private int minigame_timer;                 // 미니게임 타이머
    private int timer;                          // 타이머를 세기 위한 타이머

    private Coroutine minigame_timer_coroutine; // 타이머 코루틴
    private WaitForSeconds minigame_wait = new WaitForSeconds(1f);

    // 게임 결과 전송용 델리게이트
    public delegate void GameResult(bool success);
    public static event GameResult game_result;

    // 미니게임 타이머 시작
    protected void MinigameTimerStart()
    {
        minigame_timer = 10;
        UIManager.manager.MinigameTimerActiveSet(true);
        minigame_timer_coroutine = StartCoroutine(MinigameTimer());
    }

    // 미니게임 타이머
    protected IEnumerator MinigameTimer()
    {
        timer = minigame_timer;
        while (timer > 0)
        {
            UIManager.manager.MinigameTimerSet(timer);
            yield return minigame_wait;
            timer -= 1;
        }
        UIManager.manager.MinigameTimerSet(timer);
        SendGameResult(false);
    }

    // 미니게임 타이머 멈춤
    protected void MinigameTimerStop()
    {
        if (minigame_timer_coroutine != null)
        { StopCoroutine(minigame_timer_coroutine); }
    }

    // 이벤트 실행
    protected void SendGameResult(bool result)
    {
        game_result(result);
    }
}