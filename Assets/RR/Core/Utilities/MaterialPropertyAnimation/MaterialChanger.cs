using RR.Core.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RR.Core.Utilities.MaterialPropertyAnimation
{
	public class MaterialChanger : MeshRenderChanger
	{
		[SerializeField] private UnitySerializedDictionary<string, ColorHdr> colorValues = new();
		[SerializeField] private UnitySerializedDictionary<string, float> digitalValues = new();
		[SerializeField] private UnitySerializedDictionary<string, Vector4> vector4Values = new();
		[SerializeField] private UnitySerializedDictionary<string, Texture> textureValues = new();

		private UnitySerializedDictionary<string, ColorHdr> defaultColorValues = new();
		private UnitySerializedDictionary<string, float> defaultDigitalValues = new();
		private UnitySerializedDictionary<string, Vector4> defaultVector4Values = new();
		private UnitySerializedDictionary<string, Texture> defaultTextureValues = new();

		private void Awake()
		{
			foreach (var colorValue in colorValues)
				defaultColorValues.Add(colorValue.Key, colorValue.Value);

			foreach (var digitalValue in digitalValues)
				defaultDigitalValues.Add(digitalValue.Key, digitalValue.Value);

			foreach (var vector4Value in vector4Values)
				defaultVector4Values.Add(vector4Value.Key, vector4Value.Value);

			foreach (var textureValue in textureValues)
				defaultTextureValues.Add(textureValue.Key, textureValue.Value);
		}

		[Button]
		public void ResetDefaultValues()
		{
			MeshRenderers.ForEach(meshRenderer =>
			{
				foreach (var colorValue in defaultColorValues)
					meshRenderer.material.SetColor(colorValue.Key, colorValue.Value.Color);

				foreach (var digitalValue in defaultDigitalValues)
					meshRenderer.material.SetFloat(digitalValue.Key, digitalValue.Value);

				foreach (var vector4Value in defaultVector4Values)
					meshRenderer.material.SetVector(vector4Value.Key, vector4Value.Value);

				foreach (var textureValue in defaultTextureValues)
					meshRenderer.material.SetTexture(textureValue.Key, textureValue.Value);
			});
		}

		[Button]
		public void ReplaceCustomValues()
		{
			MeshRenderers.ForEach(meshRenderer =>
			{
				foreach (var colorValue in colorValues)
					meshRenderer.material.SetColor(colorValue.Key, colorValue.Value.Color);

				foreach (var digitalValue in digitalValues)
					meshRenderer.material.SetFloat(digitalValue.Key, digitalValue.Value);

				foreach (var vector4Value in vector4Values)
					meshRenderer.material.SetVector(vector4Value.Key, vector4Value.Value);

				foreach (var textureValue in textureValues)
					meshRenderer.material.SetTexture(textureValue.Key, textureValue.Value);
			});
		}

		private void OnEnable()
		{
			ReplaceCustomValues();
		}

		private void OnDisable()
		{
			ResetDefaultValues();
		}
	}
}