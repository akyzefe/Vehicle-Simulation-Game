using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target;

    [Header("Pozisyon Ayarları")]
    public float distance = 6f;
    public float height = 2f;
    public float positionDamping = 0.1f;

    [Header("Dönüş Kayması")]
    public float lateralOffset = 1.5f;    // Sağa/sola kayma miktarı
    public float lateralDamping = 0.3f;   // Kaymanın gecikmesi

    private Vector3 currentVelocity;
    private float currentLateralOffset;
    private float lateralVelocity;
    private Rigidbody targetRb;

    void Start()
    {
        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (target == null) return;

        // Arabanın sağa/sola dönüş hızını al
        float steerInput = 0f;
        if (targetRb != null)
        {
            Vector3 localVelocity = target.InverseTransformDirection(targetRb.linearVelocity);
            steerInput = Mathf.Clamp(localVelocity.x / 10f, -1f, 1f);
        }

        // Lateral offset smooth geçiş
        float targetLateral = steerInput * lateralOffset;
        currentLateralOffset = Mathf.SmoothDamp(
            currentLateralOffset,
            targetLateral,
            ref lateralVelocity,
            lateralDamping
        );

        // Hedef pozisyon: arkada, yukarıda, ve sağa/sola kaymış
        Vector3 targetPosition = target.position
            - target.forward * distance
            + Vector3.up * height
            + target.right * currentLateralOffset;

        // Smooth pozisyon
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            positionDamping
        );

        // Arabaya bak
        Quaternion targetRotation = Quaternion.LookRotation(
            target.position - transform.position
        );
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * 10f
        );
    }
}