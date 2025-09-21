using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BudgetApplication
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Expense> Expenses { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
            ExpensesList.ItemsSource = Expenses;
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ExpenseNameTextBox.Text) || string.IsNullOrWhiteSpace(ExpenseAmountTextBox.Text))
            {
                MessageBox.Show("Please enter both a name and an amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (decimal.TryParse(ExpenseAmountTextBox.Text, out decimal amount))
            {
                Expenses.Add(new Expense { Name = ExpenseNameTextBox.Text, Amount = amount });
                ExpenseNameTextBox.Clear();
                ExpenseAmountTextBox.Clear();
                CalculateBudget();
            }
            else
            {
                MessageBox.Show("Invalid number for amount.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveExpense_Click(object sender, RoutedEventArgs e)
        {
            if (ExpensesList.SelectedItem is Expense selectedExpense)
            {
                Expenses.Remove(selectedExpense);
                CalculateBudget();
            }
            else
            {
                MessageBox.Show("Please select an expense to remove.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemovePlaceholderText(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb &&
                (tb.Text == "Expense Name" || tb.Text == "Amount"))
            {
                tb.Text = "";
                tb.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void AddPlaceholderText(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
            {
                if (tb.Name == "ExpenseNameTextBox")
                    tb.Text = "Expense Name";
                else if (tb.Name == "ExpenseAmountTextBox")
                    tb.Text = "Amount";

                tb.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "BudgetExport.csv"
                );

                using (var writer = new System.IO.StreamWriter(filePath))
                {
                    writer.WriteLine("Income," + IncomeTextBox.Text);
                    writer.WriteLine("Expense Name,Amount");

                    foreach (var expense in Expenses)
                    {
                        writer.WriteLine($"{expense.Name},{expense.Amount}");
                    }

                    writer.WriteLine();
                    writer.WriteLine($"Total Expenses,{Expenses.Sum(x => x.Amount)}");
                    writer.WriteLine($"Remaining Balance,{(decimal.TryParse(IncomeTextBox.Text, out var inc) ? inc - Expenses.Sum(x => x.Amount) : 0)}");
                }

                MessageBox.Show($"Budget exported to:\n{filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting CSV: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToPdf_Click(object sender, RoutedEventArgs e)
        {
            ExportToPdf(Expenses.ToList(), PdfTitleTextBox.Text);
        }

        private void ExportToPdf(List<Expense> expenses, string title)
        {
            var filePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "BudgetExport.pdf"
            );

            try
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        page.Size(PageSizes.A4);

                        page.Header().Element(container =>
                        container.PaddingBottom(15).AlignCenter().Text(titleText =>
                        {
                         titleText.Span(string.IsNullOrWhiteSpace(title) ? "Budget Report" : title)
                        .FontSize(20)
                        .SemiBold();
                        }));

                        page.Content()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Expense Name").FontSize(14).SemiBold();
                                    header.Cell().Element(CellStyle).Text("Amount").FontSize(14).SemiBold();
                                });

                                // Expense Rows
                                foreach (var expense in expenses)
                                {
                                    table.Cell().Element(CellStyle).Text(expense.Name);
                                    table.Cell().Element(CellStyle).Text($"{expense.Amount:F2} DKK");
                                }

                                // Summary Footer
                                decimal sumOfExpenses = expenses.Sum(expense => expense.Amount);
                                decimal.TryParse(IncomeTextBox.Text, out decimal income);
                                decimal remaining = income - sumOfExpenses;

                                table.Cell().Element(CellStyle).Text("Total Expenses").FontSize(14).SemiBold();
                                table.Cell().Element(CellStyle).Text($"{sumOfExpenses:F2} DKK").FontSize(14).SemiBold();

                                table.Cell().Element(CellStyle).Text("Remaining Balance").FontSize(14).SemiBold();
                                var remainingCell = table.Cell().Element(CellStyle).Text($"{remaining:F2} DKK").FontSize(14).SemiBold();

                                remainingCell = remaining >= 0
                                    ? remainingCell.FontColor(Colors.Green.Darken2)
                                    : remainingCell.FontColor(Colors.Red.Darken2);

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.PaddingVertical(5).BorderBottom(1).BorderColor("#E0E0E0");
                                }
                            });
                    });
                })
                .GeneratePdf(filePath);

                MessageBox.Show($"PDF exported to:\n{filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateBudget()
        {
            try
            {
                decimal income = 0;
                if (!string.IsNullOrWhiteSpace(IncomeTextBox.Text))
                {
                    income = decimal.Parse(IncomeTextBox.Text);
                }

                decimal totalExpenses = Expenses.Sum(x => x.Amount);
                decimal remaining = income - totalExpenses;

                TotalExpensesText.Text = $"Total Expenses: {totalExpenses:C}";
                RemainingText.Text = $"Remaining Balance: {remaining:C}";

                RemainingText.Foreground = remaining < 0
                    ? System.Windows.Media.Brushes.Red
                    : System.Windows.Media.Brushes.Green;
            }
            catch (Exception)
            {
                MessageBox.Show("Error calculating budget. Please check inputs.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Expense
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
