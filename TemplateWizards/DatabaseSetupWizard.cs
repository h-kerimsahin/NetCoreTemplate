using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;

namespace KrmShn.VsTemplateWizards
{
    public class DatabaseSetupWizard : IWizard
    {
        private WizardForm _form;
        private Dictionary<string, string> _replacementsDictionary;

        public void BeforeOpeningFile(ProjectItem projectItem) { }

        public void ProjectFinishedGenerating(Project project) { }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        public void RunFinished() { }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            _replacementsDictionary = replacementsDictionary;

            try
            {
                // Varsayılan değerleri üret - rastgele bir JWT Secret
                if (!_replacementsDictionary.ContainsKey("$wizard_jwtsecret$"))
                {
                    _replacementsDictionary.Add("$wizard_jwtsecret$", GenerateRandomSecret(64));
                }
                else
                {
                    _replacementsDictionary["$wizard_jwtsecret$"] = GenerateRandomSecret(64);
                }

                // Güvenli proje adını DB adı olarak kullan
                string safeName = GetSafeName(replacementsDictionary);
                string defaultConn = $@"Server=(localdb)\MSSQLLocalDB;Database={safeName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

                if (!_replacementsDictionary.ContainsKey("$wizard_connectionstring$"))
                {
                    _replacementsDictionary.Add("$wizard_connectionstring$", defaultConn);
                }
                else
                {
                    _replacementsDictionary["$wizard_connectionstring$"] = defaultConn;
                }

                if (!_replacementsDictionary.ContainsKey("$wizard_dbprovider$"))
                {
                    _replacementsDictionary.Add("$wizard_dbprovider$", "SqlServer");
                }
                else
                {
                    _replacementsDictionary["$wizard_dbprovider$"] = "SqlServer";
                }

                // Formu göster ve kullanıcıdan değer al
                using (_form = new WizardForm(_replacementsDictionary))
                {
                    if (_form.ShowDialog() == DialogResult.Cancel)
                    {
                        throw new WizardCancelledException("Template creation cancelled by user.");
                    }

                    // Formdan dönen değerleri sözlüğe yaz
                    _replacementsDictionary["$wizard_connectionstring$"] = _form.ConnectionString;
                    _replacementsDictionary["$wizard_dbprovider$"] = _form.ProviderName;
                    _replacementsDictionary["$wizard_jwtsecret$"] = _form.JwtSecret;
                }
            }
            catch (WizardCancelledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Hata durumunda varsayılan değerlerle devam et - template oluşturmayı bozma
                System.Diagnostics.Debug.WriteLine("Template Wizard error: " + ex.Message);
            }
        }

        private static string GetSafeName(Dictionary<string, string> d)
        {
            if (d.TryGetValue("$safeprojectname$", out var s) && !string.IsNullOrEmpty(s)) return s;
            if (d.TryGetValue("$projectname$", out var p) && !string.IsNullOrEmpty(p)) return p;
            return "KrmShnBackend";
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

        public bool ShouldAddProjectItem(string filePath) => true;
    }
}
