/*
    Last Updated: 2024-01-08 20:45 CEST
    Version: 1.0.0
    State: Stable
    
    Synopsis:
    Assembly metadata and configuration.
    Defines WPF-specific assembly attributes for proper XAML compilation.
*/
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            //where theme specific resource dictionaries are located
                                                //(used if a resource is not found in the page,
                                                // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly   //where the generic resource dictionary is located
                                                //(used if a resource is not found in the page,
                                                // app, or any theme specific resource dictionaries)
)]