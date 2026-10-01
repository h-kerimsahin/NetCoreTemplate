using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace KrmShn.VsTemplateWizards
{
    public class WizardForm : Form
    {
        public string ConnectionString { get; private set; }
        public string ProviderName { get; private set; }
        public string JwtSecret { get; private set; }

        private ComboBox _cboProvider;
        private TextBox _txtConnString;
        private TextBox _txtJwtSecret;
        private Button _btnOk;
        private Button _btnCancel;
        private Button _btnGenerateSecret;
        private Button _btnTestConn;
        private Label _lblInfo;

        private readonly Dictionary<string, string> _defaults;

        private static readonly Dictionary<string, string> ProviderPresets = new Dictionary<string, string>
        {
            ["SqlServer (LocalDB)"] = @"Server=(localdb)\MSSQLLocalDB;Database={DB};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
            ["SqlServer"] = @"Server=localhost;Database={DB};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
            ["PostgreSQL (Npgsql)"] = @"Host=localhost;Port=5432;Database={DB};Username=postgres;Password=postgres;Pooling=true;Include Error Detail=true",
            ["SQLite"] = @"Data Source=App_Data\{DB}.db"
        };

        private static readonly Dictionary<string, string> ProviderShortName = new Dictionary<string, string>
        {
            ["SqlServer (LocalDB)"] = "SqlServer",
            ["SqlServer"] = "SqlServer",
            ["PostgreSQL (Npgsql)"] = "PostgreSQL",
            ["SQLite"] = "Sqlite"
        };

        public WizardForm(Dictionary<string, string> defaults)
        {
            _defaults = defaults;
            defaults.TryGetValue("$wizard_connectionstring$", out var defConn);
            defaults.TryGetValue("$wizard_jwtsecret$", out var defJwt);
            ConnectionString = defConn ?? "";
            JwtSecret = defJwt ?? GenerateRandomSecret(64);
            ProviderName = "SqlServer";
            InitUI();
            ApplyDefaults(defConn);
        }

        private void InitUI()
        {
            Text = "KrmShn Net Core Backend — Setup Wizard";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(0x1e, 0x1e, 0x1e);
            ForeColor = Color.White;
            ClientSize = new Size(680, 510);
            Font = new Font("Segoe UI", 9f);

            var lblTitle = new Label
            {
                Text = "  ⚙️  Veritabanı ve Güvenlik Ayarları",
                BackColor = Color.FromArgb(0x2d, 0x6c, 0xee),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Height = 42,
                Top = 0, Left = 0, Width = ClientSize.Width,
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblTitle);

            var y = 62;

            AddLabel("Veritabanı Sağlayıcısı:", y); y += 22;
            _cboProvider = new ComboBox
            {
                Left = 24, Top = y, Width = 630, Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(0x3c, 0x3c, 0x3c),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            foreach (var k in ProviderPresets.Keys) _cboProvider.Items.Add(k);
            _cboProvider.SelectedIndexChanged += Provider_SelectedIndexChanged;
            Controls.Add(_cboProvider);
            y += 38;

            AddLabel("Connection String (isteğe göre düzenleyin):", y); y += 22;
            _txtConnString = new TextBox
            {
                Left = 24, Top = y, Width = 520, Height = 85,
                Multiline = true, ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(0x2d, 0x2d, 0x2d),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.5f)
            };
            _btnTestConn = new Button
            {
                Text = "🎯 Template'i Kullan",
                Left = 554, Top = y, Width = 100, Height = 30,
                BackColor = Color.FromArgb(0x0d, 0x73, 0x3d),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnTestConn.Click += (s, e) =>
            {
                MessageBox.Show(this,
                    "Şablon seçildikten sonra Connection String ayarları appsettings.json'a yazılacaktır.\r\n\r\n" +
                    "Projeyi çalıştırmadan önce:\r\n" +
                    "1) appsettings.json'u kontrol edin\r\n" +
                    "2) 'dotnet ef database update' komutunu çalıştırın",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            Controls.Add(_txtConnString);
            Controls.Add(_btnTestConn);
            y += 96;

            AddLabel("JWT Secret Key (rastgele üretildi - isterseniz değiştirin):", y); y += 22;
            _txtJwtSecret = new TextBox
            {
                Left = 24, Top = y, Width = 520, Height = 30,
                BackColor = Color.FromArgb(0x2d, 0x2d, 0x2d),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.5f)
            };
            _btnGenerateSecret = new Button
            {
                Text = "🔄 Yeni Üret",
                Left = 554, Top = y, Width = 100, Height = 30,
                BackColor = Color.FromArgb(0x7a, 0x4c, 0x00),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnGenerateSecret.Click += (s, e) =>
            {
                _txtJwtSecret.Text = GenerateRandomSecret(64);
            };
            Controls.Add(_txtJwtSecret);
            Controls.Add(_btnGenerateSecret);
            y += 50;

            _lblInfo = new Label
            {
                Left = 24, Top = y, Width = 630, Height = 110,
                ForeColor = Color.FromArgb(0xaa, 0xaa, 0xaa),
                Text = "📝 Bilgilendirme:\r\n" +
                       "  • Yukarıda seçtiğiniz ayarlar otomatik olarak appsettings.json'a yazılacaktır.\r\n" +
                       "  • İsterseniz proje oluşturulduktan sonra bu dosyayı düzenleyebilirsiniz.\r\n" +
                       "  • DB Provider'a göre Infrastructure katmanında gerekli paketleri eklemeniz gerekebilir (Npgsql / SQLite).\r\n" +
                       "  • Projeyi ilk çalıştırmadan önce:  dotnet ef database update  (migration'ları uygular)",
                TextAlign = ContentAlignment.TopLeft
            };
            Controls.Add(_lblInfo);

            y = ClientSize.Height - 48;
            _btnOk = new Button
            {
                Text = "✅ Devam Et",
                Left = ClientSize.Width - 248, Top = y, Width = 110, Height = 32,
                BackColor = Color.FromArgb(0x0d, 0x73, 0x3d),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            _btnCancel = new Button
            {
                Text = "İptal",
                Left = ClientSize.Width - 128, Top = y, Width = 104, Height = 32,
                BackColor = Color.FromArgb(0x3c, 0x3c, 0x3c),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
            _btnOk.Click += OkClicked;
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);
        }

        private void AddLabel(string text, int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                Left = 24, Top = y, Width = 630, Height = 18,
                ForeColor = Color.FromArgb(0xee, 0xee, 0xee),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            });
        }

        private void Provider_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_cboProvider.SelectedItem is string key && ProviderPresets.TryGetValue(key, out var template))
            {
                string safeName = "KrmShnBackend";
                if (_defaults != null)
                {
                    if (_defaults.TryGetValue("$safeprojectname$", out var s) && !string.IsNullOrEmpty(s)) safeName = s;
                    else if (_defaults.TryGetValue("$projectname$", out var p) && !string.IsNullOrEmpty(p)) safeName = p;
                }
                _txtConnString.Text = template.Replace("{DB}", safeName);
            }
        }

        private void ApplyDefaults(string defConn)
        {
            if (!string.IsNullOrEmpty(defConn))
            {
                _txtConnString.Text = defConn;
            }
            _cboProvider.SelectedIndex = 0;
            if (string.IsNullOrEmpty(_txtConnString.Text))
            {
                Provider_SelectedIndexChanged(this, EventArgs.Empty);
            }
            _txtJwtSecret.Text = JwtSecret;
        }

        private void OkClicked(object sender, EventArgs e)
        {
            ConnectionString = _txtConnString.Text?.Trim();
            JwtSecret = _txtJwtSecret.Text?.Trim();
            if (_cboProvider.SelectedItem is string key && ProviderShortName.TryGetValue(key, out var shortName))
            {
                ProviderName = shortName;
            }

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                MessageBox.Show(this, "Connection String boş bırakılamaz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
            else if (string.IsNullOrWhiteSpace(JwtSecret) || JwtSecret.Length < 16)
            {
                MessageBox.Show(this, "JWT Secret Key en az 16 karakter olmalıdır.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        }

        private static string GenerateRandomSecret(int length)
        {
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*_-+=";
            var res = new StringBuilder(length);
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[sizeof(uint)];
                while (res.Length < length)
                {
                    rng.GetBytes(buffer);
                    uint num = BitConverter.ToUInt32(buffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }
            return res.ToString();
        }
    }
}
