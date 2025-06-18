using System;
using System.Reflection;
using BepInEx;
using GorillaLocomotion;
using GorillaTag.Dev.Benchmarks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;

namespace ReBench;

[BepInPlugin(Plugin_Info.GUID, Plugin_Info.NAME, Plugin_Info.VERSION)]
public class Plugin : BaseUnityPlugin
{
    private VisualBenchmark _benchmark;
    private GameObject _benchmarkGameObject;
    private Camera _cam;
    
    public void Awake() => GorillaTagger.OnPlayerSpawned(Init);

    private void Init()
    {
        if (XRSettings.isDeviceActive) return;
        
        _benchmarkGameObject =  new GameObject();
        _benchmark = _benchmarkGameObject.AddComponent<VisualBenchmark>();
        _benchmark.benchmarkLocations = FindObjectOfType<SpawnManager>().ChildrenXfs();
        _benchmark.enabled = true;
        
        AccessTools.Field(typeof(VisualBenchmark), "cam").SetValue(_benchmark, _cam);
        AccessTools.Field(typeof(VisualBenchmark), "isQuitting").SetValue(_benchmark, false);
        _cam = Camera.main;//_benchmarkGameObject.AddComponent<Camera>();
        
        GorillaTagger.Instance.thirdPersonCamera.SetActive(false);
    }

    private void UnInit() => Destroy(_benchmarkGameObject);

    private void Update()
    {
        if (!_cam || !_benchmark || !_benchmark.enabled) return;
        
        int index = (int)AccessTools.Field(typeof(VisualBenchmark), "currentLocationIndex").GetValue(_benchmark);

        if (_cam.transform.position == _benchmark.benchmarkLocations[index].position) return;

        if (_benchmark.benchmarkLocations[index].TryGetComponent(out SpawnPoint spawnPoint))
        {
            //_cam.transform.position = _benchmark.benchmarkLocations[index].position;
            _cam.transform.rotation = _benchmark.benchmarkLocations[index].rotation;
            
            GTPlayer.Instance.TeleportTo(_benchmark.benchmarkLocations[index].position, _benchmark.benchmarkLocations[index].rotation);
            
            ZoneManagement.SetActiveZone(spawnPoint.startZone);
        }
    }
}
