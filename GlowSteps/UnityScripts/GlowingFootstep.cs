using System;
using NicholaScott.BepInEx.Utils.Instancing;
using Unity.Netcode;
using UnityEngine;

namespace NicholaScott.LethalCompany.GlowSteps.UnityScripts
{
	public class GlowingFootstep : MonoBehaviour
	{
		public void Awake()
		{
			this._materialReference = GlowingFootstep.CreateNewMaterial();
			this._subPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
			this._subPlane.GetComponent<Renderer>().material = this._materialReference;
			this._subPlane.GetComponent<Collider>().enabled = false;
			this._subPlane.transform.SetParent(base.transform, false);
			this._subPlane.transform.localScale = Vector3.one / 15f;
		}

		public void SyncAll(GlowingFootstep.Data footstepData)
		{
			this.SyncTransform(footstepData);
			this.SyncMaterial(footstepData);
			this.SyncColorIntensity(footstepData);
		}

		private void SyncTransform(GlowingFootstep.Data footstepData)
		{
			Transform transform = base.transform;
			transform.position = footstepData.Position;
			transform.rotation = footstepData.Rotation;
		}

		private void SyncMaterial(GlowingFootstep.Data footstepData)
		{
			string text = (footstepData.LeftFoot ? "L" : "R") + ((footstepData.Strength >= 3) ? "Heavy" : ((footstepData.Strength >= 2) ? "Medium" : "Light"));
			Texture2D texture2D = Singleton<GlowSteps>.Instance.FootstepTexts[text];
			this._materialReference.SetTexture(GlowingFootstep.MainTex, texture2D);
			this._materialReference.SetTexture(GlowingFootstep.BaseColorMap, texture2D);
		}

		public void SyncColorIntensity(GlowingFootstep.Data footstepData)
		{
			float num = Mathf.Clamp(footstepData.TimeLeftAlive, 0f, 30f) / 30f;
			GlowSteps.Configuration configuration = Singleton<GlowSteps, GlowSteps.Configuration>.Configuration;
			num = Mathf.Min(num, Mathf.Pow(Mathf.Clamp(2f - footstepData.LastDistance / configuration.DistanceFalloff, 0f, 1f), 4f));
			this._subPlane.GetComponent<Renderer>().material.SetVector(GlowingFootstep.EmissiveColor, new Vector4(footstepData.Color.x, footstepData.Color.y, footstepData.Color.z, 1f) * num);
		}

		private static Material CreateNewMaterial()
		{
			Material material = new Material(FootstepManager.CatwalkMaterial);
			material.SetTexture(GlowingFootstep.NormalMap, null);
			material.SetFloat(GlowingFootstep.EmissiveColorMode, 1f);
			material.SetFloat(GlowingFootstep.EmissiveExposureWeight, 1f);
			material.mainTextureScale = Vector2.one;
			material.color = Color.white;
			return material;
		}

		private Material _materialReference;

		private GameObject _subPlane;

		private static readonly int EmissiveExposureWeight = Shader.PropertyToID("_EmissiveExposureWeight");

		private static readonly int EmissiveColorMode = Shader.PropertyToID("_EmissiveColorMode");

		private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");

		private static readonly int BaseColorMap = Shader.PropertyToID("_BaseColorMap");

		private static readonly int NormalMap = Shader.PropertyToID("_NormalMap");

		private static readonly int MainTex = Shader.PropertyToID("_MainTex");

		public class Data : INetworkSerializable
		{
			public void UpdateFootstepData(float delta)
			{
				bool flag = GameNetworkManager.Instance.localPlayerController != null;
				if (flag)
				{
					this.LastDistance = Vector3.Distance(this.Position, GameNetworkManager.Instance.localPlayerController.playerEye.position);
					float distanceFalloff = Singleton<GlowSteps, GlowSteps.Configuration>.Configuration.DistanceFalloff;
					bool flag2 = this.LastDistance <= distanceFalloff;
					if (flag2)
					{
						this.ShouldDraw = true;
					}
					bool flag3 = this.LastDistance > distanceFalloff || this.TimeLeftAlive <= 0f;
					if (flag3)
					{
						this.ShouldDraw = false;
					}
				}
				else
				{
					this.ShouldDraw = false;
				}
				this.TimeLeftAlive -= delta;
			}

			public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
				serializer.SerializeValue(ref this.Position);
				serializer.SerializeValue(ref this.Rotation);
				serializer.SerializeValue<float>(ref this.TimeLeftAlive, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<int>(ref this.Strength, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<bool>(ref this.LeftFoot, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue(ref this.Color);
			}

			public static int SizeOf()
			{
				return 45;
			}

			public Vector3 Position;

			public Quaternion Rotation;

			public float TimeLeftAlive;

			public int Strength;

			public bool LeftFoot;

			public Vector3 Color;

			public bool ShouldDraw;

			public bool IsEnabled;

			public GlowingFootstep Linked;

			public float LastDistance;
		}
	}
}
