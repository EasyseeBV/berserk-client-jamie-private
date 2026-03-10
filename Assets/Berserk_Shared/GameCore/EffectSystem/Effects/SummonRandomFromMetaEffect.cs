using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.Models;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.SummonRandomFromMeta)]
    public class SummonRandomFromMetaEffect : KeywordEffect
    {
        private static readonly Random RANDOM = new();

        public override bool CanExecute()
        {
            return base.CanExecute() && ValueModRounded() > 0;
        }

        protected override void OnExecute()
        {
            if (string.IsNullOrEmpty(EffectData.Meta))
                return;
            
            var ids = EffectData.Meta.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(id => id.Trim())
                                     .ToArray();

            if (ids.Length == 0)
                return;
            
            var chosenId = ids[RANDOM.Next(ids.Length)];

            var cardData = GameContext.GameDatabase.GetCard(chosenId);
            if (cardData == null)
                return;

            foreach (var target in Targets)
            {
                var summonCount = ValueModRounded();
                for (var i = 0; i < summonCount; i++)
                {
                    var runtimeCard = LogicContext.RuntimeFactory.CreateRuntimeCard(cardData.Id, target.RuntimeData.OwnerUserId);
                    if (runtimeCard == null)
                        continue;

                    var playModel = new CmdParamsModel(GameContext.Timer.RuntimeData.TimeHash, new PlayCardArgs())
                    {
                        ExecutorObjectId = runtimeCard.RuntimeData.Id
                    };
                    LogicContext.CommandController.Execute<ApprovedPlayCardCmd>(
                        target.RuntimeData.OwnerUserId,
                        playModel,
                        true
                    );
                }
            }
        }
    }
}