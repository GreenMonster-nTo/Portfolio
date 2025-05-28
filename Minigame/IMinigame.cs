using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMinigame
{
    void MinigameInitialize();                  // 미니게임 초기화
    void MinigameActive();                      // 미니게임 액티브
    void MinigameInactive();                    // 미니게임 인액티브

    void MinigameStart();                       // 미니게임 시작
    void MinigameEnd(bool result);              // 미니게임 끝
}