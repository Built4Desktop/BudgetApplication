# 💰 BudgetApplication

A simple **budget management desktop app** built with **C# .NET WPF**.  
This application allows you to track your income, add/remove expenses, and export your budget to a CSV or PDF File, for easy record-keeping.

---

## ✨ Features

- **Add & remove expenses** with names and amounts  
- **Automatic budget calculation**:
  - Total expenses
  - Remaining balance (highlighted in green if positive, red if negative)  
- **Input validation** (prevents empty or invalid values)  
- **Placeholder text** in inputs for a clean UI experience  
- **Export to CSV**:
  - Saves a `.csv` file to your Desktop
- **Export to PDF**:
  - Saves a `.pdf` file to your Desktop  
  - Includes income, expenses, total expenses, and remaining balance  

---

## 🛠️ Technologies Used

- C# (.NET 6 / WPF)
- XAML for UI design
- ObservableCollection for live expense tracking
- CSV export using `System.IO.StreamWriter`
- PDF export using `QuestPDF` Library

---

## 🚀 Getting Started

### Prerequisites
- [.NET 6 SDK or newer](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (or later) with WPF workload installed

### Run locally
1. Clone this repository:
   ```bash
   git clone https://github.com/Built4Desktop/BudgetApplication.git
