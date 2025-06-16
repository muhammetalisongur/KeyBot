namespace KeyBot
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private GroupBox keyGroup;
        private Label keyLabel;
        private ComboBox keyComboBox;
        private RadioButton singleKeyRadio;
        private RadioButton multiKeyRadio;
        private RadioButton mouseRadio;
        private GroupBox multiKeyGroup;
        private ComboBox mouseComboBox;
        private ListBox keySequenceList;
        private Button addKeyButton;
        private Button removeKeyButton;
        private Button clearAllKeysButton;
        private ComboBox newKeyComboBox;
        private NumericUpDown keyDelayNumeric;
        private Label keyDelayLabel;
        private GroupBox timeGroup;
        private Label intervalLabel;
        private NumericUpDown intervalNumeric;
        private Label repeatLabel;
        private NumericUpDown repeatNumeric;
        private CheckBox infiniteCheckBox;
        private Button startButton;
        private Button stopButton;
        private Label statusLabel;
        private ProgressBar progressBar;
        private Label developerLabel;
        private ToolTip keySequenceToolTip;
        private Button captureKeyButton;
        private Button captureMouseButton;
        private Button capturePositionButton;
        private Label positionLabel;
        private ToolTip captureToolTip;
        private Button clearCustomKeysButton;
        private Button removeCustomKeyButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            keyGroup = new GroupBox();
            clearCustomKeysButton = new Button();
            removeCustomKeyButton = new Button();
            capturePositionButton = new Button();
            positionLabel = new Label();
            multiKeyGroup = new GroupBox();
            captureMouseButton = new Button();
            captureKeyButton = new Button();
            keyDelayLabel = new Label();
            keyDelayNumeric = new NumericUpDown();
            newKeyComboBox = new ComboBox();
            clearAllKeysButton = new Button();
            removeKeyButton = new Button();
            addKeyButton = new Button();
            keySequenceList = new ListBox();
            multiKeyRadio = new RadioButton();
            mouseRadio = new RadioButton();
            singleKeyRadio = new RadioButton();
            keyComboBox = new ComboBox();
            mouseComboBox = new ComboBox();
            keyLabel = new Label();
            timeGroup = new GroupBox();
            infiniteCheckBox = new CheckBox();
            repeatNumeric = new NumericUpDown();
            repeatLabel = new Label();
            intervalNumeric = new NumericUpDown();
            intervalLabel = new Label();
            startButton = new Button();
            stopButton = new Button();
            statusLabel = new Label();
            progressBar = new ProgressBar();
            developerLabel = new Label();
            keySequenceToolTip = new ToolTip(components);
            captureToolTip = new ToolTip(components);
            keyGroup.SuspendLayout();
            multiKeyGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)keyDelayNumeric).BeginInit();
            timeGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)repeatNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)intervalNumeric).BeginInit();
            SuspendLayout();
            // 
            // keyGroup
            // 
            keyGroup.Controls.Add(clearCustomKeysButton);
            keyGroup.Controls.Add(removeCustomKeyButton);
            keyGroup.Controls.Add(capturePositionButton);
            keyGroup.Controls.Add(positionLabel);
            keyGroup.Controls.Add(multiKeyGroup);
            keyGroup.Controls.Add(multiKeyRadio);
            keyGroup.Controls.Add(mouseRadio);
            keyGroup.Controls.Add(singleKeyRadio);
            keyGroup.Controls.Add(keyComboBox);
            keyGroup.Controls.Add(mouseComboBox);
            keyGroup.Controls.Add(keyLabel);
            keyGroup.Location = new Point(23, 27);
            keyGroup.Margin = new Padding(3, 4, 3, 4);
            keyGroup.Name = "keyGroup";
            keyGroup.Padding = new Padding(3, 4, 3, 4);
            keyGroup.Size = new Size(503, 385);
            keyGroup.TabIndex = 0;
            keyGroup.TabStop = false;
            keyGroup.Text = "İşlem Seçimi";
            // 
            // clearCustomKeysButton
            // 
            clearCustomKeysButton.BackColor = Color.LightCoral;
            clearCustomKeysButton.Cursor = Cursors.Hand;
            clearCustomKeysButton.Location = new Point(348, 107);
            clearCustomKeysButton.Name = "clearCustomKeysButton";
            clearCustomKeysButton.Size = new Size(120, 30);
            clearCustomKeysButton.TabIndex = 7;
            clearCustomKeysButton.Text = "Tümünü Temizle";
            captureToolTip.SetToolTip(clearCustomKeysButton, "Tüm özel tuşları ve fare işlemlerini temizle");
            clearCustomKeysButton.UseVisualStyleBackColor = false;
            clearCustomKeysButton.Click += ClearCustomKeysButton_Click;
            // 
            // removeCustomKeyButton
            // 
            removeCustomKeyButton.BackColor = Color.LightYellow;
            removeCustomKeyButton.Cursor = Cursors.Hand;
            removeCustomKeyButton.Location = new Point(348, 71);
            removeCustomKeyButton.Name = "removeCustomKeyButton";
            removeCustomKeyButton.Size = new Size(120, 30);
            removeCustomKeyButton.TabIndex = 8;
            removeCustomKeyButton.Text = "Seçili Tuşu Sil";
            captureToolTip.SetToolTip(removeCustomKeyButton, "Seçili özel tuşu sil");
            removeCustomKeyButton.UseVisualStyleBackColor = false;
            removeCustomKeyButton.Click += RemoveCustomKeyButton_Click;
            // 
            // capturePositionButton
            // 
            capturePositionButton.BackColor = Color.LightGreen;
            capturePositionButton.Cursor = Cursors.Hand;
            capturePositionButton.Location = new Point(286, 88);
            capturePositionButton.Name = "capturePositionButton";
            capturePositionButton.Size = new Size(56, 35);
            capturePositionButton.TabIndex = 9;
            capturePositionButton.Text = "📍";
            captureToolTip.SetToolTip(capturePositionButton, "Fare konumunu yakala");
            capturePositionButton.UseVisualStyleBackColor = false;
            capturePositionButton.Click += CapturePositionButton_Click;
            // 
            // positionLabel
            // 
            positionLabel.AutoSize = true;
            positionLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            positionLabel.ForeColor = Color.Red;
            positionLabel.Location = new Point(23, 354);
            positionLabel.Name = "positionLabel";
            positionLabel.Size = new Size(156, 20);
            positionLabel.TabIndex = 9;
            positionLabel.Text = "Konum: Belirlenmedi";
            // 
            // multiKeyGroup
            // 
            multiKeyGroup.Controls.Add(captureMouseButton);
            multiKeyGroup.Controls.Add(captureKeyButton);
            multiKeyGroup.Controls.Add(keyDelayLabel);
            multiKeyGroup.Controls.Add(keyDelayNumeric);
            multiKeyGroup.Controls.Add(newKeyComboBox);
            multiKeyGroup.Controls.Add(clearAllKeysButton);
            multiKeyGroup.Controls.Add(removeKeyButton);
            multiKeyGroup.Controls.Add(addKeyButton);
            multiKeyGroup.Controls.Add(keySequenceList);
            multiKeyGroup.Enabled = false;
            multiKeyGroup.Location = new Point(23, 144);
            multiKeyGroup.Name = "multiKeyGroup";
            multiKeyGroup.Size = new Size(460, 200);
            multiKeyGroup.TabIndex = 4;
            multiKeyGroup.TabStop = false;
            multiKeyGroup.Text = "İşlem Dizisi";
            // 
            // captureMouseButton
            // 
            captureMouseButton.BackColor = Color.LightYellow;
            captureMouseButton.Cursor = Cursors.Hand;
            captureMouseButton.Location = new Point(394, 140);
            captureMouseButton.Name = "captureMouseButton";
            captureMouseButton.Size = new Size(56, 35);
            captureMouseButton.TabIndex = 8;
            captureMouseButton.Text = "🖱️";
            captureToolTip.SetToolTip(captureMouseButton, "Fareyi tıklayın veya çevirin");
            captureMouseButton.UseVisualStyleBackColor = false;
            captureMouseButton.Click += CaptureMouseButton_Click;
            // 
            // captureKeyButton
            // 
            captureKeyButton.BackColor = Color.LightBlue;
            captureKeyButton.Cursor = Cursors.Hand;
            captureKeyButton.Location = new Point(330, 140);
            captureKeyButton.Name = "captureKeyButton";
            captureKeyButton.Size = new Size(56, 35);
            captureKeyButton.TabIndex = 7;
            captureKeyButton.Text = "⌨️";
            captureToolTip.SetToolTip(captureKeyButton, "Klavyeden herhangi bir tuşa basın");
            captureKeyButton.UseVisualStyleBackColor = false;
            captureKeyButton.Click += CaptureKeyButton_Click;
            // 
            // keyDelayLabel
            // 
            keyDelayLabel.AutoSize = true;
            keyDelayLabel.Location = new Point(330, 75);
            keyDelayLabel.Name = "keyDelayLabel";
            keyDelayLabel.Size = new Size(97, 20);
            keyDelayLabel.TabIndex = 5;
            keyDelayLabel.Text = "Gecikme (sn):";
            // 
            // keyDelayNumeric
            // 
            keyDelayNumeric.DecimalPlaces = 1;
            keyDelayNumeric.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            keyDelayNumeric.Location = new Point(330, 100);
            keyDelayNumeric.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            keyDelayNumeric.Name = "keyDelayNumeric";
            keyDelayNumeric.Size = new Size(120, 27);
            keyDelayNumeric.TabIndex = 4;
            keyDelayNumeric.Value = new decimal(new int[] { 5, 0, 0, 65536 });
            // 
            // newKeyComboBox
            // 
            newKeyComboBox.Cursor = Cursors.Hand;
            newKeyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            newKeyComboBox.FormattingEnabled = true;
            newKeyComboBox.Items.AddRange(new object[] { "Space", "Enter", "Tab", "Escape", "Backspace", "Delete", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Left", "Right", "Up", "Down", "--- FARE İŞLEMLERİ ---", "Sol Tık", "Sağ Tık", "Orta Tık", "Tekerlek Yukarı", "Tekerlek Aşağı", "Çift Tık" });
            newKeyComboBox.Location = new Point(330, 30);
            newKeyComboBox.Name = "newKeyComboBox";
            newKeyComboBox.Size = new Size(120, 28);
            newKeyComboBox.TabIndex = 3;
            // 
            // clearAllKeysButton
            // 
            clearAllKeysButton.Cursor = Cursors.Hand;
            clearAllKeysButton.Location = new Point(240, 120);
            clearAllKeysButton.Name = "clearAllKeysButton";
            clearAllKeysButton.Size = new Size(75, 35);
            clearAllKeysButton.TabIndex = 6;
            clearAllKeysButton.Text = "Tüm. Sil";
            clearAllKeysButton.UseVisualStyleBackColor = true;
            clearAllKeysButton.Click += ClearAllKeysButton_Click;
            // 
            // removeKeyButton
            // 
            removeKeyButton.Cursor = Cursors.Hand;
            removeKeyButton.Location = new Point(240, 75);
            removeKeyButton.Name = "removeKeyButton";
            removeKeyButton.Size = new Size(75, 35);
            removeKeyButton.TabIndex = 2;
            removeKeyButton.Text = "Sil";
            removeKeyButton.UseVisualStyleBackColor = true;
            removeKeyButton.Click += RemoveKeyButton_Click;
            // 
            // addKeyButton
            // 
            addKeyButton.Cursor = Cursors.Hand;
            addKeyButton.Location = new Point(240, 30);
            addKeyButton.Name = "addKeyButton";
            addKeyButton.Size = new Size(75, 35);
            addKeyButton.TabIndex = 1;
            addKeyButton.Text = "Ekle";
            addKeyButton.UseVisualStyleBackColor = true;
            addKeyButton.Click += AddKeyButton_Click;
            // 
            // keySequenceList
            // 
            keySequenceList.AllowDrop = true;
            keySequenceList.FormattingEnabled = true;
            keySequenceList.Location = new Point(20, 30);
            keySequenceList.Name = "keySequenceList";
            keySequenceList.Size = new Size(200, 124);
            keySequenceList.TabIndex = 0;
            keySequenceToolTip.SetToolTip(keySequenceList, "Öğeleri sürükleyerek sırasını değiştirebilirsiniz");
            keySequenceList.DragDrop += KeySequenceList_DragDrop;
            keySequenceList.DragEnter += KeySequenceList_DragEnter;
            keySequenceList.DragOver += KeySequenceList_DragOver;
            keySequenceList.MouseDown += KeySequenceList_MouseDown;
            keySequenceList.MouseMove += KeySequenceList_MouseMove;
            // 
            // multiKeyRadio
            // 
            multiKeyRadio.AutoSize = true;
            multiKeyRadio.Cursor = Cursors.Hand;
            multiKeyRadio.Location = new Point(102, 93);
            multiKeyRadio.Name = "multiKeyRadio";
            multiKeyRadio.Size = new Size(106, 24);
            multiKeyRadio.TabIndex = 3;
            multiKeyRadio.Text = "Çoklu İşlem";
            multiKeyRadio.UseVisualStyleBackColor = true;
            multiKeyRadio.CheckedChanged += MultiKeyRadio_CheckedChanged;
            // 
            // mouseRadio
            // 
            mouseRadio.AutoSize = true;
            mouseRadio.Cursor = Cursors.Hand;
            mouseRadio.Location = new Point(214, 93);
            mouseRadio.Name = "mouseRadio";
            mouseRadio.Size = new Size(57, 24);
            mouseRadio.TabIndex = 7;
            mouseRadio.Text = "Fare";
            mouseRadio.UseVisualStyleBackColor = true;
            mouseRadio.CheckedChanged += MouseRadio_CheckedChanged;
            // 
            // singleKeyRadio
            // 
            singleKeyRadio.AutoSize = true;
            singleKeyRadio.Checked = true;
            singleKeyRadio.Cursor = Cursors.Hand;
            singleKeyRadio.Location = new Point(23, 93);
            singleKeyRadio.Name = "singleKeyRadio";
            singleKeyRadio.Size = new Size(73, 24);
            singleKeyRadio.TabIndex = 2;
            singleKeyRadio.TabStop = true;
            singleKeyRadio.Text = "Klavye";
            singleKeyRadio.UseVisualStyleBackColor = true;
            singleKeyRadio.CheckedChanged += SingleKeyRadio_CheckedChanged;
            // 
            // keyComboBox
            // 
            keyComboBox.Cursor = Cursors.Hand;
            keyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            keyComboBox.FormattingEnabled = true;
            keyComboBox.Items.AddRange(new object[] { "Space", "Enter", "Tab", "Escape", "Backspace", "Delete", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Left", "Right", "Up", "Down" });
            keyComboBox.Location = new Point(160, 36);
            keyComboBox.Margin = new Padding(3, 4, 3, 4);
            keyComboBox.Name = "keyComboBox";
            keyComboBox.Size = new Size(140, 28);
            keyComboBox.TabIndex = 1;
            // 
            // mouseComboBox
            // 
            mouseComboBox.Cursor = Cursors.Hand;
            mouseComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            mouseComboBox.Enabled = false;
            mouseComboBox.FormattingEnabled = true;
            mouseComboBox.Items.AddRange(new object[] { "Sol Tık", "Sağ Tık", "Orta Tık", "Tekerlek Yukarı", "Tekerlek Aşağı", "Çift Tık" });
            mouseComboBox.Location = new Point(328, 36);
            mouseComboBox.Margin = new Padding(3, 4, 3, 4);
            mouseComboBox.Name = "mouseComboBox";
            mouseComboBox.Size = new Size(140, 28);
            mouseComboBox.TabIndex = 8;
            // 
            // keyLabel
            // 
            keyLabel.AutoSize = true;
            keyLabel.Location = new Point(23, 40);
            keyLabel.Name = "keyLabel";
            keyLabel.Size = new Size(131, 20);
            keyLabel.TabIndex = 0;
            keyLabel.Text = "Klavye/Fare İşlemi:";
            // 
            // timeGroup
            // 
            timeGroup.Controls.Add(infiniteCheckBox);
            timeGroup.Controls.Add(repeatNumeric);
            timeGroup.Controls.Add(repeatLabel);
            timeGroup.Controls.Add(intervalNumeric);
            timeGroup.Controls.Add(intervalLabel);
            timeGroup.Location = new Point(23, 424);
            timeGroup.Margin = new Padding(3, 4, 3, 4);
            timeGroup.Name = "timeGroup";
            timeGroup.Padding = new Padding(3, 4, 3, 4);
            timeGroup.Size = new Size(503, 133);
            timeGroup.TabIndex = 1;
            timeGroup.TabStop = false;
            timeGroup.Text = "Zaman Ayarları";
            // 
            // infiniteCheckBox
            // 
            infiniteCheckBox.AutoSize = true;
            infiniteCheckBox.Cursor = Cursors.Hand;
            infiniteCheckBox.Location = new Point(23, 80);
            infiniteCheckBox.Margin = new Padding(3, 4, 3, 4);
            infiniteCheckBox.Name = "infiniteCheckBox";
            infiniteCheckBox.Size = new Size(121, 24);
            infiniteCheckBox.TabIndex = 4;
            infiniteCheckBox.Text = "Sınırsız Tekrar";
            infiniteCheckBox.UseVisualStyleBackColor = true;
            infiniteCheckBox.CheckedChanged += InfiniteCheckBox_CheckedChanged;
            // 
            // repeatNumeric
            // 
            repeatNumeric.Location = new Point(400, 36);
            repeatNumeric.Margin = new Padding(3, 4, 3, 4);
            repeatNumeric.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            repeatNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            repeatNumeric.Name = "repeatNumeric";
            repeatNumeric.Size = new Size(91, 27);
            repeatNumeric.TabIndex = 3;
            repeatNumeric.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // repeatLabel
            // 
            repeatLabel.AutoSize = true;
            repeatLabel.Location = new Point(286, 40);
            repeatLabel.Name = "repeatLabel";
            repeatLabel.Size = new Size(93, 20);
            repeatLabel.TabIndex = 2;
            repeatLabel.Text = "Tekrar Sayısı:";
            // 
            // intervalNumeric
            // 
            intervalNumeric.DecimalPlaces = 1;
            intervalNumeric.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            intervalNumeric.Location = new Point(149, 36);
            intervalNumeric.Margin = new Padding(3, 4, 3, 4);
            intervalNumeric.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            intervalNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            intervalNumeric.Name = "intervalNumeric";
            intervalNumeric.Size = new Size(91, 27);
            intervalNumeric.TabIndex = 1;
            intervalNumeric.Value = new decimal(new int[] { 10, 0, 0, 65536 });
            // 
            // intervalLabel
            // 
            intervalLabel.AutoSize = true;
            intervalLabel.Location = new Point(23, 40);
            intervalLabel.Name = "intervalLabel";
            intervalLabel.Size = new Size(105, 20);
            intervalLabel.TabIndex = 0;
            intervalLabel.Text = "Aralık (saniye):";
            // 
            // startButton
            // 
            startButton.BackColor = Color.LightGreen;
            startButton.Cursor = Cursors.Hand;
            startButton.Location = new Point(23, 580);
            startButton.Margin = new Padding(3, 4, 3, 4);
            startButton.Name = "startButton";
            startButton.Size = new Size(114, 53);
            startButton.TabIndex = 2;
            startButton.Text = "Başlat";
            startButton.UseVisualStyleBackColor = false;
            startButton.Click += StartButton_Click;
            // 
            // stopButton
            // 
            stopButton.BackColor = Color.LightCoral;
            stopButton.Cursor = Cursors.Hand;
            stopButton.Enabled = false;
            stopButton.Location = new Point(160, 580);
            stopButton.Margin = new Padding(3, 4, 3, 4);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(114, 53);
            stopButton.TabIndex = 3;
            stopButton.Text = "Durdur";
            stopButton.UseVisualStyleBackColor = false;
            stopButton.Click += StopButton_Click;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(280, 585);
            statusLabel.MaximumSize = new Size(250, 500);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(44, 20);
            statusLabel.TabIndex = 4;
            statusLabel.Text = "Hazır";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(23, 655);
            progressBar.Margin = new Padding(3, 4, 3, 4);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(503, 27);
            progressBar.TabIndex = 5;
            // 
            // developerLabel
            // 
            developerLabel.AutoSize = true;
            developerLabel.Cursor = Cursors.Hand;
            developerLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic);
            developerLabel.ForeColor = Color.Gray;
            developerLabel.Location = new Point(23, 702);
            developerLabel.Name = "developerLabel";
            developerLabel.Size = new Size(412, 19);
            developerLabel.TabIndex = 6;
            developerLabel.Text = "Developed by: Muhammet Ali Songur | muhammetalisongur.com";
            developerLabel.Click += DeveloperLabel_Click;
            // 
            // keySequenceToolTip
            // 
            keySequenceToolTip.AutoPopDelay = 5000;
            keySequenceToolTip.InitialDelay = 1000;
            keySequenceToolTip.ReshowDelay = 500;
            // 
            // captureToolTip
            // 
            captureToolTip.AutoPopDelay = 10000;
            captureToolTip.InitialDelay = 500;
            captureToolTip.ReshowDelay = 100;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(546, 730);
            Controls.Add(developerLabel);
            Controls.Add(progressBar);
            Controls.Add(statusLabel);
            Controls.Add(stopButton);
            Controls.Add(startButton);
            Controls.Add(timeGroup);
            Controls.Add(keyGroup);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KeyBot - Tuş Otomasyonu";
            keyGroup.ResumeLayout(false);
            keyGroup.PerformLayout();
            multiKeyGroup.ResumeLayout(false);
            multiKeyGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)keyDelayNumeric).EndInit();
            timeGroup.ResumeLayout(false);
            timeGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)repeatNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)intervalNumeric).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
} 