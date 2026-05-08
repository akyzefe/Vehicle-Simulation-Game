using UnityEngine;

public class AutoWheelFixer : MonoBehaviour
{
    [Header("Sorunlu Tekerlek Görsellerini Sürükle (Meshler)")]
    public Transform[] wheelMeshes;

    [Header("Düzeltme Açısı (Mesh'i ne kadar döndürelim?)")]
    public Vector3 localCorrectionAngle = new Vector3(90, 0, 0);

    [ContextMenu("Tekerlekleri Otomatik Sar ve Düzelt (ÇALIŞTIR)")]
    public void WrapAndFixWheels()
    {
        foreach (Transform wheelMesh in wheelMeshes)
        {
            if (wheelMesh == null) continue;

            if (wheelMesh.parent != null && wheelMesh.parent.name.EndsWith("_Wrapper"))
            {
                Debug.LogWarning(wheelMesh.name + " zaten düzeltilmiş!");
                continue;
            }

            // 1. Kılıfı (Wrapper) oluştur
            GameObject wrapper = new GameObject(wheelMesh.name + "_Wrapper");

            // 2. Kılıfı tam olarak tekerleğin şu anki pozisyonuna koy
            wrapper.transform.position = wheelMesh.position;
            
            // 3. Kılıfı arabanın gövdesine bağla
            wrapper.transform.parent = wheelMesh.parent;

            // 4. Kılıfın rotasyonunu SIFIRLA (Fizik motoru bunu sevecek)
            wrapper.transform.localRotation = Quaternion.identity;

            // 5. Tekerleği kılıfın içine at
            wheelMesh.parent = wrapper.transform;

            // DİKKAT: Pozisyonu sıfırlamıyoruz (Kaybolmasını engeller)
            
            // 6. Sadece tekerleğin görsel açısını düzelt
            wheelMesh.localRotation = Quaternion.Euler(localCorrectionAngle);

            Debug.Log(wheelMesh.name + " güvenli bir şekilde sarıldı!");
        }
    }
}