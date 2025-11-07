using System.Collections.Generic;
using UnityEngine;
using EzySlice;

[RequireComponent(typeof(Collider))]
public class Sword : MonoBehaviour
{
    [Header("Targeting")]
    public string sliceableLayerName = "Sliceable";

    [Header("Blade geometry (local axes)")]
    public Vector3 bladeAxis = Vector3.forward;
    public Vector3 bladeLateral = Vector3.right;

    [Header("Cut behavior")]
    public Material crossSectionMaterial;
    public GameObject slashHitPrefab;
    public ParticleSystem slashShinePrefab;
    public float minVelocityForCut = 0.5f;
    public float sameTargetCooldown = 0.12f;

    [Header("Physics for pieces")]
    public float pieceExplosionForce = 60f;
    public float pieceExplosionRadius = 0.6f;
    public LayerMask sliceableLayers = ~0;

    [Header("Piece safety (prevents immediate re-slicing)")]
    public int temporaryNonSliceableLayer = 2;
    public float pieceNonSliceDuration = 0.25f;

    [Header("Lifetime")]
    public float slicedPieceLifetime = 5f;

    Vector3 lastPosition;
    Rigidbody bladeRb;
    Dictionary<GameObject, float> lastSliceTime = new Dictionary<GameObject, float>();
    [SerializeField] private GameSceneController sceneController;
    
    public AudioSource audioSource;

    class PieceState
    {
        public GameObject piece;
        public int originalLayer;
        public float restoreTime;
    }

    List<PieceState> activePieces = new List<PieceState>();

    void Start()
    {
        lastPosition = transform.position;
        bladeRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        lastPosition = transform.position;

        float now = Time.time;
        for (int i = activePieces.Count - 1; i >= 0; i--)
        {
            PieceState ps = activePieces[i];
            if (ps == null || ps.piece == null)
            {
                activePieces.RemoveAt(i);
                continue;
            }

            if (now >= ps.restoreTime)
            {
                SetLayerRecursively(ps.piece, ps.originalLayer);
                activePieces.RemoveAt(i);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        TrySliceFromCollision(collision);
    }

    void OnTriggerEnter(Collider other)
    {
        TrySliceFromTrigger(other, transform.position, Vector3.zero);
    }

    void TrySliceFromCollision(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & sliceableLayers) == 0) return;

        Collider otherCol = collision.collider;
        if (otherCol == null) return;

        GameObject target = otherCol.gameObject;
        if (!MatchesLayer(target)) return;

        Vector3 contactPoint = collision.contacts[0].point;

        Vector3 bladeVel = Vector3.zero;
        if (bladeRb != null)
        {
            bladeVel = bladeRb.GetPointVelocity(contactPoint);
        }
        else
        {
            bladeVel = (transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 1e-6f);
        }

        TrySliceTarget(target, contactPoint, bladeVel);
    }

    void TrySliceFromTrigger(Collider other, Vector3 fallbackContact, Vector3 fallbackVel)
    {
        GameObject target = other.gameObject;

        if (((1 << target.layer) & sliceableLayers) == 0) return;
        if (!MatchesLayer(target)) return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        Vector3 contactPoint = fallbackContact;
        if (other.Raycast(ray, out hit, 1.5f))
            contactPoint = hit.point;

        Vector3 bladeVel = fallbackVel;
        if (bladeRb != null)
            bladeVel = bladeRb.GetPointVelocity(contactPoint);
        else
            bladeVel = (transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 1e-6f);

        TrySliceTarget(target, contactPoint, bladeVel);
    }

    bool MatchesLayer(GameObject go)
    {
        if (string.IsNullOrEmpty(sliceableLayerName)) return true;

        int desiredLayer = LayerMask.NameToLayer(sliceableLayerName);
        if (desiredLayer < 0) return true;

        return go.layer == desiredLayer;
    }

