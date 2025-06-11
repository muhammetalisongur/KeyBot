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

        private System.Windows.Forms.Timer? automationTimer;
        private System.Windows.Forms.Timer? countdownTimer;
        private int currentRepeatCount = 0;
        private int targetRepeatCount = 0;
        private bool isRunning = false;
        private List<KeySequenceItem> keySequence = new List<KeySequenceItem>();
        private int currentKeyIndex = 0;
        private int countdownSeconds = 0;
        private const string SettingsFileName = "KeyBot_Settings.json";

        public MainForm()
        {
            InitializeComponent();
            
            // Ayarları yükle
            LoadSettings();
        }

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
                
                string selectedKey = keyComboBox.SelectedItem.ToString()!;
                if (selectedKey == "--- FARE İŞLEMLERİ ---")
                {
                    MessageBox.Show("Lütfen geçerli bir tuş veya fare işlemi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
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

                    // Sonraki tuş için delay ayarla
                    if (currentKeyIndex < keySequence.Count && automationTimer != null)
                    {
                        var nextKey = keySequence[currentKeyIndex];
                        automationTimer.Interval = (int)(nextKey.Delay * 1000);
                    }
                    else if (automationTimer != null)
                    {
                        // Döngü tamamlandı, ana interval'e dön
                        automationTimer.Interval = (int)(intervalNumeric.Value * 1000);
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
                        // UI elemanlarını ayarlara göre güncelle
                        singleKeyRadio.Checked = settings.IsSingleKeyMode;
                        mouseRadio.Checked = settings.IsMouseMode;
                        multiKeyRadio.Checked = !settings.IsSingleKeyMode && !settings.IsMouseMode;
                        
                        // ComboBox'lar için index bulma
                        for (int i = 0; i < keyComboBox.Items.Count; i++)
                        {
                            if (keyComboBox.Items[i]?.ToString() == settings.SelectedKey)
                            {
                                keyComboBox.SelectedIndex = i;
                                newKeyComboBox.SelectedIndex = i; // Çoklu tuş ComboBox'ı da aynı tuşa ayarla
                                break;
                            }
                        }
                        
                        // Fare ComboBox için index bulma
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
                        
                        // Çoklu tuş dizisini yükle
                        keySequence.Clear();
                        keySequenceList.Items.Clear();
                        foreach (var item in settings.KeySequence)
                        {
                            keySequence.Add(item);
                            keySequenceList.Items.Add(item);
                        }
                        
                        // UI durumunu güncelle
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
                // Hata durumunda varsayılan değerler
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
            catch
            {
                // Kaydetme hatası durumunda sessizce devam et
            }
        }

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
    }


} 