using UnityEngine;

// Attach to PlayerCanvas1 so it detaches from the player at runtime
// and stops moving with the player.
public class DetachOnAwake : MonoBehaviour
{
    private void Awake() => transform.SetParent(null);
}
