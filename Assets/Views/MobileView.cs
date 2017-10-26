using MarkLight.Views.UI;
using MarkLight;
using UnityEngine;

public class MobileView : UIView {

    protected const int MAX_ELEMENT_WIDTH = 12;
    protected const int MAX_ELEMENT_HEIGHT = 12;

    protected int screenWidth {
        get { return Screen.width; }
    }
    protected int screenHeight {
        get { return Screen.height; }
    }

    public int MobileWidth = 0;
    public int MobileHeight = 0;

    public override void InitializeInternal() {
        base.InitializeInternal();
        if (MobileWidth > 0) {
            WIDTH = new ElementSize();
            WIDTH.Unit = ElementSizeUnit.Pixels;
            WIDTH.Value = Mathf.Min(MobileWidth, MAX_ELEMENT_WIDTH) * screenWidth / MAX_ELEMENT_WIDTH;
            Width.Value = WIDTH;
        } else {
            WIDTH = Width.Value;
        }
        if (MobileHeight > 0) {
            HEIGHT = new ElementSize();
            HEIGHT.Unit = ElementSizeUnit.Pixels;
            HEIGHT.Value = Mathf.Min(MobileHeight, MAX_ELEMENT_HEIGHT) * screenHeight / MAX_ELEMENT_HEIGHT;
            Height.Value = HEIGHT;
        } else {
            HEIGHT = Height.Value;
        }
    }

    public ElementSize ElementWidth {
        get {
            return WIDTH;
        }
        set { }
    }

    public ElementSize ElementHeight {
        get {
            return HEIGHT;
        }
        set { }
    }
    private ElementSize WIDTH;
    private ElementSize HEIGHT;
}
