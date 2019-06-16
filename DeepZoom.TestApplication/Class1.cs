using DeepZoom.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DeepZoom.TestApplication
{
    class MouseTouch
    {
        private bool mouse_down = false;

        public void Mouse_Down(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                mouse_down = true;
            else
                mouse_down = false;
        }

        public void Mouse_Move(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }
    }
}
