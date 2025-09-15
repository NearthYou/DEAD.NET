using System.Collections.Generic;
using UnityEngine;
using Hexamap;
using Cysharp.Threading.Tasks;

public class ParticleLODManager : MonoBehaviour
{
    [Header("LOD Settings")]
    [SerializeField] private float highQualityDistance = 10f;
    [SerializeField] private float mediumQualityDistance = 20f;
    [SerializeField] private float lowQualityDistance = 40f;
    
    [Header("Particle Settings")]
    [SerializeField] private bool enableParticleLOD = true;
    [SerializeField] private float updateInterval = 0.1f;
    
    public enum ParticleLODLevel
    {
        Disabled = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }
    
    [System.Serializable]
    public class ParticleInfo
    {
        public GameObject tileObject;
        public ParticleSystem[] particleSystems;
        public ParticleLODLevel currentLOD;
        public float distanceToPlayer;
        public bool isInSight;
        
        public ParticleInfo(GameObject tile, ParticleSystem[] particles)
        {
            tileObject = tile;
            particleSystems = particles;
            currentLOD = ParticleLODLevel.Disabled;
            distanceToPlayer = float.MaxValue;
            isInSight = false;
        }
    }
    
    private Dictionary<GameObject, ParticleInfo> particleCache = new Dictionary<GameObject, ParticleInfo>();
    private List<ParticleInfo> allParticles = new List<ParticleInfo>();
    
    private MapController mapController;
    private Player player;
    private Camera mainCamera;
    
    private float lastUpdateTime;
    private int currentUpdateIndex = 0;
    private int particlesPerFrame = 2;
    
    private HashSet<Tile> cachedSightTiles;
    private float lastSightUpdateTime;
    private float sightCacheInterval = 0.5f;
    
    private void Start()
    {
        InitializeReferences();
        InitializeParticleSystemAsync().Forget();
    }
    
    private void InitializeReferences()
    {
        mapController = FindObjectOfType<MapController>();
        player = FindObjectOfType<Player>();
        mainCamera = Camera.main;
        
        if (mapController == null || player == null || mainCamera == null)
        {
            enabled = false;
        }
    }
    
    private async UniTask InitializeParticleSystemAsync()
    {
        await UniTask.WaitUntil(() => mapController.LoadingComplete);
        
        await UniTask.Delay(100);
        
        var allTiles = mapController.GetAllTiles();
                
        int processedCount = 0;
        int batchSize = 10;
        
        foreach (var tile in allTiles)
        {
            GameObject tileObject = (GameObject)tile.GameEntity;
            if (tileObject != null)
            {
                RegisterTileParticles(tileObject);
                processedCount++;
                
                if (processedCount % batchSize == 0)
                {
                    await UniTask.Yield();
                }
            }
        }        
    }
    
    public void RegisterTileParticles(GameObject tileObject)
    {
        if (particleCache.ContainsKey(tileObject))
            return;
            
        ParticleSystem[] particles = tileObject.GetComponentsInChildren<ParticleSystem>(true);
        
        if (particles.Length > 0)
        {
            ParticleInfo info = new ParticleInfo(tileObject, particles);
            particleCache[tileObject] = info;
            allParticles.Add(info);
            
            SetParticleLOD(info, ParticleLODLevel.Disabled);
        }
    }
    
    public void UnregisterTileParticles(GameObject tileObject)
    {
        if (particleCache.TryGetValue(tileObject, out ParticleInfo info))
        {
            allParticles.Remove(info);
            particleCache.Remove(tileObject);
        }
    }
    
    private void Update()
    {
        if (!enableParticleLOD || allParticles.Count == 0)
            return;
            
        if (Time.time - lastUpdateTime < updateInterval * 2)
            return;
            
        lastUpdateTime = Time.time;
        
        UpdateParticlesInBatches();
    }
    
    private void UpdateParticlesInBatches()
    {
        if (allParticles == null || allParticles.Count == 0)
            return;
            
        int endIndex = Mathf.Min(currentUpdateIndex + particlesPerFrame, allParticles.Count);
        
        for (int i = currentUpdateIndex; i < endIndex; i++)
        {
            if (allParticles[i] != null)
            {
                UpdateParticleLOD(allParticles[i]);
            }
        }
        
        currentUpdateIndex = endIndex;
        
        if (currentUpdateIndex >= allParticles.Count)
        {
            currentUpdateIndex = 0;
        }
    }
    
    private void UpdateParticleLOD(ParticleInfo info)
    {
        if (info == null || info.tileObject == null || player == null)
            return;
            
        if (info.tileObject.transform == null)
            return;
            
        float distance = Vector3.Distance(player.transform.position, info.tileObject.transform.position);
        info.distanceToPlayer = distance;
        
        info.isInSight = IsTileInPlayerSight(info.tileObject);
        
        ParticleLODLevel newLOD = DetermineLODLevel(distance, info.isInSight);
        
        if (newLOD != info.currentLOD)
        {
            SetParticleLOD(info, newLOD);
            info.currentLOD = newLOD;
        }
    }
    
    private bool IsTileInPlayerSight(GameObject tileObject)
    {
        if (mapController == null || tileObject == null)
            return false;
            
        UpdateSightCache();
        
        if (cachedSightTiles == null)
            return false;
        
        var tileController = tileObject.GetComponent<TileController>();
        if (tileController == null)
            return false;
            
        return cachedSightTiles.Contains(tileController.Model);
    }
    
