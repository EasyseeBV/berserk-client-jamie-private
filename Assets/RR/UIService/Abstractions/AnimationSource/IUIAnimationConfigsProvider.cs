using System.Collections.Generic;
using System.Threading.Tasks;

namespace RR.UIService.AnimationSource
{
    public interface IUIAnimationConfigsProvider
    {
        IEnumerable<IUIAnimationConfig> LoadAll();
        Task<IEnumerable<IUIAnimationConfig>> LoadAllAsync();
    }
}