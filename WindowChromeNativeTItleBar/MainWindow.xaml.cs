using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace WindowChromeNativeTItleBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // ==========================================
        // INotifyPropertyChanged の実装
        // ==========================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ==========================================
        // プロパティ定義
        // ==========================================
        private bool _useCustomTitleBar;

        public bool UseCustomTitleBar
        {
            get => _useCustomTitleBar;
            set
            {
                if (_useCustomTitleBar != value)
                {
                    _useCustomTitleBar = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCustomTitleBar));

                    // WindowChrome のアタッチ・デタッチを切り替え
                    UpdateTitleBarMode(_useCustomTitleBar);
                }
            }
        }

        public bool IsCustomTitleBar => _useCustomTitleBar;

        
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            // 1.起動時に標準API（Win32 API）経由でINIから読み込み
            _useCustomTitleBar = ConfigManager.GetCustomTitleBarSetting();

            // 2. 初期状態の適用
            UpdateTitleBarMode(_useCustomTitleBar);

            OnPropertyChanged(nameof(UseCustomTitleBar));
            OnPropertyChanged(nameof(IsCustomTitleBar));
        }
        // WindowChrome の動的アタッチ・デタッチ処理
        private void UpdateTitleBarMode(bool isCustom)
        {
            if (isCustom)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowChrome.SetWindowChrome(this, new WindowChrome
                {
                    CaptionHeight = 32,
                    ResizeBorderThickness = new Thickness(6),
                    UseAeroCaptionButtons = true,
                    GlassFrameThickness = new Thickness(-1),
                    CornerRadius = new CornerRadius(1)
                });
            }
            else
            {
                WindowChrome.SetWindowChrome(this, null);
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }
        // タイトルバー切り替えトグルボタン
        private void ToggleTitleBarButton_Click(object sender, RoutedEventArgs e)
        {
            UseCustomTitleBar = !UseCustomTitleBar;

            // 標準API（Win32 API）経由でINIに即時保存
            ConfigManager.SaveCustomTitleBarSetting(_useCustomTitleBar);
        }

        // ネイティブシステムコマンドを使用したコントロールボックス処理
        // ウィンドウ初期化時にWin32メッセージフックを追加
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (PresentationSource.FromVisual(this) is HwndSource source)
            {
                source.AddHook(HwndHook);
            }
        }

        // Win32メッセージフック（WM_NCHITTEST で HTMAXBUTTON を返す）
        // Win32メッセージフック（ActualWidth / ActualHeight を使ったスマートな判定）
        // Win32メッセージフック（高DPI対応 ＋ HTMIN / HTMAX / HTCLOSE 判定）
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTMINBUTTON = 8;
            const int HTMAXBUTTON = 9;
            const int HTCLOSE = 20;

            if (msg == WM_NCHITTEST && _useCustomTitleBar)
            {
                // 1. lParam から物理ピクセルのスクリーン座標を取得
                int x = (short)(lParam.ToInt32() & 0xFFFF);
                int y = (short)(lParam.ToInt32() >> 16);
                var physicalScreenPoint = new Point(x, y);

                // 2. 物理ピクセルを WPF のデバイス非依存ピクセル (DIPs) に変換
                var source = PresentationSource.FromVisual(this);
                if (source?.CompositionTarget != null)
                {
                    var dipScreenPoint = source.CompositionTarget.TransformFromDevice.Transform(physicalScreenPoint);
                    var clientPoint = this.PointFromScreen(dipScreenPoint);

                    // 3. 各ボタンの ActualWidth / ActualHeight と位置から判定
                    if (CloseButton != null)
                    {
                        var pos = CloseButton.TranslatePoint(new Point(0, 0), this);
                        if (new Rect(pos.X, pos.Y, CloseButton.ActualWidth, CloseButton.ActualHeight).Contains(clientPoint))
                        {
                            handled = true;
                            return (IntPtr)HTCLOSE;
                        }
                    }

                    if (MaximizeButton != null)
                    {
                        var pos = MaximizeButton.TranslatePoint(new Point(0, 0), this);
                        if (new Rect(pos.X, pos.Y, MaximizeButton.ActualWidth, MaximizeButton.ActualHeight).Contains(clientPoint))
                        {
                            handled = true;
                            return (IntPtr)HTMAXBUTTON;
                        }
                    }

                    if (MinimizeButton != null)
                    {
                        var pos = MinimizeButton.TranslatePoint(new Point(0, 0), this);
                        if (new Rect(pos.X, pos.Y, MinimizeButton.ActualWidth, MinimizeButton.ActualHeight).Contains(clientPoint))
                        {
                            handled = true;
                            return (IntPtr)HTMINBUTTON;
                        }
                    }
                }
            }

            return IntPtr.Zero;
        }
        // ==========================================
        // コントロールボックス操作
        // ==========================================
        private void Minimize_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized) SystemCommands.RestoreWindow(this);
            else SystemCommands.MaximizeWindow(this);
        }
        private void Close_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);
    }
}