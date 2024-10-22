using System;
using BerserkV3.Common.AudioSystem.Abstractions;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Applications
{
	public class VFXApplication : IVfxApplication
	{
		private readonly IVfxFactory vfxFactory;
		private readonly IAudioApplication audioApplication;
		private static IGameContainers gameContainers;

		public VFXApplication(
			IVfxFactory vfxFactory, 
			IGameContainers gameContainers,
			IAudioApplication audioApplication)
		{
			this.vfxFactory = vfxFactory;
			this.audioApplication = audioApplication;
			VFXApplication.gameContainers = gameContainers;
		}

		public UniTask PlaySingleVFXAsync(object id, params object[] args)
		{
			var vfx = SpawnVfx(id, args);
			return vfx.WaitForParticlesAsync();
		}

		public UniTask PlaySingleVFXAsync(object id, Vector3 position)
		{
			var vfx = SpawnVfx(id, position);
			return vfx.WaitForParticlesAsync();
		}

		public UniTask PlaySingleVFXAsync(object id, Transform parent)
		{
			var vfx = SpawnVfx(id, parent);
			return vfx.WaitForParticlesAsync();
		}

		public VFXView SpawnVfx(object id, params object[] args)
		{
			var vfx = vfxFactory.Create(id.ToString());
			vfx.SetArguments(args);
			audioApplication.PlaySound(vfx.AudioKeyword);
			return vfx;
		}

		public VFXView SpawnVfx(object id, Vector3 position)
		{
			var vfx = vfxFactory.Create(id.ToString());
			vfx.SetPosition(position);
			audioApplication.PlaySound(vfx.AudioKeyword);
			return vfx;
		}
		
		public VFXView SpawnVfx(object id, Transform parent = null)
		{
			var vfx = vfxFactory.Create(id.ToString());
			SetVfxParent(vfx, parent);
			audioApplication.PlaySound(vfx.AudioKeyword);
			return vfx;
		}
		
		public VFXView SpawnVfxToParent(object id, Transform parent = null)
		{
			parent = TryFixParent(parent);
			var vfx = vfxFactory.Create(id.ToString(), parent);
			vfx.SetPosition(GetPosition(parent));
			audioApplication.PlaySound(vfx.AudioKeyword);
			return vfx;
		}

		private static void SetVfxParent(VFXView vfx, Transform parent)
		{
			parent = TryFixParent(parent);
			if (vfx.AttachToSource)
				vfx.SetParent(parent)
					.SetLocalPosition(Vector3.zero);
			else
				vfx.SetPosition(GetPosition(parent));
		}

		private static Transform TryFixParent(Transform parent)
		{
			return parent.Value() ? parent : gameContainers?.VfxContainer;
		}

		private static Vector3 GetPosition(Transform parent, Space space = Space.World)
		{
			if(!parent.Value())
				return Vector3.zero;
			
			return space switch
			{
				Space.World => parent.position,
				Space.Self  => parent.localPosition,
				_ => throw new ArgumentOutOfRangeException(nameof(space), space, null)
			};
		}
	}
}