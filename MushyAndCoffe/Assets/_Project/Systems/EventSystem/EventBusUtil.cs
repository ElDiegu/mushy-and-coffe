using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EventSystem
{
	public static class EventBusUtil
	{
		public static IReadOnlyList<Type> EventTypes { get; set; }
		public static IReadOnlyList<Type> EventBusTypes { get; set; }

#if UNITY_EDITOR
		public static PlayModeStateChange PlayModeState { get; set; }
		
		[InitializeOnLoadMethod]
		public static void InitializeEditor() 
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			PlayModeState = state;
			if (state == PlayModeStateChange.ExitingPlayMode) 
			{
				ClearAllBuses();
			}
		}
#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void Initialize()
		{
			EventTypes = PredefinedAssemblyUtil.GetTypes(typeof(IEvent));
			EventBusTypes = InitializeAllBuses();
		}

		private static IReadOnlyList<Type> InitializeAllBuses()
		{
			List<Type> eventBusTypes = new List<Type>();

			var typedef = typeof(EventBus<>);

			foreach (var eventType in EventTypes)
			{
				var busType = typedef.MakeGenericType(eventType);
				eventBusTypes.Add(busType);
				Debug.Log($"Intialized EventBus<{eventType.Name}>");
			}

			return eventBusTypes;
		}

		public static void ClearAllBuses()
		{
			Debug.Log("Clearing all buses...");
			for (int i = 0; i < EventBusTypes.Count; i++)
			{
				var busType = EventBusTypes[i];
				var clearMethod = busType.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
				clearMethod.Invoke(null, null);
			}
		}
	}
}
