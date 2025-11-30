using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace wpfuidemo.ViewModels;

public partial class PropertyGridViewModel : ObservableObject
{
    [ObservableProperty]
    private Person _selectedPerson;

    public PropertyGridViewModel()
    {
        _selectedPerson = new Person
        {
            Name = "John Doe",
            Age = 30,
            IsActive = true,
            BirthDate = new DateTime(1990, 1, 1),
            Department = Department.Development,
            Salary = 5000.50m,
            Bio = "Software Engineer with 5 years of experience."
        };
    }
}

public enum Department
{
    HR,
    Development,
    Sales,
    Marketing
}

public class Person : ObservableObject
{
    private string _name = string.Empty;
    private int _age;
    private bool _isActive;
    private DateTime _birthDate;
    private Department _department;
    private decimal _salary;
    private string _bio = string.Empty;

    [Category("Basic Info")]
    [DisplayName("Full Name")]
    [Description("The full name of the person.")]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    [Category("Basic Info")]
    [Description("The age of the person.")]
    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    [Category("Status")]
    [DisplayName("Is Active")]
    [Description("Whether the person is currently active.")]
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    [Category("Basic Info")]
    [DisplayName("Date of Birth")]
    [Description("The birth date of the person.")]
    public DateTime BirthDate
    {
        get => _birthDate;
        set => SetProperty(ref _birthDate, value);
    }

    [Category("Work Info")]
    [Description("The department the person works in.")]
    public Department Department
    {
        get => _department;
        set => SetProperty(ref _department, value);
    }

    [Category("Work Info")]
    [Description("The monthly salary.")]
    [ReadOnly(true)]
    public decimal Salary
    {
        get => _salary;
        set => SetProperty(ref _salary, value);
    }

    [Category("Additional Info")]
    [Description("A short biography.")]
    public string Bio
    {
        get => _bio;
        set => SetProperty(ref _bio, value);
    }
}
