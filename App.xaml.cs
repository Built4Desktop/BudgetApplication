using System.Configuration;
using System.Data;
using System.Windows;
using QuestPDF.Infrastructure;

namespace BudgetApplication
{
 
    public partial class App : Application
    {
        public App()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }
    }

}