    private void UpdateSightCache()
    {
        if (Time.time - lastSightUpdateTime < sightCacheInterval)
            return;
            
        lastSightUpdateTime = Time.time;
        
        var sightTiles = mapController.GetPlayerSightTilesForParticles();
        
        if (cachedSightTiles == null)
            cachedSightTiles = new HashSet<Tile>();
        else
            cachedSightTiles.Clear();
            
        if (sightTiles != null)
        {
            foreach (var tile in sightTiles)
            {
                cachedSightTiles.Add(tile);
            }
        }
    }
    
    private ParticleLODLevel DetermineLODLevel(float distance, bool inSight)
    {
        if (!inSight)
            return ParticleLODLevel.Disabled;
            
        if (distance <= highQualityDistance)
            return ParticleLODLevel.High;
        else if (distance <= mediumQualityDistance)
            return ParticleLODLevel.Medium;
        else if (distance <= lowQualityDistance)
            return ParticleLODLevel.Low;
        else
            return ParticleLODLevel.Disabled;
    }
    
    private void SetParticleLOD(ParticleInfo info, ParticleLODLevel lodLevel)
    {
        if (info.particleSystems == null)
            return;
            
        foreach (var particleSystem in info.particleSystems)
        {
            if (particleSystem == null)
                continue;
                
            switch (lodLevel)
            {
                case ParticleLODLevel.Disabled:
                    SetParticleSystemSettings(particleSystem, false, 0f, 0f, 0f);
                    break;
                    
                case ParticleLODLevel.Low:
                    SetParticleSystemSettings(particleSystem, true, 10f, 50f, 0.5f);
                    break;
                    
                case ParticleLODLevel.Medium:
                    SetParticleSystemSettings(particleSystem, true, 20f, 100f, 0.8f);
                    break;
                    
                case ParticleLODLevel.High:
                    SetParticleSystemSettings(particleSystem, true, 30f, 200f, 1.0f);
                    break;
            }
        }
    }
    
    private void SetParticleSystemSettings(ParticleSystem ps, bool enabled, float emissionRate, float maxParticles, float simulationSpeed)
    {
        var emission = ps.emission;
        var main = ps.main;
        
        if (enabled && !ps.isPlaying)
        {
            ps.Play();
        }
        else if (!enabled && ps.isPlaying)
        {
            ps.Stop();
        }
        
        // emissionRate를 직접 설정 (곱셈이 아닌 절대값)
        var rateOverTime = emission.rateOverTime;
        rateOverTime.constant = emissionRate;
        emission.rateOverTime = rateOverTime;
        
        // maxParticles를 직접 설정 (곱셈이 아닌 절대값)
        main.maxParticles = Mathf.RoundToInt(maxParticles);
        
        main.simulationSpeed = simulationSpeed;
    }
    
    /// <summary>
    /// 특정 타일의 파티클을 강제로 업데이트합니다.
    /// </summary>
    public void ForceUpdateTileParticles(GameObject tileObject)
    {
        if (particleCache.TryGetValue(tileObject, out ParticleInfo info))
        {
            UpdateParticleLOD(info);
        }
    }
    
    /// <summary>
    /// 모든 파티클의 LOD를 즉시 업데이트합니다.
    /// </summary>
    public void ForceUpdateAllParticles()
    {
        // 동기 버전 - 최소한의 배치 처리만 적용
        int batchSize = 50; // 한 번에 처리할 파티클 수
        
        for (int i = 0; i < allParticles.Count; i += batchSize)
        {
            int endIndex = Mathf.Min(i + batchSize, allParticles.Count);
            
            for (int j = i; j < endIndex; j++)
            {
                if (allParticles[j] != null)
                {
                    UpdateParticleLOD(allParticles[j]);
                }
            }
        }
    }
    
    /// <summary>
    /// 모든 파티클의 LOD를 비동기로 업데이트합니다.
    /// </summary>
    public async UniTask ForceUpdateAllParticlesAsync()
    {
        int batchSize = 20; // 한 번에 처리할 파티클 수
        
        for (int i = 0; i < allParticles.Count; i += batchSize)
        {
            int endIndex = Mathf.Min(i + batchSize, allParticles.Count);
            
            for (int j = i; j < endIndex; j++)
            {
                if (allParticles[j] != null)
                {
                    UpdateParticleLOD(allParticles[j]);
                }
            }
            
            // 프레임 분산을 위해 yield
            if (i + batchSize < allParticles.Count)
            {
                await UniTask.Yield();
            }
        }
    }
    
    /// <summary>
    /// 파티클 LOD 설정을 변경합니다.
    /// </summary>
    public void SetLODSettings(float highDist, float mediumDist, float lowDist)
    {
        highQualityDistance = highDist;
        mediumQualityDistance = mediumDist;
        lowQualityDistance = lowDist;
        
        ForceUpdateAllParticles();
    }
    
    /// <summary>
    /// 파티클 LOD 활성화 상태를 변경합니다.
    /// </summary>
    public void SetParticleLODEnabled(bool enabled)
    {
        enableParticleLOD = enabled;
        
        if (!enabled)
        {
            foreach (var info in allParticles)
            {
                SetParticleLOD(info, ParticleLODLevel.Disabled);
            }
        }
        else
        {
            ForceUpdateAllParticles();
        }
    }
    
    private void OnDestroy()
    {
        foreach (var info in allParticles)
        {
            if (info.particleSystems != null)
            {
                foreach (var ps in info.particleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop();
                    }
                }
            }
        }
    }
    
}
