using System;
using NicholaScott.BepInEx.Utils.Instancing;
using NicholaScott.LethalCompany.GlowSteps.UnityScripts;
using UnityEngine;
using UnityEngine.Pool;

namespace NicholaScott.LethalCompany.GlowSteps
{
	public class FootstepPool : ObjectPool<GlowingFootstep>
	{
		public FootstepPool()
			: base(new Func<GlowingFootstep>(FootstepPool.CreateNewFootstep), delegate(GlowingFootstep fs)
			{
				fs.gameObject.SetActive(true);
			}, delegate(GlowingFootstep fs)
			{
				fs.gameObject.SetActive(false);
			}, new Action<GlowingFootstep>(FootstepPool.DestroyFootstep), true, 10, 10000)
		{
		}

		private static GlowingFootstep CreateNewFootstep()
		{
			Singleton<GlowSteps>.Logger.LogInfo(string.Format("Creating new footstep object in pool. Object count {0} ", Singleton<GlowSteps>.Instance.footyManager.PooledObjects.CountAll) + string.Format("& active {0}", Singleton<GlowSteps>.Instance.footyManager.PooledObjects.CountActive));
			GameObject gameObject = new GameObject("Glow Step", new Type[] { typeof(GlowingFootstep) });
			gameObject.transform.SetParent(Singleton<GlowSteps>.Instance.footyManager.transform);
			return gameObject.GetComponent<GlowingFootstep>();
		}

		private static void DestroyFootstep(GlowingFootstep footstep)
		{
			Singleton<GlowSteps>.Logger.LogInfo(string.Format("Destroying footstep object in pool. Object count {0} ", Singleton<GlowSteps>.Instance.footyManager.PooledObjects.CountAll) + string.Format("& active {0}", Singleton<GlowSteps>.Instance.footyManager.PooledObjects.CountActive));
			UnityEngine.Object.Destroy(footstep.gameObject);
		}
	}
}
