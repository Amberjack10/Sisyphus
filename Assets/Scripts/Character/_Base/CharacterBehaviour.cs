using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
    [SerializeField] protected int id; // DB에서 가져올 ID

    public Status currentStat; // Base + Modifier 가 적용된 진짜 스탯들

    public bool isDie;
    public Action OnDieEvent;

    public bool isHit;
    public Action OnHitEvent;
}