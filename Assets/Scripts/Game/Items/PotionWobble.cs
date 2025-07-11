using UnityEngine;
using UnityEngine.Serialization;

//Modified Potion: https://www.youtube.com/watch?v=tI3USKIbnh0&t=28s
//Made by Gabriel Aguiar

public class PotionWobble : MonoBehaviour
{
    private static readonly int WobbleX = Shader.PropertyToID("_WobbleX");
    private static readonly int WobbleZ = Shader.PropertyToID("_WobbleZ");
    [FormerlySerializedAs("MaxWobble"), SerializeField] private float maxWobble = 0.03f;
    [FormerlySerializedAs("WobbleSpeed"), SerializeField] private float wobbleSpeed = 1f;
    [FormerlySerializedAs("Recovery"), SerializeField] private float recovery = 1f;
    
    private Renderer _rend;
    private Vector3 _lastPos;
    private Vector3 _velocity;
    private Vector3 _lastRot;
    private Vector3 _angularVelocity;
    private float _wobbleAmountX;
    private float _wobbleAmountZ;
    private float _wobbleAmountToAddX;
    private float _wobbleAmountToAddZ;
    private float _pulse;
    private float _time = 0.5f;
    
    // Use this for initialization
    void Start()
    {
        _rend = GetComponent<Renderer>();
    }
    
    //Ideally, this script should automatically disable itself if it's too far from the camera.
    
    private void Update()
    {
        float dt = Time.deltaTime;
        float s = dt * recovery;
        _time += dt;
        // decrease wobble over time
        _wobbleAmountToAddX = Mathf.Lerp(_wobbleAmountToAddX, 0, s);
        _wobbleAmountToAddZ = Mathf.Lerp(_wobbleAmountToAddZ, 0, s);

        // make a sine wave of the decreasing wobble
        _pulse = 2 * Mathf.PI * wobbleSpeed;
        float p = _pulse * _time;
        _wobbleAmountX = _wobbleAmountToAddX * Mathf.Sin(p);
        _wobbleAmountZ = _wobbleAmountToAddZ * Mathf.Sin(p);

        // send it to the shader
        _rend.material.SetFloat(WobbleX, _wobbleAmountX);
        _rend.material.SetFloat(WobbleZ, _wobbleAmountZ);

        // velocity
        _velocity = (_lastPos - transform.position) / dt;
        _angularVelocity = transform.rotation.eulerAngles - _lastRot;

        // add clamped velocity to wobble
        _wobbleAmountToAddX += Mathf.Clamp((_velocity.x + (_angularVelocity.z * 0.2f)) * maxWobble, -maxWobble, maxWobble);
        _wobbleAmountToAddZ += Mathf.Clamp((_velocity.z + (_angularVelocity.x * 0.2f)) * maxWobble, -maxWobble, maxWobble);

        // keep last position
        _lastPos = transform.position;
        _lastRot = transform.rotation.eulerAngles;
    }
}