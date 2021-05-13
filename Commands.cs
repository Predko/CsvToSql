using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace CSVtoDataBase
{
    public static class Commands
    {
        public static readonly RoutedUICommand ReadReports = new RoutedUICommand(
            nameof(ReadReports),
            nameof(ReadReports),
            typeof(Commands));

        public static readonly RoutedUICommand UpdateDataBase = new RoutedUICommand(
            nameof(UpdateDataBase),
            nameof(UpdateDataBase),
            typeof(Commands));
    }
}
