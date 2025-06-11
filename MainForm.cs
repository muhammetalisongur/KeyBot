using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using KeyBot.Models;

namespace KeyBot
{
    public partial class MainForm : Form
    {
        #region DLL Imports ve Constants

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        const int KEYEVENTF_KEYUP = 0x0002;
        
        // Mouse event constants
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_WHEEL = 0x0800;

        #endregion

        #region Private Fields

        private System.Windows.Forms.Timer? automationTimer;
        private System.Windows.Forms.Timer? countdownTimer;
        private int currentRepeatCount = 0;
        private int targetRepeatCount = 0;
        private bool isRunning = false;
        private List<KeySequenceItem> keySequence = new List<KeySequenceItem>();
        private int currentKeyIndex = 0;
        private int countdownSeconds = 0;
        private const string SettingsFileName = "KeyBot_Settings.json";
        private int draggedItemIndex = -1;
        private bool isDragging = false;
        private bool isCapturingKey = false;
        private bool isCapturingMouse = false;

        #endregion

        #region Constructor ve Form Events

        public MainForm()
        {
            InitializeComponent();
            
            LoadSettings();
        }

        #endregion

        #region UI Event Handlers

        private void InfiniteCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            repeatNumeric.Enabled = !infiniteCheckBox.Checked;
            if (infiniteCheckBox.Checked)
            {
                repeatLabel.Text = "Tekrar Sayısı: ∞";
            }
            else
            {
                repeatLabel.Text = "Tekrar Sayısı:";
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (isRunning) return;

            // Tek tuş modu kontrolü
            if (singleKeyRadio.Checked)
            {
                if (keyComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen bir tuş veya fare işlemi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Klavye işlemi seçili - sadece klavye tuşları mevcut
            }
            // Fare modu kontrolü
            else if (mouseRadio.Checked)
            {
                if (mouseComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen bir fare işlemi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            // Çoklu tuş modu kontrolü  
            else if (multiKeyRadio.Checked)
            {
                if (keySequence.Count == 0)
                {
                    MessageBox.Show("Lütfen tuş dizisine en az bir tuş ekleyin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Kullanıcıya hedef uygulamaya geçiş için uyarı
            MessageBox.Show("3 saniye sonra otomasyon başlayacak!\n\nBu süre içinde hedef uygulamaya geçiş yapın (Alt+Tab)", 
                          "Hazır Olun!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Başlatma sesi çal
            SystemSounds.Exclamation.Play();

            bool isInfinite = infiniteCheckBox.Checked;
            targetRepeatCount = isInfinite ? -1 : (int)repeatNumeric.Value;
            currentRepeatCount = 0;
            currentKeyIndex = 0;
            isRunning = true;

            startButton.Enabled = false;
            stopButton.Enabled = true;

            // 3 saniye geri sayım başlat
            countdownSeconds = 3;
            statusLabel.Text = $"Başlatılıyor... {countdownSeconds}";
            
            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000; // 1 saniye
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }

        private void AutomationTimer_Tick(object? sender, EventArgs e)
        {
            bool isInfinite = infiniteCheckBox.Checked;
            
            if (!isInfinite && currentRepeatCount >= targetRepeatCount)
            {
                StopAutomation();
                return;
            }

            if (singleKeyRadio.Checked)
            {
                // Tek tuş modu
                var selectedKey = keyComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedKey))
                {
                    SendKey(selectedKey);
                    currentRepeatCount++;

                    if (isInfinite)
                    {
                        statusLabel.Text = $"Sınırsız çalışıyor... ({currentRepeatCount})";
                    }
                    else
                    {
                        progressBar.Value = currentRepeatCount;
                        statusLabel.Text = $"Çalışıyor... ({currentRepeatCount}/{targetRepeatCount})";
                    }
                }
            }
            else if (mouseRadio.Checked)
            {
                // Fare modu
                var selectedMouse = mouseComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedMouse))
                {
                    SendMouseAction(selectedMouse);
                    currentRepeatCount++;

                    if (isInfinite)
                    {
                        statusLabel.Text = $"Sınırsız çalışıyor... ({currentRepeatCount}) - {selectedMouse}";
                    }
                    else
                    {
                        progressBar.Value = currentRepeatCount;
                        statusLabel.Text = $"Çalışıyor... ({currentRepeatCount}/{targetRepeatCount}) - {selectedMouse}";
                    }
                }
            }
            else if (multiKeyRadio.Checked)
            {
                // Çoklu tuş modu
                if (keySequence.Count > 0)
                {
                    var currentKey = keySequence[currentKeyIndex];
                    SendKey(currentKey.KeyName);
                    
                    currentKeyIndex++;
                    if (currentKeyIndex >= keySequence.Count)
                    {
                        currentKeyIndex = 0;
                        currentRepeatCount++;
                    }

                    if (isInfinite)
                    {
                        statusLabel.Text = $"Sınırsız çalışıyor... ({currentRepeatCount}) - {currentKey.KeyName}";
                    }
                    else
                    {
                        progressBar.Value = currentRepeatCount;
                        statusLabel.Text = $"Çalışıyor... ({currentRepeatCount}/{targetRepeatCount}) - {currentKey.KeyName}";
                    }
                }
            }
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            countdownSeconds--;
            
            if (countdownSeconds > 0)
            {
                statusLabel.Text = $"Başlatılıyor... {countdownSeconds}";
                SystemSounds.Beep.Play();
            }
            else
            {
                // Geri sayım bitti, gerçek otomasyonu başlat
                countdownTimer?.Stop();
                countdownTimer?.Dispose();
                countdownTimer = null;
                
                SystemSounds.Hand.Play(); // Başlatma sesi
                StartAutomation();
            }
        }

        private void StartAutomation()
        {
            bool isInfinite = infiniteCheckBox.Checked;
            
            if (isInfinite)
            {
                statusLabel.Text = "Sınırsız çalışıyor...";
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.MarqueeAnimationSpeed = 50;
            }
            else
            {
                statusLabel.Text = "Çalışıyor...";
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Maximum = targetRepeatCount;
                progressBar.Value = 0;
            }

            int intervalMs = (int)(intervalNumeric.Value * 1000);

            automationTimer = new System.Windows.Forms.Timer();
            automationTimer.Interval = intervalMs;
            automationTimer.Tick += AutomationTimer_Tick;
            automationTimer.Start();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            StopAutomation();
        }

        private void StopAutomation()
        {
            // Geri sayım timer'ını da durdur
            if (countdownTimer != null)
            {
                countdownTimer.Stop();
                countdownTimer.Dispose();
                countdownTimer = null;
            }

            if (automationTimer != null)
            {
                automationTimer.Stop();
                automationTimer.Dispose();
                automationTimer = null;
            }

            isRunning = false;

            // Durdurma sesi çal
            SystemSounds.Asterisk.Play();

            startButton.Enabled = true;
            stopButton.Enabled = false;
            statusLabel.Text = "Durduruldu";
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
        }

        private void SendKey(string keyName)
        {
            // Fare işlemi kontrolü
            if (keyName.Contains("Tık") || keyName.Contains("Tekerlek") || keyName == "--- FARE İŞLEMLERİ ---")
            {
                if (keyName != "--- FARE İŞLEMLERİ ---")
                {
                    SendMouseAction(keyName);
                }
                return;
            }

            // Klavye tuşu işlemi
            byte vkCode = GetVirtualKeyCode(keyName);
            if (vkCode != 0)
            {
                // Tuşa bas
                keybd_event(vkCode, (byte)MapVirtualKey(vkCode, 0), 0, 0);
                // Tuşu bırak
                keybd_event(vkCode, (byte)MapVirtualKey(vkCode, 0), KEYEVENTF_KEYUP, 0);
            }
        }

        private void SendMouseAction(string mouseAction)
        {
            switch (mouseAction)
            {
                case "Sol Tık":
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case "Sağ Tık":
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case "Orta Tık":
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;
                case "Tekerlek Yukarı":
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 120, 0);
                    break;
                case "Tekerlek Aşağı":
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, unchecked((uint)-120), 0);
                    break;
                case "Çift Tık":
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
            }
        }

        private byte GetVirtualKeyCode(string keyName)
        {
            return keyName switch
            {
                "Space" => 0x20,
                "Enter" => 0x0D,
                "Tab" => 0x09,
                "Escape" => 0x1B,
                "Backspace" => 0x08,
                "Delete" => 0x2E,
                "Left" => 0x25,
                "Up" => 0x26,
                "Right" => 0x27,
                "Down" => 0x28,
                "F1" => 0x70,
                "F2" => 0x71,
                "F3" => 0x72,
                "F4" => 0x73,
                "F5" => 0x74,
                "F6" => 0x75,
                "F7" => 0x76,
                "F8" => 0x77,
                "F9" => 0x78,
                "F10" => 0x79,
                "F11" => 0x7A,
                "F12" => 0x7B,
                _ when keyName.Length == 1 && char.IsLetter(keyName[0]) => (byte)keyName.ToUpper()[0],
                _ when keyName.Length == 1 && char.IsDigit(keyName[0]) => (byte)keyName[0],
                _ => 0
            };
        }

        private void DeveloperLabel_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://muhammetalisongur.com",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Web sitesi açılmazsa sessizce devam et
            }
        }

