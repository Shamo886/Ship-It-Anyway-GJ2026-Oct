using System.Collections;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Min(0.01f)]
    [SerializeField] private float moveDuration = 0.22f;

    public void SnapTo(Vector3 position)
    {
        transform.position = position;
    }

    public IEnumerator AnimateMove(Vector3 destination)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            transform.position = Vector3.Lerp(start, destination, t);
            yield return null;
        }
        transform.position = destination;
    }
}
