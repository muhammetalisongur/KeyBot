using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using KeyBot.Models;

namespace KeyBot
{
    public partial class MainForm : Form
    {
        #region DLL Imports

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, 
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, 
            uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        const int KEYEVENTF_KEYUP = 0x0002;
        const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        const uint MOUSEEVENTF_LEFTUP = 0x0004;
        const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        const uint MOUSEEVENTF_WHEEL = 0x0800;
        const uint MOUSEEVENTF_MOVE = 0x0001;
        const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        #endregion

        #region Fields

        private System.Windows.Forms.Timer? automationTimer;
        private System.Windows.Forms.Timer? countdownTimer;
        private System.Windows.Forms.Timer? positionTrackingTimer;
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
        private bool isCapturingPosition = false;
        private POINT capturedMousePosition = new POINT { X = -1, Y = -1 };
        private List<string> customKeys = new List<string>();
        private List<string> customMouseActions = new List<string>();

        #endregion

        #region Constructor

        /// <summary>
        /// Form constructor - initialize components and load settings
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            keyComboBox.SelectedIndexChanged += KeyComboBox_SelectedIndexChanged;
            newKeyComboBox.SelectedIndexChanged += NewKeyComboBox_SelectedIndexChanged;
            LoadSettings();
            UpdatePositionDisplay();
        }

