using MyBox;
using UnityEngine;

public class FPSCounter : MonoBehaviour {
  public bool EditorOnly;

  [Separator]
  [SerializeField] private float _updateInterval = 1f;
  [SerializeField] private int _targetFrameRate = 30;

#pragma warning disable 0649
  [Separator]
  [SerializeField] private Anchor _anchor;
  [SerializeField] private int _xOffset;
  [SerializeField] private int _yOffset;

  [Separator]
  [SerializeField] private Color goodColor = new Color(.4f, .6f, .4f);
  [SerializeField] private Color okColor = new Color(.8f, .8f, .2f, .6f);
  [SerializeField] private Color badColor = new Color(.8f, .6f, .6f);

  [Separator]
  [SerializeField] private Font font;
  [SerializeField] private int fontSize = 14;
#pragma warning restore 0649

  /// <summary>
  /// Skip some time at start to skip performance drop on game start
  /// and produce more accurate Avg FPS
  /// </summary>
  private float _idleTime = 2;

  private float _elapsed;
  private int _frames;
  private int _quantity;
  private float _fps;
  private float _averageFps;

  private float _okFps;
  private float _badFps;

  private Rect _rect1;
  private Rect _rect2;


  private void Awake() {
    if (EditorOnly && !Application.isEditor) return;

    var percent = _targetFrameRate / 100;
    var percent10 = percent * 10;
    var percent40 = percent * 40;
    _okFps = _targetFrameRate - percent10;
    _badFps = _targetFrameRate - percent40;

    var xPos = 0;
    var yPos = 0;
    var linesHeight = 40 * (fontSize / 14);
    var linesWidth = 90 * (fontSize / 14);
    if (_anchor == Anchor.LeftBottom || _anchor == Anchor.RightBottom) yPos = Screen.height - linesHeight;
    if (_anchor == Anchor.RightTop || _anchor == Anchor.RightBottom) xPos = Screen.width - linesWidth;
    xPos += _xOffset;
    yPos += _yOffset;
    var yPos2 = yPos + 18 * (fontSize / 14);
    _rect1 = new Rect(xPos, yPos, linesWidth, linesHeight);
    _rect2 = new Rect(xPos, yPos2, linesWidth, linesHeight);

    _elapsed = _updateInterval;
  }

  private void Update() {
    if (EditorOnly && Application.isEditor) return;

    if (_idleTime > 0) {
      _idleTime -= Time.deltaTime;
      return;
    }

    _elapsed += Time.deltaTime;
    ++_frames;

    if (_elapsed >= _updateInterval) {
      _fps = _frames / _elapsed;
      _elapsed = 0;
      _frames = 0;
    }

    _quantity++;
    _averageFps += (_fps - _averageFps) / _quantity;
  }

  private void OnGUI() {
    if (EditorOnly && Application.isEditor) return;

    GUI.skin.label.font = GUI.skin.button.font = GUI.skin.box.font = font;
    GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = fontSize;

    var defaultColor = GUI.color;
    var color = goodColor;
    if (_fps <= _okFps || _averageFps <= _okFps) color = okColor;
    if (_fps <= _badFps || _averageFps <= _badFps) color = badColor;
    GUI.color = color;
    GUI.Label(_rect1, "FPS: " + (int)_fps);
    GUI.Label(_rect2, "Avg FPS: " + (int)_averageFps);
    GUI.color = defaultColor;
  }

  private enum Anchor {
    LeftTop, LeftBottom, RightTop, RightBottom
  }
}
