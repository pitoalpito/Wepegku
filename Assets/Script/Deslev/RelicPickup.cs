
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RelicPickup : MonoBehaviour
{
    [Header("Relic Settings")]
    public GameObject relicObject;

    [Header("Door Indicator")]
    public Light2D indicatorLight;

    [Header("Light Settings")]
    public float activatedIntensity = 1.5f;
    public Color activatedColor = Color.yellow;

    private bool collected = false;

    private void Start()
    {
        // Lampu mati saat game dimulai.
        if (indicatorLight != null)
        {
            indicatorLight.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("RELIC BERHASIL DIAMBIL!");
            CollectRelic();
        }
    }

    private void CollectRelic()
    {
        collected = true;

        // Nyalakan lampu indikator.
        if (indicatorLight != null)
        {
            indicatorLight.enabled = true;
            indicatorLight.intensity = activatedIntensity;
            indicatorLight.color = activatedColor;
        }

        // Hilangkan relic.
        if (relicObject != null)
        {
            relicObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

}
