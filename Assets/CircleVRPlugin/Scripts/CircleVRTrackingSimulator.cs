using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleVRTrackingSimulator : MonoBehaviour
{
    internal class SimulateTracker : MonoBehaviour
    {
        private const float height = 1.75f;
        private const float t = 0.5f;

        private Vector3 start;
        private Vector3 dest;

        private float barrierRadius;

        public void Init(float barrierRadius)
        {
            this.barrierRadius = barrierRadius;

            start = GetRandomPointInCircle(barrierRadius , height);
            dest = GetRandomPointInCircle(barrierRadius, height);

            transform.position = start;

            StartCoroutine(SetRandomDest());

            Debug.Log("[Tracker Simulator] Initialized");
        }

        private Vector3 GetRandomPointInCircle(float radius , float height)
        {
            Vector3 result = Vector3.zero;

            float angle = Random.Range(0.0f, 360.0f);
            float range = Random.Range(0.0f, radius);

            result = new Vector3(range * Mathf.Cos(angle), height, range * Mathf.Sin(angle));

            return result;
        }

        private IEnumerator SetRandomDest()
        {
            while(true)
            {
                while (Vector3.Distance(transform.position, dest) > 0.3f)
                    yield return null;

                yield return new WaitForSeconds(2.0f);

                dest = GetRandomPointInCircle(barrierRadius, height);
                Debug.Log(dest);
                yield return null;
            }
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, dest, t * Time.deltaTime);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }

    internal class SetterFollower : MonoBehaviour
    {
        private CircleVRTrackerSetter target;

        public void Init(CircleVRTrackerSetter setter)
        {
            target = setter;
            Debug.Log("[Setter Follower] Initialized");
        }

        private void Update()
        {
            if (target)
                transform.position = Vector3.Lerp(transform.position, target.transform.position , 1.0f);
        }
    }


    public void Init(Configuration config)
    {
        CircleVRTrackerSetter[] setters = FindObjectsOfType<CircleVRTrackerSetter>();

        if(setters.Length > 0)
        {
            foreach (var pair in config.circlevr.pairs)
            {
                CircleVRTrackerSetter result = null;

                foreach (var setter in setters)
                {
                    if (setter.trackerID == pair.trackerID)
                    {
                        result = setter;
                        break;
                    }
                }

                if(!result)
                {
                    CircleVRTrackingSystem.CreateTracker(pair.trackerID).gameObject.AddComponent<SimulateTracker>().Init(config.circlevr.safetyBarrierRadius);
                    continue;
                }

                CircleVRTrackingSystem.CreateTracker(pair.trackerID).gameObject.AddComponent<SetterFollower>().Init(result);
            }
            return;
        }

        foreach (var pair in config.circlevr.pairs)
        {
            CircleVRTrackingSystem.CreateTracker(pair.trackerID).gameObject.AddComponent<SimulateTracker>().Init(config.circlevr.safetyBarrierRadius);
        }
    }
}
