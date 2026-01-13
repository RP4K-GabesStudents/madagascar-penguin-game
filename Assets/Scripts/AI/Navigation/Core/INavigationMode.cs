using System.Collections;

namespace AI.Navigation.Core
{
    public interface INavigationMode
    {
        public IEnumerator ExecuteState();
        
    }
}
