using UnityEngine;
using Horror.Manager;

namespace Horror.Changer
{
    public class MapChanger : MonoBehaviour
    {
        [SerializeField] private string _id;
        [SerializeField] private Renderer _targetRenderer;
        [SerializeField] private AudioSource _targetAudio;

        private void Start()
        {
            HorrorManager.Instance.OnHorrorChange += HandleHorrorChange;
        }

        protected virtual void HandleHorrorChange(HorrorMapChange newChange)
        {
            if (newChange == null) return;
            if (!IsTargeted(newChange)) return;

            switch (newChange.type)
            {
                case HorrorType.Audio:
                    if (_targetAudio != null && newChange.musicToPlay != null)
                    {
                        _targetAudio.clip = newChange.musicToPlay;
                        _targetAudio.PlayOneShot(_targetAudio.clip);
                    }
                    break;
                case HorrorType.Texture:
                    if (_targetRenderer != null && newChange.textureToChange != null)
                        _targetRenderer.material.mainTexture = newChange.textureToChange;
                    break;
                case HorrorType.Both:
                    _targetAudio.clip = newChange.musicToPlay;
                    _targetAudio.PlayOneShot(_targetAudio.clip);
                    _targetRenderer.material.mainTexture = newChange.textureToChange;
                    break;
            }
        }

        private bool IsTargeted(HorrorMapChange change)
        {
            if (change.targetIDs == null || change.targetIDs.Length == 0) return true;

            foreach (var id in change.targetIDs)
            {
                if (id == _id) return true;
            }
            return false;
        }

        private void OnDisable()
        {
            HorrorManager.Instance.OnHorrorChange -= HandleHorrorChange;
        }
    }   
}

