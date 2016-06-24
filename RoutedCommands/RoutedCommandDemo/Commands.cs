using System.Windows.Input;

namespace RoutedCommandDemo
{
    public static class Commands
    {
        /// <summary>
        /// Represents the Foo feature of the program.
        /// </summary>
        public static readonly RoutedCommand Foo = new RoutedCommand();
    }
}