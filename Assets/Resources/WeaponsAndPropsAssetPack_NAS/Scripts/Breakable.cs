using System.Collections;
using UnityEngine;

namespace WeaponsAndPropsAssetPack_NAS.Scripts
{
    public class Breakable : MonoBehaviour
    {
        [SerializeField] private Transform wholeObject;
        [SerializeField] private Transform fracturedObject;
        [SerializeField] private bool isCyclic;
        
        // Core Variables
        private bool isBroken;
        private bool isClean;
        private bool objectReseted = true;
        private Transform fracturedObjectInstance;
        private bool shouldBreak;

        // Timers
        private const float timeToCleanUp = 5f;
        private const float timeToReconstructObject = 2f;
        private const float timerTimeUnit = 1f;
        
        private void Start()
        {
            // Removed TriggerBreak(); so objects do not break on spawn
        }

        public void Break()
        {
            if (isBroken) return; // Prevent breaking multiple times

            shouldBreak = true;
        }

        private void Update()
        {
            if (shouldBreak)
            {
                BreakObject();
            }
        }

        private void BreakObject()
        {
            wholeObject.gameObject.SetActive(false);
            fracturedObjectInstance = Instantiate(fracturedObject, wholeObject.position, wholeObject.rotation);
            fracturedObjectInstance.gameObject.SetActive(true);
            isBroken = true;
            shouldBreak = false;
            StartCoroutine(CleanUpCoroutine());
        }

        private void CleanUp()
        {
            isClean = true;
            Destroy(fracturedObjectInstance.gameObject);
        }

        private IEnumerator ResetObject()
        {
            if (isClean)
            {
                yield return new WaitForSeconds(timeToReconstructObject);
                wholeObject.gameObject.SetActive(true);
                isBroken = false;
                isClean = false;
                objectReseted = true;
            }
        }

        private IEnumerator CleanUpCoroutine()
        {
            float timer = 0f;
            while (isBroken && !isClean)
            {
                if (timer >= timeToCleanUp)
                {
                    CleanUp();
                }

                yield return new WaitForSeconds(timerTimeUnit);
                timer += 1f;
            }

            if (isCyclic)
            {
                yield return ResetObject();
            }

            yield return null;
        }

        // Detects collision with weapons or objects tagged as "Weapon"
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Weapon")) // Make sure the weapon has this tag
            {
                Break();
            }
        }
    }
}