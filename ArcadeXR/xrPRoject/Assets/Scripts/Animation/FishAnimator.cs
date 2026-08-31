using UnityEngine;

namespace Animation.Fish
{
    public class FishAnimator : MonoBehaviour
    {
        private const string FISH_CATCHED_ANIMATION = "IsCatched";
        private const string FISH_BEING_CATCHED_ANIMATION = "IsCatching";

        private Animator anim;

        private void Start()
        {
            anim = GetComponentInChildren<Animator>();
        }

        public void SetFishCatched()
        {
            if (anim == null) return;
            anim.SetBool(FISH_CATCHED_ANIMATION, true);
        }

        public void SetFishCatching()
        {
            if (anim == null) return;
            anim.SetBool(FISH_BEING_CATCHED_ANIMATION, true);
        }

        public void SetFishSwimmingFromBeingCatching()
        {
            if (anim == null) return;
            anim.SetBool(FISH_BEING_CATCHED_ANIMATION, false);
        }
    }
}

