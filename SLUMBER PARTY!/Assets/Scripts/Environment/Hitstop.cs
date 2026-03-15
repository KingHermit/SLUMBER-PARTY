using System.Collections;
using UnityEngine;

namespace Environment
{
    public class Hitstop : MonoBehaviour
    {
        public bool waiting {  get; private set; }

        public void Stop(float duration)
        {
            if (waiting) { return; }

            Time.timeScale = 0.0f;
            StartCoroutine(Wait(duration));
        }

        IEnumerator Wait(float duration)
        {
            waiting = true;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1.0f;
            waiting = false;
        }
    }
}

