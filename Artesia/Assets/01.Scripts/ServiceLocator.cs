using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 서비스 등록/조회를 담당하는 경량 ServiceLocator.
///
/// [실제 게임]
///   각 Manager의 Awake()에서 ServiceLocator.Register(this) 로 등록합니다.
///
/// [테스트 씬]
///   Manager 없이도 동작합니다. 서비스가 등록되지 않은 경우
///   자동으로 Null 구현체(NullXxx)를 반환하므로 에러 없이 실행됩니다.
///   필요하다면 TestBootstrap 같은 MonoBehaviour에서 직접 등록해도 됩니다.
/// </summary>
public static class ServiceLocator
{
    static readonly Dictionary<Type, object> _registry = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        _registry[typeof(T)] = service;
        Debug.Log($"[ServiceLocator] Registered: {typeof(T).Name}");
    }

    public static T Get<T>() where T : class
    {
        if (_registry.TryGetValue(typeof(T), out var service))
            return service as T;

        // 등록된 서비스가 없으면 Null 구현체로 폴백 (테스트 씬 등에서 에러 방지)
        return GetNullImplementation<T>();
    }

    static T GetNullImplementation<T>() where T : class
    {
        if (typeof(T) == typeof(ITurnSystem))    return new NullTurnSystem()    as T;
        if (typeof(T) == typeof(IBattleLog))     return new NullBattleLog()     as T;
        if (typeof(T) == typeof(IUIDamageNotifier)) return new NullUIDamageNotifier() as T;
        if (typeof(T) == typeof(IEnemyRegistry)) return new NullEnemyRegistry() as T;

        Debug.LogWarning($"[ServiceLocator] No registration or fallback for: {typeof(T).Name}");
        return null;
    }

    /// <summary>씬 전환 등에서 필요 시 초기화</summary>
    public static void Clear() => _registry.Clear();
}
