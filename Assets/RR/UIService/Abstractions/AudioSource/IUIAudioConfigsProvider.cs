using System.Collections.Generic;
using System.Threading.Tasks;

namespace RR.UIService.AudioSource
{
    public interface IUIAudioConfigsProvider
    {
        IEnumerable<IUIAudioConfig> LoadAll();
        Task<IEnumerable<IUIAudioConfig>> LoadAllAsync();
    }
}