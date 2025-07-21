using UnityEngine;

namespace _02_Scripts.Player.Effect
{
    public class ImpactEffect : MonoBehaviour, IPoolObject
    {
        [SerializeField] private string poolID = "ImpactEffect";
        [SerializeField] private int poolSize = 8;
        public GameObject GameObject => gameObject;
        public string PoolID => poolID;

        public int PoolSize => poolSize;

        public void PlayEffect(Vector3 _pos, float _duration)
        {
            transform.position = _pos;
            gameObject.SetActive(true);
            if (TryGetComponent<ParticleSystem>(out var particleSystem))
            {
                particleSystem.Play();
                Invoke(nameof(ReturnToObject),_duration);
            }
        }

        private void ReturnToObject()
        {
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }

        public void OnSpawnFromPool()
        {
        }

        public void OnReturnToPool()
        {
            gameObject.SetActive(false);
            CancelInvoke(nameof(ReturnToObject));
            if (TryGetComponent<ParticleSystem>(out var particleSystem))
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}