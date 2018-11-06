using System;
using TrickyUnits;
using Gtk;

namespace Devlog
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // TODO: args recognition!
            Application.Init();
            MainWindow win = new MainWindow();
            win.Show();
            Application.Run();
        }
    }
}
