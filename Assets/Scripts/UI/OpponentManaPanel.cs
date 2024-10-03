using RR.UI.FrameSystem;
using UnityEngine;

namespace UI
{
    public partial class OpponentManaPanel : BaseView
    {
        public void VisualizeMana(int mana, int max)
        {
            ManaValueTxt.SetText($"{Mathf.Max(0, mana)}/{max}");
        }
    }
}