    void TrySliceTarget(GameObject target, Vector3 contactPoint, Vector3 bladeVelocity)
    {
        if (target == null) return;

        float now = Time.time;
        if (lastSliceTime.TryGetValue(target, out float t0))
        {
            if (now - t0 < sameTargetCooldown) return;
        }

        float speed = bladeVelocity.magnitude;
        if (speed < minVelocityForCut)
        {
        }

        Vector3 bladeDirWorld = transform.TransformDirection(bladeAxis).normalized;
        Vector3 bladeLatWorld = transform.TransformDirection(bladeLateral).normalized;

        Vector3 planeNormal = Vector3.zero;
        if (bladeVelocity.sqrMagnitude > 1e-6f)
        {
            planeNormal = Vector3.Cross(bladeDirWorld, bladeVelocity).normalized;
        }

        if (planeNormal.sqrMagnitude < 1e-5f)
        {
            planeNormal = Vector3.Cross(bladeDirWorld, transform.up);
            if (planeNormal.sqrMagnitude < 1e-5f)
                planeNormal = transform.up;
            planeNormal = planeNormal.normalized;
        }

        Vector3 planePoint = contactPoint;
        try
        {
            MeshFilter mf = target.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Bounds meshBoundsLocal = mf.sharedMesh.bounds;
                Vector3 localContact = target.transform.InverseTransformPoint(contactPoint);

                Vector3 localSize = meshBoundsLocal.size;
                int largestAxis = 0;
                if (localSize.y > localSize.x && localSize.y > localSize.z) largestAxis = 1;
                if (localSize.z > localSize.x && localSize.z > localSize.y) largestAxis = 2;

                float minAxis = 0f, sizeAxis = 1f, coord = 0f;
                if (largestAxis == 0) { minAxis = meshBoundsLocal.min.x; sizeAxis = meshBoundsLocal.size.x; coord = localContact.x; }
                else if (largestAxis == 1) { minAxis = meshBoundsLocal.min.y; sizeAxis = meshBoundsLocal.size.y; coord = localContact.y; }
                else { minAxis = meshBoundsLocal.min.z; sizeAxis = meshBoundsLocal.size.z; coord = localContact.z; }

                float portion = Mathf.Clamp01((coord - minAxis) / Mathf.Max(sizeAxis, 1e-6f));

                Vector3 meshWorldSize = Vector3.Scale(meshBoundsLocal.size, target.transform.lossyScale);
                float breadth = (largestAxis == 0) ? meshWorldSize.x : (largestAxis == 1) ? meshWorldSize.y : meshWorldSize.z;

                float offsetAmount = (portion - 0.5f) * breadth;
                planePoint = contactPoint + bladeLatWorld * offsetAmount;
            }
        }
        catch
        {
            planePoint = contactPoint;
        }

        SlicedHull hull = null;
        try
        {
            hull = target.Slice(planePoint, planeNormal, crossSectionMaterial);
        }
        catch (System.Exception)
        {
            return;
        }

        if (hull == null)
        {
            return;
        }

        lastSliceTime[target] = now;

        GameObject upper = hull.CreateUpperHull(target, crossSectionMaterial);
        GameObject lower = hull.CreateLowerHull(target, crossSectionMaterial);

        if (upper == null || lower == null)
        {
            return;
        }

        SetupPiece(upper, target);
        SetupPiece(lower, target);

        if (upper.TryGetComponent<Rigidbody>(out Rigidbody ur))
        {
            ur.AddExplosionForce(pieceExplosionForce, planePoint, pieceExplosionRadius);
        }
        if (lower.TryGetComponent<Rigidbody>(out Rigidbody lr))
        {
            lr.AddExplosionForce(pieceExplosionForce * 0.5f, planePoint, pieceExplosionRadius);
        }

        Destroy(upper, slicedPieceLifetime);
        Destroy(lower, slicedPieceLifetime);

        if (slashHitPrefab != null)
        {
            var go = Instantiate(slashHitPrefab, planePoint, Quaternion.LookRotation(planeNormal));
            Destroy(go, 3.5f);
        }

        if (slashShinePrefab != null)
        {
            var ps = Instantiate(slashShinePrefab, planePoint, Quaternion.LookRotation(planeNormal));
            Destroy(ps.gameObject, 3.5f);
        }
        
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }

        sceneController.Sum();
        Destroy(target);
        
    }

    void SetupPiece(GameObject piece, GameObject original)
    {
        piece.name = original.name + "_sliced";
        piece.tag = original.tag;

        int originalLayer = original.layer;

        piece.transform.position = original.transform.position;
        piece.transform.rotation = original.transform.rotation;
        piece.transform.localScale = original.transform.localScale;

        MeshCollider mc = piece.GetComponent<MeshCollider>();
        if (mc == null)
            mc = piece.AddComponent<MeshCollider>();

        mc.convex = true;

        Rigidbody rb = piece.GetComponent<Rigidbody>();
        if (rb == null)
            rb = piece.AddComponent<Rigidbody>();

        rb.mass = Mathf.Clamp(original.TryGetComponent<Rigidbody>(out var orb) ? orb.mass * 0.5f : 1f, 0.1f, 10f);

        SetLayerRecursively(piece, temporaryNonSliceableLayer);

        activePieces.Add(new PieceState { piece = piece, originalLayer = originalLayer, restoreTime = Time.time + pieceNonSliceDuration });
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
