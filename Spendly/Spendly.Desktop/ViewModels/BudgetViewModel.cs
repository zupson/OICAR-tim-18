using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Spendly.Desktop.Models;

namespace Spendly.Desktop.ViewModels;

public partial class BudgetViewModel : ObservableObject
{
    public ObservableCollection<Budget> Budgets { get; } = new()
    {
        new() { Id=1, Name="Hrana & Namirnice", Category="Hrana",     Limit=800, Spent=680  },
        new() { Id=2, Name="Prijevoz",          Category="Prijevoz",  Limit=300, Spent=245  },
        new() { Id=3, Name="Zabava",            Category="Zabava",    Limit=200, Spent=210  },
        new() { Id=4, Name="Zdravlje",          Category="Zdravlje",  Limit=400, Spent=120  },
        new() { Id=5, Name="Režije",            Category="Režije",    Limit=500, Spent=390  },
    };
}
