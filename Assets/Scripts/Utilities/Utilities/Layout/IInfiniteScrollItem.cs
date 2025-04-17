
namespace Utilities.Layout
{
    public interface IInfiniteScrollItem
    {
        public void OnSelected();
        public void OnDeselected();
        public void OnHover();
        public void OnUnHover();

        public void ListenTo(IInfiniteScrollItem clone);
    }
}
