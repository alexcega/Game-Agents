using UnityEngine;

public class WallGlow : MonoBehaviour
{

    [Header("Wall Glow Settings")]
    [Tooltip("How many times brighter the wall’s emission gets on touch")]
    [SerializeField] private float wallEmissionBoost = 10f;


    private Material _mat;
    private Color _originalEmission;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Grab the instantiated material (so we don't overwrite the shared asset)
        _mat = GetComponent<Renderer>().material;
        // Cache whatever the wall’s base emission was
        _originalEmission = _mat.GetColor("_EmissionColor");
    }
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("hit");
        // Only glow on hits from puck or players
        // if (collision.gameObject.CompareTag("Puck") ||
        //     collision.gameObject.CompareTag("Player"))
        // {
        // HDR lets us go above 1.0 for bright “glow”
        _mat.SetColor("_EmissionColor", _originalEmission * wallEmissionBoost);
        // }
    }

    void OnCollisionExit(Collision collision){
        _mat.SetColor("_EmissionColor", _originalEmission);        
    }
}
