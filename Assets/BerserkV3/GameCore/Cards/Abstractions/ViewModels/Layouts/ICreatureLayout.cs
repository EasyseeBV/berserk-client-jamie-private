using System.Threading;
using Berserk.Shared.Data.Enums;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.Cards
{
	public interface ICreatureLayout : IRuntimeLayout
	{
		bool IsAllowedExternal { get; set; }
		bool IsHealthVisible { get; }
		bool IsArmorVisible { get; }
		bool IsAttackVisible { get; }
		
		void SetActiveArmor(bool value);

		void SetArmor(int value);

		void SetActiveHealth(bool value);

		void SetHealth(int value);

		void SetActiveAttack(bool value);

		void SetAttack(int value);

		UniTask AddFaceAsync(FaceId faceId, CancellationToken token = default);
		UniTask RemoveFaceAsync(FaceId faceId);
	}
}