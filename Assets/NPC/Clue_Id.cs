using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clue_Id : MonoBehaviour
{
    // ClueId.cs
public enum ClueId {
    None = 0,
        // Boy
        Boy_EmergencyDoor_BeforeFire = 101,

        // Rescuer
        Rescuer_BagMissing = 201,          // 가방이 사라졌다
        Rescuer_Bag_Lighter = 202,         // 라이터 단서
        Rescuer_Bag_FuelSmell = 203,        // 기름/연료 냄새 단서

             // Passenger (침착한 승객)
        Passenger_WatchMemo = 301,   // 시계를 보며 메모
        Passenger_TimingHint = 302    // 특정 시간/타이밍을 염두
    }

   
}
