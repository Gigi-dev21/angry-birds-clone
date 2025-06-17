using UnityEngine;
using UnityEngine.InputSystem;

public class SlingShotArea : MonoBehaviour
{
    [SerializeField] private LayerMask _slingshotAreaMask;

    public bool IsWithinSlingShotArea()
    {

        Vector2 worldPostion = Camera.main.ScreenToWorldPoint(InputManager.MousePosition);
        if (Physics2D.OverlapPoint(worldPostion, _slingshotAreaMask))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
