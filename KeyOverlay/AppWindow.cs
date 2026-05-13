using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;

namespace KeyOverlay
{
    public class AppWindow
    {
        public static AppWindow instance;

        // Dragging / position lock
        private bool _isDragging;
        private Vector2i _dragStartPos;
        private Vector2i _windowStartPos;
        private bool _isPositionLocked;
        private bool _isWindowVisible = true;
        private bool _exitRequested;

        // System tray
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _lockMenuItem;
        private ToolStripMenuItem _showHideMenuItem;
        private ToolStripMenuItem _osuModeMenuItem;
        private ToolStripMenuItem _maniaModeMenuItem;

        private readonly RenderWindow _window;
        private readonly List<Key> _keyList = new();
        private readonly List<RectangleShape> _squareList;
        private readonly float _barSpeed;
        private readonly float _barMaxHeight;
        private readonly float _barFadeDistance;
        private readonly float _ratioX;
        private readonly float _ratioY;
        private readonly int _outlineThickness;
        private readonly Color _backgroundColor;
        private readonly Color _keyBackgroundColor;
        private readonly Color _barColor;
        private readonly Color _fontColor;
        private readonly Color _pressFontColor;
        private readonly Sprite _background;
        private readonly bool _fading;
        private readonly bool _counter;
        private readonly List<Drawable> _staticDrawables = new();
        private readonly List<Text> _keyText = new();
        private readonly uint _maxFPS;
        private readonly int _keySize;
        private readonly int _margin;
        private Clock _clock = new();
        public int defaultKeySize;
        public int minKeySize;
        public Color defaultBorderColor;
        public int sizeFrames;
        public int sizeDiff;
        public int sizeSteps;
        public int barOffsetY;
        public float barScaleMult;

        public AppWindow(string configFileName)
        {
            instance = this;
            var config = ReadConfig(configFileName);
            var windowWidth = config["windowWidth"];
            var windowHeight = config["windowHeight"];
            var windowPosX = config["windowPosX"];
            var windowPosY = config["windowPosY"];
             sizeFrames = int.Parse(config["sizeFrames"]);
             barOffsetY = int.Parse(config["barOffsetY"]);
             barScaleMult = float.Parse(config["barScaleMult"]); 
            _window = new RenderWindow(new VideoMode(uint.Parse(windowWidth!), uint.Parse(windowHeight!)),
                "KeyOverlay", Styles.None);
            
            
            //calculate screen ratio relative to original program size for easy resizing
            _ratioX = float.Parse(windowWidth) / 480f;
            _ratioY = float.Parse(windowHeight) / 960f;
            _window.Position = new Vector2i(int.Parse(windowPosX), int.Parse(windowPosY));

            DWM_BLURBEHIND bb = new DWM_BLURBEHIND
            {
                dwFlags = DWM_BB.Enable | DWM_BB.BlurRegion,
                fEnable = true,
                hRgnBlur = DWM.CreateRectRgn(0, 0, -1, -1)
            };
            DWM.DwmEnableBlurBehindWindow(_window.SystemHandle, ref bb);

            DWM.SetClickThroughAble(_window.SystemHandle, false);

            // Keep the overlay above other windows.
            DWM.SetAlwaysOnTop(_window.SystemHandle, true);

            InitializeSysTray();

            _window.MouseButtonPressed += OnMouseButtonPressed;
            _window.MouseButtonReleased += OnMouseButtonReleased;

            _barSpeed = float.Parse(config["barSpeed"], CultureInfo.InvariantCulture);
            _barMaxHeight = ReadConfigFloat(config, "barMaxHeight", 180f);
            _barFadeDistance = ReadConfigFloat(config, "barFadeDistance", 320f);
            _outlineThickness = int.Parse(config["outlineThickness"]);
            _backgroundColor = CreateItems.CreateColor("0,0,0,0");
            _keyBackgroundColor = CreateItems.CreateColor(config["keyColor"]);
            _barColor = CreateItems.CreateColor(config["barColor"]);
            _maxFPS = uint.Parse(config["maxFPS"]);


            //get background image if in config
            if (config["backgroundImage"] != "")
                _background = new Sprite(new Texture(
                    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Resources",
                        config["backgroundImage"]))));

            //create keys which will be used to create the squares and text
            var keyAmount = int.Parse(config["keyAmount"]);
            for (var i = 1; i <= keyAmount; i++)
                try
                {
                    var key = new Key(config[$"key" + i]);
                    if (config.ContainsKey($"displayKey" + i))
                        if (config[$"displayKey" + i] != "")
                            key.KeyLetter = config[$"displayKey" + i];
                    _keyList.Add(key);
                }
                catch (InvalidOperationException e)
                {
                    //invalid key
                    Console.WriteLine(e.Message);
                    using var sw = new StreamWriter("keyErrorMessage.txt");
                    sw.WriteLine(e.Message);
                }

