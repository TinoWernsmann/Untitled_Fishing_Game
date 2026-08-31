using UnityEngine;

namespace Player.Points
{
    public class PlayerPoints : MonoBehaviour
    {
        private int _score;

        public int CurrentScore => _score;

        public void HandleScoring(int score)
        {   
            if (_score + score < 0)
            {
                _score = 0;
                Debug.Log("Neuer Score: " + _score);
                return;
            }
            _score += score;
            Debug.Log("Neuer Score: " + _score);
        }
    } 
}

