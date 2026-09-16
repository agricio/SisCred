using System;
using System.Windows.Forms;
using System.Globalization;
using OfficeOpenXml;
using CrudApp.Database;
using CrudApp.Forms;
using QuestPDF.Infrastructure;
using CrudApp.Services;

namespace CrudApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
                {
                    QuestPDF.Settings.License = LicenseType.Community;
                    
                    // EPPlus (forma compatível)
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    
                    // FORÇA CULTURA BRASILEIRA
                    var cultura = new CultureInfo("pt-BR");

                    CultureInfo.DefaultThreadCurrentCulture = cultura;
                    CultureInfo.DefaultThreadCurrentUICulture = cultura;
                    
                    ApplicationConfiguration.Initialize();
                    Database.Database.Initialize();

                    using (var splash = new FormSplash())
                    {
                        splash.Show();
                        Application.DoEvents();   // força desenhar a tela

                        // Simula carregamento (config, banco, etc)
                        System.Threading.Thread.Sleep(3000);
                    }

                    using (var login = new FormLogin())
                    {
                        var result = login.ShowDialog();
                        if (result != DialogResult.OK)
                            return;

                        Session.Login(
                            login.LoggedUserId,
                            login.LoggedUsername,
                            login.LoggedUserRole
                        );

                        Form formInicial;

                        switch (Session.CurrentUserRole)
                        {
                            case "admin":
                                formInicial = new FormDashboard();
                                break;

                            case "user":
                                formInicial = new FormDashboardUser();
                                break;

                            default:
                                MessageBox.Show("Perfil de usuário inválido.");
                                return;
                        }

                        Application.Run(formInicial);
                    }
                }
            catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Erro ao iniciar");
                }
                    
        }
    }
}




