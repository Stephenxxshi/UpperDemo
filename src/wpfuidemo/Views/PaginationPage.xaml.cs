using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace wpfuidemo.Views
{
    public partial class PaginationPage : UserControl, INotifyPropertyChanged
    {
        private int _totalItems;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private List<Person> _allPeople;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int TotalItems
        {
            get => _totalItems;
            set { _totalItems = value; OnPropertyChanged(); }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set 
            { 
                if (_currentPage != value)
                {
                    _currentPage = value; 
                    OnPropertyChanged();
                    UpdateGrid();
                }
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set 
            { 
                if (_pageSize != value)
                {
                    _pageSize = value; 
                    OnPropertyChanged();
                    // Reset to page 1 when page size changes, or keep current logic
                    CurrentPage = 1; 
                    UpdateGrid();
                }
            }
        }

        public PaginationPage()
        {
            InitializeComponent();
            DataContext = this;
            GenerateData();
            UpdateGrid();
        }

        private void GenerateData()
        {
            _allPeople = new List<Person>();
            for (int i = 1; i <= 123; i++)
            {
                _allPeople.Add(new Person
                {
                    Id = i,
                    Name = $"User {i}",
                    Email = $"user{i}@example.com",
                    Role = i % 3 == 0 ? "Admin" : (i % 2 == 0 ? "Editor" : "User")
                });
            }
            TotalItems = _allPeople.Count;
        }

        private void UpdateGrid()
        {
            if (_allPeople == null) return;

            var pagedData = _allPeople
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
            
            DemoGrid.ItemsSource = pagedData;
        }

        private void DataPagination_PageChanged(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            // This event is fired by the control, but since we bound CurrentPage TwoWay, 
            // the setter of CurrentPage will handle the update.
            // However, if we didn't bind TwoWay, we would update here:
            // CurrentPage = e.NewValue;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
