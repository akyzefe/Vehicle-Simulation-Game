using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    [Header("UI & Kadran")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI gearText;
    public TextMeshProUGUI rpmText;
    public Image absIcon;
    public Image tcsIcon;

    [Header("Test ve Ölçüm")]
    private float timer0to100 = 0f;
    private bool isMeasuring0to100 = false;
    private bool hasReached100 = false;
    
    [Header("İbreler (Needles)")]
    public RectTransform speedNeedle; // Hız ibresi UI Image
    public RectTransform rpmNeedle;   // Devir ibresi UI Image
    public float minSpeedAngle = 210f; 
    public float maxSpeedAngle = -30f;
    public float minRpmAngle = 210f;
    public float maxRpmAngle = -30f;

    [Header("Wheel Colliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Wheel Meshes")]
    public Transform wheelFL_T;
    public Transform wheelFR_T;
    public Transform wheelRL_T;
    public Transform wheelRR_T;

    [Header("Kapsül Sistemi")]
    public bool kapsulSistemiKullan = false;

    public Vector3 solOnAciDuzeltme;
    public Vector3 sagOnAciDuzeltme;
    public Vector3 solArkaAciDuzeltme;
    public Vector3 sagArkaAciDuzeltme;

    private Transform[] kapsuller = new Transform[4];

    public enum DriveType { FWD, RWD, AWD }

    [Header("Çekiş Sistemi")]
    public DriveType driveType = DriveType.AWD; // Cipe uygun AWD varsayılan yapıldı

    [Range(0f, 1f)] 
    public float frontTorqueSplit = 0.5f; // AWD için güç %50 - %50 dağıtıldı

    [Header("Araba Ayarları")]
    public float motorTorque = 2500f; // Patinajı yenmesi için artırıldı
    public float brakeTorque = 3000f;
    public float maxSteerAngle = 30f;
    public float maxSpeed = 200f;

    [Header("Direksiyon")]
    public float highSpeedSteerLimit = 10f;

    [Header("Vites Hissi (Shift)")]
    private float gearCooldownTimer = 0f; // Vitesin hemen geri düşmesini engelleyen kilit
    public float shiftDelay = 0.3f; // Vites atma süresi (Güç kesintisi)
    private float shiftTimer = 0f;
    private bool isShifting = false;

    [Header("Grip / Drift")]
    public float forwardGrip = 3.0f; // Kalkış patinajını önlemek için yüksek yol tutuş
    public float sidewaysGrip = 1.5f;
    public float driftSidewaysGrip = 0.7f;

    [Header("Downforce")]
    public float downforce = 100f;

    [Header("Stabilite")]
    public float centerOfMassY = -0.5f;
    public float antiRollForce = 3000f;

    [Header("TCS")]
    public bool useTCS = true;
    public float slipLimit = 0.7f; // Motoru boğmaması için esnetildi
    public float tcsStrength = 0.2f;

    [Header("ABS")]
    public bool useABS = true;
    public float absSlipLimit = 0.3f;
    public float absStrength = 0.6f;

    [Header("Gear System")]
    public int currentGear = 1; // -1 olursa Geri vites (R)
    public int maxGear = 5;
    public float[] gearSpeeds = { 0, 40, 80, 120, 160, 220 };

    [Header("RPM")]
    public float maxRPM = 7000f;
    private float currentRPM;

    public float currentSpeed;
    private Rigidbody rb;

    private bool absActive;
    private bool tcsActive;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, centerOfMassY, 0);
        rb.mass = 1500f;
        rb.linearDamping = 0.2f;
        rb.angularDamping = 3f;

        SetupWheelCollider(wheelFL);
        SetupWheelCollider(wheelFR);
        SetupWheelCollider(wheelRL);
        SetupWheelCollider(wheelRR);

        if (kapsulSistemiKullan)
        {
            kapsuller[0] = KapsulOlustur(wheelFL, wheelFL_T, solOnAciDuzeltme);
            kapsuller[1] = KapsulOlustur(wheelFR, wheelFR_T, sagOnAciDuzeltme);
            kapsuller[2] = KapsulOlustur(wheelRL, wheelRL_T, solArkaAciDuzeltme);
            kapsuller[3] = KapsulOlustur(wheelRR, wheelRR_T, sagArkaAciDuzeltme);
        }
    }

    Transform KapsulOlustur(WheelCollider wc, Transform t, Vector3 rot)
    {
        GameObject kapsul = new GameObject(t.name + "_Kapsul");
        kapsul.transform.position = wc.transform.position;
        kapsul.transform.rotation = wc.transform.parent.rotation;
        kapsul.transform.parent = wc.transform.parent;

        t.parent = kapsul.transform;
        t.localPosition = Vector3.zero;
        t.localEulerAngles = rot;

        return kapsul.transform;
    }

    void SetupWheelCollider(WheelCollider wc)
    {
        JointSpring spring = wc.suspensionSpring;
        spring.spring = 25000f;
        spring.damper = 2500f;
        wc.suspensionSpring = spring;

        WheelFrictionCurve fwd = wc.forwardFriction;
        fwd.stiffness = forwardGrip;
        wc.forwardFriction = fwd;

        WheelFrictionCurve side = wc.sidewaysFriction;
        side.stiffness = sidewaysGrip;
        wc.sidewaysFriction = side;
    }

    void Update()
    {
        // Yönlü hız hesabı (Geri gidiyorsa eksi değer verir)
        float forwardVelocity = Vector3.Dot(rb.linearVelocity, transform.forward);
        
        // Ekranda hızın hep pozitif görünmesi için mutlak değer alıyoruz
        currentSpeed = Mathf.Abs(forwardVelocity * 3.6f);

        // --- 0-100 KRONOMETRESİ ---
        if (currentSpeed > 1f && currentSpeed < 100f && !hasReached100)
        {
            isMeasuring0to100 = true;
            timer0to100 += Time.deltaTime; // Saniyeleri say
        }
        else if (currentSpeed >= 100f && isMeasuring0to100)
        {
            isMeasuring0to100 = false;
            hasReached100 = true;
            Debug.Log("<color=green><b>MERCEDES 0-100 SÜRESİ: " + timer0to100.ToString("F2") + " Saniye!</b></color>");
        }
        
        // Araba durduğunda sayacı tekrar test için sıfırla
        if (currentSpeed < 1f) 
        {
            timer0to100 = 0f;
            hasReached100 = false;
        }

        // Vites, RPM ve UI Güncellemeleri
        UpdateGear(forwardVelocity);
        UpdateRPM();
        UpdateUI();

        var keyboard = Keyboard.current;

        // DRIFT (Shift)
        bool drifting = keyboard != null && keyboard.leftShiftKey.isPressed;

        float currentGrip = drifting ? driftSidewaysGrip : sidewaysGrip;

        SetGrip(wheelFL, currentGrip);
        SetGrip(wheelFR, currentGrip);
        SetGrip(wheelRL, currentGrip);
        SetGrip(wheelRR, currentGrip);

        if (kapsulSistemiKullan)
        {
            UpdateKapsul(wheelFL, kapsuller[0]);
            UpdateKapsul(wheelFR, kapsuller[1]);
            UpdateKapsul(wheelRL, kapsuller[2]);
            UpdateKapsul(wheelRR, kapsuller[3]);
        }
        else
        {
            UpdateEski(wheelFL, wheelFL_T, solOnAciDuzeltme);
            UpdateEski(wheelFR, wheelFR_T, sagOnAciDuzeltme);
            UpdateEski(wheelRL, wheelRL_T, solArkaAciDuzeltme);
            UpdateEski(wheelRR, wheelRR_T, sagArkaAciDuzeltme);
        }
    }

    void SetGrip(WheelCollider wc, float grip)
    {
        WheelFrictionCurve side = wc.sidewaysFriction;
        side.stiffness = grip;
        wc.sidewaysFriction = side;
    }

    void UpdateKapsul(WheelCollider col, Transform kapsul)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        kapsul.position = pos;
        kapsul.rotation = rot;
    }

    void UpdateEski(WheelCollider col, Transform t, Vector3 offset)
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        t.position = pos;
        t.rotation = rot * Quaternion.Euler(offset);
    }

    void FixedUpdate()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float motor = keyboard.wKey.isPressed ? 1f : keyboard.sKey.isPressed ? -1f : 0f;
        float steer = keyboard.aKey.isPressed ? -1f : keyboard.dKey.isPressed ? 1f : 0f;
        bool braking = keyboard.spaceKey.isPressed;

        // DYNAMIC STEERING
        float speedFactor = currentSpeed / maxSpeed;
        float dynamicSteer = Mathf.Lerp(maxSteerAngle, highSpeedSteerLimit, speedFactor);

        wheelFL.steerAngle = steer * dynamicSteer;
        wheelFR.steerAngle = steer * dynamicSteer;

        // VİTES ATMA SÜRESİNİ HESAPLA (DEBRİYAJ)
        if (isShifting)
        {
            shiftTimer -= Time.fixedDeltaTime;
            if (shiftTimer <= 0) isShifting = false;
        }

        // MOTOR TORKU HESAPLAMA (İleri ve Geri Vites Limitleri)
        float torque = 0f;
        if (motor != 0 && !braking)
        {
            if (currentGear == -1 && currentSpeed < 40f) // Geri viteste max 40 km/h
                torque = motor * motorTorque;
            else if (currentGear > 0 && currentSpeed < maxSpeed) // İleri viteste normal hız
                torque = motor * motorTorque;
        }

        // EĞER VİTES ATILIYORSA GÜCÜ KES (DEBRİYAJA BASILDI)
        if (isShifting)
        {
            torque = 0f; 
        }

        // TORQUE SPLIT
        float frontTorque = torque * frontTorqueSplit;
        float rearTorque = torque * (1f - frontTorqueSplit);

        // Çekiş Sistemine Göre Ham Torku Dağıtma
        float flTorque = (driveType == DriveType.FWD || driveType == DriveType.AWD) ? frontTorque : 0f;
        float frTorque = (driveType == DriveType.FWD || driveType == DriveType.AWD) ? frontTorque : 0f;
        float rlTorque = (driveType == DriveType.RWD || driveType == DriveType.AWD) ? rearTorque : 0f;
        float rrTorque = (driveType == DriveType.RWD || driveType == DriveType.AWD) ? rearTorque : 0f;

        // TCS Uygulama
        tcsActive = false;
        wheelFL.motorTorque = ApplyTCS(wheelFL, flTorque);
        wheelFR.motorTorque = ApplyTCS(wheelFR, frTorque);
        wheelRL.motorTorque = ApplyTCS(wheelRL, rlTorque);
        wheelRR.motorTorque = ApplyTCS(wheelRR, rrTorque);

        float brake = braking ? brakeTorque : 0f;

        // ABS Uygulama
        absActive = false;
        wheelFL.brakeTorque = ApplyABS(wheelFL, brake);
        wheelFR.brakeTorque = ApplyABS(wheelFR, brake);
        wheelRL.brakeTorque = ApplyABS(wheelRL, brake);
        wheelRR.brakeTorque = ApplyABS(wheelRR, brake);

        // DOWNFORCE
        rb.AddForce(-transform.up * downforce * rb.linearVelocity.magnitude);

        AntiRoll(wheelFL, wheelFR);
        AntiRoll(wheelRL, wheelRR);
    }

    void AntiRoll(WheelCollider left, WheelCollider right)
    {
        WheelHit hit;
        float travelL = 1f, travelR = 1f;

        bool groundedL = left.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-left.transform.InverseTransformPoint(hit.point).y - left.radius) / left.suspensionDistance;

        bool groundedR = right.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-right.transform.InverseTransformPoint(hit.point).y - right.radius) / right.suspensionDistance;

        float antiRoll = (travelL - travelR) * antiRollForce;

        if (groundedL)
            rb.AddForceAtPosition(left.transform.up * -antiRoll, left.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(right.transform.up * antiRoll, right.transform.position);
    }

    // --- YENİ EKLENEN SİSTEM METOTLARI ---

    void UpdateGear(float forwardVelocity)
    {
        // Koruma süresi (Cooldown) zamanlayıcısı
        if (gearCooldownTimer > 0) 
        {
            gearCooldownTimer -= Time.deltaTime;
        }

        // Eğer vites atma (debriyaj) işlemi devam ediyorsa yeni hesaplama yapma, bekle!
        if (isShifting) return; 

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float motorInput = keyboard.wKey.isPressed ? 1f : keyboard.sKey.isPressed ? -1f : 0f;
        int targetGear = currentGear;

        // GERİ VİTES (R) MANTIĞI
        if (motorInput < 0 && forwardVelocity < 1f && currentSpeed < 5f) 
        {
            targetGear = -1; 
        }
        // İLERİ VİTESE GEÇİŞ (1. Vites)
        else if (motorInput > 0 && currentGear == -1)
        {
            targetGear = 1;
        }
        // NORMAL İLERİ VİTES BÜYÜTME/KÜÇÜLTME (1'den 5'e)
        else if (targetGear > 0)
        {
            // VİTES BÜYÜTME
            if (currentGear < maxGear && currentSpeed >= gearSpeeds[currentGear])
            {
                targetGear = currentGear + 1;
            }
            // VİTES KÜÇÜLTME (Hız 10 km/h düştüyse VE koruma süresi bittiyse)
            else if (currentGear > 1 && currentSpeed < (gearSpeeds[currentGear - 1] - 10f) && gearCooldownTimer <= 0)
            {
                targetGear = currentGear - 1; 
            }
        }

        // Eğer vites gerçekten değiştiyse animasyonu ve kilitleri başlat
        if (targetGear != currentGear)
        {
            currentGear = targetGear;
            isShifting = true;
            shiftTimer = shiftDelay; // Gücü kesme süresi
            
            // YENİ: Vites attıktan sonra 1 saniye boyunca vites küçültmeyi YASAKLA
            gearCooldownTimer = 1.0f; 
        }
    }

    void UpdateRPM()
    {
        // Geri vitesteysek RPM'i hıza göre basitçe hesapla
        if (currentGear == -1)
        {
            float t = Mathf.InverseLerp(0, 40f, currentSpeed); // Max geri hızı 40
            currentRPM = Mathf.Lerp(1000, maxRPM, t);
            return;
        }

        float min = gearSpeeds[Mathf.Clamp(currentGear - 1, 0, gearSpeeds.Length - 1)];
        float max = gearSpeeds[Mathf.Clamp(currentGear, 0, gearSpeeds.Length - 1)];
        float factor = Mathf.InverseLerp(min, max, currentSpeed);
        currentRPM = Mathf.Lerp(1000, maxRPM, factor);
    }

    void UpdateUI()
    {
        if (speedText)
            speedText.text = Mathf.RoundToInt(currentSpeed) + " km/h";

        if (gearText)
        {
            // Eğer -1 ise ekrana R yaz, değilse vites sayısını yaz
            if (currentGear == -1) gearText.text = "R";
            else gearText.text = currentGear.ToString();
        }

        if (rpmText)
            rpmText.text = Mathf.RoundToInt(currentRPM) + " RPM";

        if (absIcon) SetAlpha(absIcon, absActive ? 1 : 0);
        if (tcsIcon) SetAlpha(tcsIcon, tcsActive ? 1 : 0);

        // KADRAN İBRELERİ (NEEDLES) DÖNDÜRME SİSTEMİ
        if (speedNeedle != null)
        {
            float speedFactor = Mathf.Clamp01(currentSpeed / maxSpeed);
            float currentSpeedAngle = Mathf.Lerp(minSpeedAngle, maxSpeedAngle, speedFactor);
            speedNeedle.localEulerAngles = new Vector3(0, 0, currentSpeedAngle);
        }

        if (rpmNeedle != null)
        {
            // Vites atarken debriyaja basıldığında devir ibresi anlık düşsün
            float targetRpm = isShifting ? 1500f : currentRPM; 
            float rpmFactor = Mathf.Clamp01(targetRpm / maxRPM);
            float currentRpmAngle = Mathf.Lerp(minRpmAngle, maxRpmAngle, rpmFactor);
            rpmNeedle.localEulerAngles = new Vector3(0, 0, currentRpmAngle);
        }
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = Mathf.Lerp(c.a, a, Time.deltaTime * 10f);
        img.color = c;
    }

    float ApplyTCS(WheelCollider wc, float torque)
    {
        if (!useTCS) return torque;

        WheelHit hit;
        if (wc.GetGroundHit(out hit))
        {
            if (Mathf.Abs(hit.forwardSlip) > slipLimit)
            {
                tcsActive = true;
                return torque * (1f - tcsStrength);
            }
        }
        return torque;
    }

    float ApplyABS(WheelCollider wc, float brake)
    {
        if (!useABS) return brake;

        WheelHit hit;
        if (wc.GetGroundHit(out hit))
        {
            if (Mathf.Abs(hit.forwardSlip) > absSlipLimit)
            {
                absActive = true;
                return brake * (1f - absStrength);
            }
        }
        return brake;
    }
}