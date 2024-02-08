using System;
using System.Collections.Generic;
using NicholaScott.BepInEx.Utils.Instancing;
using NicholaScott.LethalCompany.GlowSteps.UnityScripts;
using UnityEngine;

namespace NicholaScott.LethalCompany.GlowSteps
{
	public class FootstepManager : MonoBehaviour
	{
		public static Material CatwalkMaterial
		{
			get
			{
				bool flag = FootstepManager._catwalkMaterial == null;
				if (flag)
				{
					GameObject gameObject = GameObject.Find("Environment/HangarShip/CatwalkShip");
					Material material;
					if (gameObject == null)
					{
						material = null;
					}
					else
					{
						Renderer component = gameObject.GetComponent<Renderer>();
						material = ((component != null) ? component.material : null);
					}
					FootstepManager._catwalkMaterial = material;
				}
				return FootstepManager._catwalkMaterial;
			}
		}

		public void Start()
		{
			base.InvokeRepeating("UpdateAllFootstepData", 1f, Singleton<GlowSteps, GlowSteps.Configuration>.Configuration.UpdateRate);
		}

		public void AddNewFootstep(GlowingFootstep.Data footstepData)
		{
			this.FootstepData.Add(footstepData);
		}

		public void UpdateAllFootstepData()
		{
			bool flag = FootstepManager.CatwalkMaterial == null;
			if (!flag)
			{
				for (int i = 0; i < this.FootstepData.Count; i++)
				{
					GlowingFootstep.Data data = this.FootstepData[i];
					data.UpdateFootstepData(Singleton<GlowSteps, GlowSteps.Configuration>.Configuration.UpdateRate);
					bool flag2 = data.IsEnabled && data.ShouldDraw;
					if (flag2)
					{
						data.Linked.SyncColorIntensity(data);
					}
					bool flag3 = !data.IsEnabled && data.ShouldDraw;
					if (flag3)
					{
						GlowingFootstep glowingFootstep = this.PooledObjects.Get();
						data.IsEnabled = true;
						data.Linked = glowingFootstep;
						glowingFootstep.SyncAll(data);
					}
					bool flag4 = data.IsEnabled && !data.ShouldDraw;
					if (flag4)
					{
						this.PooledObjects.Release(data.Linked);
						data.IsEnabled = false;
					}
					this.FootstepData[i] = data;
				}
				this.FootstepData.RemoveAll((GlowingFootstep.Data d) => !d.IsEnabled && !d.ShouldDraw && d.TimeLeftAlive <= 0f);
			}
		}

		public readonly List<GlowingFootstep.Data> FootstepData = new List<GlowingFootstep.Data>();

		public readonly FootstepPool PooledObjects = new FootstepPool();

		private static Material _catwalkMaterial;
	}
}
