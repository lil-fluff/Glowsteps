using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using NicholaScott.BepInEx.Utils.Configuration;
using NicholaScott.BepInEx.Utils.Instancing;
using NicholaScott.BepInEx.Utils.Patching;
using UnityEngine;

namespace NicholaScott.LethalCompany.GlowSteps
{
	[BepInDependency("NicholaScott.BepInEx.Utils", "1.2.0")]
	[BepInPlugin("NicholaScott.LethalCompany.GlowSteps", "Glow Steps", "1.1.2")]
	public class GlowSteps : BaseUnityPlugin
	{
		public void Awake()
		{
			Singleton<GlowSteps>.Instance = this;
			Singleton<GlowSteps, GlowSteps.Configuration>.Configuration = BepInEx.Utils.Configuration.Extensions.BindStruct<GlowSteps.Configuration>(base.Config, new GlowSteps.Configuration
			{
				DistanceFalloff = 20f,
				SecondsUntilFade = 60f,
				InFactory = true,
				Color = new Vector3(0.1f, 1f, 0.1f),
				UpdateRate = 0.1f
			});
			Vector2Int vector2Int = new Vector2Int(512, 512);
			this.LoadResource("LHeavy", vector2Int);
			this.LoadResource("LMedium", vector2Int);
			this.LoadResource("LLight", vector2Int);
			this.LoadResource("RHeavy", vector2Int);
			this.LoadResource("RMedium", vector2Int);
			this.LoadResource("RLight", vector2Int);
            BepInEx.Utils.Patching.Extensions.PatchAttribute<Production>(Assembly.GetExecutingAssembly(), base.Info.Metadata.GUID, new Action<object>(base.Logger.LogInfo));
		}

		private void LoadResource(string resourceName, Vector2Int dimensions)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string text = executingAssembly.GetName().Name + ".Images." + resourceName + ".png";
			Texture2D texture2D = new Texture2D(dimensions.x, dimensions.y);
			texture2D.LoadImage(BepInEx.Utils.Resources.Extensions.ReadAllBytes(executingAssembly.GetManifestResourceStream(text)));
			this.FootstepTexts[resourceName] = texture2D;
		}

		public readonly Dictionary<string, Texture2D> FootstepTexts = new Dictionary<string, Texture2D>();

		public FootstepManager footyManager;

		public struct Configuration
		{
			[ConfigEntryDefinition(Description = "The distance until footsteps are no longer visible.")]
			public float DistanceFalloff;

			public float SecondsUntilFade;

			[ConfigEntryDefinition(Description = "The rate to update footsteps. This should be kept <= 0.1")]
			public float UpdateRate;

			[ConfigEntryDefinition(Description = "Whether the footsteps show up only in the factory or outside as well.")]
			public bool InFactory;

			[ConfigEntryDefinition(Description = "Three normalized(0-1) numbers representing an RGB value.")]
			public Vector3 Color;
		}
	}
}
