﻿using System.Windows;

namespace Dev2.Activities.Designers2.Calculate
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
        }

        protected override IInputElement GetInitialFocusElement() => DataGrid.GetFocusElement(0);
    }
}