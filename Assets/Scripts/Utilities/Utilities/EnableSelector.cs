using UnityEngine;
using UnityEngine.UI;

public class EnableSelector : MonoBehaviour
{
    [SerializeField] private Selectable startingSelectable;

    // Start is called before the first frame update
    private void OnEnable()
    {
        startingSelectable.Select();
    }
}
