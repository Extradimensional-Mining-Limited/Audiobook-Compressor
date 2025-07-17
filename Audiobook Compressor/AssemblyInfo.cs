/*
    Last Updated: 2025-07-17 00:06 CEST
    Version: 1.0.2
    State: Stable
    Signed: User
    
    Synopsis:
    Assembly metadata and configuration.
    Defines WPF-specific assembly attributes for proper XAML compilation.
    Updated file header structure and documented in Summary.md.
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