using MarkLight.Views.UI;
using MarkLight;

public class MainMenu : MobileView {

    public int menuViewWidth;
    public int menuViewHeight;
    public int menuBarWidth;
    public int menuBarHeight;
    public int regionSize;

    public ElementSize GetElementWidth(int elementCount) {
        return MobileView.getElementHeight(elementCount);
    }

    public ElementSize GetElementHeight(int elementCount) {
        return MobileView.getElementHeight(elementCount);
    }
}