            //create squares and add them to _staticDrawables list
            var outlineColor = CreateItems.CreateColor(config["borderColor"]);
            _keySize = int.Parse(config["keySize"]);
            defaultBorderColor = outlineColor;
            defaultKeySize = _keySize;
            minKeySize = (int)(_keySize * float.Parse(config["keyPressShrinkMult"]));
            _margin = int.Parse(config["margin"]);
            _squareList = CreateItems.CreateKeys(keyAmount, _outlineThickness, _keySize, _ratioX, _ratioY, _margin,
                _window, _keyBackgroundColor, outlineColor);
            foreach (var square in _squareList) _staticDrawables.Add(square);

            //create text and add it ti _staticDrawables list
            _fontColor = CreateItems.CreateColor(config["fontColor"]);
            _pressFontColor = CreateItems.CreateColor(config["pressFontColor"]);
            for (var i = 0; i < keyAmount; i++)
            {
                var text = CreateItems.CreateText(_keyList.ElementAt(i).KeyLetter, _squareList.ElementAt(i),
                    _fontColor, false);
                _keyText.Add(text);
                _staticDrawables.Add(text);
            }

            if (config["fading"] == "yes")
                _fading = true;
            if (config["keyCounter"] == "yes")
                _counter = true;
        }

        private Dictionary<string, string> ReadConfig(string configFileName)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var objectDict = new Dictionary<string, string>();
            var file = configFileName == null ? 
                File.ReadLines(Path.Combine(assemblyPath ?? "", "config.txt")).ToArray() :
                File.ReadLines(Path.Combine(assemblyPath ?? "", configFileName)).ToArray();
            foreach (var s in file) objectDict.Add(s.Split("=")[0], s.Split("=")[1]);
            return objectDict;
        }

        private float ReadConfigFloat(Dictionary<string, string> config, string key, float defaultValue)
        {
            if (!config.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private void OnClose(object sender, EventArgs e)
        {
            RequestExit();
        }

        public void Run()
        {
            _window.Closed += OnClose;
            _window.SetFramerateLimit(_maxFPS);

            //Creating a sprite for the fading effect
/*            var fadingList = Fading.GetBackgroundColorFadingTexture(_backgroundColor, _window.Size.X, _ratioY);
            var fadingTexture = new RenderTexture(_window.Size.X, (uint)(255 * 2 * _ratioY));
            fadingTexture.Clear(Color.Transparent);
            if (_fading)
                foreach (var sprite in fadingList)
                    fadingTexture.Draw(sprite);
            fadingTexture.Display();
            var fadingSprite = new Sprite(fadingTexture.Texture);*/



            while (_window.IsOpen && !_exitRequested)
            {
                _window.Clear(_backgroundColor);
                _window.DispatchEvents();
                UpdateWindowDrag();
                //if no keys are being held fill the square with bg color
                foreach (var square in _squareList) square.FillColor = _keyBackgroundColor;
                //if a key is being held, change the key bg and increment hold variable of key
                foreach (var key in _keyList)
                {

                    var defaultPos = _squareList.ElementAt(_keyList.IndexOf(key)).Position;
                   sizeDiff = defaultKeySize - minKeySize;
                     sizeSteps = sizeDiff / (sizeFrames == 0 ? 1 : sizeFrames);


                    if (key.isKey && Keyboard.IsKeyPressed(key.KeyboardKey) ||
                        !key.isKey && Mouse.IsButtonPressed(key.MouseButton))
                    {
                        key.Hold++;
                        if (_keyText.ElementAt(_keyList.IndexOf(key)).FillColor != _pressFontColor)
                            _keyText.ElementAt(_keyList.IndexOf(key)).FillColor = _pressFontColor;
                        _squareList.ElementAt(_keyList.IndexOf(key)).FillColor = _barColor;
                        _squareList.ElementAt(_keyList.IndexOf(key)).OutlineColor = _barColor;
                        //_squareList.ElementAt(_keyList.IndexOf(key)).Size = new Vector2f(minKeySize, minKeySize);
                        if (_squareList.ElementAt(_keyList.IndexOf(key)).Size.X > minKeySize)
                        {
                            _squareList.ElementAt(_keyList.IndexOf(key)).Size = new Vector2f(_squareList.ElementAt(_keyList.IndexOf(key)).Size.X-sizeSteps, _squareList.ElementAt(_keyList.IndexOf(key)).Size.Y-sizeSteps);
                            _squareList.ElementAt(_keyList.IndexOf(key)).Position = new Vector2f(defaultPos.X + sizeSteps/2f, defaultPos.Y + sizeSteps/2f);
                        }
                      
                        var sizeOffset = (defaultKeySize - minKeySize) / 2f;
                        //if (key.Hold==1)    
                        //_squareList.ElementAt(_keyList.IndexOf(key)).Position = new Vector2f(defaultPos.X + sizeOffset, defaultPos.Y + sizeOffset);
                    }
                    else
                    {
                        if (_keyText.ElementAt(_keyList.IndexOf(key)).FillColor != _fontColor)
                        {
                            _keyText.ElementAt(_keyList.IndexOf(key)).FillColor = _fontColor;
                            _squareList.ElementAt(_keyList.IndexOf(key)).OutlineColor = AppWindow.instance.defaultBorderColor;
                        }
                        //_squareList.ElementAt(_keyList.IndexOf(key)).Size = new Vector2f(defaultKeySize, defaultKeySize);
                        if (_squareList.ElementAt(_keyList.IndexOf(key)).Size.X < defaultKeySize)
                        {
                            _squareList.ElementAt(_keyList.IndexOf(key)).Size = new Vector2f(_squareList.ElementAt(_keyList.IndexOf(key)).Size.X+sizeSteps, _squareList.ElementAt(_keyList.IndexOf(key)).Size.Y+sizeSteps);
                            _squareList.ElementAt(_keyList.IndexOf(key)).Position = new Vector2f(defaultPos.X - sizeSteps/2f, defaultPos.Y - sizeSteps/2f);
                        }
                        var sizeOffset = (AppWindow.instance.defaultKeySize - AppWindow.instance.minKeySize) / 2f;
                       // if (key.Hold != 0)
                         //   _squareList.ElementAt(_keyList.IndexOf(key)).Position = new Vector2f(defaultPos.X - sizeOffset , defaultPos.Y - sizeOffset);
                        key.Hold = 0;
                    }
                }

                MoveBars(_keyList, _squareList);

                //draw bg from image if not null

                if (_background is not null)
                    _window.Draw(_background);
                foreach (var staticDrawable in _staticDrawables) _window.Draw(staticDrawable);

                foreach (var key in _keyList)
                {
                    if (_counter)
                    {
                        var text = CreateItems.CreateText(Convert.ToString(key.Counter),
                            _squareList.ElementAt(_keyList.IndexOf(key)),
                            _fontColor, true);
                        _window.Draw(text);
                    }

                    foreach (var bar in key.BarList)
                        _window.Draw(bar);
                }

                //_window.Draw(fadingSprite);

                _window.Display();
            }
        }

        /// <summary>
        /// if a key is a new input create a new bar, if it is being held stretch it and move all bars up
        /// </summary>
        private void MoveBars(List<Key> keyList, List<RectangleShape> squareList)
        {
            var moveDist = _clock.Restart().AsSeconds() * _barSpeed;

            for (var i = 0; i < keyList.Count; i++)
            {
                var key = keyList[i];
                var square = squareList[i];
                var isPressed = key.Hold > 0;
                RectangleShape activeBar = null;

                if (key.Hold == 1)
                {
                    activeBar = CreateItems.CreateBar(square, _outlineThickness, moveDist);
                    key.BarList.Add(activeBar);
                    key.Counter++;
                }
                else if (key.Hold > 1)
                {
                    if (key.BarList.Count == 0)
                        key.BarList.Add(CreateItems.CreateBar(square, _outlineThickness, moveDist));

                    activeBar = key.BarList.Last();
                }

                if (activeBar != null)
                {
                    activeBar.Size = new Vector2f(activeBar.Size.X, Math.Min(activeBar.Size.Y + moveDist, _barMaxHeight));
                    activeBar.FillColor = _barColor;
                    AnchorActiveBar(activeBar, square);
                }

                foreach (var rect in key.BarList.ToArray())
                {
                    if (isPressed && ReferenceEquals(rect, activeBar))
                        continue;

                    rect.Position = new Vector2f(rect.Position.X, rect.Position.Y - moveDist);
                    ApplyBarFade(rect, square);
                }

                key.BarList.RemoveAll(rect => rect.FillColor.A == 0 || rect.Position.Y + rect.Size.Y < 0);
            }
        }

        private void AnchorActiveBar(RectangleShape rect, RectangleShape square)
        {
            var bottomY = square.Position.Y - defaultKeySize - _outlineThickness - sizeSteps / 2f - barOffsetY;
            rect.Position = new Vector2f(rect.Position.X, bottomY - rect.Size.Y);
        }

        private void ApplyBarFade(RectangleShape rect, RectangleShape square)
        {
            var startBottom = square.Position.Y - defaultKeySize - _outlineThickness - barOffsetY;
            var currentBottom = rect.Position.Y + rect.Size.Y;
            var distanceMoved = Math.Max(0, startBottom - currentBottom);
            var fadeProgress = Math.Clamp(distanceMoved / Math.Max(1, _barFadeDistance), 0, 1);
            var color = rect.FillColor;
            color.A = (byte)Math.Round(_barColor.A * (1 - fadeProgress));
            rect.FillColor = color;
        }

        private void OnMouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == Mouse.Button.Left && !_isPositionLocked)
            {
                _isDragging = true;
                _dragStartPos = Mouse.GetPosition(null);
                _windowStartPos = _window.Position;
            }

            if (e.Button == Mouse.Button.Right)
                ShowContextMenu();
        }

        private void OnMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.Button == Mouse.Button.Left)
                _isDragging = false;
        }

        private void UpdateWindowDrag()
        {
            if (!_isDragging || _isPositionLocked)
                return;

            var currentMousePos = Mouse.GetPosition(null);
            var delta = currentMousePos - _dragStartPos;
            _window.Position = _windowStartPos + delta;
        }

        private void InitializeSysTray()
        {
            _contextMenu = new ContextMenuStrip();

            _lockMenuItem = new ToolStripMenuItem("鎖定位置");
            _lockMenuItem.Click += TogglePositionLock;
            _contextMenu.Items.Add(_lockMenuItem);

            _showHideMenuItem = new ToolStripMenuItem("隱藏視窗");
            _showHideMenuItem.Click += ToggleWindowVisibility;
            _contextMenu.Items.Add(_showHideMenuItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            _osuModeMenuItem = new ToolStripMenuItem("osu 模式 (A S)");
            _osuModeMenuItem.Click += (sender, e) => ApplyKeyPreset("osu", "A", "S");
            _contextMenu.Items.Add(_osuModeMenuItem);

            _maniaModeMenuItem = new ToolStripMenuItem("mania 模式 (D F J K)");
            _maniaModeMenuItem.Click += (sender, e) => ApplyKeyPreset("mania", "D", "F", "J", "K");
            _contextMenu.Items.Add(_maniaModeMenuItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("關閉應用");
            exitItem.Click += CloseApplication;
            _contextMenu.Items.Add(exitItem);

            _notifyIcon = new NotifyIcon
            {
                ContextMenuStrip = _contextMenu,
                Icon = GetTrayIcon(),
                Text = "KeyOverlay",
                Visible = true
            };
            _notifyIcon.DoubleClick += ToggleWindowVisibility;
        }

        private Icon GetTrayIcon()
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Resources", "mania.ico");
            if (File.Exists(iconPath))
                return new Icon(iconPath);

            try
            {
                return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;
            }
            catch
            {
                return SystemIcons.Application;
            }
        }

        private void ShowContextMenu()
        {
            _contextMenu?.Show(System.Windows.Forms.Cursor.Position);
        }

        private void ApplyKeyPreset(string modeName, params string[] keys)
        {
            _keyList.Clear();
            _squareList.Clear();
            _staticDrawables.Clear();
            _keyText.Clear();

            foreach (var keyName in keys)
                _keyList.Add(new Key(keyName));

            _squareList.AddRange(CreateItems.CreateKeys(keys.Length, _outlineThickness, _keySize, _ratioX, _ratioY,
                _margin, _window, _keyBackgroundColor, defaultBorderColor, 4));

            foreach (var square in _squareList)
                _staticDrawables.Add(square);

            for (var i = 0; i < _keyList.Count; i++)
            {
                var text = CreateItems.CreateText(_keyList[i].KeyLetter, _squareList[i], _fontColor, false);
                _keyText.Add(text);
                _staticDrawables.Add(text);
            }

            if (_osuModeMenuItem != null)
                _osuModeMenuItem.Checked = modeName == "osu";
            if (_maniaModeMenuItem != null)
                _maniaModeMenuItem.Checked = modeName == "mania";
        }

        private void TogglePositionLock(object sender, EventArgs e)
        {
            SetPositionLocked(!_isPositionLocked);
        }

        private void SetPositionLocked(bool locked)
        {
            _isPositionLocked = locked;
            _isDragging = false;

            DWM.SetClickThroughAble(_window.SystemHandle, locked);
            _lockMenuItem.Text = locked ? "解除位置鎖定" : "鎖定位置";
        }

        private void ToggleWindowVisibility(object sender, EventArgs e)
        {
            _isWindowVisible = !_isWindowVisible;
            _window.SetVisible(_isWindowVisible);

            if (_showHideMenuItem != null)
                _showHideMenuItem.Text = _isWindowVisible ? "隱藏視窗" : "顯示視窗";
        }

        private void CloseApplication(object sender, EventArgs e)
        {
            RequestExit();
        }

        private void RequestExit()
        {
            if (_exitRequested)
                return;

            _exitRequested = true;
            DisposeTrayIcon();

            if (_window.IsOpen)
                _window.Close();

            Application.ExitThread();
        }

        private void DisposeTrayIcon()
        {
            if (_notifyIcon == null)
                return;

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }
}
