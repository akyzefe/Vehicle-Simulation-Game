#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class WheelPivotFixer : MonoBehaviour
{
    [Header("Görsel Tekerlekler (Sahnede düzgün duranlar)")]
    public Transform fl_Mesh;
    public Transform fr_Mesh;
    public Transform rl_Mesh;
    public Transform rr_Mesh;

    [Header("Yeşil Çemberler (Wheel Colliders)")]
    public WheelCollider fl_Collider;
    public WheelCollider fr_Collider;
    public WheelCollider rl_Collider;
    public WheelCollider rr_Collider;

    [ContextMenu("Sadece Pivotları Düzelt (Duruşu Bozmaz)")]
    public void FixPivots()
    {
        FixSinglePivot(fl_Collider, fl_Mesh, "Pivot_FL");
        FixSinglePivot(fr_Collider, fr_Mesh, "Pivot_FR");
        FixSinglePivot(rl_Collider, rl_Mesh, "Pivot_RL");
        FixSinglePivot(rr_Collider, rr_Mesh, "Pivot_RR");

        Debug.Log("İşlem Tamam! Pivotlar arabaya hizalandı. Görsellik bozulmadı.");
    }

    private void FixSinglePivot(WheelCollider col, Transform mesh, string pivotName)
    {
        if (col == null || mesh == null) return;

        // Zaten kılıfa alınmışsa tekrar işlem yapma
        if (mesh.parent != null && mesh.parent.name == pivotName) return;

        // 1. Yeni, tertemiz bir pivot objesi oluştur
        GameObject pivot = new GameObject(pivotName);

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(pivot, "Pivot Oluşturuldu");
#endif

        // 2. Pivotu tam yeşil çemberin (asıl dönme merkezinin) konumuna koy
        pivot.transform.position = col.transform.position;
        
        // 3. PİVOTUN YÖNÜNÜ ARABANIN YÖNÜYLE BİREBİR AYNI YAP (Sihir burada)
        pivot.transform.rotation = col.transform.parent.rotation; // Arabanın gövdesinin rotasyonunu alır

        // 4. Pivotu arabanın gövdesine bağla
#if UNITY_EDITOR
        Undo.SetTransformParent(pivot.transform, col.transform.parent, "Pivot Bağlandı");
#else
        pivot.transform.parent = col.transform.parent;
#endif

        // 5. TEKERLEĞİ PİVOTUN İÇİNE AT (Duruşunu Asla Bozmaz)
        // SetParent'in ikinci parametresi "true" olduğu için, 
        // tekerlek sahnede nasıl duruyorsa öyle kalır, sadece hiyerarşide yeri değişir.
#if UNITY_EDITOR
        Undo.SetTransformParent(mesh, pivot.transform, "Mesh Pivota Bağlandı");
#else
        mesh.SetParent(pivot.transform, true);
#endif
    }
}