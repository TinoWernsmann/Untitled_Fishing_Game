using System;
using Core.Catching;
using Unity.Burst.Intrinsics;
using UnityEngine;

/// <summary>
/// Data class for every catchable in the game.
/// It is used to create Trash and Fish.
/// </summary>
[CreateAssetMenu(menuName = "Catch/New Catch Data")]
public class CatchData : ScriptableObject
{
    [SerializeField] private string _name;
    [TextArea]
    [SerializeField] private string _description;
    [Tooltip("Enter negative numbers for score subtraction.")]
    [SerializeField] private int _scoreValue;
    [SerializeField] private CatchBase _prefab;
    [Tooltip("Enter layer the fish will spawn in. First layer = 1")]
    [SerializeField] private int _spawnLayer;

    //score based on minigame
    private float scoreIncrease = 1;
    private const float maxScoreIncrease = 10;

    private bool wasCatchedStatus = false;

    


    public void SetCatched(bool b)
    {
        wasCatchedStatus = b;
    }

    public void SetScoreIncrease(float s, bool wasCatched)
    {
        SetCatched(wasCatched);
        s = Math.Min(s, maxScoreIncrease);
        scoreIncrease = s;
    }

    public int GetScoreValue()
    {
        float computed = ScoreValue * scoreIncrease;
        return Mathf.CeilToInt(computed);
    }
    
    public bool GetCatchedStatus()
    {
        return wasCatchedStatus;
    }


    public CatchBase Prefab => _prefab;
    public string Name => _name;
    public string Description => _description;
    public int ScoreValue => _scoreValue;
    public int Layer => _spawnLayer;
}
