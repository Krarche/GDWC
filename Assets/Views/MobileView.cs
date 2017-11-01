using MarkLight.Views.UI;
using MarkLight;
using UnityEngine;

public class MobileView : UIView {

    protected const int MAX_ELEMENT_WIDTH = 12;
    protected const int MAX_ELEMENT_HEIGHT = 12;

    protected static int screenWidth {
        get { return Screen.width; }
    }
    protected static int screenHeight {
        get { return Screen.height; }
    }

    public int MobileWidth = 0;
    public int MobileHeight = 0;

    public override void InitializeInternal() {
        base.InitializeInternal();
        if (MobileWidth > 0) {
            Width.Value = getElementWidth(MobileWidth);
        }
        if (MobileHeight > 0) {
            Height.Value = getElementWidth(MobileHeight);
        }
    }

    public static ElementSize getElementWidth(int elementCount) {
        ElementSize WIDTH = new ElementSize();
        WIDTH.Unit = ElementSizeUnit.Pixels;
        WIDTH.Value = Mathf.Min(elementCount, MAX_ELEMENT_WIDTH) * screenWidth / MAX_ELEMENT_WIDTH;
        return WIDTH;
    }

    public static ElementSize getElementHeight(int elementCount) {
        ElementSize HEIGHT = new ElementSize();
        HEIGHT.Unit = ElementSizeUnit.Pixels;
        HEIGHT.Value = Mathf.Min(elementCount, MAX_ELEMENT_HEIGHT) * screenHeight / MAX_ELEMENT_HEIGHT;
        return HEIGHT;
    }
}