        /// <summary>
        /// Handle key combo box selection changes
        /// </summary>
        private void KeyComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SyncComboBoxSelections(keyComboBox.SelectedItem?.ToString() ?? "");
        }
        
        /// <summary>
        /// Handle new key combo box selection changes
        /// </summary>
        private void NewKeyComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SyncComboBoxSelections(newKeyComboBox.SelectedItem?.ToString() ?? "");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle infinite checkbox change - toggle repeat count control
        /// </summary>
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

        /// <summary>
        /// Start automation process with validation and countdown
        /// </summary>
        private void StartButton_Click(object sender, EventArgs e)
        {
            if (isRunning) return;

            if (singleKeyRadio.Checked)
            {
                if (keyComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen bir tuş veya fare işlemi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else if (mouseRadio.Checked)
            {
                if (mouseComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Lütfen bir fare işlemi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else if (multiKeyRadio.Checked)
            {
                if (keySequence.Count == 0)
                {
                    MessageBox.Show("Lütfen tuş dizisine en az bir tuş ekleyin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            MessageBox.Show("3 saniye sonra otomasyon başlayacak!\n\nBu süre içinde hedef uygulamaya geçiş yapın (Alt+Tab)", 
                          "Hazır Olun!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            SystemSounds.Exclamation.Play();

            bool isInfinite = infiniteCheckBox.Checked;
            targetRepeatCount = isInfinite ? -1 : (int)repeatNumeric.Value;
            currentRepeatCount = 0;
            currentKeyIndex = 0;
            isRunning = true;

            startButton.Enabled = false;
            stopButton.Enabled = true;

            countdownSeconds = 3;
            statusLabel.Text = $"Başlatılıyor... {countdownSeconds}";
            
            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += CountdownTimer_Tick;
            countdownTimer.Start();
        }

        /// <summary>
        /// Execute automation actions based on selected mode and settings
        /// </summary>
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
                if (keySequence.Count > 0)
                {
                    var currentKey = keySequence[currentKeyIndex];
                    
                    if (currentKey.MouseX.HasValue && currentKey.MouseY.HasValue)
                    {
                        SendMouseActionWithPosition(currentKey.KeyName, currentKey.MouseX.Value, currentKey.MouseY.Value);
                    }
                    else
                    {
                        SendKey(currentKey.KeyName);
                    }
                    
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

        /// <summary>
        /// Handle countdown timer tick - countdown before starting automation
        /// </summary>
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
                countdownTimer?.Stop();
                countdownTimer?.Dispose();
                countdownTimer = null;
                
                SystemSounds.Hand.Play();
                StartAutomation();
            }
        }

        /// <summary>
        /// Initialize and start the automation timer
        /// </summary>
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

        /// <summary>
        /// Handle stop button click event
        /// </summary>
        private void StopButton_Click(object sender, EventArgs e)
        {
            StopAutomation();
        }

        /// <summary>
        /// Stop all automation timers and reset UI state
        /// </summary>
        private void StopAutomation()
        {
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

            StopPositionTracking();

            isRunning = false;

            SystemSounds.Asterisk.Play();

            startButton.Enabled = true;
            stopButton.Enabled = false;
            statusLabel.Text = "Durduruldu";
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
        }

        /// <summary>
        /// Send keyboard key or mouse action
        /// </summary>
        private void SendKey(string keyName)
        {
            if (keyName.Contains("Tık") || keyName.Contains("Tekerlek") || keyName == "--- FARE İŞLEMLERİ ---")
            {
                if (keyName != "--- FARE İŞLEMLERİ ---")
                {
                    SendMouseAction(keyName);
                }
                return;
            }

            byte vkCode = GetVirtualKeyCode(keyName);
            if (vkCode != 0)
            {
                keybd_event(vkCode, (byte)MapVirtualKey(vkCode, 0), 0, 0);
                keybd_event(vkCode, (byte)MapVirtualKey(vkCode, 0), KEYEVENTF_KEYUP, 0);
            }
        }

        /// <summary>
        /// Execute mouse action at captured position
        /// </summary>
        private void SendMouseAction(string mouseAction)
        {
            if (capturedMousePosition.X != -1 && capturedMousePosition.Y != -1)
            {
                SetCursorPos(capturedMousePosition.X, capturedMousePosition.Y);
                System.Threading.Thread.Sleep(10);
            }

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

        /// <summary>
        /// Execute mouse action at specific coordinates
        /// </summary>
        private void SendMouseActionWithPosition(string mouseAction, int x, int y)
        {
            SetCursorPos(x, y);
            System.Threading.Thread.Sleep(10);

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
                case "Çift Tık":
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(50);
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case "Tekerlek Yukarı":
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 120, 0);
                    break;
                case "Tekerlek Aşağı":
                    mouse_event(MOUSEEVENTF_WHEEL, 0, 0, unchecked((uint)-120), 0);
                    break;
            }
        }

        /// <summary>
        /// Get virtual key code for key name
        /// </summary>
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

        /// <summary>
        /// Handle developer label click event
        /// </summary>
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
            }
        }

        /// <summary>
        /// Handle single key radio button changes
        /// </summary>
        private void SingleKeyRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIStates();
        }

        /// <summary>
        /// Handle mouse radio button changes
        /// </summary>
        private void MouseRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIStates();
        }

        /// <summary>
        /// Handle multi key radio button changes
        /// </summary>
        private void MultiKeyRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUIStates();
        }

        /// <summary>
        /// Add selected key/action to sequence list
        /// </summary>
        private void AddKeyButton_Click(object sender, EventArgs e)
        {
            if (newKeyComboBox.SelectedItem != null)
            {
                string selectedItem = newKeyComboBox.SelectedItem.ToString()!;
                
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
                
                SyncComboBoxSelections(newItem.KeyName);
            }
        }

        /// <summary>
        /// Remove selected item from sequence list
        /// </summary>
        private void RemoveKeyButton_Click(object sender, EventArgs e)
        {
            if (keySequenceList.SelectedIndex >= 0)
            {
                int index = keySequenceList.SelectedIndex;
                keySequence.RemoveAt(index);
                keySequenceList.Items.RemoveAt(index);
            }
        }

        /// <summary>
        /// Clear all items from sequence list
        /// </summary>
        private void ClearAllKeysButton_Click(object sender, EventArgs e)
        {
            keySequence.Clear();
            keySequenceList.Items.Clear();
        }

        /// <summary>
        /// Override form closing event to save settings
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopAutomation();
            SaveSettings();
            base.OnFormClosing(e);
        }

        #endregion

        #region Settings

        /// <summary>
        /// Load application settings from JSON file
        /// </summary>
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
                        
                        LoadCustomItems(settings);
                        
                        SetSelectedKey(settings.SelectedKey);
                        SetSelectedMouse(settings.SelectedMouse);
                        
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
                        
                        capturedMousePosition.X = settings.CapturedMouseX;
                        capturedMousePosition.Y = settings.CapturedMouseY;
                        
                        UpdateUIStates();
                    }
                }
                else
                {
                    keyComboBox.SelectedIndex = 0;
                    newKeyComboBox.SelectedIndex = 0;
                    mouseComboBox.SelectedIndex = 0;
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

        /// <summary>
        /// Save current application settings to JSON file
        /// </summary>
        private void SaveSettings()
        {
            try
            {
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
                    KeySequence = new List<KeySequenceItem>(keySequence),
                    CapturedMouseX = capturedMousePosition.X,
                    CapturedMouseY = capturedMousePosition.Y,
                    CustomKeys = new List<string>(customKeys),
                    CustomMouseActions = new List<string>(customMouseActions)
                };
                
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFileName, json);
            }
            catch{}
        }
        
        /// <summary>
        /// Load custom keys and mouse actions from settings
        /// </summary>
        private void LoadCustomItems(AppSettings settings)
        {
            customKeys.Clear();
            customKeys.AddRange(settings.CustomKeys);
            
            foreach (var customKey in settings.CustomKeys)
            {
                if (!IsMouseAction(customKey))
                {
                    if (!ComboBoxContainsItem(keyComboBox, customKey))
                    {
                        keyComboBox.Items.Add(customKey);
                    }
                }
                
                if (!ComboBoxContainsItem(newKeyComboBox, customKey))
                {
                    AddKeyActionToNewKeyComboBox(customKey);
                }
            }
            
            customMouseActions.Clear();
            customMouseActions.AddRange(settings.CustomMouseActions);
            
            foreach (var customMouse in settings.CustomMouseActions)
            {
                if (!ComboBoxContainsItem(mouseComboBox, customMouse))
                {
                    mouseComboBox.Items.Add(customMouse);
                }
                
                if (!ComboBoxContainsItem(newKeyComboBox, customMouse))
                {
                    AddMouseActionToNewKeyComboBox(customMouse);
                }
            }
        }
        
        /// <summary>
        /// Set selected key in combo boxes
        /// </summary>
        private void SetSelectedKey(string selectedKey)
        {
            if (string.IsNullOrEmpty(selectedKey)) return;
            
            bool foundInKeyCombo = false;
            for (int i = 0; i < keyComboBox.Items.Count; i++)
            {
                if (keyComboBox.Items[i]?.ToString() == selectedKey)
                {
                    keyComboBox.SelectedIndex = i;
                    foundInKeyCombo = true;
                    break;
                }
            }
            
            bool foundInNewKeyCombo = false;
            for (int i = 0; i < newKeyComboBox.Items.Count; i++)
            {
                if (newKeyComboBox.Items[i]?.ToString() == selectedKey)
                {
                    newKeyComboBox.SelectedIndex = i;
                    foundInNewKeyCombo = true;
                    break;
                }
            }
            
            if (!foundInKeyCombo && keyComboBox.Items.Count > 0)
            {
                keyComboBox.SelectedIndex = 0;
            }
            if (!foundInNewKeyCombo && newKeyComboBox.Items.Count > 0)
            {
                newKeyComboBox.SelectedIndex = 0;
            }
        }
        
        /// <summary>
        /// Set selected mouse action in combo box
        /// </summary>
        private void SetSelectedMouse(string selectedMouse)
        {
            if (string.IsNullOrEmpty(selectedMouse)) return;
            
            bool foundInMouseCombo = false;
            for (int i = 0; i < mouseComboBox.Items.Count; i++)
            {
                if (mouseComboBox.Items[i]?.ToString() == selectedMouse)
                {
                    mouseComboBox.SelectedIndex = i;
                    foundInMouseCombo = true;
                    break;
                }
            }
            
            if (!foundInMouseCombo && mouseComboBox.Items.Count > 0)
            {
                mouseComboBox.SelectedIndex = 0;
            }
        }

        #endregion

        #region UI Utilities

        /// <summary>
        /// Update UI control states based on selected mode
        /// </summary>
        private void UpdateUIStates()
        {
            keyComboBox.Enabled = singleKeyRadio.Checked;
            mouseComboBox.Enabled = mouseRadio.Checked;
            multiKeyGroup.Enabled = multiKeyRadio.Checked;
            
            bool isMouseActionSelected = false;
            
            if (mouseRadio.Checked)
            {
                isMouseActionSelected = true;
            }
            
            if (multiKeyRadio.Checked && newKeyComboBox.SelectedItem != null)
            {
                string? selectedKey = newKeyComboBox.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedKey))
                {
                    isMouseActionSelected = IsMouseActionFromCategory(selectedKey);
                }
            }
            
            capturePositionButton.Enabled = isMouseActionSelected;
        }
        
        /// <summary>
        /// Check if action name is a mouse action
        /// </summary>
        private bool IsMouseAction(string actionName)
        {
            return actionName == "Sol Tık" || actionName == "Sağ Tık" || actionName == "Orta Tık" || 
                   actionName == "Tekerlek Yukarı" || actionName == "Tekerlek Aşağı" || actionName == "Çift Tık" ||
                   actionName.Contains("Tık") || actionName.Contains("Tekerlek");
        }

        /// <summary>
        /// Check if action is a mouse action from mouse category
        /// </summary>
        private bool IsMouseActionFromCategory(string actionName)
        {
            if (actionName == "--- FARE İŞLEMLERİ ---")
                return false;
            
            bool isStandardMouseAction = actionName == "Sol Tık" || actionName == "Sağ Tık" || actionName == "Orta Tık" || 
                                        actionName == "Tekerlek Yukarı" || actionName == "Tekerlek Aşağı" || actionName == "Çift Tık" ||
                                        (actionName.Contains("Tık") || actionName.Contains("Tekerlek"));
            
            bool isCustomMouseAction = customMouseActions.Contains(actionName);
                
            return isStandardMouseAction || isCustomMouseAction;
        }

        /// <summary>
        /// Synchronize selection across combo boxes
        /// </summary>
        private void SyncComboBoxSelections(string keyName)
        {
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
        }

        /// <summary>
        /// Check if combo box contains specific item
        /// </summary>
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

        /// <summary>
        /// Add key action to new key combo box
        /// </summary>
        private void AddKeyActionToNewKeyComboBox(string keyName)
        {
            int separatorIndex = -1;
            for (int i = 0; i < newKeyComboBox.Items.Count; i++)
            {
                if (newKeyComboBox.Items[i]?.ToString() == "--- FARE İŞLEMLERİ ---")
                {
                    separatorIndex = i;
                    break;
                }
            }
            
            if (separatorIndex >= 0)
            {
                newKeyComboBox.Items.Insert(separatorIndex, keyName);
            }
            else
            {
                newKeyComboBox.Items.Add(keyName);
            }
        }

        /// <summary>
        /// Add mouse action to new key combo box
        /// </summary>
        private void AddMouseActionToNewKeyComboBox(string mouseAction)
        {
            int separatorIndex = -1;
            for (int i = 0; i < newKeyComboBox.Items.Count; i++)
            {
                if (newKeyComboBox.Items[i]?.ToString() == "--- FARE İŞLEMLERİ ---")
                {
                    separatorIndex = i;
                    break;
                }
            }
            
            if (separatorIndex >= 0)
            {
                newKeyComboBox.Items.Add(mouseAction);
            }
            else
            {
                newKeyComboBox.Items.Add(mouseAction);
            }
        }

        #endregion

        #region Key Capture

        /// <summary>
        /// Handle capture key button click event
        /// </summary>
        private void CaptureKeyButton_Click(object sender, EventArgs e)
        {
            if (!isCapturingKey)
            {
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

        /// <summary>
        /// Handle capture mouse button click event
        /// </summary>
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

        /// <summary>
        /// Override command key processing for capture modes
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (isCapturingPosition)
            {
                if (keyData == Keys.Escape)
                {
                    EndPositionCapture();
                    statusLabel.Text = "Konum yakalama iptal edildi";
                    return true;
                }
                else if (keyData == Keys.Space)
                {
                    CaptureCurrentPosition();
                    return true;
                }
            }
            
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

        /// <summary>
        /// Override mouse click event for mouse capture
        /// </summary>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (isCapturingMouse)
            {
                if (IsPointOverCaptureButtons(e.Location))
                {
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

        /// <summary>
        /// Override mouse double click event for mouse capture
        /// </summary>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (isCapturingMouse)
            {
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

        /// <summary>
        /// Override mouse wheel event for mouse capture
        /// </summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (isCapturingMouse)
            {
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

        /// <summary>
        /// Convert keyboard key to string representation
        /// </summary>
        private string ConvertKeyToString(Keys key)
        {
            string keyboardChar = GetKeyboardCharacter(key);
            if (!string.IsNullOrEmpty(keyboardChar))
            {
                return keyboardChar;
            }
            
            switch (key)
            {
                case Keys.Space: return "Space";
                case Keys.Enter: return "Enter";
                case Keys.Tab: return "Tab";
                case Keys.Escape: return "Escape";
                case Keys.Back: return "Backspace";
                case Keys.Delete: return "Delete";
                case Keys.Left: return "Left";
                case Keys.Right: return "Right";
                case Keys.Up: return "Up";
                case Keys.Down: return "Down";
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
                case Keys.Home: return "Home";
                case Keys.End: return "End";
                case Keys.PageUp: return "PageUp";
                case Keys.PageDown: return "PageDown";
                case Keys.Insert: return "Insert";
                case Keys.Pause: return "Pause";
                case Keys.PrintScreen: return "PrintScreen";
                case Keys.CapsLock: return "CapsLock";
                case Keys.Scroll: return "ScrollLock";
                
                default:
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        return key.ToString();
                    }
                    if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        return key.ToString().Replace("D", "");
                    }
                    
                    return GenerateCustomKeyName(key.ToString());
            }
        }

        /// <summary>
        /// Convert mouse button to string representation
        /// </summary>
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
                    return GenerateCustomMouseName(button.ToString());
            }
        }

        /// <summary>
        /// Generate custom name for unknown key
        /// </summary>
        private string GenerateCustomKeyName(string originalKeyName)
        {
            string baseName = $"Özel Tuş ({originalKeyName})";
            
            string? customName = PromptForCustomName("Yeni Klavye Tuşu", baseName, 
                $"Yakalanan tuş: {originalKeyName}\n\nBu tuş için özel bir isim girin:");
                
            return customName ?? baseName;
        }

        /// <summary>
        /// Generate custom name for unknown mouse action
        /// </summary>
        private string GenerateCustomMouseName(string originalMouseName)
        {
            string baseName = $"Özel Fare ({originalMouseName})";
            
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

        /// <summary>
        /// Check if point is over any capture button
        /// </summary>
        private bool IsPointOverCaptureButtons(Point location)
        {
            Rectangle mouseButtonBounds = new Rectangle(
                captureMouseButton.Location.X + multiKeyGroup.Location.X,
                captureMouseButton.Location.Y + multiKeyGroup.Location.Y,
                captureMouseButton.Width,
                captureMouseButton.Height
            );

            Rectangle keyButtonBounds = new Rectangle(
                captureKeyButton.Location.X + multiKeyGroup.Location.X,
                captureKeyButton.Location.Y + multiKeyGroup.Location.Y,
                captureKeyButton.Width,
                captureKeyButton.Height
            );

            Rectangle positionButtonBounds = new Rectangle(
                capturePositionButton.Location.X + multiKeyGroup.Location.X,
                capturePositionButton.Location.Y + multiKeyGroup.Location.Y,
                capturePositionButton.Width,
                capturePositionButton.Height
            );

            return mouseButtonBounds.Contains(location) || 
                   keyButtonBounds.Contains(location) || 
                   positionButtonBounds.Contains(location);
        }

        /// <summary>
        /// Get active keyboard layout character representation
        /// </summary>
        private string GetKeyboardCharacter(Keys key)
        {
            try
            {
                IntPtr keyboardLayout = GetKeyboardLayout(0);
                
                byte[] keyboardState = new byte[256];
                if (!GetKeyboardState(keyboardState))
                    return string.Empty;
                
                uint virtualKey = (uint)key;
                uint scanCode = MapVirtualKey(virtualKey, 0);
                
                StringBuilder result = new StringBuilder(10);
                int charCount = ToUnicodeEx(virtualKey, scanCode, keyboardState, result, result.Capacity, 0, keyboardLayout);
                
                if (charCount > 0 && result.Length > 0)
                {
                    string character = result.ToString();
                    
                    if (!char.IsControl(character[0]) && character.Length == 1)
                    {
                        string currentLanguage = InputLanguage.CurrentInputLanguage.Culture.Name;
                        return $"{character} ({currentLanguage})";
                    }
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Add captured key to all relevant combo boxes
        /// </summary>
        private void AddCapturedKeyToComboBoxes(string keyName)
        {
            bool existsInKeyCombo = ComboBoxContainsItem(keyComboBox, keyName);
            bool existsInNewKeyCombo = ComboBoxContainsItem(newKeyComboBox, keyName);
            
            if (!existsInKeyCombo)
            {
                if (!IsMouseAction(keyName))
                {
                    keyComboBox.Items.Add(keyName);
                }
                
                if (!customKeys.Contains(keyName))
                {
                    customKeys.Add(keyName);
                }
                
                statusLabel.Text = $"Yeni tuş eklendi: {keyName}";
            }
            else
            {
                statusLabel.Text = $"Mevcut tuş seçildi: {keyName}";
            }
            
            if (!existsInNewKeyCombo)
            {
                AddKeyActionToNewKeyComboBox(keyName);
            }
            
            if (!IsMouseAction(keyName))
            {
                keyComboBox.SelectedItem = keyName;
            }
            newKeyComboBox.SelectedItem = keyName;
        }
        
        private void AddCapturedMouseToComboBoxes(string mouseAction)
        {
            bool existsInMouseCombo = ComboBoxContainsItem(mouseComboBox, mouseAction);
            bool existsInNewKeyCombo = ComboBoxContainsItem(newKeyComboBox, mouseAction);
            
            if (!existsInMouseCombo)
            {
                mouseComboBox.Items.Add(mouseAction);
                
                if (!customMouseActions.Contains(mouseAction))
                {
                    customMouseActions.Add(mouseAction);
                }
                
                statusLabel.Text = $"Yeni fare işlemi eklendi: {mouseAction}";
            }
            else
            {
                statusLabel.Text = $"Mevcut fare işlemi seçildi: {mouseAction}";
            }
            
            if (!existsInNewKeyCombo)
            {
                AddMouseActionToNewKeyComboBox(mouseAction);
            }
            
            mouseComboBox.SelectedItem = mouseAction;
            newKeyComboBox.SelectedItem = mouseAction;
        }

        /// <summary>
        /// End key capture mode and restore UI
        /// </summary>
        private void EndKeyCapture()
        {
            isCapturingKey = false;
            captureKeyButton.Text = "⌨️";
            captureKeyButton.BackColor = Color.LightBlue;
            this.KeyPreview = false;
            
            captureToolTip.SetToolTip(captureKeyButton, "Klavyeden herhangi bir tuşa basın");
        }

        /// <summary>
        /// End mouse capture mode and restore UI
        /// </summary>
        private void EndMouseCapture()
        {
            isCapturingMouse = false;
            captureMouseButton.Text = "🖱️";
            captureMouseButton.BackColor = Color.LightYellow;
            
            captureToolTip.SetToolTip(captureMouseButton, "Fareyi tıklayın veya çevirin");
        }

        /// <summary>
        /// Toggle position capture mode
        /// </summary>
        private void CapturePositionButton_Click(object sender, EventArgs e)
        {
            if (isCapturingPosition)
            {
                EndPositionCapture();
                return;
            }

            if (isCapturingKey || isCapturingMouse)
            {
                MessageBox.Show("Önce diğer yakalama işlemini bitirin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StartPositionCapture();
        }

        /// <summary>
        /// Start position capture process
        /// </summary>
        private void StartPositionCapture()
        {
            bool isMouseActionSelectedInSingleMode = false;
            if (singleKeyRadio.Checked && keyComboBox.SelectedItem != null)
            {
                string? selectedKey = keyComboBox.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedKey))
                {
                    isMouseActionSelectedInSingleMode = IsMouseAction(selectedKey);
                }
            }
            
            bool isMouseActionSelectedInMultiMode = false;
            if (multiKeyRadio.Checked && newKeyComboBox.SelectedItem != null)
            {
                string? selectedKey = newKeyComboBox.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedKey))
                {
                    isMouseActionSelectedInMultiMode = IsMouseAction(selectedKey);
                }
            }
            
            if (!mouseRadio.Checked && !multiKeyRadio.Checked && !isMouseActionSelectedInSingleMode && !isMouseActionSelectedInMultiMode)
            {
                MessageBox.Show("Konum yakalama sadece Fare modunda, Çoklu İşlem modunda veya Klavye modunda fare işaretçisi seçildiğinde kullanılabilir!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedMouseAction;
            
            if (mouseRadio.Checked)
            {
                selectedMouseAction = mouseComboBox.SelectedItem?.ToString() ?? "Sol Tık";
            }
            else if (singleKeyRadio.Checked && keyComboBox.SelectedItem != null)
            {
                string? selectedKey = keyComboBox.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedKey) && IsMouseAction(selectedKey))
                {
                    selectedMouseAction = selectedKey;
                }
                else
                {
                    selectedMouseAction = "Sol Tık";
                }
            }
            else if (multiKeyRadio.Checked && newKeyComboBox.SelectedItem != null)
            {
                string? selectedKey = newKeyComboBox.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedKey) && IsMouseAction(selectedKey))
                {
                    selectedMouseAction = selectedKey;
                }
                else
                {
                    selectedMouseAction = "Sol Tık";
                }
            }
            else
            {
                selectedMouseAction = "Sol Tık";
            }
            
            isCapturingPosition = true;
            capturePositionButton.BackColor = Color.LightCoral;
            capturePositionButton.Text = "⏹";
            captureToolTip.SetToolTip(capturePositionButton, "ESC ile iptal edin veya SPACE ile konumu kaydet");

            StartPositionTracking();
            
            MessageBox.Show($"'{selectedMouseAction}' için konum yakalamak:\n1. İstediğiniz yere fareyi götürün\n2. SPACE tuşuna basın\n\nİptal için ESC tuşuna basın", 
                          "Konum Yakalama", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Start position tracking timer for real-time position updates
        /// </summary>
        private void StartPositionTracking()
        {
            positionTrackingTimer = new System.Windows.Forms.Timer();
            positionTrackingTimer.Interval = 50;
            positionTrackingTimer.Tick += PositionTrackingTimer_Tick;
            positionTrackingTimer.Start();
            
            this.Focus();
            this.BringToFront();
        }

        /// <summary>
        /// Handle position tracking timer tick events
        /// </summary>
        private void PositionTrackingTimer_Tick(object? sender, EventArgs e)
        {
            if (isCapturingPosition)
            {
                POINT currentPos;
                GetCursorPos(out currentPos);
                
                string selectedMouseAction;
                
                if (mouseRadio.Checked)
                {
                    selectedMouseAction = mouseComboBox.SelectedItem?.ToString() ?? "Sol Tık";
                }
                else if (singleKeyRadio.Checked && keyComboBox.SelectedItem != null)
                {
                    string? selectedKey = keyComboBox.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(selectedKey) && IsMouseAction(selectedKey))
                    {
                        selectedMouseAction = selectedKey;
                    }
                    else
                    {
                        selectedMouseAction = "Sol Tık";
                    }
                }
                else if (multiKeyRadio.Checked && newKeyComboBox.SelectedItem != null)
                {
                    string? selectedKey = newKeyComboBox.SelectedItem.ToString();
                    if (!string.IsNullOrEmpty(selectedKey) && IsMouseAction(selectedKey))
                    {
                        selectedMouseAction = selectedKey;
                    }
                    else
                    {
                        selectedMouseAction = "Sol Tık";
                    }
                }
                else
                {
                    selectedMouseAction = "Sol Tık";
                }
                
                positionLabel.Text = $"{selectedMouseAction}: ({currentPos.X}, {currentPos.Y}) - SPACE ile kaydet";
                positionLabel.ForeColor = Color.Blue;
            }
        }

        /// <summary>
        /// End position capture and restore UI
        /// </summary>
        private void EndPositionCapture()
        {
            if (isCapturingPosition)
            {
                isCapturingPosition = false;
                capturePositionButton.BackColor = Color.LightGreen;
                capturePositionButton.Text = "📍";
                captureToolTip.SetToolTip(capturePositionButton, "Fare konumunu yakala");
                
                StopPositionTracking();
                
                positionLabel.Text = "Konum: Belirlenmedi";
                positionLabel.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Stop position tracking timer
        /// </summary>
        private void StopPositionTracking()
        {
            if (positionTrackingTimer != null)
            {
                positionTrackingTimer.Stop();
                positionTrackingTimer.Dispose();
                positionTrackingTimer = null;
            }
        }

        /// <summary>
        /// Capture current mouse position and add to sequence
        /// </summary>
        private void CaptureCurrentPosition()
        {
            POINT currentPos;
            GetCursorPos(out currentPos);
            
            AddPositionToSequence(currentPos.X, currentPos.Y);
            EndPositionCapture();
        }

        /// <summary>
        /// Update position display label
        /// </summary>
        private void UpdatePositionDisplay()
        {
            if (capturedMousePosition.X != -1 && capturedMousePosition.Y != -1)
            {
                positionLabel.Text = $"Konum: ({capturedMousePosition.X}, {capturedMousePosition.Y})";
                positionLabel.ForeColor = Color.Green;
            }
            else
            {
                positionLabel.Text = "Konum: Belirlenmedi";
                positionLabel.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Add position-based action to sequence
        /// </summary>
        private void AddPositionToSequence(int x, int y)
        {
            string selectedAction;
            
            if (mouseRadio.Checked)
            {
                selectedAction = mouseComboBox.SelectedItem?.ToString() ?? "Sol Tık";
            }
            else if (singleKeyRadio.Checked && keyComboBox.SelectedItem != null)
            {
                string? selectedKey = keyComboBox.SelectedItem.ToString();
                selectedAction = !string.IsNullOrEmpty(selectedKey) ? selectedKey : "Sol Tık";
            }
            else if (multiKeyRadio.Checked && newKeyComboBox.SelectedItem != null)
            {
                string? selectedKey = newKeyComboBox.SelectedItem.ToString();
                selectedAction = !string.IsNullOrEmpty(selectedKey) ? selectedKey : "Sol Tık";
            }
            else
            {
                selectedAction = "Sol Tık";
            }
            
            var newItem = new KeySequenceItem
            {
                KeyName = selectedAction,
                Delay = keyDelayNumeric.Value,
                MouseX = x,
                MouseY = y
            };
            
            keySequence.Add(newItem);
            keySequenceList.Items.Add(newItem);
            
            statusLabel.Text = $"{selectedAction} konumu eklendi: ({x}, {y})";
        }

        #endregion

        #region Drag & Drop

        /// <summary>
        /// Handle drag and drop mouse down event for list items
        /// </summary>
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

        /// <summary>
        /// Handle drag and drop mouse move event for list items
        /// </summary>
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

        /// <summary>
        /// Handle drag enter event for list items
        /// </summary>
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

        /// <summary>
        /// Handle drag over event for list items
        /// </summary>
        private void KeySequenceList_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(typeof(KeySequenceItem)))
            {
                e.Effect = DragDropEffects.Move;
                
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

        /// <summary>
        /// Handle drag drop event for list items reordering
        /// </summary>
        private void KeySequenceList_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data!.GetDataPresent(typeof(KeySequenceItem)) && draggedItemIndex >= 0)
            {
                Point clientPoint = keySequenceList.PointToClient(new Point(e.X, e.Y));
                int targetIndex = keySequenceList.IndexFromPoint(clientPoint);
                
                if (targetIndex >= 0 && targetIndex != draggedItemIndex)
                {
                    var draggedItem = keySequence[draggedItemIndex];
                    
                    keySequence.RemoveAt(draggedItemIndex);
                    keySequenceList.Items.RemoveAt(draggedItemIndex);
                    
                    if (targetIndex > draggedItemIndex)
                    {
                        targetIndex--;
                    }
                    
                    keySequence.Insert(targetIndex, draggedItem);
                    keySequenceList.Items.Insert(targetIndex, draggedItem);
                    
                    keySequenceList.SelectedIndex = targetIndex;
                }
            }
            
            draggedItemIndex = -1;
            isDragging = false;
            keySequenceList.Cursor = Cursors.Default;
        }

        #endregion
    }
} 