        private void SingleKeyRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIStates();
        }

        private void MouseRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIStates();
        }

        private void MultiKeyRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIStates();
        }

        private void AddKeyButton_Click(object sender, EventArgs e)
        {
            if (newKeyComboBox.SelectedItem != null)
            {
                string selectedItem = newKeyComboBox.SelectedItem.ToString()!;
                
                // Ayırıcı metinleri engelle
                if (selectedItem == "--- FARE İŞLEMLERİ ---")
                {
                    MessageBox.Show("Lütfen geçerli bir tuş veya fare işlemi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newItem = new KeySequenceItem
                {
                    KeyName = selectedItem,
                    Delay = keyDelayNumeric.Value
                };
                
                keySequence.Add(newItem);
                keySequenceList.Items.Add(newItem);
                
                // Son seçimi tüm ComboBox'larda senkronize et
                SyncComboBoxSelections(newItem.KeyName);
            }
        }

        private void RemoveKeyButton_Click(object sender, EventArgs e)
        {
            if (keySequenceList.SelectedIndex >= 0)
            {
                int index = keySequenceList.SelectedIndex;
                keySequence.RemoveAt(index);
                keySequenceList.Items.RemoveAt(index);
            }
        }

        private void ClearAllKeysButton_Click(object sender, EventArgs e)
        {
            keySequence.Clear();
            keySequenceList.Items.Clear();
        }

        #endregion

        #region Settings Management

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    string json = File.ReadAllText(SettingsFileName);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    
                    if (settings != null)
                    {
                        singleKeyRadio.Checked = settings.IsSingleKeyMode;
                        mouseRadio.Checked = settings.IsMouseMode;
                        multiKeyRadio.Checked = !settings.IsSingleKeyMode && !settings.IsMouseMode;
                        
                        for (int i = 0; i < keyComboBox.Items.Count; i++)
                        {
                            if (keyComboBox.Items[i]?.ToString() == settings.SelectedKey)
                            {
                                keyComboBox.SelectedIndex = i;
                                newKeyComboBox.SelectedIndex = i;
                                break;
                            }
                        }
                        
                        for (int i = 0; i < mouseComboBox.Items.Count; i++)
                        {
                            if (mouseComboBox.Items[i]?.ToString() == settings.SelectedMouse)
                            {
                                mouseComboBox.SelectedIndex = i;
                                break;
                            }
                        }
                        
                        intervalNumeric.Value = settings.Interval;
                        repeatNumeric.Value = settings.RepeatCount;
                        infiniteCheckBox.Checked = settings.IsInfinite;
                        
                        keySequence.Clear();
                        keySequenceList.Items.Clear();
                        foreach (var item in settings.KeySequence)
                        {
                            keySequence.Add(item);
                            keySequenceList.Items.Add(item);
                        }
                        
                        UpdateUIStates();
                    }
                }
                else
                {
                    // İlk kez açılıyorsa varsayılan değerler
                    keyComboBox.SelectedIndex = 0; // Space
                    newKeyComboBox.SelectedIndex = 0; // Space
                    mouseComboBox.SelectedIndex = 0; // Sol Tık
                    UpdateUIStates();
                }
            }
            catch
            {
                keyComboBox.SelectedIndex = 0;
                newKeyComboBox.SelectedIndex = 0;
                mouseComboBox.SelectedIndex = 0;
                UpdateUIStates();
            }
        }

        private void SaveSettings()
        {
            try
            {
                // Hangi mod aktifse o ComboBox'tan son seçimi al
                string selectedKey = singleKeyRadio.Checked 
                    ? (keyComboBox.SelectedItem?.ToString() ?? "Space")
                    : (newKeyComboBox.SelectedItem?.ToString() ?? "Space");

                string selectedMouse = mouseComboBox.SelectedItem?.ToString() ?? "Sol Tık";

                var settings = new AppSettings
                {
                    IsSingleKeyMode = singleKeyRadio.Checked,
                    IsMouseMode = mouseRadio.Checked,
                    SelectedKey = selectedKey,
                    SelectedMouse = selectedMouse,
                    Interval = intervalNumeric.Value,
                    RepeatCount = (int)repeatNumeric.Value,
                    IsInfinite = infiniteCheckBox.Checked,
                    KeySequence = new List<KeySequenceItem>(keySequence)
                };
                
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFileName, json);
            }
            catch{}
            
        }

        #endregion

        #region Utility Methods

        private void UpdateUIStates()
        {
            keyComboBox.Enabled = singleKeyRadio.Checked;
            mouseComboBox.Enabled = mouseRadio.Checked;
            multiKeyGroup.Enabled = multiKeyRadio.Checked;
        }

        private void SyncComboBoxSelections(string keyName)
        {
            // Tüm ComboBox'larda aynı tuşu seç
            for (int i = 0; i < keyComboBox.Items.Count; i++)
            {
                if (keyComboBox.Items[i]?.ToString() == keyName)
                {
                    keyComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            for (int i = 0; i < newKeyComboBox.Items.Count; i++)
            {
                if (newKeyComboBox.Items[i]?.ToString() == keyName)
                {
                    newKeyComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Fare ComboBox için ayrı kontrol gerekmez çünkü farklı öğeler içerir
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAutomation();
            SaveSettings();
            base.OnFormClosing(e);
        }

        private bool IsPointOverCaptureButtons(Point location)
        {
            // Sadece captureMouseButton kontrolü yap (kullanıcı isteği)
            Rectangle mouseButtonBounds = new Rectangle(
                captureMouseButton.Location.X + multiKeyGroup.Location.X,
                captureMouseButton.Location.Y + multiKeyGroup.Location.Y,
                captureMouseButton.Width,
                captureMouseButton.Height
            );

            // Tıklama noktası sadece captureMouseButton üzerinde mi?
            return mouseButtonBounds.Contains(location);
        }

        #endregion

        #region Tuş Yakalama İşlemleri

        private void CaptureKeyButton_Click(object sender, EventArgs e)
        {
            if (!isCapturingKey)
            {
                // Yakalama modunu başlat
                isCapturingKey = true;
                captureKeyButton.Text = "Vazgeç";
                captureKeyButton.BackColor = Color.Orange;
                this.KeyPreview = true;
                this.Focus();
                
                captureToolTip.SetToolTip(captureKeyButton, "Tekrar tıklayarak vazgeçebilirsiniz");
                
                statusLabel.Text = "Tuş yakalama modu aktif - klavyeden bir tuşa basın (Vazgeçmek için butona tekrar tıklayın)";
            }
            else
            {
                EndKeyCapture();
                statusLabel.Text = "Tuş yakalama iptal edildi";
            }
        }

        private void CaptureMouseButton_Click(object sender, EventArgs e)
        {
            if (!isCapturingMouse)
            {
                isCapturingMouse = true;
                captureMouseButton.Text = "Vazgeç";
                captureMouseButton.BackColor = Color.Orange;
                
                captureToolTip.SetToolTip(captureMouseButton, "Tekrar tıklayarak vazgeçebilirsiniz");
                
                statusLabel.Text = "Fare yakalama modu aktif - fareyi tıklayın (Vazgeçmek için butona tekrar tıklayın)";
            }
            else
            {
                EndMouseCapture();
                statusLabel.Text = "Fare yakalama iptal edildi";
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (isCapturingKey)
            {
                string keyName = ConvertKeyToString(keyData);
                if (!string.IsNullOrEmpty(keyName))
                {
                    AddCapturedKeyToComboBoxes(keyName);
                    
                    EndKeyCapture();
                    
                    statusLabel.Text = $"Tuş yakalandı: {keyName}";
                    return true;
                }
            }
            
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (isCapturingMouse)
            {
                // Capture butonları üzerinde mi kontrol et
                if (IsPointOverCaptureButtons(e.Location))
                {
                    // Capture butonları üzerindeyse fare yakalama yapma, sadece buton işlevini çalıştır
                    base.OnMouseClick(e);
                    return;
                }

                string mouseAction = ConvertMouseToString(e.Button);
                if (!string.IsNullOrEmpty(mouseAction))
                {
                    AddCapturedMouseToComboBoxes(mouseAction);
                    
                    EndMouseCapture();
                    
                    statusLabel.Text = $"Fare işlemi yakalandı: {mouseAction}";
                    return;
                }
            }
            
            base.OnMouseClick(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (isCapturingMouse)
            {
                // Capture butonları üzerinde mi kontrol et
                if (IsPointOverCaptureButtons(e.Location))
                {
                    base.OnMouseDoubleClick(e);
                    return;
                }

                string mouseAction = "Çift Tık";
                
                AddCapturedMouseToComboBoxes(mouseAction);
                
                EndMouseCapture();
                
                statusLabel.Text = $"Fare işlemi yakalandı: {mouseAction}";
                return;
            }
            
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (isCapturingMouse)
            {
                // Capture butonları üzerinde mi kontrol et
                if (IsPointOverCaptureButtons(e.Location))
                {
                    base.OnMouseWheel(e);
                    return;
                }

                string mouseAction = e.Delta > 0 ? "Tekerlek Yukarı" : "Tekerlek Aşağı";
                
                AddCapturedMouseToComboBoxes(mouseAction);
                
                EndMouseCapture();
                
                statusLabel.Text = $"Fare işlemi yakalandı: {mouseAction}";
                return;
            }
            
            base.OnMouseWheel(e);
        }

        private string ConvertKeyToString(Keys key)
        {
            // Özel tuşlar için dönüşüm
            switch (key)
            {
                // Temel tuşlar
                case Keys.Space: return "Space";
                case Keys.Enter: return "Enter";
                case Keys.Tab: return "Tab";
                case Keys.Escape: return "Escape";
                case Keys.Back: return "Backspace";
                case Keys.Delete: return "Delete";
                
                // Ok tuşları
                case Keys.Left: return "Left";
                case Keys.Right: return "Right";
                case Keys.Up: return "Up";
                case Keys.Down: return "Down";
                
                // Fonksiyon tuşları
                case Keys.F1: return "F1";
                case Keys.F2: return "F2";
                case Keys.F3: return "F3";
                case Keys.F4: return "F4";
                case Keys.F5: return "F5";
                case Keys.F6: return "F6";
                case Keys.F7: return "F7";
                case Keys.F8: return "F8";
                case Keys.F9: return "F9";
                case Keys.F10: return "F10";
                case Keys.F11: return "F11";
                case Keys.F12: return "F12";
                
                // Modifier tuşları
                case Keys.LControlKey:
                case Keys.RControlKey:
                case Keys.Control: return "Ctrl";
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                case Keys.Shift: return "Shift";
                case Keys.LMenu:
                case Keys.RMenu:
                case Keys.Alt: return "Alt";
                case Keys.LWin:
                case Keys.RWin: return "Windows";
                
                // Sayı tuş takımı
                case Keys.NumPad0: return "NumPad0";
                case Keys.NumPad1: return "NumPad1";
                case Keys.NumPad2: return "NumPad2";
                case Keys.NumPad3: return "NumPad3";
                case Keys.NumPad4: return "NumPad4";
                case Keys.NumPad5: return "NumPad5";
                case Keys.NumPad6: return "NumPad6";
                case Keys.NumPad7: return "NumPad7";
                case Keys.NumPad8: return "NumPad8";
                case Keys.NumPad9: return "NumPad9";
                case Keys.Add: return "NumPad+";
                case Keys.Subtract: return "NumPad-";
                case Keys.Multiply: return "NumPad*";
                case Keys.Divide: return "NumPad/";
                case Keys.Decimal: return "NumPad.";
                case Keys.NumLock: return "NumLock";
                
                // Diğer özel tuşlar
                case Keys.Home: return "Home";
                case Keys.End: return "End";
                case Keys.PageUp: return "PageUp";
                case Keys.PageDown: return "PageDown";
                case Keys.Insert: return "Insert";
                case Keys.Pause: return "Pause";
                case Keys.PrintScreen: return "PrintScreen";
                case Keys.CapsLock: return "CapsLock";
                case Keys.Scroll: return "ScrollLock";
                
                // Noktalama işaretleri
                case Keys.OemSemicolon: return ";";
                case Keys.Oemplus: return "+";
                case Keys.Oemcomma: return ",";
                case Keys.OemMinus: return "-";
                case Keys.OemPeriod: return ".";
                case Keys.OemQuestion: return "/";
                case Keys.Oemtilde: return "`";
                case Keys.OemOpenBrackets: return "[";
                case Keys.OemPipe: return "\\";
                case Keys.OemCloseBrackets: return "]";
                case Keys.OemQuotes: return "'";
                
                default:
                    // Harf ve rakam tuşları
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        return key.ToString();
                    }
                    if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        return key.ToString().Replace("D", ""); // D0-D9 -> 0-9
                    }
                    
                    // Bilinmeyen tuş - otomatik isim oluştur
                    return GenerateCustomKeyName(key.ToString());
            }
        }

        private string ConvertMouseToString(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left: return "Sol Tık";
                case MouseButtons.Right: return "Sağ Tık";
                case MouseButtons.Middle: return "Orta Tık";
                case MouseButtons.XButton1: return "Fare Tuş 4";
                case MouseButtons.XButton2: return "Fare Tuş 5";
                default: 
                    // Bilinmeyen fare düğmesi - otomatik isim oluştur
                    return GenerateCustomMouseName(button.ToString());
            }
        }

        private string GenerateCustomKeyName(string originalKeyName)
        {
            // Özel tuş isimlendirme sistemi
            string baseName = $"Özel Tuş ({originalKeyName})";
            
            // Kullanıcıdan isim al
            string? customName = PromptForCustomName("Yeni Klavye Tuşu", baseName, 
                $"Yakalanan tuş: {originalKeyName}\n\nBu tuş için özel bir isim girin:");
                
            return customName ?? baseName;
        }

        private string GenerateCustomMouseName(string originalMouseName)
        {
            // Özel fare işlemi isimlendirme sistemi
            string baseName = $"Özel Fare ({originalMouseName})";
            
            // Kullanıcıdan isim al
            string? customName = PromptForCustomName("Yeni Fare İşlemi", baseName,
                $"Yakalanan fare işlemi: {originalMouseName}\n\nBu işlem için özel bir isim girin:");
                
            return customName ?? baseName;
        }

        private string? PromptForCustomName(string title, string defaultName, string message)
        {
            using (var form = new Form())
            {
                form.Text = title;
                form.Size = new Size(400, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var label = new Label()
                {
                    Text = message,
                    Location = new Point(10, 10),
                    Size = new Size(360, 60),
                    AutoSize = false
                };

                var textBox = new TextBox()
                {
                    Text = defaultName,
                    Location = new Point(10, 80),
                    Size = new Size(360, 25)
                };

                var okButton = new Button()
                {
                    Text = "Tamam",
                    Location = new Point(220, 120),
                    Size = new Size(75, 25),
                    DialogResult = DialogResult.OK
                };

                var cancelButton = new Button()
                {
                    Text = "İptal",
                    Location = new Point(305, 120),
                    Size = new Size(75, 25),
                    DialogResult = DialogResult.Cancel
                };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(okButton);
                form.Controls.Add(cancelButton);

                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                textBox.Focus();
                textBox.SelectAll();

                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string result = textBox.Text.Trim();
                    return string.IsNullOrEmpty(result) ? defaultName : result;
                }

                return null;
            }
        }

        private void AddCapturedKeyToComboBoxes(string keyName)
        {
            // Mevcut ComboBox'larda bu tuş zaten var mı kontrol et
            bool existsInKeyCombo = ComboBoxContainsItem(keyComboBox, keyName);
            bool existsInNewKeyCombo = ComboBoxContainsItem(newKeyComboBox, keyName);
            
            // Eğer mevcut değilse en üste ekle
            if (!existsInKeyCombo)
            {
                keyComboBox.Items.Insert(0, keyName);
                statusLabel.Text = $"Yeni tuş eklendi: {keyName}";
            }
            else
            {
                statusLabel.Text = $"Mevcut tuş seçildi: {keyName}";
            }
            
            if (!existsInNewKeyCombo)
            {
                newKeyComboBox.Items.Insert(0, keyName);
            }
            
            // Yeni eklenen/bulunan tuşu seç
            keyComboBox.SelectedItem = keyName;
            newKeyComboBox.SelectedItem = keyName;
        }

        private void AddCapturedMouseToComboBoxes(string mouseAction)
        {
            // Mouse ComboBox'da bu işlem zaten var mı kontrol et
            bool existsInMouseCombo = ComboBoxContainsItem(mouseComboBox, mouseAction);
            bool existsInNewKeyCombo = ComboBoxContainsItem(newKeyComboBox, mouseAction);
            
            // Eğer mevcut değilse en üste ekle
            if (!existsInMouseCombo)
            {
                mouseComboBox.Items.Insert(0, mouseAction);
                statusLabel.Text = $"Yeni fare işlemi eklendi: {mouseAction}";
            }
            else
            {
                statusLabel.Text = $"Mevcut fare işlemi seçildi: {mouseAction}";
            }
            
            // NewKeyComboBox'a da fare işlemlerini ekle (çünkü çoklu tuş modunda kullanılıyor)
            if (!existsInNewKeyCombo)
            {
                newKeyComboBox.Items.Insert(0, mouseAction);
            }
            
            // Yeni eklenen/bulunan işlemi seç
            mouseComboBox.SelectedItem = mouseAction;
            newKeyComboBox.SelectedItem = mouseAction;
        }

        private bool ComboBoxContainsItem(ComboBox comboBox, string item)
        {
            foreach (var existingItem in comboBox.Items)
            {
                if (existingItem?.ToString() == item)
                {
                    return true;
                }
            }
            return false;
        }

        private void EndKeyCapture()
        {
            isCapturingKey = false;
            captureKeyButton.Text = "⌨️ Tuş";
            captureKeyButton.BackColor = Color.LightBlue;
            this.KeyPreview = false;
            
            // Tooltip'i geri al
            captureToolTip.SetToolTip(captureKeyButton, "Klavyeden herhangi bir tuşa basın");
        }

        private void EndMouseCapture()
        {
            isCapturingMouse = false;
            captureMouseButton.Text = "🖱️ Fare";
            captureMouseButton.BackColor = Color.LightYellow;
            
            // Tooltip'i geri al
            captureToolTip.SetToolTip(captureMouseButton, "Fareyi tıklayın veya çevirin");
        }

        #endregion

        #region Drag & Drop İşlemleri

        private void KeySequenceList_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && keySequenceList.Items.Count > 0)
            {
                int index = keySequenceList.IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    draggedItemIndex = index;
                    isDragging = true;
                    keySequenceList.Cursor = Cursors.SizeAll;
                }
            }
        }

        private void KeySequenceList_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging && e.Button == MouseButtons.Left && draggedItemIndex >= 0)
            {
                keySequenceList.DoDragDrop(keySequenceList.Items[draggedItemIndex], DragDropEffects.Move);
                isDragging = false;
                keySequenceList.Cursor = Cursors.Default;
            }
            else if (!isDragging)
            {
                // Fare bir öğenin üzerindeyse cursor değiştir
                int index = keySequenceList.IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    keySequenceList.Cursor = Cursors.SizeAll;
                }
                else
                {
                    keySequenceList.Cursor = Cursors.Default;
                }
            }
        }

        private void KeySequenceList_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(typeof(KeySequenceItem)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void KeySequenceList_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(typeof(KeySequenceItem)))
            {
                e.Effect = DragDropEffects.Move;
                
                // Drop konumunu vurgula
                Point clientPoint = keySequenceList.PointToClient(new Point(e.X, e.Y));
                int targetIndex = keySequenceList.IndexFromPoint(clientPoint);
                
                if (targetIndex >= 0)
                {
                    keySequenceList.SelectedIndex = targetIndex;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void KeySequenceList_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(typeof(KeySequenceItem)) && draggedItemIndex >= 0)
            {
                Point clientPoint = keySequenceList.PointToClient(new Point(e.X, e.Y));
                int targetIndex = keySequenceList.IndexFromPoint(clientPoint);
                
                if (targetIndex >= 0 && targetIndex != draggedItemIndex)
                {
                    // Öğeyi yeni konuma taşı
                    var draggedItem = keySequence[draggedItemIndex];
                    
                    // Önce orijinal konumdan kaldır
                    keySequence.RemoveAt(draggedItemIndex);
                    keySequenceList.Items.RemoveAt(draggedItemIndex);
                    
                    // Index ayarlaması (eğer hedef index dragged item'dan büyükse 1 azalt)
                    if (targetIndex > draggedItemIndex)
                    {
                        targetIndex--;
                    }
                    
                    // Yeni konuma ekle
                    keySequence.Insert(targetIndex, draggedItem);
                    keySequenceList.Items.Insert(targetIndex, draggedItem);
                    
                    // Yeni konumu seç
                    keySequenceList.SelectedIndex = targetIndex;
                }
            }
            
            // Sürükleme işlemini bitir
            draggedItemIndex = -1;
            isDragging = false;
            keySequenceList.Cursor = Cursors.Default;
        }

        #endregion
    }
} 