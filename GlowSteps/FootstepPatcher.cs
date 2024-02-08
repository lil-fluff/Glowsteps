using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using NicholaScott.BepInEx.Utils.Instancing;
using NicholaScott.BepInEx.Utils.Patching;
using NicholaScott.LethalCompany.GlowSteps.UnityScripts;
using UnityEngine;

namespace NicholaScott.LethalCompany.GlowSteps
{
	[Production]
	public static class FootstepPatcher
	{
		[HarmonyPatch(typeof(Terminal), "Start")]
		[HarmonyPostfix]
		public static void CreateFootstepManager(Terminal __instance)
		{
			bool flag = Singleton<GlowSteps>.Instance.footyManager != null;
			if (!flag)
			{
				GameObject gameObject = new GameObject("Footstep Manager", new Type[] { typeof(FootstepManager) });
				Singleton<GlowSteps>.Instance.footyManager = gameObject.GetComponent<FootstepManager>();
				UnityEngine.Object.DontDestroyOnLoad(Singleton<GlowSteps>.Instance.footyManager.gameObject);
			}
		}

		[HarmonyPatch(typeof(PlayerControllerB), "PlayFootstepSound")]
		[HarmonyPrefix]
		public static void AddPositionToTracking(PlayerControllerB __instance)
		{
			GlowSteps.Configuration configuration = Singleton<GlowSteps, GlowSteps.Configuration>.Configuration;
			int num = (__instance.IsOwner ? (__instance.isSprinting ? 3 : ((__instance.isCrouching || __instance.isExhausted || __instance.isMovementHindered != 0) ? 1 : 2)) : 2);
			bool flag = __instance.IsOwner && !__instance.isSprinting;
			if (!flag)
			{
				bool flag2 = !__instance.isInsideFactory && configuration.InFactory;
				if (!flag2)
				{
					bool flag3 = !FootstepPatcher._leftFoots.ContainsKey(__instance.playerSteamId);
					if (flag3)
					{
						FootstepPatcher._leftFoots.Add(__instance.playerSteamId, -1f);
					}
					float num2 = FootstepPatcher._leftFoots[__instance.playerSteamId];
					Transform transform = __instance.transform;
					Ray ray = new Ray(transform.position + transform.right * 0.2f * num2, Vector3.down);
					RaycastHit raycastHit;
					bool flag4 = !Physics.Raycast(ray, out raycastHit, 10f, LayerMask.GetMask(new string[] { "Room", "Railing", "Default" }));
					if (!flag4)
					{
						Vector3 vector = new Vector3((__instance.playerSteamId & 16711680UL) >> 16, (__instance.playerSteamId & 65280UL) >> 8, __instance.playerSteamId & 255UL) / 255f;
						GlowingFootstep.Data data = new GlowingFootstep.Data
						{
							Color = (__instance.IsOwner ? configuration.Color : vector),
							LeftFoot = (num2 <= 0f),
							Strength = num,
							TimeLeftAlive = configuration.SecondsUntilFade,
							Position = raycastHit.point + new Vector3(0f, 0.001f, 0f),
							Rotation = Quaternion.LookRotation(__instance.transform.forward * -1f, raycastHit.normal)
						};
						Singleton<GlowSteps>.Instance.footyManager.AddNewFootstep(data);
						Dictionary<ulong, float> leftFoots = FootstepPatcher._leftFoots;
						ulong playerSteamId = __instance.playerSteamId;
						leftFoots[playerSteamId] *= -1f;
					}
				}
			}
		}

		private static Dictionary<ulong, float> _leftFoots = new Dictionary<ulong, float>();
	}
}
