﻿using System.Windows.Controls;
using System.Windows.Input;
using BusinessLib;

namespace TreeViewWithViewModelDemo
{
    public partial class TextSearchDemoControl : UserControl
    {
        readonly FamilyTreeViewModel _familyTree;  

        public TextSearchDemoControl()
        {
            InitializeComponent();

            // Get raw family tree data from a database.
            Person rootPerson = Database.GetFamilyTree();

            // Create UI-friendly wrappers around the 
            // raw data objects (i.e. the view-model).
            _familyTree = new FamilyTreeViewModel(rootPerson);

            // Let the UI bind to the view-model.
            base.DataContext = _familyTree;

            // This is a good search string for showing the bring-into-view feature.
            _familyTree.SearchText = "y";
        }

        void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _familyTree.SearchCommand.Execute(null);
        }
    }
}