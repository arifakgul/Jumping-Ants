using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeteorManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    [Header("Prefabs / Pool Tags")]
    [SerializeField] private string meteorTag = "Meteor";
    [SerializeField] private string warningTag = "Warning";

    [Header("Spawn Settings")]
    [SerializeField] private float warningDuration = 1.2f;
    [SerializeField] private float meteorSpawnAbovePlatform = 14f; // platformun üstünden spawn
    [SerializeField] private float meteorTriggerDistance = 3f;     // oyuncu bu kadar yaklaştığında meteor düşmeye başlar
    [SerializeField] private float warningOffsetAbovePlayer = 2f;  // oyuncunun üstünde gösterim mesafesi
    [SerializeField] private float meteorDelay = 0.25f;          // aynı row’daki meteorlar arası gecikme

    // Sorted Dictionary: rowY -> list of platform positions
    private SortedDictionary<float, List<Vector3>> pendingMeteors = new SortedDictionary<float, List<Vector3>>();

    void OnEnable()
    {
        PlatformSpawner.OnMeteorRowDetected += RegisterMeteor;
    }

    void OnDisable()
    {
        PlatformSpawner.OnMeteorRowDetected -= RegisterMeteor;
    }

    void Update()
    {
        if (pendingMeteors.Count == 0) return;

        // En alttaki meteor satırını kontrol et
        float firstRowY = pendingMeteors.Keys.Min();
        float distanceToRow = firstRowY - player.position.y;

        // Oyuncu yeterince yaklaştığında meteor spawn
        if (distanceToRow <= meteorTriggerDistance)
        {
            var positions = pendingMeteors[firstRowY];
            pendingMeteors.Remove(firstRowY);

            StartCoroutine(SpawnMeteorRowWithWarnings(positions));
        }
    }

    private void RegisterMeteor(float rowY, Vector3 platformPos)
    {
        if (!pendingMeteors.ContainsKey(rowY))
            pendingMeteors[rowY] = new List<Vector3>();

        pendingMeteors[rowY].Add(platformPos);
    }

    private IEnumerator SpawnMeteorRowWithWarnings(List<Vector3> positions)
    {
        foreach (var pos in positions)
        {
            // Warning pozisyonu — oyuncunun üstünde ve lane hizasında
            float camTop = cam.transform.position.y + cam.orthographicSize - 1f;
            float warnY = Mathf.Min(player.position.y + warningOffsetAbovePlayer, camTop);

            Vector3 warnPos = new Vector3(pos.x, warnY, 0f);
            GameObject warning = ObjectPool.Instance.SpawnFromPool(warningTag, warnPos, Quaternion.identity);

            // Meteor spawn’ı biraz gecikmeli
            StartCoroutine(SpawnMeteorAfterDelay(pos, warningDuration, warning));

            yield return new WaitForSeconds(meteorDelay);
        }
    }

    private IEnumerator SpawnMeteorAfterDelay(Vector3 platformPos, float delay, GameObject warning)
    {
        yield return new WaitForSeconds(delay);

        // Meteor spawn platformun üstünde
        Vector3 meteorPos = new Vector3(platformPos.x, platformPos.y + meteorSpawnAbovePlatform, 0f);
        ObjectPool.Instance.SpawnFromPool(meteorTag, meteorPos, Quaternion.identity);

        // Warning geri gönder
        if (warning != null)
            ObjectPool.Instance.ReturnToPool(warningTag, warning);
    }
}