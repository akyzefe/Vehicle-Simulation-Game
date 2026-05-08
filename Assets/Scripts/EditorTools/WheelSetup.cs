using UnityEngine;

public class AutoWheelSetup : MonoBehaviour
{
    [Header("Görsel Tekerlekler (FBX İçindeki Mesh'ler)")]
    public Transform gorselOnSol;
    public Transform gorselOnSag;
    public Transform gorselArkaSol;
    public Transform gorselArkaSag;

    [Header("Temel Wheel Collider Ayarları")]
    public float tekerlekKutlesi = 20f;

    // Bu etiket, Unity Editor'de scripte sağ tıklayıp bu fonksiyonu çalıştırmamızı sağlar
    [ContextMenu("Tekerlekleri Otomatik Kur")]
    public void TekerlekleriKur()
    {
        // 1. Hiyerarşide temizlik (Eğer önceden oluşturulmuşsa sil)
        Transform eskiParent = transform.Find("WheelColliders");
        if (eskiParent != null)
        {
            DestroyImmediate(eskiParent.gameObject);
        }

        // 2. Ana taşıyıcı klasörü (Boş Obje) oluştur
        GameObject wcParent = new GameObject("WheelColliders");
        wcParent.transform.SetParent(this.transform);
        wcParent.transform.localPosition = Vector3.zero;
        wcParent.transform.localRotation = Quaternion.identity;

        // 3. Tek tek 4 tekerleği üret
        ColliderUret(gorselOnSol, "WC_OnSol", wcParent.transform);
        ColliderUret(gorselOnSag, "WC_OnSag", wcParent.transform);
        ColliderUret(gorselArkaSol, "WC_ArkaSol", wcParent.transform);
        ColliderUret(gorselArkaSag, "WC_ArkaSag", wcParent.transform);

        Debug.Log("Tekerlek kurulumu başarıyla tamamlandı!");
    }

    private void ColliderUret(Transform gorselTekerlek, string isim, Transform parent)
    {
        if (gorselTekerlek == null) 
        {
            Debug.LogWarning(isim + " için görsel tekerlek atanmamış!");
            return;
        }

        // Fiziksel objeyi yarat ve hiyerarşiye diz
        GameObject wcObj = new GameObject(isim);
        wcObj.transform.SetParent(parent);

        // En Kritik Yer 1: Konumu görsel tekerlekle milimetrik eşitle
        wcObj.transform.position = gorselTekerlek.position;
        wcObj.transform.rotation = Quaternion.identity;
        // Wheel Collider ekle
        WheelCollider wc = wcObj.AddComponent<WheelCollider>();
        wc.mass = tekerlekKutlesi;

        // En Kritik Yer 2: Yarıçapı (Radius) Mesh boyutundan matematiksel olarak hesapla
        MeshRenderer renderer = gorselTekerlek.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            // extents.y, 3D modelin merkezinden en üst noktasına olan mesafedir (yani yarıçaptır)
            wc.radius = renderer.bounds.extents.y;
        }
        else
        {
            Debug.LogWarning(gorselTekerlek.name + " objesinde MeshRenderer bulunamadı. Yarıçapı elle girmelisin.");
        }
    }
}